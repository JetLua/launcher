using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace launcher {
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : Window {
        public Main() {
            InitializeComponent();

            //获取scrollview
            var category = (ListBox)FindName("category");
            var items = (ListBox)FindName("list");

            const int j = 10;
            Category[] categories = new Category[j];
            for (var i = 0; i < j; i++) {
                categories[i] = new Category() {
                    Name = "小楼一夜听春雨"
                };
            }
            category.ItemsSource = categories;
            items.ItemsSource = categories;
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
