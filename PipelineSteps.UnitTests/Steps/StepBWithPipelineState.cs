using PipelineSteps.UnitTests.TestAssets;
using System;
using System.Threading.Tasks;

namespace PipelineSteps.UnitTests.Steps
{
    public interface IStepBWithPipelineState : IStep
    {

    }

    public class StepBWithPipelineState : Step, IStepBWithPipelineState
    {
        private IStepTracker tracker;

        public StepBWithPipelineState(IStepTracker tracker) : base("StepBWithPipelineState")
        {
            this.tracker = tracker;
        }

        public override async Task ExecuteAsync(NextStepAction nextStep, params object[] arguments)
        {
            Console.WriteLine($"{DateTime.Now} - StepBWithPipelineState");

            MyCustomPipelineState state = GetPipelineState<MyCustomPipelineState>();

            tracker.Track(state.FirstName);
            tracker.Track(state.LastName);

            state.FirstName = "Hello World!";
            state.LastName = "";

            await Task.Delay(2000);
            
            await nextStep();
        }
    }
}
