namespace Hex;

internal interface ICallable
{
    internal int Arity();
    internal object? Call(Interpreter interpreter, List<object?> arguments);
}

internal class ClockFunction : ICallable
{
    public int Arity()
    {
        return 0;
    }

    public object Call(Interpreter interpreter, List<object?> arguments)
    {
        return (double)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
    }

    public override string ToString()
    {
        return "<native spell>";
    }
}