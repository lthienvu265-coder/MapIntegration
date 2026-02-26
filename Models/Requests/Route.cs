namespace IndoorWayfinder.Api.Models.Responses;

public class RouteRequest
{
    public int MapId { get; set; }
    public int? StartId { get; set; }
    public int? EndId { get; set; }
    public string? Q { get; set; }
    public double? Cx { get; set; }
    public double? Cy { get; set; }
}

public class Instruction
{
    public string Kind { get; set; } = "";
    public string Text { get; set; } = "";
    public int AtIndex { get; set; }
    public double DistancePx { get; set; }
}

public class RouteResponse
{
    public List<int> PathNodeIds { get; set; } = new();
    public List<List<double>> Polyline { get; set; } = new();
    public double LengthPx { get; set; }
    public List<Instruction> Instructions { get; set; } = new();
}