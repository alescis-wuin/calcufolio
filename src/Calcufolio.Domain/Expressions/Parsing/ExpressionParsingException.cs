namespace Calcufolio.Domain.Expressions.Parsing;

public sealed class ExpressionParsingException : FormatException
{
    public ExpressionParsingException()
    {
    }

    public ExpressionParsingException(
        string? message)
        : base(message)
    {
    }

    public ExpressionParsingException(
        string? message,
        Exception? innerException)
        : base(message, innerException)
    {
    }

    public ExpressionParsingException(
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
