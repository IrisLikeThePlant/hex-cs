namespace Hex;

internal class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
{
    internal readonly Environment Globals = new Environment();
    private Environment _environment;
    private readonly Dictionary<Expr, int> _locals = new();

    internal Interpreter()
    {
        _environment = Globals;
        Globals.Define("Clock", new ClockFunction());
    }
    
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

    public object? VisitStmtClass(Stmt.Class stmt)
    {
        _environment.Define(stmt.Name.Lexeme, null);

        Dictionary<string, HexFunction> methods = new();
        foreach (var method in stmt.Methods)
        {
            HexFunction function = new HexFunction(method, _environment, method.Name.Lexeme.Equals("init"));
            methods[method.Name.Lexeme] = function;
        }
        
        HexClass klass = new HexClass(stmt.Name.Lexeme, methods);
        _environment.Assign(stmt.Name, klass);
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

    public object? VisitStmtReturn(Stmt.Return stmt)
    {
        object? value = null;

        if (stmt.Value != null)
            value = Evaluate(stmt.Value);

        throw new Return(value);
    }

    public object? VisitStmtVar(Stmt.Var stmt)
    {
        object? value = null;
        if (stmt.Initializer != null)
            value = Evaluate(stmt.Initializer);
        _environment.Define(stmt.Name.Lexeme, value);
        return null;
    }
    
    public object? VisitStmtFunction(Stmt.Function stmt)
    {
        HexFunction function = new HexFunction(stmt, _environment, false);
        _environment.Define(stmt.Name.Lexeme, function);
        return null;
    }

    public object? VisitStmtIf(Stmt.If stmt)
    {
        if (IsTruthy(Evaluate(stmt.Condition)))
            Execute(stmt.ThenBranch);
        else if (stmt.ElseBranch != null)
            Execute(stmt.ElseBranch);
        return null;
    }

    public object? VisitStmtWhile(Stmt.While stmt)
    {
        while (IsTruthy(Evaluate(stmt.Condition)))
            Execute(stmt.Body);
        return null;
    }

    public object? VisitExprCall(Expr.Call expr)
    {
        object callee = Evaluate(expr.Callee);

        List<object?> arguments = [];
        foreach (var argument in expr.Arguments)
        {
            arguments.Add(Evaluate(argument));
        }

        if (callee is not ICallable)
            throw new RuntimeError(expr.Paren, "Can only call functions and classes.");
        
        ICallable function = (ICallable)callee;

        if (arguments.Count != function.Arity())
            throw new RuntimeError(expr.Paren,
                "Expected " + function.Arity() + " arguments but got " + arguments.Count + ".");
        
        return function.Call(this, arguments);
    }

    public object? VisitExprGet(Expr.Get expr)
    {
        var obj = Evaluate(expr.Obj);

        if (obj is HexInstance)
            return ((HexInstance)obj).Get(expr.Name);

        throw new RuntimeError(expr.Name, "Only instances of classes have properties.");
    }

    public object? VisitExprSet(Expr.Set expr)
    {
        var obj = Evaluate(expr.Obj);

        if (obj is not HexInstance)
            throw new RuntimeError(expr.Name, "Only instances of classes have fields");

        var value = Evaluate(expr.Value);
        ((HexInstance)obj).Set(expr.Name, value);
        return value;
    }

    public object? VisitExprThis(Expr.This expr)
    {
        return LookUpVariable(expr.Keyword, expr);
    }

    public object? VisitExprVariable(Expr.Variable expr)
    {
        return LookUpVariable(expr.Name, expr);
    }

    private object? LookUpVariable(Token name, Expr expr)
    {
        if (_locals.TryGetValue(expr, out int distance))
            return _environment.GetAt(distance, name.Lexeme);
        else
            return Globals.Get(name);
    }

    public object? VisitExprAssign(Expr.Assign expr)
    {
        object value = Evaluate(expr.Value);
        
        if (_locals.TryGetValue(expr, out int distance))
            _environment.AssignAt(distance, expr.Name, value);
        else
            Globals.Assign(expr.Name, value);
        
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

    public object? VisitExprLogical(Expr.Logical expr)
    {
        object left = Evaluate(expr.Lhs);

        if (expr.OperatorToken.Type == TokenType.Or)
        {
            if (IsTruthy(left))
                return left;
        }
        else
        {
            if (!IsTruthy(left))
                return left;
        }

        return Evaluate(expr.Rhs);
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

    internal void ExecuteBlock(List<Stmt> statements, Environment environment)
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

    internal void Resolve(Expr expr, int depth)
    {
        _locals[expr] = depth;
    }
}