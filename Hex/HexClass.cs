namespace Hex;

internal class HexClass : ICallable
{
    internal readonly string Name;
    internal readonly HexClass? SuperClass;
    private readonly Dictionary<string, HexFunction> _methods;

    internal HexClass(string name, HexClass superClass, Dictionary<string, HexFunction> methods)
    {
        this.Name = name;
        this._methods = methods;
        this.SuperClass = superClass;
    }

    public override string ToString()
    {
        return Name;
    }

    public int Arity()
    {
        HexFunction? initializer = FindMethod("init");
        if (initializer == null) return 0;
        return initializer.Arity();
    }

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        HexInstance instance = new HexInstance(this);
        HexFunction? initializer = FindMethod("init");
        initializer?.Bind(instance).Call(interpreter, arguments);
        return instance;
    }

    internal HexFunction? FindMethod(string name)
    {
        if (_methods.TryGetValue(name, out var method))
            return method;

        if (SuperClass != null)
            return SuperClass.FindMethod(name);

        return null;
    }
}