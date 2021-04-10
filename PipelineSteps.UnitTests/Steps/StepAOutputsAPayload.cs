using PipelineSteps.UnitTests.TestAssets;
using System;
using System.Threading.Tasks;

namespace PipelineSteps.UnitTests.Steps
{
    public interface IStepAOutputsAPayload : IStep
    {

    }

    public class StepAOutputsAPayload : Step, IStepAOutputsAPayload
    {
        private IStepTracker tracker;

        public StepAOutputsAPayload(IStepTracker tracker) : base("StepAOutputsAPayload")
        {
            this.tracker = tracker;
        }

        public override async Task ExecuteAsync(NextStepAction nextStep, params object[] arguments)
        {
            Console.WriteLine($"{DateTime.Now} - StepAOutputsAPayload");

            if (tracker != null) this.tracker.Track("StepAOutputsAPayload");

            await Task.Delay(2000);

            await nextStep();
        }
    }
}
