namespace IndoorWayfinder.Api.Models.Responses;

public class RouteRequest
{
    public int map_id { get; set; }
    public int? start_id { get; set; }
    public int? end_id { get; set; }
    public string? Q { get; set; }
    public double? Cx { get; set; }
    public double? Cy { get; set; }
}

public class Instruction
{
    public string Kind { get; set; } = "";
    public string Text { get; set; } = "";
    public int at_index { get; set; }
    public double distance_px { get; set; }
}

public class RouteResponse
{
    public List<int> path_node_ids { get; set; } = new();
    public List<List<double>> Polyline { get; set; } = new();
    public double length_px { get; set; }
    public List<Instruction> Instructions { get; set; } = new();
}