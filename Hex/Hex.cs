using System.Text;

namespace Hex;

public class Hex
{
    private static readonly Interpreter Interpreter = new Interpreter();
    
    private static bool _hadError = false;
    private static bool _hadRuntimeError = false;
    
    public static void Main(string[] args)
    {
        switch (args.Length)
        {
            case > 1:
                Console.WriteLine("Usage: hex [script]");
                break;
            case 1:
                RunFile(args[0]);
                break;
            default:
                RunPrompt();
                break;
        }
    }

    private static void RunFile(String path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        string content = Encoding.Default.GetString(bytes);
        Run(content);
        
        if (_hadError) Environment.Exit(65);
        if (_hadRuntimeError) Environment.Exit(70);
    }

    private static void RunPrompt()
    {
        while (true)
        {
            Console.WriteLine("> ");
            String? line = Console.ReadLine();
            if (line == null) break;
            Run(line);
            _hadError = false;
            _hadRuntimeError = false;
        }
    }

    private static void Run(string source)
    {
        Lexer lexer = new Lexer(source);
        List<Token> tokens = lexer.ScanTokens();

        Parser parser = new Parser(tokens);
        Expr? expression = parser.Parse();
        
        if (_hadError) return;
        Interpreter.Interpret(expression);
    }

    internal static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    internal static void Error(Token token, string message)
    {
        if (token.Type == TokenType.Eof)
            Report(token.Line, "at end", message);
        else
            Report(token.Line, "at '" + token.Lexeme + "'", message);
    }

    internal static void RuntimeError(RuntimeError error)
    {
        Console.WriteLine(error.Message + "\n[line " + error.Token.Line + "]");
        _hadRuntimeError = true;
    }

    private static void Report(int line, string where, string message)
    {
        Console.WriteLine($"[line {line}] Error {where}: {message}");
        _hadError = true;
    }
}