namespace IndoorWayfinder.Api.Models;

public class Edge
{
    public int Id { get; set; }
    public int MapId { get; set; }
    public int StartNodeId { get; set; }
    public int EndNodeId { get; set; }
    public int Floor { get; set; } = 1;
    public string Polyline { get; set; } = string.Empty; // JSON string
    public double Weight { get; set; }
    public bool Bidirectional { get; set; } = true;
    public string? Meta { get; set; }

    public Map Map { get; set; } = null!;
}

