using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipelineSteps
{
    public interface IPipeline
    {
        void SetPipelineState(object stateObject);
        T GetCurrentPipelineState<T>();
        void registerRootStep(IStep step);
        void registerStep(IStep step);
        void clearAllSteps();
        void resetAllExecutionFlags();
        Task<object[]> Run();
    }

    public class Pipeline : IPipeline
    {
        private List<IStep> steps;
        private int stepsIndexCursor;
        private string pipelineName;
        private object[] lastStepArguments;
        private object pipelineState;

        public Pipeline()
        {
            steps = new List<IStep>();
            pipelineName = string.Empty;
        }

        public Pipeline(string pipelineName)
        {
            this.pipelineName = pipelineName;
        }

        public string PipelineName
        {
            get {
                return this.pipelineName;
            }

            set {
                this.pipelineName = value;
            }
        }

        public T GetCurrentPipelineState<T>()
        {
            return (T)this.pipelineState;
        }

        public void registerRootStep(IStep step)
        {
            if (steps.Count == 0)
            {             
                step.SetPipelineState(this.pipelineState);
                steps.Add(step);
            } 
            else
            {
                throw new Exception("Pipeline root step already set!");
            }
        }

        public void registerStep(IStep step)
        {
            if (steps.Count == 0) throw new Exception("Root step not registered. Register root step using `void registerRootStep(IStep step)`");

            step.SetPipelineState(this.pipelineState);

            steps.Add(step);
        }

        private async Task GoNext(string nextStepName, params object[] arguments)
        {
            steps[stepsIndexCursor].HasBeenExecuted = true;

            if (nextStepName == string.Empty)
            {
                stepsIndexCursor++;
            }
            else
            {
                stepsIndexCursor = steps.FindIndex(step => step.StepName == nextStepName); 
            }

            if (stepsIndexCursor == -1)
            {
                throw new Exception($"Next step '{nextStepName}' doesn't exist!");
            }

            if (stepsIndexCursor < steps.Count)
            {
                NextStepAction nextStepAction = GoNext;
                
                if (steps[stepsIndexCursor].HasBeenExecuted) 
                    throw new Exception($"Error, step '{steps[stepsIndexCursor].StepName}' has already been executed.");

                await steps[stepsIndexCursor].ExecuteAsync(nextStepAction, arguments);
            } 
            else
            {
                lastStepArguments = arguments;
            }
        }

        public async Task<object[]> Run()
        {
            if (steps.Count > 0)
            {
                NextStepAction nextStepAction = GoNext;

                stepsIndexCursor = 0;

                if (steps[stepsIndexCursor].HasBeenExecuted) 
                    throw new Exception($"Error, step '{steps[stepsIndexCursor].StepName}' has already been executed.");

                await steps[stepsIndexCursor].ExecuteAsync(nextStepAction);

                return this.lastStepArguments;
            }
            return null;
        }

        public void SetPipelineState(object stateObject)
        {
            this.pipelineState = stateObject;

            if (steps.Count > 0) 
            {
                steps.ForEach(step => {
                    step.SetPipelineState(this.pipelineState);
                });
            }
            else
            {
                throw new Exception("Call 'SetPipelineState' after registering steps");
            }
        }

        public void clearAllSteps()
        {
            steps.Clear();
        }

        public void resetAllExecutionFlags()
        {
            steps.ForEach(step => { step.HasBeenExecuted = false; });
        }
    }
}
