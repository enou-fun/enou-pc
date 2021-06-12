using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Config.Net;
using HandyControl.Controls;
using KeyboardMouseHookLibrary;
using OCRLibrary;
using MessageBox = System.Windows.MessageBox;

namespace Enou
{
    public partial class MainWindow
    {
        private int gid; //当前选中的顺序，并非游戏ID
        private IntPtr hwnd;

        public static MainWindow Instance { get; set; }

        public MainWindow()
        {
            Instance = this;
            Common.mainWin = this;

            InitializeLanguage();
            InitializeComponent();


            ThreadPool.RegisterWaitForSingleObject(App.ProgramStarted, OnProgramStarted, null, -1, false);
            //注册全局OCR热键
            this.SourceInitialized += new EventHandler(MainWindow_SourceInitialized);
        }

        // 当收到第二个进程的通知时，显示窗体
        void OnProgramStarted(object state, bool timeout)
        {
            MessageBox.Show("已经打开一个进程");
            this.Dispatcher.Invoke(() => { Visibility = Visibility.Visible; });
         
        }


        private static void InitializeLanguage()
        {
            var appResource = Application.Current.Resources.MergedDictionaries;
            foreach (var item in appResource)
            {
                if (item.Source.ToString().Contains("lang") && item.Source.ToString() != $@"lang/{Common.appSettings.AppLanguage}.xaml")
                {
                    appResource.Remove(item);
                    break;
                }
            }
        }

        //按下快捷键时被调用的方法
        public void CallBack()
        {
            Common.GlobalOCR();
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AddNewGameDrawer.IsOpen = true;
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            hwnd = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(hwnd)?.AddHook(WndProc);
            //注册热键
            Common.GlobalOCRHotKey = new GlobalHotKey();
            if (Common.GlobalOCRHotKey.RegisterHotKeyByStr(Common.appSettings.GlobalOCRHotkey, hwnd, CallBack) == false)
            {
                Growl.ErrorGlobal(Application.Current.Resources["MainWindow_GlobalOCRError_Hint"].ToString());
            }
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            Common.GlobalOCRHotKey.ProcessHotKey(System.Windows.Forms.Message.Create(hwnd, msg, wParam, lParam));
            return IntPtr.Zero;
        }

        private static SettingsWindow _settingsWindow;

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_settingsWindow == null || _settingsWindow.IsVisible == false)
            {
                _settingsWindow = new SettingsWindow();
                _settingsWindow.Show();
            }
            else
            {
                _settingsWindow.WindowState = WindowState.Normal;
                _settingsWindow.Activate();
            }
        }


        private static void Border_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var b = (Border)sender;
            b.BorderThickness = new Thickness(2);
        }

        private static void Border_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var b = (Border)sender;
            b.BorderThickness = new Thickness(0);
        }


        private void CloseDrawerBtn_Click(object sender, RoutedEventArgs e)
        {
            GameInfoDrawer.IsOpen = false;
        }

        private void BlurWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            switch (Common.appSettings.OnClickCloseButton)
            {
                case "Minimization":
                    Visibility = Visibility.Collapsed;
                    break;
                case "Exit":
                    Common.GlobalOCRHotKey.UnRegisterGlobalHotKey(hwnd, CallBack);
                    CloseNotifyIcon();
                    Application.Current.Shutdown();
                    break;
            }
        }

        public void CloseNotifyIcon()
        {
            Instance.NotifyIconContextContent.Visibility = Visibility.Collapsed;
        }

        private void ButtonPush_OnClick(object sender, RoutedEventArgs e)
        {
            NotifyIconContextContent.CloseContextControl();
            this.Dispatcher.Invoke(() => { Visibility = Visibility.Visible; });
        }

        /// <summary>
        /// 切换语言通用事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Language_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                switch (menuItem.Tag)
                {
                    case "zh-cn":
                        Common.appSettings.AppLanguage = "zh-CN";
                        HandyControl.Controls.MessageBox.Show("语言配置已修改！重启软件后生效！", "提示");
                        break;
                    case "en-us":
                        Common.appSettings.AppLanguage = "en-US";
                        HandyControl.Controls.MessageBox.Show("Language configuration has been modified! It will take effect after restarting Enou!", "Hint");
                        break;
                }
            }
        }

        private void BlurWindow_ContentRendered(object sender, EventArgs e)
        {
        }

        private void ComicTransBtn_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
           // this.Visibility = Visibility.Collapsed;
        }

        private void BlurWindow_Loaded(object sender, RoutedEventArgs e)
        {
            HttpClientWrapper.GetKnownWords(0, 100);
        }

        private void ModifyWordSyncLabel(int offset)
        {
            this.labelWordSyncPercent.Content = "单词已同步" + offset;
        }

        public void InvokeModifyWordSyncLabel(int offset)
        {
            this.Dispatcher.Invoke(new Action<int>(ModifyWordSyncLabel), new object[] { offset });
        }
    }
}