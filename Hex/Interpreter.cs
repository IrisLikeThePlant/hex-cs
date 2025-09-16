namespace Hex;

internal class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
{
    private Environment _environment = new Environment();
    
    internal void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        catch (RuntimeError e)
        {
            Hex.RuntimeError(e);
        }
    }

    public object? VisitStmtBlock(Stmt.Block stmt)
    {
        ExecuteBlock(stmt.Statements, new Environment(_environment));
        return null;
    }

    public object? VisitStmtExpression(Stmt.Expression stmt)
    {
        Evaluate(stmt.Expr);
        return null;
    }

    public object? VisitStmtPrint(Stmt.Print stmt)
    {
        object value = Evaluate(stmt.Expr);
        Console.WriteLine(Stringify(value));
        return null;
    }

    public object? VisitStmtVar(Stmt.Var stmt)
    {
        object? value = null;
        if (stmt.Initializer != null)
            value = Evaluate(stmt.Initializer);
        _environment.Define(stmt.Name.Lexeme, value);
        return null;
    }

    public object? VisitExprVariable(Expr.Variable expr)
    {
        return _environment.Get(expr.Name);
    }

    public object? VisitExprAssign(Expr.Assign expr)
    {
        object value = Evaluate(expr.Value);
        _environment.Assign(expr.Name, value);
        return value;
    }

    public object VisitExprTernary(Expr.Ternary expr)
    {
        object condition = Evaluate(expr.Condition);
        if (IsTruthy(condition))
            return Evaluate(expr.TrueBranch);
        return Evaluate(expr.FalseBranch);
    }

    public object VisitExprBinary(Expr.Binary expr)
    {
        object left = Evaluate(expr.Lhs);
        object right = Evaluate(expr.Rhs);

        switch (expr.OperatorToken.Type)
        {
            case TokenType.Greater:
                CheckNumberOperands(expr.OperatorToken, left, right);
                return (double)left > (double)right;
            case TokenType.GreaterEqual:
                CheckNumberOperands(expr.OperatorToken, left, right);
                return (double)left >= (double)right;
            case TokenType.Less:
                CheckNumberOperands(expr.OperatorToken, left, right);
                return (double)left < (double)right;
            case TokenType.LessEqual:
                CheckNumberOperands(expr.OperatorToken, left, right);
                return (double)left <= (double)right;
            case TokenType.BangEqual:
                return !IsEqual(left, right);
            case TokenType.EqualEqual:
                return IsEqual(left, right);
            case TokenType.Minus:
                CheckNumberOperands(expr.OperatorToken, left, right);
                return (double)left - (double)right;
            case TokenType.Plus:
                if (left is double dLeft && right is double dRight)
                    return dLeft + dRight;
                if (left is string sL && right is string sR)
                    return sL + sR;
                if (left is string sLeft && right is not string)
                    return sLeft + right.ToString();
                if (left is not string && right is string sRight)
                    return left.ToString() + sRight;
                throw new RuntimeError(expr.OperatorToken, "Operands must be two numbers or two strings.");
            case TokenType.Slash:
                CheckNumberOperands(expr.OperatorToken, left, right);
                if ((double)right == 0)
                    throw new RuntimeError(expr.OperatorToken, "You can't divide by zero, dummy.");
                return (double)left / (double)right;
            case TokenType.Star:
                CheckNumberOperands(expr.OperatorToken, left, right);
                return (double)left * (double)right;
        }

        throw new RuntimeError(expr.OperatorToken, "This code was supposed to be unreachable.");
    }

    public object VisitExprGrouping(Expr.Grouping expr)
    {
        return Evaluate(expr.Expression);
    }

    public object VisitExprLiteral(Expr.Literal expr)
    {
        return expr.Value;
    }

    public object VisitExprUnary(Expr.Unary expr)
    {
        object right = Evaluate(expr.Rhs);

        switch (expr.OperatorToken.Type)
        {
            case TokenType.Minus:
                CheckNumberOperand(expr.OperatorToken, right);
                return -(double)right;
            case TokenType.Bang:
                return !IsTruthy(right);
        }

        throw new RuntimeError(expr.OperatorToken, "This code was supposed to be unreachable.");
    }

    private object Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }
    
    private void Execute(Stmt stmt)
    {
        stmt.Accept(this);
    }

    private void ExecuteBlock(List<Stmt> statements, Environment environment)
    {
        Environment previous = this._environment;
        try
        {
            this._environment = environment;

            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            this._environment = previous;
        }
    }

    private bool IsTruthy(object? obj)
    {
        if (obj == null) return false;
        if (obj is bool bObj) return bObj;
        return true;
    }

    private bool IsEqual(object? a, object? b)
    {
        if (a == null && b == null) return true;
        if (a == null) return false;
        return a.Equals(b);
    }

    private void CheckNumberOperand(Token operatorToken, object operand)
    {
        if (operand is double) return;
        throw new RuntimeError(operatorToken, "Operand must be a number.");
    }
    
    private void CheckNumberOperands(Token operatorToken, object left, object right)
    {
        if (left is double && right is double) return;
        throw new RuntimeError(operatorToken, "Operands must be numbers.");
    }

    private string Stringify(object? obj)
    {
        if (obj == null) return "nix";

        if (obj is double)
        {
            string text = obj.ToString();
            if (text.EndsWith(".0"))
                text = text.Substring(0, text.Length - 2);
            return text;
        }

        return obj.ToString();
    }
}