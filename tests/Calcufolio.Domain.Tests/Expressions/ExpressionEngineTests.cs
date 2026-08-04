using Calcufolio.Domain.Calculations;
using Calcufolio.Domain.Expressions;
using Calcufolio.Domain.Expressions.Evaluation;
using Calcufolio.Domain.Expressions.Lexing;
using Calcufolio.Domain.Expressions.Parsing;
using Calcufolio.Domain.Expressions.Syntax;

namespace Calcufolio.Domain.Tests.Expressions;

public sealed class ExpressionEngineTests
{
    private readonly ExpressionEngine _engine =
        new(
            new ExpressionParser(
                new ExpressionTokenizer()),
            new ExpressionEvaluator(
                new CalculationEngine()));

    [Theory]
    [InlineData("1 + 2 * 3", 7.0)]
    [InlineData("(1 + 2) * 3", 9.0)]
    [InlineData("2^3^2", 512.0)]
    [InlineData("-2^2", -4.0)]
    [InlineData("2^-3", 0.125)]
    [InlineData("8 ÷ 4 × 3", 6.0)]
    [InlineData("1.25e2 + .5", 125.5)]
    public void EvaluateReturnsExpectedResult(
        string expression,
        double expected)
    {
        double result =
            _engine.Evaluate(
                expression);

        Assert.Equal(
            expected,
            result,
            12);
    }

    [Fact]
    public void EvaluateResolvesVariables()
    {
        Dictionary<string, double> variables =
            new(
                StringComparer.Ordinal)
            {
                ["principal"] = 1250.0,
                ["rate"] = 0.04,
            };

        double result =
            _engine.Evaluate(
                "principal * (1 + rate)",
                variables);

        Assert.Equal(
            1300.0,
            result,
            12);
    }

    [Fact]
    public void EvaluateForwardsParsedSyntaxAndVariables()
    {
        StubExpressionSyntax syntax =
            new();

        RecordingParser parser =
            new(
                syntax);

        RecordingEvaluator evaluator =
            new(
                42.5);

        ExpressionEngine engine =
            new(
                parser,
                evaluator);

        Dictionary<string, double> variables =
            new(
                StringComparer.Ordinal)
            {
                ["value"] = 42.5,
            };

        double result =
            engine.Evaluate(
                "value",
                variables);

        Assert.Equal(
            "value",
            parser.Expression);

        Assert.Same(
            syntax,
            evaluator.Expression);

        Assert.Same(
            variables,
            evaluator.Variables);

        Assert.Equal(
            42.5,
            result,
            12);
    }

    [Fact]
    public void EvaluatePreservesTokenizationFailure()
    {
        ExpressionTokenizationException exception =
            Assert.Throws<ExpressionTokenizationException>(
                () => _engine.Evaluate(
                    "1 @ 2"));

        Assert.Equal(
            2,
            exception.Position);
    }

    [Fact]
    public void EvaluatePreservesParsingFailure()
    {
        ExpressionParsingException exception =
            Assert.Throws<ExpressionParsingException>(
                () => _engine.Evaluate(
                    "(1 + 2"));

        Assert.Equal(
            6,
            exception.Position);
    }

    [Fact]
    public void EvaluatePreservesEvaluationFailure()
    {
        ExpressionEvaluationException exception =
            Assert.Throws<ExpressionEvaluationException>(
                () => _engine.Evaluate(
                    "1 / 0"));

        Assert.Equal(
            2,
            exception.Position);
    }

    [Fact]
    public void EvaluateRejectsMissingExpression()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => _engine.Evaluate(
                    null!));

        Assert.Equal(
            "expression",
            exception.ParamName);
    }

    [Fact]
    public void ConstructorRejectsMissingParser()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => new ExpressionEngine(
                    null!,
                    new ExpressionEvaluator(
                        new CalculationEngine())));

        Assert.Equal(
            "parser",
            exception.ParamName);
    }

    [Fact]
    public void ConstructorRejectsMissingEvaluator()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => new ExpressionEngine(
                    new ExpressionParser(
                        new ExpressionTokenizer()),
                    null!));

        Assert.Equal(
            "evaluator",
            exception.ParamName);
    }

    private sealed class RecordingParser
        : IExpressionParser
    {
        private readonly ExpressionSyntax _syntax;

        public RecordingParser(
            ExpressionSyntax syntax)
        {
            _syntax = syntax;
        }

        public string? Expression { get; private set; }

        public ExpressionSyntax Parse(
            string expression)
        {
            Expression = expression;

            return _syntax;
        }
    }

    private sealed class RecordingEvaluator
        : IExpressionEvaluator
    {
        private readonly double _result;

        public RecordingEvaluator(
            double result)
        {
            _result = result;
        }

        public ExpressionSyntax? Expression { get; private set; }

        public IReadOnlyDictionary<string, double>? Variables
        {
            get;
            private set;
        }

        public double Evaluate(
            ExpressionSyntax expression,
            IReadOnlyDictionary<string, double>? variables = null)
        {
            Expression = expression;
            Variables = variables;

            return _result;
        }
    }

    private sealed class StubExpressionSyntax
        : ExpressionSyntax
    {
        public StubExpressionSyntax()
            : base(
                new ExpressionSourceSpan(
                    0,
                    0))
        {
        }
    }
}
