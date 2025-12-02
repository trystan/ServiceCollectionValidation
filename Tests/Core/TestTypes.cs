using System;

namespace Tests.Core;

public interface ITestService { }
public class TestService : ITestService { }
public interface ITestServiceA { }
public class TestServiceA : ITestServiceA { }
public interface ITestServiceB { }
public class TestServiceB : ITestServiceB { }

public interface ITestGeneric<T> { }
public class TestGeneric<T> : ITestGeneric<T> { }

#pragma warning disable CS9113 // Parameter is unread.
#pragma warning disable IDE0060 // Remove unused parameter
public class GenericUser(ITestGeneric<GenericUser> example) : ITestService { }
public class ServiceProviderUser(IServiceProvider example) : ITestService { }
public class TestParent(ITestServiceA ChildA) : ITestService { }
public class TestParentWithDefaultChild(int SomeCompileTimeConstant = 8) : ITestService { }

public class TestParentWithTwoConstructors : ITestService
{
    public TestParentWithTwoConstructors() { }

    public TestParentWithTwoConstructors(ITestServiceA childA) { }
}
#pragma warning restore CS9113 // Parameter is unread.
#pragma warning restore IDE0060 // Remove unused parameter

