using System;
using System.Collections.Generic;

namespace IndoorWayfinder.Api.Models;

public class Map
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Node> Nodes { get; set; } = new List<Node>();
    public ICollection<Edge> Edges { get; set; } = new List<Edge>();
}

