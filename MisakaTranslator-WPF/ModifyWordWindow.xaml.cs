using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
    /// ModifyWordWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ModifyWordWindow : Window
    {
        public long WordId { get; set; }
        public String Word { get; set; }

        public ModifyWordWindow(Point mousePoint)
        {
            InitializeComponent();
            this.Topmost = true;


            this.Top = mousePoint.Y - this.textBoxWord.Margin.Top - this.textBoxWord.Height*2 ;
            this.Left = mousePoint.X - this.textBoxWord.Margin.Left - this.textBoxWord.Width/2 -5;
            this.textBoxWord.Focus();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            HttpClientWrapper.ModifyWord(this.WordId, this.textBoxWord.Text);

            this.Close();
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.textBoxWord.Text = Word;
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
