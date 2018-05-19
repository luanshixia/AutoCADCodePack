﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;

namespace AutoCADCommands
{
    /// <summary>
    /// 命令行用户交互
    /// </summary>
    public class Interaction
    {
        /// <summary>
        /// 获取活动Editor
        /// </summary>
        public static Editor ActiveEditor
        {
            get
            {
                return Application.DocumentManager.MdiActiveDocument.Editor;
            }
        }

        /// <summary>
        /// 输出命令行信息
        /// </summary>
        /// <param name="message">信息</param>
        public static void Write(string message)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage(message);
        }

        /// <summary>
        /// 输出命令行信息
        /// </summary>
        /// <param name="message">信息</param>
        /// <param name="args">参数</param>
        public static void Write(string message, params object[] args)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage(message, args);
        }

        /// <summary>
        /// 输出命令行信息
        /// </summary>
        /// <param name="message">信息</param>
        public static void WriteLine(string message)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("\n");
            ed.WriteMessage(message);
        }

        /// <summary>
        /// 输出命令行信息
        /// </summary>
        /// <param name="message">信息</param>
        /// <param name="args">参数</param>
        public static void WriteLine(string message, params object[] args)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("\n");
            ed.WriteMessage(message, args);
        }

        /// <summary>
        /// 获取字符串
        /// </summary>
        /// <param name="message">提示</param>
        /// <returns>字符串</returns>
        public static string GetString(string message)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptResult res = ed.GetString(message);
            if (res.Status == PromptStatus.OK)
            {
                return res.StringResult;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取字符串
        /// </summary>
        /// <param name="message">提示</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>字符串</returns>
        public static string GetString(string message, string defaultValue)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptResult res = ed.GetString(new PromptStringOptions(message) { DefaultValue = defaultValue });
            if (res.Status == PromptStatus.OK)
            {
                return res.StringResult;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取关键字
        /// </summary>
        /// <param name="message">提示</param>
        /// <param name="keys">关键字数组</param>
        /// <param name="defaultIndex">默认选项</param>
        /// <returns>用户选择的关键字</returns>
        public static string GetKeywords(string message, string[] keys, int defaultIndex = 0)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptKeywordOptions opt = new PromptKeywordOptions(message); // mod 20140527
            keys.ToList().ForEach(k => opt.Keywords.Add(k));
            opt.Keywords.Default = keys[defaultIndex];
            opt.AllowNone = true;
            PromptResult res = ed.GetKeywords(opt);
            if (res.Status == PromptStatus.OK)
            {
                return res.StringResult;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取数值
        /// </summary>
        /// <param name="message">提示</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>数值</returns>
        public static double GetValue(string message, double? defaultValue = null)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptDoubleResult res = defaultValue == null 
                ? ed.GetDouble(new PromptDoubleOptions(message) { AllowNone = true })
                : ed.GetDouble(new PromptDoubleOptions(message) { DefaultValue = defaultValue.Value, AllowNone = true });
            if (res.Status == PromptStatus.OK)
            {
                return res.Value;
            }
            else
            {
                return double.NaN;
            }
        }

        /// <summary>
        /// 获取距离
        /// </summary>
        /// <param name="message">提示</param>
        /// <returns>距离</returns>
        public static double GetDistance(string message)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptDoubleResult res = ed.GetDistance(message);
            if (res.Status == PromptStatus.OK)
            {
                return res.Value;
            }
            else
            {
                return double.NaN;
            }
        }

        /// <summary>
        /// 获取角度
        /// </summary>
        /// <param name="message">提示</param>
        /// <returns>角度</returns>
        public static double GetAngle(string message)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptDoubleResult res = ed.GetAngle(message);
            if (res.Status == PromptStatus.OK)
            {
                return res.Value;
            }
            else
            {
                return double.NaN;
            }
        }

        /// <summary>
        /// 获取点
        /// </summary>
        /// <param name="message">提示</param>
        /// <returns>点</returns>
        public static Point3d GetPoint(string message)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptPointOptions opt = new PromptPointOptions(message);
            opt.AllowNone = true;
            PromptPointResult res = ed.GetPoint(opt);
            if (res.Status == PromptStatus.OK)
            {
                return res.Value;
            }
            else
            {
                return Algorithms.NullPoint3d;
            }
        }

        /// <summary>
        /// 可视化获取直线段终点
        /// </summary>
        /// <param name="message">提示</param>
        /// <param name="startPoint">起点</param>
        /// <returns>终点</returns>
        public static Point3d GetLineEndPoint(string message, Point3d startPoint)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            LineJig jig = new LineJig(startPoint, message);
            PromptResult res = ed.Drag(jig);
            if (res.Status == PromptStatus.OK)
            {
                return jig.EndPoint;
            }
            else
            {
                return Algorithms.NullPoint3d;
            }
        }

        /// <summary>
        /// 获取对角点
        /// </summary>
        /// <param name="message">提示</param>
        /// <param name="basePoint">第一点</param>
        /// <returns>对角点</returns>
        public static Point3d GetCorner(string message, Point3d basePoint)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptCornerOptions opt = new PromptCornerOptions(message, basePoint) { AllowNone = true }; // mod 20140527
            PromptPointResult res = ed.GetCorner(opt);
            if (res.Status == PromptStatus.OK)
            {
                return res.Value;
            }
            else
            {
                return Algorithms.NullPoint3d;
            }
        }

        /// <summary>
        /// 获取二维范围
        /// </summary>
        /// <param name="message">提示</param>
        /// <returns>二维范围</returns>
        public static Extents3d? GetExtents(string message)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptPointResult res = ed.GetPoint(message);
            if (res.Status != PromptStatus.OK)
            {
                return null;
            }
            Point3d p1 = res.Value;
            res = ed.GetCorner(message, p1);
            if (res.Status != PromptStatus.OK)
            {
                return null;
            }
            Point3d p2 = res.Value;
            return new Extents3d(new Point3d(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y), 0), new Point3d(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y), 0));
        }

        /// <summary>
        /// 获取实体
        /// </summary>
        /// <param name="message">提示</param>
        /// <returns>实体ID</returns>
        public static ObjectId GetEntity(string message)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptEntityResult res = ed.GetEntity(message);
            if (res.Status == PromptStatus.OK)
            {
                return res.ObjectId;
            }
            else
            {
                return ObjectId.Null;
            }
        }

        /// <summary>
        /// 获取实体
        /// </summary>
        /// <param name="message">提示</param>
        /// <param name="allowedType">允许类型</param>
        /// <param name="exactMatch">是否严格匹配</param>
        /// <returns>实体ID</returns>
        public static ObjectId GetEntity(string message, Type allowedType, bool exactMatch = true) // newly 20130514
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptEntityOptions opt = new PromptEntityOptions(message);
            opt.SetRejectMessage("请选择" + allowedType.Name); // Must use this first
            opt.AddAllowedClass(allowedType, exactMatch);
            PromptEntityResult res = ed.GetEntity(opt);
            if (res.Status == PromptStatus.OK)
            {
                return res.ObjectId;
            }
            else
            {
                return ObjectId.Null;
            }
        }

        /// <summary>
        /// 获取实体和点选位置
        /// </summary>
        /// <param name="message">提示</param>
        /// <returns>实体ID和点选位置</returns>
        public static Tuple<ObjectId, Point3d> GetPick(string message)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptEntityResult res = ed.GetEntity(message);
            if (res.Status == PromptStatus.OK)
            {
                return new Tuple<ObjectId, Point3d>(res.ObjectId, res.PickedPoint);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取多个实体
        /// </summary>
        /// <param name="message">提示</param>
        /// <returns>实体ID数组</returns>
        public static ObjectId[] GetSelection(string message)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptSelectionOptions opt = new PromptSelectionOptions { MessageForAdding = message };
            ed.WriteMessage(message);
            PromptSelectionResult res = ed.GetSelection(opt);
            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds();
            }
            else
            {
                return new ObjectId[0];
            }
        }

        /// <summary>
        /// 按过滤器获取多个实体
        /// </summary>
        /// <param name="message">提示</param>
        /// <param name="filter">过滤器，结构同TypedValue</param>
        /// <returns>实体ID数组</returns>
        public static ObjectId[] GetSelection(string message, TupleList<int, object> filter)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptSelectionOptions opt = new PromptSelectionOptions { MessageForAdding = message };
            ed.WriteMessage(message);
            PromptSelectionResult res = ed.GetSelection(opt, new SelectionFilter(filter.Select(x => new TypedValue(x.Item1, x.Item2)).ToArray()));
            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds();
            }
            else
            {
                return new ObjectId[0];
            }
        }

        /// <summary>
        /// 获取指定类型的多个实体
        /// </summary>
        /// <param name="message">提示</param>
        /// <param name="allowedType">允许实体类型，用逗号分隔。例："*LINE,ARC,CIRCLE"</param>
        /// <returns>实体ID数组</returns>
        public static ObjectId[] GetSelection(string message, string allowedType)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptSelectionOptions opt = new PromptSelectionOptions { MessageForAdding = message };
            ed.WriteMessage(message);
            PromptSelectionResult res = ed.GetSelection(opt, new SelectionFilter(new TypedValue[] { new TypedValue(0, allowedType) }));
            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds();
            }
            else
            {
                return new ObjectId[0];
            }
        }

        /// <summary>
        /// 窗选式空间查询
        /// </summary>
        /// <param name="pt1">角点1</param>
        /// <param name="pt2">角点2</param>
        /// <param name="allowedType">允许类型</param>
        /// <returns>选择集</returns>
        public static ObjectId[] GetWindowSelection(Point3d pt1, Point3d pt2, string allowedType = "*")
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptSelectionResult res = ed.SelectWindow(pt1, pt2, new SelectionFilter(new TypedValue[] { new TypedValue(0, allowedType) }));
            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds();
            }
            else
            {
                return new ObjectId[0];
            }
        }

        /// <summary>
        /// 叉选式空间查询
        /// </summary>
        /// <param name="pt1">角点1</param>
        /// <param name="pt2">角点2</param>
        /// <param name="allowedType">允许类型</param>
        /// <returns>选择集</returns>
        public static ObjectId[] GetCrossingSelection(Point3d pt1, Point3d pt2, string allowedType = "*")
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptSelectionResult res = ed.SelectCrossingWindow(pt1, pt2, new SelectionFilter(new TypedValue[] { new TypedValue(0, allowedType) }));
            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds();
            }
            else
            {
                return new ObjectId[0];
            }
        }

        /// <summary>
        /// 获取命令前拾取集。注意：CommandMethod特性的CommadFlags要添加UserPickSet位域
        /// </summary>
        /// <returns>实体ID数组</returns>
        public static ObjectId[] GetPickSet()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptSelectionResult res = ed.SelectImplied();
            if (res.Status == PromptStatus.OK)
            {
                //ed.WriteMessage("已选择 {0} 个实体", res.Value.Count);
                return res.Value.GetObjectIds();
            }
            else
            {
                return new ObjectId[0];
            }
        }

        /// <summary>
        /// 设置命令前拾取集。
        /// </summary>
        /// <param name="ids">实体ID数组</param>
        public static void SetPickSet(ObjectId[] ids)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.SetImpliedSelection(ids);
        }

        /// <summary>
        /// 获取用本CodePack中的绘图函数最后添加的实体
        /// </summary>
        /// <returns>实体ID</returns>
        public static ObjectId GetNewestEntity()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptSelectionResult res = ed.SelectLast();
            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds()[0];
            }
            else
            {
                return ObjectId.Null;
            }
        }

        /// <summary>
        /// 获取用本CodePack中的绘图函数最后一批添加的实体
        /// </summary>
        /// <returns>实体ID数组</returns>
        public static ObjectId[] GetNewestEntities()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptSelectionResult res = ed.SelectLast();
            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds();
            }
            else
            {
                return new ObjectId[0];
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern System.IntPtr SetFocus(System.IntPtr hwnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern System.IntPtr SetParent(System.IntPtr child, System.IntPtr parent);

        /// <summary>
        /// 使活动文档窗口获取焦点
        /// </summary>
        public static void SetActiveDocFocus()
        {
            SetFocus(Application.DocumentManager.MdiActiveDocument.Window.Handle);
        }

        /// <summary>
        /// 设置当前图层
        /// </summary>
        /// <param name="layer">图层名</param>
        public static void SetCurrentLayer(string layer)
        {
            HostApplicationServices.WorkingDatabase.Clayer = DbHelper.GetLayerId(layer);
        }

        /// <summary>
        /// 发送命令行
        /// </summary>
        /// <param name="command">命令</param>
        public static void Command(string command)
        {
            Application.DocumentManager.MdiActiveDocument.SendStringToExecute(command, false, false, false);
        }

        /// <summary>
        /// 开启新命令 newly 20140731
        /// </summary>
        /// <param name="command">命令</param>
        public static void StartCommand(string command)
        {
            string esc = string.Empty;
            string cmds = Application.GetSystemVariable("CMDNAMES").ToString();
            if (cmds.Length > 0)
            {
                int cmdNum = cmds.Split('\'').Length;
                for (int i = 0; i < cmdNum; i++)
                {
                    esc += '\x03';
                }
            }
            Application.DocumentManager.MdiActiveDocument.SendStringToExecute(esc + command, true, false, true);
        }

#if R18
        /// <summary>
        /// COM接口发送命令行
        /// </summary>
        /// <param name="command"></param>
        //public static void Command2(string command)
        //{
        //    //var doc = Application.DocumentManager.MdiActiveDocument.AcadDocument;
        //    //System.Reflection.MethodInfo mi = doc.GetType().GetMethod("SendCommand", new Type[] { typeof(string) });
        //    //mi.Invoke(doc, new object[] { command });
        //    var doc = Application.DocumentManager.MdiActiveDocument.AcadDocument as Autodesk.AutoCAD.Interop.AcadDocument;
        //    doc.SendCommand(command);
        //}
#endif

        /// <summary>
        /// 显示任务对话框
        /// </summary>
        /// <param name="mainInstruction">主说明</param>
        /// <param name="yesChoice">是选项</param>
        /// <param name="noChoice">否选项</param>
        /// <param name="title">对话框标题</param>
        /// <param name="content">内容</param>
        /// <param name="footer">脚注</param>
        /// <param name="expanded">详细说明</param>
        /// <returns>用户选择</returns>
        public static bool TaskDialog(string mainInstruction, string yesChoice, string noChoice, string title = "AutoCAD", string content = "", string footer = "", string expanded = "")
        {
            //WPF任务对话框
            TaskDialog td = new TaskDialog();
            td.WindowTitle = title;
            td.MainInstruction = mainInstruction;
            td.ContentText = content;
            td.MainIcon = TaskDialogIcon.Information;
            td.FooterIcon = TaskDialogIcon.Warning;
            td.FooterText = footer;
            td.CollapsedControlText = "详细信息";
            td.ExpandedControlText = "详细信息";
            td.ExpandedByDefault = false;
            td.ExpandedText = expanded;
            td.AllowDialogCancellation = false;
            td.UseCommandLinks = true;
            td.Buttons.Add(new TaskDialogButton(1, yesChoice));
            td.Buttons.Add(new TaskDialogButton(2, noChoice));
            td.DefaultButton = 1;
            int[] btnId = null;
            td.Callback = delegate(ActiveTaskDialog atd, TaskDialogCallbackArgs e, object sender)
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
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 高亮实体
        /// </summary>
        /// <param name="entIds">实体集</param>
        public static void HighlightObjects(IEnumerable<ObjectId> entIds)
        {
            entIds.QForEach<Entity>(x => x.Highlight());
        }

        /// <summary>
        /// 取消高亮实体
        /// </summary>
        /// <param name="entIds">实体集</param>
        public static void UnhighlightObjects(IEnumerable<ObjectId> entIds)
        {
            entIds.QForEach<Entity>(x => x.Unhighlight());
        }

        /// <summary>
        /// 缩放到实体
        /// </summary>
        /// <param name="entIds">实体集</param>
        public static void ZoomObjects(IEnumerable<ObjectId> entIds)
        {
            Extents3d extent = entIds.GetExtents();
            Interaction.ZoomView(extent);
        }

        /// <summary>
        /// 缩放到指定范围
        /// </summary>
        /// <param name="extent">范围</param>
        public static void ZoomView(Extents3d extent)
        {
            Zoom(extent.MinPoint, extent.MaxPoint, new Point3d(), 1);
        }

        /// <summary>
        /// 范围缩放
        /// </summary>
        public static void ZoomExtents()
        {
            if (HostApplicationServices.WorkingDatabase.TileMode) // 当前为模型空间
            {
                Zoom(HostApplicationServices.WorkingDatabase.Extmin, HostApplicationServices.WorkingDatabase.Extmax, new Point3d(), 1);
            }
            else // 当前为图纸空间
            {
                //BlockTableRecord btr = HostApplicationServices.WorkingDatabase.CurrentSpaceId.QOpenForRead() as BlockTableRecord;
                //Extents3d extents = btr.Cast<ObjectId>().GetExtents();
                Zoom(new Point3d(), new Point3d(), new Point3d(), 1);
            }
        }

        //
        // Zoom函数来源于AutoCAD .NET Developer's Guide
        //
        internal static void Zoom(Point3d pMin, Point3d pMax, Point3d pCenter, double dFactor)
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            int nCurVport = System.Convert.ToInt32(Application.GetSystemVariable("CVPORT"));

            // Get the extents of the current space no points
            // or only a center point is provided
            // Check to see if Model space is current
            if (acCurDb.TileMode == true)
            {
                if (pMin.Equals(new Point3d()) == true &&
                         pMax.Equals(new Point3d()) == true)
                {
                    pMin = acCurDb.Extmin;
                    pMax = acCurDb.Extmax;
                }
            }
            else
            {
                // Check to see if Paper space is current
                if (nCurVport == 1)
                {
                    // Get the extents of Paper space
                    if (pMin.Equals(new Point3d()) == true &&
                        pMax.Equals(new Point3d()) == true)
                    {
                        pMin = acCurDb.Pextmin;
                        pMax = acCurDb.Pextmax;
                    }
                }
                else
                {
                    // Get the extents of Model space
                    if (pMin.Equals(new Point3d()) == true &&
                        pMax.Equals(new Point3d()) == true)
                    {
                        pMin = acCurDb.Extmin;
                        pMax = acCurDb.Extmax;
                    }
                }
            }

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Get the current view
                using (ViewTableRecord acView = acDoc.Editor.GetCurrentView())
                {
                    Extents3d eExtents;

                    // Translate WCS coordinates to DCS
                    Matrix3d matWCS2DCS;
                    matWCS2DCS = Matrix3d.PlaneToWorld(acView.ViewDirection);
                    matWCS2DCS = Matrix3d.Displacement(acView.Target - Point3d.Origin) * matWCS2DCS;
                    matWCS2DCS = Matrix3d.Rotation(-acView.ViewTwist,
                                                   acView.ViewDirection,
                                                   acView.Target) * matWCS2DCS;

                    // If a center point is specified, define the min and max
                    // point of the extents
                    // for Center and Scale modes
                    if (pCenter.DistanceTo(Point3d.Origin) != 0)
                    {
                        pMin = new Point3d(pCenter.X - (acView.Width / 2),
                                           pCenter.Y - (acView.Height / 2), 0);

                        pMax = new Point3d((acView.Width / 2) + pCenter.X,
                                           (acView.Height / 2) + pCenter.Y, 0);
                    }

                    // Create an extents object using a line
                    using (Line acLine = new Line(pMin, pMax))
                    {
                        eExtents = new Extents3d(acLine.Bounds.Value.MinPoint,
                                                 acLine.Bounds.Value.MaxPoint);
                    }

                    // Calculate the ratio between the width and height of the current view
                    double dViewRatio;
                    dViewRatio = (acView.Width / acView.Height);

                    // Tranform the extents of the view
                    matWCS2DCS = matWCS2DCS.Inverse();
                    eExtents.TransformBy(matWCS2DCS);

                    double dWidth;
                    double dHeight;
                    Point2d pNewCentPt;

                    // Check to see if a center point was provided (Center and Scale modes)
                    if (pCenter.DistanceTo(Point3d.Origin) != 0)
                    {
                        dWidth = acView.Width;
                        dHeight = acView.Height;

                        if (dFactor == 0)
                        {
                            pCenter = pCenter.TransformBy(matWCS2DCS);
                        }

                        pNewCentPt = new Point2d(pCenter.X, pCenter.Y);
                    }
                    else // Working in Window, Extents and Limits mode
                    {
                        // Calculate the new width and height of the current view
                        dWidth = eExtents.MaxPoint.X - eExtents.MinPoint.X;
                        dHeight = eExtents.MaxPoint.Y - eExtents.MinPoint.Y;

                        // Get the center of the view
                        pNewCentPt = new Point2d(((eExtents.MaxPoint.X + eExtents.MinPoint.X) * 0.5),
                                                 ((eExtents.MaxPoint.Y + eExtents.MinPoint.Y) * 0.5));
                    }

                    // Check to see if the new width fits in current window
                    if (dWidth > (dHeight * dViewRatio)) dHeight = dWidth / dViewRatio;

                    // Resize and scale the view
                    if (dFactor != 0)
                    {
                        acView.Height = dHeight * dFactor;
                        acView.Width = dWidth * dFactor;
                    }

                    // Set the center of the view
                    acView.CenterPoint = pNewCentPt;

                    // Set the current view
                    acDoc.Editor.SetCurrentView(acView);
                }

                // Commit the changes
                acTrans.Commit();
            }
        }

        /// <summary>
        /// 可视化插入实体
        /// </summary>
        /// <param name="ent">实体</param>
        /// <returns>实体ID</returns>
        public static ObjectId InsertEntity(Entity ent)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PositionJig jig = new PositionJig(ent);
            PromptResult res = ed.Drag(jig);
            if (res.Status == PromptStatus.OK)
            {
                return jig.Ent.AddToCurrentSpace();
            }
            else
            {
                return ObjectId.Null;
            }
        }

        /// <summary>
        /// 可视化插入可缩放实体
        /// </summary>
        /// <param name="ent">实体</param>
        /// <param name="basePoint">基点</param>
        /// <param name="message">提示信息</param>
        /// <returns>实体ID</returns>
        public static ObjectId InsertScalingEntity(Entity ent, Point3d basePoint, string message = "\n指定对角点")
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            //ScalingJig jig = new ScalingJig(ent, basePoint, message);
            ScaleJig jig = new ScaleJig(ent, basePoint, message);
            PromptResult res = ed.Drag(jig);
            if (res.Status == PromptStatus.OK)
            {
                return jig.Ent.AddToCurrentSpace();
            }
            else
            {
                return ObjectId.Null;
            }
        }

        /// <summary>
        /// 可视化插入可旋转实体
        /// </summary>
        /// <param name="ent">实体</param>
        /// <param name="center">中心</param>
        /// <param name="message">提示信息</param>
        /// <returns>实体ID</returns>
        public static ObjectId InsertRotationEntity(Entity ent, Point3d center, string message = "\n指定方向")
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            RotationJig jig = new RotationJig(ent, center, message);
            PromptResult res = ed.Drag(jig);
            if (res.Status == PromptStatus.OK)
            {
                return jig.Ent.AddToCurrentSpace();
            }
            else
            {
                return ObjectId.Null;
            }
        }

        /// <summary>
        /// 操作系统保存文件对话框
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="fileName">文件名</param>
        /// <param name="filter">类型过滤器</param>
        /// <returns>文件名</returns>
        public static string SaveFileDialogBySystem(string title, string fileName, string filter)
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog { Title = title, FileName = fileName, Filter = filter };
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return sfd.FileName;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 操作系统打开文件对话框
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="fileName">文件名</param>
        /// <param name="filter">类型过滤器</param>
        /// <returns>文件名</returns>
        public static string OpenFileDialogBySystem(string title, string fileName, string filter)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog { Title = title, FileName = fileName, Filter = filter };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return ofd.FileName;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 文件夹对话框
        /// </summary>
        /// <param name="instruction">提示</param>
        /// <returns>文件夹名</returns>
        public static string FolderDialog(string instruction)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog { Description = instruction, RootFolder = Environment.SpecialFolder.Desktop, ShowNewFolderButton = true };
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return fbd.SelectedPath;
            }
            else
            {
                return string.Empty;
            }
        }

        // TODO: file dialog by AutoCAD
        //public static void SaveFileDialogByAutoCAD()
        //{
        //}

        /// <summary>
        /// AutoCAD颜色对话框
        /// </summary>
        /// <returns>颜色</returns>
        public static Autodesk.AutoCAD.Colors.Color ColorDialog()
        {
            Autodesk.AutoCAD.Windows.ColorDialog cd = new Autodesk.AutoCAD.Windows.ColorDialog();
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return cd.Color;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取用户交互创建的Polyline
        /// </summary>
        /// <param name="message">提示</param>
        /// <returns>多段线</returns>
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
        /// 多个实体，输入编号逐个查看
        /// </summary>
        /// <param name="ids">实体ID数组</param>
        /// <param name="action">动作</param>
        public static void ZoomHighlightView(List<ObjectId> ids, Action<int> action = null) // newly 20130815
        {
            if (ids.Count > 0)
            {
                List<ObjectId> highlightIds = new List<ObjectId>();
                while (true)
                {
                    string input = Interaction.GetString("\n输入编号查看，按回车退出");
                    if (input == null)
                    {
                        break;
                    }
                    var index = Convert.ToInt32(input);
                    if (index <= 0 || index > ids.Count)
                    {
                        Interaction.WriteLine("编号不在范围内。");
                        continue;
                    }

                    if (action != null)
                    {
                        action(index);
                    }
                    highlightIds.Clear();
                    highlightIds.Add(ids[index - 1]);
                    Interaction.ZoomObjects(highlightIds);
                    Interaction.HighlightObjects(highlightIds);
                }
            }
        }
    }

    internal class LineJig : EntityJig
    {
        private Point3d _startPoint;
        private Point3d _endPoint;
        private string _message;

        public Point3d EndPoint { get { return _endPoint; } }

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
            else if (endPoint != _endPoint)
            {
                _endPoint = endPoint;
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
                line.EndPoint = _endPoint;
            }
            catch
            {
            }
            return true;
        }
    }

    internal class PositionJig : EntityJig
    {
        private Entity _ent;
        private Point3d _pos = Point3d.Origin;
        private Vector3d _move;

        public Entity Ent { get { return _ent; } }

        public PositionJig(Entity ent)
            : base(ent)
        {
            _ent = ent;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions jppo = new JigPromptPointOptions("\n指定位置");
            jppo.Keywords.Add(""); // mod 20140527
            jppo.Cursor = CursorType.EntitySelect;
            jppo.UseBasePoint = false;
            jppo.UserInputControls = UserInputControls.NullResponseAccepted;
            Point3d pos = prompts.AcquirePoint(jppo).Value;
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
                _ent.TransformBy(Matrix3d.Displacement(_move));
            }
            catch
            {
            }
            return true;
        }
    }

    internal class ScaleJig : EntityJig
    {
        private Entity _ent;
        private Point3d _pos = Point3d.Origin;
        private Vector3d _move;
        private Point3d _basePoint;
        private string _message;
        private Extents3d _extents;
        //private double _scale;
        private double _mag;

        public Entity Ent { get { return _ent; } }

        public ScaleJig(Entity ent, Point3d basePoint, string message)
            : base(ent)
        {
            _ent = ent;
            _basePoint = basePoint;
            _message = message;
            _extents = _ent.GeometricExtents;
            //_scale = 1;
            _mag = 1;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions jppo = new JigPromptPointOptions(_message);
            jppo.Keywords.Add(""); // mod 20140527
            jppo.Cursor = CursorType.EntitySelect;
            jppo.UseBasePoint = false;
            jppo.UserInputControls = UserInputControls.NullResponseAccepted;
            Point3d corner = prompts.AcquirePoint(jppo).Value;
            Point3d pos = Point3d.Origin + 0.5 * (_basePoint.GetAsVector() + corner.GetAsVector());
            var extents = _ent.GeometricExtents;
            double scale = Math.Min(
                Math.Abs(corner.X - _basePoint.X) / (extents.MaxPoint.X - extents.MinPoint.X),
                Math.Abs(corner.Y - _basePoint.Y) / (extents.MaxPoint.Y - extents.MinPoint.Y));
            if (scale < Consts.Epsilon) // 在一开始scale势必很小。过小的scale会导致矩阵运算出现非等比缩放变换，从而被CAD拒绝，导致异常。
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
                _ent.TransformBy(Matrix3d.Displacement(_move));
                _ent.TransformBy(Matrix3d.Scaling(_mag, _pos)); // 必须先平移，再缩放。
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

        public Entity Ent { get { return Entity; } }

        public RotationJig(Entity ent, Point3d center, string message)
            : base(ent)
        {
            _center = center;
            _message = message;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions jppo = new JigPromptPointOptions(_message);
            jppo.Keywords.Add(""); // mod 20140527
            jppo.Cursor = CursorType.EntitySelect;
            jppo.BasePoint = _center;
            jppo.UseBasePoint = true;
            jppo.UserInputControls = UserInputControls.NullResponseAccepted;
            Point3d end = prompts.AcquirePoint(jppo).Value;
            if (end.IsNull())
            {
                return SamplerStatus.Cancel;
            }
            else if (end != _end)
            {
                _end = end;
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
                Vector3d dir = _end - _center;
                double angle = dir.GetAngleTo(Vector3d.YAxis);
                if (dir.X > 0)
                {
                    angle = Math.PI * 2 - angle;
                }
                Entity.TransformBy(Matrix3d.Rotation(angle - _angle, Vector3d.ZAxis, _center));
                _angle = angle;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
