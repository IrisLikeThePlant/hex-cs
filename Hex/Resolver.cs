namespace Hex;

internal class Resolver : Expr.IVisitor<Object?>, Stmt.IVisitor<Object?>
{
    private readonly Interpreter _interpreter;
    private readonly Stack<Dictionary<string, bool>> _scopes = new();
    private FunctionType _currentFunction = FunctionType.None;
    private ClassType _currentClass = ClassType.None;

    internal Resolver(Interpreter interpreter)
    {
        this._interpreter = interpreter;
    }

    public object? VisitExprAssign(Expr.Assign expr)
    {
        Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);
        return null;
    }

    public object? VisitExprTernary(Expr.Ternary expr)
    {
        Resolve(expr.Condition);
        Resolve(expr.TrueBranch);
        Resolve(expr.FalseBranch);
        return null;
    }

    public object? VisitExprBinary(Expr.Binary expr)
    {
        Resolve(expr.Lhs);
        Resolve(expr.Rhs);
        return null;
    }

    public object? VisitExprCall(Expr.Call expr)
    {
        Resolve(expr.Callee);
        
        foreach(Expr argument in expr.Arguments)
            Resolve(argument);

        return null;
    }

    public object? VisitExprGet(Expr.Get expr)
    {
        Resolve(expr.Obj);
        return null;
    }

    public object? VisitExprSet(Expr.Set expr)
    {
        Resolve(expr.Value);
        Resolve(expr.Obj);
        return null;
    }

    public object? VisitExprSuper(Expr.Super expr)
    {
        if (_currentClass == ClassType.None)
            Hex.Error(expr.Keyword, "Can't use 'matron' outside of a class.");
        else if (_currentClass != ClassType.Subclass)
            Hex.Error(expr.Keyword, "Can't use 'matron' in a class with no superclass.");
        
        ResolveLocal(expr, expr.Keyword);
        return null;
    }

    public object? VisitExprThis(Expr.This expr)
    {
        if (_currentClass == ClassType.None)
        {
            Hex.Error(expr.Keyword, "Can't use 'this' outside of a class.");
            return null;
        }
        
        ResolveLocal(expr, expr.Keyword);
        return null;
    }

    public object? VisitExprGrouping(Expr.Grouping expr)
    {
        Resolve(expr.Expression);
        return null;
    }

    public object? VisitExprLiteral(Expr.Literal expr)
    {
        return null;
    }

    public object? VisitExprLogical(Expr.Logical expr)
    {
        Resolve(expr.Lhs);
        Resolve(expr.Rhs);
        return null;
    }

    public object? VisitExprUnary(Expr.Unary expr)
    {
        Resolve(expr.Rhs);
        return null;
    }

    public object? VisitExprVariable(Expr.Variable expr)
    {
        if (_scopes.Count != 0 && _scopes.Peek().TryGetValue(expr.Name.Lexeme, out bool canInitialize) && canInitialize == false)
            Hex.Error(expr.Name, "Can't read local variable in its own initializer.");

        ResolveLocal(expr, expr.Name);
        return null;
    }

    private void ResolveLocal(Expr expr, Token name)
    {
        var scopesArray = _scopes.ToArray();
        for (int i = 0; i < scopesArray.Length; i++)
        {
            if (scopesArray[i].ContainsKey(name.Lexeme))
            {
                _interpreter.Resolve(expr, i);
                return;
            }
        }
    }
    
    public object? VisitStmtBlock(Stmt.Block stmt)
    {
        BeginScope();
        Resolve(stmt.Statements);
        EndScope();
        return null;
    }

    public object? VisitStmtClass(Stmt.Class stmt)
    {
        ClassType enclosingClass = _currentClass;
        _currentClass = ClassType.Class;
        
        Declare(stmt.Name);
        Define(stmt.Name);
        
        if (stmt.SuperClass != null && stmt.Name.Lexeme.Equals(stmt.SuperClass.Name.Lexeme))
            Hex.Error(stmt.SuperClass.Name, "A class can't inherit from itself.");
        
        if (stmt.SuperClass != null)
        {
            _currentClass = ClassType.Subclass;
            Resolve(stmt.SuperClass);
        }

        if (stmt.SuperClass != null)
        {
            BeginScope();
            _scopes.Peek()["matron"] = true;
        }
        
        BeginScope();
        _scopes.Peek()["this"] = true;

        foreach (var method in stmt.Methods)
        {
            FunctionType declaration = FunctionType.Method;
            if (method.Name.Lexeme.Equals("this"))
                declaration = FunctionType.Initializer;
            
            ResolveFunction(method, declaration);
        }
        
        EndScope();
        
        if (stmt.SuperClass != null)
            EndScope();

        _currentClass = enclosingClass;
        return null;
    }

    public object? VisitStmtExpression(Stmt.Expression stmt)
    {
        Resolve(stmt.Expr);
        return null;
    }

    public object? VisitStmtFunction(Stmt.Function stmt)
    {
        Declare(stmt.Name);
        Define(stmt.Name);
        ResolveFunction(stmt, FunctionType.Function);
        return null;
    }

    private void ResolveFunction(Stmt.Function func, FunctionType type)
    {
        FunctionType enclosingFunction = _currentFunction;
        _currentFunction = type;
        
        BeginScope();
        foreach (var parameter in func.Parameters)
        {
            Declare(parameter);
            Define(parameter);
        }
        Resolve(func.Body);
        EndScope();

        _currentFunction = enclosingFunction;
    }

    public object? VisitStmtIf(Stmt.If stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.ThenBranch);
        if (stmt.ElseBranch != null)
            Resolve(stmt.ElseBranch);
        return null;
    }

    public object? VisitStmtPrint(Stmt.Print stmt)
    {
        Resolve(stmt.Expr);
        return null;
    }

    public object? VisitStmtReturn(Stmt.Return stmt)
    {
        if (_currentFunction == FunctionType.None)
            Hex.Error(stmt.Keyword, "Can't return from top-level code.");

        if (stmt.Value != null)
        {
            if (_currentFunction == FunctionType.Initializer)
                Hex.Error(stmt.Keyword, "Can't return a value from an initializer.");
            
            Resolve(stmt.Value);
        }
        return null;
    }

    public object? VisitStmtVar(Stmt.Var stmt)
    {
        Declare(stmt.Name);
        if (stmt.Initializer != null)
            Resolve(stmt.Initializer);
        Define(stmt.Name);
        return null;
    }

    private void Declare(Token name)
    {
        if (_scopes.Count == 0)
            return;

        Dictionary<string, bool> scope = _scopes.Peek();
        
        if (scope.ContainsKey(name.Lexeme))
            Hex.Error(name, "There already exists a variable with this name in this scope.");
        
        scope[name.Lexeme] = false;
    }

    private void Define(Token name)
    {
        if (_scopes.Count == 0)
            return;

        _scopes.Peek()[name.Lexeme] = true;
    }

    public object? VisitStmtWhile(Stmt.While stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.Body);
        return null;
    }

    private void BeginScope()
    {
        _scopes.Push(new Dictionary<string, bool>());
    }

    private void EndScope()
    {
        _scopes.Pop();
    }
    
    internal void Resolve(List<Stmt> statements)
    {
        foreach (Stmt statement in statements)
            Resolve(statement);
    }

    private void Resolve(Stmt stmt)
    {
        stmt.Accept(this);
    }

private void Resolve(Expr expr)
    {
        expr.Accept(this);
    }

    private enum FunctionType
    {
        None, Function, Initializer, Method
    }

    private enum ClassType
    {
        None, Class, Subclass
    }
}