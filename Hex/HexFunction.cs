namespace Hex;

internal class HexFunction : ICallable
{
    private readonly Stmt.Function _declaration;
    private readonly Environment _closure;
    private readonly bool _isInitializer;

    internal HexFunction(Stmt.Function declaration, Environment closure, bool isInitializer)
    {
        this._declaration = declaration;
        this._closure = closure;
        this._isInitializer = isInitializer;
    }
    
    public int Arity()
    {
        return _declaration.Parameters.Count;
    }

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        Environment environment = new Environment(_closure);

        for (int i = 0; i < _declaration.Parameters.Count; i++)
        {
            environment.Define(_declaration.Parameters[i].Lexeme, arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(_declaration.Body, environment);
        }
        catch (Return returnValue)
        {
            if (_isInitializer) return _closure.GetAt(0, "this");
            return returnValue.Value;
        }

        if (_isInitializer) return _closure.GetAt(0, "this");
        return null;
    }

    public HexFunction Bind(HexInstance instance)
    {
        Environment environment = new Environment(_closure);
        environment.Define("this", instance);
        return new HexFunction(_declaration, environment, _isInitializer);
    }
    
    public override string ToString()
    {
        return "<spell " + _declaration.Name.Lexeme + ">";
    }
}