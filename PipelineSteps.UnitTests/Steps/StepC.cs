using PipelineSteps.UnitTests.TestAssets;
using System;
using System.Threading.Tasks;

namespace PipelineSteps.UnitTests.Steps
{
    public interface IStepC : IStep { 
    }

    public class StepC : Step, IStepC
    {
        private IStepTracker tracker;

        public StepC(IStepTracker tracker) : base("StepC")
        {
            this.tracker = tracker;
        }

        public override async Task ExecuteAsync(NextStepAction nextStep, params object[] arguments)
        {
            Console.WriteLine($"{DateTime.Now} - StepC");

            if (tracker != null) tracker.Track("StepC");

            await Task.Delay(2000);

            await nextStep();
        }
    }
}
