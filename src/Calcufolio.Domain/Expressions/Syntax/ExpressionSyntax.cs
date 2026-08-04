namespace Calcufolio.Domain.Expressions.Syntax;

public readonly record struct ExpressionSourceSpan
{
    public ExpressionSourceSpan(
        int position,
        int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(position);
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        Position = position;
        Length = length;
    }

    public int Position { get; }

    public int Length { get; }

    public int End =>
        checked(Position + Length);

    public static ExpressionSourceSpan FromBounds(
        int start,
        int end)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);

        if (end < start)
        {
            throw new ArgumentOutOfRangeException(
                nameof(end),
                end,
                "The source span end cannot precede its start.");
        }

        return new ExpressionSourceSpan(
            start,
            end - start);
    }
}

public abstract class ExpressionSyntax
{
    protected ExpressionSyntax(
        ExpressionSourceSpan span)
    {
        Span = span;
    }

    public ExpressionSourceSpan Span { get; }

    public int Position =>
        Span.Position;

    public int Length =>
        Span.Length;

    public int End =>
        Span.End;
}

public sealed class NumberExpressionSyntax : ExpressionSyntax
{
    internal NumberExpressionSyntax(
        double value,
        string lexeme,
        ExpressionSourceSpan span)
        : base(span)
    {
        ArgumentNullException.ThrowIfNull(lexeme);

        Value = value;
        Lexeme = lexeme;
    }

    public double Value { get; }

    public string Lexeme { get; }
}

public sealed class IdentifierExpressionSyntax : ExpressionSyntax
{
    internal IdentifierExpressionSyntax(
        string name,
        ExpressionSourceSpan span)
        : base(span)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
    }

    public string Name { get; }
}

public enum ExpressionUnaryOperator
{
    Positive,
    Negate,
}

public sealed class UnaryExpressionSyntax : ExpressionSyntax
{
    internal UnaryExpressionSyntax(
        ExpressionUnaryOperator operation,
        int operatorPosition,
        ExpressionSyntax operand,
        ExpressionSourceSpan span)
        : base(span)
    {
        ArgumentNullException.ThrowIfNull(operand);
        ArgumentOutOfRangeException.ThrowIfNegative(operatorPosition);

        Operation = operation;
        OperatorPosition = operatorPosition;
        Operand = operand;
    }

    public ExpressionUnaryOperator Operation { get; }

    public int OperatorPosition { get; }

    public ExpressionSyntax Operand { get; }
}

public enum ExpressionBinaryOperator
{
    Add,
    Subtract,
    Multiply,
    Divide,
    Power,
}

public sealed class BinaryExpressionSyntax : ExpressionSyntax
{
    internal BinaryExpressionSyntax(
        ExpressionSyntax left,
        ExpressionBinaryOperator operation,
        int operatorPosition,
        ExpressionSyntax right,
        ExpressionSourceSpan span)
        : base(span)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        ArgumentOutOfRangeException.ThrowIfNegative(operatorPosition);

        Left = left;
        Operation = operation;
        OperatorPosition = operatorPosition;
        Right = right;
    }

    public ExpressionSyntax Left { get; }

    public ExpressionBinaryOperator Operation { get; }

    public int OperatorPosition { get; }

    public ExpressionSyntax Right { get; }
}

public sealed class GroupExpressionSyntax : ExpressionSyntax
{
    internal GroupExpressionSyntax(
        ExpressionSyntax expression,
        ExpressionSourceSpan span)
        : base(span)
    {
        ArgumentNullException.ThrowIfNull(expression);

        Expression = expression;
    }

    public ExpressionSyntax Expression { get; }
}
