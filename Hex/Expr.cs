namespace Hex;

public abstract class Expr
{

    internal interface IVisitor<T>
    {
        T? VisitExprAssign(Assign expr);
        T? VisitExprTernary(Ternary expr);
        T? VisitExprBinary(Binary expr);
        T? VisitExprCall(Call expr);
        T? VisitExprGet(Get expr);
        T? VisitExprGrouping(Grouping expr);
        T? VisitExprLiteral(Literal expr);
        T? VisitExprLogical(Logical expr);
        T? VisitExprSet(Set expr);
        T? VisitExprThis(This expr);
        T? VisitExprUnary(Unary expr);
        T? VisitExprVariable(Variable expr);
    }

    internal abstract T Accept<T>(IVisitor<T> visitor);

    public class Assign : Expr
    {
        internal Assign(Token name, Expr value)
        {
            this.Name = name;
            this.Value = value;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitExprAssign(this);
        }

        internal readonly Token Name;
        internal readonly Expr Value;
    }

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

    public class Call : Expr
    {
        internal Call(Expr callee, Token paren, List<Expr> arguments)
        {
            this.Callee = callee;
            this.Paren = paren;
            this.Arguments = arguments;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitExprCall(this);
        }

        internal readonly Expr Callee;
        internal readonly Token Paren;
        internal readonly List<Expr> Arguments;
    }

    public class Get : Expr
    {
        internal Get(Expr obj, Token name)
        {
            this.Obj = obj;
            this.Name = name;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitExprGet(this);
        }

        internal readonly Expr Obj;
        internal readonly Token Name;
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

    public class Logical : Expr
    {
        internal Logical(Expr lhs, Token operatorToken, Expr rhs)
        {
            this.Lhs = lhs;
            this.OperatorToken = operatorToken;
            this.Rhs = rhs;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitExprLogical(this);
        }

        internal readonly Expr Lhs;
        internal readonly Token OperatorToken;
        internal readonly Expr Rhs;
    }

    public class Set : Expr
    {
        internal Set(Expr obj, Token name, Expr value)
        {
            this.Obj = obj;
            this.Name = name;
            this.Value = value;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitExprSet(this);
        }

        internal readonly Expr Obj;
        internal readonly Token Name;
        internal readonly Expr Value;
    }

    public class This : Expr
    {
        internal This(Token keyword)
        {
            this.Keyword = keyword;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitExprThis(this);
        }

        internal readonly Token Keyword;
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

    public class Variable : Expr
    {
        internal Variable(Token name)
        {
            this.Name = name;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitExprVariable(this);
        }

        internal readonly Token Name;
    }

}
