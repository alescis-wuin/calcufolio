namespace Calcufolio.Domain.Expressions.Lexing;

public enum ExpressionTokenKind
{
    Number,
    Identifier,
    Plus,
    Minus,
    Multiply,
    Divide,
    Power,
    LeftParenthesis,
    RightParenthesis,
    End,
}
