using System.Text;

namespace Hex;

public class Hex
{
    internal static bool HadError = false;
    
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
        
        if (HadError) Environment.Exit(65);
    }

    private static void RunPrompt()
    {
        while (true)
        {
            Console.WriteLine("> ");
            String? line = Console.ReadLine();
            if (line == null) break;
            Run(line);
            HadError = false;
        }
    }

    private static void Run(string source)
    {
        Lexer lexer = new Lexer(source);
        List<Token> tokens = lexer.ScanTokens();
        
        foreach (Token token in tokens)
        {
            Console.WriteLine(token);
        }
    }

    internal static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    private static void Report(int line, string where, string message)
    {
        Console.WriteLine($"[line {line}] Error {where}: {message}");
        HadError = true;
    }
}