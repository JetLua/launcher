using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;




namespace launcher {
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : Window {

        private const string FILE_NAME = "data.json";

        private ListBox Categories;
        private ListBox Items;

        public Main() {
            InitializeComponent();

            //获取scrollview
            Categories = (ListBox)FindName("category");
            Items = (ListBox)FindName("list");

            ReadData();
        }


        private Dictionary<string, string[]> Parse(string json) {
            Dictionary<string, string[]> categories = new Dictionary<string, string[]>();
            try {
                categories = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(json);
            } catch (Exception err) {
                System.Windows.Forms.MessageBox.Show(err.ToString());
            }
            return categories;            
        }

        private async void ReadData() {
            if (File.Exists(FILE_NAME)) { 
                var stream = new StreamReader(FILE_NAME);
                var txt = await stream.ReadToEndAsync();
                var _categories = Parse(txt);
                Category[] categories = new Category[_categories.Count];
                int i = 0;
                foreach(var key in _categories.Keys) {
                    categories.SetValue(new Category() {
                        Name = key,
                        Items = _categories[key]
                    }, i++);
                }
                Categories.ItemsSource = categories;
                Items.ItemsSource = categories;
            } else {
                new FileStream(FILE_NAME, FileMode.CreateNew);
            }
        }

        private void OnDrop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                var name = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
                Trace.WriteLine(name);
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e) {
            DragMove();
        }

        private void OnListScroll(object sender, MouseWheelEventArgs e) {
            ScrollViewer scroller = (ScrollViewer)sender;
            scroller.ScrollToVerticalOffset(scroller.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void OnCategoryScroll(object sender, MouseWheelEventArgs e) {
            ListBox box = (ListBox)sender;
            var border = (Border)VisualTreeHelper.GetChild(box, 0);
            var scroller = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
            scroller.ScrollToHorizontalOffset(scroller.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
