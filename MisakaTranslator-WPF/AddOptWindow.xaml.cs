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
using System.Windows.Shapes;

namespace MisakaTranslator_WPF
{
    /// <summary>
    /// AddOptWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddOptWindow : Window
    {
        public AddOptWindow(string src = "")
        {
            InitializeComponent();
            this.Topmost = true;

            List<string> wordtype = new List<string>()
            {
                "人名",
                "地名"
            };

            srcText.Text = src;
            wordTypeCombox.ItemsSource = wordtype;
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}