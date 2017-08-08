using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading;
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

                //托盘图标
                var ctxMenu = new System.Windows.Forms.ContextMenu();
                var exit = new System.Windows.Forms.MenuItem() {
                    Text = "退出"
                };
                var autostart = new System.Windows.Forms.MenuItem() {
                    Text = "开机启动"
                };
                exit.Click += OnExit;
                autostart.Click += OnAutostart;

                ctxMenu.MenuItems.Add(autostart);
                ctxMenu.MenuItems.Add(exit);


                new System.Windows.Forms.NotifyIcon() {
                    Icon = launcher.Properties.Resources.icon,
                    Visible = true,
                    ContextMenu = ctxMenu
                };
            }
        }

        private void AddAutostart() {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)) {
                key.SetValue("launcher", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
            }
        }

        private void RemoveAutostart() {

        }

        private void OnAutostart(object sender, EventArgs e) {
            var item = sender as System.Windows.Forms.MenuItem;
            item.Checked = item.Checked ? false : true;

            if (item.Checked) {
                AddAutostart();
            } else {
                RemoveAutostart();
            }

        }

        private void OnExit(object sender, EventArgs e) {
            Current.Shutdown();
        }
    }
}
