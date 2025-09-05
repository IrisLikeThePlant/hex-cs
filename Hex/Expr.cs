namespace Hex;

public abstract class Expr
{

    internal interface IVisitor<T>
    {
        T VisitExprTernary(Ternary expr);
        T VisitExprBinary(Binary expr);
        T VisitExprGrouping(Grouping expr);
        T VisitExprLiteral(Literal expr);
        T VisitExprUnary(Unary expr);
    }

    internal abstract T Accept<T>(IVisitor<T> visitor);

    public class Ternary : Expr
    {
        internal Ternary(Expr condition, Expr trueBranch, Expr falseBranch)
        {
            this.Condition = condition;
            this.TrueBranch = trueBranch;
            this.FalseBranch = falseBranch;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitExprTernary(this);
        }

        internal readonly Expr Condition;
        internal readonly Expr TrueBranch;
        internal readonly Expr FalseBranch;
    }

    public class Binary : Expr
    {
        internal Binary(Expr lhs, Token operatorToken, Expr rhs)
        {
            this.Lhs = lhs;
            this.OperatorToken = operatorToken;
            this.Rhs = rhs;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitExprBinary(this);
        }

        internal readonly Expr Lhs;
        internal readonly Token OperatorToken;
        internal readonly Expr Rhs;
    }

    public class Grouping : Expr
    {
        internal Grouping(Expr expression)
        {
            this.Expression = expression;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitExprGrouping(this);
        }

        internal readonly Expr Expression;
    }

    public class Literal : Expr
    {
        internal Literal(Object value)
        {
            this.Value = value;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitExprLiteral(this);
        }

        internal readonly Object? Value;
    }

    public class Unary : Expr
    {
        internal Unary(Token operatorToken, Expr rhs)
        {
            this.OperatorToken = operatorToken;
            this.Rhs = rhs;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitExprUnary(this);
        }

        internal readonly Token OperatorToken;
        internal readonly Expr Rhs;
    }

}
