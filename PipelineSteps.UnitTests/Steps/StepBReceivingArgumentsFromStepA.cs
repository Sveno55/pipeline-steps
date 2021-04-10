using PipelineSteps.UnitTests.TestAssets;
using System;
using System.Threading.Tasks;

namespace PipelineSteps.UnitTests.Steps
{
    public interface IStepBReceivingArgumentsFromStepA : IStep
    {

    }

    public class StepBReceivingArgumentsFromStepA : Step, IStepBReceivingArgumentsFromStepA
    {
        private IStepTracker tracker;

        public StepBReceivingArgumentsFromStepA(IStepTracker tracker) : base("StepBReceivingArgumentsFromStepA")
        {
            this.tracker = tracker;
        }

        public override async Task ExecuteAsync(NextStepAction nextStep, params object[] arguments)
        {
            Console.WriteLine($"{DateTime.Now} - StepBReceivingArgumentsFromStepA");

            if (tracker != null) tracker.Track("StepBReceivingArgumentsFromStepA");

            tracker.Track(arguments[0].ToString());
            tracker.Track(arguments[1].ToString());
            tracker.Track(arguments[2].ToString());

            await Task.Delay(2000);
            
            await nextStep();
        }
    }
}
