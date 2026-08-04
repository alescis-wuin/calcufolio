namespace Calcufolio.Domain.Expressions;

public interface IExpressionEngine
{
    double Evaluate(
        string expression,
        IReadOnlyDictionary<string, double>? variables = null);
}
