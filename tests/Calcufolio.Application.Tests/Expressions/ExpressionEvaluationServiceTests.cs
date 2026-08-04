using Calcufolio.Application.Expressions;
using Calcufolio.Domain.Calculations;
using Calcufolio.Domain.Expressions;
using Calcufolio.Domain.Expressions.Evaluation;
using Calcufolio.Domain.Expressions.Lexing;
using Calcufolio.Domain.Expressions.Parsing;

namespace Calcufolio.Application.Tests.Expressions;

public sealed class ExpressionEvaluationServiceTests
{
    [Theory]
    [InlineData("1 + 2 * 3", 7.0, "7")]
    [InlineData("1 / 4", 0.25, "0.25")]
    [InlineData("-0", 0.0, "0")]
    public void EvaluateReturnsFormattedSuccess(
        string expression,
        double expectedValue,
        string expectedDisplayValue)
    {
        ExpressionEvaluationService service =
            CreateService();

        ExpressionEvaluationResult result =
            service.Evaluate(
                expression);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            expectedValue,
            result.Value);

        Assert.Equal(
            expectedDisplayValue,
            result.DisplayValue);

        Assert.Null(result.Error);
    }

    [Fact]
    public void EvaluateForwardsVariables()
    {
        ExpressionEvaluationService service =
            CreateService();

        Dictionary<string, double> variables =
            new(
                StringComparer.Ordinal)
            {
                ["principal"] = 1250.0,
                ["rate"] = 0.04,
            };

        ExpressionEvaluationResult result =
            service.Evaluate(
                "principal * (1 + rate)",
                variables);

        Assert.True(result.IsSuccess);

        Assert.Equal(
            1300.0,
            result.Value);

        Assert.Equal(
            "1300",
            result.DisplayValue);
    }

    [Fact]
    public void EvaluateForwardsExpressionAndVariableReference()
    {
        RecordingExpressionEngine engine =
            new(
                42.5);

        ExpressionEvaluationService service =
            new(
                engine);

        Dictionary<string, double> variables =
            new(
                StringComparer.Ordinal)
            {
                ["value"] = 42.5,
            };

        ExpressionEvaluationResult result =
            service.Evaluate(
                "value",
                variables);

        Assert.Equal(
            "value",
            engine.Expression);

        Assert.Same(
            variables,
            engine.Variables);

        Assert.Equal(
            42.5,
            result.Value);

        Assert.Equal(
            "42.5",
            result.DisplayValue);
    }

    [Fact]
    public void EvaluateMapsTokenizationFailure()
    {
        ExpressionEvaluationResult result =
            CreateService().Evaluate(
                "1 @ 2");

        ExpressionEvaluationError error =
            Assert.IsType<ExpressionEvaluationError>(
                result.Error);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Null(result.DisplayValue);

        Assert.Equal(
            ExpressionEvaluationErrorKind.Tokenization,
            error.Kind);

        Assert.Equal(
            2,
            error.Position);

        Assert.Contains(
            "Unsupported expression character",
            error.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void EvaluateMapsParsingFailure()
    {
        ExpressionEvaluationResult result =
            CreateService().Evaluate(
                "(1 + 2");

        ExpressionEvaluationError error =
            Assert.IsType<ExpressionEvaluationError>(
                result.Error);

        Assert.False(result.IsSuccess);

        Assert.Equal(
            ExpressionEvaluationErrorKind.Parsing,
            error.Kind);

        Assert.Equal(
            6,
            error.Position);
    }

    [Fact]
    public void EvaluateMapsEvaluationFailure()
    {
        ExpressionEvaluationResult result =
            CreateService().Evaluate(
                "1 / 0");

        ExpressionEvaluationError error =
            Assert.IsType<ExpressionEvaluationError>(
                result.Error);

        Assert.False(result.IsSuccess);

        Assert.Equal(
            ExpressionEvaluationErrorKind.Evaluation,
            error.Kind);

        Assert.Equal(
            2,
            error.Position);

        Assert.Contains(
            "Division by zero",
            error.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void EvaluateMapsUnpositionedDomainFailure()
    {
        ThrowingExpressionEngine engine =
            new(
                new ExpressionEvaluationException(
                    "Evaluation failed."));

        ExpressionEvaluationService service =
            new(
                engine);

        ExpressionEvaluationResult result =
            service.Evaluate(
                "expression");

        ExpressionEvaluationError error =
            Assert.IsType<ExpressionEvaluationError>(
                result.Error);

        Assert.Equal(
            ExpressionEvaluationErrorKind.Evaluation,
            error.Kind);

        Assert.Null(error.Position);
    }

    [Fact]
    public void EvaluateDoesNotSuppressUnexpectedFailure()
    {
        InvalidOperationException expected =
            new(
                "Unexpected failure.");

        ExpressionEvaluationService service =
            new(
                new ThrowingExpressionEngine(
                    expected));

        InvalidOperationException actual =
            Assert.Throws<InvalidOperationException>(
                () => service.Evaluate(
                    "expression"));

        Assert.Same(
            expected,
            actual);
    }

    [Fact]
    public void ConstructorRejectsMissingExpressionEngine()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => new ExpressionEvaluationService(
                    null!));

        Assert.Equal(
            "expressionEngine",
            exception.ParamName);
    }

    [Fact]
    public void EvaluateRejectsMissingExpression()
    {
        ExpressionEvaluationService service =
            CreateService();

        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => service.Evaluate(
                    null!));

        Assert.Equal(
            "expression",
            exception.ParamName);
    }

    [Fact]
    public void SuccessRejectsNonFiniteValue()
    {
        ArgumentOutOfRangeException exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => ExpressionEvaluationResult.Success(
                    double.PositiveInfinity,
                    "Infinity"));

        Assert.Equal(
            "value",
            exception.ParamName);
    }

    [Fact]
    public void SuccessRejectsMissingDisplayValue()
    {
        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => ExpressionEvaluationResult.Success(
                    1.0,
                    " "));

        Assert.Equal(
            "displayValue",
            exception.ParamName);
    }

    [Fact]
    public void FailureRejectsMissingError()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => ExpressionEvaluationResult.Failure(
                    null!));

        Assert.Equal(
            "error",
            exception.ParamName);
    }

    [Fact]
    public void ErrorRejectsNegativePosition()
    {
        ArgumentOutOfRangeException exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new ExpressionEvaluationError(
                    ExpressionEvaluationErrorKind.Parsing,
                    "Parsing failed.",
                    -1));

        Assert.Equal(
            "position",
            exception.ParamName);
    }

    private static ExpressionEvaluationService CreateService()
    {
        IExpressionTokenizer tokenizer =
            new ExpressionTokenizer();

        IExpressionParser parser =
            new ExpressionParser(
                tokenizer);

        IExpressionEvaluator evaluator =
            new ExpressionEvaluator(
                new CalculationEngine());

        IExpressionEngine engine =
            new ExpressionEngine(
                parser,
                evaluator);

        return new ExpressionEvaluationService(
            engine);
    }

    private sealed class RecordingExpressionEngine
        : IExpressionEngine
    {
        private readonly double _result;

        public RecordingExpressionEngine(
            double result)
        {
            _result = result;
        }

        public string? Expression { get; private set; }

        public IReadOnlyDictionary<string, double>? Variables
        {
            get;
            private set;
        }

        public double Evaluate(
            string expression,
            IReadOnlyDictionary<string, double>? variables = null)
        {
            Expression = expression;
            Variables = variables;

            return _result;
        }
    }

    private sealed class ThrowingExpressionEngine
        : IExpressionEngine
    {
        private readonly Exception _exception;

        public ThrowingExpressionEngine(
            Exception exception)
        {
            _exception = exception;
        }

        public double Evaluate(
            string expression,
            IReadOnlyDictionary<string, double>? variables = null)
        {
            throw _exception;
        }
    }
}
