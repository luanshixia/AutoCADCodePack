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
    /// InputBox.xaml code behind.
    /// </summary>
    public partial class InputBox : Window
    {
        public string Value { get { return txtValue.Text; } }

        public InputBox()
        {
            InitializeComponent();
        }

        public InputBox(string defaultValue)
        {
            InitializeComponent();

            txtValue.Text = defaultValue;
        }

        public InputBox(string tip, string defaultValue)
        {
            InitializeComponent();

            this.Title = tip;
            txtValue.Text = defaultValue;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
