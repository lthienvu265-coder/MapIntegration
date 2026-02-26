namespace IndoorWayfinder.Api.Controllers
{
    using FuzzySharp;
    using IndoorWayfinder.Api.Data;
    using IndoorWayfinder.Api.Models;
    using IndoorWayfinder.Api.Models.Requests;
    using IndoorWayfinder.Api.Utils;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [ApiController]
    [Route("api/aliases")]
    public class AliasesController : ControllerBase
    {
        private readonly WayfinderContext _context;

        public AliasesController(WayfinderContext context)
        {
            _context = context;
        }

        // CREATE
        [HttpPost]
        public async Task<ActionResult<AliasOut>> CreateAlias(AliasIn payload)
        {
            var node = await _context.Nodes.FindAsync(payload.Node_id);
            if (node == null)
                return NotFound(new { detail = "Node không tồn tại." });

            var alias = new Alias
            {
                NodeId = payload.Node_id,
                Name = payload.Name,
                NormName = TextUtils.NormalizeName(payload.Name),
                Lang = payload.Lang,
                Weight = payload.Weight,
                Generated = payload.Generated
            };

            _context.Aliases.Add(alias);
            await _context.SaveChangesAsync();

            return new AliasOut
            {
                Id = alias.Id,
                Node_id = alias.NodeId,
                Name = alias.Name,
                NormName = alias.NormName,
                Lang = alias.Lang,
                Weight = alias.Weight,
                Generated = alias.Generated
            };
        }

        // LIST
        [HttpGet]
        public async Task<ActionResult<List<AliasOut>>> ListAliases([FromQuery] int? nodeId)
        {
            var query = _context.Aliases.AsQueryable();

            if (nodeId.HasValue)
                query = query.Where(a => a.NodeId == nodeId.Value);

            var items = await query.ToListAsync();

            return items.Select(a => new AliasOut
            {
                Id = a.Id,
                Node_id = a.NodeId,
                Name = a.Name,
                NormName = a.NormName,
                Lang = a.Lang,
                Weight = a.Weight,
                Generated = a.Generated
            }).ToList();
        }

        // DELETE
        [HttpDelete("{aliasId}")]
        public async Task<IActionResult> DeleteAlias(int aliasId)
        {
            var alias = await _context.Aliases.FindAsync(aliasId);
            if (alias == null)
                return NotFound(new { detail = "Alias không tồn tại." });

            _context.Aliases.Remove(alias);
            await _context.SaveChangesAsync();

            return Ok(new { ok = true });
        }

        // SEARCH
        [HttpGet("search")]
        public async Task<ActionResult<List<AliasSearchOut>>> SearchAlias(
            [FromQuery] string q,
            [FromQuery] int limit = 5)
        {
            if (string.IsNullOrWhiteSpace(q))
                return new List<AliasSearchOut>();

            var normQ = TextUtils.NormalizeName(q);
            if (string.IsNullOrWhiteSpace(normQ))
                return new List<AliasSearchOut>();

            var items = await _context.Aliases.ToListAsync();
            if (!items.Any())
                return new List<AliasSearchOut>();

            var results = items
                .Select(a => new
                {
                    Alias = a,
                    Score = Fuzz.TokenSetRatio(normQ, TextUtils.NormalizeName(a.Name))
                })
                .OrderByDescending(x => x.Score)
                .Take(limit)
                .ToList();

            return results.Select(r => new AliasSearchOut
            {
                NodeId = r.Alias.NodeId,
                AliasId = r.Alias.Id,
                Name = r.Alias.Name,
                Score = r.Score
            }).ToList();
        }
    }
}
