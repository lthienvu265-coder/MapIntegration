using System;
using System.Collections.Generic;

namespace IndoorWayfinder.Api.Utils;

public static class GeoUtils
{
    public readonly struct Point
    {
        public readonly double X;
        public readonly double Y;

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public static double SignedTurnAngleScreen(Point p1, Point p2, Point p3)
    {
        var theta1 = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
        var theta2 = Math.Atan2(p3.Y - p2.Y, p3.X - p2.X);
        var d = (theta2 - theta1) * 180.0 / Math.PI;
        while (d <= -180.0) d += 360.0;
        while (d > 180.0) d -= 360.0;
        return d;
    }

    public static double PolylineLength(IReadOnlyList<Point> poly)
    {
        if (poly == null || poly.Count < 2)
            return 0.0;

        double total = 0.0;

        for (int i = 1; i < poly.Count; i++)
        {
            var dx = poly[i].X - poly[i - 1].X;
            var dy = poly[i].Y - poly[i - 1].Y;
            total += Math.Sqrt(dx * dx + dy * dy);
        }

        return total;
    }

    public static double Dist(Point a, Point b)
    {
        double dx = a.X - b.X;
        double dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public static bool AlmostSame(Point a, Point b, double tol = 1.5)
        => Dist(a, b) <= tol;

    public static IList<Point> MergePolysWithTolerance(IEnumerable<IList<Point>> polys, double tol = 1.5)
    {
        var result = new List<Point>();
        foreach (var poly in polys)
        {
            if (poly == null || poly.Count == 0) continue;
            if (result.Count == 0)
            {
                result.AddRange(poly);
                continue;
            }

            var last = result[^1];
            var first = poly[0];
            var lastOfPoly = poly[^1];

            if (AlmostSame(last, first, tol))
            {
                for (int i = 1; i < poly.Count; i++)
                    result.Add(poly[i]);
            }
            else if (AlmostSame(last, lastOfPoly, tol))
            {
                for (int i = poly.Count - 2; i >= 0; i--)
                    result.Add(poly[i]);
            }
            else
            {
                result.Add(first);
                for (int i = 1; i < poly.Count; i++)
                    result.Add(poly[i]);
            }
        }

        return result;
    }

    public static IList<Point> DedupePolyline(IList<Point> poly, double tol = 1.0)
    {
        if (poly == null || poly.Count == 0) return Array.Empty<Point>();
        var result = new List<Point> { poly[0] };
        for (int i = 1; i < poly.Count; i++)
        {
            if (Dist(result[^1], poly[i]) > tol)
                result.Add(poly[i]);
        }
        return result;
    }

    public static IList<Point> OrientPolylineToUv(IList<Point> poly, Point uPos, Point vPos)
    {
        if (poly == null || poly.Count == 0) return poly;
        var d0 = Dist(poly[0], uPos);
        var dN = Dist(poly[^1], uPos);
        if (dN + 1e-6 < d0)
        {
            var rev = new List<Point>(poly.Count);
            for (int i = poly.Count - 1; i >= 0; i--)
                rev.Add(poly[i]);
            return rev;
        }
        return poly;
    }

    public static double HeadingAngleFromPolyline(IList<Point> poly, double minDist = 25.0)
    {
        if (poly == null || poly.Count < 2) return 0.0;
        var p0 = poly[0];
        double acc = 0.0;
        var last = p0;
        double dxSum = 0.0, dySum = 0.0;

        for (int i = 1; i < poly.Count; i++)
        {
            var p = poly[i];
            var dx = p.X - last.X;
            var dy = p.Y - last.Y;
            var seg = Math.Sqrt(dx * dx + dy * dy);
            if (seg <= 1e-6) continue;
            dxSum += dx;
            dySum += dy;
            acc += seg;
            last = p;
            if (acc >= minDist) break;
        }

        var ang = Math.Atan2(dySum, dxSum) * 180.0 / Math.PI;
        while (ang <= -180.0) ang += 360.0;
        while (ang > 180.0) ang -= 360.0;
        return ang;
    }

    public static string InitialHeadingTextFromAngle(double angleDeg)
    {
        var a = angleDeg;
        if (-45.0 <= a && a <= 45.0)
            return "từ trái sang phải";
        if (45.0 < a && a <= 135.0)
            return "từ trên xuống";
        if (-135.0 <= a && a < -45.0)
            return "từ dưới lên";
        return "từ phải sang trái";
    }

    public static double PolylineLength(List<List<double>> poly)
    {
        double total = 0;

        for (int i = 1; i < poly.Count; i++)
        {
            var dx = poly[i][0] - poly[i - 1][0];
            var dy = poly[i][1] - poly[i - 1][1];
            total += Math.Sqrt(dx * dx + dy * dy);
        }

        return total;
    }
}

