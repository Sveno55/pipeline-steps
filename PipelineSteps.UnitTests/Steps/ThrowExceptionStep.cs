using PipelineSteps.UnitTests.TestAssets;
using System;
using System.Threading.Tasks;

namespace PipelineSteps.UnitTests.Steps
{
    public interface IThrowExceptionStep : IStep
    {

    }

    public class ThrowExceptionStep : Step, IThrowExceptionStep
    {
        private IStepTracker tracker;

        public ThrowExceptionStep(IStepTracker tracker) : base("ThrowExceptionStep")
        {
            this.tracker = tracker;
        }

        public override async Task ExecuteAsync(NextStepAction nextStep, params object[] arguments)
        {
            Console.WriteLine($"{DateTime.Now} - ThrowExceptionStep");

            if (tracker != null) tracker.Track("ThrowExceptionStep");

            throw new Exception("Exception occured in step: ThrowExceptionStep");            
        }
    }
}
