using Autodesk.AutoCAD.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AcadApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using WinFormsPropertyGrid = System.Windows.Forms.PropertyGrid;
using WpfWindow = System.Windows.Window;

namespace Dreambuild.AutoCAD
{
    /// <summary>
    /// The GUI component gallery.
    /// </summary>
    public static class Gui
    {
        /// <summary>
        /// Shows a GetOption window.
        /// </summary>
        /// <param name="tip">The tip.</param>
        /// <param name="options">The options.</param>
        /// <returns>The choice.</returns>
        public static int GetOption(string tip, params string[] options)
        {
            var window = new WpfWindow
            {
                Width = 300,
                SizeToContent = SizeToContent.Height,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.ToolWindow,
                Title = "Options"
            };

            var stackPanel = new StackPanel
            {
                Margin = new Thickness(5)
            };

            var textBlock = new TextBlock
            {
                Text = (tip == string.Empty ? "Please choose one..." : tip),
                TextWrapping = TextWrapping.Wrap
            };

            int result = -1;
            var buttons = options
                .Select((option, index) => new Button
                {
                    Content = option,
                    Tag = index
                })
                .ToList();

            buttons.ForEach(button => button.Click += (s, args) =>
            {
                result = (int)button.Tag;
                window.DialogResult = true;
            });

            stackPanel.Children.Add(textBlock);
            buttons.ForEach(button => stackPanel.Children.Add(button));
            window.Content = stackPanel;

            AcadApplication.ShowModalWindow(window);
            return result;
        }

        /// <summary>
        /// Shows a GetChoice window.
        /// </summary>
        /// <param name="tip">The tip.</param>
        /// <param name="choices">The choices.</param>
        /// <returns>The choice.</returns>
        public static string GetChoice(string tip, params string[] choices)
        {
            var window = new WpfWindow
            {
                Width = 300,
                SizeToContent = SizeToContent.Height,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.ToolWindow,
                Title = "Choices"
            };

            var stackPanel = new StackPanel
            {
                Margin = new Thickness(10)
            };

            var textBlock = new TextBlock
            {
                Text = (tip == string.Empty ? "Please choose one..." : tip),
                TextWrapping = TextWrapping.Wrap
            };

            var listBox = new ListBox
            {
                Height = 200
            };

            choices.ForEach(choice => listBox.Items.Add(new ListBoxItem
            {
                Content = choice
            }));

            var okButton = new Button
            {
                Content = "OK",
                Margin = new Thickness(5)
            };

            okButton.Click += (s, args) => window.DialogResult = true;
            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(listBox);
            stackPanel.Children.Add(okButton);
            window.Content = stackPanel;

            AcadApplication.ShowModalWindow(window);
            return listBox.SelectedItem == null
                ? string.Empty
                : (listBox.SelectedItem as ListBoxItem).Content.ToString();
        }

        /// <summary>
        /// Shows a GetChoices window.
        /// </summary>
        /// <param name="tip">The tip.</param>
        /// <param name="choices">The choices.</param>
        /// <returns>The final choices.</returns>
        public static string[] GetChoices(string tip, params string[] choices)
        {
            var window = new WpfWindow
            {
                Width = 300,
                SizeToContent = SizeToContent.Height,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.ToolWindow,
                Title = "Choices"
            };

            var stackPanel = new StackPanel
            {
                Margin = new Thickness(10)
            };

            var textBlock = new TextBlock
            {
                Text = (tip == string.Empty ? "Please check items..." : tip),
                TextWrapping = TextWrapping.Wrap
            };

            var listBox = new ListBox
            {
                Height = 200
            };

            choices.ForEach(choice => listBox.Items.Add(new CheckBox
            {
                Content = choice
            }));

            var okButton = new Button
            {
                Content = "OK",
                Margin = new Thickness(5)
            };

            okButton.Click += (s, args) => window.DialogResult = true;
            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(listBox);
            stackPanel.Children.Add(okButton);
            window.Content = stackPanel;

            AcadApplication.ShowModalWindow(window);
            return listBox.Items
                .Cast<CheckBox>()
                .Where(checkBox => checkBox.IsChecked == true)
                .Select(checkBox => checkBox.Content.ToString())
                .ToArray();
        }

        /// <summary>
        /// Shows a text report window.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="content">The content.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="modal">Modal or not.</param>
        public static void TextReport(string title, string content, double width, double height, bool modal = false)
        {
            var tr = new TextReport
            {
                Width = width,
                Height = height,
                Title = title
            };
            tr.ContentArea.Text = content;
            if (modal)
            {
                AcadApplication.ShowModalWindow(tr);
            }
            else
            {
                AcadApplication.ShowModelessWindow(tr);
            }
        }

        /// <summary>
        /// Shows a multi-inputs window.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="entries">The input entries.</param>
        public static void MultiInputs(string title, Dictionary<string, string> entries)
        {
            var mi = new MultiInputs();
            mi.Ready(entries, title);
            AcadApplication.ShowModalWindow(mi);
        }

        /// <summary>
        /// Shows an input box.
        /// </summary>
        /// <param name="tip">The tip.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The input.</returns>
        public static string InputBox(string tip, string defaultValue = "")
        {
            var input = new InputBox(tip, defaultValue);
            if (AcadApplication.ShowModalWindow(input) == true)
            {
                return input.Value;
            }
            return string.Empty;
        }

        /// <summary>
        /// Shows a busy indicator.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="worker"></param>
        public static void BusyIndicator(string title, BackgroundWorker worker)
        {
            // todo: BusyIndicator
            throw new NotImplementedException();
        }

        /// <summary>
        /// Shows a progress indicator.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="worker">The background worker.</param>
        public static void ProgressIndicator(string title, BackgroundWorker worker)
        {
            var tpw = new TaskProgressWindow
            {
                Title = title + " (0%)"
            };
            tpw.CancelButton.Click += (s, args) =>
            {
                tpw.CancelButton.IsEnabled = false;
                worker.CancelAsync();
            };
            worker.ProgressChanged += (s, args) =>
            {
                tpw.ProgressBar.Value = args.ProgressPercentage;
                tpw.Title = $"{title} ({args.ProgressPercentage}%)";
            };
            worker.RunWorkerCompleted += (s, args) => tpw.Close();
            AcadApplication.ShowModalWindow(tpw);
        }

        private static PaletteSet propertyPalette = new PaletteSet("Properties");
        private static WinFormsPropertyGrid propertyGrid = new WinFormsPropertyGrid();

        /// <summary>
        /// Shows a property palette.
        /// </summary>
        /// <param name="obj">The property object.</param>
        public static void PropertyPalette(object obj) // newly 20140529
        {
            if (!propertyPalette.Visible)
            {
                if (propertyPalette.Count == 0)
                {
                    propertyPalette.Add("Properties", propertyGrid);
                    propertyPalette.Dock = DockSides.Left;
                }
                propertyPalette.Visible = true;
            }
            propertyGrid.SelectedObject = obj;
        }
    }
}
