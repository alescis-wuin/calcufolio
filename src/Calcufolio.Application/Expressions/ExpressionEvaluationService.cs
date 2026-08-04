using System.Globalization;
using Calcufolio.Domain.Expressions;
using Calcufolio.Domain.Expressions.Evaluation;
using Calcufolio.Domain.Expressions.Lexing;
using Calcufolio.Domain.Expressions.Parsing;

namespace Calcufolio.Application.Expressions;

public sealed class ExpressionEvaluationService
    : IExpressionEvaluationService
{
    private readonly IExpressionEngine _expressionEngine;

    public ExpressionEvaluationService(
        IExpressionEngine expressionEngine)
    {
        ArgumentNullException.ThrowIfNull(expressionEngine);

        _expressionEngine = expressionEngine;
    }

    public ExpressionEvaluationResult Evaluate(
        string expression,
        IReadOnlyDictionary<string, double>? variables = null)
    {
        ArgumentNullException.ThrowIfNull(expression);

        try
        {
            double value =
                _expressionEngine.Evaluate(
                    expression,
                    variables);

            return ExpressionEvaluationResult.Success(
                value,
                FormatNumber(value));
        }
        catch (ExpressionTokenizationException exception)
        {
            return CreateFailure(
                ExpressionEvaluationErrorKind.Tokenization,
                exception.Message,
                exception.Position);
        }
        catch (ExpressionParsingException exception)
        {
            return CreateFailure(
                ExpressionEvaluationErrorKind.Parsing,
                exception.Message,
                exception.Position);
        }
        catch (ExpressionEvaluationException exception)
        {
            return CreateFailure(
                ExpressionEvaluationErrorKind.Evaluation,
                exception.Message,
                exception.Position);
        }
    }

    private static ExpressionEvaluationResult CreateFailure(
        ExpressionEvaluationErrorKind kind,
        string message,
        int position)
    {
        return ExpressionEvaluationResult.Failure(
            new ExpressionEvaluationError(
                kind,
                message,
                position >= 0
                    ? position
                    : null));
    }

    private static string FormatNumber(
        double value)
    {
        if (value == 0.0)
        {
            return "0";
        }

        return value.ToString(
            "G15",
            CultureInfo.InvariantCulture);
    }
}
