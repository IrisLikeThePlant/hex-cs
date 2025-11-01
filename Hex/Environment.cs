namespace Hex;

public class Environment
{
    public readonly Environment? Enclosing;
    private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

    public Environment()
    {
        Enclosing = null;
    }

    public Environment(Environment enclosing)
    {
        this.Enclosing = enclosing;
    }
    
    internal void Define(string name, object value)
    {
        _values.Add(name, value);
    }

    internal object Get(Token name)
    {
        if (_values.ContainsKey(name.Lexeme))
            return _values[name.Lexeme];

        if (Enclosing != null)
            return Enclosing.Get(name);
        
        throw new RuntimeError(name, "Undefined variable '" + name.Lexeme + "'.");
    }

    internal object? GetAt(int distance, string name)
    {
        Ancestor(distance)._values.TryGetValue(name, out var value);
        return value;
    }

    private Environment Ancestor(int distance)
    {
        Environment environment = this;
        for (int i = 0; i < distance; i++)
            environment = environment.Enclosing;
        return environment;
    }

    internal void Assign(Token name, object value)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            _values[name.Lexeme] = value;
            return;
        }

        if (Enclosing != null)
        {
            Enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeError(name, "Undefined variable '" + name.Lexeme + "'.");
    }

    internal void AssignAt(int distance, Token name, object value)
    {
        Ancestor(distance)._values[name.Lexeme] = value;
    }
}