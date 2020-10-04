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

namespace MisakaTranslator_WPF
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

        public void DoSearchWithoutInit(System.Drawing.Image img)
        {
            IOptChaRec ocr;
            string res = null;
            if (Common.appSettings.OCRsource == "TesseractOCR")
            {
                ocr = new TesseractOCR();
                if (ocr.OCR_Init("", "") != false)
                {
                    ocr.SetOCRSourceLang(Common.appSettings.GlobalOCRLang);
                    res = ocr.OCRProcess(new System.Drawing.Bitmap(img));

                    if (res != null)
                    {
                    }
                    else
                    {
                        HandyControl.Controls.Growl.ErrorGlobal($"TesseractOCR {Application.Current.Resources["APITest_Error_Hint"]}\n{ocr.GetLastError()}");
                    }
                }
                else
                {
                    HandyControl.Controls.Growl.ErrorGlobal($"TesseractOCR {Application.Current.Resources["APITest_Error_Hint"]}\n{ocr.GetLastError()}");
                }
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
                String web = res.Replace("\n", "%20").Replace(" ", "%20").Replace("\t", "%20").Replace("\r", "%20");
                String enouServer = res.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");
                SearchOnWeb(web);
                long wordId = SendWordToEnouServerGetId(enouServer);
                ModifyWordAsync(wordId, enouServer );
            }
        }
 
        private void SearchOnWeb(string word)
        {
            string str = "start www.bing.com/search?q=" + word + "%20define\"&\"ensearch=1";

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序

            //向cmd窗口发送输入信息
            p.StandardInput.WriteLine(str + " &exit");

            p.StandardInput.AutoFlush = true;
            p.Close();

        }

        private long SendWordToEnouServerGetId(string word)
        {
            bool hasToken = !Common.appSettings.EnouAccountToken.Equals(String.Empty);
            if(!hasToken)
            {
                ChechAndGetTokenAsync();
            }

            String jsonWord = "{\"word\":\"" + word + "\"}";
            Console.WriteLine(jsonWord);
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonWord);

            String token = Common.appSettings.EnouAccountToken;
            Console.WriteLine(" SendWordToEnouServerAsync token is " + token);

            String api = Common.appSettings.EnouServerWordApi;
            WebRequest request = WebRequest.Create(api);
            request.Method = "POST";
            request.Headers.Add("token", token);
            request.ContentType = "application/json";
            request.ContentLength = byteArray.Length;

            using (Stream st = request.GetRequestStream())
                st.Write(byteArray, 0, byteArray.Length);

            WebResponse webResponse= null;
            long retWordId = 0;
            try
            {
                webResponse = request.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null && response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        ChechAndGetTokenAsync();
                        if(Common.appSettings.EnouAccountToken == null)
                        {
                            // todo login fail, throw exception
                        }
                        else
                        {
                            return SendWordToEnouServerGetId(word);
                        }
                    }
                }
            }
            finally
            {
                if(webResponse != null)
                {
                    Stream myResponseStream = webResponse.GetResponseStream();
                    StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
                    string retString = myStreamReader.ReadToEnd();
                    webResponse.Close();

                    JObject jObject = JObject.Parse(retString);
                    File.WriteAllText("log.txt","jObject is " + jObject.ToString());
                    retWordId = long.Parse( jObject["id"].ToString());
                }
            }

            return retWordId;
        }

        private void ModifyWordAsync(long wordId, string word)
        {

            Point point = new Point { X = System.Windows.Forms.Control.MousePosition.X, Y = System.Windows.Forms.Control.MousePosition.Y };
            ModifyWordWindow modifyWordWindow = new ModifyWordWindow() { Top = point.Y, Left = point.X, Topmost = true };
            modifyWordWindow.WordId = wordId;
            modifyWordWindow.Word = word;

            modifyWordWindow.Show();
        } 


        private void ChechAndGetTokenAsync()
        {

            String account = Common.appSettings.EnouAccount;
            String password = Common.appSettings.EnouPassword;
            String loginInfo = "{\"account\":\"" + account + "\", \"password\":\""+password+"\"}";
            Console.WriteLine(loginInfo);


            HttpClient client = new HttpClient();
            Uri uri = new Uri(Common.appSettings.EnouServerLoginApi);
            client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

            StringContent content = new StringContent(loginInfo, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = client.PostAsync(uri, content).Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonString = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine("token is " + jsonString);
                    Common.appSettings.EnouAccountToken = jsonString;
                }
            }
            catch
            {

            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //dataInit();
        }
    }
}