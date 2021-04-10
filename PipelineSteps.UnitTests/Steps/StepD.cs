using PipelineSteps.UnitTests.TestAssets;
using System;
using System.Threading.Tasks;

namespace PipelineSteps.UnitTests.Steps
{
    public interface IStepD : IStep
    {

    }

    public class StepD : Step, IStepD
    {
        private IStepTracker tracker;

        public StepD(IStepTracker tracker) : base("StepD")
        {
            this.tracker = tracker;
        }

        public override async Task ExecuteAsync(NextStepAction nextStep, params object[] arguments)
        {
            Console.WriteLine($"{DateTime.Now} - StepD");

            if (tracker != null) tracker.Track("StepD");

            await Task.Delay(2000);

            await nextStep();
        }
    }
}
