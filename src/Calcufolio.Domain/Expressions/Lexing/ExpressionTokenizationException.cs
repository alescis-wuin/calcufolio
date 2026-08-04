namespace Calcufolio.Domain.Expressions.Lexing;

public sealed class ExpressionTokenizationException : FormatException
{
    public ExpressionTokenizationException()
    {
    }

    public ExpressionTokenizationException(
        string? message)
        : base(message)
    {
    }

    public ExpressionTokenizationException(
        string? message,
        Exception? innerException)
        : base(message, innerException)
    {
    }

    public ExpressionTokenizationException(
        string message,
        int position)
        : base(
            $"{message} (position {position}).")
    {
        ArgumentOutOfRangeException.ThrowIfNegative(position);

        Position = position;
    }

    public int Position { get; } = -1;
}
