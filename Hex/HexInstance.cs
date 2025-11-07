namespace Hex;

internal class HexInstance
{
    private HexClass _class;
    private readonly Dictionary<string, object> _fields = new();

    internal HexInstance(HexClass klass)
    {
        this._class = klass;
    }

    internal object? Get(Token name)
    {
        if (_fields.ContainsKey(name.Lexeme))
            return _fields[name.Lexeme];

        HexFunction? method = _class.FindMethod(name.Lexeme);
        if (method != null) return method.Bind(this);

        throw new RuntimeError(name, "Undefined property '" + name.Lexeme + "'.");
    }

    internal void Set(Token name, object value)
    {
        _fields.Add(name.Lexeme, value);
    }

    public override string ToString()
    {
        return _class.Name + " instance";
    }
}