using PipelineSteps.UnitTests.Steps;

namespace PipelineSteps.UnitTests.TestAssets
{
    public interface IPipelineBuilder
    {
        IPipeline Build();
    }
    public class PipelineBuilder : IPipelineBuilder
    {
        private IStepA stepA;
        private IStepB stepB;
        private IStepC stepC;
        private IStepD stepD;

        public PipelineBuilder(IStepA stepA, IStepB stepB, IStepC stepC, IStepD stepD)
        {
            this.stepA = stepA;
            this.stepB = stepB;
            this.stepC = stepC;
            this.stepD = stepD;
        }

        public IPipeline Build()
        {
            IPipeline pipeline = new Pipeline();
            pipeline.registerRootStep(this.stepA);
            pipeline.registerStep(this.stepB);
            pipeline.registerStep(this.stepC);
            pipeline.registerStep(this.stepD);
            return pipeline;
        }
    }
}
