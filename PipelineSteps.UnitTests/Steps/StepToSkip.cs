using PipelineSteps.UnitTests.TestAssets;
using System;
using System.Threading.Tasks;

namespace PipelineSteps.UnitTests.Steps
{
    public interface IStepToSkip : IStep
    {

    }

    public class StepToSkip : Step, IStepToSkip
    {
        private IStepTracker tracker;

        public StepToSkip(IStepTracker tracker) : base("StepToSkip")
        {
            this.tracker = tracker;
        }

        public override async Task ExecuteAsync(NextStepAction nextStep, params object[] arguments)
        {
            Console.WriteLine($"{DateTime.Now} - StepToSkip");

            if (tracker != null) tracker.Track("StepToSkip");

            await Task.Delay(2000);

            await nextStep();
        }
    }
}
