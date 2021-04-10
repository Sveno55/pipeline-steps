using PipelineSteps.UnitTests.TestAssets;
using System;
using System.Threading.Tasks;

namespace PipelineSteps.UnitTests.Steps
{
    public interface IStepAWithNextStepThatDoesntExist : IStep
    {

    }

    public class StepAWithNextStepThatDoesntExist : Step, IStepAWithNextStepThatDoesntExist
    {
        private IStepTracker tracker;

        public StepAWithNextStepThatDoesntExist(IStepTracker tracker) : base("StepAWithNextStepThatDoesntExist")
        {
            this.tracker = tracker;
        }

        public override async Task ExecuteAsync(NextStepAction nextStep, params object[] arguments)
        {
            Console.WriteLine($"{DateTime.Now} - StepAWithNextStepThatDoesntExist");
            if (tracker != null) this.tracker.Track("StepAWithNextStepThatDoesntExist");

            await Task.Delay(2000);

            await nextStep("asofkj2r2rjflqq23");
        }
    }
}
