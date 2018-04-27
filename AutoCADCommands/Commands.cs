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
    /// The "Draw" module: directly draw entities (with AutoCAD-command-like functions)
    /// </summary>
    public static class Draw
    {
        #region point

        /// <summary>
        /// 绘制点
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static ObjectId Point(Point3d p)
        {
            return Draw.AddToCurrentSpace(new DBPoint(p));
        }

        /// <summary>
        /// 绘制定数等分点
        /// </summary>
        /// <param name="cv"></param>
        /// <param name="n"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ObjectId[] Divide(Curve cv, int n, Entity obj)
        {
            double start = cv.GetDistAtParam(cv.StartParam);
            double end = cv.GetDistAtParam(cv.EndParam);
            double delta = (end - start) / n;
            var inserts = Enumerable.Range(1, n - 1).Select(x => start + x * delta).ToList();
            var objs = inserts.Select(x =>
            {
                var objNew = obj.Clone() as Entity;
                if (objNew is DBPoint)
                {
                    (objNew as DBPoint).Position = cv.GetPointAtParam(x);
                }
                else if (objNew is BlockReference)
                {
                    (objNew as BlockReference).Position = cv.GetPointAtParam(x);
                }
                return objNew;
            }).ToArray();
            return objs.AddToCurrentSpace();
        }

        /// <summary>
        /// 绘制定距等分点
        /// </summary>
        /// <param name="cv"></param>
        /// <param name="length"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ObjectId[] Measure(Curve cv, double length, Entity obj)
        {
            double start = cv.GetDistAtParam(cv.StartParam);
            double end = cv.GetDistAtParam(cv.EndParam);
            double m = (end - start) / length;
            int n = Convert.ToInt32(m);
            var inserts = Enumerable.Range(1, n).Select(x => start + x * length).ToList();
            var objs = inserts.Select(x =>
            {
                var objNew = obj.Clone() as Entity;
                if (objNew is DBPoint)
                {
                    (objNew as DBPoint).Position = cv.GetPointAtParam(x);
                }
                else if (objNew is BlockReference)
                {
                    (objNew as BlockReference).Position = cv.GetPointAtParam(x);
                }
                return objNew;
            }).ToArray();
            return objs.AddToCurrentSpace();
        }

        #endregion

        #region line

        /// <summary>
        /// 绘制直线
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static ObjectId Line(Point3d p1, Point3d p2)
        {
            return Draw.AddToCurrentSpace(new Line(p1, p2));
        }

        /// <summary>
        /// 绘制直线
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static ObjectId[] Line(params Point3d[] points)
        {
            return Enumerable.Range(0, points.Length - 1).Select(x => Line(points[x], points[x + 1])).ToArray();
        }

        /// <summary>
        /// 绘制圆弧：三点（调用ArcFromGeoArc）
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public static ObjectId Arc3P(Point3d p1, Point3d p2, Point3d p3)
        {
            CircularArc3d arc = new CircularArc3d(p1, p2, p3);
            Vector3d dir1 = p1 - arc.Center;
            double startangle = dir1.GetAngleTo(Vector3d.XAxis);
            if (dir1.Y < 0)
            {
                startangle = Math.PI * 2 - startangle;
            }
            Vector3d dir2 = p3 - arc.Center;
            double endangle = dir2.GetAngleTo(Vector3d.XAxis);
            if (dir2.Y < 0)
            {
                endangle = Math.PI * 2 - endangle;
            }
            Arc dbArc = new Arc(arc.Center, arc.Radius, startangle, endangle);
            double angle = arc.EndAngle;
            if (dbArc.GetDistToPoint(p2) > Consts.Epsilon)
            {
                angle = -angle;
            }
            return ArcSCA(p1, arc.Center, angle);
        }

        //public static void ArcSCE(Point3d start, Point3d center, Point3d end)
        //{
        //}

        /// <summary>
        /// 绘制圆弧：起点圆心角度（调用ArcFromGeoArc）
        /// </summary>
        /// <param name="start"></param>
        /// <param name="center"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static ObjectId ArcSCA(Point3d start, Point3d center, double angle)
        {
            double radius = center.DistanceTo(start);
            Vector3d dir1 = start - center;
            double startangle = dir1.GetAngleTo(Vector3d.XAxis);
            if (dir1.Y < 0)
            {
                startangle = Math.PI * 2 - startangle;
            }
            double endangle;
            if (angle > 0)
            {
                endangle = startangle + angle;
            }
            else
            {
                double a = startangle;
                startangle = startangle + angle;
                endangle = a;
            }
            CircularArc3d arc = new CircularArc3d(center, Vector3d.ZAxis, Vector3d.XAxis, radius, startangle, endangle);
            return ArcFromGeoArc(arc);
        }

        //public static void ArcSCL(Point3d start, Point3d center, double length)
        //{
        //}

        //public static void ArcSEA(Point3d start, Point3d end, double angle)
        //{
        //}

        //public static void ArcSED(Point3d start, Point3d end, Vector3d dir)
        //{
        //}

        //public static void ArcSER(Point3d start, Point3d end, double radius)
        //{
        //}

        /// <summary>
        /// 绘制圆弧：从GeoArc
        /// </summary>
        /// <param name="arc"></param>
        /// <returns></returns>
        public static ObjectId ArcFromGeoArc(CircularArc3d arc)
        {
            return Draw.AddToCurrentSpace(new Arc(arc.Center, arc.Radius, arc.StartAngle, arc.EndAngle));
        }

        /// <summary>
        /// 创建多段线 newly 20140717
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static ObjectId Pline(params Point3d[] points)
        {
            return Pline(points.ToList());
        }

        /// <summary>
        /// 绘制多段线
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static ObjectId Pline(IEnumerable<Point3d> points)
        {
            return Pline(points, 0);
        }

        /// <summary>
        /// 绘制多段线
        /// </summary>
        /// <param name="points"></param>
        /// <param name="globalWidth"></param>
        /// <returns></returns>
        public static ObjectId Pline(IEnumerable<Point3d> points, double globalWidth)
        {
            TupleList<Point3d, double> pairs = new TupleList<Point3d, double>();
            points.ToList().ForEach(x => pairs.Add(x, 0));
            return Pline(pairs, globalWidth);
        }

        /// <summary>
        /// 绘制多段线
        /// </summary>
        /// <param name="pointBulgePairs"></param>
        /// <param name="globalWidth"></param>
        /// <returns></returns>
        public static ObjectId Pline(TupleList<Point3d, double> pointBulgePairs, double globalWidth)
        {
            Polyline poly = new Polyline();
            Enumerable.Range(0, pointBulgePairs.Count).ToList().ForEach(x =>
                poly.AddVertexAt(x, new Point2d(pointBulgePairs[x].Item1.X, pointBulgePairs[x].Item1.Y), pointBulgePairs[x].Item2, globalWidth, globalWidth)
                );
            return Draw.AddToCurrentSpace(poly);
        }

        /// <summary>
        /// 绘制拟合点样条线
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static ObjectId SplineFit(Point3d[] points)
        {
            Point3dCollection pts = new Point3dCollection(points);
            Spline spline = new Spline(pts, 3, Consts.Epsilon);
            return Draw.AddToCurrentSpace(spline);
        }

        /// <summary>
        /// 绘制控制点样条线
        /// </summary>
        /// <param name="points"></param>
        /// <param name="closed"></param>
        /// <returns></returns>
        public static ObjectId SplineCV(Point3d[] points, bool closed = false)
        {
            Point3dCollection pts = new Point3dCollection(points);
            DoubleCollection knots;
            DoubleCollection weights;
            if (!closed)
            {
                var knots1 = Enumerable.Range(0, points.Length - 2).Select(x => (double)x).ToList();
                knots1.Insert(0, 0);
                knots1.Insert(0, 0);
                knots1.Insert(0, 0);
                knots1.Add(points.Length - 3);
                knots1.Add(points.Length - 3);
                knots1.Add(points.Length - 3);
                knots = new DoubleCollection(knots1.ToArray());
                weights = new DoubleCollection(Enumerable.Repeat(1, points.Length).Select(x => (double)x).ToArray());
            }
            else
            {
                pts.Add(points[0]);
                pts.Add(points[1]);
                pts.Add(points[2]);
                var knots1 = Enumerable.Range(0, points.Length + 7).Select(x => (double)x).ToList();
                knots = new DoubleCollection(knots1.ToArray());
                weights = new DoubleCollection(Enumerable.Repeat(1, points.Length + 3).Select(x => (double)x).ToArray());
            }
            Spline spline = new Spline(3, true, closed, closed, pts, knots, weights, 0, 0);
            return Draw.AddToCurrentSpace(spline);
        }

        #endregion

        #region shape

        /// <summary>
        /// 绘制矩形
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static ObjectId Rectang(Point3d p1, Point3d p2)
        {
            Point3d[] points = new Point3d[4];
            points[0] = p1;
            points[1] = new Point3d(p1.X, p2.Y, 0);
            points[2] = p2;
            points[3] = new Point3d(p2.X, p1.Y, 0);
            ObjectId result = Draw.Pline(points);
            result.QOpenForWrite<Polyline>(pl => pl.Closed = true);
            return result;
        }

        /// <summary>
        /// 绘制正多边形
        /// </summary>
        /// <param name="n"></param>
        /// <param name="center"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static ObjectId Polygon(int n, Point3d center, Point3d end)
        {
            Point3d[] points = new Point3d[n];
            if (n < 3)
            {
                return ObjectId.Null;
            }
            else
            {
                points[0] = end;
                //points[n] = end;
                Vector3d X = end - center;
                for (int i = 1; i < n; i++)
                {
                    X = X.RotateBy(Math.PI * 2.0 / (double)n, Vector3d.ZAxis);
                    points[i] = new Point3d(center.X + X.X, center.Y + X.Y, center.Z + X.Z);
                }
                ObjectId result = Pline(points);
                result.QOpenForWrite<Polyline>(pl => pl.Closed = true);
                return result;
            }
        }

        /// <summary>
        /// 绘制圆：圆心半径
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static ObjectId Circle(Point3d center, double radius)
        {
            return Draw.AddToCurrentSpace(new Circle(center, Vector3d.ZAxis, radius));
        }

        /// <summary>
        /// 绘制圆：两点
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static ObjectId Circle(Point3d p1, Point3d p2)
        {
            return Circle(Point3d.Origin + 0.5 * ((p1 - Point3d.Origin) + (p2 - Point3d.Origin)), 0.5 * p1.DistanceTo(p2));
        }

        /// <summary>
        /// 绘制圆：三点
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public static ObjectId Circle(Point3d p1, Point3d p2, Point3d p3)
        {
            CircularArc2d geo = new CircularArc2d(new Point2d(p1.X, p1.Y), new Point2d(p2.X, p2.Y), new Point2d(p3.X, p3.Y));
            return Circle(new Point3d(geo.Center.X, geo.Center.Y, 0), geo.Radius);
        }

        //public static void Circle(Line l1, Line l2, double radius)
        //{
        //}

        //public static void Circle(Line l1, Line l2, Line l3)
        //{
        //}

        /// <summary>
        /// 绘制椭圆
        /// </summary>
        /// <param name="center"></param>
        /// <param name="endX"></param>
        /// <param name="radiusY"></param>
        /// <returns></returns>
        public static ObjectId Ellipse(Point3d center, Point3d endX, double radiusY)
        {
            double radiusRatio = center.DistanceTo(endX) / radiusY;
            Vector3d axisX = endX - center;
            if (center.DistanceTo(endX) > radiusY)
            {
                radiusRatio = radiusY / center.DistanceTo(endX);
            }
            else
            {
                axisX = axisX.RotateBy(Math.PI / 2.0, Vector3d.ZAxis);
                axisX = axisX.MultiplyBy(radiusY / center.DistanceTo(endX));
            }
            return Draw.AddToCurrentSpace(new Ellipse(center, Vector3d.ZAxis, axisX, radiusRatio, 0, 2 * Math.PI));
        }

        #endregion

        #region complex

        /// <summary>
        /// 绘制图案填充：根据种子点
        /// </summary>
        /// <param name="hatchName"></param>
        /// <param name="seed"></param>
        public static ObjectId Hatch(string hatchName, Point3d seed)
        {
            ObjectId loop = Draw.Boundary(seed, BoundaryType.Polyline);
            ObjectId result = Draw.Hatch(new ObjectId[] { loop }, hatchName);
            loop.Erase(); // newly 20140521
            return result;
        }

        /// <summary>
        /// 绘制图案填充：根据实体
        /// </summary>
        /// <param name="hatchName"></param>
        /// <param name="ents"></param>
        public static ObjectId Hatch(string hatchName, Entity[] ents)
        {
            // step1 取相交点
            Point3dCollection points = new Point3dCollection();
            for (int i = 0; i < ents.Length; i++)
            {
                for (int j = i + 1; j < ents.Length; j++)
                {
                    ents[i].IntersectWith3264(ents[j], Intersect.OnBothOperands, points);
                }
            }
            // step2 点排序
            var pts = points.Cast<Point3d>().ToList();
            var centroid = new Point3d(pts.Average(p => p.X), pts.Average(p => p.Y), pts.Average(p => p.Z));
            pts = pts.OrderBy(p =>
            {
                var dir = p - centroid;
                var angle = (p - centroid).GetAngleTo(Vector3d.XAxis);
                if (dir.Y < 0)
                {
                    angle = Math.PI * 2 - angle;
                }
                return angle;
            }).ToList();
            // step3 
            return Draw.Hatch(pts, hatchName);
        }

        /// <summary>
        /// 绘制图案填充：根据闭合区域
        /// </summary>
        /// <param name="loopIds"></param>
        /// <param name="hatchName"></param>
        /// <param name="scale"></param>
        /// <param name="angle"></param>
        /// <param name="associative"></param>
        /// <returns></returns>
        public static ObjectId Hatch(ObjectId[] loopIds, string hatchName = "SOLID", double scale = 1, double angle = 0, bool associative = false)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                Hatch hatch = new Hatch();
                BlockTableRecord space = trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                ObjectId result = space.AppendEntity(hatch);
                trans.AddNewlyCreatedDBObject(hatch, true);

                hatch.SetDatabaseDefaults();
                hatch.Normal = new Vector3d(0, 0, 1);
                hatch.Elevation = 0.0;
                hatch.Associative = associative;
                hatch.PatternScale = scale;
                hatch.SetHatchPattern(HatchPatternType.PreDefined, hatchName);
                hatch.PatternAngle = angle; // 按.NET设计规范，属性必须能被以任意顺序设置。然而此处PatternAngle则必须在SetHatchPattern调用后设置，显然AutoCAD API的设计是不合理的。
                hatch.HatchStyle = HatchStyle.Outer;
                loopIds.ToList().ForEach(loop => hatch.AppendLoop(HatchLoopTypes.External, new ObjectIdCollection(new ObjectId[] { loop })));
                hatch.EvaluateHatch(true);

                trans.Commit();
                return result;
            }
        }

        /// <summary>
        /// 绘制图案填充：根据点列
        /// </summary>
        /// <param name="points"></param>
        /// <param name="hatchName"></param>
        /// <param name="scale"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static ObjectId Hatch(IEnumerable<Point3d> points, string hatchName = "SOLID", double scale = 1, double angle = 0)
        {
            var pts = points.ToList();
            if (pts.First() != pts.Last())
            {
                pts.Add(pts.First());
            }
            ObjectId loop = Draw.Pline(pts);
            ObjectId result = Draw.Hatch(new ObjectId[] { loop }, hatchName, scale, angle);
            loop.Erase();
            return result;
        }

        /// <summary>
        /// 绘制边界
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ObjectId Boundary(Point3d seed, BoundaryType type)
        {
            var loop = Application.DocumentManager.MdiActiveDocument.Editor.TraceBoundary(seed, false);
            if (loop.Count > 0)
            {
                if (type == BoundaryType.Polyline)
                {
                    Polyline poly = loop[0] as Polyline;
                    if (poly.Closed)
                    {
                        poly.AddVertexAt(poly.NumberOfVertices, poly.StartPoint.ToPoint2d(), 0, 0, 0);
                        poly.Closed = false;
                    }
                    return Draw.AddToCurrentSpace(poly);
                }
                else // type == BoundaryType.Region
                {
                    var region = Autodesk.AutoCAD.DatabaseServices.Region.CreateFromCurves(loop);
                    if (region.Count > 0)
                    {
                        return Draw.AddToCurrentSpace(region[0] as Region);
                    }
                    else
                    {
                        return ObjectId.Null;
                    }
                }
            }
            else
            {
                return ObjectId.Null;
            }
        }

        /// <summary>
        /// 绘制面域
        /// </summary>
        /// <param name="curveId"></param>
        /// <returns></returns>
        public static ObjectId Region(ObjectId curveId)
        {
            Curve cv = curveId.QOpenForRead<Curve>();
            if (cv == null)
            {
                return ObjectId.Null;
            }
            var region = Autodesk.AutoCAD.DatabaseServices.Region.CreateFromCurves(new DBObjectCollection { cv });
            if (region.Count > 0)
            {
                return Draw.AddToCurrentSpace(region[0] as Region);
            }
            else
            {
                return ObjectId.Null;
            }
        }

        // 20110907
        /// <summary>
        /// 绘制单行文字
        /// </summary>
        /// <param name="text"></param>
        /// <param name="height"></param>
        /// <param name="pos"></param>
        /// <param name="rotation"></param>
        /// <param name="centerAligned"></param>
        /// <param name="textStyle"></param>
        /// <returns></returns>
        public static ObjectId Text(string text, double height, Point3d pos, double rotation = 0, bool centerAligned = false, string textStyle = Consts.TextStyleName)
        {
            var textStyleId = DbHelper.GetTextStyleId(textStyle);
            var style = textStyleId.QOpenForRead<TextStyleTableRecord>();
            DBText txt = new DBText { TextString = text, Position = pos, Rotation = rotation, TextStyleId = textStyleId, Height = height, Oblique = style.ObliquingAngle, WidthFactor = style.XScale };
            if (centerAligned)
            {
                txt.HorizontalMode = TextHorizontalMode.TextCenter;
                txt.VerticalMode = TextVerticalMode.TextVerticalMid;
            }
            ObjectId id = Draw.AddToCurrentSpace(txt);
            return id;
        }

        /// <summary>
        /// 绘制多行文字
        /// </summary>
        /// <param name="text"></param>
        /// <param name="height"></param>
        /// <param name="pos"></param>
        /// <param name="rotation"></param>
        /// <param name="centerAligned"></param>
        /// <param name="width"></param>
        /// <param name="textStyle"></param>
        /// <returns></returns>
        public static ObjectId MText(string text, double height, Point3d pos, double rotation = 0, bool centerAligned = false, double width = 0, string textStyle = Consts.TextStyleName)
        {
            var textStyleId = DbHelper.GetTextStyleId(textStyle);
            MText mt = new MText { Contents = text, TextHeight = height, Location = pos, Rotation = rotation, TextStyleId = textStyleId, Width = width };
            ObjectId id = Draw.AddToCurrentSpace(mt);
            if (centerAligned)
            {
                Point3d center = id.GetCenter();
                id.Move(mt.Location - center);
            }
            return id;
        }

        /// <summary>
        /// 绘制消隐：点列
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static ObjectId Wipeout(IEnumerable<Point3d> points)
        {
            Wipeout wipe = new Autodesk.AutoCAD.DatabaseServices.Wipeout();
            wipe.SetFrom(new Point2dCollection(points.Select(x => x.ToPoint2d()).ToArray()), Vector3d.ZAxis);
            ObjectId result = Draw.AddToCurrentSpace(wipe);
            result.Draworder(DraworderOperation.MoveToTop);
            return result;
        }

        /// <summary>
        /// 绘制消隐：实体
        /// </summary>
        /// <param name="entId"></param>
        /// <returns></returns>
        public static ObjectId Wipeout(ObjectId entId)
        {
            Extents3d extent = entId.QSelect(x => x.GeometricExtents);
            return Wipeout(extent);
        }

        /// <summary>
        /// 绘制消隐：范围
        /// </summary>
        /// <param name="extent"></param>
        /// <returns></returns>
        public static ObjectId Wipeout(Extents3d extent)
        {
            Point3d a = new Point3d(extent.MinPoint.X, extent.MaxPoint.Y, 0);
            Point3d b = new Point3d(extent.MaxPoint.X, extent.MinPoint.Y, 0);
            return Wipeout(new Point3d[] { extent.MinPoint, a, extent.MaxPoint, b, extent.MinPoint });
        }

        /// <summary>
        /// 插入块参照
        /// </summary>
        /// <param name="blockName"></param>
        /// <param name="p"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static ObjectId Insert(string blockName, Point3d p, double rotation = 0, double scale = 1)
        {
            return Draw.AddToCurrentSpace(new BlockReference(p, DbHelper.GetBlockId(blockName))
            {
                Rotation = rotation,
                ScaleFactors = new Scale3d(scale)
            });
        }

        /// <summary>
        /// 定义块：若干实体
        /// </summary>
        /// <param name="entIds"></param>
        /// <param name="blockName"></param>
        /// <returns></returns>
        public static ObjectId Block(IEnumerable<ObjectId> entIds, string blockName)
        {
            return Block(entIds, blockName, entIds.GetCenter());
        }

        /// <summary>
        /// 定义块：若干实体，可指定基点
        /// </summary>
        /// <param name="entIds"></param>
        /// <param name="blockName"></param>
        /// <param name="basePoint"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public static ObjectId Block(IEnumerable<ObjectId> entIds, string blockName, Point3d basePoint, bool overwrite = true)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ObjectId result = ObjectId.Null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForWrite) as BlockTable;
                if (bt.Has(blockName))
                {
                    BlockTableRecord old = trans.GetObject(bt[blockName], OpenMode.ForWrite) as BlockTableRecord;

                    if (!overwrite)
                    {
                        Interaction.Write($"{blockName} already exists and was not overwritten.");
                        return old.Id;
                    }
                    old.Erase();
                }

                BlockTableRecord block = new BlockTableRecord();
                block.Name = blockName;
                foreach (var ent in entIds)
                {
                    Entity entObj = trans.GetObject(ent, OpenMode.ForRead) as Entity;
                    entObj = entObj.Clone() as Entity;
                    entObj.TransformBy(Matrix3d.Displacement(-basePoint.GetAsVector()));
                    block.AppendEntity(entObj);
                }
                result = bt.Add(block);
                trans.AddNewlyCreatedDBObject(block, true);
                trans.Commit();
            }
            return result;
        }

        /// <summary>
        /// Creates a block given entities. 
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="blockName"></param>
        /// <param name="basePoint"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public static ObjectId Block(IEnumerable<Entity> entities, string blockName, Point3d basePoint, bool overwrite = true)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ObjectId result = ObjectId.Null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForWrite) as BlockTable;
                if (bt.Has(blockName))
                {
                    BlockTableRecord old = trans.GetObject(bt[blockName], OpenMode.ForWrite) as BlockTableRecord;

                    if (!overwrite)
                    {
                        Interaction.Write($"{blockName} already exists and was not overwritten.");
                        return old.Id;
                    }
                    old.Erase();
                }

                BlockTableRecord block = new BlockTableRecord();
                block.Name = blockName;
                foreach (var ent in entities)
                {
                    if (!ent.IsWriteEnabled)
                    {
                        ent.UpgradeOpen();
                    }
                    var entObj = ent.Clone() as Entity;
                    entObj.TransformBy(Matrix3d.Displacement(-basePoint.GetAsVector()));
                    block.AppendEntity(entObj);
                }
                result = bt.Add(block);
                trans.AddNewlyCreatedDBObject(block, true);
                trans.Commit();
            }
            return result;
        }

        /// <summary>
        /// Creates a block and then inserts a reference to the model space.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="blockName"></param>
        /// <param name="blockBasePoint"></param>
        /// <param name="blockReferencePoint"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public static ObjectId CreateBlockAndInsertReference(IEnumerable<Entity> entities, string blockName, Point3d blockBasePoint, Point3d blockReferencePoint, double rotation = 0, double scale = 1, bool overwrite = true)
        {
            var blockId = Block(entities, blockName, blockBasePoint, overwrite);
            var blockReference = NoDraw.Insert(blockId, blockReferencePoint, rotation, scale);

            return AddToModelSpace(blockReference);
        }

        /// <summary>
        /// 定义块：从另一DWG中获取
        /// </summary>
        /// <param name="blockName"></param>
        /// <param name="sourceDwg"></param>
        /// <returns></returns>
        public static ObjectId BlockInDwg(string blockName, string sourceDwg)
        {
            ObjectId result = ObjectId.Null;
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable blkTab = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                if (blkTab.Has(blockName))
                {
                    return blkTab[blockName];
                }
                else
                {
                    Database sourceDb = new Database(false, false);
                    sourceDb.ReadDwgFile(sourceDwg, FileOpenMode.OpenForReadAndAllShare, true, string.Empty);
                    ObjectId bId = DbHelper.GetBlockId(sourceDb, blockName);
                    if (bId == ObjectId.Null)
                    {
                        result = ObjectId.Null;
                    }
                    else
                    {
                        Database tempDb = sourceDb.Wblock(bId);
                        result = db.Insert(blockName, tempDb, false);
                    }
                    trans.Commit();
                    return result;
                }
            }
        }

        /// <summary>
        /// 定义块：以另一DWG整个作为
        /// </summary>
        /// <param name="blockName"></param>
        /// <param name="sourceDwg"></param>
        /// <returns></returns>
        public static ObjectId BlockOfDwg(string blockName, string sourceDwg)
        {
            Database sourceDb = new Database(false, false);
            sourceDb.ReadDwgFile(sourceDwg, FileOpenMode.OpenForReadAndAllShare, true, "");
            return HostApplicationServices.WorkingDatabase.Insert(blockName, sourceDb, false);
        }

        /// <summary>
        /// 绘制表格
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="title"></param>
        /// <param name="contents"></param>
        /// <param name="rowHeight"></param>
        /// <param name="columnWidth"></param>
        /// <param name="textHeight"></param>
        /// <param name="textStyle"></param>
        /// <returns></returns>
        public static ObjectId Table(Point3d pos, string title, List<List<string>> contents, double rowHeight, double columnWidth, double textHeight, string textStyle = Consts.TextStyleName)
        {
            Table tb = new Table();
            tb.TableStyle = HostApplicationServices.WorkingDatabase.Tablestyle;
            int numRow = contents.Count + 1;
            tb.InsertRows(0, rowHeight, numRow);
            int numCol = contents.Max(row => row.Count);
            tb.InsertColumns(0, columnWidth, numCol);
            tb.DeleteRows(numRow, 1);
            tb.DeleteColumns(numCol, 1);
            tb.Position = pos;
            tb.SetRowHeight(rowHeight);
            tb.SetColumnWidth(columnWidth);
            tb.Cells.TextHeight = textHeight;
            tb.Cells.TextStyleId = DbHelper.GetTextStyleId(textStyle);

            tb.MergeCells(CellRange.Create(tb, 0, 0, 0, numCol - 1));
            tb.Cells[0, 0].TextString = title;
            for (int i = 0; i < tb.Rows.Count - 1; i++)
            {
                for (int j = 0; j < tb.Columns.Count; j++)
                {
                    if (j < contents[i].Count)
                    {
                        tb.Cells[i + 1, j].TextString = contents[i][j];
                        tb.Cells[i + 1, j].Alignment = CellAlignment.MiddleCenter;
                    }
                }
            }

            return Draw.AddToCurrentSpace(tb);
        }

        /// <summary>
        /// 绘制多边形网格
        /// </summary>
        /// <param name="points"></param>
        /// <param name="m"></param>
        /// <param name="n"></param>
        /// <param name="mClosed"></param>
        /// <param name="nClosed"></param>
        /// <returns></returns>
        public static ObjectId PolygonMesh(List<Point3d> points, int m, int n, bool mClosed = false, bool nClosed = false)
        {
            PolygonMesh mesh = new PolygonMesh(PolyMeshType.SimpleMesh, m, n, new Point3dCollection(points.ToArray()), mClosed, nClosed);
            return mesh.AddToCurrentSpace();
        }

        #endregion

        #region dimensions

        /// <summary>
        /// 线性标注
        /// </summary>
        /// <param name="start">起点</param>
        /// <param name="end">终点</param>
        /// <param name="dim">标注点</param>
        /// <returns>标注ID</returns>
        public static ObjectId Dimlin(Point3d start, Point3d end, Point3d dim)
        {
            double dist = start.DistanceTo(end);
            AlignedDimension ad = new AlignedDimension(start, end, dim, dist.ToString(), HostApplicationServices.WorkingDatabase.Dimstyle);
            return Draw.AddToCurrentSpace(ad);
        }

        #endregion

        #region helpers

        /// <summary>
        /// 添加到模型空间
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        public static ObjectId AddToModelSpace(this Entity ent)
        {
            ObjectId id;
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                id = ((BlockTableRecord)trans.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite, false)).AppendEntity(ent);
                trans.AddNewlyCreatedDBObject(ent, true);
                trans.Commit();
            }
            return id;
        }

        /// <summary>
        /// 添加到当前空间
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        public static ObjectId AddToCurrentSpace(this Entity ent)
        {
            ObjectId id;
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                id = ((BlockTableRecord)trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite, false)).AppendEntity(ent);
                trans.AddNewlyCreatedDBObject(ent, true);
                trans.Commit();
            }
            return id;
        }

        /// <summary>
        /// 添加到当前空间
        /// </summary>
        /// <param name="ents"></param>
        /// <returns></returns>
        public static ObjectId[] AddToCurrentSpace(this IEnumerable<Entity> ents)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            List<ObjectId> ids = new List<ObjectId>();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                foreach (var ent in ents)
                {
                    ObjectId id = ((BlockTableRecord)trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite, false)).AppendEntity(ent);
                    trans.AddNewlyCreatedDBObject(ent, true);
                    ids.Add(id);
                }
                trans.Commit();
            }
            return ids.ToArray();
        }

        #endregion
    }

    // todo: add more entities
    /// <summary>
    /// The "NoDraw" module: create in-memory entities
    /// </summary>
    public static class NoDraw
    {
        /// <summary>
        /// 创建直线
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static Line Line(Point3d p1, Point3d p2)
        {
            return new Line(p1, p2);
        }

        /// <summary>
        /// 创建直线
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Line[] Line(params Point3d[] points)
        {
            return Enumerable.Range(0, points.Length - 1).Select(x => Line(points[x], points[x + 1])).ToArray();
        }

        /// <summary>
        /// 创建圆：圆心半径
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static Circle Circle(Point3d center, double radius)
        {
            return new Circle(center, Vector3d.ZAxis, radius);
        }

        /// <summary>
        /// 创建圆：两点
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static Circle Circle(Point3d p1, Point3d p2)
        {
            return Circle(Point3d.Origin + 0.5 * ((p1 - Point3d.Origin) + (p2 - Point3d.Origin)), 0.5 * p1.DistanceTo(p2));
        }

        /// <summary>
        /// 创建圆：三点
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public static Circle Circle(Point3d p1, Point3d p2, Point3d p3)
        {
            CircularArc2d geo = new CircularArc2d(new Point2d(p1.X, p1.Y), new Point2d(p2.X, p2.Y), new Point2d(p3.X, p3.Y));
            return Circle(new Point3d(geo.Center.X, geo.Center.Y, 0), geo.Radius);
        }

        /// <summary>
        /// 创建多段线 newly 20140717
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Polyline Pline(params Point3d[] points)
        {
            return Pline(points.ToList());
        }

        /// <summary>
        /// 创建多段线
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static Polyline Pline(IEnumerable<Point3d> points)
        {
            return Pline(points, 0);
        }

        /// <summary>
        /// 创建多段线
        /// </summary>
        /// <param name="points"></param>
        /// <param name="globalWidth"></param>
        /// <returns></returns>
        public static Polyline Pline(IEnumerable<Point3d> points, double globalWidth)
        {
            TupleList<Point3d, double> pairs = new TupleList<Point3d, double>();
            points.ToList().ForEach(x => pairs.Add(x, 0));
            return Pline(pairs, globalWidth);
        }

        /// <summary>
        /// 创建多段线
        /// </summary>
        /// <param name="pointBulgePairs"></param>
        /// <param name="globalWidth"></param>
        /// <returns></returns>
        public static Polyline Pline(TupleList<Point3d, double> pointBulgePairs, double globalWidth)
        {
            Polyline poly = new Polyline();
            Enumerable.Range(0, pointBulgePairs.Count).ToList().ForEach(x =>
                poly.AddVertexAt(x, new Point2d(pointBulgePairs[x].Item1.X, pointBulgePairs[x].Item1.Y), pointBulgePairs[x].Item2, globalWidth, globalWidth)
                );
            return poly;
        }

        /// <summary>
        /// 创建矩形
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static Polyline Rectang(Point3d p1, Point3d p2)
        {
            Point3d[] points = new Point3d[4];
            points[0] = p1;
            points[1] = new Point3d(p1.X, p2.Y, 0);
            points[2] = p2;
            points[3] = new Point3d(p2.X, p1.Y, 0);
            Polyline result = Pline(points);
            result.Closed = true;
            return result;
        }

        /// <summary>
        /// 创建单行文字
        /// </summary>
        /// <param name="text"></param>
        /// <param name="height"></param>
        /// <param name="pos"></param>
        /// <param name="rotation"></param>
        /// <param name="centerAligned"></param>
        /// <param name="textStyle"></param>
        /// <returns></returns>
        public static DBText Text(string text, double height, Point3d pos, double rotation = 0, bool centerAligned = false, string textStyle = Consts.TextStyleName)
        {
            var textStyleId = DbHelper.GetTextStyleId(textStyle);
            var style = textStyleId.QOpenForRead<TextStyleTableRecord>();
            DBText txt = new DBText { TextString = text, Position = pos, Rotation = rotation, TextStyleId = textStyleId, Height = height, Oblique = style.ObliquingAngle, WidthFactor = style.XScale };
            if (centerAligned) // todo: centerAligned=true会使DT消失
            {
                txt.HorizontalMode = TextHorizontalMode.TextCenter;
                txt.VerticalMode = TextVerticalMode.TextVerticalMid;
            }
            return txt;
        }

        /// <summary>
        /// 创建多行文字
        /// </summary>
        /// <param name="text"></param>
        /// <param name="height"></param>
        /// <param name="pos"></param>
        /// <param name="rotation"></param>
        /// <param name="centerAligned"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static MText MText(string text, double height, Point3d pos, double rotation = 0, bool centerAligned = false, double width = 0)
        {
            MText mt = new MText { Contents = text, TextHeight = height, Location = pos, Rotation = rotation, Width = width };
            if (centerAligned)
            {
                Point3d center = mt.GetCenter();
                mt.Move(mt.Location - center);
            }
            return mt;
        }

        /// <summary>
        /// 创建点
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static DBPoint Point(Point3d p)
        {
            return new DBPoint(p);
        }

        /// <summary>
        /// 创建块参照
        /// </summary>
        /// <param name="blockTableRecordId"></param>
        /// <param name="pos"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static BlockReference Insert(ObjectId blockTableRecordId, Point3d pos, double rotation = 0, double scale = 1)
        {
            return new BlockReference(pos, blockTableRecordId) { Rotation = rotation, ScaleFactors = new Scale3d(scale) };
        }
    }

    /// <summary>
    /// The "Modify" module: edit entities (with AutoCAD-command-like functions)
    /// </summary>
    public static class Modify // todo: need to add overloads for multiple ids
    {
        #region geometric

        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="entId"></param>
        /// <param name="vector"></param>
        public static void Move(this ObjectId entId, Vector3d vector)
        {
            using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                var entObj = trans.GetObject(entId, OpenMode.ForWrite) as Entity;
                entObj.TransformBy(Matrix3d.Displacement(vector));
                trans.Commit();
            }
        }

        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="vector"></param>
        public static void Move(this Entity ent, Vector3d vector)
        {
            ent.TransformBy(Matrix3d.Displacement(vector));
        }

        /// <summary>
        /// 复制（基于位移向量）
        /// </summary>
        /// <param name="entId"></param>
        /// <param name="vectors"></param>
        /// <returns></returns>
        public static ObjectId[] Copy(this ObjectId entId, IEnumerable<Vector3d> vectors)
        {
            using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                var entObj = trans.GetObject(entId, OpenMode.ForWrite) as Entity;
                var copies = vectors.Select(x =>
                {
                    var copy = entObj.Clone() as Entity;
                    copy.TransformBy(Matrix3d.Displacement(x));
                    return copy;
                });
                trans.Commit();
                return copies.Select(x => Draw.AddToCurrentSpace(x)).ToArray();
            }
        }

        /// <summary>
        /// 复制（基于基点和新点）
        /// </summary>
        /// <param name="entId"></param>
        /// <param name="basePoint"></param>
        /// <param name="newPoints"></param>
        /// <returns></returns>
        public static ObjectId[] Copy(this ObjectId entId, Point3d basePoint, IEnumerable<Point3d> newPoints)
        {
            return entId.Copy(newPoints.Select(x => x - basePoint));
        }

        /// <summary>
        /// 旋转
        /// </summary>
        /// <param name="entId"></param>
        /// <param name="basePoint"></param>
        /// <param name="angle"></param>
        public static void Rotate(this ObjectId entId, Point3d basePoint, double angle)
        {
            using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                var entObj = trans.GetObject(entId, OpenMode.ForWrite) as Entity;
                entObj.TransformBy(Matrix3d.Rotation(angle, Vector3d.ZAxis, basePoint));
                trans.Commit();
            }
        }

        /// <summary>
        /// 缩放
        /// </summary>
        /// <param name="entId"></param>
        /// <param name="basePoint"></param>
        /// <param name="scale"></param>
        public static void Scale(this ObjectId entId, Point3d basePoint, double scale)
        {
            using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                var entObj = trans.GetObject(entId, OpenMode.ForWrite) as Entity;
                entObj.TransformBy(Matrix3d.Scaling(scale, basePoint));
                trans.Commit();
            }
        }

        /// <summary>
        /// 偏移
        /// </summary>
        /// <param name="curveId"></param>
        /// <param name="amount"></param>
        /// <param name="side"></param>
        /// <returns></returns>
        public static ObjectId Offset(this ObjectId curveId, double amount, Point3d side)
        {
            using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                var cvObj = trans.GetObject(curveId, OpenMode.ForRead) as Curve;
                var cv1 = cvObj.GetOffsetCurves(-amount)[0] as Curve;
                var cv2 = cvObj.GetOffsetCurves(amount)[0] as Curve;
                if (cv1.GetDistToPoint(side) < cv2.GetDistToPoint(side))
                {
                    return Draw.AddToCurrentSpace(cv1);
                }
                else
                {
                    return Draw.AddToCurrentSpace(cv2);
                }
            }
        }

        /// <summary>
        /// 镜像
        /// </summary>
        /// <param name="entId"></param>
        /// <param name="axis"></param>
        /// <param name="copy"></param>
        /// <returns></returns>
        public static ObjectId Mirror(this ObjectId entId, Line axis, bool copy = true)
        {
            using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                var entObj = trans.GetObject(entId, OpenMode.ForRead) as Entity;
                Line3d axisLine = new Line3d(axis.StartPoint, axis.EndPoint);
                var mirror = entObj.Clone() as Entity;
                mirror.TransformBy(Matrix3d.Mirroring(axisLine));
                if (!copy)
                {
                    entObj.UpgradeOpen();
                    entObj.Erase();
                    trans.Commit();
                }
                return Draw.AddToCurrentSpace(mirror);
            }
        }

        /// <summary>
        /// 打断
        /// </summary>
        /// <param name="curveId"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static ObjectId[] Break(this ObjectId curveId, Point3d p1, Point3d p2)
        {
            using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                var cvObj = trans.GetObject(curveId, OpenMode.ForRead) as Curve;
                double param1 = cvObj.GetParamAtPointX(p1);
                double param2 = cvObj.GetParamAtPointX(p2);
                DBObjectCollection splits = cvObj.GetSplitCurves(new DoubleCollection(new double[] { param1, param2 }));
                var breaks = splits.Cast<Entity>();
                return new[] { breaks.First().ObjectId, breaks.Last().ObjectId };
            }
        }

        /// <summary>
        /// 打断于点
        /// </summary>
        /// <param name="curveId"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static ObjectId[] Break(this ObjectId curveId, Point3d p)
        {
            return curveId.Break(p, p);
        }

        #endregion

        #region database

        /// <summary>
        /// 炸开
        /// </summary>
        /// <param name="entId"></param>
        /// <returns></returns>
        public static ObjectId[] Explode(this ObjectId entId)
        {
            using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                var entObj = trans.GetObject(entId, OpenMode.ForWrite) as Entity;
                DBObjectCollection results = new DBObjectCollection();
                entObj.Explode(results);
                var ids = results.Cast<Entity>().Select(x => Draw.AddToCurrentSpace(x)).ToArray();
                entObj.Erase();
                trans.Commit();
                return ids;
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="entId"></param>
        public static void Erase(this ObjectId entId)
        {
            using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                var entObj = trans.GetObject(entId, OpenMode.ForWrite);
                entObj.Erase();
                trans.Commit();
            }
        }

        /// <summary>
        /// 删除组和组中的实体
        /// </summary>
        /// <param name="groupId">组ID</param>
        public static void EraseGroup(this ObjectId groupId)
        {
            DbHelper.GetEntityIdsInGroup(groupId).Cast<ObjectId>().QForEach(x => x.Erase());
            groupId.QOpenForWrite(x => x.Erase());
        }

        /// <summary>
        /// 编组
        /// </summary>
        /// <param name="entIds"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ObjectId Group(this IEnumerable<ObjectId> entIds, string name = "*", bool selectable = true)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                DBDictionary groupDict = (DBDictionary)trans.GetObject(db.GroupDictionaryId, OpenMode.ForWrite);
                Group group = new Group(name, selectable);
                foreach (var id in entIds)
                {
                    group.Append(id);
                }
                ObjectId result = groupDict.SetAt(name, group);
                trans.AddNewlyCreatedDBObject(group, true); // false with no commit?
                trans.Commit();
                return result;
            }
        }

        /// <summary>
        /// 添加到组
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="entIds"></param>
        public static void AppendToGroup(ObjectId groupId, IEnumerable<ObjectId> entIds)
        {
            using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                Group group = trans.GetObject(groupId, OpenMode.ForWrite) as Group;
                if (group != null)
                {
                    group.Append(new ObjectIdCollection(entIds.ToArray()));
                }
                trans.Commit();
            }
        }

        /// <summary>
        /// 解组
        /// </summary>
        /// <param name="groupId"></param>
        public static void Ungroup(ObjectId groupId)
        {
            var groupDictId = HostApplicationServices.WorkingDatabase.GroupDictionaryId;
            groupDictId.QOpenForWrite<DBDictionary>(x => x.Remove(groupId));
            groupId.Erase();
        }

        /// <summary>
        /// 解组（实体）
        /// </summary>
        /// <param name="entIds"></param>
        /// <returns></returns>
        public static int Ungroup(IEnumerable<ObjectId> entIds)
        {
            var groupDict = HostApplicationServices.WorkingDatabase.GroupDictionaryId.QOpenForRead<DBDictionary>();
            int count = 0;
            foreach (var entry in groupDict)
            {
                var group = entry.Value.QOpenForRead<Group>();
                if (entIds.Any(x => group.Has(x.QOpenForRead<Entity>())))
                {
                    Ungroup(entry.Value);
                    count++;
                }
            }
            return count;
        }

        #endregion

        #region feature

        /// <summary>
        /// 阵列
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="arrayOpt"></param>
        public static void Array(Entity ent, string arrayOpt)
        {
        }

        /// <summary>
        /// 块阵列
        /// </summary>
        /// <param name="record"></param>
        /// <param name="arrayOpt"></param>
        public static void Array(BlockTableRecord record, string arrayOpt)
        {
        }

        /// <summary>
        /// 圆角
        /// </summary>
        /// <param name="cv1"></param>
        /// <param name="cv2"></param>
        /// <param name="bevelOpt"></param>
        public static void Fillet(Curve cv1, Curve cv2, string bevelOpt)
        {
        }

        /// <summary>
        /// 倒角
        /// </summary>
        /// <param name="cv1"></param>
        /// <param name="cv2"></param>
        /// <param name="bevelOpt"></param>
        public static void Chamfer(Curve cv1, Curve cv2, string bevelOpt)
        {
        }

        /// <summary>
        /// 修剪
        /// </summary>
        /// <param name="baseCurves"></param>
        /// <param name="cv"></param>
        /// <param name="p"></param>
        public static void Trim(Curve[] baseCurves, Curve cv, Point3d[] p)
        {
        }

        /// <summary>
        /// 延伸
        /// </summary>
        /// <param name="baseCurves"></param>
        /// <param name="cv"></param>
        /// <param name="p"></param>
        public static void Extend(Curve[] baseCurves, Curve cv, Point3d[] p)
        {
        }

        // 20110712修改
        /// <summary>
        /// 绘图顺序
        /// </summary>
        /// <param name="entId"></param>
        /// <param name="operation"></param>
        public static void Draworder(this ObjectId entId, DraworderOperation operation)
        {
            HostApplicationServices.WorkingDatabase.BlockTableId.QOpenForWrite<BlockTable>(bt =>
            {
                bt[BlockTableRecord.ModelSpace].QOpenForWrite<BlockTableRecord>(btr =>
                {
                    btr.DrawOrderTableId.QOpenForWrite<DrawOrderTable>(dot =>
                    {
                        switch (operation)
                        {
                            case DraworderOperation.MoveToTop:
                                dot.MoveToTop(new ObjectIdCollection { entId });
                                break;
                            case DraworderOperation.MoveToBottom:
                                dot.MoveToBottom(new ObjectIdCollection { entId });
                                break;
                            default:
                                break;
                        }
                    });
                });
            });
        }

        /// <summary>
        /// 绘图顺序
        /// </summary>
        /// <param name="entIds"></param>
        /// <param name="operation"></param>
        public static void Draworder(this IEnumerable<ObjectId> entIds, DraworderOperation operation)
        {
            HostApplicationServices.WorkingDatabase.BlockTableId.QOpenForWrite<BlockTable>(bt =>
            {
                bt[BlockTableRecord.ModelSpace].QOpenForWrite<BlockTableRecord>(btr =>
                {
                    btr.DrawOrderTableId.QOpenForWrite<DrawOrderTable>(dot =>
                    {
                        switch (operation)
                        {
                            case DraworderOperation.MoveToTop:
                                dot.MoveToTop(new ObjectIdCollection(entIds.ToArray()));
                                break;
                            case DraworderOperation.MoveToBottom:
                                dot.MoveToBottom(new ObjectIdCollection(entIds.ToArray()));
                                break;
                            default:
                                break;
                        }
                    });
                });
            });
        }

        #endregion

        #region property

        /// <summary>
        /// 设置文字样式
        /// </summary>
        /// <param name="name">样式名</param>
        /// <param name="fontFamily">字体名。e.g."宋体""@宋体"</param>
        /// <param name="textHeight">字高</param>
        /// <param name="italicAngle">斜角</param>
        /// <param name="xScale">宽度因子</param>
        /// <param name="vertical">是否竖排</param>
        /// <param name="bigFont">大字体</param>
        /// <returns>文字样式ID</returns>
        public static ObjectId TextStyle(string fontFamily, double textHeight, double italicAngle = 0, double xScale = 1, bool vertical = false, string bigFont = "", string textStyle = Consts.TextStyleName)
        {
            ObjectId result = DbHelper.GetTextStyleId(textStyle, true);
            result.QOpenForWrite<TextStyleTableRecord>(tstr =>
            {
                tstr.Font = new Autodesk.AutoCAD.GraphicsInterface.FontDescriptor(fontFamily, false, false, 0, 34);
                tstr.TextSize = textHeight;
                tstr.ObliquingAngle = italicAngle;
                tstr.XScale = xScale;
                tstr.IsVertical = vertical;
                tstr.BigFontFileName = bigFont;
            });
            return result;
        }

        /// <summary>
        /// 设置图层
        /// </summary>
        /// <param name="entId"></param>
        /// <param name="layer"></param>
        public static void SetLayer(this ObjectId entId, string layer)
        {
            entId.QOpenForWrite<Entity>(ent =>
            {
                ent.LayerId = DbHelper.GetLayerId(layer);
            });
        }

        /// <summary>
        /// 设置线型
        /// </summary>
        /// <param name="id"></param>
        /// <param name="linetype"></param>
        /// <param name="linetypeScale"></param>
        public static void SetLinetype(this ObjectId id, string linetype, double linetypeScale = 1)
        {
            id.QOpenForWrite<Entity>(ent =>
            {
                ent.LinetypeId = DbHelper.GetLinetypeId(linetype);
                ent.LinetypeScale = linetypeScale;
            });
        }

        /// <summary>
        /// 设置标注样式，无此样式则设为默认样式
        /// </summary>
        /// <param name="dimId">标注ID</param>
        /// <param name="dimstyle">样式名</param>
        public static void SetDimstyle(this ObjectId dimId, string dimstyle)
        {
            ObjectId dimstyleId = DbHelper.GetDimstyleId(dimstyle);
            using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                Dimension dim = trans.GetObject(dimId, OpenMode.ForWrite) as Dimension;
                dim.DimensionStyle = dimstyleId;
                trans.Commit();
            }
        }

        /// <summary>
        /// 设置多行文字样式，无此样式则设为默认样式
        /// </summary>
        /// <param name="mtId">多行文字ID</param>
        /// <param name="textstyle">样式名</param>
        public static void SetTextStyle(this ObjectId mtId, string textstyle)
        {
            ObjectId textstyleId = DbHelper.GetTextStyleId(textstyle);
            using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                MText mt = trans.GetObject(mtId, OpenMode.ForWrite) as MText;
                mt.TextStyleId = textstyleId;
                trans.Commit();
            }
        }

        #endregion
    }

    /// <summary>
    /// Boundary trace entity type
    /// </summary>
    public enum BoundaryType
    {
        /// <summary>
        /// Trace with polyline
        /// </summary>
        Polyline = 0,
        /// <summary>
        /// Trace with region
        /// </summary>
        Region = 1
    }

    /// <summary>
    /// Draworder type
    /// </summary>
    public enum DraworderOperation
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// 上移
        /// </summary>
        MoveAbove = 1,
        /// <summary>
        /// 下移
        /// </summary>
        MoveBelow = 2,
        /// <summary>
        /// 前置
        /// </summary>
        MoveToTop = 3,
        /// <summary>
        /// 后置
        /// </summary>
        MoveToBottom = 4
    }
}
