using Calcufolio.Domain.Calculations;

namespace Calcufolio.Application.Calculations;

public sealed record PendingBinaryOperation(
    double LeftOperand,
    BinaryOperator Operation);
