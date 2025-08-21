using System.Text;

namespace Hex;

public class AstPrinter : Expr.IVisitor<string>
{
    public string Print(Expr expr)
    {
        return expr.Accept(this);
    }
    
    public string VisitExprBinary(Expr.Binary expr)
    {
        return Parenthesize(expr.OperatorToken.Lexeme, [expr.Lhs, expr.Rhs]);
    }

    public string VisitExprGrouping(Expr.Grouping expr)
    {
        return Parenthesize("group", [expr.Expression]);
    }

    public string VisitExprLiteral(Expr.Literal expr)
    {
        return expr.Value == null ? "nix" : expr.Value.ToString();
    }

    public string VisitExprUnary(Expr.Unary expr)
    {
        return Parenthesize(expr.OperatorToken.Lexeme, [expr.Rhs]);
    }

    private string Parenthesize(string name, List<Expr> exprs)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append('(').Append(name);
        
        foreach (var expr in exprs)
        {
            builder.Append(' ');
            builder.Append(expr.Accept(this));
        }

        builder.Append(')');
        return builder.ToString();
    }
}