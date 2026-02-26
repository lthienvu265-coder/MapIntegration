namespace IndoorWayfinder.Api.Controllers
{
    using IndoorWayfinder.Api.Data;
    using IndoorWayfinder.Api.Models;
    using IndoorWayfinder.Api.Models.Requests;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [ApiController]
    [Route("api/nodes")]
    public class NodesController : ControllerBase
    {
        private readonly WayfinderContext _context;

        public NodesController(WayfinderContext context)
        {
            _context = context;
        }

        // CREATE
        [HttpPost]
        public async Task<ActionResult<NodeOut>> CreateNode(NodeIn payload)
        {
            var map = await _context.Maps.FindAsync(payload.MapId);
            if (map == null)
                return NotFound(new { detail = "Map không tồn tại." });

            var node = new Node
            {
                MapId = payload.MapId,
                X = payload.X,
                Y = payload.Y,
                IsLandmark = payload.IsLandmark,
                Floor = payload.Floor,
                Meta = payload.Meta
            };

            _context.Nodes.Add(node);
            await _context.SaveChangesAsync();

            return new NodeOut
            {
                Id = node.Id,
                MapId = node.MapId,
                X = node.X,
                Y = node.Y,
                IsLandmark = node.IsLandmark,
                Floor = node.Floor,
                Meta = node.Meta
            };
        }

        // LIST
        [HttpGet]
        public async Task<ActionResult<List<NodeOut>>> ListNodes(
            [FromQuery] int mapId,
            [FromQuery] int? floor)
        {
            var query = _context.Nodes.Where(n => n.MapId == mapId);

            if (floor.HasValue)
                query = query.Where(n => n.Floor == floor.Value);

            var nodes = await query.ToListAsync();

            return nodes.Select(n => new NodeOut
            {
                Id = n.Id,
                MapId = n.MapId,
                X = n.X,
                Y = n.Y,
                IsLandmark = n.IsLandmark,
                Floor = n.Floor,
                Meta = n.Meta
            }).ToList();
        }

        // GET BY ID
        [HttpGet("{nodeId}")]
        public async Task<ActionResult<NodeOut>> GetNode(int nodeId)
        {
            var node = await _context.Nodes.FindAsync(nodeId);
            if (node == null)
                return NotFound(new { detail = "Node không tồn tại." });

            return new NodeOut
            {
                Id = node.Id,
                MapId = node.MapId,
                X = node.X,
                Y = node.Y,
                IsLandmark = node.IsLandmark,
                Floor = node.Floor,
                Meta = node.Meta
            };
        }

        // UPDATE (PATCH)
        [HttpPatch("{nodeId}")]
        public async Task<ActionResult<NodeOut>> UpdateNode(int nodeId, NodeUpdate payload)
        {
            var node = await _context.Nodes.FindAsync(nodeId);
            if (node == null)
                return NotFound(new { detail = "Node không tồn tại." });

            if (payload.X.HasValue) node.X = payload.X.Value;
            if (payload.Y.HasValue) node.Y = payload.Y.Value;
            if (payload.IsLandmark.HasValue) node.IsLandmark = payload.IsLandmark.Value;
            if (payload.Floor.HasValue) node.Floor = payload.Floor.Value;
            if (payload.Meta != null) node.Meta = payload.Meta;

            await _context.SaveChangesAsync();

            return new NodeOut
            {
                Id = node.Id,
                MapId = node.MapId,
                X = node.X,
                Y = node.Y,
                IsLandmark = node.IsLandmark,
                Floor = node.Floor,
                Meta = node.Meta
            };
        }

        // DELETE
        [HttpDelete("{nodeId}")]
        public async Task<IActionResult> DeleteNode(int nodeId)
        {
            var node = await _context.Nodes.FindAsync(nodeId);
            if (node == null)
                return NotFound(new { detail = "Node không tồn tại." });

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1️⃣ Delete aliases
                await _context.Aliases
                    .Where(a => a.NodeId == nodeId)
                    .ExecuteDeleteAsync();

                // 2️⃣ Delete edges where start or end = node
                await _context.Edges
                    .Where(e => e.StartNodeId == nodeId || e.EndNodeId == nodeId)
                    .ExecuteDeleteAsync();

                // 3️⃣ Delete node
                await _context.Nodes
                    .Where(n => n.Id == nodeId)
                    .ExecuteDeleteAsync();

                await transaction.CommitAsync();

                return Ok(new { ok = true });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
