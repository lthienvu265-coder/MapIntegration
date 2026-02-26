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
        if (req.start_id == null || req.end_id == null)
            return BadRequest("Missing start or end");

        var result = await _service.ComputeRouteAsync(
            req.map_id,
            req.start_id.Value,
            req.end_id.Value);

        return Ok(result);
    }
}