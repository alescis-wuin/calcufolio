namespace Calcufolio.Domain.Expressions.Evaluation;

public sealed class ExpressionEvaluationException : ArithmeticException
{
    public ExpressionEvaluationException()
    {
    }

    public ExpressionEvaluationException(
        string? message)
        : base(message)
    {
    }

    public ExpressionEvaluationException(
        string? message,
        Exception? innerException)
        : base(message, innerException)
    {
    }

    public ExpressionEvaluationException(
        string message,
        int position)
        : this(
            message,
            position,
            null)
    {
    }

    public ExpressionEvaluationException(
        string message,
        int position,
        Exception? innerException)
        : base(
            FormatMessage(
                message,
                position),
            innerException)
    {
        Position = position;
    }

    public int Position { get; } = -1;

    private static string FormatMessage(
        string message,
        int position)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentOutOfRangeException.ThrowIfNegative(position);

        return $"{message} (position {position}).";
    }
}
