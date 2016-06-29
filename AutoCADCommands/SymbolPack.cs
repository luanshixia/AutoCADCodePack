using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace AutoCADCommands
{
    /// <summary>
    /// 符号包
    /// </summary>
    public class SymbolPack
    {
        public static ObjectId Arrow(Point3d end, Point3d head, ArrowOption opt)
        {
            ObjectId body = Draw.Line(end, head);
            body.QOpenForWrite<Entity>(x => x.ColorIndex = opt.ColorIndex);
            ObjectId arrow = Modify.Group(new ObjectId[] { body });

            if (opt.HeadStyle == ArrowHeadStyle.ClockHand)
            {
                Vector3d dir = head - end;
                dir = dir.GetNormal();
                Vector3d nor = new Vector3d(dir.Y, -dir.X, 0);
                Point3d headBase = head - opt.HeadLength * dir;
                Point3d left = headBase - opt.HeadWidth / 2 * nor;
                Point3d right = headBase + opt.HeadWidth / 2 * nor;
                ObjectId l = Draw.Line(head, left);
                ObjectId r = Draw.Line(head, right);
                l.QOpenForWrite<Entity>(x => x.ColorIndex = opt.ColorIndex);
                r.QOpenForWrite<Entity>(x => x.ColorIndex = opt.ColorIndex);
                Modify.AppendToGroup(arrow, new ObjectId[] { l, r });
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
            var lines = Algorithms.Range(step, length, step)
                .Select(pos => NoDraw.Pline(line1.GetPointAtDistX(pos), line2.GetPointAtDistX(pos))).ToList();
            lines.Add(line1);
            lines.Add(line2);
            return lines.ToArray().AddToCurrentSpace();
        }

        public static ObjectId[] LineBundle(Polyline alignment, LineBundleDefinition[] bundle)
        {
            var ids = new List<ObjectId>();
            bundle.ToList().ForEach(b =>
            {
                var poly = alignment.GetOffsetCurves(b.Offset)[0] as Polyline;
                poly.ConstantWidth = b.Width;
                if (b.DashArray == null || b.DashArray.Length == 0)
                {
                    ids.Add(poly.AddToCurrentSpace());
                }
                else
                {
                    ids.AddRange(DashedLine(poly, b.DashArray));
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

    public class LineBundleDefinition
    {
        public double Width;
        public double[] DashArray;
        public double Offset;
    }

    public class ArrowOption
    {
        public byte ColorIndex = 0;
        public double HeadLength = 5;
        public double HeadWidth = 5;
        public ArrowHeadStyle HeadStyle = ArrowHeadStyle.ClockHand;
    }

    public enum ArrowHeadStyle
    {
        RosePink,
        ClockHand,
        BlackTriangle,
        WhiteTriangle,
        Diamond
    }

    /// <summary>
    /// 函数图象绘制器
    /// </summary>
    public class GraphPlotter
    {
        private GraphOption _option;
        private Interv _xRange;
        private Interv _yRange; // 原始，不乘ratio
        private List<ObjectId> _entIds = new List<ObjectId>();
        private TupleList<IEnumerable<Point2d>, int> _curves = new TupleList<IEnumerable<Point2d>, int>();

        public GraphPlotter(GraphOption opt)
        {
            _option = opt;
            _xRange = opt.xRange;
            _yRange = opt.yRange;
        }

        public GraphPlotter()
        {
            _option = new GraphOption();
        }

        public double RealRatio 
        { 
            get 
            { 
                return _option.yRatio * (_xRange.Length * _option.xRedundanceFactor) / (_yRange.Length * _option.yRedundanceFactor); 
            } 
        }

        public void Plot(IEnumerable<Point2d> points, int color = -1)
        {
            _curves.Add(points.OrderBy(x => x.X).ToArray(), color);

            Interv xRange = new Interv(points.Min(x => x.X), points.Max(x => x.X));
            Interv yRange = new Interv(points.Min(x => x.Y), points.Max(x => x.Y));
            if (_xRange == null)
            {
                _xRange = xRange;
            }
            else
            {
                _xRange = _xRange.AddInterval(xRange);
            }
            if (_yRange == null)
            {
                _yRange = yRange;
            }
            else
            {
                _yRange = _yRange.AddInterval(yRange);
            }
        }

        public void Plot(Func<double, double> function, Interv xRange, int color = -1)
        {
            double delta = xRange.Length / _option.SampleCount;
            var points = Enumerable.Range(0, _option.SampleCount + 1).Select(x =>
            {
                double xx = xRange.Start + x * delta;
                return new Point2d(xx, function(xx));
            }).ToArray();
            Plot(points, color);
        }

        public ObjectId GetGraphBlock()
        {
            if (_xRange == null || _yRange == null)
            {
                throw new System.Exception("未指定绘制内容");
            }

            // 控制最小范围
            if (_xRange.Length < Consts.Epsilon)
            {
                _xRange = new Interv(_xRange.Start - Consts.Epsilon, _xRange.End + Consts.Epsilon);
            }
            if (_yRange.Length < Consts.Epsilon)
            {
                _yRange = new Interv(_yRange.Start - Consts.Epsilon, _yRange.End + Consts.Epsilon);
            }

            // 获取刻度值
            double[] xStops = GetDivStops(_option.xDelta, _xRange, _option.xRedundanceFactor);
            double[] yStops = GetDivStops(_option.yDelta, _yRange, _option.yRedundanceFactor);

            // 刻度网格
            List<ObjectId> gridLines = new List<ObjectId>();
            foreach (var xStop in xStops)
            {
                gridLines.Add(Draw.Line(new Point3d(xStop, RealRatio * yStops.First(), 0), new Point3d(xStop, RealRatio * yStops.Last(), 0)));
            }
            foreach (var yStop in yStops)
            {
                gridLines.Add(Draw.Line(new Point3d(xStops.First(), RealRatio * yStop, 0), new Point3d(xStops.Last(), RealRatio * yStop, 0)));
            }
            gridLines.QForEach<Entity>(x => x.ColorIndex = _option.GridColor);
            _entIds.AddRange(gridLines);

            // 刻度标记
            List<ObjectId> labels = new List<ObjectId>();
            double txtHeight = _xRange.Length / 50;
            foreach (var xStop in xStops)
            {
                labels.Add(Draw.MText(xStop.ToString("0.###"), txtHeight, new Point3d(xStop, RealRatio * yStops.First() - 2 * txtHeight, 0), 0, true));
            }
            foreach (var yStop in yStops)
            {
                labels.Add(Draw.MText(yStop.ToString("0.###"), txtHeight, new Point3d(xStops.First() - 2 * txtHeight, RealRatio * yStop, 0), 0, true));
            }
            labels.QForEach<Entity>(x => x.ColorIndex = _option.LabelColor);
            _entIds.AddRange(labels);

            // 曲线
            foreach (var curve in _curves)
            {
                ObjectId plineId = Draw.Pline(curve.Item1.OrderBy(x => x.X).Select(x => new Point3d(x.X, RealRatio * x.Y, 0)));
                int color1 = curve.Item2 == -1 ? _option.CurveColor : curve.Item2;
                plineId.QOpenForWrite<Entity>(x => x.ColorIndex = color1);
                _entIds.Add(plineId);
            }

            // 返回块
            ObjectId result = Draw.Block(_entIds, "tjGraph" + LogManager.GetTimeBasedName(), _entIds.GetCenter());
            _entIds.QForEach(x => x.Erase());
            _entIds.Clear();
            return result;
        }

        private double[] GetDivStops(int divs, Interv range)
        {
            // 算法有问题。从数学上考虑，对应一个不定方程，很复杂。

            // 每个刻度点都是刻度间隔的整数倍。问题转化为求divs个连续整数。
            double delta = Math.Ceiling(range.Length / divs);
            int nDigit = (int)Math.Log10(range.Length / divs); //delta.ToString().Length;
            double scale = Math.Pow(10, nDigit - 1);
            delta = Math.Ceiling(delta / scale) * scale;
            int mid = (int)Math.Floor((range.Start + range.End) / 2 / delta);
            int start = mid - divs / 2;
            return Enumerable.Range(start, divs + 1).Select(x => x * delta).ToArray();
        }

        private double[] GetDivStops(double delta, Interv range, double redundanceFactor = 1)
        {
            List<double> result = new List<double>();
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
    }
}
