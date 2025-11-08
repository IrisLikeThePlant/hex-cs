namespace Tool;

public class GenerateAst
{
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            TextWriter errorWriter = Console.Error;
            errorWriter.WriteLine("Usage: generate_ast <output directory>");
            Environment.Exit(64);
        }

        string outputDir = args[0];
        DefineAst(outputDir, "Expr", [
            "Assign   : Token name, Expr value",
            "Ternary  : Expr condition, Expr trueBranch, Expr falseBranch",
            "Binary   : Expr lhs, Token operatorToken, Expr rhs",
            "Call     : Expr callee, Token paren, List<Expr> arguments",
            "Get      : Expr obj, Token name",
            "Grouping : Expr expression",
            "Literal  : Object value",
            "Logical  : Expr lhs, Token operatorToken, Expr rhs",
            "Set      : Expr obj, Token name, Expr value",
            "Super    : Token keyword, Token method", 
            "This     : Token keyword",
            "Unary    : Token operatorToken, Expr rhs",
            "Variable : Token name"
        ]);
        DefineAst(outputDir, "Stmt", [
            "Block      : List<Stmt> statements",
            "Class      : Token name, Expr.Variable? superClass, List<Stmt.Function> methods",
            "Expression : Expr expr",
            "Function   : Token name, List<Token> parameters, List<Stmt> body",
            "If         : Expr condition, Stmt thenBranch, Stmt elseBranch",
            "Print      : Expr expr",
            "Return     : Token keyword, Expr value",
            "Var        : Token name, Expr initializer",
            "While      : Expr condition, Stmt body"
        ]);
    }

    private static void DefineAst(string outputDir, string baseName, List<string> types)
    {
        string path = outputDir + "/" + baseName + ".cs";
        StreamWriter writer = new StreamWriter(path);
        
        writer.WriteLine("namespace Hex;");
        writer.WriteLine();
        writer.WriteLine("public abstract class " + baseName);
        writer.WriteLine("{");
        writer.WriteLine();

        DefineVisitor(writer, baseName, types);
        
        writer.WriteLine();
        writer.WriteLine("    internal abstract T Accept<T>(IVisitor<T> visitor);");
        writer.WriteLine();

        foreach (var type in types)
        {
            string className = type.Split(":")[0].Trim();
            string fields = type.Split(":")[1].Trim();
            DefineType(writer, baseName, className, fields);
        }
        
        writer.WriteLine("}");
        writer.Close();
    }

    private static void DefineType(StreamWriter writer, string baseName, string className, string fieldList)
    {
        writer.WriteLine("    public class " + className + " : " + baseName);
        writer.WriteLine("    {");
        writer.WriteLine("        internal " + className + "(" + fieldList + NullableIfNullable(fieldList) +")");
        writer.WriteLine("        {");

        string[] fields = fieldList.Split(", ");
        foreach (var field in fields)
        {
            string name = field.Split(" ")[1];
            writer.WriteLine("            this." + char.ToUpper(name[0]) + name.Substring(1) + " = " + name + ";");
        }
        
        writer.WriteLine("        }");
        writer.WriteLine();
        writer.WriteLine("        internal override T Accept<T>(IVisitor<T> visitor)");
        writer.WriteLine("        {");
        writer.WriteLine("            return visitor.Visit" + baseName + className + "(this);");
        writer.WriteLine("        }");
        writer.WriteLine();
        
        foreach (var field in fields)
        {
            string type = field.Split(" ")[0];
            string name = field.Split(" ")[1];
            writer.WriteLine("        internal readonly " + type + NullableIfNullable(type) + " " + char.ToUpper(name[0]) + name.Substring(1) + ";");
        }
        
        writer.WriteLine("    }\n");
    }

    private static void DefineVisitor(StreamWriter writer, string baseName, List<string> types)
    {
        writer.WriteLine("    internal interface IVisitor<T>");
        writer.WriteLine("    {");
        foreach (var type in types)
        {
            string typeName = type.Split(":")[0].Trim();
            writer.WriteLine("        T? Visit" + baseName + typeName + "(" + typeName + " " + baseName.ToLower() + ");");
        }
        writer.WriteLine("    }");
    }

    private static string NullableIfNullable(string type)
    {
        return type == "Object" ? "?" : "";
    }
}