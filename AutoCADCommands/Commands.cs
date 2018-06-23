using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoCADCommands
{
    /// <summary>
    /// The "Draw" module: directly draw entities (with AutoCAD-command-like functions)
    /// </summary>
    public static class Draw
    {
        #region point

        /// <summary>
        /// Draws a point.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Point(Point3d position)
        {
            return NoDraw.Point(position).AddToCurrentSpace();
        }

        /// <summary>
        /// Draws 'divide' entities.
        /// </summary>
        /// <param name="curve">The curve.</param>
        /// <param name="number">The number of curve segments.</param>
        /// <param name="entity">The entity to draw with. Either DBPoint or BlockReference.</param>
        /// <returns></returns>
        public static ObjectId[] Divide(Curve curve, int number, Entity entity)
        {
            return Draw
                .GetCurvePoints(curve, number: number)
                .PlaceEntity(entity)
                .AddToCurrentSpace();
        }

        /// <summary>
        /// Draws 'measure' entities.
        /// </summary>
        /// <param name="curve">The curve.</param>
        /// <param name="interval">The length of curve segments.</param>
        /// <param name="entity">The entity to draw with. Either DBPoint or BlockReference.</param>
        /// <returns></returns>
        public static ObjectId[] Measure(Curve curve, double interval, Entity entity)
        {
            return Draw
                .GetCurvePoints(curve, interval: interval)
                .PlaceEntity(entity)
                .AddToCurrentSpace();
        }

        private static IEnumerable<Point3d> GetCurvePoints(Curve curve, int number = -1, double interval = -1)
        {
            double start = curve.GetDistAtParam(curve.StartParam);
            double end = curve.GetDistAtParam(curve.EndParam);
            interval = interval == -1 ? (end - start) / number : interval;
            number = number == -1 ? (int)Math.Floor((end - start) / interval) : number;

            return Enumerable
                .Range(1, number - 1)
                .Select(n => curve.GetPointAtParam(start + n * interval));
        }

        private static IEnumerable<Entity> PlaceEntity(this IEnumerable<Point3d> positions, Entity entity)
        {
            foreach (var position in positions)
            {
                var newEntity = entity.Clone() as Entity;
                if (newEntity is DBPoint)
                {
                    (newEntity as DBPoint).Position = position;
                }
                else if (newEntity is BlockReference)
                {
                    (newEntity as BlockReference).Position = position;
                }

                yield return newEntity;
            }
        }

        #endregion

        #region line

        /// <summary>
        /// Draws a line.
        /// </summary>
        /// <param name="point1">The point 1.</param>
        /// <param name="point2">The point 2.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Line(Point3d point1, Point3d point2)
        {
            return NoDraw.Line(point1, point2).AddToCurrentSpace();
        }

        /// <summary>
        /// Draws multiple lines by a sequence of points.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns>The object IDs.</returns>
        public static ObjectId[] Line(params Point3d[] points)
        {
            return NoDraw.Line(points).AddToCurrentSpace();
        }

        /// <summary>
        /// Draws an arc from 3 points.
        /// </summary>
        /// <param name="point1">The point 1.</param>
        /// <param name="point2">The point 2.</param>
        /// <param name="point3">The point 3.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Arc3P(Point3d point1, Point3d point2, Point3d point3)
        {
            return NoDraw.Arc3P(point1, point2, point3).AddToCurrentSpace();
        }

        //public static ObjectId ArcSCE(Point3d start, Point3d center, Point3d end)
        //{
        //}

        /// <summary>
        /// Draws an arc from start, center, and angle.
        /// </summary>
        /// <param name="start">The start point.</param>
        /// <param name="center">The center point.</param>
        /// <param name="angle">The angle.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId ArcSCA(Point3d start, Point3d center, double angle)
        {
            return NoDraw.ArcSCA(start, center, angle).AddToCurrentSpace();
        }

        //public static ObjectId ArcSCL(Point3d start, Point3d center, double length)
        //{
        //}

        //public static ObjectId ArcSEA(Point3d start, Point3d end, double angle)
        //{
        //}

        //public static ObjectId ArcSED(Point3d start, Point3d end, Vector3d dir)
        //{
        //}

        //public static ObjectId ArcSER(Point3d start, Point3d end, double radius)
        //{
        //}

        /// <summary>
        /// Draws an arc from a geometry arc.
        /// </summary>
        /// <param name="arc">The geometry arc.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId ArcFromGeometry(CircularArc3d arc)
        {
            return NoDraw.ArcFromGeometry(arc).AddToCurrentSpace();
        }

        /// <summary>
        /// Draws an arc from a geometry arc.
        /// </summary>
        /// <param name="arc">The geometry arc.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId ArcFromGeometry(CircularArc2d arc)
        {
            return NoDraw.ArcFromGeometry(arc).AddToCurrentSpace();
        }

        /// <summary>
        /// Draws a polyline by a sequence of points.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Pline(params Point3d[] points)
        {
            return NoDraw.Pline(points).AddToCurrentSpace();
        }

        /// <summary>
        /// Draws a polyline by a sequence of points and a global width.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="globalWidth">The global width. Default is 0.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Pline(IEnumerable<Point3d> points, double globalWidth = 0)
        {
            return NoDraw.Pline(points, globalWidth).AddToCurrentSpace();
        }

        /// <summary>
        /// Draws a polyline by a sequence of vertices (position + bulge).
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="globalWidth">The global width. Default is 0.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Pline(List<(Point3d, double)> vertices, double globalWidth = 0)
        {
            return NoDraw.Pline(vertices, globalWidth).AddToCurrentSpace();
        }

        /// <summary>
        /// Draws a spline by fit points.
        /// </summary>
        /// <param name="points">The points to fit.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId SplineFit(Point3d[] points)
        {
            return NoDraw.SplineFit(points).AddToCurrentSpace();
        }

        /// <summary>
        /// Draws a spline by control points.
        /// </summary>
        /// <param name="points">The control points.</param>
        /// <param name="closed">Whether to close the spline.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId SplineCV(Point3d[] points, bool closed = false)
        {
            return NoDraw.SplineCV(points, closed).AddToCurrentSpace();
        }

        #endregion

        #region shape

        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="point1">The point 1.</param>
        /// <param name="point2">The point 2.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Rectang(Point3d point1, Point3d point2)
        {
            return NoDraw.Rectang(point1, point2).AddToCurrentSpace();
        }

        /// <summary>
        /// Draws a regular N-polygon.
        /// </summary>
        /// <param name="n">The N.</param>
        /// <param name="center">The center.</param>
        /// <param name="end">One vertex.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Polygon(int n, Point3d center, Point3d end)
        {
            if (n < 3)
            {
                return ObjectId.Null;
            }

            var direction = end - center;
            var points = Enumerable
                .Range(0, n)
                .Select(index => center.Add(direction.RotateBy(2 * Math.PI / n * index, Vector3d.ZAxis)))
                .ToArray();

            var result = Draw.Pline(points);
            result.QOpenForWrite<Polyline>(poly => poly.Closed = true);
            return result;
        }

        /// <summary>
        /// Draws a circle from center and radius.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="radius">The radius.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Circle(Point3d center, double radius)
        {
            return NoDraw.Circle(center, radius).AddToCurrentSpace();
        }

        /// <summary>
        /// Draws a circle from diameter ends.
        /// </summary>
        /// <param name="point1">The point 1.</param>
        /// <param name="point2">The point 2.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Circle(Point3d point1, Point3d point2)
        {
            return NoDraw.Circle(point1, point2).AddToCurrentSpace();
        }

        /// <summary>
        /// Draws a circle from 3 points.
        /// </summary>
        /// <param name="point1">The point 1.</param>
        /// <param name="point2">The point 2.</param>
        /// <param name="point3">The point 3.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Circle(Point3d point1, Point3d point2, Point3d point3)
        {
            return NoDraw.Circle(point1, point2, point3).AddToCurrentSpace();
        }

        /// <summary>
        /// Draws a circle from a geometry circle.
        /// </summary>
        /// <param name="circle">The geometry circle.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Circle(CircularArc3d circle)
        {
            return NoDraw.Circle(circle).AddToCurrentSpace();
        }

        /// <summary>
        /// Draws a circle from a geometry circle.
        /// </summary>
        /// <param name="circle">The geometry circle.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Circle(CircularArc2d circle)
        {
            return NoDraw.Circle(circle).AddToCurrentSpace();
        }

        //public ObjectId void Circle(Line l1, Line l2, double radius)
        //{
        //}

        //public ObjectId void Circle(Line l1, Line l2, Line l3)
        //{
        //}

        /// <summary>
        /// Draws an ellipse by center, endX, and radiusY.
        /// </summary>
        /// <remarks>
        /// The ellipse will be drawn on the XY plane.
        /// </remarks>
        /// <param name="center">The center.</param>
        /// <param name="endX">The intersection point of the ellipse and its X axis.</param>
        /// <param name="radiusY">The Y radius.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Ellipse(Point3d center, Point3d endX, double radiusY)
        {
            return NoDraw.Ellipse(center, endX, radiusY).AddToCurrentSpace();
        }

        #endregion

        #region complex

        /// <summary>
        /// Draws hatch by seed.
        /// </summary>
        /// <param name="hatchName">The hatch name.</param>
        /// <param name="seed">The seed.</param>
        public static ObjectId Hatch(string hatchName, Point3d seed)
        {
            var loop = Draw.Boundary(seed, BoundaryType.Polyline);
            var result = Draw.Hatch(new[] { loop }, hatchName);
            loop.Erase(); // newly 20140521
            return result;
        }

        /// <summary>
        /// Draws hatch by entities.
        /// </summary>
        /// <param name="hatchName">The hatch name.</param>
        /// <param name="entities">The entities.</param>
        public static ObjectId Hatch(string hatchName, Entity[] entities)
        {
            // Step1 - find intersections
            var points = new Point3dCollection();
            for (int i = 0; i < entities.Length; i++)
            {
                for (int j = i + 1; j < entities.Length; j++)
                {
                    entities[i].IntersectWith3264(entities[j], Intersect.OnBothOperands, points);
                }
            }

            // Step2 - sort points
            var pointList = points.Cast<Point3d>().ToList();
            var centroid = new Point3d(pointList.Average(p => p.X), pointList.Average(p => p.Y), pointList.Average(p => p.Z));
            pointList = pointList
                .OrderBy(point =>
                {
                    var dir = point - centroid;
                    var angle = (point - centroid).GetAngleTo(Vector3d.XAxis);
                    if (dir.Y < 0)
                    {
                        angle = Math.PI * 2 - angle;
                    }
                    return angle;
                })
                .ToList();

            // Step2 - draw
            return Draw.Hatch(pointList, hatchName);
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
            var db = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                var hatch = new Hatch();
                var space = trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                var result = space.AppendEntity(hatch);
                trans.AddNewlyCreatedDBObject(hatch, true);

                hatch.SetDatabaseDefaults();
                hatch.Normal = new Vector3d(0, 0, 1);
                hatch.Elevation = 0.0;
                hatch.Associative = associative;
                hatch.PatternScale = scale;
                hatch.SetHatchPattern(HatchPatternType.PreDefined, hatchName);
                hatch.PatternAngle = angle; // PatternAngle has to be after SetHatchPattern(). This is AutoCAD .NET SDK violating Framework Design Guidelines, which requires properties to be set in arbitrary order.
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
        /// Draws boundary.
        /// </summary>
        /// <param name="seed">The seed.</param>
        /// <param name="type">The boundary type.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Boundary(Point3d seed, BoundaryType type)
        {
            var loop = Application.DocumentManager.MdiActiveDocument.Editor.TraceBoundary(seed, false);
            if (loop.Count > 0)
            {
                if (type == BoundaryType.Polyline)
                {
                    var poly = loop[0] as Polyline;
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
                }
            }

            return ObjectId.Null;
        }

        /// <summary>
        /// Draws a region.
        /// </summary>
        /// <param name="curveId">The boundary curve.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Region(ObjectId curveId)
        {
            var curve = curveId.QOpenForRead<Curve>();
            if (curve != null)
            {
                var region = Autodesk.AutoCAD.DatabaseServices.Region.CreateFromCurves(new DBObjectCollection { curve });
                if (region.Count > 0)
                {
                    return Draw.AddToCurrentSpace(region[0] as Region);
                }
            }

            return ObjectId.Null;
        }

        /// <summary>
        /// Draws a DT.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="height">The height.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="centerAligned">Whether to center align.</param>
        /// <param name="textStyle">The text style name.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Text(string text, double height, Point3d position, double rotation = 0, bool centerAligned = false, string textStyle = Consts.TextStyleName)
        {
            return NoDraw.Text(text, height, position, rotation, centerAligned, textStyle).AddToCurrentSpace();
        }

        /// <summary>
        /// Draws an MT.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="height">The height.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="centerAligned">Whether to center align.</param>
        /// <param name="width">The width.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId MText(string text, double height, Point3d position, double rotation = 0, bool centerAligned = false, double width = 0)
        {
            return NoDraw.MText(text, height, position, rotation, centerAligned, width).AddToCurrentSpace();
        }

        /// <summary>
        /// Draws wipeout from a sequence of points.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Wipeout(params Point3d[] points)
        {
            var wipe = new Wipeout();
            wipe.SetFrom(
                points: new Point2dCollection(points.Select(x => x.ToPoint2d()).ToArray()),
                normal: Vector3d.ZAxis);

            var result = Draw.AddToCurrentSpace(wipe);
            result.Draworder(DraworderOperation.MoveToTop);
            return result;
        }

        /// <summary>
        /// Draws wipeout from an entity.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Wipeout(ObjectId entityId)
        {
            var extent = entityId.QOpenForRead<Entity>().GeometricExtents;
            return Draw.Wipeout(extent);
        }

        /// <summary>
        /// Draws wipeout from extents.
        /// </summary>
        /// <param name="extents">The extents.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Wipeout(Extents3d extents)
        {
            var a = new Point3d(extents.MinPoint.X, extents.MaxPoint.Y, 0);
            var b = new Point3d(extents.MaxPoint.X, extents.MinPoint.Y, 0);
            return Draw.Wipeout(extents.MinPoint, a, extents.MaxPoint, b, extents.MinPoint);
        }

        /// <summary>
        /// Inserts block reference.
        /// </summary>
        /// <param name="blockName"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static ObjectId Insert(string blockName, Point3d position, double rotation = 0, double scale = 1)
        {
            return NoDraw
                .Insert(
                    DbHelper.GetBlockId(blockName),
                    position,
                    rotation,
                    scale)
                .AddToCurrentSpace();
        }

        /// <summary>
        /// Defines a block given entities.
        /// </summary>
        /// <param name="entityIds"></param>
        /// <param name="blockName"></param>
        /// <returns></returns>
        public static ObjectId Block(IEnumerable<ObjectId> entityIds, string blockName)
        {
            return Draw.Block(
                entityIds,
                blockName,
                basePoint: entityIds.GetCenter());
        }

        /// <summary>
        /// Defines a block given entities.
        /// </summary>
        /// <param name="entityIds"></param>
        /// <param name="blockName"></param>
        /// <param name="basePoint"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public static ObjectId Block(IEnumerable<ObjectId> entityIds, string blockName, Point3d basePoint, bool overwrite = true)
        {
            return Draw.Block(
                entityIds.QOpenForRead<Entity>(),
                blockName,
                basePoint,
                overwrite);
        }

        /// <summary>
        /// Defines a block given entities. 
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="blockName"></param>
        /// <param name="basePoint"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public static ObjectId Block(IEnumerable<Entity> entities, string blockName, Point3d basePoint, bool overwrite = true)
        {
            var db = HostApplicationServices.WorkingDatabase;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var blockTable = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                if (blockTable.Has(blockName))
                {
                    var oldBlock = trans.GetObject(blockTable[blockName], OpenMode.ForRead) as BlockTableRecord;
                    if (!overwrite)
                    {
                        Interaction.Write($"Block '{blockName}' already exists and was not overwritten.");
                        return oldBlock.Id;
                    }

                    blockTable.UpgradeOpen();
                    oldBlock.UpgradeOpen();
                    oldBlock.Erase();
                }

                var block = new BlockTableRecord
                {
                    Name = blockName
                };

                foreach (var entity in entities)
                {
                    var copy = entity.Clone() as Entity;
                    copy.TransformBy(Matrix3d.Displacement(-basePoint.GetAsVector()));
                    block.AppendEntity(copy);
                }

                var result = blockTable.Add(block);
                trans.AddNewlyCreatedDBObject(block, true);
                trans.Commit();
                return result;
            }
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
            Draw.Block(entities, blockName, blockBasePoint, overwrite);
            return Draw.Insert(blockName, blockReferencePoint, rotation, scale);
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
        /// Adds an entity to the model space.
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        [Obsolete("Most of the time you should call AddToCurrentSpace().")]
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
        /// Adds an entity to current space.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <param name="db">The database.</param>
        /// <returns>The objected IDs.</returns>
        public static ObjectId AddToCurrentSpace(this Entity entity, Database db = null)
        {
            if (db == null)
            {
                db = HostApplicationServices.WorkingDatabase;
            }

            using (var trans = db.TransactionManager.StartTransaction())
            {
                var currentSpace = (BlockTableRecord)trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite, false);
                var id = currentSpace.AppendEntity(entity);
                trans.AddNewlyCreatedDBObject(entity, true);
                trans.Commit();
                return id;
            }
        }

        /// <summary>
        /// Adds entities to current space.
        /// </summary>
        /// <param name="entities">The entities to add.</param>
        /// <param name="db">The database.</param>
        /// <returns>The objected IDs.</returns>
        public static ObjectId[] AddToCurrentSpace(this IEnumerable<Entity> entities, Database db = null)
        {
            if (db == null)
            {
                db = HostApplicationServices.WorkingDatabase;
            }

            using (var trans = db.TransactionManager.StartTransaction())
            {
                var currentSpace = (BlockTableRecord)trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite, false);
                var ids = entities
                    .ToArray() // Truly get entities before moving on.
                    .Select(entity =>
                    {
                        var id = currentSpace.AppendEntity(entity);
                        trans.AddNewlyCreatedDBObject(entity, true);
                        return id;
                    })
                    .ToArray();

                trans.Commit();
                return ids;
            }
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
        /// Creates a line.
        /// </summary>
        /// <param name="point1">The point 1.</param>
        /// <param name="point2">The point 2.</param>
        /// <returns>The result.</returns>
        public static Line Line(Point3d point1, Point3d point2)
        {
            return new Line(point1, point2);
        }

        /// <summary>
        /// Creates multiple lines from a sequence of points.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns>The result.</returns>
        public static Line[] Line(params Point3d[] points)
        {
            return Enumerable
                .Range(0, points.Length - 1)
                .Select(x => Line(points[x], points[x + 1]))
                .ToArray();
        }

        /// <summary>
        /// Creates an arc from 3 points.
        /// </summary>
        /// <param name="point1">The point 1.</param>
        /// <param name="point2">The point 2.</param>
        /// <param name="point3">The point 3.</param>
        /// <returns>The result.</returns>
        public static Arc Arc3P(Point3d point1, Point3d point2, Point3d point3)
        {
            var arc = new CircularArc3d(point1, point2, point3);
            return NoDraw.ArcFromGeometry(arc);
        }

        //public static Arc ArcSCE(Point3d start, Point3d center, Point3d end)
        //{
        //}

        /// <summary>
        /// Creates an arc from start, center, and angle.
        /// </summary>
        /// <param name="start">The start point.</param>
        /// <param name="center">The center point.</param>
        /// <param name="angle">The angle.</param>
        /// <returns>The result.</returns>
        public static Arc ArcSCA(Point3d start, Point3d center, double angle)
        {
            double radius = center.DistanceTo(start);
            var dir1 = start - center;
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
            var arc = new CircularArc3d(center, Vector3d.ZAxis, Vector3d.XAxis, radius, startangle, endangle);
            return NoDraw.ArcFromGeometry(arc);
        }

        //public static Arc ArcSCL(Point3d start, Point3d center, double length)
        //{
        //}

        //public static Arc ArcSEA(Point3d start, Point3d end, double angle)
        //{
        //}

        //public static Arc ArcSED(Point3d start, Point3d end, Vector3d dir)
        //{
        //}

        //public static Arc ArcSER(Point3d start, Point3d end, double radius)
        //{
        //}

        /// <summary>
        /// Creates an arc from a geometry arc.
        /// </summary>
        /// <param name="arc">The geometry arc.</param>
        /// <returns>The result.</returns>
        public static Arc ArcFromGeometry(CircularArc3d arc)
        {
            return new Arc(arc.Center, arc.Normal, arc.Radius, arc.StartAngle, arc.EndAngle);
        }

        /// <summary>
        /// Creates an arc from a geometry arc.
        /// </summary>
        /// <param name="arc">The geometry arc.</param>
        /// <returns>The result.</returns>
        public static Arc ArcFromGeometry(CircularArc2d arc)
        {
            return new Arc(arc.Center.ToPoint3d(), Vector3d.ZAxis, arc.Radius, arc.StartAngle, arc.EndAngle);
        }

        /// <summary>
        /// Creates a circle from center and radius.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="radius">The radius.</param>
        /// <returns>The result.</returns>
        public static Circle Circle(Point3d center, double radius)
        {
            return new Circle(center, Vector3d.ZAxis, radius);
        }

        /// <summary>
        /// Creates a circle from diameter ends.
        /// </summary>
        /// <param name="point1">The point 1.</param>
        /// <param name="point2">The point 2.</param>
        /// <returns>The result.</returns>
        public static Circle Circle(Point3d point1, Point3d point2)
        {
            return NoDraw.Circle(
                center: Point3d.Origin + 0.5 * ((point1 - Point3d.Origin) + (point2 - Point3d.Origin)),
                radius: 0.5 * point1.DistanceTo(point2));
        }

        /// <summary>
        /// Creates a circle from 3 points.
        /// </summary>
        /// <param name="point1">The point 1.</param>
        /// <param name="point2">The point 2.</param>
        /// <param name="point3">The point 3.</param>
        /// <returns>The result.</returns>
        public static Circle Circle(Point3d point1, Point3d point2, Point3d point3)
        {
            var geo = new CircularArc3d(point1, point2, point3);
            return NoDraw.Circle(geo);
        }

        /// <summary>
        /// Creates a circle from a geometry circle.
        /// </summary>
        /// <param name="circle">The geometry circle.</param>
        /// <returns>The result.</returns>
        public static Circle Circle(CircularArc3d circle)
        {
            return new Circle(circle.Center, circle.Normal, circle.Radius);
        }

        /// <summary>
        /// Creates a circle from a geometry circle.
        /// </summary>
        /// <param name="circle">The geometry circle.</param>
        /// <returns>The result.</returns>
        public static Circle Circle(CircularArc2d circle)
        {
            return new Circle(circle.Center.ToPoint3d(), Vector3d.ZAxis, circle.Radius);
        }

        /// <summary>
        /// Creates an ellipse by center, endX, and radiusY.
        /// </summary>
        /// <remarks>
        /// The ellipse will be created on the XY plane.
        /// </remarks>
        /// <param name="center">The center.</param>
        /// <param name="endX">The intersection point of the ellipse and its X axis.</param>
        /// <param name="radiusY">The Y radius.</param>
        /// <returns>The result.</returns>
        public static Ellipse Ellipse(Point3d center, Point3d endX, double radiusY)
        {
            var radiusRatio = center.DistanceTo(endX) / radiusY;
            var axisX = endX - center;
            if (center.DistanceTo(endX) > radiusY)
            {
                radiusRatio = radiusY / center.DistanceTo(endX);
            }
            else
            {
                axisX = axisX.RotateBy(Math.PI / 2.0, Vector3d.ZAxis);
                axisX = axisX.MultiplyBy(radiusY / center.DistanceTo(endX));
            }

            return new Ellipse(
                center: center,
                unitNormal: Vector3d.ZAxis,
                majorAxis: axisX,
                radiusRatio: radiusRatio,
                startAngle: 0,
                endAngle: 2 * Math.PI);
        }

        /// <summary>
        /// Creates a polyline by a sequence of points.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns>The result.</returns>
        public static Polyline Pline(params Point3d[] points)
        {
            return NoDraw.Pline(points.ToList());
        }

        /// <summary>
        /// Creates a polyline by a sequence of points and a global width.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="globalWidth">The global width. Default is 0.</param>
        /// <returns>The result.</returns>
        public static Polyline Pline(IEnumerable<Point3d> points, double globalWidth = 0)
        {
            return NoDraw.Pline(
                vertices: points.Select(point => (point, 0d)).ToList(),
                globalWidth: globalWidth);
        }

        /// <summary>
        /// Creates a polyline by a sequence of vertices (position + bulge).
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="globalWidth">The global width. Default is 0.</param>
        /// <returns>The result.</returns>
        public static Polyline Pline(List<(Point3d, double)> vertices, double globalWidth = 0)
        {
            var poly = new Polyline();
            Enumerable
                .Range(0, vertices.Count)
                .ToList().ForEach(index => poly.AddVertexAt(
                    index: index,
                    pt: vertices[index].Item1.ToPoint2d(),
                    bulge: vertices[index].Item2,
                    startWidth: globalWidth,
                    endWidth: globalWidth));

            return poly;
        }

        /// <summary>
        /// Creates a spline by fit points.
        /// </summary>
        /// <param name="points">The points to fit.</param>
        /// <returns>The result.</returns>
        public static Spline SplineFit(Point3d[] points)
        {
            return new Spline(
                point: new Point3dCollection(points),
                order: 3,
                fitTolerance: Consts.Epsilon);
        }

        /// <summary>
        /// Creates a spline by control points.
        /// </summary>
        /// <param name="points">The control points.</param>
        /// <param name="closed">Whether to close the spline.</param>
        /// <returns>The result.</returns>
        public static Spline SplineCV(Point3d[] points, bool closed = false)
        {
            var controlPoints = new Point3dCollection(points);
            DoubleCollection knots;
            DoubleCollection weights;
            if (!closed)
            {
                knots = new DoubleCollection(Enumerable.Range(0, points.Length - 2).Select(index => (double)index).ToArray());
                knots.Insert(0, 0);
                knots.Insert(0, 0);
                knots.Insert(0, 0);
                knots.Add(points.Length - 3);
                knots.Add(points.Length - 3);
                knots.Add(points.Length - 3);
                weights = new DoubleCollection(Enumerable.Repeat(1, points.Length).Select(index => (double)index).ToArray());
            }
            else
            {
                controlPoints.Add(points[0]);
                controlPoints.Add(points[1]);
                controlPoints.Add(points[2]);
                knots = new DoubleCollection(Enumerable.Range(0, points.Length + 7).Select(index => (double)index).ToArray());
                weights = new DoubleCollection(Enumerable.Repeat(1, points.Length + 3).Select(index => (double)index).ToArray());
            }

            return new Spline(
                degree: 3,
                rational: true,
                closed: closed,
                periodic: closed,
                controlPoints: controlPoints,
                knots: knots,
                weights: weights,
                controlPointTolerance: 0,
                knotTolerance: 0);
        }

        /// <summary>
        /// Creates a rectangle.
        /// </summary>
        /// <param name="point1">The point 1.</param>
        /// <param name="point2">The point 2.</param>
        /// <returns>The result.</returns>
        public static Polyline Rectang(Point3d point1, Point3d point2)
        {
            var result = NoDraw.Pline(new[]
            {
                point1,
                new Point3d(point1.X, point2.Y, 0),
                point2,
                new Point3d(point2.X, point1.Y, 0)
            });
            result.Closed = true;
            return result;
        }

        /// <summary>
        /// Creates a DT.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="height">The height.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="centerAligned">Whether to center align.</param>
        /// <param name="textStyle">The text style name.</param>
        /// <returns>The result.</returns>
        public static DBText Text(string text, double height, Point3d position, double rotation = 0, bool centerAligned = false, string textStyle = Consts.TextStyleName)
        {
            var textStyleId = DbHelper.GetTextStyleId(textStyle);
            var style = textStyleId.QOpenForRead<TextStyleTableRecord>();
            var dbText = new DBText
            {
                TextString = text,
                Position = position,
                Rotation = rotation,
                TextStyleId = textStyleId,
                Height = height,
                Oblique = style.ObliquingAngle,
                WidthFactor = style.XScale
            };

            if (centerAligned) // todo: centerAligned=true makes DT vanished
            {
                dbText.HorizontalMode = TextHorizontalMode.TextCenter;
                dbText.VerticalMode = TextVerticalMode.TextVerticalMid;
            }

            return dbText;
        }

        /// <summary>
        /// Creates an MT.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="height">The height.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="centerAligned">Whether to center align.</param>
        /// <param name="width">The width.</param>
        /// <returns>The result.</returns>
        public static MText MText(string text, double height, Point3d position, double rotation = 0, bool centerAligned = false, double width = 0)
        {
            var mText = new MText
            {
                Contents = text,
                TextHeight = height,
                Location = position,
                Rotation = rotation,
                Width = width
            };

            if (centerAligned)
            {
                mText.Move(mText.Location - mText.GetCenter());
            }

            return mText;
        }

        /// <summary>
        /// Creates a point.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>The result.</returns>
        public static DBPoint Point(Point3d position)
        {
            return new DBPoint(position);
        }

        /// <summary>
        /// Creates a block reference.
        /// </summary>
        /// <param name="blockTableRecordId">The block table record ID.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="scale">The scale.</param>
        /// <returns>The result.</returns>
        public static BlockReference Insert(ObjectId blockTableRecordId, Point3d position, double rotation = 0, double scale = 1)
        {
            return new BlockReference(position, blockTableRecordId)
            {
                Rotation = rotation,
                ScaleFactors = new Scale3d(scale)
            };
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
