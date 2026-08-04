using Calcufolio.Domain.Expressions.Syntax;

namespace Calcufolio.Domain.Expressions.Evaluation;

public interface IExpressionEvaluator
{
    double Evaluate(
        ExpressionSyntax expression,
        IReadOnlyDictionary<string, double>? variables = null);
}
