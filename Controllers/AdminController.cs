namespace IndoorWayfinder.Api.Controllers
{
    using IndoorWayfinder.Api.Data;
    using IndoorWayfinder.Api.Models.Requests;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly WayfinderContext _context;

        public AdminController(WayfinderContext context)
        {
            _context = context;
        }

        [HttpPost("clear-map")]
        public async Task<IActionResult> ClearMap([FromBody] ClearMapIn payload)
        {
            var map = await _context.Maps
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == payload.MapId);

            if (map == null)
                return NotFound(new { detail = "Map không tồn tại." });

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Get node IDs
                var nodeIds = await _context.Nodes
                    .Where(n => n.MapId == map.Id)
                    .Select(n => n.Id)
                    .ToListAsync();

                int deletedAliases = 0;
                int deletedEdges = 0;
                int deletedNodes = 0;
                bool deletedMap = false;
                bool removedFile = false;

                // 1️⃣ Delete aliases
                if (nodeIds.Any())
                {
                    deletedAliases = await _context.Aliases
                        .Where(a => nodeIds.Contains(a.NodeId))
                        .ExecuteDeleteAsync();
                }

                // 2️⃣ Delete edges
                deletedEdges = await _context.Edges
                    .Where(e => e.MapId == map.Id)
                    .ExecuteDeleteAsync();

                // 3️⃣ Delete nodes
                deletedNodes = await _context.Nodes
                    .Where(n => n.MapId == map.Id)
                    .ExecuteDeleteAsync();

                // 4️⃣ Delete map (optional)
                if (payload.DeleteMap)
                {
                    deletedMap = await _context.Maps
                        .Where(m => m.Id == map.Id)
                        .ExecuteDeleteAsync() > 0;

                    // Remove upload file (optional)
                    if (payload.DeleteUpload &&
                        !string.IsNullOrEmpty(map.ImagePath) &&
                        System.IO.File.Exists(map.ImagePath))
                    {
                        try
                        {
                            System.IO.File.Delete(map.ImagePath);
                            removedFile = true;
                        }
                        catch
                        {
                            removedFile = false;
                        }
                    }
                }

                await transaction.CommitAsync();

                return Ok(new
                {
                    ok = true,
                    deleted = new
                    {
                        aliases = deletedAliases,
                        edges = deletedEdges,
                        nodes = deletedNodes,
                        map = deletedMap,
                        upload_removed = removedFile
                    }
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
