using IndoorWayfinder.Api.Models.Responses;
using IndoorWayfinder.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace IndoorWayfinder.Api.Controllers;

[ApiController]
[Route("api/route")]
public class RouteController : ControllerBase
{
    private readonly RouteService _service;

    public RouteController(RouteService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<RouteResponse>> Route(RouteRequest req)
    {
        if (req.StartId == null || req.EndId == null)
            return BadRequest("Missing start or end");

        var result = await _service.ComputeRouteAsync(
            req.MapId,
            req.StartId.Value,
            req.EndId.Value);

        return Ok(result);
    }
}