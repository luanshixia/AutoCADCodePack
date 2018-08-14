using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dreambuild.AutoCAD
{
    /// <summary>
    /// The symbol pack.
    /// </summary>
    public static class SymbolPack
    {
        public static ObjectId Arrow(Point3d end, Point3d head, ArrowOption opt)
        {
            var body = Draw.Line(end, head);
            body.QOpenForWrite<Entity>(line => line.ColorIndex = opt.ColorIndex);
            var arrow = Modify.Group(new[] { body });

            if (opt.HeadStyle == ArrowHeadStyle.ClockHand)
            {
                var dir = head - end;
                dir = dir.GetNormal();
                var nor = new Vector3d(dir.Y, -dir.X, 0);
                var headBase = head - opt.HeadLength * dir;
                var left = headBase - opt.HeadWidth / 2 * nor;
                var right = headBase + opt.HeadWidth / 2 * nor;
                var leftLine = Draw.Line(head, left);
                var rightLine = Draw.Line(head, right);
                leftLine.QOpenForWrite<Entity>(line => line.ColorIndex = opt.ColorIndex);
                rightLine.QOpenForWrite<Entity>(line => line.ColorIndex = opt.ColorIndex);
                Modify.AppendToGroup(arrow, new[] { leftLine, rightLine });
            }

            return arrow;
        }

        public static ObjectId BridgeEar(Point3d pos, Vector3d dir, bool right, double size)
        {
            var angle = right ? -Math.PI / 4 : Math.PI / 4;
            var earDir = dir.GetNormal().RotateBy(angle, Vector3d.ZAxis);
            var start = pos;
            var end = start + size * earDir;
            return Draw.Pline(start, end);
        }

        public static Tuple<ObjectId, ObjectId> BridgeEarFor(Curve cv, bool right, double size)
        {
            var startDir = -cv.GetFirstDerivative(cv.StartParam);
            var endDir = cv.GetFirstDerivative(cv.EndParam);
            var startRight = right ? false : true;
            var endRight = right ? true : false;
            var startEar = BridgeEar(cv.StartPoint, startDir, startRight, size);
            var endEar = BridgeEar(cv.EndPoint, endDir, endRight, size);
            return Tuple.Create(startEar, endEar);
        }

        public static ObjectId[] Stairs(Point3d p1, Point3d p2, Point3d p3, double step)
        {
            var line1 = NoDraw.Pline(p1, p2);
            var width = line1.GetDistToPoint(p3, true);
            var line21 = line1.GetOffsetCurves(width)[0] as Polyline;
            var line22 = line1.GetOffsetCurves(-width)[0] as Polyline;
            var line2 = line21.GetDistToPoint(p3) < line22.GetDistToPoint(p3) ? line21 : line22;
            var length = line1.Length;
            var lines = Algorithms
                .Range(step, length, step)
                .Select(pos => NoDraw.Pline(line1.GetPointAtDistX(pos), line2.GetPointAtDistX(pos)))
                .ToList();
            lines.Add(line1);
            lines.Add(line2);
            return lines.ToArray().AddToCurrentSpace();
        }

        public static ObjectId[] LineBundle(Polyline alignment, LineBundleElement[] bundle)
        {
            var ids = new List<ObjectId>();
            bundle.ForEach(bundleElement =>
            {
                var poly = alignment.GetOffsetCurves(bundleElement.Offset)[0] as Polyline;
                poly.ConstantWidth = bundleElement.Width;
                if (bundleElement.DashArray == null || bundleElement.DashArray.Length == 0)
                {
                    ids.Add(poly.AddToCurrentSpace());
                }
                else
                {
                    ids.AddRange(DashedLine(poly, bundleElement.DashArray));
                }
            });
            return ids.ToArray();
        }

        public static ObjectId[] DashedLine(Curve cv, double[] dashArray)
        {
            var cvs = new List<Curve>();
            double dist = 0;
            int i = 0;
            bool dash = false;
            while (dist < cv.GetDistAtParam(cv.EndParam))
            {
                double length = dashArray[i];
                if (!dash)
                {
                    cvs.Add(cv.GetSubCurve(new Interv(dist, dist + length)));
                }
                i = (i + 1) % dashArray.Length;
                dist += length;
                dash = !dash;
            }
            return cvs.ToArray().AddToCurrentSpace();
        }
    }

    /// <summary>
    /// The line bundle element.
    /// </summary>
    public class LineBundleElement
    {
        public double Width;
        public double[] DashArray;
        public double Offset;
    }

    /// <summary>
    /// The arrow option.
    /// </summary>
    public class ArrowOption
    {
        public byte ColorIndex = 0;
        public double HeadLength = 5;
        public double HeadWidth = 5;
        public ArrowHeadStyle HeadStyle = ArrowHeadStyle.ClockHand;
    }

    /// <summary>
    /// The arrow head style.
    /// </summary>
    public enum ArrowHeadStyle
    {
        RosePink,
        ClockHand,
        BlackTriangle,
        WhiteTriangle,
        Diamond
    }

    /// <summary>
    /// The (math function) graph plotter.
    /// </summary>
    public class GraphPlotter
    {
        private GraphOption Option { get; }

        private Interv XRange { get; set; }

        private Interv YRange { get; set; } // Original, without ratio applied.

        private List<(IEnumerable<Point2d>, int)> Curves { get; } = new List<(IEnumerable<Point2d>, int)>();

        private double RealRatio => this.Option.yRatio * (this.XRange.Length * this.Option.xRedundanceFactor) / (this.YRange.Length * this.Option.yRedundanceFactor);

        public GraphPlotter()
        {
            this.Option = new GraphOption();
        }

        public GraphPlotter(GraphOption opt)
        {
            this.Option = opt;
            this.XRange = opt.xRange;
            this.YRange = opt.yRange;
        }

        public void Plot(IEnumerable<Point2d> points, int color = -1)
        {
            this.Curves.Add((points.OrderBy(point => point.X).ToArray(), color));

            var xRange = new Interv(points.Min(point => point.X), points.Max(point => point.X));
            var yRange = new Interv(points.Min(point => point.Y), points.Max(point => point.Y));
            if (this.XRange == null)
            {
                this.XRange = xRange;
            }
            else
            {
                this.XRange = this.XRange.AddInterval(xRange);
            }
            if (this.YRange == null)
            {
                this.YRange = yRange;
            }
            else
            {
                this.YRange = this.YRange.AddInterval(yRange);
            }
        }

        public void Plot(Func<double, double> function, Interv xRange, int color = -1)
        {
            double delta = xRange.Length / this.Option.SampleCount;
            var points = Enumerable
                .Range(0, this.Option.SampleCount + 1)
                .Select(index =>
                {
                    double coord = xRange.Start + index * delta;
                    return new Point2d(coord, function(coord));
                })
                .ToArray();

            this.Plot(points, color);
        }

        public ObjectId GetGraphBlock()
        {
            if (this.XRange == null || this.YRange == null)
            {
                throw new Exception("Plot undefined.");
            }

            var entIds = new List<ObjectId>();

            // Ranges cannot be less than epsilon.
            if (this.XRange.Length < Consts.Epsilon)
            {
                this.XRange = new Interv(this.XRange.Start - Consts.Epsilon, this.XRange.End + Consts.Epsilon);
            }
            if (this.YRange.Length < Consts.Epsilon)
            {
                this.YRange = new Interv(this.YRange.Start - Consts.Epsilon, this.YRange.End + Consts.Epsilon);
            }

            // Stops
            double[] xStops = GraphPlotter.GetDivStops(this.Option.xDelta, this.XRange, this.Option.xRedundanceFactor);
            double[] yStops = GraphPlotter.GetDivStops(this.Option.yDelta, this.YRange, this.Option.yRedundanceFactor);

            // Grid lines
            var gridLines = new List<ObjectId>();
            foreach (var xStop in xStops)
            {
                gridLines.Add(Draw.Line(new Point3d(xStop, this.RealRatio * yStops.First(), 0), new Point3d(xStop, this.RealRatio * yStops.Last(), 0)));
            }
            foreach (var yStop in yStops)
            {
                gridLines.Add(Draw.Line(new Point3d(xStops.First(), this.RealRatio * yStop, 0), new Point3d(xStops.Last(), this.RealRatio * yStop, 0)));
            }
            gridLines.QForEach<Entity>(line => line.ColorIndex = this.Option.GridColor);
            entIds.AddRange(gridLines);

            // Labels
            var labels = new List<ObjectId>();
            double txtHeight = this.XRange.Length / 50;
            foreach (var xStop in xStops)
            {
                labels.Add(Draw.MText(xStop.ToString("0.###"), txtHeight, new Point3d(xStop, this.RealRatio * yStops.First() - 2 * txtHeight, 0), 0, true));
            }
            foreach (var yStop in yStops)
            {
                labels.Add(Draw.MText(yStop.ToString("0.###"), txtHeight, new Point3d(xStops.First() - 2 * txtHeight, this.RealRatio * yStop, 0), 0, true));
            }
            labels.QForEach<Entity>(mt => mt.ColorIndex = this.Option.LabelColor);
            entIds.AddRange(labels);

            // Curves
            foreach (var curve in this.Curves)
            {
                var plineId = Draw.Pline(curve.Item1.OrderBy(point => point.X).Select(point => new Point3d(point.X, this.RealRatio * point.Y, 0)));
                int color1 = curve.Item2 == -1 ? this.Option.CurveColor : curve.Item2;
                plineId.QOpenForWrite<Entity>(pline => pline.ColorIndex = color1);
                entIds.Add(plineId);
            }

            // Returns a block.
            var result = Draw.Block(entIds, "tjGraph" + LogManager.GetTimeBasedName(), entIds.GetCenter());
            entIds.QForEach(entity => entity.Erase());
            entIds.Clear();
            return result;
        }

        private static double[] GetDivStops(double delta, Interv range, double redundanceFactor = 1)
        {
            var result = new List<double>();
            double redundance = (redundanceFactor - 1) / 2 * range.Length;
            result.Add(range.Start - redundance);
            double start = Math.Ceiling((range.Start - redundance) / delta) * delta;
            for (double t = start; t < range.End + redundance; t += delta)
            {
                result.Add(t);
            }
            result.Add(range.End + redundance);
            return result.ToArray();
        }
    }

    /// <summary>
    /// The graph option.
    /// </summary>
    public class GraphOption
    {
        //public int xDivs = 5;
        //public int yDivs = 5;
        public double xDelta = 10;
        public double yDelta = 10;
        public double yRatio = 1;
        public double xRedundanceFactor = 1;
        public double yRedundanceFactor = 1.5;
        public Interv xRange = null;
        public Interv yRange = null;
        public int SampleCount = 100;
        public byte GridColor = 0;
        public byte CurveColor = 2;
        public byte LabelColor = 1;
    }

    public class TablePlotter
    {
        // TODO: implement this.
    }
}
