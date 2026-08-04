using Calcufolio.Domain.Expressions.Lexing;
using Calcufolio.Domain.Expressions.Parsing;
using Calcufolio.Domain.Expressions.Syntax;

namespace Calcufolio.Domain.Tests.Expressions.Parsing;

public sealed class ExpressionParserTests
{
    private readonly ExpressionParser _parser =
        new(
            new ExpressionTokenizer());

    [Fact]
    public void ParseHonorsMultiplicationPrecedence()
    {
        BinaryExpressionSyntax root =
            AssertBinary(
                _parser.Parse(
                    "1 + 2 * 3"),
                ExpressionBinaryOperator.Add,
                2);

        AssertNumber(
            root.Left,
            1.0,
            "1",
            0);

        BinaryExpressionSyntax multiply =
            AssertBinary(
                root.Right,
                ExpressionBinaryOperator.Multiply,
                6);

        AssertNumber(
            multiply.Left,
            2.0,
            "2",
            4);

        AssertNumber(
            multiply.Right,
            3.0,
            "3",
            8);
    }

    [Fact]
    public void ParseParenthesesOverridePrecedence()
    {
        BinaryExpressionSyntax root =
            AssertBinary(
                _parser.Parse(
                    "(1 + 2) * 3"),
                ExpressionBinaryOperator.Multiply,
                8);

        GroupExpressionSyntax group =
            Assert.IsType<GroupExpressionSyntax>(
                root.Left);

        BinaryExpressionSyntax add =
            AssertBinary(
                group.Expression,
                ExpressionBinaryOperator.Add,
                3);

        AssertNumber(
            add.Left,
            1.0,
            "1",
            1);

        AssertNumber(
            add.Right,
            2.0,
            "2",
            5);

        Assert.Equal(
            0,
            group.Position);

        Assert.Equal(
            7,
            group.Length);

        AssertNumber(
            root.Right,
            3.0,
            "3",
            10);
    }

    [Fact]
    public void ParseMakesPowerRightAssociative()
    {
        BinaryExpressionSyntax root =
            AssertBinary(
                _parser.Parse(
                    "2^3^2"),
                ExpressionBinaryOperator.Power,
                1);

        AssertNumber(
            root.Left,
            2.0,
            "2",
            0);

        BinaryExpressionSyntax nestedPower =
            AssertBinary(
                root.Right,
                ExpressionBinaryOperator.Power,
                3);

        AssertNumber(
            nestedPower.Left,
            3.0,
            "3",
            2);

        AssertNumber(
            nestedPower.Right,
            2.0,
            "2",
            4);
    }

    [Fact]
    public void ParseAppliesPowerBeforeLeadingUnaryOperator()
    {
        UnaryExpressionSyntax root =
            AssertUnary(
                _parser.Parse(
                    "-2^2"),
                ExpressionUnaryOperator.Negate,
                0);

        AssertBinary(
            root.Operand,
            ExpressionBinaryOperator.Power,
            2);
    }

    [Fact]
    public void ParseAllowsUnaryExponent()
    {
        BinaryExpressionSyntax root =
            AssertBinary(
                _parser.Parse(
                    "2^-3"),
                ExpressionBinaryOperator.Power,
                1);

        AssertUnary(
            root.Right,
            ExpressionUnaryOperator.Negate,
            2);
    }

    [Theory]
    [InlineData("1+2", ExpressionBinaryOperator.Add)]
    [InlineData("1-2", ExpressionBinaryOperator.Subtract)]
    [InlineData("1−2", ExpressionBinaryOperator.Subtract)]
    [InlineData("1*2", ExpressionBinaryOperator.Multiply)]
    [InlineData("1×2", ExpressionBinaryOperator.Multiply)]
    [InlineData("1/2", ExpressionBinaryOperator.Divide)]
    [InlineData("1÷2", ExpressionBinaryOperator.Divide)]
    [InlineData("1^2", ExpressionBinaryOperator.Power)]
    public void ParseMapsBinaryOperatorAliases(
        string expression,
        ExpressionBinaryOperator expectedOperator)
    {
        AssertBinary(
            _parser.Parse(
                expression),
            expectedOperator,
            1);
    }

    [Theory]
    [InlineData("+1", ExpressionUnaryOperator.Positive)]
    [InlineData("-1", ExpressionUnaryOperator.Negate)]
    [InlineData("−1", ExpressionUnaryOperator.Negate)]
    public void ParseMapsUnaryOperatorAliases(
        string expression,
        ExpressionUnaryOperator expectedOperator)
    {
        AssertUnary(
            _parser.Parse(
                expression),
            expectedOperator,
            0);
    }

