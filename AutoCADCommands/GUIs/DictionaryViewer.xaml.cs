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

namespace Dreambuild.AutoCAD
{
    /// <summary>
    /// DictionaryViewer.xaml code behind.
    /// </summary>
    public partial class DictionaryViewer : Window
    {
        private readonly Func<IEnumerable<string>> _getDictNames;
        private readonly Func<string, IEnumerable<string>> _getEntryNames;
        private readonly Func<string, string, string> _getValue;
        private readonly Action<string, string, string> _setValue;

        public DictionaryViewer()
        {
            InitializeComponent();
        }

        public DictionaryViewer(Func<IEnumerable<string>> getDictNames, Func<string, IEnumerable<string>> getEntryNames, Func<string, string, string> getValue, Action<string, string, string> setValue)
        {
            InitializeComponent();

            _getDictNames = getDictNames;
            _getEntryNames = getEntryNames;
            _getValue = getValue;
            _setValue = setValue;
        }

        public void DictionaryViewer_Loaded(object sender, RoutedEventArgs e)
        {
            _getDictNames().ToList().ForEach(x => lstDicts.Items.Add(x));
            lstDicts.SelectedIndex = 0;
        }

        private void lstDicts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lstEntries.Items.Clear();
            string dict = lstDicts.SelectedItem.ToString();
            _getEntryNames(dict).OrderBy(x => x).Select(x => new { Key = x, Value = _getValue(dict, x) }).ToList().ForEach(x => lstEntries.Items.Add(x));
        }

        private void lstEntries_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstDicts.SelectedIndex == -1 || lstEntries.SelectedIndex == -1)
            {
                return;
            }
            string dict = lstDicts.SelectedItem.ToString();
            string key = _getEntryNames(dict).OrderBy(x => x).ToList()[lstEntries.SelectedIndex];
            string oldValue = _getValue(dict, key);

            var ib = new InputBox(oldValue)
            {
                Owner = this
            }; // mod 20130201
            if (ib.ShowDialog() == true)
            {
                _setValue(dict, key, ib.Value);
            }

            // Update ListView
            lstDicts_SelectionChanged(null, null);
        }
    }
}
