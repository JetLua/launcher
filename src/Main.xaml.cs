using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace launcher {
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    
    class Format {
        public static string Save(ObservableCollection<launcher.Category> categories) {
            var _categories = new Dictionary<string, ArrayList>();
            for (var i = 0; i < categories.Count - 1; i++) {
                var category = categories[i];
                var items = new ArrayList();
                foreach (var item in category.Items) {
                    items.Add(new Dictionary<string, string> {
                        { "Name", item.Name },
                        { "Path", item.Path }
                    });
                }
                _categories[category.Name] = items;
            }
            return JsonConvert.SerializeObject(_categories);
        }
    }

    public partial class Main : Window {

        private string FILE_NAME = String.Concat(AppDomain.CurrentDomain.BaseDirectory, "data.json");

        private ListBox CategoryBox;
        private ListBox ItemBox;
        private int CategoryIndex = 0;
        private ObservableCollection<Category> Categories = new ObservableCollection<Category>();

        public Main() {
            InitializeComponent();

            //获取scrollview
            CategoryBox = FindName("category") as ListBox;
            ItemBox = FindName("item") as ListBox;

            //赋值给 app
            App.MainBody = this;

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
            }
        }

        public void Save() {
            using (var stream = new StreamWriter(FILE_NAME)) {
                //stream.Write(JsonConvert.SerializeObject(Categories));
                stream.Write(Format.Save(Categories));
            }
            //Format.Save(Categories);
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



        /*
        * item 操作
        */

        //编辑 item
        private void OnEditItem(object sender, MouseButtonEventArgs e) {
            var index = ItemBox.SelectedIndex;
            var item = Categories[CategoryIndex].Items[index];

            item.Flag =
                item.Flag == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;

        }

        //删除 item
        private void OnDeleteItem(object sender, MouseButtonEventArgs e) {
            var index = ItemBox.SelectedIndex;
            var item = Categories[CategoryIndex].Items[index];
            Categories[CategoryIndex].Items.RemoveAt(index);
            e.Handled = true;
        }

        //启动
        private void OnLaunch(object sender, MouseButtonEventArgs e) {
            Trace.WriteLine("launch");
            var index = ItemBox.SelectedIndex;
            var path = Categories[CategoryIndex].Items[index].Path;
            Process.Start(path);
        }


        //开始拖动 item
        private void OnItemDragStart(object sender, MouseButtonEventArgs e) {

            Trace.WriteLine("drag item");
            var item = ((sender as ListBoxItem).DataContext) as Item;

            if (sender is ListBoxItem && item.Flag == Visibility.Visible) {
                var _item = sender as ListBoxItem;
                DragDrop.DoDragDrop(_item, _item, DragDropEffects.Move);
            } else {
                (sender as ListBoxItem).IsSelected = true;
            }
        }

        // item 放下
        private void OnItemDrop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(typeof(ListBoxItem))) {
                var target = sender as ListBoxItem;
                var source = e.Data.GetData(typeof(ListBoxItem)) as ListBoxItem;

                var tIndex = ItemBox.Items.IndexOf(target.DataContext);
                var sIndex = ItemBox.Items.IndexOf(source.DataContext);

                //Trace.WriteLine(string.Concat(tIndex, ",", sIndex));
                target.IsSelected = true;

                if (tIndex == sIndex) {
                    var index = ItemBox.SelectedIndex;
                    var path = Categories[CategoryIndex].Items[index].Path;
                    Process.Start(path);
                } else {
                    Categories[CategoryIndex].Items.Move(sIndex, tIndex);
                }

            }
        }

        private void OnItemBoxDragOver(object sender, DragEventArgs e) {
            //版本一：
            var scroller = sender as ScrollViewer;
            var pointInBox = e.GetPosition(ItemBox);
            var pointInScroller = e.GetPosition(scroller);
            var delta = 3;
            if (pointInScroller.Y < scroller.ActualHeight / 2 - 25) {
                scroller.ScrollToVerticalOffset(scroller.VerticalOffset - delta);
            } else if (pointInScroller.Y > scroller.ActualHeight / 2 + 25) {
                scroller.ScrollToVerticalOffset(scroller.VerticalOffset + delta);
            }
        }

        /*
        * 分类操作
        */

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

        //选择分类
        private void OnSelectCategory(object sender, MouseButtonEventArgs e) {

            Trace.WriteLine("select category");

            CategoryIndex = CategoryBox.SelectedIndex;

            //点击 + ：总是最后一个
            //添加分类
            if (CategoryIndex + 1 == Categories.Count) {
                Categories.Insert(CategoryIndex, new Category() {
                    Name = string.Concat("新建分类@", Categories.Count - 1),
                    Items = new ObservableCollection<Item>()
                });
                ItemBox.ItemsSource = Categories[CategoryIndex].Items;
            }
            
        }

        //拖拽开始
        private void OnCategoryDragStart(object sender, MouseButtonEventArgs e) {
            Trace.WriteLine("category drag");
            var category = ((sender as ListBoxItem).DataContext) as Category;
            if (sender is ListBoxItem && category.Flag == Visibility.Visible) {
                var item = sender as ListBoxItem;
                DragDrop.DoDragDrop(item, item, DragDropEffects.Move);
                //Trace.WriteLine(CategoryBox.SelectedIndex);
            } else {
                (sender as ListBoxItem).IsSelected = true;
            }
        }

        //放下
        private void OnCategoryDrop(object sender, DragEventArgs e) {
            Trace.WriteLine("category drop");
            if (e.Data.GetDataPresent(typeof(ListBoxItem))) {
                var target = sender as ListBoxItem;
                var source = e.Data.GetData(typeof(ListBoxItem)) as ListBoxItem;

                var tIndex = CategoryBox.Items.IndexOf(target.DataContext);
                var sIndex = CategoryBox.Items.IndexOf(source.DataContext);

                target.IsSelected = true;
                CategoryIndex = CategoryBox.SelectedIndex;
                Trace.WriteLine(CategoryBox.SelectedIndex);


                //判断目标item是否是加号
                if (tIndex == Categories.Count - 1) {
                    if (tIndex == sIndex) {
                        //添加分类
                        Categories.Insert(CategoryIndex, new Category() {
                            Name = string.Concat("新建分类@", Categories.Count - 1),
                            Items = new ObservableCollection<Item>()
                        });
                    }
                } else {
                    Categories.Move(sIndex, tIndex);
                }

                ItemBox.ItemsSource = Categories[CategoryIndex].Items;

            }
        }

        private void OnCategoryDragOver(object sender, DragEventArgs e) {
            var box = sender as ListBox;
            var border = VisualTreeHelper.GetChild(box, 0) as Border;
            var scroller = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
            var point = e.GetPosition(CategoryBox);
            scroller.ScrollToHorizontalOffset(point.X - box.ActualWidth / 2);
        }
    }
}
