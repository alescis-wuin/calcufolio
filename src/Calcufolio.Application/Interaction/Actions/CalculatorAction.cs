namespace Calcufolio.Application.Interaction.Actions;

public abstract record CalculatorAction;

public sealed record AppendDigitAction : CalculatorAction
{
    public AppendDigitAction(string digit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(digit);

        if (digit.Length != 1 ||
            !char.IsAsciiDigit(digit[0]))
        {
            throw new ArgumentException(
                "The digit must contain exactly one ASCII numeric character.",
                nameof(digit));
        }

        Digit = digit;
    }

    public string Digit { get; }
}

public sealed record AppendDecimalSeparatorAction : CalculatorAction;

public sealed record SelectOperatorAction : CalculatorAction
{
    public SelectOperatorAction(string operatorSymbol)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operatorSymbol);

        OperatorSymbol = operatorSymbol;
    }

    public string OperatorSymbol { get; }
}

public sealed record EvaluateAction : CalculatorAction;

public sealed record ClearAction : CalculatorAction;
