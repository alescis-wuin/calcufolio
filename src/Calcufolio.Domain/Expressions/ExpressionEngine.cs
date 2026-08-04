using Calcufolio.Domain.Expressions.Evaluation;
using Calcufolio.Domain.Expressions.Parsing;
using Calcufolio.Domain.Expressions.Syntax;

namespace Calcufolio.Domain.Expressions;

public sealed class ExpressionEngine : IExpressionEngine
{
    private readonly IExpressionParser _parser;
    private readonly IExpressionEvaluator _evaluator;

    public ExpressionEngine(
        IExpressionParser parser,
        IExpressionEvaluator evaluator)
    {
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(evaluator);

        _parser = parser;
        _evaluator = evaluator;
    }

    public double Evaluate(
        string expression,
        IReadOnlyDictionary<string, double>? variables = null)
    {
        ArgumentNullException.ThrowIfNull(expression);

        ExpressionSyntax syntax =
            _parser.Parse(
                expression);

        return _evaluator.Evaluate(
            syntax,
            variables);
    }
}
