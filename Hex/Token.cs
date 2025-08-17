namespace Hex;

public class Token
{
    private readonly TokenType _type;
    private readonly String _lexeme;
    private readonly Object _literal;
    private readonly int _line;

    Token(TokenType type, String lexeme, Object literal, int line)
    {
        this._type = type;
        this._lexeme = lexeme;
        this._literal = literal;
        this._line = line;
    }

    public override string ToString()
    {
        return _type + " " + _lexeme + " " + _literal;
    }
}