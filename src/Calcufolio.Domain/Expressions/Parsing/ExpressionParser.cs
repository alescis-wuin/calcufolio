using System.Globalization;
using Calcufolio.Domain.Expressions.Lexing;
using Calcufolio.Domain.Expressions.Syntax;

namespace Calcufolio.Domain.Expressions.Parsing;

public sealed class ExpressionParser : IExpressionParser
{
    private readonly IExpressionTokenizer _tokenizer;

    public ExpressionParser(
        IExpressionTokenizer tokenizer)
    {
        ArgumentNullException.ThrowIfNull(tokenizer);

        _tokenizer = tokenizer;
    }

    public ExpressionSyntax Parse(
        string expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        return new ParserState(
            _tokenizer.Tokenize(
                expression)).Parse();
    }

    private sealed class ParserState
    {
        private readonly IReadOnlyList<ExpressionToken> _tokens;
        private int _index;

        public ParserState(
            IReadOnlyList<ExpressionToken> tokens)
        {
            ArgumentNullException.ThrowIfNull(tokens);

            if (tokens.Count == 0 ||
                tokens[^1].Kind != ExpressionTokenKind.End)
            {
                throw new ArgumentException(
                    "The token sequence must end with an explicit end token.",
                    nameof(tokens));
            }

            _tokens = tokens;
        }

        private ExpressionToken Current =>
            _tokens[_index];

        public ExpressionSyntax Parse()
        {
            if (Current.Kind == ExpressionTokenKind.End)
            {
                throw Error(
                    "An expression is required",
                    Current);
            }

            ExpressionSyntax expression =
                ParseAdditive();

            if (Current.Kind != ExpressionTokenKind.End)
            {
                throw Error(
                    $"Unexpected token '{Current.Lexeme}'",
                    Current);
            }

            return expression;
        }

        private ExpressionSyntax ParseAdditive()
        {
            ExpressionSyntax left =
                ParseMultiplicative();

            while (Current.Kind is
                   ExpressionTokenKind.Plus or
                   ExpressionTokenKind.Minus)
            {
                ExpressionToken operatorToken =
                    Advance();

                ExpressionSyntax right =
                    ParseMultiplicative();

                left = CreateBinary(
                    left,
                    operatorToken,
                    right);
            }

            return left;
        }

        private ExpressionSyntax ParseMultiplicative()
        {
            ExpressionSyntax left =
                ParseUnary();

            while (Current.Kind is
                   ExpressionTokenKind.Multiply or
                   ExpressionTokenKind.Divide)
            {
                ExpressionToken operatorToken =
                    Advance();

                ExpressionSyntax right =
                    ParseUnary();

                left = CreateBinary(
                    left,
                    operatorToken,
                    right);
            }

            return left;
        }

        private ExpressionSyntax ParseUnary()
        {
            if (Current.Kind is
                ExpressionTokenKind.Plus or
                ExpressionTokenKind.Minus)
            {
                ExpressionToken operatorToken =
                    Advance();

                ExpressionSyntax operand =
                    ParseUnary();

                return new UnaryExpressionSyntax(
                    operatorToken.Kind ==
                        ExpressionTokenKind.Plus
                            ? ExpressionUnaryOperator.Positive
                            : ExpressionUnaryOperator.Negate,
                    operatorToken.Position,
                    operand,
                    ExpressionSourceSpan.FromBounds(
                        operatorToken.Position,
                        operand.End));
            }

            return ParsePower();
        }

        private ExpressionSyntax ParsePower()
        {
            ExpressionSyntax left =
                ParsePrimary();

            if (Current.Kind != ExpressionTokenKind.Power)
            {
                return left;
            }

            ExpressionToken operatorToken =
                Advance();

            ExpressionSyntax right =
                ParseUnary();

            return CreateBinary(
                left,
                operatorToken,
                right);
        }

        private ExpressionSyntax ParsePrimary()
        {
            return Current.Kind switch
            {
                ExpressionTokenKind.Number =>
                    ParseNumber(),

                ExpressionTokenKind.Identifier =>
                    ParseIdentifier(),

                ExpressionTokenKind.LeftParenthesis =>
                    ParseGroup(),

                _ => throw Error(
                    "Expected a number, identifier, unary operator, or parenthesized expression",
                    Current),
            };
        }

        private NumberExpressionSyntax ParseNumber()
        {
            ExpressionToken token =
                Advance();

            if (!double.TryParse(
                    token.Lexeme,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double value) ||
                !double.IsFinite(value))
            {
                throw Error(
                    $"Numeric literal '{token.Lexeme}' is outside the supported range",
                    token);
            }

            return new NumberExpressionSyntax(
                value,
                token.Lexeme,
                GetSpan(token));
        }

        private IdentifierExpressionSyntax ParseIdentifier()
        {
            ExpressionToken token =
                Advance();

            return new IdentifierExpressionSyntax(
                token.Lexeme,
                GetSpan(token));
        }

        private GroupExpressionSyntax ParseGroup()
        {
            ExpressionToken openingToken =
                Advance();

            if (Current.Kind == ExpressionTokenKind.RightParenthesis)
            {
                throw Error(
                    "A parenthesized expression cannot be empty",
                    Current);
            }

            ExpressionSyntax expression =
                ParseAdditive();

            if (Current.Kind != ExpressionTokenKind.RightParenthesis)
            {
                throw Error(
                    "Expected a closing parenthesis",
                    Current);
            }

            ExpressionToken closingToken =
                Advance();

            return new GroupExpressionSyntax(
                expression,
                ExpressionSourceSpan.FromBounds(
                    openingToken.Position,
                    closingToken.Position +
                        closingToken.Length));
        }

        private static BinaryExpressionSyntax CreateBinary(
            ExpressionSyntax left,
            ExpressionToken operatorToken,
            ExpressionSyntax right)
        {
            return new BinaryExpressionSyntax(
                left,
                GetBinaryOperator(
                    operatorToken),
                operatorToken.Position,
                right,
                ExpressionSourceSpan.FromBounds(
                    left.Position,
                    right.End));
        }

        private ExpressionToken Advance()
        {
            ExpressionToken token =
                Current;

            if (token.Kind != ExpressionTokenKind.End)
            {
                _index++;
            }

            return token;
        }

        private static ExpressionBinaryOperator GetBinaryOperator(
            ExpressionToken token)
        {
            return token.Kind switch
            {
                ExpressionTokenKind.Plus =>
                    ExpressionBinaryOperator.Add,

                ExpressionTokenKind.Minus =>
                    ExpressionBinaryOperator.Subtract,

                ExpressionTokenKind.Multiply =>
                    ExpressionBinaryOperator.Multiply,

                ExpressionTokenKind.Divide =>
                    ExpressionBinaryOperator.Divide,

                ExpressionTokenKind.Power =>
                    ExpressionBinaryOperator.Power,

                _ => throw new ArgumentOutOfRangeException(
                    nameof(token),
                    token.Kind,
                    "The token is not a binary operator."),
            };
        }

        private static ExpressionSourceSpan GetSpan(
            ExpressionToken token)
        {
            return new ExpressionSourceSpan(
                token.Position,
                token.Length);
        }

        private static ExpressionParsingException Error(
            string message,
            ExpressionToken token)
        {
            return new ExpressionParsingException(
                message,
                token.Position);
        }
    }
}
