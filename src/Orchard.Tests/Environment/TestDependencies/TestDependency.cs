namespace Orchard.Tests.Environment.TestDependencies {
    
    public interface ITestDependency : IDependency {
        
    }

    public class TestDependency : ITestDependency{
    }

    public interface ITestSingletonDependency : ISingletonDependency {

    }

    public class TestSingletonDependency : ITestSingletonDependency {
    }


    public interface ITestTransientDependency : ITransientDependency {

    }

    public class TestTransientDependency : ITestTransientDependency {
    }
}
