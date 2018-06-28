using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

namespace Dreambuild.AutoCAD
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
                if (newEntity is DBPoint point)
                {
                    point.Position = position;
                }
                else if (newEntity is BlockReference insert)
                {
                    insert.Position = position;
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
        /// <returns>The object ID.</returns>
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
        /// <returns>The object ID.</returns>
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
            var centroid = new Point3d(
                pointList.Average(p => p.X),
                pointList.Average(p => p.Y),
                pointList.Average(p => p.Z));

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
        /// Draws hatch by closed area.
        /// </summary>
        /// <param name="loopIds">The loop IDs.</param>
        /// <param name="hatchName">The hatch name.</param>
        /// <param name="scale">The scale.</param>
        /// <param name="angle">The angle.</param>
        /// <param name="associative">Whether it is associative.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Hatch(ObjectId[] loopIds, string hatchName = "SOLID", double scale = 1, double angle = 0, bool associative = false)
        {
            var db = DbHelper.GetDatabase(loopIds);
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
                loopIds.ForEach(loop => hatch.AppendLoop(
                    HatchLoopTypes.External,
                    new ObjectIdCollection(new[] { loop })));

                hatch.EvaluateHatch(true);

                trans.Commit();
                return result;
            }
        }

        /// <summary>
        /// Draws hatch by a sequence of points.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="hatchName">The hatch name.</param>
        /// <param name="scale">The scale.</param>
        /// <param name="angle">The angle.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Hatch(IEnumerable<Point3d> points, string hatchName = "SOLID", double scale = 1, double angle = 0)
        {
            var pts = points.ToList();
            if (pts.First() != pts.Last())
            {
                pts.Add(pts.First());
            }
            var loop = Draw.Pline(pts);
            var result = Draw.Hatch(new[] { loop }, hatchName, scale, angle);
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
        /// <param name="blockName">The block name.</param>
        /// <param name="position">The insert position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="scale">The scale.</param>
        /// <returns>The object ID.</returns>
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
        /// <param name="entityIds">The entity IDs.</param>
        /// <param name="blockName">The block name.</param>
        /// <returns>The block table record ID.</returns>
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
        /// <param name="entityIds">The entity IDs.</param>
        /// <param name="blockName">The block name.</param>
        /// <param name="basePoint">The base point.</param>
        /// <param name="overwrite">Whether to overwrite.</param>
        /// <returns>The block table record ID.</returns>
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
        /// <param name="entities">The entities.</param>
        /// <param name="blockName">The block name.</param>
        /// <param name="basePoint">The base point.</param>
        /// <param name="overwrite">Whether to overwrite.</param>
        /// <returns>The block table record ID.</returns>
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
        /// <param name="entities">The entities.</param>
        /// <param name="blockName">The block name.</param>
        /// <param name="blockBasePoint">The block base point.</param>
        /// <param name="insertPosition">The insert position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="scale">The scale.</param>
        /// <param name="overwrite">Whether to overwrite.</param>
        /// <returns>The insert object ID.</returns>
        public static ObjectId CreateBlockAndInsertReference(IEnumerable<Entity> entities, string blockName, Point3d blockBasePoint, Point3d insertPosition, double rotation = 0, double scale = 1, bool overwrite = true)
        {
            Draw.Block(entities, blockName, blockBasePoint, overwrite);
            return Draw.Insert(blockName, insertPosition, rotation, scale);
        }

        /// <summary>
        /// Defines a block by copying from another DWG.
        /// </summary>
        /// <param name="blockName">The block name.</param>
        /// <param name="sourceDwg">The source DWG.</param>
        /// <returns>The block table record ID.</returns>
        public static ObjectId BlockInDwg(string blockName, string sourceDwg)
        {
            var db = HostApplicationServices.WorkingDatabase;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                if (blockTable.Has(blockName))
                {
                    return blockTable[blockName];
                }

                var sourceDb = new Database(false, false);
                sourceDb.ReadDwgFile(sourceDwg, FileOpenMode.OpenForReadAndAllShare, true, string.Empty);
                var blockId = DbHelper.GetBlockId(blockName, sourceDb);
                if (blockId == ObjectId.Null)
                {
                    return ObjectId.Null;
                }

                var tempDb = sourceDb.Wblock(blockId);
                var result = db.Insert(blockName, tempDb, false);
                trans.Commit();
                return result;
            }
        }

        /// <summary>
        /// Defines a block by taking another DWG as a whole.
        /// </summary>
        /// <param name="blockName">The block name.</param>
        /// <param name="sourceDwg">The source DWG.</param>
        /// <returns>The block table record ID.</returns>
        public static ObjectId BlockOfDwg(string blockName, string sourceDwg)
        {
            var sourceDb = new Database(false, false);
            sourceDb.ReadDwgFile(sourceDwg, FileOpenMode.OpenForReadAndAllShare, true, string.Empty);
            return HostApplicationServices.WorkingDatabase.Insert(blockName, sourceDb, false);
        }

        /// <summary>
        /// Draws a table.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="title">The title.</param>
        /// <param name="contents">The contents.</param>
        /// <param name="rowHeight">The row height.</param>
        /// <param name="columnWidth">The column width.</param>
        /// <param name="textHeight">The text height.</param>
        /// <param name="textStyle">The text style.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Table(Point3d position, string title, List<List<string>> contents, double rowHeight, double columnWidth, double textHeight, string textStyle = Consts.TextStyleName)
        {
            var tb = new Table
            {
                TableStyle = HostApplicationServices.WorkingDatabase.Tablestyle,
                Position = position
            };

            var numRow = contents.Count + 1;
            tb.InsertRows(0, rowHeight, numRow);
            var numCol = contents.Max(row => row.Count);
            tb.InsertColumns(0, columnWidth, numCol);
            tb.DeleteRows(numRow, 1);
            tb.DeleteColumns(numCol, 1);
            tb.SetRowHeight(rowHeight);
            tb.SetColumnWidth(columnWidth);
            tb.Cells.TextHeight = textHeight;
            tb.Cells.TextStyleId = DbHelper.GetTextStyleId(textStyle);

            tb.MergeCells(CellRange.Create(tb, 0, 0, 0, numCol - 1));
            tb.Cells[0, 0].TextString = title;
            for (var i = 0; i < tb.Rows.Count - 1; i++)
            {
                for (var j = 0; j < tb.Columns.Count; j++)
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
        /// Draws a polygon mesh.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="m">The m value.</param>
        /// <param name="n">The n value.</param>
        /// <param name="mClosed">Whether to close mesh in m dimension.</param>
        /// <param name="nClosed">Whether to close mesh in n dimension.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId PolygonMesh(List<Point3d> points, int m, int n, bool mClosed = false, bool nClosed = false)
        {
            var mesh = new PolygonMesh(PolyMeshType.SimpleMesh, m, n, new Point3dCollection(points.ToArray()), mClosed, nClosed);
            return mesh.AddToCurrentSpace();
        }

        #endregion

        #region dimensions

        /// <summary>
        /// Draws a linear dimension.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="dim">The dim.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Dimlin(Point3d start, Point3d end, Point3d dim)
        {
            double dist = start.DistanceTo(end);
            var ad = new AlignedDimension(start, end, dim, dist.ToString(), HostApplicationServices.WorkingDatabase.Dimstyle);
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
            db = db ?? HostApplicationServices.WorkingDatabase;
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
            db = db ?? HostApplicationServices.WorkingDatabase;
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
                .ForEach(index => poly.AddVertexAt(
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
        /// Moves an entity.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="displacement">The displacement.</param>
        public static void Move(this ObjectId entityId, Vector3d displacement)
        {
            using (var trans = entityId.Database.TransactionManager.StartTransaction())
            {
                var entity = trans.GetObject(entityId, OpenMode.ForWrite) as Entity;
                entity.TransformBy(Matrix3d.Displacement(displacement));
                trans.Commit();
            }
        }

        /// <summary>
        /// Moves an entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="displacement">The displacement.</param>
        public static void Move(this Entity entity, Vector3d displacement)
        {
            entity.TransformBy(Matrix3d.Displacement(displacement));
        }

        /// <summary>
        /// Copies an entity given a list of displacement.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="displacements">The displacements.</param>
        /// <returns>The object IDs.</returns>
        public static ObjectId[] Copy(this ObjectId entityId, IEnumerable<Vector3d> displacements)
        {
            var db = entityId.Database;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var entity = trans.GetObject(entityId, OpenMode.ForWrite) as Entity;
                var currentSpace = (BlockTableRecord)trans.GetObject(db.CurrentSpaceId, OpenMode.ForWrite, false);
                var copyIds = displacements
                    .Select(x =>
                    {
                        var copy = entity.Clone() as Entity;
                        copy.TransformBy(Matrix3d.Displacement(x));
                        var id = currentSpace.AppendEntity(copy);
                        trans.AddNewlyCreatedDBObject(copy, true);
                        return id;
                    })
                    .ToArray();

                trans.Commit();
                return copyIds;
            }
        }

        /// <summary>
        /// Copies an entity given a base point and a list of new points.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="basePoint">The base point.</param>
        /// <param name="newPoints">The new points.</param>
        /// <returns>The object IDs.</returns>
        public static ObjectId[] Copy(this ObjectId entityId, Point3d basePoint, IEnumerable<Point3d> newPoints)
        {
            return entityId.Copy(newPoints.Select(newPoint => newPoint - basePoint));
        }

        /// <summary>
        /// Rotates an entity.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="center">The center.</param>
        /// <param name="angle">The angle.</param>
        /// <param name="axis">The axis. Default is the Z axis.</param>
        public static void Rotate(this ObjectId entityId, Point3d center, double angle, Vector3d? axis = null)
        {
            using (var trans = entityId.Database.TransactionManager.StartTransaction())
            {
                var entity = trans.GetObject(entityId, OpenMode.ForWrite) as Entity;
                entity.TransformBy(Matrix3d.Rotation(angle, axis ?? Vector3d.ZAxis, center));
                trans.Commit();
            }
        }

        /// <summary>
        /// Scales an entity.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="basePoint">The base point.</param>
        /// <param name="scale">The scale.</param>
        public static void Scale(this ObjectId entityId, Point3d basePoint, double scale)
        {
            using (var trans = entityId.Database.TransactionManager.StartTransaction())
            {
                var entity = trans.GetObject(entityId, OpenMode.ForWrite) as Entity;
                entity.TransformBy(Matrix3d.Scaling(scale, basePoint));
                trans.Commit();
            }
        }

        /// <summary>
        /// Offsets a curve.
        /// </summary>
        /// <param name="curveId">The curve ID.</param>
        /// <param name="distance">The offset distance.</param>
        /// <param name="side">A point to indicate which side.</param>
        /// <returns>The objecct ID.</returns>
        public static ObjectId Offset(this ObjectId curveId, double distance, Point3d side)
        {
            var curve = curveId.QOpenForRead<Curve>();
            var curve1 = curve.GetOffsetCurves(-distance)[0] as Curve;
            var curve2 = curve.GetOffsetCurves(distance)[0] as Curve;
            return Draw.AddToCurrentSpace(curve1.GetDistToPoint(side) < curve2.GetDistToPoint(side)
                ? curve1
                : curve2);
        }

        /// <summary>
        /// Mirrors an entity.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="axis">The axis.</param>
        /// <param name="copy">Whether to copy.</param>
        /// <returns>The object ID.</returns>
        public static ObjectId Mirror(this ObjectId entityId, Line axis, bool copy = true)
        {
            var entity = entityId.QOpenForRead<Entity>();
            var axisLine = new Line3d(axis.StartPoint, axis.EndPoint);
            var mirror = entity.Clone() as Entity;
            mirror.TransformBy(Matrix3d.Mirroring(axisLine));
            if (!copy)
            {
                entityId.Erase();
            }
            return Draw.AddToCurrentSpace(mirror);
        }

        /// <summary>
        /// Breaks a curve.
        /// </summary>
        /// <param name="curveId">The curve ID.</param>
        /// <param name="point1">The point 1.</param>
        /// <param name="point2">The point 2.</param>
        /// <returns>The object IDs.</returns>
        public static ObjectId[] Break(this ObjectId curveId, Point3d point1, Point3d point2)
        {
            using (var trans = curveId.Database.TransactionManager.StartTransaction())
            {
                var curve = trans.GetObject(curveId, OpenMode.ForRead) as Curve;
                var param1 = curve.GetParamAtPointX(point1);
                var param2 = curve.GetParamAtPointX(point2);
                var splits = curve.GetSplitCurves(new DoubleCollection(new[] { param1, param2 }));
                var breaks = splits.Cast<Entity>();
                return new[] { breaks.First().ObjectId, breaks.Last().ObjectId };
            }
        }

        /// <summary>
        /// Breaks a curve at point.
        /// </summary>
        /// <param name="curveId">The curve ID.</param>
        /// <param name="position">The position.</param>
        /// <returns>The object IDs.</returns>
        public static ObjectId[] Break(this ObjectId curveId, Point3d position)
        {
            return curveId.Break(position, position);
        }

        #endregion

        #region database

        /// <summary>
        /// Explodes an entity.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <returns>The object IDs.</returns>
        public static ObjectId[] Explode(this ObjectId entityId)
        {
            var entity = entityId.QOpenForRead<Entity>();
            var results = new DBObjectCollection();
            entity.Explode(results);
            entityId.Erase();
            return results
                .Cast<Entity>()
                .Select(newEntity => newEntity.AddToCurrentSpace())
                .ToArray(); ;
        }

        /// <summary>
        /// Erases an entity.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        public static void Erase(this ObjectId entityId)
        {
            using (var trans = entityId.Database.TransactionManager.StartTransaction())
            {
                var entity = trans.GetObject(entityId, OpenMode.ForWrite);
                entity.Erase();
                trans.Commit();
            }
        }

        /// <summary>
        /// Deletes a group and erases the entities.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        public static void EraseGroup(this ObjectId groupId)
        {
            DbHelper
                .GetEntityIdsInGroup(groupId)
                .Cast<ObjectId>()
                .QForEach(x => x.Erase());

            groupId.QOpenForWrite(x => x.Erase());
        }

        /// <summary>
        /// Groups entities.
        /// </summary>
        /// <param name="entityIds">The entity IDs.</param>
        /// <param name="name">The group name.</param>
        /// <param name="selectable">Whether to allow select.</param>
        /// <returns>The group ID.</returns>
        public static ObjectId Group(this IEnumerable<ObjectId> entityIds, string name = "*", bool selectable = true)
        {
            var db = DbHelper.GetDatabase(entityIds);
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var groupDict = (DBDictionary)trans.GetObject(db.GroupDictionaryId, OpenMode.ForWrite);
                var group = new Group(name, selectable);
                foreach (var id in entityIds)
                {
                    group.Append(id);
                }
                var result = groupDict.SetAt(name, group);
                trans.AddNewlyCreatedDBObject(group, true); // false with no commit?
                trans.Commit();
                return result;
            }
        }

        /// <summary>
        /// Appends entities to group.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <param name="entityIds">The entity IDs.</param>
        public static void AppendToGroup(ObjectId groupId, params ObjectId[] entityIds)
        {
            using (var trans = DbHelper.GetDatabase(entityIds).TransactionManager.StartTransaction())
            {
                var group = trans.GetObject(groupId, OpenMode.ForWrite) as Group;
                if (group != null)
                {
                    group.Append(new ObjectIdCollection(entityIds));
                }
                trans.Commit();
            }
        }

        /// <summary>
        /// Ungroups a group.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        public static void Ungroup(ObjectId groupId)
        {
            var groupDictId = groupId.Database.GroupDictionaryId;
            groupDictId.QOpenForWrite<DBDictionary>(groupDict => groupDict.Remove(groupId));
            groupId.Erase();
        }

        /// <summary>
        /// Ungroups a group by entities.
        /// </summary>
        /// <param name="entityIds">The entity IDs.</param>
        /// <returns>The number of groups ungrouped.</returns>
        public static int Ungroup(IEnumerable<ObjectId> entityIds)
        {
            var groupDict = DbHelper.GetDatabase(entityIds).GroupDictionaryId.QOpenForRead<DBDictionary>();
            var count = 0;
            foreach (var entry in groupDict)
            {
                var group = entry.Value.QOpenForRead<Group>();
                if (entityIds.Any(entityId => group.Has(entityId.QOpenForRead<Entity>())))
                {
                    Modify.Ungroup(entry.Value);
                    count++;
                }
            }

            return count;
        }

        #endregion

        #region feature

        //public static void Array(Entity entity, string arrayOpt)
        //{
        //}

        //public static void Fillet(Curve cv1, Curve cv2, string bevelOpt)
        //{
        //}

        //public static void Chamfer(Curve cv1, Curve cv2, string bevelOpt)
        //{
        //}

        //public static void Trim(Curve[] baseCurves, Curve cv, Point3d[] p)
        //{
        //}

        //public static void Extend(Curve[] baseCurves, Curve cv, Point3d[] p)
        //{
        //}

        /// <summary>
        /// Sets draworder.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="operation">The operation.</param>
        public static void Draworder(this ObjectId entityId, DraworderOperation operation)
        {
            entityId.Database.BlockTableId.QOpenForWrite<BlockTable>(blockTable =>
            {
                blockTable[BlockTableRecord.ModelSpace].QOpenForWrite<BlockTableRecord>(blockTableRecord =>
                {
                    blockTableRecord.DrawOrderTableId.QOpenForWrite<DrawOrderTable>(drawOrderTable =>
                    {
                        switch (operation)
                        {
                            case DraworderOperation.MoveToTop:
                                drawOrderTable.MoveToTop(new ObjectIdCollection { entityId });
                                break;
                            case DraworderOperation.MoveToBottom:
                                drawOrderTable.MoveToBottom(new ObjectIdCollection { entityId });
                                break;
                            default:
                                break;
                        }
                    });
                });
            });
        }

        /// <summary>
        /// Sets draworder.
        /// </summary>
        /// <param name="entityIds">The entity IDs.</param>
        /// <param name="operation">The operation.</param>
        public static void Draworder(this IEnumerable<ObjectId> entityIds, DraworderOperation operation)
        {
            DbHelper
                .GetDatabase(entityIds)
                .BlockTableId
                .QOpenForWrite<BlockTable>(blockTable =>
                {
                    blockTable[BlockTableRecord.ModelSpace].QOpenForWrite<BlockTableRecord>(blockTableRecord =>
                    {
                        blockTableRecord.DrawOrderTableId.QOpenForWrite<DrawOrderTable>(drawOrderTable =>
                        {
                            switch (operation)
                            {
                                case DraworderOperation.MoveToTop:
                                    drawOrderTable.MoveToTop(new ObjectIdCollection(entityIds.ToArray()));
                                    break;
                                case DraworderOperation.MoveToBottom:
                                    drawOrderTable.MoveToBottom(new ObjectIdCollection(entityIds.ToArray()));
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
        /// Updates a text style.
        /// </summary>
        /// <param name="fontFamily">The font family. e.g."宋体""@宋体"</param>
        /// <param name="textHeight">The text height.</param>
        /// <param name="italicAngle">The italic angle.</param>
        /// <param name="xScale">The X scale.</param>
        /// <param name="vertical">Use vertical.</param>
        /// <param name="bigFont">Use big font.</param>
        /// <param name="textStyleName">The text style name.</param>
        /// <returns>The text style ID.</returns>
        public static ObjectId TextStyle(string fontFamily, double textHeight, double italicAngle = 0, double xScale = 1, bool vertical = false, string bigFont = "", string textStyleName = Consts.TextStyleName)
        {
            var result = DbHelper.GetTextStyleId(textStyleName, true);
            result.QOpenForWrite<TextStyleTableRecord>(tstr =>
            {
                tstr.Font = new FontDescriptor(fontFamily, false, false, 0, 34);
                tstr.TextSize = textHeight;
                tstr.ObliquingAngle = italicAngle;
                tstr.XScale = xScale;
                tstr.IsVertical = vertical;
                tstr.BigFontFileName = bigFont;
            });

            return result;
        }

        /// <summary>
        /// Sets layer.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="layer">The layer name.</param>
        public static void SetLayer(this ObjectId entityId, string layer)
        {
            entityId.QOpenForWrite<Entity>(entity =>
            {
                entity.LayerId = DbHelper.GetLayerId(layer);
            });
        }

        /// <summary>
        /// Sets line type.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="linetype">The line type.</param>
        /// <param name="linetypeScale">The scale.</param>
        public static void SetLinetype(this ObjectId entityId, string linetype, double linetypeScale = 1)
        {
            entityId.QOpenForWrite<Entity>(entity =>
            {
                entity.LinetypeId = DbHelper.GetLinetypeId(linetype);
                entity.LinetypeScale = linetypeScale;
            });
        }

        /// <summary>
        /// Sets dimension style.
        /// </summary>
        /// <param name="dimId">The dimension ID.</param>
        /// <param name="dimstyle">The style name.</param>
        public static void SetDimstyle(this ObjectId dimId, string dimstyle)
        {
            var dimstyleId = DbHelper.GetDimstyleId(dimstyle);
            using (var trans = dimId.Database.TransactionManager.StartTransaction())
            {
                var dim = trans.GetObject(dimId, OpenMode.ForWrite) as Dimension;
                dim.DimensionStyle = dimstyleId;
                trans.Commit();
            }
        }

        /// <summary>
        /// Sets text style for DT, MT, or DIM.
        /// </summary>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="textStyleName">The text style name.</param>
        public static void SetTextStyle(this ObjectId entityId, string textStyleName)
        {
            var textStyleId = DbHelper.GetTextStyleId(textStyleName);
            entityId.QOpenForWrite<Entity>(entity =>
            {
                if (entity is MText mText)
                {
                    mText.TextStyleId = textStyleId;
                }
                else if (entity is DBText text)
                {
                    text.TextStyleId = textStyleId;
                }
                else if (entity is Dimension dimension)
                {
                    dimension.TextStyleId = textStyleId;
                }
            });
        }

        #endregion
    }

    /// <summary>
    /// Boundary trace entity type.
    /// </summary>
    public enum BoundaryType
    {
        /// <summary>
        /// Trace with polyline.
        /// </summary>
        Polyline = 0,
        /// <summary>
        /// Trace with region.
        /// </summary>
        Region = 1
    }

    /// <summary>
    /// Draworder type.
    /// </summary>
    public enum DraworderOperation
    {
        /// <summary>
        /// No operation.
        /// </summary>
        None = 0,
        /// <summary>
        /// Move above.
        /// </summary>
        MoveAbove = 1,
        /// <summary>
        /// Move below.
        /// </summary>
        MoveBelow = 2,
        /// <summary>
        /// Move to top.
        /// </summary>
        MoveToTop = 3,
        /// <summary>
        /// Move to bottom.
        /// </summary>
        MoveToBottom = 4
    }
}
