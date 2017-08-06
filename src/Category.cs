using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace launcher {
    class Category {
        public string Name { get; set; }
        public string[] Items { get; set; }
        public ImageSource Icon { get; set; }
    }
}
