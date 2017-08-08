using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Collections;
using System.Diagnostics;

namespace launcher {
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : Window {

        private const string FILE_NAME = "data.json";

        private ListBox CategoryBox;
        private ListBox ItemBox;
        private int CategoryIndex = 0;
        private ObservableCollection<Category> Categories = new ObservableCollection<Category>();

        public Main() {
            InitializeComponent();

            //获取scrollview
            CategoryBox = FindName("category") as ListBox;
            ItemBox = FindName("item") as ListBox;

            ReadData();
        }


        private Dictionary<string, ArrayList> Parse(string json) {
            Dictionary<string, ArrayList> categories = null;
            try {
                categories = JsonConvert.DeserializeObject<Dictionary<string, ArrayList>>(json);
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


                //关闭流
                stream.Close();

                if (categories != null) {
                    //遍历分类
                    foreach (var key in categories.Keys) {
                        //添加 icon
                        var items = new ObservableCollection<Item>();
                        foreach (var i in categories[key]) {
                            var _i = i as Newtonsoft.Json.Linq.JObject;
                            items.Add(new Item() {
                                Name = (string)_i.GetValue("Name"),
                                Path = (string)_i.GetValue("Path"),
                                Icon = GetIcon((string)_i.GetValue("Path"))
                            });
                        }
                        Categories.Add(new Category() {
                            Name = key,
                            Items = items
                        });
                    }
                }

            } else {
                //创建初始分类
                new FileStream(FILE_NAME, FileMode.CreateNew);
            }

            Categories.Add(new Category() {
                Name = "+",
                Items = new ObservableCollection<Item>()
            });

            CategoryBox.ItemsSource = Categories;
            ItemBox.ItemsSource = Categories[CategoryIndex].Items;
        }

        private void OnDrop(object sender, DragEventArgs e) {
            if (Categories.Count < 2) {
                MessageBox.Show("请先点击 + , 创建分类。");
            } else if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                var path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();

                //当前分类添加 item
                Categories[CategoryIndex].Items.Add(new Item() {
                    Name = GetName(path),
                    Path = path,
                    Icon = GetIcon(path)
                });

                Save();
            }
        }

        private void Save() {
            Trace.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(Categories));
        }

        private ImageSource GetIcon(string path) {
            var icon = IntPtr.Zero;
            var ok = Api.SHDefExtractIcon(path, 0, 0, out icon, out IntPtr _, 96);
            ImageSource img = null;
            if (ok == 0) {
                img = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    icon,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions()
                );
                Api.DestroyIcon(icon);
            } else {
                img = FindResource("lost") as ImageSource;
            }
            return img;
        }

        private string GetName(string path) {
            return Path.GetFileName(path);
        }

        private void OnDragMove(object sender, MouseButtonEventArgs e) {
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

        private void OnEditItem(object sender, MouseButtonEventArgs e) {
            var index = ItemBox.SelectedIndex;
            var item = Categories[CategoryIndex].Items[index];

            item.Flag =
                item.Flag == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;

        }

        private void OnDeleteItem(object sender, MouseButtonEventArgs e) {
            var index = ItemBox.SelectedIndex;
            var item = Categories[CategoryIndex].Items[index];
            Categories[CategoryIndex].Items.RemoveAt(index);
        }

        private void OnSelectCategory(object sender, MouseButtonEventArgs e) {
            CategoryIndex = CategoryBox.SelectedIndex;

            //点击 + ：总是最后一个
            //添加分类
            if (CategoryIndex + 1 == Categories.Count) {
                Categories.Insert(CategoryIndex, new Category() {
                    Name = "新建分类",
                    Items = new ObservableCollection<Item>()
                });
            }
            ItemBox.ItemsSource = Categories[CategoryIndex].Items;
        }

        //删除分类
        private void OnDeleteCategory(object sender, MouseButtonEventArgs e) {
            var index = CategoryBox.SelectedIndex;
            Categories.RemoveAt(index);

            //重新显示当前 item
            CategoryIndex = 0;
            ItemBox.ItemsSource = Categories[CategoryIndex].Items;

            e.Handled = true;
        }

        //编辑分类
        private void OnEditCategory(object sender, MouseButtonEventArgs e) {
            var index = CategoryBox.SelectedIndex;
            var item = Categories[index];

            item.Flag =
                item.Flag == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        //启动
        private void OnLaunch(object sender, MouseButtonEventArgs e) {
            var index = ItemBox.SelectedIndex;
            var path = Categories[CategoryIndex].Items[index].Path;
            Process.Start(path);
        }
    }
}
