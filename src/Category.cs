using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows;

namespace launcher {
    class Category : INotifyPropertyChanged {
        private Visibility flag = Visibility.Visible;
        public Visibility Flag {
            get { return flag; }
            set {
                flag = value;
                NotifyPropertyChanged();
            }
        }
        private string name;
        public string Name {
            get { return name; }
            set {
                name = value;
                NotifyPropertyChanged();
            }
        }
        public ObservableCollection<Item> Items { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string name = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public static bool Convert(bool flag) {
            return !flag;
        }
    }

    class Item : INotifyPropertyChanged {
        private string name;
        private Visibility flag = Visibility.Visible;
        public string Name {
            get { return name; }
            set {
                name = value;
                NotifyPropertyChanged();
            }
        }

        public Visibility Flag {
            get { return flag; }
            set {
                flag = value;
                NotifyPropertyChanged();
            }
        }
        public string Path { get; set; }
        public ImageSource Icon { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string name="") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public static bool Convert(bool flag) {
            return !flag;
        }
    }
}
