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

namespace Enou
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {

        private int originLabelWeight;
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void label_MouseClick(object sender, MouseButtonEventArgs e)
        {
            Tools.OpenBrowser("http://www.enou.fun");
        }

        private void label_MouseEnter(object sender, MouseEventArgs e)
        {
            this.label.FontWeight = FontWeight.FromOpenTypeWeight(originLabelWeight*2);
        }

        private void label_MouseLeave(object sender, MouseEventArgs e)
        {
            this.label.FontWeight = FontWeight.FromOpenTypeWeight(originLabelWeight);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            originLabelWeight = this.label.FontWeight.ToOpenTypeWeight();
            textBoxAccount.Text = Common.appSettings.EnouAccount;
            if(Common.appSettings.RememberPassword)
            {
                passwordBox.Password = Common.appSettings.EnouAccountToken;
            }

            if(Common.appSettings.AutoLogin)
            {
                checkBoxAutoLogin.IsChecked = true;
                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }

            if(Common.appSettings.RememberPassword)
            {
                checkBoxRmbPwd.IsChecked = true;
            }
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            // todo.. seems there are some bugs here..
            String account = this.textBoxAccount.Text;
            String password = this.passwordBox.Password;


            bool loginSucceed = false;
            if(Common.appSettings.RememberPassword)
            {
                loginSucceed = HttpClientWrapper.LoginByToken();

            }
            else
            {
                loginSucceed = HttpClientWrapper.LoginByPwd(account, password);
            }

            if (!loginSucceed)
            {
                labelHint.Content = "账号/密码错误，登录失败";
                Common.appSettings.EnouAccountToken = String.Empty;
                this.passwordBox.Password = String.Empty;
            }


            if (loginSucceed)
            {
                Common.LoadIgnoreWords();
                Common.LoadKnownWords();

                if(Common.appSettings.EnouAccount != account)
                {
                    Common.ClearKnownWord();
                }
                Common.appSettings.EnouAccount = account;
                var mainWindow = new MainWindow();
                var modifyWordWindow = new LearnWordWindow();// do not delete this..
                mainWindow.Show();
                this.Close();
            }

        }

        private void checkBoxAutoLogin_Checked(object sender, RoutedEventArgs e)
        {
            Common.appSettings.AutoLogin = true;
        }

        private void checkBoxAutoLogin_Unchecked(object sender, RoutedEventArgs e)
        {
            Common.appSettings.AutoLogin = false;
        }

        private void checkBoxRmbPwd_Checked(object sender, RoutedEventArgs e)
        {
            Common.appSettings.RememberPassword = false;
        }

        private void checkBoxRmbPwd_Unchecked(object sender, RoutedEventArgs e)
        {
            Common.appSettings.RememberPassword = true;
        }
    }
}
