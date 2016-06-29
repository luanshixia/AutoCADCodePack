using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AutoCADCommands
{
    /// <summary>
    /// MultiInputs.xaml 的交互逻辑
    /// </summary>
    public partial class MultiInputs : Window
    {
        public Dictionary<string, string> Values { get; private set; }

        public MultiInputs()
        {
            InitializeComponent();
        }

        public void Ready(Dictionary<string, string> entries, string title = "信息采集器")
        {
            Values = entries;
            foreach (var entry in entries)
            {
                DockPanel dp = new DockPanel();
                TextBlock lable = new TextBlock { Text = entry.Key, ToolTip = entry.Key, Width = 150, VerticalAlignment = System.Windows.VerticalAlignment.Center };
                TextBox text = new TextBox { Text = entry.Value };
                DockPanel.SetDock(lable, Dock.Left);
                dp.Children.Add(lable);
                dp.Children.Add(text);
                dp.Tag = entry;
                stack1.Children.Add(dp);
            }
            this.Title = title;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            foreach (DockPanel dp in stack1.Children)
            {
                var entry = (KeyValuePair<string, string>)dp.Tag;
                var text = dp.Children[1] as TextBox;
                Values[entry.Key] = text.Text;
            }
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
