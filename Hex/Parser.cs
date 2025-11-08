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

    internal List<Stmt>? Parse()
    {
        List<Stmt> statements = [];
        while (!IsAtEnd())
        {
            statements.Add(Declaration());
        }

        return statements;
    }

    private Stmt? Declaration()
    {
        try
        {
            if (Match(TokenType.Var))
                return VarDeclaration();
            if (Match(TokenType.Class))
                return ClassDeclaration();
            if (Match(TokenType.Fun))
                return Function("function");

            return Statement();
        }
        catch (ParseException e)
        {
            Synchronize();
            return null;
        }
    }

    private Stmt ClassDeclaration()
    {
        Token name = ConsumeIfMatch(TokenType.Identifier, "Expect class name.");

        Expr.Variable? superClass = null;
        if (Match(TokenType.Less))
        {
            ConsumeIfMatch(TokenType.Identifier, "Expect superclass name.");
            superClass = new Expr.Variable(Previous());
        }
        
        ConsumeIfMatch(TokenType.LeftBrace, "Expect '{' before class body.");

        List<Stmt.Function> methods = [];
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            ConsumeIfMatch(TokenType.Fun, "Expect spell keyword.");
            methods.Add(Function("method"));
        }

        ConsumeIfMatch(TokenType.RightBrace, "Expect '}' after class body.");
        return new Stmt.Class(name, superClass, methods);
    }

    private Stmt.Function Function(string kind)
    {
        Token name = ConsumeIfMatch(TokenType.Identifier, "Expect " + kind + " name.");
        ConsumeIfMatch(TokenType.LeftParen, "Expect '(' after " + kind + " name.");
        List<Token> parameters = new List<Token>();

        if (!Check(TokenType.RightParen))
        {
            do
            {
                if (parameters.Count >= 255)
                    Error(Peek(), "Can't have more than 255 parameters.");
                
                parameters.Add(ConsumeIfMatch(TokenType.Identifier, "Expect parameter type."));
            } while (Match(TokenType.Comma));
        }

        ConsumeIfMatch(TokenType.RightParen, "Expect ')' after parameters.");
        ConsumeIfMatch(TokenType.LeftBrace, "Expect '{' before " + kind + " body.");

        List<Stmt> body = Block();

        return new Stmt.Function(name, parameters, body);
    }

    private Stmt VarDeclaration()
    {
        Token name = ConsumeIfMatch(TokenType.Identifier, "Expect variable name.");

        Expr? initializer = null;
        if (Match(TokenType.Equal))
        {
            initializer = Expression();
        }

        ConsumeIfMatch(TokenType.Semicolon, "Expect ';' after variable declaration.");
        return new Stmt.Var(name, initializer);
    }
    
    private Stmt Statement()
    {
        if (Match(TokenType.For))
            return ForStatement();
        if (Match(TokenType.If))
            return IfStatement();
        if (Match(TokenType.Print))
            return PrintStatement();
        if (Match(TokenType.Return))
            return ReturnStatement();
        if (Match(TokenType.While))
            return WhileStatement();
        if (Match(TokenType.LeftBrace))
            return new Stmt.Block(Block());
        return ExpressionStatement();
    }

    private Stmt ForStatement()
    {
        ConsumeIfMatch(TokenType.LeftParen, "Expect '(' after 'for'.");

        Stmt? initializer;
        if (Match(TokenType.Semicolon))
        {
            initializer = null;
        }
        else if (Match(TokenType.Var))
        {
            initializer = VarDeclaration();
        }
        else
        {
            initializer = ExpressionStatement();
        }

        Expr? condition = null;
        if (!Check(TokenType.Semicolon))
            condition = Expression();
        ConsumeIfMatch(TokenType.Semicolon, "Expect ';' after loop condition.");

        Expr? increment = null;
        if (!Check(TokenType.RightParen))
            increment = Expression();
        ConsumeIfMatch(TokenType.RightParen, "Expect ')' after for clauses.");

        Stmt body = Statement();

        if (increment != null)
            body = new Stmt.Block([body, new Stmt.Expression(increment)]);
        if (condition == null)
            condition = new Expr.Literal(true);
        body = new Stmt.While(condition, body);

        if (initializer != null)
            body = new Stmt.Block([initializer, body]);
        
        return body;
    }
    
    private Stmt IfStatement()
    {
        ConsumeIfMatch(TokenType.LeftParen, "Expect '(' after 'if'");
        Expr condition = Expression();
        ConsumeIfMatch(TokenType.RightParen, "Expect ')' after if condition");

        Stmt thenBranch = Statement();
        Stmt? elseBranch = null;
        if (Match(TokenType.Else))
            elseBranch = Statement();

        return new Stmt.If(condition, thenBranch, elseBranch);
    }

    private Stmt PrintStatement()
    {
        Expr value = Expression();
        ConsumeIfMatch(TokenType.Semicolon, "Expect ';' after value.");
        return new Stmt.Print(value);
    }

    private Stmt ReturnStatement()
    {
        Token keyword = Previous();
        Expr? value = null;

        if (!Check(TokenType.Semicolon))
            value = Expression();

        ConsumeIfMatch(TokenType.Semicolon, "Expect ';' after return value.");
        return new Stmt.Return(keyword, value);
    }

    private Stmt WhileStatement()
    {
        ConsumeIfMatch(TokenType.LeftParen, "Expect '(' after 'while'.");
        Expr condition = Expression();
        ConsumeIfMatch(TokenType.RightParen, "Expect ')' after condition.");

        Stmt body = Statement();

        return new Stmt.While(condition, body);
    }
    
    private Stmt ExpressionStatement()
    {
        Expr expression = Expression();
        ConsumeIfMatch(TokenType.Semicolon, "Expect ';' after expression.");
        return new Stmt.Expression(expression);
    }

    private List<Stmt> Block()
    {
        List<Stmt> statements = [];
        
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
            statements.Add(Declaration());

        ConsumeIfMatch(TokenType.RightBrace, "Expect '}' after block.");
        return statements;
    }

    private Expr Expression()
    {
        return Assignment();
    }

    private Expr Assignment()
    {
        Expr expr = Or();

        if (Match(TokenType.Equal))
        {
            Token equals = Previous();
            Expr value = Assignment();

            if (expr is Expr.Variable varExpr)
                return new Expr.Assign(varExpr.Name, value);
            if (expr is Expr.Get getVar)
                return new Expr.Set(getVar.Obj, getVar.Name, value);

            Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr Or()
    {
        Expr expr = And();

        while (Match(TokenType.Or))
        {
            Token operatorToken = Previous();
            Expr rhs = And();
            expr = new Expr.Logical(expr, operatorToken, rhs);
        }

        return expr;
    }

    private Expr And()
    {
        Expr expr = Ternary();

        while (Match(TokenType.And))
        {
            Token operatorToken = Previous();
            Expr rhs = Ternary();
            expr = new Expr.Logical(expr, operatorToken, rhs);
        }

        return expr;
    }

    // private Expr Comma()
    // {
    //     Expr expr = Ternary();
    //
    //     while (Match(TokenType.Comma))
    //     {
    //         Token operatorToken = Previous();
    //         Expr rhs = Ternary();
    //         expr = new Expr.Binary(expr, operatorToken, rhs);
    //     }
    //
    //     return expr;
    // }

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

        return Call();
    }

    private Expr Call()
    {
        Expr expr = Primary();

        while (true)
        {
            if (Match(TokenType.LeftParen))
                expr = FinishCall(expr);
            else if (Match(TokenType.Dot))
            {
                Token name = ConsumeIfMatch(TokenType.Identifier, "Expect property name after '.'.");
                expr = new Expr.Get(expr, name);
            }
            else
                break;
        }

        return expr;
    }

    private Expr FinishCall(Expr callee)
    {
        List<Expr> arguments = new List<Expr>();
        if (!Check(TokenType.RightParen))
        {
            do
            {
                if (arguments.Count >= 255)
                    Error(Peek(), "Can't have more than 255 arguments.");
                arguments.Add(Expression());
            } while (Match(TokenType.Comma));
        }

        Token paren = ConsumeIfMatch(TokenType.RightParen, "Expect ')' after arguments.");

        return new Expr.Call(callee, paren, arguments);
    }

    private Expr Primary()
    {
        if (Match(TokenType.False)) return new Expr.Literal(false);
        if (Match(TokenType.True)) return new Expr.Literal(true);
        if (Match(TokenType.Nil)) return new Expr.Literal(null);
        if (Match(TokenType.Number, TokenType.String)) return new Expr.Literal(Previous().Literal);
        if (Match(TokenType.Super))
        {
            Token keyword = Previous();
            ConsumeIfMatch(TokenType.Dot, "Expect '.' after 'super'");
            Token method = ConsumeIfMatch(TokenType.Identifier, "Expect superclass method name.");
            return new Expr.Super(keyword, method);
        }
        if (Match(TokenType.This)) return new Expr.This(Previous());
        if (Match(TokenType.Identifier)) return new Expr.Variable(Previous());
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