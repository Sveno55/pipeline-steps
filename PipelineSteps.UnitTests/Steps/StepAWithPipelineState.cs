using PipelineSteps.UnitTests.TestAssets;
using System;
using System.Threading.Tasks;

namespace PipelineSteps.UnitTests.Steps
{
    public interface IStepAWithPipelineState : IStep
    {

    }

    public class StepAWithPipelineState : Step, IStepAWithPipelineState
    {
        private IStepTracker tracker;

        public StepAWithPipelineState(IStepTracker tracker) : base("StepAWithPipelineState")
        {
            this.tracker = tracker;
        }

        public override async Task ExecuteAsync(NextStepAction nextStep, params object[] arguments)
        {
            Console.WriteLine($"{DateTime.Now} - StepAWithPipelineState");

            MyCustomPipelineState state = GetPipelineState<MyCustomPipelineState>();

            state.FirstName = "Bill";
            state.LastName = "Gates";

            await Task.Delay(2000);

            await nextStep();
        }
    }
}
