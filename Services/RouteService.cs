using IndoorWayfinder.Api.Data;
using IndoorWayfinder.Api.Models;
using IndoorWayfinder.Api.Models.Responses;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static IndoorWayfinder.Api.Controllers.RouteController;

namespace IndoorWayfinder.Api.Services;

public class RouteService
{
    private readonly WayfinderContext _context;

    public RouteService(WayfinderContext context)
    {
        _context = context;
    }

    // =============================
    // PUBLIC ENTRY
    // =============================
    public async Task<RouteResponse> ComputeRouteAsync(
        int mapId,
        int startId,
        int endId)
    {
        var nodes = await _context.Nodes
            .Where(n => n.MapId == mapId)
            .ToListAsync();

        var edges = await _context.Edges
            .Where(e => e.MapId == mapId)
            .ToListAsync();

        var graph = BuildGraph(nodes, edges);

        if (!graph.ContainsKey(startId) || !graph.ContainsKey(endId))
            throw new Exception("Invalid node");

        var path = Dijkstra(graph, startId, endId);

        var mergedPolyline = MergePathPolyline(path, edges);

        var length = PolylineLength(mergedPolyline);

        var instructions = await BuildInstructions(
            mapId,
            mergedPolyline,
            startId,
            endId);

        return new RouteResponse
        {
            PathNodeIds = path,
            Polyline = mergedPolyline,
            LengthPx = length,
            Instructions = instructions
        };
    }

    // =============================
    // GRAPH
    // =============================
    private Dictionary<int, List<(int to, double weight)>> BuildGraph(
        List<Node> nodes,
        List<Edge> edges)
    {
        var graph = new Dictionary<int, List<(int, double)>>();

        foreach (var n in nodes)
            graph[n.Id] = new List<(int, double)>();

        foreach (var e in edges)
        {
            graph[e.StartNodeId].Add((e.EndNodeId, e.Weight));
            if (e.Bidirectional)
                graph[e.EndNodeId].Add((e.StartNodeId, e.Weight));
        }

        return graph;
    }

    private List<int> Dijkstra(
        Dictionary<int, List<(int to, double weight)>> graph,
        int start,
        int end)
    {
        var dist = new Dictionary<int, double>();
        var prev = new Dictionary<int, int?>();

        var pq = new PriorityQueue<int, double>();

        foreach (var node in graph.Keys)
        {
            dist[node] = double.MaxValue;
            prev[node] = null;
        }

        dist[start] = 0;
        pq.Enqueue(start, 0);

        while (pq.Count > 0)
        {
            var u = pq.Dequeue();

            if (u == end) break;

            foreach (var (v, weight) in graph[u])
            {
                var alt = dist[u] + weight;
                if (alt < dist[v])
                {
                    dist[v] = alt;
                    prev[v] = u;
                    pq.Enqueue(v, alt);
                }
            }
        }

        var path = new List<int>();
        int? curr = end;

        while (curr != null)
        {
            path.Insert(0, curr.Value);
            curr = prev[curr.Value];
        }

        return path;
    }

    // =============================
    // POLYLINE
    // =============================
    private List<List<double>> MergePathPolyline(
        List<int> path,
        List<Edge> edges)
    {
        var result = new List<List<double>>();

        for (int i = 1; i < path.Count; i++)
        {
            var u = path[i - 1];
            var v = path[i];

            var edge = edges.FirstOrDefault(e =>
                (e.StartNodeId == u && e.EndNodeId == v) ||
                (e.Bidirectional && e.StartNodeId == v && e.EndNodeId == u));

            if (edge == null || edge.Polyline == null)
                throw new Exception("Missing polyline");

            var poly = JsonSerializer.Deserialize<List<List<double>>>(edge.Polyline!);

            if (poly == null || poly.Count == 0)
                throw new Exception("Invalid polyline format");

            if (result.Count == 0)
                result.AddRange(poly);
            else
                result.AddRange(poly.Skip(1));
        }

        return result;
    }

    private double PolylineLength(List<List<double>> poly)
    {
        double sum = 0;
        for (int i = 1; i < poly.Count; i++)
        {
            var dx = poly[i][0] - poly[i - 1][0];
            var dy = poly[i][1] - poly[i - 1][1];
            sum += Math.Sqrt(dx * dx + dy * dy);
        }
        return sum;
    }

    // =============================
    // INSTRUCTIONS
    // =============================
    private async Task<List<Instruction>> BuildInstructions(
        int mapId,
        List<List<double>> poly,
        int startId,
        int endId)
    {
        var list = new List<Instruction>();

        var startName = await BestAlias(startId);

        list.Add(new Instruction
        {
            Kind = "start",
            Text = $"Bắt đầu tại {startName}",
            AtIndex = 0
        });

        if (poly.Count < 2)
            return list;

        for (int i = 1; i < poly.Count - 1; i++)
        {
            var angle = SignedTurnAngle(
                poly[i - 1],
                poly[i],
                poly[i + 1]);

            if (Math.Abs(angle) < 25) continue;

            var kind = angle > 0 ? "right" : "left";
            var text = angle > 0 ? "rẽ phải" : "rẽ trái";

            list.Add(new Instruction
            {
                Kind = kind,
                Text = text,
                AtIndex = i
            });
        }

        var endName = await BestAlias(endId);

        list.Add(new Instruction
        {
            Kind = "arrive",
            Text = $"Đã đến {endName}",
            AtIndex = poly.Count - 1
        });

        return list;
    }

    private double SignedTurnAngle(
        List<double> a,
        List<double> b,
        List<double> c)
    {
        var v1x = b[0] - a[0];
        var v1y = b[1] - a[1];

        var v2x = c[0] - b[0];
        var v2y = c[1] - b[1];

        var cross = v1x * v2y - v1y * v2x;
        var dot = v1x * v2x + v1y * v2y;

        return Math.Atan2(cross, dot) * 180 / Math.PI;
    }

    private async Task<string> BestAlias(int nodeId)
    {
        var alias = await _context.Aliases
            .Where(a => a.NodeId == nodeId)
            .OrderByDescending(a => a.Weight)
            .FirstOrDefaultAsync();

        return alias?.Name ?? $"điểm #{nodeId}";
    }
}