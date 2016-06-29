using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace AutoCADCommands
{
    /// <summary>
    /// 图形用户交互
    /// </summary>
    public static class Gui
    {
        /// <summary>
        /// 显示一个命令选项面板
        /// </summary>
        /// <param name="tip"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static int GetOption(string tip, params string[] options)
        {
            Window window = new Window { Width = 300, SizeToContent = SizeToContent.Height, WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen, ShowInTaskbar = false, WindowStyle = WindowStyle.ToolWindow, Title = "请选择" };
            StackPanel sp = new StackPanel { Margin = new Thickness(5) };
            TextBlock tb = new TextBlock { Text = (tip == string.Empty ? "请选择一个选项。" : tip), TextWrapping = TextWrapping.Wrap };
            int result = -1;
            var btns = options.Select((x, i) => new Button { Content = x, Tag = i }).ToList();
            btns.ForEach(x => x.Click += (s, args) => { result = (int)x.Tag; window.DialogResult = true; });
            sp.Children.Add(tb);
            btns.ForEach(x => sp.Children.Add(x));
            window.Content = sp;
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModalWindow(window);
            return result;
        }

        /// <summary>
        /// 显示一个列表选择面板
        /// </summary>
        /// <param name="tip"></param>
        /// <param name="choices"></param>
        /// <returns></returns>
        public static string GetChoice(string tip, params string[] choices)
        {
            Window window = new Window { Width = 300, SizeToContent = SizeToContent.Height, WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen, ShowInTaskbar = false, WindowStyle = WindowStyle.ToolWindow, Title = "请选择" };
            StackPanel sp = new StackPanel { Margin = new Thickness(10) };
            TextBlock tb = new TextBlock { Text = (tip == string.Empty ? "请选择一个项目。" : tip), TextWrapping = TextWrapping.Wrap };
            ListBox list = new ListBox { Height = 200 };
            choices.ToList().ForEach(x => list.Items.Add(new ListBoxItem { Content = x }));
            Button btnOk = new Button { Content = "确定", Margin = new Thickness(5) };
            btnOk.Click += (s, args) => window.DialogResult = true;
            sp.Children.Add(tb);
            sp.Children.Add(list);
            sp.Children.Add(btnOk);
            window.Content = sp;
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModalWindow(window);
            return list.SelectedItem == null ? string.Empty : (list.SelectedItem as ListBoxItem).Content.ToString();
        }

        /// <summary>
        /// 显示一个可多选的列表选择面板
        /// </summary>
        /// <param name="tip"></param>
        /// <param name="choices"></param>
        /// <returns></returns>
        public static string[] GetChoices(string tip, params string[] choices)
        {
            Window window = new Window { Width = 300, SizeToContent = SizeToContent.Height, WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen, ShowInTaskbar = false, WindowStyle = WindowStyle.ToolWindow, Title = "请选择" };
            StackPanel sp = new StackPanel { Margin = new Thickness(10) };
            TextBlock tb = new TextBlock { Text = (tip == string.Empty ? "请选择一个选项。" : tip), TextWrapping = TextWrapping.Wrap };
            ListBox list = new ListBox { Height = 200 };
            choices.ToList().ForEach(x => list.Items.Add(new CheckBox { Content = x }));
            Button btnOk = new Button { Content = "确定", Margin = new Thickness(5) };
            btnOk.Click += (s, args) => window.DialogResult = true;
            sp.Children.Add(tb);
            sp.Children.Add(list);
            sp.Children.Add(btnOk);
            window.Content = sp;
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModalWindow(window);
            return list.Items.Cast<CheckBox>().Where(x => x.IsChecked == true).Select(x => x.Content.ToString()).ToArray();
        }

        /// <summary>
        /// 显示一个文本报告窗口
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="modal"></param>
        public static void TextReport(string title, string content, double width, double height, bool modal = false)
        {
            TextReport tr = new TextReport();
            tr.Width = width;
            tr.Height = height;
            tr.Title = title;
            tr.txtContent.Text = content;
            if (modal)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowModalWindow(tr);
            }
            else
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowModelessWindow(tr);
            }
        }

        /// <summary>
        /// 显示一个多值输入窗口
        /// </summary>
        /// <param name="title"></param>
        /// <param name="entries"></param>
        public static void MultiInputs(string title, Dictionary<string, string> entries)
        {
            MultiInputs mi = new MultiInputs();
            mi.Ready(entries, title);
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModalWindow(mi);
        }

        /// <summary>
        /// 显示一个输入框
        /// </summary>
        /// <param name="tip"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string InputBox(string tip, string defaultValue = "")
        {
            InputBox input = new InputBox(tip, defaultValue);
            if (Autodesk.AutoCAD.ApplicationServices.Application.ShowModalWindow(input) == true)
            {
                return input.Value;
            }
            return string.Empty;
        }

        /// <summary>
        /// 繁忙提示框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="worker"></param>
        public static void BusyIndicator(string title, BackgroundWorker worker)
        {
            // todo: BusyIndicator
        }

        /// <summary>
        /// 进度提示框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="worker"></param>
        public static void ProgressIndicator(string title, BackgroundWorker worker)
        {
            TaskProgressWindow tpw = new TaskProgressWindow { Title = title + " (0%)" };
            tpw.CancelButton.Click += (s, args) =>
            {
                tpw.CancelButton.IsEnabled = false;
                worker.CancelAsync();
            };
            worker.ProgressChanged += (s, args) =>
            {
                tpw.ProgressBar.Value = args.ProgressPercentage;
                tpw.Title = string.Format("{0} ({1}%)", title, args.ProgressPercentage);
            };
            worker.RunWorkerCompleted += (s, args) => tpw.Close();
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModalWindow(tpw);
        }

        private static Autodesk.AutoCAD.Windows.PaletteSet propertyPalette = new Autodesk.AutoCAD.Windows.PaletteSet("属性");
        private static System.Windows.Forms.PropertyGrid propertyGrid = new System.Windows.Forms.PropertyGrid();

        public static void PropertyPalette(object obj) // newly 20140529
        {
            if (!propertyPalette.Visible)
            {
                if (propertyPalette.Count == 0)
                {
                    propertyPalette.Add("属性", propertyGrid);
                    propertyPalette.Dock = Autodesk.AutoCAD.Windows.DockSides.Left;
                }
                propertyPalette.Visible = true;
            }
            propertyGrid.SelectedObject = obj;
        }
    }
}
