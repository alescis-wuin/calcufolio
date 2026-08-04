using Calcufolio.Application.Expressions;
using Calcufolio.Application.Interaction.Controller;
using Calcufolio.Application.Interaction.Editor.Reducer;
using Calcufolio.Application.Interaction.Preview;
using Calcufolio.Application.Interaction.State;
using Calcufolio.Domain.Calculations;
using Calcufolio.Domain.Expressions;
using Calcufolio.Domain.Expressions.Evaluation;
using Calcufolio.Domain.Expressions.Lexing;
using Calcufolio.Domain.Expressions.Parsing;
using Calcufolio.Presentation.ViewModels;

namespace Calcufolio.Presentation.Tests.ViewModels;

internal static class MainViewModelTestFactory
{
    public static MainViewModel Create(
        CalculatorState? initialState = null)
    {
        ICalculatorStateStore stateStore = initialState is null
            ? new CalculatorStateStore()
            : new CalculatorStateStore(initialState);

        IExpressionEvaluationService expressionEvaluationService =
            CreateExpressionEvaluationService();

        ICalculatorController controller =
            new CalculatorController(
                expressionEvaluationService,
                new EditorStateReducer(),
                stateStore);

        return new MainViewModel(
            controller,
            stateStore,
            new CalculationPreviewService(
                expressionEvaluationService));
    }

    private static ExpressionEvaluationService CreateExpressionEvaluationService()
    {
        ICalculationEngine calculationEngine =
            new CalculationEngine();

        IExpressionTokenizer tokenizer =
            new ExpressionTokenizer();

        IExpressionParser parser =
            new ExpressionParser(
                tokenizer);

        IExpressionEvaluator evaluator =
            new ExpressionEvaluator(
                calculationEngine);

        IExpressionEngine engine =
            new ExpressionEngine(
                parser,
                evaluator);

        return new ExpressionEvaluationService(
            engine);
    }
}
