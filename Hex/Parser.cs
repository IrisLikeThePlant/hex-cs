namespace Hex;

internal class Parser
{
    public class ParseException : Exception {}
    
    private readonly List<Token> _tokens;
    private int _current = 0;

    internal Parser(List<Token> tokens)
    {
        this._tokens = tokens;
    }

    internal Expr? Parse()
    {
        try
        {
            return Expression();
        }
        catch (ParseException error)
        {
            return null;
        }
    }

    private Expr Expression()
    {
        return Comma();
    }

    // expression -> comma ;
    // comma -> equality ( "," equality )* ;
    private Expr Comma()
    {
        Expr expr = Ternary();

        while (Match(TokenType.Comma))
        {
            Token operatorToken = Previous();
            Expr rhs = Ternary();
            expr = new Expr.Binary(expr, operatorToken, rhs);
        }

        return expr;
    }

    // comma -> ternary ( "," ternary )* ;
    // ternary  -> equality ( "?" expression ":" ternary )* ;
    private Expr Ternary()
    {
        Expr expr = Equality();

        if (Match(TokenType.Question))
        {
            Expr trueBranch = Expression();
            ConsumeIfMatch(TokenType.Colon, "Expected ':' after expression");
            Expr falseBranch = Ternary();
            expr = new Expr.Ternary(expr, trueBranch, falseBranch);
        }

        return expr;
    }
    
    private Expr Equality()
    {
        Expr expr = Comparison();

        while (Match(TokenType.BangEqual, TokenType.EqualEqual))
        {
            Token operatorToken = Previous();
            Expr rhs = Comparison();
            expr = new Expr.Binary(expr, operatorToken, rhs);
        }

        return expr;
    }

    private Expr Comparison()
    {
        Expr expr = Term();

        while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
        {
            Token operatorToken = Previous();
            Expr rhs = Term();
            expr = new Expr.Binary(expr, operatorToken, rhs);
        }

        return expr;
    }

    private Expr Term()
    {
        Expr expr = Factor();
        
        while (Match(TokenType.Plus, TokenType.Minus))
        {
            Token operatorToken = Previous();
            Expr rhs = Factor();
            expr = new Expr.Binary(expr, operatorToken, rhs);
        }

        return expr;
    }
    
    private Expr Factor()
    {
        Expr expr = Unary();
        
        while (Match(TokenType.Star, TokenType.Slash))
        {
            Token operatorToken = Previous();
            Expr rhs = Unary();
            expr = new Expr.Binary(expr, operatorToken, rhs);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (Match(TokenType.Bang, TokenType.Minus))
        {
            Token operatorToken = Previous();
            Expr rhs = Unary();
            return new Expr.Unary(operatorToken, rhs);
        }

        return Primary();
    }

    private Expr Primary()
    {
        if (Match(TokenType.False)) return new Expr.Literal(false);
        if (Match(TokenType.True)) return new Expr.Literal(true);
        if (Match(TokenType.Nil)) return new Expr.Literal(null);
        if (Match(TokenType.Number, TokenType.String)) return new Expr.Literal(Previous().Literal);
        if (Match(TokenType.LeftParen))
        {
            Expr expr = Expression();
            ConsumeIfMatch(TokenType.RightParen, "Expected ')' after expression.");
            return new Expr.Grouping(expr);
        }

        throw Error(Peek(), "Expected expression.");
    }

    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Consume();
                return true;
            }
        }

        return false;
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private Token Consume()
    {
        if (!IsAtEnd()) _current++;
        return Previous();
    }

    private Token ConsumeIfMatch(TokenType type, string message)
    {
        if (Check(type)) return Consume();
        throw Error(Peek(), message);
    }

    private ParseException Error(Token token, string message)
    {
        Hex.Error(token, message);
        return new ParseException();
    }

    private void Synchronize()
    {
        Consume();

        while (!IsAtEnd())
        {
            if (Previous().Type == TokenType.Semicolon) return;

            switch (Peek().Type)
            {
                case TokenType.Class:
                case TokenType.Fun:
                case TokenType.Var:
                case TokenType.For:
                case TokenType.If:
                case TokenType.While:
                case TokenType.Print:
                case TokenType.Return:
                    return;
            }

            Consume();
        }
    }

    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.Eof;
    }

    private Token Peek()
    {
        return _tokens.ElementAt(_current);
    }

    private Token Previous()
    {
        return _tokens.ElementAt(_current - 1);
    }
}