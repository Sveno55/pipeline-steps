using PipelineSteps.UnitTests.TestAssets;
using System;
using System.Threading.Tasks;

namespace PipelineSteps.UnitTests.Steps
{
    public interface IStepBSkipToStepD : IStep
    {

    }

    public class StepBSkipToStepD : Step, IStepBSkipToStepD
    {
        private IStepTracker tracker;

        public StepBSkipToStepD(IStepTracker tracker) : base("StepBSkipToStepD")
        {
            this.tracker = tracker;
        }

        public override async Task ExecuteAsync(NextStepAction nextStep, params object[] arguments)
        {
            Console.WriteLine($"{DateTime.Now} - StepBSkipToStepD");

            if (tracker != null) tracker.Track("StepBSkipToStepD");

            await Task.Delay(2000);

            await nextStep("StepD");
        }
    }
}
