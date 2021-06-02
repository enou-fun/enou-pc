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

namespace Enou
{
    /// <summary>
    /// ModifyWordWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LearnWordWindow : Window
    {
        public String Text { get; set; }

        private List<String> wordList = new List<String>();

        private ToolTip toolTip = new ToolTip();

        public static LearnWordWindow Instance { get; set; }

        public LearnWordWindow()
        {
            Instance = this;

            InitializeComponent();
            this.Topmost = true;

        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {

            foreach(var word in wordList)
            {
                LearnWord(word);
            }
            Refresh();

            //this.Hide();
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public void Refresh()
        {

            wordList = TurnTextToWords(Text).ToList();
            GenerateButtons(wordList, wrapPanelFiltered);
        }

        private IEnumerable<String> TurnTextToWords(String text)
        {
            var punctuation = text.Where(Char.IsPunctuation).Distinct().ToArray();
            var words = text.Split().Select(x => x.Trim(punctuation)).ToList();
            words.RemoveAll(x => x.Trim().Equals(String.Empty));
            //words.RemoveAll(x => Common.WordAlreadyKnown(x));
            //words.RemoveAll(x => Common.WordIgnored(x));
            return words;
        }

        private void GenerateButtons(IEnumerable<String> words, Panel panel)
        {
            panel.Children.Clear();
            foreach(var word in words)
            {
                Button button = new Button();
                button.VerticalAlignment = VerticalAlignment.Top;
                button.Content = word;
                button.Click += new System.Windows.RoutedEventHandler(this.SearchWebButton_Click);
                button.FontSize = 16;
                button.Padding = new Thickness(2, 2, 2, 2);
                button.BorderThickness = new Thickness(0, 0, 0, 0);
                if(!Common.WordAlreadyKnown(word) && !Common.WordIgnored(word))
                {
                    button.Foreground = Brushes.Red;
                    button.FontWeight = FontWeights.Bold;
                }
                panel.Children.Add(button);
            }
        }

        private void LearnWord(String word)
        {
            bool succeed = HttpClientWrapper.LearnWord(word);
            if(succeed)
            {
                Common.AddKnownWord(word);
            } 
            else
            {
                Common.AddIgnoreWords(word);
            }
        }


        private void SearchWebButton_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button)
            {

                Button button = sender as Button;
                string str = "www.bing.com/search?q=" + button.Content + "%20define\"&\"ensearch=1";

                Tools.OpenBrowser(str);
            }
        }

        private void buttonTest_Click(object sender, RoutedEventArgs e)
        {
            toolTip.IsOpen = !toolTip.IsOpen;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void originTextBoxWord_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
