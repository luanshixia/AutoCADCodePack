using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using ColorDialog = Autodesk.AutoCAD.Windows.ColorDialog;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

namespace AutoCADCommands
{
    /// <summary>
    /// Command-line user interactions.
    /// </summary>
    public class Interaction
    {
        /// <summary>
        /// Gets the MDI active docutment's editor.
        /// </summary>
        public static Editor ActiveEditor
        {
            get
            {
                return Application.DocumentManager.MdiActiveDocument.Editor;
            }
        }

        /// <summary>
        /// Writes message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void Write(string message)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage(message);
        }

        /// <summary>
        /// Writes message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void Write(string message, params object[] args)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage(message, args);
        }

        /// <summary>
        /// Writes message line.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void WriteLine(string message)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("\n");
            ed.WriteMessage(message);
        }

        /// <summary>
        /// Writes message line.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void WriteLine(string message, params object[] args)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("\n");
            ed.WriteMessage(message, args);
        }

        /// <summary>
        /// Gets string.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The string.</returns>
        public static string GetString(string message)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.GetString(message);
            if (res.Status == PromptStatus.OK)
            {
                return res.StringResult;
            }

            return null;
        }

        /// <summary>
        /// Gets string.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The string.</returns>
        public static string GetString(string message, string defaultValue)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.GetString(new PromptStringOptions(message) { DefaultValue = defaultValue });
            if (res.Status == PromptStatus.OK)
            {
                return res.StringResult;
            }

            return null;
        }

        /// <summary>
        /// Gets keywords.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="keywords">The keywords.</param>
        /// <param name="defaultIndex">The default index.</param>
        /// <returns>The keyword result.</returns>
        public static string GetKeywords(string message, string[] keywords, int defaultIndex = 0)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var opt = new PromptKeywordOptions(message)
            {
                AllowNone = true
            }; // mod 20140527

            keywords.ToList().ForEach(key => opt.Keywords.Add(key));
            opt.Keywords.Default = keywords[defaultIndex];

            var res = ed.GetKeywords(opt);
            if (res.Status == PromptStatus.OK)
            {
                return res.StringResult;
            }

            return null;
        }

        /// <summary>
        /// Gets numeric value.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value.</returns>
        public static double GetValue(string message, double? defaultValue = null)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var res = defaultValue == null
                ? ed.GetDouble(new PromptDoubleOptions(message) { AllowNone = true })
                : ed.GetDouble(new PromptDoubleOptions(message) { DefaultValue = defaultValue.Value, AllowNone = true });

            if (res.Status == PromptStatus.OK)
            {
                return res.Value;
            }

            return double.NaN;
        }

        /// <summary>
        /// Gets distance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The distance.</returns>
        public static double GetDistance(string message)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.GetDistance(message);
            if (res.Status == PromptStatus.OK)
            {
                return res.Value;
            }

            return double.NaN;
        }

        /// <summary>
        /// Gets angle.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The angle.</returns>
        public static double GetAngle(string message)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.GetAngle(message);
            if (res.Status == PromptStatus.OK)
            {
                return res.Value;
            }

            return double.NaN;
        }

        /// <summary>
        /// Gets point.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The point.</returns>
        public static Point3d GetPoint(string message)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var opt = new PromptPointOptions(message)
            {
                AllowNone = true
            };

            var res = ed.GetPoint(opt);
            if (res.Status == PromptStatus.OK)
            {
                return res.Value;
            }

            return Algorithms.NullPoint3d;
        }

        /// <summary>
        /// Gets another point of a line.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="startPoint">The first point.</param>
        /// <returns>The point.</returns>
        public static Point3d GetLineEndPoint(string message, Point3d startPoint)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var jig = new LineJig(startPoint, message);
            var res = ed.Drag(jig);
            if (res.Status == PromptStatus.OK)
            {
                return jig.EndPoint;
            }

            return Algorithms.NullPoint3d;
        }

        /// <summary>
        /// Get corner point.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="basePoint">The first point.</param>
        /// <returns>The point.</returns>
        public static Point3d GetCorner(string message, Point3d basePoint)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var opt = new PromptCornerOptions(message, basePoint) { AllowNone = true }; // mod 20140527
            var res = ed.GetCorner(opt);
            if (res.Status == PromptStatus.OK)
            {
                return res.Value;
            }

            return Algorithms.NullPoint3d;
        }

        /// <summary>
        /// Gets 2-d extents.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The extents.</returns>
        public static Extents3d? GetExtents(string message)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.GetPoint(message);
            if (res.Status != PromptStatus.OK)
            {
                return null;
            }

            var p1 = res.Value;
            res = ed.GetCorner(message, p1);
            if (res.Status != PromptStatus.OK)
            {
                return null;
            }

            var p2 = res.Value;
            return new Extents3d(
                new Point3d(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y), 0), 
                new Point3d(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y), 0));
        }

        /// <summary>
        /// Gets entity.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The entity ID.</returns>
        public static ObjectId GetEntity(string message)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.GetEntity(message);
            if (res.Status == PromptStatus.OK)
            {
                return res.ObjectId;
            }

            return ObjectId.Null;
        }

        /// <summary>
        /// Gets entity.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="allowedType">The allowed entity type.</param>
        /// <param name="exactMatch">Use exact match.</param>
        /// <returns>The entity ID.</returns>
        public static ObjectId GetEntity(string message, Type allowedType, bool exactMatch = true) // newly 20130514
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var opt = new PromptEntityOptions(message);
            opt.SetRejectMessage("Allowed type: " + allowedType.Name); // Must call this first
            opt.AddAllowedClass(allowedType, exactMatch);
            var res = ed.GetEntity(opt);
            if (res.Status == PromptStatus.OK)
            {
                return res.ObjectId;
            }

            return ObjectId.Null;
        }

        /// <summary>
        /// Gets entity and pick position.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The entity ID and the pick position.</returns>
        public static (ObjectId, Point3d)? GetPick(string message)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.GetEntity(message);
            if (res.Status == PromptStatus.OK)
            {
                return (res.ObjectId, res.PickedPoint);
            }

            return null;
        }

        /// <summary>
        /// Gets multiple entities.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The entity IDs.</returns>
        public static ObjectId[] GetSelection(string message)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var opt = new PromptSelectionOptions { MessageForAdding = message };
            ed.WriteMessage(message);
            var res = ed.GetSelection(opt);
            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds();
            }

            return Array.Empty<ObjectId>();
        }

        /// <summary>
        /// Gets multiple entities.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>The entity IDs.</returns>
        public static ObjectId[] GetSelection(string message, params (int, object)[] filter)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var opt = new PromptSelectionOptions { MessageForAdding = message };
            ed.WriteMessage(message);
            var res = ed.GetSelection(opt, new SelectionFilter(filter.Select(x => new TypedValue(x.Item1, x.Item2)).ToArray()));
            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds();
            }

            return Array.Empty<ObjectId>();
        }

        /// <summary>
        /// Gets multiple entities.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="allowedType">The allowed types. e.g. "*LINE,ARC,CIRCLE"</param>
        /// <returns>The entity IDs.</returns>
        public static ObjectId[] GetSelection(string message, string allowedType)
        {
            return Interaction.GetSelection(message, ((int)DxfCode.Start, allowedType));
        }

        /// <summary>
        /// Gets multiple entities by window.
        /// </summary>
        /// <param name="pt1">The corner 1.</param>
        /// <param name="pt2">The corner 2.</param>
        /// <param name="allowedType">The allowed types.</param>
        /// <returns>The selection.</returns>
        public static ObjectId[] GetWindowSelection(Point3d pt1, Point3d pt2, string allowedType = "*")
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.SelectWindow(pt1, pt2, new SelectionFilter(new[] { new TypedValue((int)DxfCode.Start, allowedType) }));
            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds();
            }

            return Array.Empty<ObjectId>();
        }

        /// <summary>
        /// Gets multiple entities by crossing.
        /// </summary>
        /// <param name="pt1">The corner 1.</param>
        /// <param name="pt2">The corner 2.</param>
        /// <param name="allowedType">The allowed types.</param>
        /// <returns>The selection.</returns>
        public static ObjectId[] GetCrossingSelection(Point3d pt1, Point3d pt2, string allowedType = "*")
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.SelectCrossingWindow(pt1, pt2, new SelectionFilter(new[] { new TypedValue((int)DxfCode.Start, allowedType) }));
            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds();
            }

            return Array.Empty<ObjectId>();
        }

        /// <summary>
        /// Gets the pick set.
        /// </summary>
        /// <remarks>
        /// Don't forget to add CommandFlags.UsePickSet to CommandMethod.
        /// </remarks>
        /// <returns>The selection.</returns>
        public static ObjectId[] GetPickSet()
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.SelectImplied();
            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds();
            }

            return Array.Empty<ObjectId>();
        }

        /// <summary>
        /// Sets the pick set.
        /// </summary>
        /// <param name="entityIds">The entity IDs.</param>
        public static void SetPickSet(ObjectId[] entityIds)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.SetImpliedSelection(entityIds);
        }

        /// <summary>
        /// Gets the last added entity.
        /// </summary>
        /// <returns>The entity ID.</returns>
        public static ObjectId GetNewestEntity()
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.SelectLast();
            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds()[0];
            }

            return ObjectId.Null;
        }

        /// <summary>
        /// Gets the last added entities.
        /// </summary>
        /// <returns>The entity IDs.</returns>
        public static ObjectId[] GetNewestEntities()
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.SelectLast();
            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds();
            }

            return Array.Empty<ObjectId>();
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SetFocus(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr child, IntPtr parent);

        /// <summary>
        /// Sets focus to the active document.
        /// </summary>
        public static void SetActiveDocFocus()
        {
            Interaction.SetFocus(Application.DocumentManager.MdiActiveDocument.Window.Handle);
        }

        /// <summary>
        /// Sets the current layer.
        /// </summary>
        /// <param name="layerName">The layer name.</param>
        public static void SetCurrentLayer(string layerName)
        {
            HostApplicationServices.WorkingDatabase.Clayer = DbHelper.GetLayerId(layerName);
        }

        /// <summary>
        /// Sends command to execute.
        /// </summary>
        /// <param name="command">The command.</param>
        public static void Command(string command)
        {
            Application.DocumentManager.MdiActiveDocument.SendStringToExecute(command, false, false, false);
        }

        /// <summary>
        /// Starts new command.
        /// </summary>
        /// <param name="command">The command.</param>
        public static void StartCommand(string command)
        {
            var existingCommands = Application.GetSystemVariable("CMDNAMES").ToString();
            var escapes = existingCommands.Length > 0
                ? string.Join(string.Empty, Enumerable.Repeat('\x03', existingCommands.Split('\'').Length))
                : string.Empty;

            Application.DocumentManager.MdiActiveDocument.SendStringToExecute(escapes + command, true, false, true);
        }

        /// <summary>
        /// Shows a task dialog.
        /// </summary>
        /// <param name="mainInstruction">The main instruction.</param>
        /// <param name="yesChoice">The yes choice.</param>
        /// <param name="noChoice">The no choice.</param>
        /// <param name="title">The dialog title.</param>
        /// <param name="content">The content.</param>
        /// <param name="footer">The footer.</param>
        /// <param name="expanded">The expanded text.</param>
        /// <returns>The choice.</returns>
        public static bool TaskDialog(string mainInstruction, string yesChoice, string noChoice, string title = "AutoCAD", string content = "", string footer = "", string expanded = "")
        {
            var td = new TaskDialog
            {
                WindowTitle = title,
                MainInstruction = mainInstruction,
                ContentText = content,
                MainIcon = TaskDialogIcon.Information,
                FooterIcon = TaskDialogIcon.Warning,
                FooterText = footer,
                CollapsedControlText = "Details",
                ExpandedControlText = "Details",
                ExpandedByDefault = false,
                ExpandedText = expanded,
                AllowDialogCancellation = false,
                UseCommandLinks = true
            };
            td.Buttons.Add(new TaskDialogButton(1, yesChoice));
            td.Buttons.Add(new TaskDialogButton(2, noChoice));
            td.DefaultButton = 1;
            int[] btnId = null;
            td.Callback = (atd, e, sender) =>
            {
                if (e.Notification == TaskDialogNotification.ButtonClicked)
                {
                    btnId = new int[3];
                    btnId[e.ButtonId] = 1;
                }
                return false;
            };
            td.Show(Application.MainWindow.Handle);
            if (btnId.ToList().IndexOf(1) == 1)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Highlights entities.
        /// </summary>
        /// <param name="entityIds">The entity IDs.</param>
        public static void HighlightObjects(IEnumerable<ObjectId> entityIds)
        {
            entityIds.QForEach<Entity>(x => x.Highlight());
        }

        /// <summary>
        /// Unhighlights entities.
        /// </summary>
        /// <param name="entityIds">The entity IDs.</param>
        public static void UnhighlightObjects(IEnumerable<ObjectId> entityIds)
        {
            entityIds.QForEach<Entity>(x => x.Unhighlight());
        }

        /// <summary>
        /// Zooms to entities' extents.
        /// </summary>
        /// <param name="entityIds">The entity IDs.</param>
        public static void ZoomObjects(IEnumerable<ObjectId> entityIds)
        {
            var extent = entityIds.GetExtents();
            Interaction.ZoomView(extent);
        }

        /// <summary>
        /// Zooms to extents.
        /// </summary>
        /// <param name="extents">The extents.</param>
        public static void ZoomView(Extents3d extents)
        {
            Interaction.Zoom(extents.MinPoint, extents.MaxPoint, new Point3d(), 1);
        }

        /// <summary>
        /// Zooms to all entities' extents.
        /// </summary>
        public static void ZoomExtents()
        {
            if (HostApplicationServices.WorkingDatabase.TileMode) // Model space
            {
                Interaction.Zoom(HostApplicationServices.WorkingDatabase.Extmin, HostApplicationServices.WorkingDatabase.Extmax, new Point3d(), 1);
            }
            else // Paper space
            {
                Interaction.Zoom(new Point3d(), new Point3d(), new Point3d(), 1);
            }
        }

        /// <summary>
        /// The internal Zoom() method (credit: AutoCAD .NET Developer's Guide).
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="center"></param>
        /// <param name="factor"></param>
        internal static void Zoom(Point3d min, Point3d max, Point3d center, double factor)
        {
            // Get the current document and database
            var document = Application.DocumentManager.MdiActiveDocument;
            var database = document.Database;

            int currentViewport = Convert.ToInt32(Application.GetSystemVariable("CVPORT"));

            // Get the extents of the current space no points
            // or only a center point is provided
            // Check to see if Model space is current
            if (database.TileMode)
            {
                if (min.Equals(new Point3d()) && max.Equals(new Point3d()))
                {
                    min = database.Extmin;
                    max = database.Extmax;
                }
            }
            else
            {
                // Check to see if Paper space is current
                if (currentViewport == 1)
                {
                    // Get the extents of Paper space
                    if (min.Equals(new Point3d()) && max.Equals(new Point3d()))
                    {
                        min = database.Pextmin;
                        max = database.Pextmax;
                    }
                }
                else
                {
                    // Get the extents of Model space
                    if (min.Equals(new Point3d()) && max.Equals(new Point3d()))
                    {
                        min = database.Extmin;
                        max = database.Extmax;
                    }
                }
            }

            // Start a transaction
            using (var trans = database.TransactionManager.StartTransaction())
            {
                // Get the current view
                using (var currentView = document.Editor.GetCurrentView())
                {
                    Extents3d extents;

                    // Translate WCS coordinates to DCS
                    var matWCS2DCS = Matrix3d.PlaneToWorld(currentView.ViewDirection);
                    matWCS2DCS = Matrix3d.Displacement(currentView.Target - Point3d.Origin) * matWCS2DCS;
                    matWCS2DCS = Matrix3d.Rotation(
                        angle: -currentView.ViewTwist,
                        axis: currentView.ViewDirection,
                        center: currentView.Target) * matWCS2DCS;

                    // If a center point is specified, define the min and max
                    // point of the extents
                    // for Center and Scale modes
                    if (center.DistanceTo(Point3d.Origin) != 0)
                    {
                        min = new Point3d(center.X - (currentView.Width / 2), center.Y - (currentView.Height / 2), 0);
                        max = new Point3d((currentView.Width / 2) + center.X, (currentView.Height / 2) + center.Y, 0);
                    }

                    // Create an extents object using a line
                    using (Line line = new Line(min, max))
                    {
                        extents = new Extents3d(line.Bounds.Value.MinPoint, line.Bounds.Value.MaxPoint);
                    }

                    // Calculate the ratio between the width and height of the current view
                    double viewRatio = currentView.Width / currentView.Height;

                    // Tranform the extents of the view
                    matWCS2DCS = matWCS2DCS.Inverse();
                    extents.TransformBy(matWCS2DCS);

                    double width;
                    double height;
                    Point2d newCenter;

                    // Check to see if a center point was provided (Center and Scale modes)
                    if (center.DistanceTo(Point3d.Origin) != 0)
                    {
                        width = currentView.Width;
                        height = currentView.Height;

                        if (factor == 0)
                        {
                            center = center.TransformBy(matWCS2DCS);
                        }

                        newCenter = new Point2d(center.X, center.Y);
                    }
                    else // Working in Window, Extents and Limits mode
                    {
                        // Calculate the new width and height of the current view
                        width = extents.MaxPoint.X - extents.MinPoint.X;
                        height = extents.MaxPoint.Y - extents.MinPoint.Y;

                        // Get the center of the view
                        newCenter = new Point2d(
                            ((extents.MaxPoint.X + extents.MinPoint.X) * 0.5),
                            ((extents.MaxPoint.Y + extents.MinPoint.Y) * 0.5));
                    }

                    // Check to see if the new width fits in current window
                    if (width > (height * viewRatio))
                    {
                        height = width / viewRatio;
                    }

                    // Resize and scale the view
                    if (factor != 0)
                    {
                        currentView.Height = height * factor;
                        currentView.Width = width * factor;
                    }

                    // Set the center of the view
                    currentView.CenterPoint = newCenter;

                    // Set the current view
                    document.Editor.SetCurrentView(currentView);
                }

                // Commit the changes
                trans.Commit();
            }
        }

        /// <summary>
        /// Inserts entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="message">The message.</param>
        /// <returns>The entity ID.</returns>
        public static ObjectId InsertEntity(Entity entity, string message = "\nSpecify insert point")
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var jig = new PositionJig(entity, message);
            var res = ed.Drag(jig);
            if (res.Status == PromptStatus.OK)
            {
                return jig.Ent.AddToCurrentSpace();
            }

            return ObjectId.Null;
        }

        /// <summary>
        /// Inserts scaling entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="basePoint">The base point.</param>
        /// <param name="message">The message.</param>
        /// <returns>The entity ID.</returns>
        public static ObjectId InsertScalingEntity(Entity entity, Point3d basePoint, string message = "\nSpecify diagonal point")
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var jig = new ScaleJig(entity, basePoint, message);
            var res = ed.Drag(jig);
            if (res.Status == PromptStatus.OK)
            {
                return jig.Ent.AddToCurrentSpace();
            }

            return ObjectId.Null;
        }

        /// <summary>
        /// Inserts rotation entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="center">The center.</param>
        /// <param name="message">The message.</param>
        /// <returns>The entity ID.</returns>
        public static ObjectId InsertRotationEntity(Entity entity, Point3d center, string message = "\nSpecify direction")
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var jig = new RotationJig(entity, center, message);
            var res = ed.Drag(jig);
            if (res.Status == PromptStatus.OK)
            {
                return jig.Ent.AddToCurrentSpace();
            }

            return ObjectId.Null;
        }

        /// <summary>
        /// Shows OS save file dialog.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="filter">The type filter.</param>
        /// <returns>The file name result.</returns>
        public static string SaveFileDialogBySystem(string title, string fileName, string filter)
        {
            var sfd = new SaveFileDialog
            {
                Title = title,
                FileName = fileName,
                Filter = filter
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                return sfd.FileName;
            }

            return string.Empty;
        }

        /// <summary>
        /// Shows OS open file dialog.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="filter">The type filter.</param>
        /// <returns>The file name result.</returns>
        public static string OpenFileDialogBySystem(string title, string fileName, string filter)
        {
            var ofd = new OpenFileDialog
            {
                Title = title,
                FileName = fileName,
                Filter = filter
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                return ofd.FileName;
            }

            return string.Empty;
        }

        /// <summary>
        /// The OS folder dialog.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <returns>The folder result.</returns>
        public static string FolderDialog(string description)
        {
            var fbd = new FolderBrowserDialog
            {
                Description = description,
                RootFolder = Environment.SpecialFolder.Desktop,
                ShowNewFolderButton = true
            };

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                return fbd.SelectedPath;
            }

            return string.Empty;
        }

        // TODO: file dialog by AutoCAD
        //public static void SaveFileDialogByAutoCAD()
        //{
        //}

        /// <summary>
        /// Shows AutoCAD color dialog.
        /// </summary>
        /// <returns>The color result.</returns>
        public static Color ColorDialog()
        {
            var cd = new ColorDialog();
            if (cd.ShowDialog() == DialogResult.OK)
            {
                return cd.Color;
            }

            return null;
        }

        /// <summary>
        /// Creates polyline interactively.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The polyline result.</returns>
        public static Polyline GetPromptPolyline(string message) // newly 20130806
        {
            var point = Interaction.GetPoint(message);
            if (point.IsNull())
            {
                return null;
            }
            var poly = NoDraw.Pline(new[] { point });
            var prev = point;
            var tempIds = new List<ObjectId>();
            while (true)
            {
                point = Interaction.GetLineEndPoint(message, prev);
                if (point.IsNull())
                {
                    break;
                }
                tempIds.Add(Draw.Line(prev, point));
                poly.AddVertexAt(poly.NumberOfVertices, point.ToPoint2d(), 0, 0, 0);
                prev = point;
            }
            tempIds.QForEach(x => x.Erase());
            return poly;
        }

        /// <summary>
        /// View entities interactively.
        /// </summary>
        /// <param name="entityIds">The entity IDs.</param>
        /// <param name="action">The action.</param>
        public static void ZoomHighlightView(List<ObjectId> entityIds, Action<int> action = null) // newly 20130815
        {
            if (entityIds.Count > 0)
            {
                var highlightIds = new List<ObjectId>();
                while (true)
                {
                    string input = Interaction.GetString("\nType in a number to view, press ENTER to exit: ");
                    if (input == null)
                    {
                        break;
                    }
                    var index = Convert.ToInt32(input);
                    if (index <= 0 || index > entityIds.Count)
                    {
                        Interaction.WriteLine("Invalid entity number.");
                        continue;
                    }

                    action?.Invoke(index);
                    highlightIds.Clear();
                    highlightIds.Add(entityIds[index - 1]);
                    Interaction.ZoomObjects(highlightIds);
                    Interaction.HighlightObjects(highlightIds);
                }
            }
        }

        /// <summary>
        /// Starts a FlexEntityJig drag.
        /// </summary>
        /// <typeparam name="TOptions">The type of JigPromptOptions.</typeparam>
        /// <typeparam name="TResult">The type of jig PromptResult.</typeparam>
        /// <param name="options">The options.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="updateAction">The update action.</param>
        /// <returns>The prompt result.</returns>
        public static PromptResult StartDrag<TOptions, TResult>(TOptions options, Entity entity, Func<Entity, TResult, bool> updateAction)
            where TOptions : JigPromptOptions
            where TResult : PromptResult
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var jig = new FlexEntityJig(options, entity, (ent, result) => updateAction(ent, (TResult)result));
            return ed.Drag(jig);
        }

        /// <summary>
        /// Starts a FlexEntityJig point drag.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="updateAction">The update action.</param>
        /// <returns>The prompt result.</returns>
        public static PromptResult StartDrag(string message, Entity entity, Func<Entity, PromptPointResult, bool> updateAction)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var options = new JigPromptPointOptions(message); // TODO: other options?
            var jig = new FlexEntityJig(options, entity, (ent, result) => updateAction(ent, (PromptPointResult)result));
            return ed.Drag(jig);
        }

        /// <summary>
        /// Starts a FlexDrawJig drag.
        /// </summary>
        /// <typeparam name="TOptions">The type of JigPromptOptions.</typeparam>
        /// <typeparam name="TResult">The type of jig PromptResult.</typeparam>
        /// <param name="options">The options.</param>
        /// <param name="updateAction">The update action.</param>
        /// <returns>The prompt result.</returns>
        public static PromptResult StartDrag<TOptions, TResult>(TOptions options, Func<TResult, Drawable> updateAction)
            where TOptions: JigPromptOptions
            where TResult: PromptResult
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var jig = new FlexDrawJig(options, result => updateAction((TResult)result));
            return ed.Drag(jig);
        }

        /// <summary>
        /// Starts a FlexDrawJig point drag.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="updateAction">The update action.</param>
        /// <returns>The prompt result.</returns>
        public static PromptResult StartDrag(string message, Func<PromptPointResult, Drawable> updateAction)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var options = new JigPromptPointOptions(message); // TODO: other options?
            var jig = new FlexDrawJig(options, result => updateAction((PromptPointResult)result));
            return ed.Drag(jig);
        }
    }

    internal class LineJig : EntityJig
    {
        private Point3d _startPoint;
        private string _message;

        public Point3d EndPoint { get; private set; }

        public LineJig(Point3d startPoint, string message)
            : base(new Line(startPoint, Point3d.Origin))
        {
            _startPoint = startPoint;
            _message = message;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions jppo = new JigPromptPointOptions(_message);
            jppo.Keywords.Add(""); // mod 20140527
            jppo.Cursor = CursorType.RubberBand;
            jppo.BasePoint = _startPoint;
            jppo.UseBasePoint = true;
            Point3d endPoint = prompts.AcquirePoint(jppo).Value;
            if (endPoint.IsNull())
            {
                return SamplerStatus.Cancel;
            }
            else if (endPoint != EndPoint)
            {
                EndPoint = endPoint;
                return SamplerStatus.OK;
            }
            else
            {
                return SamplerStatus.NoChange;
            }
        }

        protected override bool Update()
        {
            try
            {
                Line line = Entity as Line;
                line.EndPoint = EndPoint;
            }
            catch
            {
            }
            return true;
        }
    }

    internal class PositionJig : EntityJig
    {
        private Point3d _pos = Point3d.Origin;
        private Vector3d _move;
        private string _message;

        public Entity Ent { get; }

        public PositionJig(Entity entity, string message)
            : base(entity)
        {
            this.Ent = entity;
            this._message = message;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var jppo = new JigPromptPointOptions(this._message)
            {
                Cursor = CursorType.EntitySelect,
                UseBasePoint = false,
                UserInputControls = UserInputControls.NullResponseAccepted
            };
            jppo.Keywords.Add(""); // mod 20140527
            var pos = prompts.AcquirePoint(jppo).Value;
            if (pos.IsNull())
            {
                return SamplerStatus.Cancel;
            }
            else if (pos != _pos)
            {
                _move = pos - _pos;
                _pos = pos;
                return SamplerStatus.OK;
            }
            else
            {
                _move = pos - _pos;
                return SamplerStatus.NoChange;
            }
        }

        protected override bool Update()
        {
            try
            {
                Ent.TransformBy(Matrix3d.Displacement(_move));
            }
            catch
            {
            }
            return true;
        }
    }

    internal class ScaleJig : EntityJig
    {
        private Point3d _pos = Point3d.Origin;
        private Vector3d _move;
        private Point3d _basePoint;
        private string _message;
        private Extents3d _extents;
        //private double _scale;
        private double _mag;

        public Entity Ent { get; }

        public ScaleJig(Entity entity, Point3d basePoint, string message)
            : base(entity)
        {
            this.Ent = entity;
            this._basePoint = basePoint;
            this._message = message;
            this._extents = Ent.GeometricExtents;
            // this._scale = 1;
            this._mag = 1;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var jppo = new JigPromptPointOptions(_message)
            {
                Cursor = CursorType.EntitySelect,
                UseBasePoint = false,
                UserInputControls = UserInputControls.NullResponseAccepted
            };
            jppo.Keywords.Add(""); // mod 20140527
            var corner = prompts.AcquirePoint(jppo).Value;
            var pos = Point3d.Origin + 0.5 * (_basePoint.GetAsVector() + corner.GetAsVector());
            var extents = Ent.GeometricExtents;
            double scale = Math.Min(
                Math.Abs(corner.X - _basePoint.X) / (extents.MaxPoint.X - extents.MinPoint.X),
                Math.Abs(corner.Y - _basePoint.Y) / (extents.MaxPoint.Y - extents.MinPoint.Y));

            // NOTE: the scale is likely small at the beginning. Too small a scale leads to non-proportional scaling for matrix operation, and thus gets rejected by AutoCAD and causes exception.
            if (scale < Consts.Epsilon)
            {
                scale = Consts.Epsilon;
            }
            if (pos.IsNull())
            {
                return SamplerStatus.Cancel;
            }
            else if (pos != _pos)
            {
                _move = pos - _pos;
                _pos = pos;
                //_mag = scale / _scale;
                //_scale = scale;
                _mag = scale;
                return SamplerStatus.OK;
            }
            else
            {
                _move = pos - _pos;
                return SamplerStatus.NoChange;
            }
        }

        protected override bool Update()
        {
            try
            {
                // NOTE: mind the order.
                Ent.TransformBy(Matrix3d.Displacement(_move));
                Ent.TransformBy(Matrix3d.Scaling(_mag, _pos));
            }
            catch
            {
            }
            return true;
        }
    }

    internal class RotationJig : EntityJig
    {
        private Point3d _center;
        private Point3d _end;
        private string _message;
        private double _angle;

        public Entity Ent => base.Entity;

        public RotationJig(Entity entity, Point3d center, string message)
            : base(entity)
        {
            this._center = center;
            this._message = message;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var jppo = new JigPromptPointOptions(this._message)
            {
                Cursor = CursorType.EntitySelect,
                BasePoint = _center,
                UseBasePoint = true,
                UserInputControls = UserInputControls.NullResponseAccepted
            };
            jppo.Keywords.Add(""); // mod 20140527
            var end = prompts.AcquirePoint(jppo).Value;
            if (end.IsNull())
            {
                return SamplerStatus.Cancel;
            }
            else if (end != _end)
            {
                this._end = end;
                return SamplerStatus.OK;
            }

            return SamplerStatus.NoChange;
        }

        protected override bool Update()
        {
            try
            {
                var dir = this._end - this._center;
                double angle = dir.GetAngleTo(Vector3d.YAxis);
                if (dir.X > 0)
                {
                    angle = Math.PI * 2 - angle;
                }
                Entity.TransformBy(Matrix3d.Rotation(angle - this._angle, Vector3d.ZAxis, this._center));
                this._angle = angle;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
