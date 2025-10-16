namespace Hex;

internal class Return : Exception
{
    internal readonly object? Value;

    internal Return(object? value) : base(null)
    {
        this.Value = value;
    }
}