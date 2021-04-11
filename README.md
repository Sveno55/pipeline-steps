# pipeline-steps

This small project is a lightweight framework that facilitates writing unit tests for functions that contain heavily layered and/or sequential business rules. 

The strategy is to breakdown the 'monolith' of business rules into a series of steps that are individually much more easy to unit test and that can be executed as a whole using a `Pipeline` 

#### Table of Contents  
* [Example](#Example)
* [Usage](#Usage)  
  * [Create your pipeline](#create-your-pipeline)
  * [Create your custom step](#create-your-custom-step)
  * [Register your step](#register-your-step)
  * [Execute the pipeline](#execute-the-pipeline)
* [Pipeline output](#pipeline-output)
* [Sharing data](#sharing-data)
* [Dependency Injection & Unit Testing](#dependency-injection--unit-testing)
* [Running Pipelines in Parallel](#running-pipelines-in-parallel)
<a name="Example"/>
<a name="Usage"/>
<a name="create-your-pipeline"/>
<a name="create-your-custom-step"/>
<a name="register-your-step"/>
<a name="execute-the-pipeline"/>
<a name="pipeline-output"/>
<a name="sharing-data"/>
<a name="dependency-injection--unit-testing"/>
<a name="running-pipelines-in-parallel"/>


## Example

```
    IPipeline pipeline = new Pipeline();

    pipeline.registerRootStep(new StepA());
    pipeline.registerStep(new StepB());
    pipeline.registerStep(new StepC());
    pipeline.registerStep(new StepD());

    await pipeline.Run();
```

## Usage

### Create your pipeline
Create a pipeline of steps to execute
```
IPipeline pipeline = new Pipeline();
```
---
### Create your custom step
Create your custom step's interface that extends `IStep`
```
public interface IMyCustomStep : IStep
```
---
Create the concrete class of your custom step that extends `Step` and implements your custom interface `IMyCustomStep`
```
public class MyCustomStep : Step, IMyCustomStep
```
---
In the constructor of your concrete custom step class, call the base classe's constructor by providing the step's name. Make sure that your **step name is unique** accross all steps in the pipeline. You can use your constructor's signature to inject the dependencies you need for this step.
```
public MyCustomStep() : base("MyCustomStepName")
```
---
The abstract Step class will force you to implement the function `async Task ExecuteAsync(NextStepAction nextStep, params object[] arguments)`. This is where you will implement your business rule's main logic.

The invocation of the `nextStep` action allows to chain/navigate to the next sequential step.

* `nextStep()` : with no parameters, will tell the pipeline to move to the next sequentially registered step.
* `nextStep("StepB")` : the first parameter is a step name, it tells the pipeline to move to a specific step.
* `nextStep("StepB", ..., ...)` : the following parameters, will be passed on as an array of `params object[] arguments` to the next step. This is one of the methods to pass data from one step to the next.
```
public override async Task ExecuteAsync(NextStepAction nextStep, params object[] arguments)
{
  /** Your step's main business rule goes here **/
  
  nextStep();
}
```
### Register your step
```
pipeline.registerRootStep(new MyCustomStep());
```

### Execute the pipeline
```
await pipeline.Run();
```

## Pipeline output
The pipeline returns an array of objects that represents the arguments passed to the last step's `nextStep()` call. If no arguments are passed then the returned array of objects will be empty.
```
object[] results = await pipeline.Run();
```

## Sharing data
There are two ways of sharing data between pipelines.
#### Method 1
* By passing data through `nextStep()` action. The first parameter is the next step name and any following parameters is passed on as `params object[] arguments` to the next step.

#### Method 2 
* You can specify a pipeline state which will be accessible to all steps. A pipeline state is simply a custom POCO class. **It is important to `.SetPipelineState(..)` only after all the steps have been registered.** The POCO class instance is registered to the pipeline as follow:
```
  MyCustomPOCO myCustomPOCO = new MyCustomPOCO();
  myCustomPOCO.FirstName = "";
  myCustomPOCO.LastName = "";

  pipeline.SetPipelineState(myCustomPOCO);
```
To access the state from within a step, use the following:
```
        public override async Task ExecuteAsync(NextStepAction nextStep, params object[] arguments)
        {
            MyCustomPOCO state = GetPipelineState<MyCustomPOCO>();

            state.FirstName = "John";
            state.LastName = "Doe";

            await nextStep();
        }
```

if you wish to access the pipeline state after your pipeline execution, you can use the following pipeline method:
```
pipeline.GetCurrentPipelineState<MyCustomPOCOState>();
```

## Dependency Injection & Unit Testing
It's simply a matter of registering your custom interface `IMyCustomStep` (which inherits from `IStep`) to your DI container. For example:
```
IServiceCollection services = new ServiceCollection();

services.AddSingleton<IMyCustomStepA, MyCustomStepA>();
services.AddSingleton<IMyCustomStepB, MyCustomStepB>();
services.AddSingleton<IMyCustomStepC, MyCustomStepC>();
services.AddSingleton<IMyCustomStepD, MyCustomStepD>();
```
Here's an example of unit testing a step (with Fake.It.Easy):
```
  [Test]
  [Parallelizable]
  public async Task TestStepExample()
  {
      //Mocking the nextStep action
      NextStepAction fakeNextStep = A.Fake<NextStepAction>();

      //Instantiating the concrete step class (to test) and passing any necessary mocked dependencies
      IStep myCustomStep = new MyCustomStep();
      
      //control over the inputs as pipeline state
      myCustomStep.SetPipelineState(new CustomState() { FirstName = "John", Age = 21 });
      
      //control of the inputs as nextStep arguments
      await stepA.ExecuteAsync(fakeNextStep, "john.doe@gmail.com", 21, true, new List<string>());

      //Asserting values passed to the nextStep action.
      A.CallTo(() => fakeNextStep.Invoke(string.Empty, true, 1234, "hello world")).MustHaveHappened();
  }
```

## Running Pipelines in Parallel
```
    IPipeline pipeline1 = new Pipeline();
    IPipeline pipeline2 = new Pipeline();

    pipeline1.registerRootStep(new StepA());
    pipeline1.registerStep(new StepB());
    
    pipeline2.registerRootStep(new Step1());
    pipeline2.registerStep(new Step2());

    Task task1 = pipeline1.Run();
    Task task2 = pipeline2.Run();
    
    await Task.WhenAll(task1, task2);
```
