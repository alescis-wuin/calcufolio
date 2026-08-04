namespace Calcufolio.Domain.Expressions.Lexing;

public sealed record ExpressionToken
{
    public ExpressionToken(
        ExpressionTokenKind kind,
        string lexeme,
        int position)
    {
        ArgumentNullException.ThrowIfNull(lexeme);
        ArgumentOutOfRangeException.ThrowIfNegative(position);

        Kind = kind;
        Lexeme = lexeme;
        Position = position;
    }

    public ExpressionTokenKind Kind { get; }

    public string Lexeme { get; }

    public int Position { get; }

    public int Length =>
        Lexeme.Length;
}
