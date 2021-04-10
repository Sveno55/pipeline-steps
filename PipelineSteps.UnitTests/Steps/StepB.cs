using PipelineSteps.UnitTests.TestAssets;
using System;
using System.Threading.Tasks;

namespace PipelineSteps.UnitTests.Steps
{
    public interface IStepB : IStep
    {

    }

    public class StepB : Step, IStepB
    {
        private IStepTracker tracker;

        public StepB(IStepTracker tracker) : base("StepB")
        {
            this.tracker = tracker;
        }

        public override async Task ExecuteAsync(NextStepAction nextStep, params object[] arguments)
        {
            Console.WriteLine($"{DateTime.Now} - StepB");

            if (tracker != null) tracker.Track("StepB");

            await Task.Delay(2000);
            
            await nextStep();
        }
    }
}
