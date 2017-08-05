using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Collections.ObjectModel;

namespace launcher {
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : Window {

        private const string FILE_NAME = "data.json";

        private ListBox CategoryBox;
        private ListBox ItemBox;
        private ObservableCollection<Category> Categories = new ObservableCollection<Category>();

        public Main() {
            InitializeComponent();

            //获取scrollview
            CategoryBox = FindName("category") as ListBox;
            ItemBox = FindName("list") as ListBox;

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
                var categories = Parse(txt);

                if (categories == null || categories.Count == 0) {
                    MessageBox.Show("无数据");
                    return;
                }

                foreach (var key in categories.Keys) {
                    Categories.Add(new Category() {
                        Name = key,
                        Items = categories[key]
                    });
                }

                CategoryBox.ItemsSource = Categories;
                ItemBox.ItemsSource = Categories;
            } else {
                new FileStream(FILE_NAME, FileMode.CreateNew);
            }
        }

        private void OnDrop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                var name = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
                Trace.WriteLine(name);
            }
            Categories.Add(new Category() {
                Name = "ok",
                Items = new string[] {"ok"}
            });
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e) {
            DragMove();
        }

        private void OnListScroll(object sender, MouseWheelEventArgs e) {
            var scroller = sender as ScrollViewer;
            scroller.ScrollToVerticalOffset(scroller.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void OnCategoryScroll(object sender, MouseWheelEventArgs e) {
            var box = sender as ListBox;
            var border = VisualTreeHelper.GetChild(box, 0) as Border;
            var scroller = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
            scroller.ScrollToHorizontalOffset(scroller.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
