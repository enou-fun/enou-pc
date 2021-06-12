using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using KeyboardMouseHookLibrary;
using Newtonsoft.Json.Linq;
using OCRLibrary;

namespace Enou
{
    public class Common
    {
        public static IAppSettings appSettings; //应用设置

        public static int transMode; //全局使用中的翻译模式 1=hook 2=ocr


        public static string UsingSrcLang; //全局使用中的源语言
        public static string UsingDstLang; //全局使用中的目标翻译语言

        public static IOptChaRec ocr; //全局使用中的OCR对象
        public static bool isAllWindowCap; //是否全屏截图
        public static IntPtr OCRWinHwnd; //全局的OCR的工作窗口
        public static HotKeyInfo UsingHotKey; //全局使用中的触发键信息
        public static int UsingOCRDelay; //全局使用中的OCR延时

        public static Window mainWin; //全局的主窗口对象

        public static GlobalHotKey GlobalOCRHotKey; //全局OCR热键

        private static HashSet<string> knownWordSet = new HashSet<string>();

        private static HashSet<String> ignoreWordSet = new HashSet<string>();

           /// <summary>
        /// 根据进程PID找到程序所在路径
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static string FindProcessPath(int pid)
        {
            Process[] ps = Process.GetProcesses();
            string filepath = "";
            for (int i = 0; i < ps.Length; i++)
            {
                if (ps[i].Id == pid)
                {
                    try
                    {
                        filepath = ps[i].MainModule.FileName;
                    }
                    catch (System.ComponentModel.Win32Exception ex)
                    {
                        continue;
                        //这个地方直接跳过，是因为32位程序确实会读到64位的系统进程，而系统进程是不能被访问的
                    }
                    break;
                }
            }
            return filepath;
        }

        /// <summary>
        /// 全局OCR
        /// </summary>
        public static void GlobalOCR()
        {
            BitmapImage img = ImageProcFunc.ImageToBitmapImage(ScreenCapture.GetAllWindow());
            ScreenCaptureWindow scw = new ScreenCaptureWindow(img, 2);
            scw.Width = img.PixelWidth;
            scw.Height = img.PixelHeight;
            scw.Topmost = true;
            scw.Left = 0;
            scw.Top = 0;
            scw.Show();
        }

        /// <summary>
        /// 获取DPI缩放倍数
        /// </summary>
        /// <returns>DPI缩放倍数</returns>
        public static double GetScale()
        {
            Graphics currentGraphics = Graphics.FromHwnd(new WindowInteropHelper(mainWin).Handle);
            return currentGraphics.DpiX / 96;
        }

        /// <summary>
        /// 检查软件更新
        /// </summary>
        /// <returns>如果已经是最新或获取更新失败，返回NULL，否则返回更新信息可直接显示</returns>
        public static List<string> CheckUpdate() {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string currentVersion = version.ToString();

            string url = "https://hanmin0822.github.io/Enou/index.html";

            string strResult = "";

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                //声明一个HttpWebRequest请求
                request.Timeout = 30000;
                //设置连接超时时间
                request.Headers.Set("Pragma", "no-cache");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream streamReceive = response.GetResponseStream();
                Encoding encoding = Encoding.GetEncoding("GB2312");
                StreamReader streamReader = new StreamReader(streamReceive, encoding);
                strResult = streamReader.ReadToEnd();
            }
            catch
            {
                return null;
            }

            if (strResult != null) {
                string newVersion = GetMiddleStr(strResult, "LatestVersion[", "]");

                if (newVersion == null) {
                    return null;
                }

                if (currentVersion == newVersion)
                {
                    return null;
                }
                else {
                    string downloadPath = GetMiddleStr(strResult, "DownloadPath[", "]");
                    return new List<string>() {
                        newVersion,downloadPath
                    };
                }
            }

            return null;
        }

        /// <summary>
        /// 取字符串中间
        /// </summary>
        /// <param name="oldStr"></param>
        /// <param name="preStr"></param>
        /// <param name="nextStr"></param>
        /// <returns></returns>
        public static string GetMiddleStr(string oldStr, string preStr, string nextStr)
        {
            try
            {
                string tempStr = oldStr.Substring(oldStr.IndexOf(preStr) + preStr.Length);
                tempStr = tempStr.Substring(0, tempStr.IndexOf(nextStr));
                return tempStr;
            }
            catch (Exception) {
                return null;
            }
        }

        public static void AddKnownWords(List<String> wordList)
        {
            foreach(var word in wordList)
            {
                knownWordSet.Add(word);       
            }
        }

        public static int GetKnownWordsCount()
        {
            return knownWordSet.Count;
        }

        public static void AddKnownWord(String word)
        {
            knownWordSet.Add(word.ToLower());
        }

        public static bool WordAlreadyKnown(String word)
        {
            return knownWordSet.Contains(word.ToLower());
        }

        public static bool WordIgnored(String word)
        {
            return ignoreWordSet.Contains(word.ToLower());
        }

        public static void LoadIgnoreWords()
        {
            if (appSettings.EnouIgnoreWords.Equals(String.Empty))
                return;

            ignoreWordSet = appSettings.EnouIgnoreWords.Split(';').ToHashSet();
        }

        public static void SaveIgnoreWords()
        {
            appSettings.EnouIgnoreWords = String.Join(";", ignoreWordSet.ToList());
        }

        public static void AddIgnoreWords(String word)
        {
            ignoreWordSet.Add(word.ToLower());
            SaveIgnoreWords();
        }
    }
}