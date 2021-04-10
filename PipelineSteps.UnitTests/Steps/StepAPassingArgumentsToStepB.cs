using PipelineSteps.UnitTests.TestAssets;
using System;
using System.Threading.Tasks;

namespace PipelineSteps.UnitTests.Steps
{
    public interface IStepAPassingArgumentsToStepB : IStep
    {

    }

    public class StepAPassingArgumentsToStepB : Step, IStepAPassingArgumentsToStepB
    {
        private IStepTracker tracker;

        public StepAPassingArgumentsToStepB(IStepTracker tracker) : base("StepAPassingArgumentsToStepB")
        {
            this.tracker = tracker;
        }

        public override async Task ExecuteAsync(NextStepAction nextStep, params object[] arguments)
        {
            Console.WriteLine($"{DateTime.Now} - StepAPassingArgumentsToStepB");

            if (tracker != null) this.tracker.Track("StepAPassingArgumentsToStepB");

            await Task.Delay(2000);

            await nextStep(string.Empty, true, 1234, "hello world");
        }
    }
}
