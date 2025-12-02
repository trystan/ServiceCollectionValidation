using Microsoft.AspNetCore.Mvc;
using Tests.Core;

namespace Tests.AspNetCore;

#pragma warning disable CS9113 // Parameter is unread.
public class TestController(ITestService service) : ControllerBase
{
}

[NonController]
public class TestNonController(ITestService service) : ControllerBase
{
}
#pragma warning restore CS9113 // Parameter is unread.