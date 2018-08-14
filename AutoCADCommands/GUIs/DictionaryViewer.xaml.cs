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

        public DictionaryViewer(
            Func<IEnumerable<string>> getDictNames,
            Func<string, IEnumerable<string>> getEntryNames,
            Func<string, string, string> getValue,
            Action<string, string, string> setValue)
        {
            InitializeComponent();

            _getDictNames = getDictNames;
            _getEntryNames = getEntryNames;
            _getValue = getValue;
            _setValue = setValue;
        }

        public void DictionaryViewer_Loaded(object sender, RoutedEventArgs e)
        {
            _getDictNames().ForEach(name => this.DictionaryList.Items.Add(name));
            this.DictionaryList.SelectedIndex = 0;
        }

        private void DictionaryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.EntryList.Items.Clear();
            string dict = this.DictionaryList.SelectedItem.ToString();
            _getEntryNames(dict)
                .OrderBy(name => name)
                .Select(name => new
                {
                    Key = name,
                    Value = _getValue(dict, name)
                })
                .ForEach(entry => this.EntryList.Items.Add(entry));
        }

        private void EntryList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.DictionaryList.SelectedIndex == -1 || this.EntryList.SelectedIndex == -1)
            {
                return;
            }

            string dict = this.DictionaryList.SelectedItem.ToString();
            string key = _getEntryNames(dict).OrderBy(name => name).ToList()[this.EntryList.SelectedIndex];
            string oldValue = _getValue(dict, key);

            var inputBox = new InputBox(oldValue)
            {
                Owner = this
            };

            if (inputBox.ShowDialog() == true)
            {
                _setValue(dict, key, inputBox.Value);
            }

            // Update ListView
            this.DictionaryList_SelectionChanged(null, null);
        }
    }
}
