using Microsoft.AspNetCore.Mvc;
using Tests.Core;

namespace Tests.AspNetCore;

public class TestController : ControllerBase
{
#pragma warning disable CS9113 // Parameter is unread.
    public TestController(ITestService service)
#pragma warning restore CS9113 // Parameter is unread.
    {
    }
}

[NonController]
public class TestNonController : ControllerBase
{
#pragma warning disable CS9113 // Parameter is unread.
    public TestNonController(ITestService service)
#pragma warning restore CS9113 // Parameter is unread.
    {
    }
}
