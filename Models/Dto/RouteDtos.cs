using System.Collections.Generic;

namespace IndoorWayfinder.Api.Models.Dto;

public class RouteRequest
{
    public int MapId { get; set; }
    public int? StartId { get; set; }
    public int? EndId { get; set; }
    public string? Q { get; set; }
    public double? Cx { get; set; }
    public double? Cy { get; set; }
}

public class InstructionDto
{
    public string Kind { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int at_index { get; set; }
    public double distance_px { get; set; }
}

public class RouteResponse
{
    public IList<int> path_node_ids { get; set; } = new List<int>();
    public IList<IList<double>> Polyline { get; set; } = new List<IList<double>>();
    public double length_px { get; set; }
    public IList<InstructionDto> Instructions { get; set; } = new List<InstructionDto>();
}

