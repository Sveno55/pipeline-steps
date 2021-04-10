using PipelineSteps.UnitTests.TestAssets;
using System;
using System.Threading.Tasks;

namespace PipelineSteps.UnitTests.Steps
{
    public interface IStepA : IStep
    {

    }

    public class StepA : Step, IStepA
    {
        private IStepTracker tracker;

        public StepA(IStepTracker tracker) : base("StepA")
        {
            this.tracker = tracker;
        }

        public override async Task ExecuteAsync(NextStepAction nextStep, params object[] arguments)
        {
            Console.WriteLine($"{DateTime.Now} - StepA");
            if (tracker != null) this.tracker.Track("StepA");

            await Task.Delay(2000);

            await nextStep();
        }
    }
}
