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

        private List<List<String>> paraWordList = new List<List<String>>();

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

            foreach(var wordPara in paraWordList)
            {
                foreach(var word in wordPara)
                {
                    if(!Common.WordAlreadyKnown(word) && !Common.WordIgnored(word) && !IsPunctuation(word[0]))
                    {
                        LearnWord(word);
                    }
                }
            }
            Refresh();

            //this.Hide();
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public void Refresh()
        {
            paraWordList = TurnTextToParagraghs(Text).ToList();
            GenerateButtons(paraWordList, wrapPanelFiltered);
        }

        private IEnumerable<List<String>> TurnTextToParagraghs(String text)
        {
            text = text.Replace("-\n", "");
            String[] seperatorArray = { "\n\n" };
            List<String> paragraphList = text.Split(seperatorArray, StringSplitOptions.RemoveEmptyEntries).ToList();
            paragraphList.RemoveAll(str => str.Trim().Equals(String.Empty));

            List<List<String>> paragraphWordList = new List<List<string>>();

            var textSplit = text.Split();

            foreach(var paraText in paragraphList)
            {
                List<String> words = TurnTextToWords(paraText);
                paragraphWordList.Add(words);
            }

            return paragraphWordList;
        }
        private List<String>  TurnTextToWords(String text)
        {
            var punctuation = text.Where(IsPunctuation).Distinct().ToArray();
            var words = text.Split(); 
            List<String> result = new List<String>();
            foreach(var word in words)
            {
                result.AddRange(Trim(word, punctuation));
            }
            //words.RemoveAll(x => Common.WordAlreadyKnown(x));
            //words.RemoveAll(x => Common.WordIgnored(x));
            return result;
        }

        private bool IsPunctuation(Char c)
        {
            return Char.IsPunctuation(c) && c != '-';
        }

        private List<String> Trim(String word, char[] punctuation)
        {
            List<String> result = new List<String>();
            List<int> splitBeforeIndex = new List<int>();
            splitBeforeIndex.Add(0);
            for(int i = 1; i < word.Length; ++i)
            {
                bool wordBeforeIsPunc = IsPunctuation(word[i - 1]);
                bool wordIsPunc = IsPunctuation(word[i]);
                if(wordIsPunc != wordBeforeIsPunc)
                {
                    splitBeforeIndex.Add(i);
                }
            }

            splitBeforeIndex.Add(word.Length);

            for(int i = 0; i < splitBeforeIndex.Count-1; ++i)
            {
                result.Add(word.Substring(splitBeforeIndex[i], splitBeforeIndex[i + 1] - splitBeforeIndex[i]));
            }
            return result;
        }

        private void GenerateButtons(List<List<String>> wordParaList, Panel panel)
        {
            panel.Children.Clear();
            foreach(var wordPara in wordParaList)
            {
                foreach(var word in wordPara)
                {
                    if (word.Length == 0)
                        continue;

                    bool isPunctuation = IsPunctuation(word[0]);
                    if (isPunctuation)
                    {
                        Label label = new Label();
                        //label.VerticalAlignment = VerticalAlignment.Top;
                        label.Content = word;
                        label.FontSize = 16;
                        label.Padding = new Thickness(1, 1, 1, 1);
                        label.BorderThickness = new Thickness(0, 0, 0, 0);
                        panel.Children.Add(label);
                        continue;
                    }

                    Button button = new Button();
                   // button.VerticalAlignment = VerticalAlignment.Top;
                    button.Content = word;
                    button.Click += new System.Windows.RoutedEventHandler(this.SearchWebButton_Click);
                    button.FontSize = 16;
                    button.Padding = new Thickness(2, 2, 2, 2);
                    button.BorderThickness = new Thickness(0, 0, 0, 0);
                    if (!Common.WordAlreadyKnown(word) && !Common.WordIgnored(word))
                    {
                        button.Foreground = Brushes.Red;
                        button.FontWeight = FontWeights.Bold;
                    }


                    panel.Children.Add(button);
                }

                Label lineLabel = new Label();
                lineLabel.Width = panel.Width;
                //lineLabel.Height = 2;
                lineLabel.BorderThickness = new Thickness(0, 0, 0, 0);
                panel.Children.Add(lineLabel);

            }
        }

        private void LearnWord(String word)
        {
            Task task = Task.Factory.StartNew(() => {
                word = word.ToLower();
                bool succeed = HttpClientWrapper.LearnWord(word);
                if (succeed)
                {
                    Common.AddKnownWord(word);
                }
                else
                {
                    Common.AddIgnoreWords(word);
                }
            });

        }


        private void SearchWebButton_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button)
            {

                Button button = sender as Button;
                string str = "www.bing.com/search?q=" + button.Content + "%20define\"&\"ensearch=1";
                LearnWord(button.Content.ToString());

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
