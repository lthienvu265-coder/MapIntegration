using System.Text.Json;
using System.Numerics;

public static class PolylineConverter
{
    public static string ToJson(List<List<double>>? polyline)
    {
        return JsonSerializer.Serialize(polyline ?? new());
    }

    public static List<List<double>> FromJson(string json)
    {
        return JsonSerializer.Deserialize<List<List<double>>>(json)
               ?? new List<List<double>>();
    }

    public static double ComputeLength(List<List<double>> polyline)
    {
        double sum = 0;

        for (int i = 1; i < polyline.Count; i++)
        {
            var dx = polyline[i][0] - polyline[i - 1][0];
            var dy = polyline[i][1] - polyline[i - 1][1];
            sum += Math.Sqrt(dx * dx + dy * dy);
        }

        return sum;
    }
}