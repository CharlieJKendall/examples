using AutomatedTests.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTests.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly IMyService _myService;

    public TestController(IMyService myService)
    {
        _myService = myService;
    }

    [HttpGet("SingleReturnValue")] public DtoClass GetSingleReturnValue() => null!;
    [HttpGet("GenericEnumerableReturnValue")] public IEnumerable<DtoClass> GetGenericEnumerableReturnValue() => null!;
    [HttpGet("ArrayReturnValue")] public DtoClass[] GetArrayReturnValue() => null!;

    [HttpGet("SingleInputValue")] public void GetSingleInputValue(DtoClass param) { }
    [HttpGet("GenericEnumerableInputValue")] public void GetGenericEnumerableInputValue(IEnumerable<DtoClass> param) { }
    [HttpGet("ArrayInputValue")] public void GetArrayInputValue(DtoClass[] param) { }
}

public class DtoClass
{
    public DtoClassNested DtoClassNested { get; set; } = null!;
}

public class DtoClassNested
{
    public DtoClassNestedTwice DtoClassNestedTwice { get; set; } = null!;
}

public class DtoClassNestedTwice
{
    public DtoEnum DtoEnum { get; set; }
    public DtoEnumWithoutDefault DtoEnumWithoutDefault { get; set; }
}

public enum DtoEnum
{
    Zero = 0,
}

public enum DtoEnumWithoutDefault
{
    One = 1,
}