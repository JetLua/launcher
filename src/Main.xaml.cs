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

        private ListBox categoryBox;
        private ListBox itemBox;
        private int categoryIndex = 0;
        private object currentItem;
        private object currentCategory;
        private bool dragItem = false;
        private bool dragCategory = false;

        private ObservableCollection<Category> categories = new ObservableCollection<Category>();

        public Main() {
            InitializeComponent();

            //获取scrollview
            categoryBox = FindName("category") as ListBox;
            itemBox = FindName("item") as ListBox;

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
                var _categories = Parse(txt);


                //关闭流
                stream.Close();

                if (_categories != null) {
                    //遍历分类
                    foreach (var key in _categories.Keys) {
                        //添加 icon
                        var items = new ObservableCollection<Item>();
                        foreach (var i in _categories[key]) {
                            var _i = i as Newtonsoft.Json.Linq.JObject;
                            items.Add(new Item() {
                                Name = (string)_i.GetValue("Name"),
                                Path = (string)_i.GetValue("Path"),
                                Icon = GetIcon((string)_i.GetValue("Path"))
                            });
                        }
                        categories.Add(new Category() {
                            Name = key,
                            Items = items
                        });
                    }
                }

            } else {
                //创建初始分类
                new FileStream(FILE_NAME, FileMode.CreateNew);
            }

            categories.Add(new Category() {
                Name = "+",
                Items = new ObservableCollection<Item>()
            });

            categoryBox.ItemsSource = categories;
            itemBox.ItemsSource = categories[categoryIndex].Items;
        }

        private void OnDrop(object sender, DragEventArgs e) {
            if (categories.Count < 2) {
                MessageBox.Show("请先点击 + , 创建分类。");
            } else if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                var path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();

                //当前分类添加 item
                categories[categoryIndex].Items.Add(new Item() {
                    Name = GetName(path),
                    Path = path,
                    Icon = GetIcon(path)
                });
            }
        }

        public void Save() {
            using (var stream = new StreamWriter(FILE_NAME)) {
                //stream.Write(JsonConvert.SerializeObject(categories));
                stream.Write(Format.Save(categories));
            }
            //Format.Save(categories);
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
            var index = itemBox.SelectedIndex;
            var item = categories[categoryIndex].Items[index];

            item.Flag =
                item.Flag == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;

        }

        //删除 item
        private void OnDeleteItem(object sender, MouseButtonEventArgs e) {
            var index = itemBox.SelectedIndex;
            var item = categories[categoryIndex].Items[index];
            categories[categoryIndex].Items.RemoveAt(index);
            e.Handled = true;
        }


        //按下鼠标
        private void OnItemMouseDown(object sender, MouseButtonEventArgs e) {
            dragItem = true;
            currentItem = sender;
        }

        private void OnItemMouseUp(object sender, MouseButtonEventArgs e) {
            dragItem = false;
            var index = itemBox.SelectedIndex;
            var item = categories[categoryIndex].Items[index] as Item;
            if (item.Flag == Visibility.Visible) {
                Process.Start(item.Path);
            }
        }

        private void OnItemMouseLeave(object sender, MouseEventArgs e) {
            if (dragItem && currentItem == sender) {
                var item = ((sender as ListBoxItem).DataContext) as Item;
                if (sender is ListBoxItem && item.Flag == Visibility.Visible) {
                    var _item = sender as ListBoxItem;
                    DragDrop.DoDragDrop(_item, _item, DragDropEffects.Move);
                    dragItem = false;
                }
            }
        }

        // item 放下
        private void OnItemDrop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(typeof(ListBoxItem))) {
                var target = sender as ListBoxItem;
                var source = e.Data.GetData(typeof(ListBoxItem)) as ListBoxItem;

                var tIndex = itemBox.Items.IndexOf(target.DataContext);
                var sIndex = itemBox.Items.IndexOf(source.DataContext);

                target.IsSelected = true;

                categories[categoryIndex].Items.Move(sIndex, tIndex);
            }
        }

        private void OnItemBoxDragOver(object sender, DragEventArgs e) {
            var scroller = sender as ScrollViewer;
            var point = e.GetPosition(scroller);
            var delta = 3;
            if (point.Y < scroller.ActualHeight / 2 - 25) {
                scroller.ScrollToVerticalOffset(scroller.VerticalOffset - delta);
            } else if (point.Y > scroller.ActualHeight / 2 + 25) {
                scroller.ScrollToVerticalOffset(scroller.VerticalOffset + delta);
            }
        }

        /*
        * 分类操作
        */

        //删除分类
        private void OnDeleteCategory(object sender, MouseButtonEventArgs e) {
            var index = categoryBox.SelectedIndex;
            categories.RemoveAt(index);

            //重新显示当前 item
            categoryIndex = 0;
            itemBox.ItemsSource = categories[categoryIndex].Items;

            e.Handled = true;
        }

        //编辑分类
        private void OnEditCategory(object sender, MouseButtonEventArgs e) {
            var index = categoryBox.SelectedIndex;
            
            //+ 号不允许编辑
            if (index + 1 == categories.Count) return;

            var item = categories[index];

            item.Flag =
                item.Flag == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }


        private void OnCategoryMouseDown(object sender, MouseButtonEventArgs e) {
            var item = sender as ListBoxItem;
            if (categoryBox.Items.IndexOf(item.DataContext) + 1 == 
                categories.Count) return;
            dragCategory = true;
            currentCategory = sender;
        }

        private void OnCategoryMouseUp(object sender, MouseButtonEventArgs e) {
            dragCategory = false;

            categoryIndex = categoryBox.SelectedIndex;
            var category = categories[categoryIndex];

            if (category.Flag != Visibility.Visible) return;

            //加号：+
            if (categoryIndex + 1 == categories.Count) {
                categories.Insert(categoryIndex, new Category() {
                    Name = string.Concat("新建分类@", categories.Count - 1),
                    Items = new ObservableCollection<Item>()
                });
                itemBox.ItemsSource = categories[categoryIndex].Items;
            } else {
                itemBox.ItemsSource = category.Items;
            }
        }

        private void OnCategoryMouseLeave(object sender, MouseEventArgs e) {
            if (dragCategory && currentCategory == sender) {
                var item = ((sender as ListBoxItem).DataContext) as Category;
                if (sender is ListBoxItem && item.Flag == Visibility.Visible) {
                    var _item = sender as ListBoxItem;
                    DragDrop.DoDragDrop(_item, _item, DragDropEffects.Move);
                    dragCategory = false;
                }
            }
        }

        //放下
        private void OnCategoryDrop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(typeof(ListBoxItem))) {
                var target = sender as ListBoxItem;
                var source = e.Data.GetData(typeof(ListBoxItem)) as ListBoxItem;

                var tIndex = categoryBox.Items.IndexOf(target.DataContext);
                var sIndex = categoryBox.Items.IndexOf(source.DataContext);

                categoryIndex = tIndex;
                source.IsSelected = true;

                //判断目标item是否是加号
                if (tIndex < categories.Count - 1) {
                    categories.Move(sIndex, tIndex);
                }
            }
        }

        private void OnCategoryDragOver(object sender, DragEventArgs e) {
            var border = VisualTreeHelper.GetChild(categoryBox, 0) as Border;
            var scroller = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
            var point = e.GetPosition(scroller);
            var delta = 3;
            if (point.X < scroller.ActualWidth / 2 - 25) {
                scroller.ScrollToHorizontalOffset(scroller.HorizontalOffset - delta);
            } else if (point.X > scroller.ActualWidth / 2 + 25) {
                scroller.ScrollToHorizontalOffset(scroller.HorizontalOffset + delta);
            }
        }

       
    }
}
