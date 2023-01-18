using Bet.Extensions.Resilience.WebApp.Sample.Models;

using Microsoft.AspNetCore.Mvc;

namespace Bet.Extensions.Resilience.WebApp.Sample.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ThrowController : ControllerBase
{
    private readonly ThrowModel _flag;

    public ThrowController(ThrowModel flag)
    {
        _flag = flag;
    }

    [HttpGet]
    public ActionResult RaiseBadRequest()
    {
        if (_flag.IsLocked)
        {
            _flag.IsLocked = false;
            return Ok();
        }

        _flag.IsLocked = true;
        return StatusCode(500);
    }
}
