using System;
using System.Collections.Generic;

namespace PipelineSteps.UnitTests.TestAssets
{
    public interface IStepTracker
    {
        void Track(string trace);
        List<string> GetTraces();
    }

    public class StepTracker : IStepTracker
    {
        private List<string> traces;
        
        public StepTracker()
        {
            traces = new List<string>();
        }
        public void Track(string trace)
        {
            Console.WriteLine(trace);
            traces.Add(trace);
        }

        public List<string> GetTraces()
        {
            return traces;
        }
    }
}
