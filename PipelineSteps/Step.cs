using System;
using System.Threading.Tasks;

namespace PipelineSteps
{
    public delegate Task NextStepAction(string nextStepName = "", params object[] arguments);
    public interface IStep
    {        
        Task ExecuteAsync(NextStepAction nextStep, params object[] arguments);
        string StepName { get; }
        void SetPipelineState(object stateObj);
        bool HasBeenExecuted { get; set; }
    }

    public abstract class Step : IStep
    {
        private string stepName;
        private object stateObj;        

        public Step(string stepName)
        {
            this.stepName = stepName;
            this.HasBeenExecuted = false;
        }

        public string StepName
        {
            get
            {
                return this.stepName;
            }
        }

        public bool HasBeenExecuted { get; set; }

        public void SetPipelineState(object stateObj)
        {
            this.stateObj = stateObj;
        }

        protected T GetPipelineState<T>()
        {
            if (this.stateObj == null) throw new Exception("Cannot use pipeline state because it is not set.");
            return (T)this.stateObj;
        }

        public abstract Task ExecuteAsync(NextStepAction nextStep, params object[] arguments);
    }
}
