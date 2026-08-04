using Calcufolio.Domain.Calculations;
using Calcufolio.Domain.Expressions.Evaluation;
using Calcufolio.Domain.Expressions.Lexing;
using Calcufolio.Domain.Expressions.Parsing;
using Calcufolio.Domain.Expressions.Syntax;

namespace Calcufolio.Domain.Tests.Expressions.Evaluation;

public sealed class ExpressionEvaluatorTests
{
    private readonly ExpressionParser _parser =
        new(
            new ExpressionTokenizer());

    private readonly ExpressionEvaluator _evaluator =
        new(
            new CalculationEngine());

    [Theory]
    [InlineData("1 + 2 * 3", 7.0)]
    [InlineData("(1 + 2) * 3", 9.0)]
    [InlineData("2^3^2", 512.0)]
    [InlineData("-2^2", -4.0)]
    [InlineData("2^-3", 0.125)]
    [InlineData("--2", 2.0)]
    [InlineData("5 - 2 - 1", 2.0)]
    [InlineData("8 / 4 / 2", 1.0)]
    [InlineData("1.25e2 + .5", 125.5)]
    [InlineData("+5", 5.0)]
    public void EvaluateReturnsExpectedResult(
        string expression,
        double expected)
    {
        double result =
            Evaluate(
                expression);

        Assert.Equal(
            expected,
            result,
            12);
    }

    [Fact]
    public void EvaluateResolvesIdentifiers()
    {
        Dictionary<string, double> variables =
            new(
                StringComparer.Ordinal)
            {
                ["x"] = 4.0,
                ["y"] = 3.0,
            };

        double result =
            Evaluate(
                "x * 2 + y",
                variables);

        Assert.Equal(
            11.0,
            result,
            12);
    }

    [Fact]
    public void EvaluateReportsUnknownIdentifierPosition()
    {
        ExpressionEvaluationException exception =
            Assert.Throws<ExpressionEvaluationException>(
                () => Evaluate(
                    "missing + 1"));

        Assert.Equal(
            0,
            exception.Position);

        Assert.Contains(
            "Unknown identifier 'missing'",
            exception.Message,
            StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void EvaluateRejectsNonFiniteIdentifierValue(
        double value)
    {
        Dictionary<string, double> variables =
            new(
                StringComparer.Ordinal)
            {
                ["value"] = value,
            };

        ExpressionEvaluationException exception =
            Assert.Throws<ExpressionEvaluationException>(
                () => Evaluate(
                    "2 + value",
                    variables));

        Assert.Equal(
            4,
            exception.Position);

        Assert.Contains(
            "does not contain a finite value",
            exception.Message,
            StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("1 / 0", 2)]
    [InlineData("1 / -0", 2)]
    public void EvaluateReportsDivisionByZeroAtOperator(
        string expression,
        int expectedPosition)
    {
        ExpressionEvaluationException exception =
            Assert.Throws<ExpressionEvaluationException>(
                () => Evaluate(
                    expression));

        Assert.Equal(
            expectedPosition,
            exception.Position);

        Assert.Contains(
            "Division by zero is not allowed",
            exception.Message,
            StringComparison.Ordinal);

        Assert.IsType<DivideByZeroException>(
            exception.InnerException);
    }

    [Fact]
    public void EvaluateReportsArithmeticOverflowAtOperator()
    {
        ExpressionEvaluationException exception =
            Assert.Throws<ExpressionEvaluationException>(
                () => Evaluate(
                    "1e308 * 1e308"));

        Assert.Equal(
            6,
            exception.Position);

        Assert.Contains(
            "outside the supported numeric range",
            exception.Message,
            StringComparison.Ordinal);

        Assert.IsType<OverflowException>(
            exception.InnerException);
    }

    [Theory]
    [InlineData("(-1)^.5", 4)]
    [InlineData("0^-1", 1)]
    public void EvaluateRejectsNonFinitePowerResult(
        string expression,
        int expectedPosition)
    {
        ExpressionEvaluationException exception =
            Assert.Throws<ExpressionEvaluationException>(
                () => Evaluate(
                    expression));

        Assert.Equal(
            expectedPosition,
            exception.Position);

        Assert.Contains(
            "not a finite number",
            exception.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void EvaluateRejectsMissingSyntax()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => _evaluator.Evaluate(
                    null!));

        Assert.Equal(
            "expression",
            exception.ParamName);
    }

    [Fact]
    public void ConstructorRejectsMissingCalculationEngine()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => new ExpressionEvaluator(
                    null!));

        Assert.Equal(
            "calculationEngine",
            exception.ParamName);
    }

    [Fact]
    public void EvaluateRejectsUnsupportedSyntaxType()
    {
        UnsupportedExpressionSyntax expression =
            new();

        NotSupportedException exception =
            Assert.Throws<NotSupportedException>(
                () => _evaluator.Evaluate(
                    expression));

        Assert.Contains(
            nameof(UnsupportedExpressionSyntax),
            exception.Message,
            StringComparison.Ordinal);
    }

    private double Evaluate(
        string expression,
        IReadOnlyDictionary<string, double>? variables = null)
    {
        ExpressionSyntax syntax =
            _parser.Parse(
                expression);

        return _evaluator.Evaluate(
            syntax,
            variables);
    }

    private sealed class UnsupportedExpressionSyntax
        : ExpressionSyntax
    {
        public UnsupportedExpressionSyntax()
            : base(
                new ExpressionSourceSpan(
                    0,
                    0))
        {
        }
    }
}
