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
    /// 布局操作
    /// </summary>
    public static class Layouts
    {
        /// <summary>
        /// 默认绘图仪
        /// </summary>
        public const string Device_DWF6 = "DWF6 ePlot.pc3";
        /// <summary>
        /// A3横版
        /// </summary>
        public const string Media_A3_Full_Landscape = "ISO_full_bleed_A3_(297.00_x_420.00_MM)";
        /// <summary>
        /// A3横版
        /// </summary>
        public const string Media_A3_Expand_Landscape = "ISO_expand_A3_(297.00_x_420.00_MM)";

        private static List<double> _scales = new List<double> { 50, 100, 200, 250 };
        public static System.Collections.ObjectModel.ReadOnlyCollection<double> Scales { get; private set; }

        static Layouts()
        {
            Scales = new System.Collections.ObjectModel.ReadOnlyCollection<double>(_scales);
        }

        /// <summary>
        /// 创建布局, 如果包含同名布局, 删掉重新创建
        /// </summary>
        /// <param name="layoutName">新建布局的名称</param>
        /// <returns>新建布局的ID</returns>
        public static ObjectId Create(string layoutName)
        {
            return Create(HostApplicationServices.WorkingDatabase, layoutName);
        }

        /// <summary>
        /// 创建布局, 如果包含同名布局, 删掉重新创建
        /// </summary>
        /// <param name="db">数据库</param>
        /// <param name="layoutName">新建布局的名称</param>
        /// <returns>新建布局的ID</returns>
        public static ObjectId Create(Database db, string layoutName)
        {
            ObjectId idLayout = ObjectId.Null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                DBDictionary dic = trans.GetObject(db.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;
                try
                {
                    idLayout = dic.GetAt(layoutName);
                }
                catch
                {
                }
                if (idLayout != ObjectId.Null)
                {
                    LayoutManager.Current.DeleteLayout(layoutName);
                }
                idLayout = LayoutManager.Current.CreateLayout(layoutName);
                trans.Commit();
            }
            return idLayout;
        }

        /// <summary>
        /// 设置容纳XY平面内容的视口
        /// </summary>
        /// <param name="viewportId">视口ID</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="center">中心</param>
        /// <param name="lookAt">视心</param>
        /// <param name="viewHeight">视高</param>
        public static void SetViewport(ObjectId viewportId, double width, double height, Point3d center, Point3d lookAt, double viewHeight)
        {
            viewportId.QOpenForWrite<Viewport>(vPort =>
            {
                vPort.Height = height;
                vPort.Width = width;
                vPort.CenterPoint = center;
                vPort.ViewHeight = viewHeight;
                vPort.ViewCenter = new Point2d(lookAt.X, lookAt.Y); //new Point2d(0, 0);
                vPort.ViewTarget = Point3d.Origin; //lookAt;
                vPort.ViewDirection = Vector3d.ZAxis;
                vPort.Locked = true;
                vPort.On = true;
            });
        }

        /// <summary>
        /// 设置布局配置
        /// </summary>
        /// <param name="layoutId">布局ID</param>
        /// <param name="device">绘图仪</param>
        /// <param name="media">介质</param>
        public static void SetConfiguration(ObjectId layoutId, string device, string media)
        {
            layoutId.QOpenForWrite<Layout>(layout =>
            {
                PlotSettingsValidator.Current.SetPlotConfigurationName(layout, device, media);
            });
        }

        /// <summary>
        /// 获取模型坐标
        /// </summary>
        /// <param name="layoutCoord">布局坐标</param>
        /// <param name="center">中心</param>
        /// <param name="lookAt">视心</param>
        /// <param name="scale">比例尺</param>
        /// <returns>模型坐标</returns>
        public static Point3d GetModelCoord(this Point3d layoutCoord, Point3d center, Point3d lookAt, double scale)
        {
            return lookAt + (layoutCoord - center) * scale;
        }

        /// <summary>
        /// 获取布局坐标
        /// </summary>
        /// <param name="modelCoord">模型坐标</param>
        /// <param name="center">中心</param>
        /// <param name="lookAt">视心</param>
        /// <param name="scale">比例尺</param>
        /// <returns>布局坐标</returns>
        public static Point3d GetLayoutCoord(this Point3d modelCoord, Point3d center, Point3d lookAt, double scale)
        {
            return center + (modelCoord - lookAt) / scale;
        }
    }
}
