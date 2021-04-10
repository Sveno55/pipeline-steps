using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using PipelineSteps.UnitTests.TestAssets;
using PipelineSteps.UnitTests.Steps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipelineSteps.UnitTests
{
    [Parallelizable(ParallelScope.Children)]
    public class Tests
    {
        [Test]
        [Parallelizable]
        public async Task TestThatNextStepWithNoParametersExecutesStepsSequentially()
        {
            IPipeline pipeline = new Pipeline();

            IStepTracker fakeStepTracker = A.Fake<IStepTracker>();

            pipeline.registerRootStep(new StepA(fakeStepTracker));
            pipeline.registerStep(new StepB(fakeStepTracker));
            pipeline.registerStep(new StepC(fakeStepTracker));
            pipeline.registerStep(new StepD(fakeStepTracker));

            await pipeline.Run();

            A.CallTo(() => fakeStepTracker.Track("StepA")).MustHaveHappened()
                .Then(A.CallTo(() => fakeStepTracker.Track("StepB")).MustHaveHappened())
                .Then(A.CallTo(() => fakeStepTracker.Track("StepC")).MustHaveHappened())
                .Then(A.CallTo(() => fakeStepTracker.Track("StepD")).MustHaveHappened());
        }

        [Test]
        [Parallelizable]
        public async Task TestThatNextStepJumpsToSpecifiedStep()
        {
            IPipeline pipeline = new Pipeline();

            IStepTracker fakeStepTracker = A.Fake<IStepTracker>();

            pipeline.registerRootStep(new StepA(fakeStepTracker));
            pipeline.registerStep(new StepBSkipToStepD(fakeStepTracker));
            pipeline.registerStep(new StepC(fakeStepTracker));
            pipeline.registerStep(new StepD(fakeStepTracker));

            await pipeline.Run();

            A.CallTo(() => fakeStepTracker.Track("StepA")).MustHaveHappened()
                .Then(A.CallTo(() => fakeStepTracker.Track("StepBSkipToStepD")).MustHaveHappened())                
                .Then(A.CallTo(() => fakeStepTracker.Track("StepD")).MustHaveHappened());

            A.CallTo(() => fakeStepTracker.Track("StepC")).MustNotHaveHappened();
        }

        [Test]
        [Parallelizable]
        public async Task TestThatNotCallingNextStepEndsPipelineExecution()
        {
            IPipeline pipeline = new Pipeline();

            IStepTracker fakeStepTracker = A.Fake<IStepTracker>();

            pipeline.registerRootStep(new StepA(fakeStepTracker));
            pipeline.registerStep(new StepBWithNoCallToNextStep(fakeStepTracker));
            pipeline.registerStep(new StepC(fakeStepTracker));
            pipeline.registerStep(new StepD(fakeStepTracker));

            await pipeline.Run();

            A.CallTo(() => fakeStepTracker.Track("StepA")).MustHaveHappened()
                .Then(A.CallTo(() => fakeStepTracker.Track("StepBWithNoCallToNextStep")).MustHaveHappened());

            A.CallTo(() => fakeStepTracker.Track("StepC")).MustNotHaveHappened();
            A.CallTo(() => fakeStepTracker.Track("StepD")).MustNotHaveHappened();
        }

        [Test]
        [Parallelizable]
        public async Task TestPassingDataFromOnStepToTheNext()
        {
            IPipeline pipeline = new Pipeline();

            IStepTracker fakeStepTracker = A.Fake<IStepTracker>();

            pipeline.registerRootStep(new StepAPassingArgumentsToStepB(fakeStepTracker));
            pipeline.registerStep(new StepBReceivingArgumentsFromStepA(fakeStepTracker));

            await pipeline.Run();

            A.CallTo(() => fakeStepTracker.Track("StepAPassingArgumentsToStepB")).MustHaveHappened()
                .Then(A.CallTo(() => fakeStepTracker.Track("StepBReceivingArgumentsFromStepA")).MustHaveHappened())
                .Then(A.CallTo(() => fakeStepTracker.Track("True")).MustHaveHappened())
                .Then(A.CallTo(() => fakeStepTracker.Track("1234")).MustHaveHappened())
                .Then(A.CallTo(() => fakeStepTracker.Track("hello world")).MustHaveHappened());
        }

        [Test]
        [Parallelizable]
        public async Task TestPipelineReturnObjects()
        {
            IPipeline pipeline = new Pipeline();

            IStepTracker fakeStepTracker = A.Fake<IStepTracker>();

            pipeline.registerRootStep(new StepAPassingArgumentsToStepB(fakeStepTracker));

            object[] results = await pipeline.Run();

            Assert.AreEqual(true, results[0]);
            Assert.AreEqual(1234, results[1]);
            Assert.AreEqual("hello world", results[2]);
        }

        [Test]
        [Parallelizable]
        public async Task TestOutputsOfStepAPassingArgumentsToStepB()
        {
            IStepTracker fakeStepTracker = A.Fake<IStepTracker>();

            IStep stepA = new StepAPassingArgumentsToStepB(fakeStepTracker);

            NextStepAction fakeNextStep = A.Fake<NextStepAction>();

            await stepA.ExecuteAsync(fakeNextStep, "bill.gates@gmail.com", 21, true, new List<string>());

            A.CallTo(() => fakeNextStep.Invoke(string.Empty, true, 1234, "hello world")).MustHaveHappened();
        }

        [Test]
        [Parallelizable]
        public async Task TestWithDependencyInjection()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<IStepTracker, StepTracker>();
            services.AddSingleton<IStepA, StepA>();
            services.AddSingleton<IStepB, StepB>();
            services.AddSingleton<IStepC, StepC>();
            services.AddSingleton<IStepD, StepD>();
            services.AddSingleton<IPipelineBuilder, PipelineBuilder>();

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            IPipelineBuilder pipelineBuilder = serviceProvider.GetRequiredService<IPipelineBuilder>();

            IPipeline pipeline = pipelineBuilder.Build();

            await pipeline.Run();

            //IStepTracker is a dependency for StepA and StepB. It has been injected by the DI container.
            IStepTracker stepTracker = serviceProvider.GetRequiredService<IStepTracker>();
            
            List<string> traces = stepTracker.GetTraces();

            Assert.AreEqual(4, traces.Count);
            Assert.AreEqual("StepA", traces[0]);
            Assert.AreEqual("StepB", traces[1]);
            Assert.AreEqual("StepC", traces[2]);
            Assert.AreEqual("StepD", traces[3]);
        }

        [Test]
        [Parallelizable]
        public async Task TestPipelineState()
        {
            IPipeline pipeline = new Pipeline();

            IStepTracker fakeStepTracker = A.Fake<IStepTracker>();

            pipeline.registerRootStep(new StepAWithPipelineState(fakeStepTracker));

            MyCustomPipelineState state = new MyCustomPipelineState();
            state.FirstName = "";
            state.LastName = "";

            pipeline.SetPipelineState(state);

            await pipeline.Run();

            Assert.AreEqual("Bill", state.FirstName);
            Assert.AreEqual("Gates", state.LastName);
        }

        [Test]
        [Parallelizable]
        public async Task TestPipelineStateAccross2Steps()
        {
            IPipeline pipeline = new Pipeline();

            IStepTracker fakeStepTracker = A.Fake<IStepTracker>();

            pipeline.registerRootStep(new StepAWithPipelineState(fakeStepTracker));
            pipeline.registerStep(new StepBWithPipelineState(fakeStepTracker));

            MyCustomPipelineState state = new MyCustomPipelineState();
            state.FirstName = "";
            state.LastName = "";

            pipeline.SetPipelineState(state);

            await pipeline.Run();

            A.CallTo(() => fakeStepTracker.Track("Bill")).MustHaveHappened()
                .Then(A.CallTo(() => fakeStepTracker.Track("Gates")).MustHaveHappened());

            MyCustomPipelineState finalState = pipeline.GetCurrentPipelineState<MyCustomPipelineState>();

            Assert.AreEqual("Hello World!", finalState.FirstName);
            Assert.AreEqual("", finalState.LastName);
        }

        [Test]
        [Parallelizable]
        public async Task TestRunTwoPipelinesInParallel()
        {
            IStepTracker fakeTracker1 = A.Fake<IStepTracker>();
            IStepTracker fakeTracker2 = A.Fake<IStepTracker>();
            IPipeline pipeline1 = new Pipeline();
            IPipeline pipeline2 = new Pipeline();

            pipeline1.registerRootStep(new StepA(fakeTracker1));
            pipeline1.registerStep(new StepB(fakeTracker1));
            pipeline1.registerStep(new StepC(fakeTracker1));
            pipeline1.registerStep(new StepD(fakeTracker1));

            pipeline2.registerRootStep(new StepAWithPipelineState(fakeTracker2));
            pipeline2.registerStep(new StepBWithPipelineState(fakeTracker2));

            pipeline2.SetPipelineState(new MyCustomPipelineState());

            Task task1 = pipeline1.Run();
            Task task2 = pipeline2.Run();

            await Task.WhenAll(task1, task2);

            A.CallTo(() => fakeTracker1.Track("StepA")).MustHaveHappened()
                .Then(A.CallTo(() => fakeTracker1.Track("StepB")).MustHaveHappened())
                .Then(A.CallTo(() => fakeTracker1.Track("StepC")).MustHaveHappened())
                .Then(A.CallTo(() => fakeTracker1.Track("StepD")).MustHaveHappened());

            MyCustomPipelineState finalState = pipeline2.GetCurrentPipelineState<MyCustomPipelineState>();

            Assert.AreEqual("Hello World!", finalState.FirstName);
            Assert.AreEqual("", finalState.LastName);
        }


        [Test]
        [Parallelizable]
        public async Task TestThatStepExceptionHaltsPipelineAndBubblesUpException()
        {
            IPipeline pipeline = new Pipeline();

            IStepTracker fakeStepTracker = A.Fake<IStepTracker>();

            pipeline.registerRootStep(new StepA(fakeStepTracker));
            pipeline.registerStep(new StepB(fakeStepTracker));
            pipeline.registerStep(new ThrowExceptionStep(fakeStepTracker));
            pipeline.registerStep(new StepD(fakeStepTracker));

            try
            {
                await pipeline.Run();

                Assert.Fail("Expected exception. None occured");
            }
            catch(Exception ex)
            {
                A.CallTo(() => fakeStepTracker.Track("StepA")).MustHaveHappened()
                    .Then(A.CallTo(() => fakeStepTracker.Track("StepB")).MustHaveHappened());

                A.CallTo(() => fakeStepTracker.Track("StepD")).MustNotHaveHappened();
            }

        }

        [Test]
        [Parallelizable]
        public void TestThatMultipleCallsToRegisterRootStepWillFail()
        {
            IPipeline pipeline = new Pipeline();

            IStepTracker fakeStepTracker = A.Fake<IStepTracker>();

            try
            {
                pipeline.registerRootStep(new StepA(fakeStepTracker));
                pipeline.registerRootStep(new StepA(fakeStepTracker));
                pipeline.registerRootStep(new StepA(fakeStepTracker));

                Assert.Fail("Expected exception. None occured");
            }
            catch(Exception ex)
            {
                Assert.AreEqual("Pipeline root step already set!", ex.Message);
            }
        }

        [Test]
        [Parallelizable]
        public void TestThatWhenNoRootStepRegisteredThrowsException()
        {
            IPipeline pipeline = new Pipeline();

            IStepTracker fakeStepTracker = A.Fake<IStepTracker>();

            try
            {
                pipeline.registerStep(new StepA(fakeStepTracker));
                pipeline.registerStep(new StepA(fakeStepTracker));
                pipeline.registerStep(new StepA(fakeStepTracker));

                Assert.Fail("Expected exception. None occured");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Root step not registered. Register root step using `void registerRootStep(IStep step)`", ex.Message);
            }
        }

        [Test]
        [Parallelizable]
        public async Task TestThatExceptionOccursWhenNextStepNameDoesntExist()
        {
            IPipeline pipeline = new Pipeline();

            IStepTracker fakeStepTracker = A.Fake<IStepTracker>();

            try
            {
                pipeline.registerRootStep(new StepAWithNextStepThatDoesntExist(fakeStepTracker));

                await pipeline.Run();

                Assert.Fail("Expected exception. None occured");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Next step 'asofkj2r2rjflqq23' doesn't exist!", ex.Message);
            }
        }

        [Test]
        [Parallelizable]
        public async Task TestThatExceptionThrownWhenUsingPipelineStepThatIsNotSet()
        {
            IPipeline pipeline = new Pipeline();

            IStepTracker fakeStepTracker = A.Fake<IStepTracker>();

            try
            {
                pipeline.registerRootStep(new StepAWithPipelineState(fakeStepTracker));

                await pipeline.Run();

                Assert.Fail("Expected exception. None occured");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Cannot use pipeline state because it is not set.", ex.Message);
            }
        }

        [Test]
        [Parallelizable]
        public async Task TestThatStepCannotLoopBack()
        {
            IPipeline pipeline = new Pipeline();

            IStepTracker fakeStepTracker = A.Fake<IStepTracker>();

            pipeline.registerRootStep(new StepA(fakeStepTracker));
            pipeline.registerStep(new StepDBackToStepA(fakeStepTracker));

            try
            {
                await pipeline.Run();

                Assert.Fail("Expected exception. None occured");
            }
            catch(Exception ex)
            {
                Assert.AreEqual("Error, step 'StepA' has already been executed.", ex.Message);
            }
        }

        [Test]
        [Parallelizable]
        public async Task TestThatCannotRerunPipelineIfFlagsAreNotReset()
        {
            IPipeline pipeline = new Pipeline();

            IStepTracker fakeStepTracker = A.Fake<IStepTracker>();

            pipeline.registerRootStep(new StepA(fakeStepTracker));
            pipeline.registerStep(new StepB(fakeStepTracker));
            pipeline.registerStep(new StepC(fakeStepTracker));
            pipeline.registerStep(new StepD(fakeStepTracker));

            try
            {
                await pipeline.Run();

                await pipeline.Run();

                Assert.Fail("Expected exception. None occured");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Error, step 'StepA' has already been executed.", ex.Message);
            }
        }

        [Test]
        [Parallelizable]
        public void TestThatPipelinesCanBeReRunWhenFlagsAreReset()
        {
            IPipeline pipeline = new Pipeline();

            IStepTracker fakeStepTracker = A.Fake<IStepTracker>();

            pipeline.registerRootStep(new StepA(fakeStepTracker));
            pipeline.registerStep(new StepB(fakeStepTracker));
            pipeline.registerStep(new StepC(fakeStepTracker));
            pipeline.registerStep(new StepD(fakeStepTracker));

            Assert.DoesNotThrowAsync(async () => {
                await pipeline.Run();

                pipeline.resetAllExecutionFlags();

                await pipeline.Run();
            });
        }
    }
}