using Calcufolio.Domain.Expressions.Lexing;

namespace Calcufolio.Domain.Tests.Expressions.Lexing;

public sealed class ExpressionTokenizerTests
{
    private readonly ExpressionTokenizer _tokenizer = new();

    [Fact]
    public void TokenizeReturnsOnlyEndForEmptyExpression()
    {
        IReadOnlyList<ExpressionToken> tokens =
            _tokenizer.Tokenize(
                string.Empty);

        ExpressionToken token =
            Assert.Single(tokens);

        AssertToken(
            token,
            ExpressionTokenKind.End,
            string.Empty,
            0);
    }

    [Fact]
    public void TokenizeReturnsEndAtWhitespaceExpressionLength()
    {
        const string Expression = " \t\n";

        IReadOnlyList<ExpressionToken> tokens =
            _tokenizer.Tokenize(
                Expression);

        ExpressionToken token =
            Assert.Single(tokens);

        AssertToken(
            token,
            ExpressionTokenKind.End,
            string.Empty,
            Expression.Length);
    }

    [Fact]
    public void TokenizeRecognizesArithmeticGrammar()
    {
        const string Expression =
            "12.5 + value_2 × (3 − .25) ÷ 2^4";

        IReadOnlyList<ExpressionToken> tokens =
            _tokenizer.Tokenize(
                Expression);

        Assert.Collection(
            tokens,
            token => AssertToken(
                token,
                ExpressionTokenKind.Number,
                "12.5",
                0),
            token => AssertToken(
                token,
                ExpressionTokenKind.Plus,
                "+",
                5),
            token => AssertToken(
                token,
                ExpressionTokenKind.Identifier,
                "value_2",
                7),
            token => AssertToken(
                token,
                ExpressionTokenKind.Multiply,
                "×",
                15),
            token => AssertToken(
                token,
                ExpressionTokenKind.LeftParenthesis,
                "(",
                17),
            token => AssertToken(
                token,
                ExpressionTokenKind.Number,
                "3",
                18),
            token => AssertToken(
                token,
                ExpressionTokenKind.Minus,
                "−",
                20),
            token => AssertToken(
                token,
                ExpressionTokenKind.Number,
                ".25",
                22),
            token => AssertToken(
                token,
                ExpressionTokenKind.RightParenthesis,
                ")",
                25),
            token => AssertToken(
                token,
                ExpressionTokenKind.Divide,
                "÷",
                27),
            token => AssertToken(
                token,
                ExpressionTokenKind.Number,
                "2",
                29),
            token => AssertToken(
                token,
                ExpressionTokenKind.Power,
                "^",
                30),
            token => AssertToken(
                token,
                ExpressionTokenKind.Number,
                "4",
                31),
            token => AssertToken(
                token,
                ExpressionTokenKind.End,
                string.Empty,
                Expression.Length));
    }

    [Theory]
    [InlineData("+", ExpressionTokenKind.Plus)]
    [InlineData("-", ExpressionTokenKind.Minus)]
    [InlineData("−", ExpressionTokenKind.Minus)]
    [InlineData("*", ExpressionTokenKind.Multiply)]
    [InlineData("×", ExpressionTokenKind.Multiply)]
    [InlineData("/", ExpressionTokenKind.Divide)]
    [InlineData("÷", ExpressionTokenKind.Divide)]
    [InlineData("^", ExpressionTokenKind.Power)]
    [InlineData("(", ExpressionTokenKind.LeftParenthesis)]
    [InlineData(")", ExpressionTokenKind.RightParenthesis)]
    public void TokenizeRecognizesOperatorAliases(
        string expression,
        ExpressionTokenKind expectedKind)
    {
        IReadOnlyList<ExpressionToken> tokens =
            _tokenizer.Tokenize(
                expression);

        Assert.Collection(
            tokens,
            token => AssertToken(
                token,
                expectedKind,
                expression,
                0),
            token => AssertToken(
                token,
                ExpressionTokenKind.End,
                string.Empty,
                expression.Length));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("12")]
    [InlineData("12.5")]
    [InlineData(".75")]
    [InlineData("5.")]
    [InlineData("1.25e-3")]
    [InlineData("2E+4")]
    [InlineData("3e−2")]
    public void TokenizeRecognizesInvariantNumericLiteral(
        string expression)
    {
        IReadOnlyList<ExpressionToken> tokens =
            _tokenizer.Tokenize(
                expression);

        Assert.Collection(
            tokens,
            token => AssertToken(
                token,
                ExpressionTokenKind.Number,
                expression,
                0),
            token => AssertToken(
                token,
                ExpressionTokenKind.End,
                string.Empty,
                expression.Length));
    }

