namespace Calcufolio.Application.Expressions;

public sealed record ExpressionEvaluationError
{
    public ExpressionEvaluationError(
        ExpressionEvaluationErrorKind kind,
        string message,
        int? position)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        if (position is < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(position),
                position,
                "The expression error position must not be negative.");
        }

        Kind = kind;
        Message = message;
        Position = position;
    }

    public ExpressionEvaluationErrorKind Kind { get; }

    public string Message { get; }

    public int? Position { get; }
}
