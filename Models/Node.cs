using System.Collections.Generic;

namespace IndoorWayfinder.Api.Models;

public class Node
{
    public int Id { get; set; }
    public int MapId { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public bool IsLandmark { get; set; } = false;
    public int Floor { get; set; } = 1;
    public string? Meta { get; set; }

    public Map Map { get; set; } = null!;
    public ICollection<Alias> Aliases { get; set; } = new List<Alias>();
}