    [Theory]
    [InlineData("sin")]
    [InlineData("sqrt")]
    [InlineData("value_2")]
    [InlineData("π")]
    [InlineData("résultat")]
    public void TokenizeRecognizesIdentifier(
        string expression)
    {
        IReadOnlyList<ExpressionToken> tokens =
            _tokenizer.Tokenize(
                expression);

        Assert.Collection(
            tokens,
            token => AssertToken(
                token,
                ExpressionTokenKind.Identifier,
                expression,
                0),
            token => AssertToken(
                token,
                ExpressionTokenKind.End,
                string.Empty,
                expression.Length));
    }

    [Theory]
    [InlineData("1e", 1)]
    [InlineData("1e+", 1)]
    [InlineData("1E−", 1)]
    public void TokenizeRejectsExponentWithoutDigits(
        string expression,
        int expectedPosition)
    {
        ExpressionTokenizationException exception =
            Assert.Throws<ExpressionTokenizationException>(
                () => _tokenizer.Tokenize(
                    expression));

        Assert.Equal(
            expectedPosition,
            exception.Position);

        Assert.Contains(
            "A numeric exponent must contain at least one digit",
            exception.Message,
            StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(".", 0)]
    [InlineData("  .  ", 2)]
    public void TokenizeRejectsDecimalWithoutDigits(
        string expression,
        int expectedPosition)
    {
        ExpressionTokenizationException exception =
            Assert.Throws<ExpressionTokenizationException>(
                () => _tokenizer.Tokenize(
                    expression));

        Assert.Equal(
            expectedPosition,
            exception.Position);

        Assert.Contains(
            "A numeric literal must contain at least one digit",
            exception.Message,
            StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("1..2", 2)]
    [InlineData("1.2.3", 3)]
    [InlineData("1e2.3", 3)]
    public void TokenizeRejectsMultipleDecimalSeparators(
        string expression,
        int expectedPosition)
    {
        ExpressionTokenizationException exception =
            Assert.Throws<ExpressionTokenizationException>(
                () => _tokenizer.Tokenize(
                    expression));

        Assert.Equal(
            expectedPosition,
            exception.Position);

        Assert.Contains(
            "multiple decimal separators",
            exception.Message,
            StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("%", 0)]
    [InlineData("2 @ 3", 2)]
    [InlineData("_value", 0)]
    [InlineData("١", 0)]
    public void TokenizeRejectsUnsupportedCharacter(
        string expression,
        int expectedPosition)
    {
        ExpressionTokenizationException exception =
            Assert.Throws<ExpressionTokenizationException>(
                () => _tokenizer.Tokenize(
                    expression));

        Assert.Equal(
            expectedPosition,
            exception.Position);

        Assert.Contains(
            "Unsupported expression character",
            exception.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void TokenizeRejectsMissingExpression()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => _tokenizer.Tokenize(
                    null!));

        Assert.Equal(
            "expression",
            exception.ParamName);
    }

    [Fact]
    public void TokenRejectsMissingLexeme()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => new ExpressionToken(
                    ExpressionTokenKind.Number,
                    null!,
                    0));

        Assert.Equal(
            "lexeme",
            exception.ParamName);
    }

    [Fact]
    public void TokenRejectsNegativePosition()
    {
        ArgumentOutOfRangeException exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new ExpressionToken(
                    ExpressionTokenKind.Number,
                    "1",
                    -1));

        Assert.Equal(
            "position",
            exception.ParamName);
    }

    private static void AssertToken(
        ExpressionToken token,
        ExpressionTokenKind expectedKind,
        string expectedLexeme,
        int expectedPosition)
    {
        Assert.Equal(
            expectedKind,
            token.Kind);

        Assert.Equal(
            expectedLexeme,
            token.Lexeme);

        Assert.Equal(
            expectedPosition,
            token.Position);

        Assert.Equal(
            expectedLexeme.Length,
            token.Length);
    }
}
