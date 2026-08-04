using Calcufolio.Domain.Expressions.Syntax;

namespace Calcufolio.Domain.Expressions.Parsing;

public interface IExpressionParser
{
    ExpressionSyntax Parse(
        string expression);
}
