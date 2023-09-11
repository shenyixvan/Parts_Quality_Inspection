using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Drawing;
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
using WpfApp1;
using YF.Utility;

namespace 零件测量.View
{
    /// <summary>
    /// LoginView.xaml 的交互逻辑
    /// </summary>
    public partial class LoginView : Window
    {
        readonly ManagementSystemView managementSystemView = new ManagementSystemView();
        //readonly PasswordConfirmationView passwordConfirmationView = new PasswordConfirmationView();
        public string _code;
        public static int _userid1;
        public static int _userid2;
        public static string _permission;
        public LoginView()
        {
            InitializeComponent();
            this.MouseMove += new MouseEventHandler(Window_MouseMove);
            _code = GetImage();
        }

        public string GetImage()
        {
            string code = "";
            Bitmap bitmap = CAPTCHAHelper.CreateVerificationCode(out code);
            ImageSource imageSource = ImageFormatConvertHelper.ChangeBitmapToImageSource(bitmap);
            img.Source = imageSource;
            return code;
        }
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed&&BorderTop.IsMouseOver)
            {
                this.DragMove();
            }
        }

        private void BorderTop_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (UpdateButton.IsEnabled == true && LogoffButton.IsEnabled == true)
            {
                UpdateButton.IsEnabled = false;
                LogoffButton.IsEnabled = false;
                UpdateButton.Visibility = Visibility.Hidden;
                LogoffButton.Visibility = Visibility.Hidden;
            }
            Common.DAnimation(0, 0.15, TranslateTransform.XProperty, rucRender);
        }

        private void ManagerButton_Click(object sender, RoutedEventArgs e)
        {
            if (UpdateButton.IsEnabled == false && LogoffButton.IsEnabled == false)
            {
                UpdateButton.IsEnabled = true;
                LogoffButton.IsEnabled = true;
                UpdateButton.Visibility = Visibility.Visible;
                LogoffButton.Visibility = Visibility.Visible;

            }
            else
            {
                UpdateButton.IsEnabled = false;
                LogoffButton.IsEnabled = false;
                UpdateButton.Visibility = Visibility.Hidden;
                LogoffButton.Visibility = Visibility.Hidden;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            //转不在线
            string name = managementSystemView.Admintb.Text;
            string sqlstr_1 = "select count(*) from manager where 用户名='" + name + "'";
            int result = Convert.ToInt16(YMysqlHelper.MysqlHelper.ExecuteScalar(sqlstr_1));
            if (result != 0)
            {
                string sqlstr_2 = "update manager set 在线状态='不在线' where 用户名='" + name + "'";
                YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlstr_2);
            }

            managementSystemView.cycle_1 = 0;
            managementSystemView.cycle_2 = 0;
            managementSystemView.Close();
            this.Close();
            Environment.Exit(0);
            
            

        }

        private void Uid1_TextChanged(object sender, TextChangedEventArgs e)
        {
            ToolTip1.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 225, 0, 0));
            if (ToolTip1.Text == "请输入要查询的用户名" && Uid1.Text != "")
            {
                ToolTip1.Text = "点击查询";
            }
            else if ((ToolTip1.Text == "注册失败" && Uid1.Text != "") ||
                     (ToolTip1.Text == "登录失败" && Uid1.Text != "") ||
                     (ToolTip1.Text == "查询失败" && Uid1.Text != "") ||
                     (ToolTip1.Text == "更改失败" && Uid1.Text != "") ||
                     (ToolTip1.Text == "注销失败" && Uid1.Text != "") ||
                     (ToolTip1.Text == "用户不存在或密码错误" && Uid1.Text != "") ||
                     (ToolTip1.Text == "注册成功" && Uid1.Text != "") ||
                     (ToolTip1.Text == "登录成功" && Uid1.Text != "") ||
                     (ToolTip1.Text == "注销成功" && Uid1.Text != "") ||
                     (ToolTip1.Text == "更改成功" && Uid1.Text != "") ||
                     (ToolTip1.Text == "点击查询" && Uid1.Text != "") ||
                      ToolTip1.Text == "请输入用户名和密码" || ToolTip1.Text == "请输入用户名" || ToolTip1.Text == "请输入密码")
            {
                ToolTip1.Text = "忘记密码?";
            }
        }

        private void Pwd1_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if ((ToolTip1.Text == "用户不存在或密码错误" && Pwd1.Password != "") ||
                (ToolTip1.Text == "注册成功" && Pwd1.Password != "") ||
                (ToolTip1.Text == "登录成功" && Pwd1.Password != "") ||
                (ToolTip1.Text == "注销成功" && Pwd1.Password != "") ||
                (ToolTip1.Text == "更改成功" && Pwd1.Password != "") ||
                (ToolTip1.Text == "注册失败" && Pwd1.Password != "") ||
                (ToolTip1.Text == "登录失败" && Pwd1.Password != "") ||
                (ToolTip1.Text == "注销失败" && Pwd1.Password != "") ||
                (ToolTip1.Text == "更改失败" && Pwd1.Password != "") ||
                (ToolTip1.Text == "点击查询" && Pwd1.Password != "") ||
                 ToolTip1.Text == "请输入用户名和密码" || ToolTip1.Text == "请输入用户名" || ToolTip1.Text == "请输入密码")
            {
                ToolTip1.Text = "忘记密码?";
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            MySqlConnection conn = new MySqlConnection(YMysqlHelper.MysqlHelper.connectionString);
            try
            {
                conn.Open();

                if (Uid1.Text == "" && Pwd1.Password != "")
                {
                    ToolTip1.Text = "请输入用户名";
                }
                else if (Uid1.Text != "" && Pwd1.Password == "")
                {
                    ToolTip1.Text = "请输入密码";
                }
                else if (Uid1.Text == "" && Pwd1.Password == "")
                {
                    ToolTip1.Text = "请输入用户名和密码";
                }
                else if (Uid1.Text != "" && Pwd1.Password != "" && CAPTCHABox.Text == "")
                {
                    ToolTip1.Text = "请输入验证码";
                }
                else
                {
                    if (CAPTCHABox.Text.Trim() == _code)
                    {

                        if (manager.IsChecked == true)
                        {
                            _permission = "manager";
                        }
                        else
                        {
                            _permission = "administrator";
                        }
                        string sql = "select * from " + _permission + " where 用户名=@para1 and 密码=@para2";
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("para1", Uid1.Text);
                        cmd.Parameters.AddWithValue("para2", Pwd1.Password);
                        MySqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            _userid2 = reader.GetInt32("序列号");
                            reader.Close();
                            string sql2 = "update " + _permission + " set 最近登录='" + DateTime.Now + "', 在线状态='在线' where 序列号='" + _userid2 + "'";
                            MySqlCommand cmd2 = new MySqlCommand(sql2, conn);
                            int result = cmd2.ExecuteNonQuery();

                            managementSystemView.Admintb.Text = Uid1.Text;
                            //界面切换
                            if (_permission == "administrator")
                            {
                                managementSystemView.Paramti.Visibility = Visibility.Collapsed;
                                managementSystemView.Switchbtn.Visibility = Visibility.Collapsed;
                                managementSystemView.bar.Visibility = Visibility.Collapsed;
                                managementSystemView.Openbtn.Visibility = Visibility.Collapsed;
                                //managementSystemView.Monitorti.Visibility = Visibility.Collapsed;
                                managementSystemView.Managerti.Margin = new Thickness(129, -1, -144, 0);
                                managementSystemView.Deviceti.Margin = new Thickness(98, -1, -113, 0);
                                managementSystemView.Deviceti.Header = "设备管理";

                                managementSystemView.Managerti.Visibility = Visibility.Visible;
                                managementSystemView.Deviceti.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                managementSystemView.Managerti.Visibility = Visibility.Collapsed;
                                managementSystemView.Deviceti.Visibility = Visibility.Collapsed;

                                managementSystemView.Paramti.Visibility = Visibility.Visible;
                                managementSystemView.Switchbtn.Visibility = Visibility.Visible;
                                managementSystemView.bar.Visibility = Visibility.Visible;
                                managementSystemView.Openbtn.Visibility = Visibility.Visible;
                                //managementSystemView.Monitorti.Visibility = Visibility.Visible;
                                managementSystemView.Deviceti.Header = "设备列表";
                            }


                            Uid1.Text = "";
                            Pwd1.Password = "";
                            CAPTCHABox.Text = "";
                            ToolTip1.Text = "登录成功";
                            ToolTip1.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 225, 0));
                            managementSystemView.Show();
                            _code = GetImage();
                        }
                        else
                        {
                            Uid1.Text = "";
                            Pwd1.Password = "";
                            CAPTCHABox.Text = "";
                            _code = GetImage();
                            ToolTip1.Text = "用户不存在或密码错误";
                        }
                    }
                    else
                    {
                        ToolTip1.Text = "验证码错误";
                        _code = GetImage();
                        CAPTCHABox.Text = "";
                    }


                }

            }
            catch (MySqlException ex)
            {
                Uid1.Text = "";
                Pwd1.Password = "";
                CAPTCHABox.Text = "";
                _code = GetImage();
                MessageBox.Show(ex.Message);
                ToolTip1.Text = "登录失败";
            }
            finally
            {
                conn.Close();
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            Common.DAnimation(-362, 0.15, TranslateTransform.XProperty, rucRender);
            LoginButton.IsEnabled = false;



            if (ruc.yhm.Text!=""&& ruc.mm.Password != "" && ruc.xm.Text != "" && ruc.gh.Text != "" && ruc.xb.Text != "" && ruc.nl.Text != "")
            {
                MySqlConnection conn = new MySqlConnection(YMysqlHelper.MysqlHelper.connectionString);
                try
                {
                    conn.Open();
                    if (manager.IsChecked == true)
                    {
                        _permission = "manager";
                    }
                    else
                    {
                        _permission = "administrator";
                    }
                    string sql = "insert into " + _permission + "(用户名,密码,最近登录,姓名,工号,性别,年龄,在线状态) values('" + ruc.yhm.Text + "','" + ruc.mm.Password + "','" + DateTime.Now + "','"+ ruc.xm.Text + "','"+ ruc.gh.Text + "','"+ ruc.xb.Text + "','"+ ruc.nl.Text + "','不在线')";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    int result = cmd.ExecuteNonQuery();
                    MessageBox.Show("注册成功！");

                }
                catch (MySqlException ex)
                {

                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
               
                Uid1.Text = "";
                Pwd1.Password = "";
                CAPTCHABox.Text = "";
                _code = GetImage();

                Common.DAnimation(0, 0.15, TranslateTransform.XProperty, rucRender);
                LoginButton.IsEnabled = true;

                ruc.yhm.Text = "";
                ruc.mm.Password = "";
                ruc.xm.Text = "";
                ruc.gh.Text = "";
                ruc.xb.Text = "";
                ruc.nl.Text = "";
            }
            else
            {
                //Common.DAnimation(0, 0.15, TranslateTransform.XProperty, rucRender);
                LoginButton.IsEnabled = true;
            }


            //if (Uid1.Text == "" && Pwd1.Password != "")
            //{
            //    ToolTip1.Text = "请输入用户名";
            //}
            //else if (Uid1.Text != "" && Pwd1.Password == "")
            //{
            //    ToolTip1.Text = "请输入密码";
            //}
            //else if (Uid1.Text == "" && Pwd1.Password == "")
            //{
            //    ToolTip1.Text = "请输入用户名和密码";
            //}
            //else if (Uid1.Text != "" && Pwd1.Password != "" && CAPTCHABox.Text == "")
            //{
            //    ToolTip1.Text = "请输入验证码";
            //}
            //else
            //{
            //    if (CAPTCHABox.Text.Trim() == _code)
            //    {
            //        MySqlConnection conn = new MySqlConnection(YMysqlHelper.MysqlHelper.connectionString);
            //        try
            //        {
            //            conn.Open();
            //            if (manager.IsChecked == true)
            //            {
            //                _permission = "manager";
            //            }
            //            else
            //            {
            //                _permission = "administrator";
            //            }
            //            string sql = "insert into " + _permission + "(username,userpassword,logindate) values('" + Uid1.Text + "','" + Pwd1.Password + "','" + DateTime.Now + "')";
            //            MySqlCommand cmd = new MySqlCommand(sql, conn);
            //            int result = cmd.ExecuteNonQuery();
            //            MessageBox.Show("注册成功！");

            //        }
            //        catch (MySqlException ex)
            //        {

            //            MessageBox.Show(ex.Message);
            //        }
            //        finally
            //        {
            //            conn.Close();
            //        }
            //        //passwordConfirmationView.GetUsername = Uid1.Text;
            //        //passwordConfirmationView.GetPassword = Pwd1.Password;
            //        Uid1.Text = "";
            //        Pwd1.Password = "";
            //        CAPTCHABox.Text = "";
            //        //passwordConfirmationView.Show();
            //        _code = GetImage();
            //    }
            //    else
            //    {
            //        ToolTip1.Text = "验证码错误";
            //        _code = GetImage();
            //        CAPTCHABox.Text = "";
            //    }

            //}
        }

        private void ToolTip1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ToolTip1.Text == "忘记密码?")
            {
                ToolTip1.Text = "点击查询";
            }
            else if (ToolTip1.Text == "点击查询")
            {
                if (Uid1.Text == "")
                {
                    ToolTip1.Text = "请输入要查询的用户名";
                }
                else
                {
                    MySqlConnection conn = new MySqlConnection(YMysqlHelper.MysqlHelper.connectionString);
                    try
                    {
                        conn.Open();
                        if (manager.IsChecked == true)
                        {
                            _permission = "manager";
                        }
                        else
                        {
                            _permission = "administrator";
                        }
                        string sql = "select * from " + _permission + " where 用户名=@para;";
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("para", Uid1.Text);
                        MySqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            string password = Convert.ToString(reader.GetString("密码"));
                            MessageBox.Show("该用户的密码为" + password);
                        }
                        else
                        {
                            MessageBox.Show("用户不存在");
                        }


                    }
                    catch (MySqlException ex)
                    {
                        Uid1.Text = "";
                        Pwd1.Password = "";
                        MessageBox.Show(ex.Message);
                        ToolTip1.Text = "查询失败";
                    }
                    finally
                    {
                        conn.Close();
                    }
                }

            }
        }

        private void WindowMinimumButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateButton.IsEnabled = false;
            LogoffButton.IsEnabled = false;
            UpdateButton.Visibility = Visibility.Hidden;
            LogoffButton.Visibility = Visibility.Hidden;
            if (ToolTip1.Text == "请输入新用户名和新密码" && (Uid1.Text != "" || Pwd1.Password != ""))
            {
                if (Uid1.Text == "" && Pwd1.Password != "")
                {
                    ToolTip1.Text = "请输入新用户名";
                }
                else if (Uid1.Text != "" && Pwd1.Password == "")
                {
                    ToolTip1.Text = "请输入新密码";
                }
                else if (Uid1.Text == "" && Pwd1.Password == "")
                {
                    ToolTip1.Text = "请输入新用户名和新密码";
                }
                else
                {
                    MySqlConnection conn = new MySqlConnection(YMysqlHelper.MysqlHelper.connectionString);
                    try
                    {
                        conn.Open();
                        if (manager.IsChecked == true)
                        {
                            _permission = "manager";
                        }
                        else
                        {
                            _permission = "administrator";
                        }
                        string sql2 = "update " + _permission + " set 用户名='" + Uid1.Text + "',密码='" + Pwd1.Password + "',最近登录='" + DateTime.Now + "' where 序列号='" + _userid1 + "'";  //'userid'会出现异常  userid会更改所有数据
                        MySqlCommand cmd2 = new MySqlCommand(sql2, conn);
                        int result = cmd2.ExecuteNonQuery();
                        ToolTip1.Text = "更改成功";
                        Uid1.Text = "";
                        Pwd1.Password = "";

                    }
                    catch (MySqlException ex)
                    {
                        MessageBox.Show(ex.Message);
                        ToolTip1.Text = "更改失败";
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            else   //未输入过原账号
            {

                if (Uid1.Text == "" && Pwd1.Password != "")
                {
                    ToolTip1.Text = "请输入原始用户名";
                }
                else if (Uid1.Text != "" && Pwd1.Password == "")
                {
                    ToolTip1.Text = "请输入原始密码";
                }
                else if (Uid1.Text == "" && Pwd1.Password == "")
                {
                    ToolTip1.Text = "请输入原始用户名和原始密码";
                }
                else
                {
                    MySqlConnection conn = new MySqlConnection(YMysqlHelper.MysqlHelper.connectionString);
                    try
                    {
                        conn.Open();
                        if (manager.IsChecked == true)
                        {
                            _permission = "manager";
                        }
                        else
                        {
                            _permission = "administrator";
                        }
                        string sql = "select * from " + _permission + " where 用户名=@para1 and 密码=@para2";
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("para1", Uid1.Text);
                        cmd.Parameters.AddWithValue("para2", Pwd1.Password);
                        MySqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            _userid1 = reader.GetInt32("userid");
                            ToolTip1.Text = "请输入新用户名和新密码";
                        }
                        else
                        {
                            Uid1.Text = "";
                            Pwd1.Password = "";
                            ToolTip1.Text = "用户不存在或密码错误";
                        }
                    }
                    catch (MySqlException ex)
                    {
                        Uid1.Text = "";
                        Pwd1.Password = "";
                        MessageBox.Show(ex.Message);
                        ToolTip1.Text = "更改失败";

                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        private void LogoffButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateButton.IsEnabled = false;
            LogoffButton.IsEnabled = false;
            UpdateButton.Visibility = Visibility.Hidden;
            LogoffButton.Visibility = Visibility.Hidden;
            int userid = 0;
            MySqlConnection conn = new MySqlConnection(YMysqlHelper.MysqlHelper.connectionString);
            try
            {
                conn.Open();

                if (Uid1.Text == "" && Pwd1.Password != "")
                {
                    ToolTip1.Text = "请输入用户名";
                }
                else if (Uid1.Text != "" && Pwd1.Password == "")
                {
                    ToolTip1.Text = "请输入密码";
                }
                else if (Uid1.Text == "" && Pwd1.Password == "")
                {
                    ToolTip1.Text = "请输入用户名和密码";
                }
                else
                {
                    if (manager.IsChecked == true)
                    {
                        _permission = "manager";
                    }
                    else
                    {
                        _permission = "administrator";
                    }
                    string sql = "select * from " + _permission + " where 用户名=@para1 and 密码=@para2";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("para1", Uid1.Text);
                    cmd.Parameters.AddWithValue("para2", Pwd1.Password);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {

                        userid = reader.GetInt32("userid");

                    }
                    else
                    {
                        Uid1.Text = "";
                        Pwd1.Password = "";
                        ToolTip1.Text = "用户不存在或密码错误";
                    }
                }

            }
            catch (MySqlException ex)
            {
                ToolTip1.Text = "注销失败";
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }


            if (userid != 0)
            {
                MySqlConnection conn1 = new MySqlConnection(YMysqlHelper.MysqlHelper.connectionString);
                try
                {
                    conn1.Open();
                    if (manager.IsChecked == true)
                    {
                        _permission = "manager";
                    }
                    else
                    {
                        _permission = "administrator";
                    }
                    string sql1 = "delete  from " + _permission + " where userid='" + userid + "'";

                    MySqlCommand cmd1 = new MySqlCommand(sql1, conn1);
                    int result = cmd1.ExecuteNonQuery();
                    ToolTip1.Text = "注销成功";
                    userid = 0;
                    Uid1.Text = "";
                    Pwd1.Password = "";
                    CAPTCHABox.Text = "";
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show(ex.Message);
                    ToolTip1.Text = "注销失败";
                    Uid1.Text = "";
                    Pwd1.Password = "";
                }
                finally
                {
                    conn1.Close();
                }
            }
        }


    }
}
