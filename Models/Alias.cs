namespace IndoorWayfinder.Api.Models;

public class Alias
{
    public int Id { get; set; }
    public int NodeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NormName { get; set; } = string.Empty;
    public string Lang { get; set; } = "vi";
    public double Weight { get; set; } = 1.0;
    public bool Generated { get; set; } = false;

    public Node Node { get; set; } = null!;
}

