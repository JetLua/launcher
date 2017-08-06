using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace launcher {
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application {
        private static Mutex id = null;
        protected override void OnStartup(StartupEventArgs e) {
            id = new Mutex(true, "launcher", out bool flag);
            if (!flag) {
                MessageBox.Show("已有运行实例");
                Current.Shutdown();
            } else {
                StartupUri = new Uri("src/Main.xaml", UriKind.Relative);
                base.OnStartup(e);
            }
        }
    }
}
