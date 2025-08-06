namespace Tests;

public interface ITestService { }
public class TestService : ITestService { }
public interface ITestServiceA { }
public class TestServiceA : ITestServiceA { }
public interface ITestServiceB { }
public class TestServiceB : ITestServiceB { }
public class TestParent(ITestServiceA childA) : ITestService { }
public class TestParentWithOptionalChild(ITestServiceA? childA = null) : ITestService { }
public class TestParentWithDefaultChild(int childA = 4) : ITestService { }
