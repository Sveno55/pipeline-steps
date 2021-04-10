using PipelineSteps.UnitTests.TestAssets;
using System;
using System.Threading.Tasks;

namespace PipelineSteps.UnitTests.Steps
{
    public interface IStepBWithNoCallToNextStep : IStep
    {

    }

    public class StepBWithNoCallToNextStep : Step, IStepBWithNoCallToNextStep
    {
        private IStepTracker tracker;

        public StepBWithNoCallToNextStep(IStepTracker tracker) : base("StepBWithNoCallToNextStep")
        {
            this.tracker = tracker;
        }

        public override async Task ExecuteAsync(NextStepAction nextStep, params object[] arguments)
        {
            Console.WriteLine($"{DateTime.Now} - StepBWithNoCallToNextStep");

            if (tracker != null) tracker.Track("StepBWithNoCallToNextStep");

            await Task.Delay(2000);
        }
    }
}
