using IndoorWayfinder.Api.Data;
using IndoorWayfinder.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using System.Globalization;
using static System.Net.Mime.MediaTypeNames;

[ApiController]
[Route("api/maps")]
public class MapsController : ControllerBase
{
    private readonly WayfinderContext _context;
    private readonly IWebHostEnvironment _env;

    private const string UploadFolder = "data/uploads";
    private const string BaseStatic = "/static/uploads";

    public MapsController(WayfinderContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpPost]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> CreateMap(
        [FromForm] string name,
        [FromForm] IFormFile file)
    {
        if (file == null)
            return BadRequest(new { detail = "File không tồn tại." });

        var allowedTypes = new[]
        {
        "image/png",
        "image/jpeg",
        "image/jpg",
        "image/webp"
    };

        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest(new { detail = "File phải là ảnh (png/jpg/webp)." });

        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), UploadFolder);
        Directory.CreateDirectory(uploadPath);

        var ts = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (string.IsNullOrWhiteSpace(ext))
            ext = ".png";

        var filename = $"map_{ts}{ext}";
        var diskPath = Path.Combine(uploadPath, filename);

        await using (var stream = new FileStream(diskPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        int width, height;
        try
        {
            using var image = await SixLabors.ImageSharp.Image.LoadAsync(diskPath);
            width = image.Width;
            height = image.Height;
        }
        catch
        {
            System.IO.File.Delete(diskPath);
            return BadRequest(new { detail = "Không đọc được ảnh." });
        }

        var map = new Map
        {
            Name = name,
            ImagePath = $"static/uploads/{filename}", // ✅ FIX HERE
            Width = width,
            Height = height,
            CreatedAt = DateTime.UtcNow
        };

        _context.Maps.Add(map);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = map.Id,
            name = map.Name,
            image_path = map.ImagePath,
            image_url = $"{BaseStatic}/static/uploads/{filename}", // ✅ PUBLIC URL
            width = map.Width,
            height = map.Height,
            created_at = map.CreatedAt.ToString("o", CultureInfo.InvariantCulture)
        });
    }

    // GET MAP BY ID
    [HttpGet("{mapId}")]
    public async Task<IActionResult> GetMap(int mapId)
    {
        var map = await _context.Maps.FindAsync(mapId);
        if (map == null)
            return NotFound(new { detail = "Map không tồn tại." });

        return Ok(new
        {
            id = map.Id,
            name = map.Name,
            image_path = map.ImagePath,
            image_url = $"{BaseStatic}/{Path.GetFileName(map.ImagePath)}",
            width = map.Width,
            height = map.Height,
            created_at = map.CreatedAt.ToString("o")
        });
    }

    // LIST MAPS
    [HttpGet]
    public async Task<IActionResult> ListMaps()
    {
        var maps = await _context.Maps
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return Ok(new
        {
            items = maps.Select(m => new
            {
                id = m.Id,
                name = m.Name,
                image_path = m.ImagePath,
                image_url = $"{BaseStatic}/{Path.GetFileName(m.ImagePath)}",
                width = m.Width,
                height = m.Height,
                created_at = m.CreatedAt.ToString("o")
            })
        });
    }
}