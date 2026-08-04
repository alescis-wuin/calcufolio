namespace Calcufolio.Application.Expressions;

public interface IExpressionEvaluationService
{
    ExpressionEvaluationResult Evaluate(
        string expression,
        IReadOnlyDictionary<string, double>? variables = null);
}
