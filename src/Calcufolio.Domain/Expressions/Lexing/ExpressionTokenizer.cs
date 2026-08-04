using System.Collections.ObjectModel;

namespace Calcufolio.Domain.Expressions.Lexing;

public sealed class ExpressionTokenizer : IExpressionTokenizer
{
    public IReadOnlyList<ExpressionToken> Tokenize(
        string expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        List<ExpressionToken> tokens = [];
        int position = 0;

        while (position < expression.Length)
        {
            char current = expression[position];

            if (char.IsWhiteSpace(current))
            {
                position++;
                continue;
            }

            if (IsDecimalDigit(current) ||
                current == '.')
            {
                tokens.Add(
                    ReadNumber(
                        expression,
                        ref position));

                continue;
            }

            if (char.IsLetter(current))
            {
                tokens.Add(
                    ReadIdentifier(
                        expression,
                        ref position));

                continue;
            }

            tokens.Add(
                ReadSymbol(
                    current,
                    position));

            position++;
        }

        tokens.Add(
            new ExpressionToken(
                ExpressionTokenKind.End,
                string.Empty,
                expression.Length));

        return new ReadOnlyCollection<ExpressionToken>(
            tokens);
    }

    private static ExpressionToken ReadNumber(
        string expression,
        ref int position)
    {
        int start = position;
        bool hasIntegerDigits =
            ReadDigits(
                expression,
                ref position);

        bool hasFractionalDigits = false;

        if (position < expression.Length &&
            expression[position] == '.')
        {
            position++;

            hasFractionalDigits =
                ReadDigits(
                    expression,
                    ref position);
        }

        if (!hasIntegerDigits &&
            !hasFractionalDigits)
        {
            throw new ExpressionTokenizationException(
                "A numeric literal must contain at least one digit",
                start);
        }

        if (position < expression.Length &&
            IsExponentMarker(
                expression[position]))
        {
            int exponentPosition = position;

            position++;

            if (position < expression.Length &&
                IsExponentSign(
                    expression[position]))
            {
                position++;
            }

            if (!ReadDigits(
                    expression,
                    ref position))
            {
                throw new ExpressionTokenizationException(
                    "A numeric exponent must contain at least one digit",
                    exponentPosition);
            }
        }

        if (position < expression.Length &&
            expression[position] == '.')
        {
            throw new ExpressionTokenizationException(
                "A numeric literal cannot contain multiple decimal separators",
                position);
        }

        return new ExpressionToken(
            ExpressionTokenKind.Number,
            expression[start..position],
            start);
    }

    private static ExpressionToken ReadIdentifier(
        string expression,
        ref int position)
    {
        int start = position;

        position++;

        while (position < expression.Length &&
               IsIdentifierContinuation(
                   expression[position]))
        {
            position++;
        }

        return new ExpressionToken(
            ExpressionTokenKind.Identifier,
            expression[start..position],
            start);
    }

    private static ExpressionToken ReadSymbol(
        char symbol,
        int position)
    {
        ExpressionTokenKind kind = symbol switch
        {
            '+' => ExpressionTokenKind.Plus,
            '-' or '−' => ExpressionTokenKind.Minus,
            '*' or '×' => ExpressionTokenKind.Multiply,
            '/' or '÷' => ExpressionTokenKind.Divide,
            '^' => ExpressionTokenKind.Power,
            '(' => ExpressionTokenKind.LeftParenthesis,
            ')' => ExpressionTokenKind.RightParenthesis,
            _ => throw new ExpressionTokenizationException(
                $"Unsupported expression character '{symbol}'",
                position),
        };

        return new ExpressionToken(
            kind,
            symbol.ToString(),
            position);
    }

    private static bool ReadDigits(
        string expression,
        ref int position)
    {
        int start = position;

        while (position < expression.Length &&
               IsDecimalDigit(
                   expression[position]))
        {
            position++;
        }

        return position > start;
    }

    private static bool IsDecimalDigit(
        char character)
    {
        return character is >= '0' and <= '9';
    }

    private static bool IsExponentMarker(
        char character)
    {
        return character is 'e' or 'E';
    }

    private static bool IsExponentSign(
        char character)
    {
        return character is '+' or '-' or '−';
    }

    private static bool IsIdentifierContinuation(
        char character)
    {
        return char.IsLetterOrDigit(character) ||
            character == '_';
    }
}
