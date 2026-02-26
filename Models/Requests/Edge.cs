namespace IndoorWayfinder.Api.Models.Requests
{
    public class EdgeIn
    {
        public int map_id { get; set; }
        public int start_node_id { get; set; }
        public int end_node_id { get; set; }
        public int? Floor { get; set; }
        public List<List<double>>? Polyline { get; set; }
        public bool Bidirectional { get; set; } = true;
        public string? Meta { get; set; }
    }

    public class EdgeOut
    {
        public int Id { get; set; }
        public int map_id { get; set; }
        public int start_node_id { get; set; }
        public int end_node_id { get; set; }
        public int Floor { get; set; }
        public List<List<double>> Polyline { get; set; } = new();
        public double Weight { get; set; }
        public bool Bidirectional { get; set; }
        public string? Meta { get; set; }
    }

    public class EdgeUpdate
    {
        public List<List<double>>? Polyline { get; set; }
        public bool? Bidirectional { get; set; }
        public string? Meta { get; set; }
    }
}
