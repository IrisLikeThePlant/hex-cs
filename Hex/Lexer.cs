namespace Hex;

public class Lexer
{
    private readonly string _source;
    private readonly List<Token> _tokens = new List<Token>();
    private int _start = 0;
    private int _current = 0;
    private int _line = 1;
    private static readonly IReadOnlyDictionary<string, TokenType> Keywords;

    static Lexer()
    {
        Keywords = new Dictionary<string, TokenType>
        {
            { "and", TokenType.And },
            { "grimoire", TokenType.Class },
            { "else", TokenType.Else },
            { "false", TokenType.False },
            { "for", TokenType.For },
            { "spell", TokenType.Fun },
            { "if", TokenType.If },
            { "nix", TokenType.Nil },
            { "or", TokenType.Or },
            { "summon", TokenType.Print },
            { "cast", TokenType.Return },
            { "matron", TokenType.Super },
            { "this", TokenType.This },
            { "true", TokenType.True },
            { "rune", TokenType.Var },
            { "while", TokenType.While }
        };
    }

    internal Lexer(string source)
    {
        this._source = source;
    }

    internal List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            ScanToken();
        }
        _tokens.Add(new Token(TokenType.Eof, "", null, _line));
        return _tokens;
    }

    private bool IsAtEnd()
    {
        return _current >= _source.Length;
    }

    private void ScanToken()
    {
        char c = Consume();
        switch (c)
        {
            case '\uFEFF': break; // UTF-8 BOM marker
            case '(': AddToken(TokenType.LeftParen); break;
            case ')': AddToken(TokenType.RightParen); break;
            case '{': AddToken(TokenType.LeftBrace); break;
            case '}': AddToken(TokenType.RightBrace); break;
            case ',': AddToken(TokenType.Comma); break;
            case '.': AddToken(TokenType.Dot); break;
            case '-': AddToken(TokenType.Minus); break;
            case '+': AddToken(TokenType.Plus); break;
            case ';': AddToken(TokenType.Semicolon); break;
            case '*': AddToken(TokenType.Star); break;
            case ':': AddToken(TokenType.Colon); break;
            case '?': AddToken(TokenType.Question); break;
            case '!':
                AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang); break;
            case '=':
                AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal); break;
            case '<':
                AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less); break;
            case '>':
                AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater); break;
            case '/':
                if (Match('/'))
                    while (Peek() != '\n' && !IsAtEnd()) Consume();
                else if (Match('*'))
                {
                    while (Peek() != '*' && PeekNext() != '/' && !IsAtEnd())
                        Consume();
                    Consume();
                    Consume();
                }
                else
                    AddToken(TokenType.Slash);
                break;
            case ' ':
            case '\r':
            case '\t':
                break;
            case '\n':
                _line++; break;
            case '"': String(); break;
            default:
                if (IsDigit(c)) Number(); 
                else if (IsAlpha(c)) Identifier();
                else Hex.Error(_line, "Unexpected character: " + c);
                break;
        }
    }

    private char Consume()
    {
        _current++;
        return _source.ElementAt(_current - 1);
    }

    private void AddToken(TokenType type)
    {
        AddToken(type, null);
    }

    private void AddToken(TokenType type, Object literal)
    {
        string text = _source.Substring(_start, _current - _start);
        _tokens.Add(new Token(type, text, literal, _line));
    }

    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (_source.ElementAt(_current) != expected) return false;

        _current++;
        return true;
    }

    private char Peek()
    {
        if (IsAtEnd()) return '\0';
        return _source.ElementAt(_current);
    }

    private void String()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n') _line++;
            Consume();
        }

        if (IsAtEnd())
        {
            Hex.Error(_line, "Unterminated string.");
            return;
        }

        Consume();
        string value = _source.Substring(_start + 1, _current - _start - 2);
        AddToken(TokenType.String, value);
    }

    private bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    private void Number()
    {
        while (IsDigit(Peek())) Consume();

        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            Consume();
            while (IsDigit(Peek())) Consume();
        }
        
        AddToken(TokenType.Number, Double.Parse(_source.Substring(_start, _current - _start)));
    }

    private char PeekNext()
    {
        if (_current + 1 >= _source.Length) return '\0';
        return _source.ElementAt(_current + 1);
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek())) Consume();
        string text = _source.Substring(_start, _current - _start);
        if (!Keywords.TryGetValue(text, out TokenType type))
            type = TokenType.Identifier;
        AddToken(type);
    }

    private bool IsAlpha(char c)
    {
        return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
    }

    private bool IsAlphaNumeric(char c)
    {
        return IsDigit(c) || IsAlpha(c);
    }
}