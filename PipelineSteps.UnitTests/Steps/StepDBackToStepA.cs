using PipelineSteps.UnitTests.TestAssets;
using System;
using System.Threading.Tasks;

namespace PipelineSteps.UnitTests.Steps
{
    public interface IStepDBackToStepA : IStep
    {

    }

    public class StepDBackToStepA : Step, IStepDBackToStepA
    {
        private IStepTracker tracker;

        public StepDBackToStepA(IStepTracker tracker) : base("StepDBackToStepA")
        {
            this.tracker = tracker;
        }

        public override async Task ExecuteAsync(NextStepAction nextStep, params object[] arguments)
        {
            Console.WriteLine($"{DateTime.Now} - StepDBackToStepA");

            if (tracker != null) tracker.Track("StepDBackToStepA");

            await Task.Delay(2000);

            await nextStep("StepA");
        }
    }
}
