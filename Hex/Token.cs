namespace Hex;

public class Token
{
    internal readonly TokenType Type;
    internal readonly string Lexeme;
    internal readonly Object? Literal;
    internal readonly int Line;

    internal Token(TokenType type, string lexeme, Object literal, int line)
    {
        this.Type = type;
        this.Lexeme = lexeme;
        this.Literal = literal;
        this.Line = line;
    }

    public override string ToString()
    {
        return Type + " " + Lexeme + " " + Literal;
    }
}