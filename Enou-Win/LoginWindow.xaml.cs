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
            if(Tools.HasToken)
            {
                passwordBox.Password = Common.appSettings.EnouAccountToken;
            }
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            String account = this.textBoxAccount.Text;
            String password = this.passwordBox.Password;




            bool loginSucceed = false;
            if(Tools.HasToken)
            {
                loginSucceed = HttpClientWrapper.LoginByToken();
            }
            else
            {
                loginSucceed = HttpClientWrapper.LoginByPwd(account, password);
            }
            if(loginSucceed)
            {
                Common.appSettings.EnouAccount = account;
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }

        }
    }
}
