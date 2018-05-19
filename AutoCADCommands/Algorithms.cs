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
    using IniData = Dictionary<string, Dictionary<string, string>>;

    /// <summary>
    /// 封装常量 / Package constants
    /// </summary>
    public static class Consts
    {
        /// <summary>
        /// 通用容差 / Universal tolerance
        /// </summary>
        public const double Epsilon = 0.001;
        /// <summary>
        /// 默认文字样式 / Default text style
        /// </summary>
        public const string TextStyleName = "AutoCADCodePackTextStyle";
        /// <summary>
        /// 编码的FXD AppName
        /// </summary>
        public const string AppNameForCode = "TongJiCode"; // like HTML tag name
        /// <summary>
        /// ID的FXD AppName
        /// </summary>
        public const string AppNameForID = "TongJiID"; // like HTML id
        /// <summary>
        /// 名称的FXD AppName
        /// </summary>
        public const string AppNameForName = "TongJiName"; // like HTML id or name
        /// <summary>
        /// 标签的FXD AppName
        /// </summary>
        public const string AppNameForTags = "TongJiTags"; // like HTML class
    }

    public static class Utils
    {
        #region 文件算法 / File algorithm

        /// <summary>
        /// 获取路径2相对于路径1的相对路径 / Get the relative path of path 2 relative to path 1
        /// </summary>
        /// <param name="baseFolder">路径1</param>
        /// <param name="path">路径2</param>
        /// <returns>返回路径2相对于路径1的路径</returns>
        /// <example>
        /// string strPath = GetRelativePath(@"C:\WINDOWS\system32", @"C:\WINDOWS\system\*.*" );
        /// //strPath == @"..\system\*.*"
        /// </example>
        public static string GetRelativePath(string baseFolder, string path)
        {
            if (!baseFolder.EndsWith("\\")) baseFolder += "\\";
            int intIndex = -1, intPos = baseFolder.IndexOf('\\');
            while (intPos >= 0)
            {
                intPos++;
                if (string.Compare(baseFolder, 0, path, 0, intPos, true) != 0) break;
                intIndex = intPos;
                intPos = baseFolder.IndexOf('\\', intPos);
            }

            if (intIndex >= 0)
            {
                path = path.Substring(intIndex);
                intPos = baseFolder.IndexOf("\\", intIndex);
                while (intPos >= 0)
                {
                    path = "..\\" + path;
                    intPos = baseFolder.IndexOf("\\", intPos + 1);
                }
            }
            return path;
        }

        private static string ExpandRelativePath(string baseFolder, string relativePath)
        {
            return (relativePath.Contains(":") ? string.Empty : baseFolder) + relativePath;
        }

        /// <summary>
        /// 解析INI文件 / Parsing INI files
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="result">结果</param>
        /// <returns>是否成功</returns>
        public static bool ParseIniFile(string fileName, IniData result)
        {
            string groupPattern = @"^\[[^\[\]]+\]$";
            string dataPattern = @"^[^=]+=[^=]+$";

            string[] lines = System.IO.File.ReadAllLines(fileName).Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            Dictionary<string, string> group = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                if (line.StartsWith("["))
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(line, groupPattern))
                    {
                        return false;
                    }
                    group = new Dictionary<string, string>();
                    string groupName = line.Trim('[', ']');
                    result.Add(groupName, group);
                }
                else
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(line, dataPattern))
                    {
                        return false;
                    }
                    string[] parts = line.Split('=').Select(x => x.Trim()).ToArray();
                    group.Add(parts[0], parts[1]);
                }
            }
            return true;
        }

        private static bool CaseFreeContains(this IEnumerable<string> source, string value)
        {
            return source.Select(x => x.ToUpper()).Contains(value.ToUpper());
        }

        private static T CaseFreeDictValue<T>(this Dictionary<string, T> source, string key)
        {
            return source.First(x => x.Key.ToUpper() == key.ToUpper()).Value;
        }

        private static void SetCaseFreeDictValue<T>(this Dictionary<string, T> source, string key, T value)
        {
            string realKey = source.First(x => x.Key.ToUpper() == key.ToUpper()).Key;
            source[realKey] = value;
        }

        private static bool Contains(this IEnumerable<string> source, string value, bool caseFree)
        {
            if (caseFree)
            {
                return source.CaseFreeContains(value);
            }
            else
            {
                return source.Contains(value);
            }
        }

        private static T DictValue<T>(this Dictionary<string, T> source, string key, bool caseFree)
        {
            if (caseFree)
            {
                return source.CaseFreeDictValue(key);
            }
            else
            {
                return source[key];
            }
        }

        private static void SetDictValue<T>(this Dictionary<string, T> source, string key, T value, bool caseFree)
        {
            if (caseFree)
            {
                source.SetCaseFreeDictValue(key, value);
            }
            else
            {
                source[key] = value;
            }
        }

        #endregion
    }

    /// <summary>
    /// 各种算法 / Various algorithms
    /// </summary>
    public static class Algorithms
    {
        #region 曲线算法 / Curve algorithm

        /// <summary>
        /// 点到曲线的距离 / Point to curve distance
        /// </summary>
        /// <param name="cv">曲线</param>
        /// <param name="point">点</param>
        /// <returns>距离</returns>
        public static double GetDistToPoint(this Curve cv, Point3d point, bool extend = false)
        {
            return cv.GetClosestPointTo(point, extend).DistanceTo(point);
        }

        /// <summary>
        /// 获取指定距离处的参数 / Get parameters at a specified distance
        /// </summary>
        /// <param name="cv">曲线</param>
        /// <param name="dist">距离</param>
        /// <returns>参数</returns>
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
        /// 获取指定参数处的距离 / Get the distance at the specified parameter
        /// </summary>
        /// <param name="cv">曲线</param>
        /// <param name="param">参数</param>
        /// <returns>距离</returns>
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
        /// 获取指定参数处的点 / Get the point at the specified parameter
        /// </summary>
        /// <param name="cv">曲线</param>
        /// <param name="param">参数</param>
        /// <returns>点</returns>
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
        /// 获取指定距离处的点 / Get points at a specified distance
        /// </summary>
        /// <param name="cv">曲线</param>
        /// <param name="dist">距离</param>
        /// <returns>点</returns>
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
        /// 获取指定点处的距离 / Get the distance at the specified point
        /// </summary>
        /// <param name="cv">曲线</param>
        /// <param name="point">点</param>
        /// <returns>距离</returns>
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
        /// 获取指定点处的参数 / Get the parameters at the specified point
        /// </summary>
        /// <param name="cv">曲线</param>
        /// <param name="point">点</param>
        /// <returns>参数</returns>
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
        /// 获取曲线子集 / Get a subset of curves
        /// </summary>
        /// <param name="curve">曲线</param>
        /// <param name="interval">曲线子集的长度区间</param>
        /// <returns>曲线子集</returns>
        public static Curve GetSubCurve(this Curve curve, Interv interval) // todo: 在函数API中移除复杂类型
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
        /// 获取曲线子集 / Get a subset of curves
        /// </summary>
        /// <param name="curve">曲线</param>
        /// <param name="interval">曲线子集的参数区间</param>
        /// <returns>曲线子集</returns>
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
        /// 获取曲线的整数参数点 / Get the integer parameter point of the curve
        /// </summary>
        /// <param name="cv">曲线</param>
        /// <returns>点集</returns>
        public static IEnumerable<Point3d> GetPoints(this Curve cv)
        {
            for (int i = 0; i <= cv.EndParam; i++)
            {
                yield return cv.GetPointAtParam(i);
            }
        }

        /// <summary>
        /// 获取多段线的顶点。与GetPoints的区别在于IsClosed=true的情况 / Get the vertex of the polyline. The difference with GetPoints is the condition of IsClosed=true
        /// </summary>
        /// <param name="poly">多段线</param>
        /// <returns>点集</returns>
        public static IEnumerable<Point3d> GetPolyPoints(this Polyline poly)
        {
            for (int i = 0; i < poly.NumberOfVertices; i++)
            {
                yield return poly.GetPoint3dAt(i);
            }
        }

        /// <summary>
        /// 获取曲线的参数等分点 / Get the parameters of the curve
        /// </summary>
        /// <param name="cv">曲线</param>
        /// <param name="divs">等分数</param>
        /// <returns>点集</returns>
        public static IEnumerable<Point3d> GetPoints(this Curve cv, int divs) // todo: 获取曲线的距离等分点 / Get the curve's distance equal points
        {
            double div = cv.EndParam / divs;
            for (double i = 0; i < cv.EndParam + div; i += div)
            {
                yield return cv.GetPointAtParam(i);
            }
        }

        private static IEnumerable<Point3d> GetPolylineFitPointsImp(this Curve cv, int divsWhenArc)
        {
            Polyline poly = cv as Polyline;
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
                        int divs = divsWhenArc == 0 ? (int)((Math.Atan(Math.Abs(poly.GetBulgeAt(i))) * 4) / (Math.PI / 18) + 4) : divsWhenArc;  // 加4应对特别小的弧度，小弧度对应的弧段长度可能很大
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
        /// 获取多段线的拟合点集 / Get polyline fitting point set
        /// </summary>
        /// <param name="cv">多段线</param>
        /// <param name="divsWhenArc">弧段的分段数，默认为0，表示智能选取</param>
        /// <returns>拟合点集</returns>
        public static IEnumerable<Point3d> GetPolylineFitPoints(this Curve cv, int divsWhenArc = 0)
        {
            try
            {
                return GetPolylineFitPointsImp(cv, divsWhenArc).ToArray();
            }
            catch
            {
                throw new PolylineNeedCleanException("请先运行多段线清理。");
            }
        }

        /// <summary>
        /// 获取曲线上以整数参数点为界的曲线段 / Get curve segments on the curve bounded by integer parameter points
        /// </summary>
        /// <param name="cv">曲线</param>
        /// <returns>曲线段集</returns>
        public static IEnumerable<Curve> GetSegments(this Curve cv)
        {
            for (int i = 0; i < cv.EndParam; i++)
            {
                yield return cv.GetSubCurveByParams(new Interv(i, i + 1));
            }
        }

        /// <summary>
        /// 计算两曲线的最小距离 / Calculate the minimum distance between two curves
        /// </summary>
        /// <param name="cv1">曲线1</param>
        /// <param name="cv2">曲线2</param>
        /// <param name="divs">等分数</param>
        /// <returns>距离</returns>
        public static double GetDistOfTwoCurve(Curve cv1, Curve cv2, int divs = 100)
        {
            var pts1 = cv1.GetPoints(divs);
            var pts2 = cv2.GetPoints(divs);
            return pts1.Min(p1 => pts2.Min(p2 => p1.DistanceTo(p2)));
        }

        #endregion

        #region 范围算法 / Range algorithm

        /// <summary>
        /// 获取实体范围 / Get entity scope
        /// </summary>
        /// <param name="entIds">实体ID集</param>
        /// <returns>范围</returns>
        public static Extents3d GetExtents(this IEnumerable<ObjectId> entIds)
        {
            Extents3d extent = entIds.First().QSelect(x => x.GeometricExtents);
            foreach (var id in entIds)
            {
                extent.AddExtents(id.QSelect(x => x.GeometricExtents));
            }
            return extent;
        }

        /// <summary>
        /// 获取实体范围 / Get entity scope
        /// </summary>
        /// <param name="ents">实体集</param>
        /// <returns>范围</returns>
        public static Extents3d GetExtents(this IEnumerable<Entity> ents)
        {
            Extents3d extent = ents.First().GeometricExtents;
            foreach (var ent in ents)
            {
                extent.AddExtents(ent.GeometricExtents);
            }
            return extent;
        }

        /// <summary>
        /// 获取范围中心 / Get scope center
        /// </summary>
        /// <param name="extents">范围</param>
        /// <returns>中心点</returns>
        public static Point3d GetCenter(this Extents3d extents)
        {
            return Point3d.Origin + 0.5 * (extents.MinPoint.GetAsVector() + extents.MaxPoint.GetAsVector());
        }

        /// <summary>
        /// 获取范围中心 / Get scope center
        /// </summary>
        /// <param name="extents">范围</param>
        /// <returns>中心点</returns>
        public static Point2d GetCenter(this Extents2d extents)
        {
            return Point2d.Origin + 0.5 * (extents.MinPoint.GetAsVector() + extents.MaxPoint.GetAsVector());
        }

        /// <summary>
        /// 范围缩放 / Range scaling
        /// </summary>
        /// <param name="extents">范围</param>
        /// <param name="factor">比例因子</param>
        /// <returns>结果</returns>
        public static Extents3d Expand(this Extents3d extents, double factor)
        {
            var center = extents.GetCenter();
            return new Extents3d(center + factor * (extents.MinPoint - center), center + factor * (extents.MaxPoint - center));
        }

        /// <summary>
        /// 指定中心长出范围 / Specify center outreach
        /// </summary>
        /// <param name="center">中心</param>
        /// <param name="size">大小</param>
        /// <returns>结果</returns>
        public static Extents3d Expand(this Point3d center, double size) // newly 20130201
        {
            Vector3d move = new Vector3d(size / 2, size / 2, size / 2);
            return new Extents3d(center - move, center + move);
        }

        /// <summary>
        /// 点在范围内判定 / Point within range
        /// </summary>
        /// <param name="extents">范围</param>
        /// <param name="point">点</param>
        /// <returns>结果</returns>
        public static bool IsPointIn(this Extents3d extents, Point3d point)
        {
            return point.X >= extents.MinPoint.X && point.X <= extents.MaxPoint.X
                && point.Y >= extents.MinPoint.Y && point.Y <= extents.MaxPoint.Y;
            //&& point.Z >= extents.MinPoint.Z && point.Z <= extents.MaxPoint.Z;
        }

        /// <summary>
        /// 获取实体范围中心 / Get entity scope center
        /// </summary>
        /// <param name="entIds">实体</param>
        /// <returns>中心点</returns>
        public static Point3d GetCenter(this IEnumerable<ObjectId> entIds)
        {
            return entIds.GetExtents().GetCenter();
        }

        /// <summary>
        /// 获取实体范围中心 / Get entity scope center
        /// </summary>
        /// <param name="ents">实体</param>
        /// <returns>中心点</returns>
        public static Point3d GetCenter(this IEnumerable<Entity> ents)
        {
            return ents.GetExtents().GetCenter();
        }

        /// <summary>
        /// 获取实体范围中心 / Get entity scope center
        /// </summary>
        /// <param name="entId">实体</param>
        /// <returns>中心点</returns>
        public static Point3d GetCenter(this ObjectId entId)
        {
            var extent = entId.QSelect(x => x.GeometricExtents);
            return extent.GetCenter();
        }

        /// <summary>
        /// 获取实体范围中心 / Get entity scope center
        /// </summary>
        /// <param name="ent">实体</param>
        /// <returns>中心点</returns>
        public static Point3d GetCenter(this Entity ent)
        {
            var extent = ent.GeometricExtents;
            return extent.GetCenter();
        }

        /// <summary>
        /// 获取范围面积 / Get area
        /// </summary>
        /// <param name="extents">范围</param>
        /// <returns>面积</returns>
        public static double GetArea(this Extents3d extents) // newly 20130514
        {
            return (extents.MaxPoint.X - extents.MinPoint.X) * (extents.MaxPoint.Y - extents.MinPoint.Y);
        }

        /// <summary>
        /// 获取范围面积 / Get area
        /// </summary>
        /// <param name="extents">范围</param>
        /// <returns>面积</returns>
        public static double GetArea(this Extents2d extents) // newly 20130514
        {
            return (extents.MaxPoint.X - extents.MinPoint.X) * (extents.MaxPoint.Y - extents.MinPoint.Y);
        }

        #endregion

        #region 点算法 / Point Algorithm

        private static Point3d _nullPoint3d = new Point3d(double.NaN, double.NaN, double.NaN);
        /// <summary>
        /// 获取一个空的Point3d / Get an empty Point3d
        /// </summary>
        public static Point3d NullPoint3d
        {
            get
            {
                return _nullPoint3d;
            }
        }

        /// <summary>
        /// Point3d为空判别 / Point3d is empty
        /// </summary>
        /// <param name="p">点</param>
        /// <returns>结果</returns>
        public static bool IsNull(this Point3d p)
        {
            return double.IsNaN(p.X);
        }

        /// <summary>
        /// Point3d转Point2d / Point3d to Point2d
        /// </summary>
        /// <param name="point">点</param>
        /// <returns>结果</returns>
        public static Point2d ToPoint2d(this Point3d point)
        {
            return new Point2d(point.X, point.Y);
        }

        /// <summary>
        /// Point2d转Point3d / Point2d to Point3d
        /// </summary>
        /// <param name="point">点</param>
        /// <returns>结果</returns>
        public static Point3d ToPoint3d(this Point2d point)
        {
            return new Point3d(point.X, point.Y, 0);
        }

        /// <summary>
        /// Vector2d转Vector3d / Vector2d to Vector3d
        /// </summary>
        /// <param name="point">点</param>
        /// <returns>结果</returns>
        public static Vector3d ToVector3d(this Vector2d point)
        {
            return new Vector3d(point.X, point.Y, 0);
        }

        /// <summary>
        /// Vector3d转Vector2d / Vector3d to Vector2d
        /// </summary>
        /// <param name="point">点</param>
        /// <returns>结果</returns>
        public static Vector2d ToVector2d(this Vector3d point)
        {
            return new Vector2d(point.X, point.Y);
        }

        /// <summary>
        /// 计算凸包 / Calculate convex hull
        /// </summary>
        /// <param name="source">点集</param>
        /// <returns>结果</returns>
        public static List<Point3d> GetConvexHull(List<Point3d> source)
        {
            List<Point3d> points = new List<Point3d>();
            List<Point3d> collection = new List<Point3d>();
            int num = 0;
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

        #region 多段线算法 / Polyline algorithm

        /// <summary>
        /// 判断多段线是否自相交 / Determine if the polyline is self-
        /// </summary>
        /// <param name="poly">多段线</param>
        /// <returns>结果</returns>
        public static bool IsSelfIntersecting(this Polyline poly) // newly by WY 20130202
        {
            var points = poly.GetPolyPoints().ToList();
            for (int i = 0; i < points.Count - 3; i++)
            {
                Point2d a1 = points[i].ToPoint2d();
                Point2d a2 = points[i + 1].ToPoint2d();
                for (int j = i + 2; j < points.Count - 1; j++)
                {
                    Point2d b1 = points[j].ToPoint2d();
                    Point2d b2 = points[j + 1].ToPoint2d();
                    if (IsLineSegIntersect(a1, a2, b1, b2))
                    {
                        if (i == 0 && j == points.Count - 2) // 正好是首尾两段，要看是否因闭合，闭合不算自相交。
                        {
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
        /// 二维向量的伪外积 / Pseudo-external product of two-dimensional vectors
        /// </summary>
        /// <param name="v1">向量1</param>
        /// <param name="v2">向量2</param>
        /// <returns>伪外积</returns>
        public static double Kross(this Vector2d v1, Vector2d v2)
        {
            return v1.X * v2.Y - v1.Y * v2.X;
        }

        /// <summary>
        /// 线段相交判定 / Segment intersection decision
        /// </summary>
        /// <param name="a1">线段a点1</param>
        /// <param name="a2">线段a点2</param>
        /// <param name="b1">线段b点1</param>
        /// <param name="b2">线段b点2</param>
        /// <returns>结果</returns>
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
        /// 用直线把闭合多段线切成两个闭合的部分 / Cut a closed polyline into two closed sections with a straight line
        /// </summary>
        /// <param name="loop">闭合多段线</param>
        /// <param name="cut">切割线</param>
        /// <returns>结果</returns>
        public static Polyline[] CutLoopToHalves(Polyline loop, Line cut)
        {
            if (loop.EndPoint != loop.StartPoint)
            {
                return new Polyline[0];
            }
            Point3dCollection points = new Point3dCollection();
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
            Polyline poly1 = new Polyline();
            Polyline poly2 = new Polyline();

            // 不含开始结束点的那一半
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

            // 包含开始结束点的那一半
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
        /// 获取多段线一个弧段的一个子集对应的凸度 / Get the convexity corresponding to a subset of one arc segment of a polyline
        /// </summary>
        /// <param name="poly">多段线</param>
        /// <param name="startParam">起始参数</param>
        /// <param name="endParam">结束参数</param>
        /// <returns>凸度</returns>
        public static double GetBulgeBetween(this Polyline poly, double startParam, double endParam)
        {
            double total = poly.GetBulgeAt((int)Math.Floor(startParam));
            return (endParam - startParam) * total;
        }

        /// <summary>
        /// 对Closed为True的多段线，改为False，并通过增加一个点来真正闭合。/ For polylines with Closed True, change to False and close by adding a point.
        /// </summary>
        /// <param name="poly">多段线</param>
        /// <returns>多段线</returns>
        public static Polyline TrulyClose(this Polyline poly)
        {
            if (poly.Closed == false)
            {
                return poly;
            }
            Polyline result = poly.Clone() as Polyline;
            result.Closed = false;
            if (result.EndPoint != result.StartPoint)
            {
                result.AddVertexAt(poly.NumberOfVertices, poly.StartPoint.ToPoint2d(), 0, 0, 0);
            }
            return result;
        }

        /// <summary>
        /// 偏移多段线 / Offset polyline
        /// </summary>
        /// <param name="poly">多段线</param>
        /// <param name="offsets">各段偏移量</param>
        /// <returns>结果</returns>
        public static Polyline OffsetPoly(this Polyline poly, double[] offsets)
        {
            poly = poly.TrulyClose();

            List<double> bulges = new List<double>();
            List<Polyline> segs1 = new List<Polyline>();
            for (int i = 0; i < poly.NumberOfVertices - 1; i++)
            {
                Polyline subPoly = new Polyline();
                subPoly.AddVertexAt(0, poly.GetPointAtParameter(i).ToPoint2d(), poly.GetBulgeAt(i), 0, 0);
                subPoly.AddVertexAt(1, poly.GetPointAtParameter(i + 1).ToPoint2d(), 0, 0, 0);
                var temp = subPoly.GetOffsetCurves(offsets[i]);
                if (temp.Count > 0)
                {
                    segs1.Add(temp[0] as Polyline);
                    bulges.Add(poly.GetBulgeAt(i));
                }
            }
            Point3dCollection points = new Point3dCollection();
            Enumerable.Range(0, segs1.Count).ToList().ForEach(x =>
            {
                int count = points.Count;
                int y = x + 1 < segs1.Count ? x + 1 : 0;
                segs1[x].IntersectWith3264(segs1[y], Autodesk.AutoCAD.DatabaseServices.Intersect.ExtendBoth, points);
                if (points.Count - count > 1) // 本段为圆弧，有两个交点
                {
                    Point3d a = points[points.Count - 2];
                    Point3d b = points[points.Count - 1];
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
            Polyline result = new Polyline();

            int j = 0;
            points.Cast<Point3d>().ToList().ForEach(x =>
                {
                    double bulge = j + 1 < points.Count ? bulges[j + 1] : 0;
                    result.AddVertexAt(j, x.ToPoint2d(), bulge, 0, 0);
                    j++;
                });
            if (poly.StartPoint == poly.EndPoint) // 若闭合，将末段与首段交点也加到开头
            {
                result.AddVertexAt(0, points[points.Count - 1].ToPoint2d(), bulges[0], 0, 0);
            }
            else // 若不闭合，在首尾加上偏移点而不是偏移线交点。
            {
                result.AddVertexAt(0, segs1[0].StartPoint.ToPoint2d(), bulges[0], 0, 0);
                result.AddVertexAt(result.NumberOfVertices, segs1.Last().EndPoint.ToPoint2d(), 0, 0, 0);
                if (result.NumberOfVertices > 3)
                {
                    result.RemoveVertexAt(result.NumberOfVertices - 2); // 放前面有可能出现几何退化异常
                }
            }
            return result;
        }

        /// <summary>
        /// 计算圆弧凸度 / Calculate arc bulge
        /// </summary>
        /// <param name="arc">圆弧</param>
        /// <param name="start">起点</param>
        /// <returns>凸度</returns>
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
        /// 直线转多段线 / Straight line to polyline
        /// </summary>
        /// <param name="line">直线</param>
        /// <returns>多段线</returns>
        public static Polyline ToPolyline(this Line line)
        {
            Polyline poly = new Polyline();
            poly.AddVertexAt(0, line.StartPoint.ToPoint2d(), 0, 0, 0);
            poly.AddVertexAt(1, line.EndPoint.ToPoint2d(), 0, 0, 0);
            return poly;
        }

        /// <summary>
        /// 圆弧转多段线 / Arc to Polyline
        /// </summary>
        /// <param name="arc">圆弧</param>
        /// <returns>多段线</returns>
        public static Polyline ToPolyline(this Arc arc)
        {
            Polyline poly = new Polyline();
            poly.AddVertexAt(0, arc.StartPoint.ToPoint2d(), arc.GetArcBulge(arc.StartPoint), 0, 0);
            poly.AddVertexAt(1, arc.EndPoint.ToPoint2d(), 0, 0, 0);
            return poly;
        }

        /// <summary>
        /// 多段线清理：去除重复点 / Polyline Cleanup: Remove duplicate points
        /// </summary>
        /// <param name="poly">多段线</param>
        public static int PolyClean_RemoveDuplicatedVertex(Polyline poly)
        {
            var points = poly.GetPolyPoints().ToArray();
            List<int> dupIndices = new List<int>();
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
        /// 多段线清理：去除过近点 / Polyline Cleanup: Remove Near Points
        /// </summary>
        /// <param name="poly">多段线</param>
        /// <param name="epsilon">距离容差</param>
        public static int PolyClean_ReducePoints(Polyline poly, double epsilon)
        {
            var points = poly.GetPolyPoints().ToArray();
            List<int> cleanList = new List<int>();
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
        /// 多段线清理：去除多余共线点 / Polyline Cleanup: Remove Excess Collinear 
        /// </summary>
        /// <param name="poly">多段线</param>
        public static void PolyClean_RemoveColinearPoints(Polyline poly)
        {
            var points = poly.GetPolyPoints().ToArray();
            List<int> cleanList = new List<int>();
            int j = 0;
            for (int i = 1; i < points.Length; i++)
            {
                // todo: 实现去除共线点
            }
        }

        /// <summary>
        /// 多段线清理：规整方向 / Polyline Cleanup: Regular 
        /// </summary>
        /// <param name="poly">多段线</param>
        /// <param name="dir">方向</param>
        public static int PolyClean_SetTopoDirection(Polyline poly, Direction dir)
        {
            int count = 0;
            if (poly.StartPoint == poly.EndPoint)
            {
                return 0;
            }
            var delta = poly.EndPoint - poly.StartPoint;
            switch (dir)
            {
                case Direction.West:
                    if (delta.X > 0)
                    {
                        poly.ReverseCurve();
                        count++;
                    }
                    break;
                case Direction.North:
                    if (delta.Y < 0)
                    {
                        poly.ReverseCurve();
                        count++;
                    }
                    break;
                case Direction.East:
                    if (delta.X < 0)
                    {
                        poly.ReverseCurve();
                        count++;
                    }
                    break;
                case Direction.South:
                    if (delta.Y > 0)
                    {
                        poly.ReverseCurve();
                        count++;
                    }
                    break;
                default:
                    break;
            }
            return count;
        }

        /// <summary>
        /// 方向 / direction
        /// </summary>
        public enum Direction
        {
            /// <summary>
            /// 无
            /// </summary>
            None,
            /// <summary>
            /// 西
            /// </summary>
            West,
            /// <summary>
            /// 北
            /// </summary>
            North,
            /// <summary>
            /// 东
            /// </summary>
            East,
            /// <summary>
            /// 南
            /// </summary>
            South
        }

        /// <summary>
        /// 连接多段线 / Connect polyline
        /// </summary>
        /// <param name="poly">多段线</param>
        /// <param name="poly1">多段线1</param>
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
        /// 连接多段线 / Connect polyline
        /// </summary>
        /// <param name="poly1">多段线1</param>
        /// <param name="poly2">多段线2</param>
        /// <returns>连接结果</returns>
        public static Polyline PolyJoin(this Polyline poly1, Polyline poly2)
        {
            int index = 0;
            Polyline poly = new Polyline();
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
        /// 连接多段线 / Connect polyline
        /// </summary>
        /// <param name="polys">多段线列表</param>
        /// <returns>连接结果</returns>
        public static Polyline PolyJoin(this List<Polyline> polys) // newly 20130807
        {
            return polys.Aggregate(Algorithms.PolyJoin);
        }

        /// <summary>
        /// 连接多段线，忽略中间顶点 / Connect polylines, ignoring intermediate vertices
        /// </summary>
        /// <param name="polys">多段线列表</param>
        /// <returns>连接结果</returns>
        public static Polyline PolyJoinIgnoreMiddleVertices(this List<Polyline> polys) // newly 20130807
        {
            Polyline poly = new Polyline();
            var points = polys.SelectMany(p => new[] { p.StartPoint, p.EndPoint }).Distinct().ToList();
            return NoDraw.Pline(points);
        }

        /// <summary>
        /// 点在多段线内判断 / Points are joined within polylines
        /// </summary>
        /// <param name="poly">多段线</param>
        /// <param name="p">点</param>
        /// <returns>结果</returns>
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
        /// 多段线形心 / Polyline center
        /// </summary>
        /// <param name="poly">多段线</param>
        /// <returns>形心</returns>
        public static Point3d Centroid(this Polyline poly) // newly 20130801
        {
            var points = poly.GetPoints().ToList();
            if (points.Count == 1)
            {
                return points[0];
            }
            else
            {
                Point3d temp = Point3d.Origin;
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

        #region 辅助算法 / Auxiliary algorithm

        /// <summary>
        /// 获取世界坐标到视口坐标的变换矩阵 / Get the transformation matrix of world coordinates to viewport coordinates
        /// </summary>
        /// <param name="viewCenter"></param>
        /// <param name="viewportCenter"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static Matrix3d WorldToViewport(Point3d viewCenter, Point3d viewportCenter, double scale)
        {
            //return Matrix3d.Displacement(viewportCenter - viewCenter);
            return Matrix3d.Scaling(1.0 / scale, viewportCenter).PostMultiplyBy(Matrix3d.Displacement(viewportCenter - viewCenter));
        }

        /// <summary>
        /// 实体相交 / Physical intersection
        /// </summary>
        /// <param name="ent">实体</param>
        /// <param name="entOther">另一实体</param>
        /// <param name="intersectType">相交类型</param>
        /// <param name="points">交点容器</param>
        public static void IntersectWith3264(this Entity ent, Entity entOther, Intersect intersectType, Point3dCollection points)
        {
            // 32位与64位AutoCAD应用程序API不同，使用运行时绑定。
            System.Reflection.MethodInfo mi = typeof(Entity).GetMethod("IntersectWith",
                new Type[] { typeof(Entity), typeof(Intersect), typeof(Point3dCollection), typeof(long), typeof(long) });
            if (mi == null) // 32位AutoCAD
            {
                mi = typeof(Entity).GetMethod("IntersectWith",
                new Type[] { typeof(Entity), typeof(Intersect), typeof(Point3dCollection), typeof(int), typeof(int) });
            }
            mi.Invoke(ent, new object[] { entOther, intersectType, points, 0, 0 });
        }

        /// <summary>
        /// 实体相交求交点 / Entity intersect intersection newly 20140805
        /// </summary>
        /// <param name="ent">实体</param>
        /// <param name="entOther">另一实体</param>
        /// <param name="intersectType">相交类型</param>
        /// <returns>交点集合</returns>
        public static List<Point3d> Intersect(this Entity ent, Entity entOther, Intersect intersectType)
        {
            Point3dCollection points = new Point3dCollection();
            IntersectWith3264(ent, entOther, intersectType, points);
            return points.Cast<Point3d>().ToList();
        }

        /// <summary>
        /// 字符串转双精度浮点 / String to double-precision floating point
        /// </summary>
        /// <param name="s">字符串</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>结果</returns>
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
        /// 字符串转双精度浮点，失败得到0 / String to double-precision floating point, failed to get 0
        /// </summary>
        /// <param name="s">字符串</param>
        /// <returns>结果</returns>
        public static double ToDouble(this string s)
        {
            return s.ToDouble(0);
        }

        /// <summary>
        /// 取余运算 / Retrieval operation
        /// </summary>
        /// <param name="n">被除数</param>
        /// <param name="m">模</param>
        /// <returns>余数，[0, m)</returns>
        public static int Mod(this int n, int m)
        {
            return (n > 0) ? (n % m) : (n % m + m);
        }

        /// <summary>
        /// 向量夹角：[0, Pi] / Vector angle: [0, Pi]
        /// </summary>
        /// <param name="v0">向量0</param>
        /// <param name="v1">向量1</param>
        /// <returns>结果</returns>
        public static double ZeroToPiAngleTo(this Vector2d v0, Vector2d v1)
        {
            return v0.GetAngleTo(v1);
        }

        /// <summary>
        /// 向量方位角：[0, 2Pi] / Vector azimuth: [0, 2Pi]
        /// </summary>
        /// <param name="v0">向量0</param>
        /// <returns>结果</returns>
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
        /// 向量夹角：[0, 2Pi]
        /// </summary>
        /// <param name="v0">向量0</param>
        /// <param name="v1">向量1</param>
        /// <returns>结果</returns>
        public static double ZeroTo2PiAngleTo(this Vector2d v0, Vector2d v1)
        {
            double angle0 = v0.DirAngleZeroTo2Pi();
            double angle1 = v1.DirAngleZeroTo2Pi();
            double angleDelta = angle1 - angle0;
            if (angleDelta < 0) angleDelta = angleDelta + 2 * Math.PI;
            return angleDelta;
        }

        /// <summary>
        /// 向量夹角：[-Pi, Pi]
        /// </summary>
        /// <param name="v0">向量0</param>
        /// <param name="v1">向量1</param>
        /// <returns>结果</returns>
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

        #region 应用级别算法

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
            List<Polyline> plines = new List<Polyline>();
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
    /// 面域边界环提取
    /// </summary>
    public static class RegionLoopService
    {
        /// <summary>
        /// 合并相邻边界
        /// </summary>
        /// <param name="ids">边界集合</param>
        /// <returns>合并后的边界集合</returns>
        public static List<ObjectId> MergeBoundary(IEnumerable<ObjectId> ids)
        {
            if (ids.Count() < 2)
            {
                return new List<ObjectId>(ids);
            }

            Region region = null;
            //将所有相邻的地块合并
            foreach (ObjectId id in ids)
            {
                DBObjectCollection objArr = new DBObjectCollection();
                objArr.Add(id.QOpenForRead());
                Region regionSub = Region.CreateFromCurves(objArr)[0] as Region;
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
            //地块最外围的边界
            var loops = region.GetLoops().Select(x => x.AddToCurrentSpace()).ToArray();
            return new List<ObjectId>(loops);
        }

        /// <summary>
        /// 获取面域边界环
        /// </summary>
        /// <param name="region">面域</param>
        /// <returns>边界环数组</returns>
        public static Polyline[] GetLoops(this Region region)
        {
            DBObjectCollection explodeResult = new DBObjectCollection();
            region.Explode(explodeResult);
            if (explodeResult[0] is Curve)
            {
                return new Polyline[] { GetLoop(GroupLoops(explodeResult)[0]) };
            }
            else // explodeResult[0] is Region
            {
                List<Polyline> polys = new List<Polyline>();
                foreach (Region sub in explodeResult)
                {
                    DBObjectCollection subResult = new DBObjectCollection();
                    sub.Explode(subResult);
                    polys.Add(GetLoop(GroupLoops(subResult)[0]));
                }
                return polys.ToArray();
            }
        }

        #region Private...

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
            Polyline result = new Polyline();
            Point3d next = start;
            int i = 0;
            foreach (Curve line in lines)
            {
                double bulge = line is Arc ? Algorithms.GetArcBulge(line as Arc, next) : 0;
                result.AddVertexAt(i, next.ToPoint2d(), bulge, 0, 0);
                next = NextPoint(line, next);
                i++;
            }
            result.AddVertexAt(i, start.ToPoint2d(), 0, 0, 0);
            return result;
        }

        //把炸开的线集按成环分组
        private static List<DBObjectCollection> GroupLoops(DBObjectCollection lines)
        {
            List<DBObjectCollection> groups = new List<DBObjectCollection>();
            int i = 0;
            while (i < lines.Count - 1)
            {
                DBObjectCollection group = IncludingRing(lines, lines[i] as Curve);
                groups.Add(group);
                i += group.Count;
            }
            return groups;
        }

        //测试用。通过此函数输出的文本文件得知，炸开后的线集是按照环来组织的，但并不是按照环中线的顺序排列的。
        private static void ShowExplodeResult(DBObjectCollection lines)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Curve line in lines)
            {
                sb.AppendLine(string.Format("({0},{1})->({2},{3})", line.StartPoint.X, line.StartPoint.Y, line.EndPoint.X, line.EndPoint.Y));
            }
            System.IO.StreamWriter sw = new System.IO.StreamWriter("C:\\ShowExplodeResult.txt");
            sw.Write(sb.ToString());
            sw.Close();
        }

        //获取线的另一端
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

        //获取下一条线
        private static Curve NextLine(DBObjectCollection lines, Curve line, Point3d point)
        {
            return lines.Cast<Curve>().First(x => (x.StartPoint == point || x.EndPoint == point) && x != line);
        }

        //获取其元素组成的环包含指定线的线集
        private static DBObjectCollection IncludingRing(DBObjectCollection lines, Curve origin)
        {
            DBObjectCollection ring = new DBObjectCollection();
            ring.Add(origin);
            Curve next = origin;
            Point3d point = origin.EndPoint;//从第一条线的终点开始遍历
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
    /// 区间
    /// </summary>
    public class Interv : Tuple<double, double>
    {
        /// <summary>
        /// 指定上下限创建区间
        /// </summary>
        /// <param name="start">下限</param>
        /// <param name="end">上限</param>
        public Interv(double start, double end)
            : base(start, end)
        {
        }

        /// <summary>
        /// 下限
        /// </summary>
        public double Start { get { return Math.Min(base.Item1, base.Item2); } }
        /// <summary>
        /// 上限
        /// </summary>
        public double End { get { return Math.Max(base.Item1, base.Item2); } }
        /// <summary>
        /// 长度
        /// </summary>
        public double Length { get { return End - Start; } }

        /// <summary>
        /// 添加点
        /// </summary>
        /// <param name="point">点</param>
        /// <returns>结果</returns>
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
        /// 添加区间
        /// </summary>
        /// <param name="interval">区间</param>
        /// <returns>结果</returns>
        public Interv AddInterval(Interv interval)
        {
            return this.AddPoint(interval.Start).AddPoint(interval.End);
        }

        /// <summary>
        /// 值在区间判断
        /// </summary>
        /// <param name="point">值</param>
        /// <returns>结果</returns>
        public bool IsPointIn(double point)
        {
            return point >= Start && point <= End;
        }
    }
}