    [Fact]
    public void ParseRecognizesIdentifierAndScientificNumber()
    {
        BinaryExpressionSyntax root =
            AssertBinary(
                _parser.Parse(
                    "value_2 + 1.25e-3"),
                ExpressionBinaryOperator.Add,
                8);

        IdentifierExpressionSyntax identifier =
            Assert.IsType<IdentifierExpressionSyntax>(
                root.Left);

        Assert.Equal(
            "value_2",
            identifier.Name);

        AssertNumber(
            root.Right,
            0.00125,
            "1.25e-3",
            10);
    }

    [Fact]
    public void ParsePreservesCompoundSourceSpans()
    {
        UnaryExpressionSyntax root =
            AssertUnary(
                _parser.Parse(
                    "  -(2 + x)  "),
                ExpressionUnaryOperator.Negate,
                2);

        Assert.Equal(
            8,
            root.Length);

        GroupExpressionSyntax group =
            Assert.IsType<GroupExpressionSyntax>(
                root.Operand);

        Assert.Equal(
            3,
            group.Position);

        Assert.Equal(
            7,
            group.Length);
    }

    [Theory]
    [InlineData("", 0, "An expression is required")]
    [InlineData("   ", 3, "An expression is required")]
    [InlineData("()", 1, "cannot be empty")]
    [InlineData("(1 + 2", 6, "Expected a closing parenthesis")]
    [InlineData("1 +", 3, "Expected a number")]
    [InlineData("1 2", 2, "Unexpected token")]
    [InlineData("1 + 2)", 5, "Unexpected token")]
    public void ParseReportsSyntaxErrorAtExactPosition(
        string expression,
        int expectedPosition,
        string expectedMessage)
    {
        ExpressionParsingException exception =
            Assert.Throws<ExpressionParsingException>(
                () => _parser.Parse(
                    expression));

        Assert.Equal(
            expectedPosition,
            exception.Position);

        Assert.Contains(
            expectedMessage,
            exception.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void ParseRejectsNonFiniteNumericLiteral()
    {
        ExpressionParsingException exception =
            Assert.Throws<ExpressionParsingException>(
                () => _parser.Parse(
                    "1e999"));

        Assert.Equal(
            0,
            exception.Position);

        Assert.Contains(
            "outside the supported range",
            exception.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void ParseRejectsMissingExpression()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => _parser.Parse(
                    null!));

        Assert.Equal(
            "expression",
            exception.ParamName);
    }

    [Fact]
    public void ConstructorRejectsMissingTokenizer()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => new ExpressionParser(
                    null!));

        Assert.Equal(
            "tokenizer",
            exception.ParamName);
    }

    [Fact]
    public void SourceSpanRejectsReversedBounds()
    {
        ArgumentOutOfRangeException exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => ExpressionSourceSpan.FromBounds(
                    4,
                    3));

        Assert.Equal(
            "end",
            exception.ParamName);
    }

    private static BinaryExpressionSyntax AssertBinary(
        ExpressionSyntax syntax,
        ExpressionBinaryOperator expectedOperator,
        int expectedOperatorPosition)
    {
        BinaryExpressionSyntax binary =
            Assert.IsType<BinaryExpressionSyntax>(
                syntax);

        Assert.Equal(
            expectedOperator,
            binary.Operation);

        Assert.Equal(
            expectedOperatorPosition,
            binary.OperatorPosition);

        return binary;
    }

    private static UnaryExpressionSyntax AssertUnary(
        ExpressionSyntax syntax,
        ExpressionUnaryOperator expectedOperator,
        int expectedOperatorPosition)
    {
        UnaryExpressionSyntax unary =
            Assert.IsType<UnaryExpressionSyntax>(
                syntax);

        Assert.Equal(
            expectedOperator,
            unary.Operation);

        Assert.Equal(
            expectedOperatorPosition,
            unary.OperatorPosition);

        return unary;
    }

    private static void AssertNumber(
        ExpressionSyntax syntax,
        double expectedValue,
        string expectedLexeme,
        int expectedPosition)
    {
        NumberExpressionSyntax number =
            Assert.IsType<NumberExpressionSyntax>(
                syntax);

        Assert.Equal(
            expectedValue,
            number.Value,
            12);

        Assert.Equal(
            expectedLexeme,
            number.Lexeme);

        Assert.Equal(
            expectedPosition,
            number.Position);

        Assert.Equal(
            expectedLexeme.Length,
            number.Length);
    }
}
