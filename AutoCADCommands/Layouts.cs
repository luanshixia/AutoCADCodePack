using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dreambuild.AutoCAD
{
    /// <summary>
    /// Layout operations.
    /// </summary>
    public static class Layouts
    {
        /// <summary>
        /// Default plotter.
        /// </summary>
        public const string Device_DWF6 = "DWF6 ePlot.pc3";
        /// <summary>
        /// A3 full landscape.
        /// </summary>
        public const string Media_A3_Full_Landscape = "ISO_full_bleed_A3_(297.00_x_420.00_MM)";
        /// <summary>
        /// A3 expand landscape.
        /// </summary>
        public const string Media_A3_Expand_Landscape = "ISO_expand_A3_(297.00_x_420.00_MM)";

        /// <summary>
        /// The supported scales.
        /// </summary>
        public static ReadOnlyCollection<double> Scales { get; } = new ReadOnlyCollection<double>(new List<double> { 50, 100, 200, 250 });

        /// <summary>
        /// Sets viewport parameters.
        /// </summary>
        /// <param name="viewportId">The viewport ID.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="center">The center.</param>
        /// <param name="lookAt">The look at direction.</param>
        /// <param name="viewHeight">Thew view height.</param>
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
        /// Sets layout configuration.
        /// </summary>
        /// <param name="layoutId">The layout ID.</param>
        /// <param name="device">The plotter.</param>
        /// <param name="media">The media.</param>
        public static void SetConfiguration(ObjectId layoutId, string device, string media)
        {
            layoutId.QOpenForWrite<Layout>(layout =>
            {
                PlotSettingsValidator.Current.SetPlotConfigurationName(layout, device, media);
            });
        }

        /// <summary>
        /// Gets model space coordinates.
        /// </summary>
        /// <param name="layoutCoord">The layout coordinates.</param>
        /// <param name="center">The center.</param>
        /// <param name="lookAt">The look at direction.</param>
        /// <param name="scale">The scale.</param>
        /// <returns>The model space coordinates.</returns>
        public static Point3d GetModelCoord(this Point3d layoutCoord, Point3d center, Point3d lookAt, double scale)
        {
            return lookAt + (layoutCoord - center) * scale;
        }

        /// <summary>
        /// Gets layout coordinates.
        /// </summary>
        /// <param name="modelCoord">The model space coordinates.</param>
        /// <param name="center">The center.</param>
        /// <param name="lookAt">The look at direction.</param>
        /// <param name="scale">The scale.</param>
        /// <returns>The layout coordintes.</returns>
        public static Point3d GetLayoutCoord(this Point3d modelCoord, Point3d center, Point3d lookAt, double scale)
        {
            return center + (modelCoord - lookAt) / scale;
        }
    }
}
