namespace Hex;

public abstract class Stmt
{

    internal interface IVisitor<T>
    {
        T? VisitStmtBlock(Block stmt);
        T? VisitStmtExpression(Expression stmt);
        T? VisitStmtFunction(Function stmt);
        T? VisitStmtIf(If stmt);
        T? VisitStmtPrint(Print stmt);
        T? VisitStmtReturn(Return stmt);
        T? VisitStmtVar(Var stmt);
        T? VisitStmtWhile(While stmt);
    }

    internal abstract T Accept<T>(IVisitor<T> visitor);

    public class Block : Stmt
    {
        internal Block(List<Stmt> statements)
        {
            this.Statements = statements;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitStmtBlock(this);
        }

        internal readonly List<Stmt> Statements;
    }

    public class Expression : Stmt
    {
        internal Expression(Expr expr)
        {
            this.Expr = expr;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitStmtExpression(this);
        }

        internal readonly Expr Expr;
    }

    public class Function : Stmt
    {
        internal Function(Token name, List<Token> parameters, List<Stmt> body)
        {
            this.Name = name;
            this.Parameters = parameters;
            this.Body = body;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitStmtFunction(this);
        }

        internal readonly Token Name;
        internal readonly List<Token> Parameters;
        internal readonly List<Stmt> Body;
    }

    public class If : Stmt
    {
        internal If(Expr condition, Stmt thenBranch, Stmt elseBranch)
        {
            this.Condition = condition;
            this.ThenBranch = thenBranch;
            this.ElseBranch = elseBranch;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitStmtIf(this);
        }

        internal readonly Expr Condition;
        internal readonly Stmt ThenBranch;
        internal readonly Stmt ElseBranch;
    }

    public class Print : Stmt
    {
        internal Print(Expr expr)
        {
            this.Expr = expr;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitStmtPrint(this);
        }

        internal readonly Expr Expr;
    }

    public class Return : Stmt
    {
        internal Return(Token keyword, Expr value)
        {
            this.Keyword = keyword;
            this.Value = value;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitStmtReturn(this);
        }

        internal readonly Token Keyword;
        internal readonly Expr Value;
    }

    public class Var : Stmt
    {
        internal Var(Token name, Expr initializer)
        {
            this.Name = name;
            this.Initializer = initializer;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitStmtVar(this);
        }

        internal readonly Token Name;
        internal readonly Expr Initializer;
    }

    public class While : Stmt
    {
        internal While(Expr condition, Stmt body)
        {
            this.Condition = condition;
            this.Body = body;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitStmtWhile(this);
        }

        internal readonly Expr Condition;
        internal readonly Stmt Body;
    }

}
