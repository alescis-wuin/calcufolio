namespace Calcufolio.Domain.Expressions.Lexing;

public interface IExpressionTokenizer
{
    IReadOnlyList<ExpressionToken> Tokenize(
        string expression);
}
