namespace IndoorWayfinder.Api.Models.Requests
{
    public class NodeIn
    {
        public int MapId { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public bool IsLandmark { get; set; } = false;
        public int Floor { get; set; }
        public string? Meta { get; set; }
    }

    public class NodeOut
    {
        public int Id { get; set; }
        public int MapId { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public bool IsLandmark { get; set; }
        public int Floor { get; set; }
        public string? Meta { get; set; }
    }

    public class NodeUpdate
    {
        public double? X { get; set; }
        public double? Y { get; set; }
        public bool? IsLandmark { get; set; }
        public int? Floor { get; set; }
        public string? Meta { get; set; }
    }
}
