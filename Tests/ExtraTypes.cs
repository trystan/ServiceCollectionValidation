namespace Tests;

public interface ITestService { }
public class TestService : ITestService { }
public interface ITestServiceA { }
public class TestServiceA : ITestServiceA { }
public interface ITestServiceB { }
public class TestServiceB : ITestServiceB { }

public interface ITestGeneric<T> { }
public class TestGeneric<T> : ITestGeneric<T> { }

#pragma warning disable CS9113 // Parameter is unread.
public class GenericUser(ITestGeneric<GenericUser> example) : ITestService { }
public class TestParent(ITestServiceA ChildA) : ITestService { }
public class TestParentWithOptionalChild(ITestServiceA? ChildA = null) : ITestService { }
public class TestParentWithDefaultChild(int SomeCompileTimeConstant = 8) : ITestService { }
#pragma warning restore CS9113 // Parameter is unread.

