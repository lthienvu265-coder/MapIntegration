namespace IndoorWayfinder.Api.Controllers
{
    using IndoorWayfinder.Api.Data;
    using IndoorWayfinder.Api.Models;
    using IndoorWayfinder.Api.Models.Requests;
    using IndoorWayfinder.Api.Utils;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Text.Json;

    [ApiController]
    [Route("api/edges")]
    public class EdgesController : ControllerBase
    {
        private readonly WayfinderContext _context;

        public EdgesController(WayfinderContext context)
        {
            _context = context;
        }

        // CREATE
        [HttpPost]
        public async Task<ActionResult<EdgeOut>> CreateEdge(EdgeIn payload)
        {
            var map = await _context.Maps.FindAsync(payload.MapId);
            if (map == null)
                return NotFound(new { detail = "Map không tồn tại." });

            var start = await _context.Nodes.FindAsync(payload.StartNodeId);
            var end = await _context.Nodes.FindAsync(payload.EndNodeId);

            if (start == null || end == null ||
                start.MapId != map.Id || end.MapId != map.Id)
                return BadRequest(new { detail = "Node không hợp lệ hoặc khác map." });

            int floor;

            if (payload.Floor.HasValue)
            {
                floor = payload.Floor.Value;
                if (start.Floor != floor || end.Floor != floor)
                    return BadRequest(new
                    {
                        detail = $"Floor không khớp: start({start.Floor})/end({end.Floor}) ≠ payload({floor})."
                    });
            }
            else
            {
                if (start.Floor != end.Floor)
                    return BadRequest(new
                    {
                        detail = $"Hai node không cùng tầng: start({start.Floor}) vs end({end.Floor})."
                    });

                floor = start.Floor;
            }

            var poly = payload.Polyline ?? new List<List<double>>();

            if (poly.Count < 2)
            {
                poly = new List<List<double>>
            {
                new() { start.X, start.Y },
                new() { end.X, end.Y }
            };
            }

            var weight = GeoUtils.PolylineLength(poly);

            var edge = new Edge
            {
                MapId = map.Id,
                StartNodeId = start.Id,
                EndNodeId = end.Id,
                Floor = floor,
                Polyline = JsonSerializer.Serialize(poly),
                Weight = weight,
                Bidirectional = payload.Bidirectional,
                Meta = payload.Meta
            };

            _context.Edges.Add(edge);
            await _context.SaveChangesAsync();

            return new EdgeOut
            {
                Id = edge.Id,
                MapId = edge.MapId,
                StartNodeId = edge.StartNodeId,
                EndNodeId = edge.EndNodeId,
                Floor = edge.Floor,
                Polyline = poly,
                Weight = edge.Weight,
                Bidirectional = edge.Bidirectional,
                Meta = edge.Meta
            };
        }

        // LIST
        [HttpGet]
        public async Task<ActionResult<List<EdgeOut>>> ListEdges(
            [FromQuery] int mapId,
            [FromQuery] int? floor)
        {
            var query = _context.Edges.Where(e => e.MapId == mapId);

            if (floor.HasValue)
                query = query.Where(e => e.Floor == floor.Value);

            var edges = await query.ToListAsync();

            return edges.Select(edge => new EdgeOut
            {
                Id = edge.Id,
                MapId = edge.MapId,
                StartNodeId = edge.StartNodeId,
                EndNodeId = edge.EndNodeId,
                Floor = edge.Floor,
                Polyline = JsonSerializer.Deserialize<List<List<double>>>(edge.Polyline)!,
                Weight = edge.Weight,
                Bidirectional = edge.Bidirectional,
                Meta = edge.Meta
            }).ToList();
        }

        // UPDATE
        [HttpPatch("{edgeId}")]
        public async Task<ActionResult<EdgeOut>> UpdateEdge(int edgeId, EdgeUpdate payload)
        {
            var edge = await _context.Edges.FindAsync(edgeId);
            if (edge == null)
                return NotFound(new { detail = "Edge không tồn tại." });

            bool changed = false;

            if (payload.Polyline != null)
            {
                if (payload.Polyline.Count < 2)
                    return BadRequest(new { detail = "Polyline cần >= 2 điểm." });

                edge.Polyline = JsonSerializer.Serialize(payload.Polyline);
                edge.Weight = GeoUtils.PolylineLength(payload.Polyline);
                changed = true;
            }

            if (payload.Bidirectional.HasValue)
            {
                edge.Bidirectional = payload.Bidirectional.Value;
                changed = true;
            }

            if (payload.Meta != null)
            {
                edge.Meta = payload.Meta;
                changed = true;
            }

            if (changed)
                await _context.SaveChangesAsync();

            return new EdgeOut
            {
                Id = edge.Id,
                MapId = edge.MapId,
                StartNodeId = edge.StartNodeId,
                EndNodeId = edge.EndNodeId,
                Floor = edge.Floor,
                Polyline = JsonSerializer.Deserialize<List<List<double>>>(edge.Polyline)!,
                Weight = edge.Weight,
                Bidirectional = edge.Bidirectional,
                Meta = edge.Meta
            };
        }

        // DELETE
        [HttpDelete("{edgeId}")]
        public async Task<IActionResult> DeleteEdge(int edgeId)
        {
            var edge = await _context.Edges.FindAsync(edgeId);
            if (edge == null)
                return NotFound(new { detail = "Edge không tồn tại." });

            _context.Edges.Remove(edge);
            await _context.SaveChangesAsync();

            return Ok(new { ok = true });
        }
    }
}
