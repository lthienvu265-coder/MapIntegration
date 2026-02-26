namespace IndoorWayfinder.Api.Models.Requests
{
    public class AliasIn
    {
        public int NodeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Lang { get; set; } = "vi";
        public double Weight { get; set; } = 1.0;
        public bool Generated { get; set; } = false;
    }

    public class AliasOut
    {
        public int Id { get; set; }
        public int NodeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NormName { get; set; } = string.Empty;
        public string Lang { get; set; } = string.Empty;
        public double Weight { get; set; }
        public bool Generated { get; set; }
    }

    public class AliasSearchOut
    {
        public int NodeId { get; set; }
        public int AliasId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Score { get; set; }
    }
}
