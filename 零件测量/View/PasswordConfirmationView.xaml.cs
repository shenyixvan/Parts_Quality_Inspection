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
using MySql.Data.MySqlClient;
using YF.Utility;
using 零件测量.View;

namespace WpfApp1
{
    
    /// <summary>
    /// PasswordConfirmationView.xaml 的交互逻辑
    /// </summary>
    public partial class PasswordConfirmationView : Window
    {
        public string GetPassword { get; set; }
        public string GetUsername { get; set; }
        public string GetPermission { get; set; }

        LoginView loginView=new LoginView();
        public PasswordConfirmationView()
        {
            InitializeComponent();
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void WindowMinimumButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            MySqlConnection conn = new MySqlConnection(YMysqlHelper.MysqlHelper.connectionString);
            try
            {
                conn.Open();
                if (PasswordConfirmation.Password != "")
                {
                    if (PasswordConfirmation.Password == GetPassword)
                    {
                        if (loginView.manager.IsChecked == true)
                        {
                            GetPermission = "manager";
                        }
                        else
                        {
                            GetPermission = "administrator";
                        }
                        string sql = "insert into "+GetPermission+"(username,userpassword,logindate) values('" + GetUsername + "','" + GetPassword + "','" + DateTime.Now + "')";
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        int result = cmd.ExecuteNonQuery();
                        this.Close();
                        MessageBox.Show("注册成功！");
                    }   
                    else
                    {
                        PasswordConfirmationTip.Text = "密码不一致";
                        PasswordConfirmation.Password = "";
                    }
                }
            }
            catch(MySqlException ex)
            {
                this.Close();
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }

        }

        private void PasswordConfirmation_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if(PasswordConfirmationTip.Text == "密码不一致"&&PasswordConfirmation.Password!="")
            {
                PasswordConfirmationTip.Text = "请再次输入密码";
            }
        }
    }
}
