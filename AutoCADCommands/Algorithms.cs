using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AutoCADCommands
{
    using IniData = Dictionary<string, Dictionary<string, string>>;

    /// <summary>
    /// Constants.
    /// </summary>
    public static class Consts
    {
        /// <summary>
        /// Universal tolerance.
        /// </summary>
        public const double Epsilon = 0.001;
        /// <summary>
        /// Default text style.
        /// </summary>
        public const string TextStyleName = "AutoCADCodePackTextStyle";
        /// <summary>
        /// FXD AppName for code.
        /// </summary>
        public const string AppNameForCode = "TongJiCode"; // like HTML tag name
        /// <summary>
        /// FXD AppName for ID.
        /// </summary>
        public const string AppNameForID = "TongJiID"; // like HTML id
        /// <summary>
        /// FXD AppName for name.
        /// </summary>
        public const string AppNameForName = "TongJiName"; // like HTML id or name
        /// <summary>
        /// FXD AppName for tags.
        /// </summary>
        public const string AppNameForTags = "TongJiTags"; // like HTML class
    }

    public static class Utils
    {
        #region File algorithms

        /// <summary>
        /// Gets the path relative to a base path.
        /// </summary>
        /// <param name="basePath">The base path.</param>
        /// <param name="path">The other path.</param>
        /// <returns>The relative path.</returns>
        /// <example>
        /// string strPath = GetRelativePath(@"C:\WINDOWS\system32", @"C:\WINDOWS\system\*.*" );
        /// //strPath == @"..\system\*.*"
        /// </example>
        public static string GetRelativePath(string basePath, string path)
        {
            if (!basePath.EndsWith("\\")) basePath += "\\";
            int intIndex = -1, intPos = basePath.IndexOf('\\');
            while (intPos >= 0)
            {
                intPos++;
                if (string.Compare(basePath, 0, path, 0, intPos, true) != 0) break;
                intIndex = intPos;
                intPos = basePath.IndexOf('\\', intPos);
            }

            if (intIndex >= 0)
            {
                path = path.Substring(intIndex);
                intPos = basePath.IndexOf("\\", intIndex);
                while (intPos >= 0)
                {
                    path = "..\\" + path;
                    intPos = basePath.IndexOf("\\", intPos + 1);
                }
            }
            return path;
        }

        /// <summary>
        /// Parses INI files
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="result">The result.</param>
        /// <returns>A value indicating if suceeded.</returns>
        public static bool ParseIniFile(string fileName, IniData result)
        {
            var groupPattern = @"^\[[^\[\]]+\]$";
            var dataPattern = @"^[^=]+=[^=]+$";

            var lines = File.ReadAllLines(fileName)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();

            var group = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                if (line.StartsWith("["))
                {
                    if (!Regex.IsMatch(line, groupPattern))
                    {
                        return false;
                    }
                    group = new Dictionary<string, string>();
                    var groupName = line.Trim('[', ']');
                    result.Add(groupName, group);
                }
                else
                {
                    if (!Regex.IsMatch(line, dataPattern))
                    {
                        return false;
                    }
                    var parts = line.Split('=').Select(x => x.Trim()).ToArray();
                    group.Add(parts[0], parts[1]);
                }
            }
            return true;
        }

        #endregion
    }

    /// <summary>
    /// Various algorithms.
    /// </summary>
    public static class Algorithms
    {
        #region Curve algorithms

        /// <summary>
        /// Gets the distance between a point and a curve.
        /// </summary>
        /// <param name="cv">The curve.</param>
        /// <param name="point">The point.</param>
        /// <param name="extend">Whether to extend curve if needed.</param>
        /// <returns>The distance.</returns>
        public static double GetDistToPoint(this Curve cv, Point3d point, bool extend = false)
        {
            return cv.GetClosestPointTo(point, extend).DistanceTo(point);
        }

        /// <summary>
        /// Gets the parameter at a specified distance on curve.
        /// </summary>
        /// <param name="cv">The curve.</param>
        /// <param name="dist">The distance.</param>
        /// <returns>The paramter.</returns>
        public static double GetParamAtDist(this Curve cv, double dist)
        {
            if (dist < 0)
            {
                dist = 0;
            }
            else if (dist > cv.GetDistanceAtParameter(cv.EndParam))
            {
                dist = cv.GetDistanceAtParameter(cv.EndParam);
            }
            return cv.GetParameterAtDistance(dist);
        }

        /// <summary>
        /// Gets the distance at a specified parameter on curve.
        /// </summary>
        /// <param name="cv">The curve.</param>
        /// <param name="param">The parameter.</param>
        /// <returns>The distance.</returns>
        public static double GetDistAtParam(this Curve cv, double param)
        {
            if (param < 0)
            {
                param = 0;
            }
            else if (param > cv.EndParam)
            {
                param = cv.EndParam;
            }
            return cv.GetDistanceAtParameter(param);
        }

        /// <summary>
        /// Gets the point at a specified parameter on curve.
        /// </summary>
        /// <param name="cv">The curve.</param>
        /// <param name="param">The parameter.</param>
        /// <returns>The point.</returns>
        public static Point3d GetPointAtParam(this Curve cv, double param)
        {
            if (param < 0)
            {
                param = 0;
            }
            else if (param > cv.EndParam)
            {
                param = cv.EndParam;
            }
            return cv.GetPointAtParameter(param);
        }

        /// <summary>
        /// Gets the point at a specified distance on curve.
        /// </summary>
        /// <param name="cv">The curve.</param>
        /// <param name="dist">The distance.</param>
        /// <returns>The point.</returns>
        public static Point3d GetPointAtDistX(this Curve cv, double dist)
        {
            if (dist < 0)
            {
                dist = 0;
            }
            else if (dist > cv.GetDistanceAtParameter(cv.EndParam))
            {
                dist = cv.GetDistanceAtParameter(cv.EndParam);
            }
            return cv.GetPointAtDist(dist);
        }

        /// <summary>
        /// Gets the distance at a specified point on curve.
        /// </summary>
        /// <param name="cv">The curve.</param>
        /// <param name="point">The point.</param>
        /// <returns>The distance.</returns>
        public static double GetDistAtPointX(this Curve cv, Point3d point)
        {
            if (point.DistanceTo(cv.StartPoint) < Consts.Epsilon)
            {
                return 0.0;
            }
            else if (point.DistanceTo(cv.EndPoint) < Consts.Epsilon)
            {
                return cv.GetDistAtPoint(cv.EndPoint);
            }
            else
            {
                try
                {
                    return cv.GetDistAtPoint(point);
                }
                catch
                {
                    return cv.GetDistAtPoint(cv.GetClosestPointTo(point, false));
                }
            }
        }

        /// <summary>
        /// Gets the parameter at a specified point on curve.
        /// </summary>
        /// <param name="cv">The curve.</param>
        /// <param name="point">The point.</param>
        /// <returns>The parameter.</returns>
        public static double GetParamAtPointX(this Curve cv, Point3d point)
        {
            if (point.DistanceTo(cv.StartPoint) < Consts.Epsilon)
            {
                return 0.0;
            }
            else if (point.DistanceTo(cv.EndPoint) < Consts.Epsilon)
            {
                return cv.GetParameterAtPoint(cv.EndPoint);
            }
            else
            {
                try
                {
                    return cv.GetParameterAtPoint(point);
                }
                catch
                {
                    return cv.GetParameterAtPoint(cv.GetClosestPointTo(point, false));
                }
            }
        }

        /// <summary>
        /// Gets subcurve from curve by distance interval.
        /// </summary>
        /// <param name="curve">The curve.</param>
        /// <param name="interval">The subcurve interval in distance.</param>
        /// <returns>The subcurve.</returns>
        public static Curve GetSubCurve(this Curve curve, Interv interval) // todo: Remove complex type from API
        {
            if (curve is Line)
            {
                curve = (curve as Line).ToPolyline();
            }
            else if (curve is Arc)
            {
                curve = (curve as Arc).ToPolyline();
            }
            double start = curve.GetParamAtDist(interval.Start);
            double end = curve.GetParamAtDist(interval.End);
            return curve.GetSubCurveByParams(new Interv(start, end));
        }

        /// <summary>
        /// Gets subcurve from curve by distance interval.
        /// </summary>
        /// <param name="curve">The curve.</param>
        /// <param name="interval">The subcurve interval in distance.</param>
        /// <returns>The subcurve.</returns>
        public static Curve GetSubCurve(this Curve curve, (double, double) interval)
        {
            return Algorithms.GetSubCurve(curve, new Interv(interval));
        }

        /// <summary>
        /// Gets subcurve from curve by parameter interval.
        /// </summary>
        /// <param name="curve">The curve.</param>
        /// <param name="interval">The subcurve interval in parameter.</param>
        /// <returns>The subcurve.</returns>
        public static Curve GetSubCurveByParams(this Curve curve, Interv interval)
        {
            if (curve is Line)
            {
                curve = (curve as Line).ToPolyline();
            }
            else if (curve is Arc)
            {
                curve = (curve as Arc).ToPolyline();
            }
            double start = interval.Start;
            double end = interval.End;
            double startDist = curve.GetDistAtParam(start);
            double endDist = curve.GetDistAtParam(end);

            //LogManager.Write("total", curve.EndParam);
            //LogManager.Write("start", start);
            //LogManager.Write("end", end);
            //LogManager.Write("type", curve.GetType());

            DBObjectCollection splits = curve.GetSplitCurves(new DoubleCollection(new double[] { start, end }));
            if (splits.Count == 3)
            {
                return splits[1] as Curve;
            }
            else
            {
                if (startDist == endDist)
                {
                    Point3d p = curve.GetPointAtParameter(start);
                    if (curve is Line)
                    {
                        return new Line(p, p);
                    }
                    else if (curve is Arc)
                    {
                        return new Arc(p, 0, 0, 0);
                    }
                    else if (curve is Polyline)
                    {
                        Polyline poly = new Polyline();
                        poly.AddVertexAt(0, p.ToPoint2d(), 0, 0, 0);
                        poly.AddVertexAt(0, p.ToPoint2d(), 0, 0, 0);
                        return poly;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    if (splits.Count == 2)
                    {
                        if (start == 0)
                        {
                            return splits[0] as Curve;
                        }
                        else
                        {
                            return splits[1] as Curve;
                        }
                    }
                    else // Count == 1
                    {
                        return splits[0] as Curve;
                    }
                }
            }
        }

        /// <summary>
        /// Gets subcurve from curve by parameter interval.
        /// </summary>
        /// <param name="curve">The curve.</param>
        /// <param name="interval">The subcurve interval in parameter.</param>
        /// <returns>The subcurve.</returns>
        public static Curve GetSubCurveByParams(this Curve curve, (double, double) interval)
        {
            return Algorithms.GetSubCurveByParams(curve, new Interv(interval));
        }

        /// <summary>
        /// Gets all points on curve whose parameters are an arithmetic sequence starting from 0.
        /// </summary>
        /// <param name="cv">The curve.</param>
        /// <param name="paramDelta">The parameter increment. Th default is 1, in which case the method returns all points on curve whose parameters are integres.</param>
        /// <returns>The points.</returns>
        public static IEnumerable<Point3d> GetPoints(this Curve cv, double paramDelta = 1)
        {
            for (var param = 0d; param <= cv.EndParam; param += paramDelta)
            {
                yield return cv.GetPointAtParam(param);
            }
        }

        /// <summary>
        /// Gets all points on curve whose distances (from start) are an arithmetic sequence starting from 0.
        /// </summary>
        /// <param name="cv">The curve.</param>
        /// <param name="distDelta">The dist increment.</param>
        /// <returns>The points.</returns>
        public static IEnumerable<Point3d> GetPointsByDist(this Curve cv, double distDelta)
        {
            for (var dist = 0d; dist <= cv.GetDistAtParam(cv.EndParam); dist += distDelta)
            {
                yield return cv.GetPointAtDistX(dist);
            }
        }

        /// <summary>
        /// Gets all vertices of a polyline.
        /// </summary>
        /// <remarks>
        /// For a polyline, the difference between this method and `GetPoints()` is when `IsClosed=true`.
        /// </remarks>
        /// <param name="poly">The polyline.</param>
        /// <returns>The points.</returns>
        public static IEnumerable<Point3d> GetPolyPoints(this Polyline poly)
        {
            for (int i = 0; i < poly.NumberOfVertices; i++)
            {
                yield return poly.GetPoint3dAt(i);
            }
        }

        /// <summary>
        /// Gets points that equally divide (parameter wise) a curve into `divs` (number of) segments.
        /// </summary>
        /// <param name="cv">The curve.</param>
        /// <param name="divs">The number of divisions.</param>
        /// <returns>The points.</returns>
        [Obsolete("This method has a design defect and will be removed.")]
        public static IEnumerable<Point3d> GetPoints(this Curve cv, int divs)
        {
            double div = cv.EndParam / divs;
            for (double i = 0; i < cv.EndParam + div; i += div)
            {
                yield return cv.GetPointAtParam(i);
            }
        }

        private static IEnumerable<Point3d> GetPolylineFitPointsImp(this Curve cv, int divsWhenArc)
        {
            var poly = cv as Polyline;
            if (poly == null)
            {
                yield return cv.StartPoint;
                yield return cv.EndPoint;
            }
            else
            {
                for (int i = 0; i < poly.EndParam - Consts.Epsilon; i++) // mod 20111101
                {
                    if (poly.GetBulgeAt(i) == 0)
                    {
                        yield return poly.GetPointAtParameter(i);
                    }
                    else
                    {
                        int divs = divsWhenArc == 0 ? (int)((Math.Atan(Math.Abs(poly.GetBulgeAt(i))) * 4) / (Math.PI / 18) + 4) : divsWhenArc;
                        // adding 4 in case extra small arcs, whose lengths might be huge.
                        // TODO: this is a design defect. We probably need to use fixed dist.
                        for (int j = 0; j < divs; j++)
                        {
                            yield return poly.GetPointAtParam(i + (double)j / divs);
                        }
                    }
                }
                yield return poly.GetPointAtParameter(poly.EndParam);
            }
        }

        /// <summary>
        /// Gets polyline fit points (in case of arcs).
        /// </summary>
        /// <param name="cv">The polyline.</param>
        /// <param name="divsWhenArc">Number of divisions for arcs. The default is 0 (smart).</param>
        /// <returns>The points.</returns>
        public static IEnumerable<Point3d> GetPolylineFitPoints(this Curve cv, int divsWhenArc = 0)
        {
            try
            {
                return Algorithms.GetPolylineFitPointsImp(cv, divsWhenArc).ToArray();
            }
            catch
            {
                throw new PolylineNeedsCleanupException();
            }
        }

        /// <summary>
        /// Gets subcurves by dividing a curve on points whose parameters are an arithmetic sequence starting from 0.
        /// </summary>
        /// <param name="cv">The curve.</param>
        /// <param name="paramDelta">The parameter increment. Th default is 1, in which case the method divides the curve on points whose parameters are integers.</param>
        /// <returns>The result curves.</returns>
        public static IEnumerable<Curve> GetSegments(this Curve cv, double paramDelta = 1)
        {
            for (var param = 0d; param < cv.EndParam; param += paramDelta)
            {
                yield return cv.GetSubCurveByParams((param, param + paramDelta));
            }
        }

        /// <summary>
        /// Gets subcurves by dividing a curve on points whose distances (from start) are an arithmetic sequence starting from 0.
        /// </summary>
        /// <param name="cv">The curve.</param>
        /// <param name="distDelta">The dist increment.</param>
        /// <returns>The result curves.</returns>
        public static IEnumerable<Curve> GetSegmentsByDist(this Curve cv, double distDelta)
        {
            for (var dist = 0d; dist < cv.GetDistAtParam(cv.EndParam); dist += distDelta)
            {
                yield return cv.GetSubCurve((dist, dist + distDelta)); // TODO: unify patterns of using "Param" and "Dist".
            }
        }

        /// <summary>
        /// Gets the minimum distance between two curves.
        /// </summary>
        /// <param name="cv1">The curve 1.</param>
        /// <param name="cv2">The curve 2.</param>
        /// <param name="divs">The number of divisions per curve used for calculating.</param>
        /// <returns>The distance.</returns>
        [Obsolete("The desgin has defect and the implementation is not optimized.")]
        public static double GetDistOfTwoCurve(Curve cv1, Curve cv2, int divs = 100)
        {
            var pts1 = cv1.GetPoints(divs);
            var pts2 = cv2.GetPoints(divs);
            return pts1.Min(p1 => pts2.Min(p2 => p1.DistanceTo(p2)));
        }

        #endregion

        #region Range algorithms

        /// <summary>
        /// Gets entity extents.
        /// </summary>
        /// <param name="entityIds">The entity IDs.</param>
        /// <returns>The result extents.</returns>
        public static Extents3d GetExtents(this IEnumerable<ObjectId> entityIds)
        {
            return Algorithms.GetExtents(entityIds.QOpenForRead<Entity>());
        }

        /// <summary>
        /// Gets entity extents.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns>The result extents.</returns>
        public static Extents3d GetExtents(this IEnumerable<Entity> entities)
        {
            var extent = entities.First().GeometricExtents;
            foreach (var ent in entities)
            {
                extent.AddExtents(ent.GeometricExtents);
            }
            return extent;
        }

        /// <summary>
        /// Gets the center of an Extents3d.
        /// </summary>
        /// <param name="extents">The extents.</param>
        /// <returns>The center.</returns>
        public static Point3d GetCenter(this Extents3d extents)
        {
            return Point3d.Origin + 0.5 * (extents.MinPoint.GetAsVector() + extents.MaxPoint.GetAsVector());
        }

        /// <summary>
        /// Gets the center of an Extents2d.
        /// </summary>
        /// <param name="extents">The extents.</param>
        /// <returns>The center.</returns>
        public static Point2d GetCenter(this Extents2d extents)
        {
            return Point2d.Origin + 0.5 * (extents.MinPoint.GetAsVector() + extents.MaxPoint.GetAsVector());
        }

        /// <summary>
        /// Scales an Extents3d.
        /// </summary>
        /// <param name="extents">The extents.</param>
        /// <param name="factor">The scale factor.</param>
        /// <returns>The result.</returns>
        public static Extents3d Expand(this Extents3d extents, double factor)
        {
            var center = extents.GetCenter();
            return new Extents3d(center + factor * (extents.MinPoint - center), center + factor * (extents.MaxPoint - center));
        }

        /// <summary>
        /// Inflates an Point3d into an Extents3d.
        /// </summary>
        /// <param name="center">The point.</param>
        /// <param name="size">The inflation size.</param>
        /// <returns>The result.</returns>
        public static Extents3d Expand(this Point3d center, double size) // newly 20130201
        {
            Vector3d move = new Vector3d(size / 2, size / 2, size / 2);
            return new Extents3d(center - move, center + move);
        }

        /// <summary>
        /// Scales an Extents2d.
        /// </summary>
        /// <param name="extents">The extents.</param>
        /// <param name="factor">The scale factor.</param>
        /// <returns>The result.</returns>
        public static Extents2d Expand(this Extents2d extents, double factor)
        {
            var center = extents.GetCenter();
            return new Extents2d(center + factor * (extents.MinPoint - center), center + factor * (extents.MaxPoint - center));
        }

        /// <summary>
        /// Inflates an Point2d into an Extents2d.
        /// </summary>
        /// <param name="center">The point.</param>
        /// <param name="size">The inflation size.</param>
        /// <returns>The result.</returns>
        public static Extents2d Expand(this Point2d center, double size)
        {
            var move = new Vector2d(size / 2, size / 2);
            return new Extents2d(center - move, center + move);
        }

        /// <summary>
        /// Determines if a point is in an extents.
        /// </summary>
        /// <param name="extents">The extents.</param>
        /// <param name="point">The point.</param>
        /// <returns>The result.</returns>
        public static bool IsPointIn(this Extents3d extents, Point3d point)
        {
            return point.X >= extents.MinPoint.X && point.X <= extents.MaxPoint.X
                && point.Y >= extents.MinPoint.Y && point.Y <= extents.MaxPoint.Y
                && point.Z >= extents.MinPoint.Z && point.Z <= extents.MaxPoint.Z;
        }

        /// <summary>
        /// Determines if a point is in an extents.
        /// </summary>
        /// <param name="extents">The extents.</param>
        /// <param name="point">The point.</param>
        /// <returns>The result.</returns>
        public static bool IsPointIn(this Extents2d extents, Point2d point)
        {
            return point.X >= extents.MinPoint.X && point.X <= extents.MaxPoint.X
                && point.Y >= extents.MinPoint.Y && point.Y <= extents.MaxPoint.Y;
        }

        /// <summary>
        /// Converts Extents3d to Extents2d.
        /// </summary>
        /// <param name="extents">The Extents3d.</param>
        /// <param name="x">The X value selector.</param>
        /// <param name="y">The Y value selector.</param>
        /// <returns>The result Extents2d.</returns>
        public static Extents2d ToExtents2d(
            this Extents3d extents,
            Func<Point3d, double> x = null,
            Func<Point3d, double> y = null)
        {
            if (x == null)
            {
                x = p => p.X;
            }

            if (y == null)
            {
                y = p => p.Y;
            }

            return new Extents2d(
                x(extents.MinPoint),
                y(extents.MinPoint),
                x(extents.MaxPoint),
                y(extents.MaxPoint));
        }

        /// <summary>
        /// Converts Extents2d to Extents3d.
        /// </summary>
        /// <param name="extents">The Extents2d.</param>
        /// <param name="x">The X value selector.</param>
        /// <param name="y">The Y value selector.</param>
        /// <param name="z">The Z value selector.</param>
        /// <returns>The result Extents3d.</returns>
        public static Extents3d ToExtents3d(
            this Extents2d extents,
            Func<Point2d, double> x = null,
            Func<Point2d, double> y = null,
            Func<Point2d, double> z = null)
        {
            if (x == null)
            {
                x = p => p.X;
            }

            if (y == null)
            {
                y = p => p.Y;
            }

            if (z == null)
            {
                z = p => 0;
            }

            var minPoint = new Point3d(x(extents.MinPoint), y(extents.MinPoint), z(extents.MinPoint));
            var maxPoint = new Point3d(x(extents.MaxPoint), y(extents.MaxPoint), z(extents.MaxPoint));
            return new Extents3d(minPoint, maxPoint);
        }

        /// <summary>
        /// Gets the center of multiple entities.
        /// </summary>
        /// <param name="entIds">The entity IDs.</param>
        /// <returns>The center.</returns>
        public static Point3d GetCenter(this IEnumerable<ObjectId> entIds)
        {
            return entIds.GetExtents().GetCenter();
        }

        /// <summary>
        /// Gets the center of multiple entities.
        /// </summary>
        /// <param name="ents">The entities.</param>
        /// <returns>The center.</returns>
        public static Point3d GetCenter(this IEnumerable<Entity> ents)
        {
            return ents.GetExtents().GetCenter();
        }

        /// <summary>
        /// Gets the center of an entity.
        /// </summary>
        /// <param name="entId">The entity ID.</param>
        /// <returns>The center.</returns>
        public static Point3d GetCenter(this ObjectId entId)
        {
            return entId.QOpenForRead<Entity>().GeometricExtents.GetCenter();
        }

        /// <summary>
        /// Gets the center of an entity.
        /// </summary>
        /// <param name="ent">The entity.</param>
        /// <returns>The center.</returns>
        public static Point3d GetCenter(this Entity ent)
        {
            return ent.GeometricExtents.GetCenter();
        }

        /// <summary>
        /// Gets the volume of an Extents3d.
        /// </summary>
        /// <param name="extents">The extents.</param>
        /// <returns>The volume.</returns>
        public static double GetVolume(this Extents3d extents)
        {
            return (extents.MaxPoint.X - extents.MinPoint.X) * (extents.MaxPoint.Y - extents.MinPoint.Y) * (extents.MaxPoint.Z - extents.MinPoint.Z);
        }

        /// <summary>
        /// Gets the area of an Extents3d.
        /// </summary>
        /// <param name="extents">The extents.</param>
        /// <returns>The area.</returns>
        [Obsolete("Use `.ToExtents2d().GetArea()` instead.")]
        public static double GetArea(this Extents3d extents) // newly 20130514
        {
            return (extents.MaxPoint.X - extents.MinPoint.X) * (extents.MaxPoint.Y - extents.MinPoint.Y);
        }

        /// <summary>
        /// Gets the area of an Extents2d.
        /// </summary>
        /// <param name="extents">The extents.</param>
        /// <returns>The area.</returns>
        public static double GetArea(this Extents2d extents) // newly 20130514
        {
            return (extents.MaxPoint.X - extents.MinPoint.X) * (extents.MaxPoint.Y - extents.MinPoint.Y);
        }

        #endregion

        #region Point algorithms

        /// <summary>
        /// Gets an empty Point3d
        /// </summary>
        public static Point3d NullPoint3d { get; } = new Point3d(double.NaN, double.NaN, double.NaN);

        /// <summary>
        /// Determines if a Point3d is empty.
        /// </summary>
        /// <param name="p">The point.</param>
        /// <returns>The result.</returns>
        public static bool IsNull(this Point3d p)
        {
            return double.IsNaN(p.X);
        }

        /// <summary>
        /// Converts Point3d to Point2d.
        /// </summary>
        /// <param name="point">The Point3d.</param>
        /// <returns>A Point2d.</returns>
        public static Point2d ToPoint2d(this Point3d point)
        {
            return new Point2d(point.X, point.Y);
        }

        /// <summary>
        /// Converts Point2d to Point3d.
        /// </summary>
        /// <param name="point">The Point2d.</param>
        /// <returns>A Point3d.</returns>
        public static Point3d ToPoint3d(this Point2d point)
        {
            return new Point3d(point.X, point.Y, 0);
        }

        /// <summary>
        /// Converts Vector2d to Vector3d.
        /// </summary>
        /// <param name="point">The Vector2d.</param>
        /// <returns>A Vector3d.</returns>
        public static Vector3d ToVector3d(this Vector2d point)
        {
            return new Vector3d(point.X, point.Y, 0);
        }

        /// <summary>
        /// Converts Vector3d to Vector2d.
        /// </summary>
        /// <param name="point">The Vector3d.</param>
        /// <returns>A Vector2d.</returns>
        public static Vector2d ToVector2d(this Vector3d point)
        {
            return new Vector2d(point.X, point.Y);
        }

        /// <summary>
        /// Gets the convex hull of multiple points.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <returns>The convex hull.</returns>
        public static List<Point3d> GetConvexHull(List<Point3d> source)
        {
            var points = new List<Point3d>();
            var collection = new List<Point3d>();
            var num = 0;
            source.Sort((p1, p2) => (p1.X - p2.X == 0) ? (int)(p1.Y - p2.Y) : (int)(p1.X - p2.X));

            points.Add(source[0]);
            points.Add(source[1]);
            for (num = 2; num <= (source.Count - 1); num++)
            {
                points.Add(source[num]);
                while ((points.Count >= 3) && !IsTurnRight(points[points.Count - 3], points[points.Count - 2], points[points.Count - 1]))
                {
                    points.RemoveAt(points.Count - 2);
                }
            }
            collection.Add(source[source.Count - 1]);
            collection.Add(source[source.Count - 2]);
            for (num = source.Count - 2; num >= 0; num--)
            {
                collection.Add(source[num]);
                while ((collection.Count >= 3) && !IsTurnRight(collection[collection.Count - 3], collection[collection.Count - 2], collection[collection.Count - 1]))
                {
                    collection.RemoveAt(collection.Count - 2);
                }
            }
            collection.RemoveAt(collection.Count - 1);
            collection.RemoveAt(0);
            points.AddRange(collection);
            return points;
        }

        private static bool IsTurnRight(Point3d px, Point3d py, Point3d pz)
        {
            double num = 0;
            num = ((pz.Y - py.Y) * (py.X - px.X)) - ((py.Y - px.Y) * (pz.X - py.X));
            return (num < 0f);
        }

        #endregion

        #region Polyline algorithms

        /// <summary>
        /// Determines if the polyline is self-intersecting.
        /// </summary>
        /// <param name="poly">The polyline.</param>
        /// <returns>The result.</returns>
        public static bool IsSelfIntersecting(this Polyline poly) // newly by WY 20130202
        {
            var points = poly.GetPolyPoints().ToList();
            for (int i = 0; i < points.Count - 3; i++)
            {
                var a1 = points[i].ToPoint2d();
                var a2 = points[i + 1].ToPoint2d();
                for (var j = i + 2; j < points.Count - 1; j++)
                {
                    var b1 = points[j].ToPoint2d();
                    var b2 = points[j + 1].ToPoint2d();
                    if (IsLineSegIntersect(a1, a2, b1, b2))
                    {
                        if (i == 0 && j == points.Count - 2)
                        {
                            // NOTE: If they happen to be the first and the last, check if polyline is closed. A closed polyline is not considered self-intersecting.
                            if (points.First().DistanceTo(points.Last()) > Consts.Epsilon)
                            {
                                return true;
                            }
                            continue;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the pseudo cross product (a.k.a. 'kross') of two Vector2ds.
        /// </summary>
        /// <param name="v1">The vector 1.</param>
        /// <param name="v2">The vector 2.</param>
        /// <returns>The pseudo cross product (a.k.a. 'kross').</returns>
        public static double Kross(this Vector2d v1, Vector2d v2)
        {
            return v1.X * v2.Y - v1.Y * v2.X;
        }

        /// <summary>
        /// Determines if two line segments intersect.
        /// </summary>
        /// <param name="a1">Line a point 1.</param>
        /// <param name="a2">Line a point 2.</param>
        /// <param name="b1">Line b point 1.</param>
        /// <param name="b2">Line b point 2.</param>
        /// <returns>The result.</returns>
        public static bool IsLineSegIntersect(Point2d a1, Point2d a2, Point2d b1, Point2d b2)
        {
            if ((a1 - a2).Kross(b1 - b2) == 0)
            {
                return false;
            }

            double lambda = 0;
            double miu = 0;

            if (b1.X == b2.X)
            {
                lambda = (b1.X - a1.X) / (a2.X - b1.X);
                double Y = (a1.Y + lambda * a2.Y) / (1 + lambda);
                miu = (Y - b1.Y) / (b2.Y - Y);
            }
            else if (a1.X == a2.X)
            {
                miu = (a1.X - b1.X) / (b2.X - a1.X);
                double Y = (b1.Y + miu * b2.Y) / (1 + miu);
                lambda = (Y - a1.Y) / (a2.Y - Y);
            }
            else if (b1.Y == b2.Y)
            {
                lambda = (b1.Y - a1.Y) / (a2.Y - b1.Y);
                double X = (a1.X + lambda * a2.X) / (1 + lambda);
                miu = (X - b1.X) / (b2.X - X);
            }
            else if (a1.Y == a2.Y)
            {
                miu = (a1.Y - b1.Y) / (b2.Y - a1.Y);
                double X = (b1.X + miu * b2.X) / (1 + miu);
                lambda = (X - a1.X) / (a2.X - X);
            }
            else
            {
                lambda = (b1.X * a1.Y - b2.X * a1.Y - a1.X * b1.Y + b2.X * b1.Y + a1.X * b2.Y - b1.X * b2.Y) / (-b1.X * a2.Y + b2.X * a2.Y + a2.X * b1.Y - b2.X * b1.Y - a2.X * b2.Y + b1.X * b2.Y);
                miu = (-a2.X * a1.Y + b1.X * a1.Y + a1.X * a2.Y - b1.X * a2.Y - a1.X * b1.Y + a2.X * b1.Y) / (a2.X * a1.Y - b2.X * a1.Y - a1.X * a2.Y + b2.X * a2.Y + a1.X * b2.Y - a2.X * b2.Y); // from Mathematica
            }

            bool result = false;
            if (lambda >= 0 || double.IsInfinity(lambda))
            {
                if (miu >= 0 || double.IsInfinity(miu))
                {
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// Cuts a closed polyline into two closed halves with a straight line
        /// </summary>
        /// <param name="loop">The closed polyline.</param>
        /// <param name="cut">The cutting line.</param>
        /// <returns>The result.</returns>
        public static Polyline[] CutLoopToHalves(Polyline loop, Line cut)
        {
            if (loop.EndPoint != loop.StartPoint)
            {
                return new Polyline[0];
            }
            var points = new Point3dCollection();
            loop.IntersectWith3264(cut, Autodesk.AutoCAD.DatabaseServices.Intersect.ExtendArgument, points);
            if (points.Count != 2)
            {
                return new Polyline[0];
            }
            double a, b;
            if (loop.GetParamAtPointX(points[0]) < loop.GetParamAtPointX(points[1]))
            {
                a = loop.GetParamAtPointX(points[0]);
                b = loop.GetParamAtPointX(points[1]);
            }
            else
            {
                a = loop.GetParamAtPointX(points[1]);
                b = loop.GetParamAtPointX(points[0]);
            }
            var poly1 = new Polyline();
            var poly2 = new Polyline();

            // The half without the polyline start/end point.
            poly2.AddVertexAt(0, loop.GetPointAtParameter(a).ToPoint2d(), loop.GetBulgeBetween(a, Math.Ceiling(a)), 0, 0);
            int i = 1;
            for (int n = (int)Math.Ceiling(a); n < b - 1; n++)
            {
                poly2.AddVertexAt(i, loop.GetPointAtParameter(n).ToPoint2d(), loop.GetBulgeAt(n), 0, 0);
                i++;
            }
            poly2.AddVertexAt(i, loop.GetPointAtParameter(Math.Floor(b)).ToPoint2d(), loop.GetBulgeBetween(Math.Floor(b), b), 0, 0);
            poly2.AddVertexAt(i + 1, loop.GetPointAtParameter(b).ToPoint2d(), 0, 0, 0);
            poly2.AddVertexAt(i + 2, loop.GetPointAtParameter(a).ToPoint2d(), 0, 0, 0);

            // The half with the polyline start/end point.
            poly1.AddVertexAt(0, loop.GetPointAtParameter(b).ToPoint2d(), loop.GetBulgeBetween(b, Math.Ceiling(b)), 0, 0);
            int j = 1;
            for (int n = (int)Math.Ceiling(b); n < loop.EndParam; n++)
            {
                poly1.AddVertexAt(j, loop.GetPointAtParameter(n).ToPoint2d(), loop.GetBulgeAt(n), 0, 0);
                j++;
            }
            for (int n = 0; n < a - 1; n++)
            {
                poly1.AddVertexAt(j, loop.GetPointAtParameter(n).ToPoint2d(), loop.GetBulgeAt(n), 0, 0);
                j++;
            }
            poly1.AddVertexAt(j, loop.GetPointAtParameter(Math.Floor(a)).ToPoint2d(), loop.GetBulgeBetween(Math.Floor(a), a), 0, 0);
            poly1.AddVertexAt(j + 1, loop.GetPointAtParameter(a).ToPoint2d(), 0, 0, 0);
            poly1.AddVertexAt(j + 2, loop.GetPointAtParameter(b).ToPoint2d(), 0, 0, 0);

            return new Polyline[] { poly1, poly2 };
        }

        /// <summary>
        /// Gets the bulge between two parameters within the same arc segment of a polyline.
        /// </summary>
        /// <param name="poly">The polyline.</param>
        /// <param name="startParam">The start parameter.</param>
        /// <param name="endParam">The end parameter.</param>
        /// <returns>The bulge.</returns>
        public static double GetBulgeBetween(this Polyline poly, double startParam, double endParam)
        {
            double total = poly.GetBulgeAt((int)Math.Floor(startParam));
            return (endParam - startParam) * total;
        }

        /// <summary>
        /// For a polyline with Closed=True, changes the value to False and closes it by adding a point.
        /// </summary>
        /// <param name="poly">The polyline.</param>
        /// <returns>The result polyline.</returns>
        public static Polyline TrulyClose(this Polyline poly)
        {
            if (poly.Closed == false)
            {
                return poly;
            }
            var result = poly.Clone() as Polyline;
            result.Closed = false;
            if (result.EndPoint != result.StartPoint)
            {
                result.AddVertexAt(poly.NumberOfVertices, poly.StartPoint.ToPoint2d(), 0, 0, 0);
            }
            return result;
        }

        /// <summary>
        /// Offsets a polyline.
        /// </summary>
        /// <param name="poly">The polyline.</param>
        /// <param name="offsets">The offsets for each segments.</param>
        /// <returns>The result polyline.</returns>
        public static Polyline OffsetPoly(this Polyline poly, double[] offsets)
        {
            poly = poly.TrulyClose();

            var bulges = new List<double>();
            var segs1 = new List<Polyline>();
            for (int i = 0; i < poly.NumberOfVertices - 1; i++)
            {
                var subPoly = new Polyline();
                subPoly.AddVertexAt(0, poly.GetPointAtParameter(i).ToPoint2d(), poly.GetBulgeAt(i), 0, 0);
                subPoly.AddVertexAt(1, poly.GetPointAtParameter(i + 1).ToPoint2d(), 0, 0, 0);
                var temp = subPoly.GetOffsetCurves(offsets[i]);
                if (temp.Count > 0)
                {
                    segs1.Add(temp[0] as Polyline);
                    bulges.Add(poly.GetBulgeAt(i));
                }
            }
            var points = new Point3dCollection();
            Enumerable.Range(0, segs1.Count).ToList().ForEach(x =>
            {
                int count = points.Count;
                int y = x + 1 < segs1.Count ? x + 1 : 0;
                segs1[x].IntersectWith3264(segs1[y], Autodesk.AutoCAD.DatabaseServices.Intersect.ExtendBoth, points);
                if (points.Count - count > 1) // This is an arc - more than 1 intersection point.
                {
                    var a = points[points.Count - 2];
                    var b = points[points.Count - 1];
                    if (segs1[x].EndPoint.DistanceTo(a) > segs1[x].EndPoint.DistanceTo(b))
                    {
                        points.Remove(a);
                    }
                    else
                    {
                        points.Remove(b);
                    }
                }
            });
            var result = new Polyline();

            int j = 0;
            points.Cast<Point3d>().ToList().ForEach(x =>
            {
                double bulge = j + 1 < points.Count ? bulges[j + 1] : 0;
                result.AddVertexAt(j, x.ToPoint2d(), bulge, 0, 0);
                j++;
            });
            if (poly.StartPoint == poly.EndPoint) // Closed polyline: add intersection to result.
            {
                result.AddVertexAt(0, points[points.Count - 1].ToPoint2d(), bulges[0], 0, 0);
            }
            else // Open polyline: add 2 offset points rather than the intersection to result.
            {
                result.AddVertexAt(0, segs1[0].StartPoint.ToPoint2d(), bulges[0], 0, 0);
                result.AddVertexAt(result.NumberOfVertices, segs1.Last().EndPoint.ToPoint2d(), 0, 0, 0);
                if (result.NumberOfVertices > 3)
                {
                    result.RemoveVertexAt(result.NumberOfVertices - 2); // Cannot be put before add - geometry will degrade.
                }
            }
            return result;
        }

        /// <summary>
        /// Gets arc bulge.
        /// </summary>
        /// <param name="arc">The arc.</param>
        /// <param name="start">The start point.</param>
        /// <returns>The bulge.</returns>
        public static double GetArcBulge(this Arc arc, Point3d start)
        {
            double bulge;
            double angle = arc.EndAngle - arc.StartAngle;
            if (angle < 0)
            {
                angle += Math.PI * 2;
            }
            if (arc.Normal.Z > 0)
            {
                bulge = Math.Tan(angle / 4);
            }
            else
            {
                bulge = -Math.Tan(angle / 4);
            }
            if (start == arc.EndPoint)
            {
                bulge = -bulge;
            }
            return bulge;
        }

        /// <summary>
        /// Converts line to polyline.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns>A polyline.</returns>
        public static Polyline ToPolyline(this Line line)
        {
            var poly = new Polyline();
            poly.AddVertexAt(0, line.StartPoint.ToPoint2d(), 0, 0, 0);
            poly.AddVertexAt(1, line.EndPoint.ToPoint2d(), 0, 0, 0);
            return poly;
        }

        /// <summary>
        /// Converts arc to polyline.
        /// </summary>
        /// <param name="arc">The arc.</param>
        /// <returns>A polyline.</returns>
        public static Polyline ToPolyline(this Arc arc)
        {
            var poly = new Polyline();
            poly.AddVertexAt(0, arc.StartPoint.ToPoint2d(), arc.GetArcBulge(arc.StartPoint), 0, 0);
            poly.AddVertexAt(1, arc.EndPoint.ToPoint2d(), 0, 0, 0);
            return poly;
        }

        /// <summary>
        /// Cleans up a polyline by removing duplicate points.
        /// </summary>
        /// <param name="poly">The polyline.</param>
        /// <returns>The number of points removed.</returns>
        public static int PolyClean_RemoveDuplicatedVertex(Polyline poly)
        {
            var points = poly.GetPolyPoints().ToArray();
            var dupIndices = new List<int>();
            for (int i = points.Length - 2; i >= 0; i--)
            {
                if (points[i].DistanceTo(points[i + 1]) < Consts.Epsilon)
                {
                    dupIndices.Add(i);
                }
            }
            dupIndices.ForEach(x => poly.RemoveVertexAt(x));
            return dupIndices.Count;
        }

        /// <summary>
        /// Cleans up a polyline by removing approximate points.
        /// </summary>
        /// <param name="poly">The polyline.</param>
        /// <param name="epsilon">The eplison.</param>
        /// <returns>The number of points removed.</returns>
        public static int PolyClean_ReducePoints(Polyline poly, double epsilon)
        {
            var points = poly.GetPolyPoints().ToArray();
            var cleanList = new List<int>();
            int j = 0;
            for (int i = 1; i < points.Length; i++)
            {
                if (points[i].DistanceTo(points[j]) < epsilon)
                {
                    cleanList.Add(i);
                }
                else
                {
                    j = i;
                }
            }
            cleanList.Reverse();
            cleanList.ForEach(x => poly.RemoveVertexAt(x));
            return cleanList.Count;
        }

        /// <summary>
        /// Cleans up a polyline by removing extra collinear points. 
        /// </summary>
        /// <param name="poly">The polyline.</param>
        public static void PolyClean_RemoveColinearPoints(Polyline poly)
        {
            var points = poly.GetPolyPoints().ToArray();
            var cleanList = new List<int>();
            int j = 0;
            for (int i = 1; i < points.Length; i++)
            {
                // TODO: implement this.
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Cleans up a polyline by setting topo direction.
        /// </summary>
        /// <param name="poly">The polyline.</param>
        /// <param name="dir">The direction.</param>
        /// <returns>A value indicating if a reversion is done.</returns>
        public static bool PolyClean_SetTopoDirection(Polyline poly, Direction dir)
        {
            if (poly.StartPoint == poly.EndPoint)
            {
                return false;
            }

            var reversed = false;
            var delta = poly.EndPoint - poly.StartPoint;
            switch (dir)
            {
                case Direction.West:
                    if (delta.X > 0)
                    {
                        poly.ReverseCurve();
                        reversed = true;
                    }
                    break;
                case Direction.North:
                    if (delta.Y < 0)
                    {
                        poly.ReverseCurve();
                        reversed = true;
                    }
                    break;
                case Direction.East:
                    if (delta.X < 0)
                    {
                        poly.ReverseCurve();
                        reversed = true;
                    }
                    break;
                case Direction.South:
                    if (delta.Y > 0)
                    {
                        poly.ReverseCurve();
                        reversed = true;
                    }
                    break;
                default:
                    break;
            }
            return reversed;
        }

        /// <summary>
        /// The direction.
        /// </summary>
        public enum Direction
        {
            /// <summary>
            /// None.
            /// </summary>
            None,
            /// <summary>
            /// West.
            /// </summary>
            West,
            /// <summary>
            /// North.
            /// </summary>
            North,
            /// <summary>
            /// East.
            /// </summary>
            East,
            /// <summary>
            /// South.
            /// </summary>
            South
        }

        /// <summary>
        /// Connects polylines.
        /// </summary>
        /// <param name="poly">The base polyline.</param>
        /// <param name="poly1">The other polyline.</param>
        public static void JoinPolyline(this Polyline poly, Polyline poly1)
        {
            int index = poly.GetPolyPoints().Count();
            int index1 = 0;
            poly1.GetPoints().ToList().ForEach(x =>
            {
                poly.AddVertexAt(index, x.ToPoint2d(), poly1.GetBulgeAt(index1), 0, 0);
                index++;
                index1++;
            });
        }

        /// <summary>
        /// Connects polylines.
        /// </summary>
        /// <param name="poly1">The polyline 1.</param>
        /// <param name="poly2">The polyline 2.</param>
        /// <returns>The result polyline.</returns>
        public static Polyline PolyJoin(this Polyline poly1, Polyline poly2)
        {
            int index = 0;
            var poly = new Polyline();
            for (int i = 0; i < poly1.NumberOfVertices; i++)
            {
                if (i == poly1.NumberOfVertices - 1 && poly1.EndPoint == poly2.StartPoint)
                {
                }
                else
                {
                    poly.AddVertexAt(index, poly1.GetPoint2dAt(i), poly1.GetBulgeAt(i), 0, 0);
                    index++;
                }
            }
            for (int i = 0; i < poly2.NumberOfVertices; i++)
            {
                poly.AddVertexAt(index, poly2.GetPoint2dAt(i), poly2.GetBulgeAt(i), 0, 0);
                index++;
            }
            return poly;
        }

        /// <summary>
        /// Connects polyline.
        /// </summary>
        /// <param name="polys">The polylines.</param>
        /// <returns>The result polyline.</returns>
        public static Polyline PolyJoin(this List<Polyline> polys) // newly 20130807
        {
            return polys.Aggregate(Algorithms.PolyJoin);
        }

        /// <summary>
        /// Connects polylines, ignoring intermediate vertices.
        /// </summary>
        /// <param name="polys">The polylines.</param>
        /// <returns>The result polyline.</returns>
        public static Polyline PolyJoinIgnoreMiddleVertices(this List<Polyline> polys) // newly 20130807
        {
            var poly = new Polyline();
            var points = polys
                .SelectMany(p => new[] { p.StartPoint, p.EndPoint })
                .Distinct()
                .ToList();

            return NoDraw.Pline(points);
        }

        /// <summary>
        /// Determines if a point is in a polygon (defined by a polyline).
        /// </summary>
        /// <param name="poly">The polygon.</param>
        /// <param name="p">The point.</param>
        /// <returns>The result.</returns>
        public static bool IsPointIn(this Polyline poly, Point3d p)
        {
            double temp = 0;
            var points = poly.GetPoints().ToList();
            for (int i = 0; i < points.Count; i++)
            {
                int j = (i < points.Count - 1) ? (i + 1) : 0;
                var v1 = points[i].ToPoint2d() - p.ToPoint2d();
                var v2 = points[j].ToPoint2d() - p.ToPoint2d();
                temp += v1.MinusPiToPiAngleTo(v2);
            }
            if (Math.Abs(Math.Abs(temp) - 2 * Math.PI) < 0.1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the centroid of a polyline.
        /// </summary>
        /// <param name="poly">The polyline.</param>
        /// <returns>The centroid.</returns>
        public static Point3d Centroid(this Polyline poly) // newly 20130801
        {
            var points = poly.GetPoints().ToList();
            if (points.Count == 1)
            {
                return points[0];
            }
            else
            {
                var temp = Point3d.Origin;
                double areaTwice = 0;
                for (int i = 0; i < points.Count; i++)
                {
                    int j = (i < points.Count - 1) ? (i + 1) : 0;
                    var v1 = points[i].GetAsVector();
                    var v2 = points[j].GetAsVector();
                    temp += v1.CrossProduct(v2).Z / 3.0 * (v1 + v2);
                    areaTwice += v1.CrossProduct(v2).Z;
                }
                return (1.0 / areaTwice) * temp;
            }
        }

        #endregion

        #region Auxiliary algorithms

        /// <summary>
        /// Gets the transformation matrix of world coordinates to viewport coordinates
        /// </summary>
        /// <param name="viewCenter">The view center.</param>
        /// <param name="viewportCenter">The viewport center.</param>
        /// <param name="scale">The scale.</param>
        /// <returns></returns>
        public static Matrix3d WorldToViewport(Point3d viewCenter, Point3d viewportCenter, double scale)
        {
            return Matrix3d
                .Scaling(1.0 / scale, viewportCenter)
                .PostMultiplyBy(Matrix3d.Displacement(viewportCenter - viewCenter));
        }

        /// <summary>
        /// Intersects entities.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="entOther">The other entity.</param>
        /// <param name="intersectType">The type.</param>
        /// <param name="points">The intersection points output.</param>
        internal static void IntersectWith3264(this Entity entity, Entity entOther, Intersect intersectType, Point3dCollection points)
        {
            // NOTE: Use runtime binding for difference between 32- and 64-bit APIs.
            var methodInfo = typeof(Entity).GetMethod("IntersectWith",
                new Type[] { typeof(Entity), typeof(Intersect), typeof(Point3dCollection), typeof(long), typeof(long) });
            if (methodInfo == null) // 32-bit AutoCAD
            {
                methodInfo = typeof(Entity).GetMethod("IntersectWith",
                new Type[] { typeof(Entity), typeof(Intersect), typeof(Point3dCollection), typeof(int), typeof(int) });
            }
            methodInfo.Invoke(entity, new object[] { entOther, intersectType, points, 0, 0 });
        }

        /// <summary>
        /// Intersects entities.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="entOther">The other entity.</param>
        /// <param name="intersectType">The type.</param>
        /// <returns>The intersection points.</returns>
        public static List<Point3d> Intersect(this Entity entity, Entity entOther, Intersect intersectType)
        {
            var points = new Point3dCollection();
            Algorithms.IntersectWith3264(entity, entOther, intersectType, points);
            return points.Cast<Point3d>().ToList();
        }

        /// <summary>
        /// Converts string to double.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The result.</returns>
        public static double ToDouble(this string s, double defaultValue)
        {
            try
            {
                return Convert.ToDouble(s);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Converts string to double with default 0.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns>The result.</returns>
        public static double ToDouble(this string s)
        {
            return s.ToDouble(0);
        }

        /// <summary>
        /// Gets remainder.
        /// </summary>
        /// <param name="n">The divident.</param>
        /// <param name="m">The divisor.</param>
        /// <returns>The remainder within [0, m).</returns>
        public static int Mod(this int n, int m)
        {
            return (n > 0) ? (n % m) : (n % m + m);
        }

        /// <summary>
        /// Gets the angle between two vectors within [0, Pi].
        /// </summary>
        /// <param name="v0">The vector 0.</param>
        /// <param name="v1">The vector 1.</param>
        /// <returns>The result.</returns>
        public static double ZeroToPiAngleTo(this Vector2d v0, Vector2d v1)
        {
            return v0.GetAngleTo(v1);
        }

        /// <summary>
        /// Gets the heading (direction) angle of a vector within [0, 2Pi].
        /// </summary>
        /// <param name="v0">The vector.</param>
        /// <returns>The result.</returns>
        public static double DirAngleZeroTo2Pi(this Vector2d v0)
        {
            double angle = v0.ZeroToPiAngleTo(Vector2d.XAxis);
            if (v0.Y < 0)
            {
                angle = 2 * Math.PI - angle;
            }
            return angle;
        }

        /// <summary>
        /// Gets the angle between two vectors within [0, 2Pi].
        /// </summary>
        /// <param name="v0">The vector 0.</param>
        /// <param name="v1">The vector 1.</param>
        /// <returns>The result.</returns>
        public static double ZeroTo2PiAngleTo(this Vector2d v0, Vector2d v1)
        {
            double angle0 = v0.DirAngleZeroTo2Pi();
            double angle1 = v1.DirAngleZeroTo2Pi();
            double angleDelta = angle1 - angle0;
            if (angleDelta < 0) angleDelta = angleDelta + 2 * Math.PI;
            return angleDelta;
        }

        /// <summary>
        /// Gets the angle between two vectors within [-Pi, Pi].
        /// </summary>
        /// <param name="v0">The vector 0.</param>
        /// <param name="v1">The vector 1.</param>
        /// <returns>The result.</returns>
        public static double MinusPiToPiAngleTo(this Vector2d v0, Vector2d v1)
        {
            double angle0 = v0.DirAngleZeroTo2Pi();
            double angle1 = v1.DirAngleZeroTo2Pi();
            double angleDelta = angle1 - angle0;
            if (angleDelta < -Math.PI) angleDelta = angleDelta + 2 * Math.PI;
            else if (angleDelta > Math.PI) angleDelta = angleDelta - 2 * Math.PI;
            return angleDelta;
        }

        public static IEnumerable<double> Range(double start, double end, double step = 1)
        {
            for (double x = start; x <= end; x += step)
            {
                yield return x;
            }
        }

        public static IEnumerable<int> Range(int start, int end, int step = 1)
        {
            for (int x = start; x <= end; x += step)
            {
                yield return x;
            }
        }

        public static IEnumerable<T> Every<T>(this IEnumerable<T> source, int step, int start = 0)
        {
            return Range(start, source.Count() - 1, step).Select(i => source.ElementAt(i));
        }

        #endregion

        #region Miscellaneous algorithms

        public static double GetCurveTotalLengthInPolygon(IEnumerable<Curve> curves, Polyline poly) // newly 20130514
        {
            return GetCurveTotalLength(curves, p => poly.IsPointIn(p));
        }

        public static double GetCurveTotalLengthInExtents(IEnumerable<Curve> curves, Extents3d extents) // newly 20130514
        {
            return GetCurveTotalLength(curves, p => extents.IsPointIn(p));
        }

        private static double GetCurveTotalLength(IEnumerable<Curve> curves, Func<Point3d, bool> isIn, int divs = 100) // newly 20130514
        {
            double totalLength = 0;
            foreach (var curve in curves)
            {
                double length = curve.GetDistAtParam(curve.EndParam);
                double divLength = length / divs;
                var points = Enumerable.Range(0, divs + 1).Select(i => curve.GetPointAtParam(i * divLength)).ToList();
                for (int i = 0; i < divs; i++)
                {
                    if (isIn(points[i]) && isIn(points[i + 1]))
                    {
                        totalLength += points[i].DistanceTo(points[i + 1]);
                    }
                }
            }
            return totalLength;
        }

        public static List<Polyline> HatchToPline(Hatch ht) // newly 20130729
        {
            var plines = new List<Polyline>();
            int loopCount = ht.NumberOfLoops;
            //System.Diagnostics.Debug.Write(loopCount);
            for (int index = 0; index < loopCount;)
            {
                if (ht.GetLoopAt(index).IsPolyline)
                {
                    var loop = ht.GetLoopAt(index).Polyline;
                    var p = new Polyline();
                    int i = 0;
                    loop.Cast<BulgeVertex>().ToList().ForEach(y =>
                    {
                        p.AddVertexAt(i, y.Vertex, y.Bulge, 0, 0);
                        i++;
                    });
                    plines.Add(p);
                    break;
                }
                else
                {
                    var loop = ht.GetLoopAt(index).Curves;
                    var p = new Polyline();
                    int i = 0;
                    loop.Cast<Curve2d>().ToList().ForEach(y =>
                    {
                        p.AddVertexAt(i, y.StartPoint, 0, 0, 0);
                        i++;
                        if (y == loop.Cast<Curve2d>().Last())
                        {
                            p.AddVertexAt(i, y.EndPoint, 0, 0, 0);
                        }
                    });
                    plines.Add(p);
                    break;
                }
            }
            return plines;
        }

        #endregion
    }

    /// <summary>
    /// The region and loop service.
    /// </summary>
    public static class RegionLoopService
    {
        /// <summary>
        /// Merges adjacent boundaries.
        /// </summary>
        /// <param name="ids">The boundary IDs to merge.</param>
        /// <returns>The boundary IDs after merger.</returns>
        public static List<ObjectId> MergeBoundary(IEnumerable<ObjectId> ids)
        {
            if (ids.Count() < 2)
            {
                return new List<ObjectId>(ids);
            }

            Region region = null;
            // Merge adjacent regions
            foreach (var id in ids)
            {
                var objArr = new DBObjectCollection
                {
                    id.QOpenForRead()
                };
                var regionSub = Region.CreateFromCurves(objArr)[0] as Region;
                if (region == null)
                {
                    region = regionSub;
                }
                else
                {
                    region.BooleanOperation(BooleanOperationType.BoolUnite, regionSub);
                }
            }

            if (region == null)
            {
                return new List<ObjectId>();
            }
            // Outer loop
            var loops = region.GetLoops().Select(x => x.AddToCurrentSpace()).ToArray();
            return new List<ObjectId>(loops);
        }

        /// <summary>
        /// Gets loops of region.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <returns>The boundary polyline array.</returns>
        public static Polyline[] GetLoops(this Region region)
        {
            var explodeResult = new DBObjectCollection();
            region.Explode(explodeResult);
            if (explodeResult[0] is Curve)
            {
                return new Polyline[] { GetLoop(GroupLoops(explodeResult)[0]) };
            }
            else // explodeResult[0] is Region
            {
                var polys = new List<Polyline>();
                foreach (Region sub in explodeResult)
                {
                    var subResult = new DBObjectCollection();
                    sub.Explode(subResult);
                    polys.Add(GetLoop(GroupLoops(subResult)[0]));
                }
                return polys.ToArray();
            }
        }

        #region Private...

        // Gets a loop from lines.
        private static Polyline GetLoop(DBObjectCollection lines)
        {
            Point3d start;
            if ((lines[0] as Curve).StartPoint != (lines[1] as Curve).StartPoint && (lines[0] as Curve).StartPoint != (lines[1] as Curve).EndPoint)
            {
                start = (lines[0] as Curve).StartPoint;
            }
            else
            {
                start = (lines[0] as Curve).EndPoint;
            }
            var result = new Polyline();
            var next = start;
            var i = 0;
            foreach (Curve line in lines)
            {
                var bulge = line is Arc ? Algorithms.GetArcBulge(line as Arc, next) : 0;
                result.AddVertexAt(i, next.ToPoint2d(), bulge, 0, 0);
                next = NextPoint(line, next);
                i++;
            }
            result.AddVertexAt(i, start.ToPoint2d(), 0, 0, 0);
            return result;
        }

        // Groups exploded lines into loops.
        private static List<DBObjectCollection> GroupLoops(DBObjectCollection lines)
        {
            var groups = new List<DBObjectCollection>();
            var i = 0;
            while (i < lines.Count - 1)
            {
                var group = IncludingRing(lines, lines[i] as Curve);
                groups.Add(group);
                i += group.Count;
            }
            return groups;
        }

        // For test only. It shows exploded lines are grouped by loops, but lines in a loop are not in order.
        private static void ShowExplodeResult(DBObjectCollection lines)
        {
            var sb = new StringBuilder();
            foreach (Curve line in lines)
            {
                sb.AppendLine($"({line.StartPoint.X},{line.StartPoint.Y})->({line.EndPoint.X},{line.EndPoint.Y})");
            }
            var sw = new StreamWriter("C:\\ShowExplodeResult.txt");
            sw.Write(sb.ToString());
            sw.Close();
        }

        // Gets the other end of a curve.
        private static Point3d NextPoint(Curve line, Point3d point)
        {
            if (point.DistanceTo(line.StartPoint) < Consts.Epsilon)
            {
                return line.EndPoint;
            }
            else if (point.DistanceTo(line.EndPoint) < Consts.Epsilon)
            {
                return line.StartPoint;
            }
            else
            {
                return Algorithms.NullPoint3d;
            }
        }

        // Gets next line.
        private static Curve NextLine(DBObjectCollection lines, Curve line, Point3d point)
        {
            return lines.Cast<Curve>().First(x => (x.StartPoint == point || x.EndPoint == point) && x != line);
        }

        // Gets lines that forms a ring that includes a specified line.
        private static DBObjectCollection IncludingRing(DBObjectCollection lines, Curve origin)
        {
            var ring = new DBObjectCollection
            {
                origin
            };
            var next = origin;
            var point = origin.EndPoint; // Iterate from the end point of first line.
            while (point != origin.StartPoint)
            {
                next = NextLine(lines, next, point);
                ring.Add(next);
                point = NextPoint(next, point);
            }
            return ring;
        }

        #endregion
    }

    /// <summary>
    /// Interval.
    /// </summary>
    public class Interv : Tuple<double, double>
    {
        /// <summary>
        /// Creates an interval by specifying start and end.
        /// </summary>
        /// <param name="start">The lower limit.</param>
        /// <param name="end">The uppper limit.</param>
        public Interv(double start, double end)
            : base(start, end)
        {
        }

        /// <summary>
        /// Creates an interval from a C# 7.0 tuple.
        /// </summary>
        /// <param name="tuple">The tuple.</param>
        public Interv((double, double) tuple)
            : this(tuple.Item1, tuple.Item2)
        {
        }

        /// <summary>
        /// The lower limit.
        /// </summary>
        public double Start => Math.Min(base.Item1, base.Item2);

        /// <summary>
        /// The upper limit.
        /// </summary>
        public double End => Math.Max(base.Item1, base.Item2);

        /// <summary>
        /// The length.
        /// </summary>
        public double Length => this.End - this.Start;

        /// <summary>
        /// Adds point to interval.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>The result (new interval).</returns>
        public Interv AddPoint(double point)
        {
            if (IsPointIn(point))
            {
                return this;
            }
            else
            {
                if (point > End)
                {
                    return new Interv(Start, point);
                }
                else // point < Start
                {
                    return new Interv(point, End);
                }
            }
        }

        /// <summary>
        /// Adds interval to interval.
        /// </summary>
        /// <param name="interval">The added interval.</param>
        /// <returns>The result (new interval).</returns>
        public Interv AddInterval(Interv interval)
        {
            return this.AddPoint(interval.Start).AddPoint(interval.End);
        }

        /// <summary>
        /// Determines if a point is in the interval.
        /// </summary>
        /// <param name="point">The value.</param>
        /// <returns>A value indicating whether the point is in.</returns>
        public bool IsPointIn(double point)
        {
            return point >= this.Start && point <= this.End;
        }
    }
}
