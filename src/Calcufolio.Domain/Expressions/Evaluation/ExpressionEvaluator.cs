using System.Collections.ObjectModel;
using Calcufolio.Domain.Calculations;
using Calcufolio.Domain.Expressions.Syntax;

namespace Calcufolio.Domain.Expressions.Evaluation;

public sealed class ExpressionEvaluator : IExpressionEvaluator
{
    private static readonly ReadOnlyDictionary<string, double>
        _emptyVariables =
            new(
                new Dictionary<string, double>(
                    StringComparer.Ordinal));

    private readonly ICalculationEngine _calculationEngine;

    public ExpressionEvaluator(
        ICalculationEngine calculationEngine)
    {
        ArgumentNullException.ThrowIfNull(calculationEngine);

        _calculationEngine = calculationEngine;
    }

    public double Evaluate(
        ExpressionSyntax expression,
        IReadOnlyDictionary<string, double>? variables = null)
    {
        ArgumentNullException.ThrowIfNull(expression);

        return EvaluateCore(
            expression,
            variables ?? _emptyVariables);
    }

    private double EvaluateCore(
        ExpressionSyntax expression,
        IReadOnlyDictionary<string, double> variables)
    {
        return expression switch
        {
            NumberExpressionSyntax number =>
                number.Value,

            IdentifierExpressionSyntax identifier =>
                ResolveIdentifier(
                    identifier,
                    variables),

            GroupExpressionSyntax group =>
                EvaluateCore(
                    group.Expression,
                    variables),

            UnaryExpressionSyntax unary =>
                EvaluateUnary(
                    unary,
                    variables),

            BinaryExpressionSyntax binary =>
                EvaluateBinary(
                    binary,
                    variables),

            _ => throw new NotSupportedException(
                $"Expression syntax type '{expression.GetType().Name}' is not supported."),
        };
    }

    private double EvaluateUnary(
        UnaryExpressionSyntax unary,
        IReadOnlyDictionary<string, double> variables)
    {
        double operand =
            EvaluateCore(
                unary.Operand,
                variables);

        double result = unary.Operation switch
        {
            ExpressionUnaryOperator.Positive =>
                operand,

            ExpressionUnaryOperator.Negate =>
                -operand,

            _ => throw new InvalidOperationException(
                "The unary expression operator is not supported."),
        };

        return EnsureFiniteResult(
            result,
            unary.OperatorPosition);
    }

    private double EvaluateBinary(
        BinaryExpressionSyntax binary,
        IReadOnlyDictionary<string, double> variables)
    {
        double left =
            EvaluateCore(
                binary.Left,
                variables);

        double right =
            EvaluateCore(
                binary.Right,
                variables);

        try
        {
            double result = binary.Operation switch
            {
                ExpressionBinaryOperator.Add =>
                    _calculationEngine.Calculate(
                        left,
                        BinaryOperator.Add,
                        right),

                ExpressionBinaryOperator.Subtract =>
                    _calculationEngine.Calculate(
                        left,
                        BinaryOperator.Subtract,
                        right),

                ExpressionBinaryOperator.Multiply =>
                    _calculationEngine.Calculate(
                        left,
                        BinaryOperator.Multiply,
                        right),

                ExpressionBinaryOperator.Divide =>
                    _calculationEngine.Calculate(
                        left,
                        BinaryOperator.Divide,
                        right),

                ExpressionBinaryOperator.Power =>
                    Math.Pow(
                        left,
                        right),

                _ => throw new InvalidOperationException(
                    "The binary expression operator is not supported."),
            };

            return EnsureFiniteResult(
                result,
                binary.OperatorPosition);
        }
        catch (DivideByZeroException exception)
        {
            throw new ExpressionEvaluationException(
                "Division by zero is not allowed",
                binary.OperatorPosition,
                exception);
        }
        catch (OverflowException exception)
        {
            throw new ExpressionEvaluationException(
                "The calculation result is outside the supported numeric range",
                binary.OperatorPosition,
                exception);
        }
    }

    private static double ResolveIdentifier(
        IdentifierExpressionSyntax identifier,
        IReadOnlyDictionary<string, double> variables)
    {
        if (!variables.TryGetValue(
                identifier.Name,
                out double value))
        {
            throw new ExpressionEvaluationException(
                $"Unknown identifier '{identifier.Name}'",
                identifier.Position);
        }

        if (!double.IsFinite(value))
        {
            throw new ExpressionEvaluationException(
                $"Identifier '{identifier.Name}' does not contain a finite value",
                identifier.Position);
        }

        return value;
    }

    private static double EnsureFiniteResult(
        double result,
        int position)
    {
        if (!double.IsFinite(result))
        {
            throw new ExpressionEvaluationException(
                "The operation result is not a finite number",
                position);
        }

        return result;
    }
}
