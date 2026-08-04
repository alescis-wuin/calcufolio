namespace Calcufolio.Application.Expressions;

public sealed record ExpressionEvaluationResult
{
    private ExpressionEvaluationResult(
        double? value,
        string? displayValue,
        ExpressionEvaluationError? error)
    {
        Value = value;
        DisplayValue = displayValue;
        Error = error;
    }

    public bool IsSuccess =>
        Error is null;

    public double? Value { get; }

    public string? DisplayValue { get; }

    public ExpressionEvaluationError? Error { get; }

    public static ExpressionEvaluationResult Success(
        double value,
        string displayValue)
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "The expression value must be finite.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            displayValue);

        return new ExpressionEvaluationResult(
            value,
            displayValue,
            null);
    }

    public static ExpressionEvaluationResult Failure(
        ExpressionEvaluationError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new ExpressionEvaluationResult(
            null,
            null,
            error);
    }
}
