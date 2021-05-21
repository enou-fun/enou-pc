using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using Newtonsoft.Json.Linq;
using OCRLibrary;
using TranslatorLibrary;

namespace Enou
{
    /// <summary>
    /// GlobalOCRWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GlobalOCRWindow : Window
    {

        public GlobalOCRWindow()
        {
            InitializeComponent();
        }


        //todo refactor 
        public void DoSearchWithoutInit(System.Drawing.Image img)
        {
            IOptChaRec ocr;
            string res = null;
            if (Common.appSettings.OCRsource == "TesseractOCR")
            {
                ocr = TesseractOCR.Instance;
                ocr.SetOCRSourceLang(Common.appSettings.GlobalOCRLang);
                res = ocr.OCRProcess(new System.Drawing.Bitmap(img));
            }
            else if (Common.appSettings.OCRsource == "BaiduOCR")
            {
                ocr = new BaiduGeneralOCR();
                if (ocr.OCR_Init(Common.appSettings.BDOCR_APIKEY, Common.appSettings.BDOCR_SecretKey))
                {
                    ocr.SetOCRSourceLang(Common.appSettings.GlobalOCRLang);
                    res = ocr.OCRProcess(new System.Drawing.Bitmap(img));

                    if (res != null)
                    {
                    }
                    else
                    {
                        HandyControl.Controls.Growl.ErrorGlobal($"百度OCR {Application.Current.Resources["APITest_Error_Hint"]}\n{ocr.GetLastError()}");
                    }
                }
                else
                {
                    HandyControl.Controls.Growl.ErrorGlobal($"百度OCR {Application.Current.Resources["APITest_Error_Hint"]}\n{ocr.GetLastError()}");
                }
            }

            if (res == null)
            {
                FirstTransText.Text = "OCR ERROR";
            }
            else
            {
                res = res.ToLower().Replace(".","").Replace(",","").Replace("!","").Replace("\"","");
                String web = res.Replace("\n", "%20").Replace(" ", "%20").Replace("\t", "%20").Replace("\r", "%20");
                String enouServer = res.Trim().Replace("\n", "").Replace("\t", "").Replace("\r", "");
                SearchOnWeb(web);
                long wordId = HttpClientWrapper.SaveWordToEnouServerGetId(enouServer);
                //ModifyWordAsync(wordId, enouServer );
            }
        }
 
        private void SearchOnWeb(string word)
        {
            string str = "www.bing.com/search?q=" + word + "%20define\"&\"ensearch=1";

            Tools.OpenBrowser(str);

        }


        private void ModifyWordAsync(long wordId, string word)
        {

            Point point = new Point { X = System.Windows.Forms.Control.MousePosition.X, Y = System.Windows.Forms.Control.MousePosition.Y };
            ModifyWordWindow modifyWordWindow = new ModifyWordWindow(point); 
            modifyWordWindow.WordId = wordId;
            modifyWordWindow.Word = word;

            modifyWordWindow.Show();
        } 



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //dataInit();
        }
    }
}