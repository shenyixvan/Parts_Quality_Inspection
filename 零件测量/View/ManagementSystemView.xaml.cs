using Microsoft.Office.Interop.Excel;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using YF.Utility;
using System.Timers;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using MahApps.Metro.Controls;
using WPFMediaKit.DirectShow.Controls;
using Modbus.Device;


namespace 零件测量.View
{
    /// <summary>
    /// ManagementSystemView.xaml 的交互逻辑
    /// </summary>
    public partial class ManagementSystemView : MetroWindow
    {
        Thread thread;


        System.Timers.Timer timer = new System.Timers.Timer(100);
        System.Timers.Timer timer_2 = new System.Timers.Timer(100);
        System.Timers.Timer timer_3 = new System.Timers.Timer(100);
        //System.Timers.Timer timer_4 = new System.Timers.Timer(100);
        //System.Timers.Timer timer_5 = new System.Timers.Timer(1000);

        //System.Timers.Timer timer2 = new System.Timers.Timer(100);
        //System.Timers.Timer timer3 = new System.Timers.Timer(100);


        public int marker = 0;
        int cameraindex = 1;



        ModbusIpMaster modbusIpMaster;
        ushort[] Modbusreturn = new ushort[1]; //18

        Boolean run = false;

        Boolean ifpause = true;         //true:一批没检测完中途暂停 false:一批检测完停止

        public UInt16 cycle_1 = 0, cycle_2 = 0;        //ifpause=true时，存储当前批零件检测时对应的传送带速度

        string ip;
        int port;

        //检测单个零件所需时间
        double v;

        public ManagementSystemView()
        {

            InitializeComponent();
            //Core.Initialize();
            DataContext = this;

            rbdp.IsChecked = true; //前端true会有实例错误

            //调试用
            Common.VelocityMatching(3, 38);
            //调试用

            //db
            Common.ReadEquipment(wp_1, 294, 245, 18);
            Common.ReadEquipment(wp_2, 170, 125, 12);




            //captureElement.VideoCaptureSource = null;
            //cameraComboBox.ItemsSource = MultimediaUtil.VideoInputNames;
            //if (MultimediaUtil.VideoInputNames.Length > 0)
            //{
            //    cameraComboBox.SelectedIndex = cameraindex;


            //    captureElement.VideoCaptureSource = cameraComboBox.Text;
            //}
            //else
            //{
            //    System.Windows.MessageBox.Show("未找到摄像头", "", MessageBoxButton.OK, MessageBoxImage.Information);
            //}
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Switchbtn.IsChecked == true)
            {
                if (System.Windows.MessageBox.Show("窗口已与设备连接成功，如关闭窗口将断开与设备的连接，确定要关闭窗口吗？", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {

                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = true;
                    this.Hide();
                    thread.Abort();
                    timer.Dispose();
                    timer_2.Dispose();
                    timer_3.Dispose();
                    //timer_4.Dispose();
                    //timer_5.Dispose();
                    //timer2.Dispose();

                }
            }
            else
            {
                e.Cancel = true;
                this.Hide();

            }
        }

        private void Closebtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

            //转不在线
            string name = Admintb.Text;
            string sqlstr_1 = "select count(*) from manager where 用户名='" + name + "'";
            int result = Convert.ToInt16(YMysqlHelper.MysqlHelper.ExecuteScalar(sqlstr_1));
            if (result != 0)
            {
                string sqlstr_2 = "update manager set 在线状态='不在线' where 用户名='" + name + "'";
                YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlstr_2);
            }

        }

        private bool isExcuted = false;
        private void Minbtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaxRestorebtn_Click(object sender, RoutedEventArgs e)
        {
            isExcuted = true;
            if (this.WindowState == System.Windows.WindowState.Normal)
            {

                this.WindowState = System.Windows.WindowState.Maximized;
            }
            else
            {

                this.WindowState = System.Windows.WindowState.Normal;
            }
            isExcuted = false;

        }


        private void Switchbtn_Checked(object sender, RoutedEventArgs e)
        {
            //显示屏参数 如未读到禁用timer3
            //timer3.Enabled = true;
            //timer3.Start();
            //timer3.Elapsed += new System.Timers.ElapsedEventHandler(ReadScreen);




            /////////////////
            Switchbtn.ToolTip = "点击断开连接";
            //Openbtn.IsChecked = false;
            Switchbtn.Foreground = Brushes.White;

            //获取通信地址
            ip = addresstb.Text;
            port = Convert.ToInt32(porttb.Text);

            thread = new Thread(PLCConnection);
            thread.Start();
            //thread.IsBackground = true;
        }

        //private void ReadScreen(object source, ElapsedEventArgs e)
        //{
        //    ushort[] ModbusreturnScreen = new ushort[5];
        //    ushort[] ModbusreturnHeight = new ushort[1];
        //    ModbusreturnScreen = modbusIpMaster.ReadHoldingRegisters(0, 11, 5);
        //    ModbusreturnHeight = modbusIpMaster.ReadHoldingRegisters(0, 1, 1);

        //    if (ModbusreturnScreen[4] == 0)             //显示屏未设置参数
        //    {

        //    }
        //    else if (ModbusreturnScreen[4] == 1)           //圆1
        //    {
        //        //取出参数
        //        string sqlStr1 = "UPDATE xinxi SET `直径`='" + ModbusreturnScreen[1].ToString() + "',`孔1`='" + ModbusreturnScreen[0].ToString() + "',`是否有孔`='1',`厚度`='" + ModbusreturnHeight[0] + "',`误差`='0.2',`孔个数`='1'";
        //        YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr1);

        //        //Openbtn.IsEnabled = true;

        //        string sqlStr = "SELECT * FROM xinxi";
        //        System.Data.DataTable dt = YMysqlHelper.MysqlHelper.ExecuteDataTable(sqlStr);
        //        string str = dt.Rows[0][7].ToString();

        //        ushort[] a = new ushort[2];
        //        int b = Convert.ToInt16(str);
        //        ushort[] Modbusreturn = new ushort[11];
        //        ushort c = (ushort)b;
        //        a[0] = 0;
        //        a[1] = c;
        //        modbusIpMaster.WriteMultipleRegisters(1, 0, a);

        //        timer_3.Stop();
        //        timer_3.Enabled = false;
        //        Openbtn.Content = "停止";

        //        timer.Enabled = true;
        //        timer.Start();
        //        timer.Elapsed += new System.Timers.ElapsedEventHandler(finish_1);



        //        //写入数据库

        //    }
        //    else if (ModbusreturnScreen[4] == 2)                //圆0
        //    {
        //        string sqlStr2 = "UPDATE xinxi SET `直径`='" + ModbusreturnScreen[0].ToString() + "',`是否有孔`='0',`厚度`='" + ModbusreturnHeight[0] + "',`误差`='0.2'";
        //        YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr2);

        //        //Openbtn.IsEnabled = true;

        //        string sqlStr = "SELECT * FROM xinxi";
        //        System.Data.DataTable dt = YMysqlHelper.MysqlHelper.ExecuteDataTable(sqlStr);
        //        string str = dt.Rows[0][7].ToString();

        //        ushort[] a = new ushort[2];
        //        int b = Convert.ToInt16(str);
        //        ushort[] Modbusreturn = new ushort[11];
        //        ushort c = (ushort)b;
        //        a[0] = 0;
        //        a[1] = c;
        //        modbusIpMaster.WriteMultipleRegisters(1, 0, a);

        //        timer_3.Stop();
        //        timer_3.Enabled = false;
        //        Openbtn.Content = "停止";

        //        timer.Enabled = true;
        //        timer.Start();
        //        timer.Elapsed += new System.Timers.ElapsedEventHandler(finish_1);

        //        //Openbtn.Content = "停止";

        //    }
        //    else if (ModbusreturnScreen[4] == 3)                  //方1
        //    {
        //        string sqlStr3 = "UPDATE xinxi SET `长`='" + ModbusreturnScreen[2].ToString() + "',`宽`='" + ModbusreturnScreen[3].ToString() + "',`孔1`='" + ModbusreturnScreen[0].ToString() + "',`是否有孔`='1',`厚度`='" + ModbusreturnHeight[0] + "',`误差`='0.2',`孔个数`='1'";
        //        YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr3);

        //        //Openbtn.IsEnabled = true;

        //        string sqlStr = "SELECT * FROM xinxi";
        //        System.Data.DataTable dt = YMysqlHelper.MysqlHelper.ExecuteDataTable(sqlStr);
        //        string str = dt.Rows[0][7].ToString();

        //        ushort[] a = new ushort[2];
        //        int b = Convert.ToInt16(str);
        //        ushort[] Modbusreturn = new ushort[11];
        //        ushort c = (ushort)b;
        //        a[0] = 0;
        //        a[1] = c;
        //        modbusIpMaster.WriteMultipleRegisters(1, 0, a);

        //        timer_3.Stop();
        //        timer_3.Enabled = false;
        //        Openbtn.Content = "停止";

        //        timer.Enabled = true;
        //        timer.Start();
        //        timer.Elapsed += new System.Timers.ElapsedEventHandler(finish_1);

        //        //Openbtn.Content = "停止";

        //    }
        //    else if (ModbusreturnScreen[4] == 4)                    //方0
        //    {
        //        string sqlStr4 = "UPDATE xinxi SET `长`='" + ModbusreturnScreen[2].ToString() + "',`宽`='" + ModbusreturnScreen[3].ToString() + "',`是否有孔`='0',`厚度`='" + ModbusreturnHeight[0] + "',`误差`='0.2'";
        //        YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr4);

        //        //Openbtn.IsEnabled = true;

        //        string sqlStr = "SELECT * FROM xinxi";
        //        System.Data.DataTable dt = YMysqlHelper.MysqlHelper.ExecuteDataTable(sqlStr);
        //        string str = dt.Rows[0][7].ToString();

        //        ushort[] a = new ushort[2];
        //        int b = Convert.ToInt16(str);
        //        ushort[] Modbusreturn = new ushort[11];
        //        ushort c = (ushort)b;
        //        a[0] = 0;
        //        a[1] = c;
        //        modbusIpMaster.WriteMultipleRegisters(1, 0, a);

        //        timer_3.Stop();
        //        timer_3.Enabled = false;
        //        Openbtn.Content = "停止";

        //        timer.Enabled = true;
        //        timer.Start();
        //        timer.Elapsed += new System.Timers.ElapsedEventHandler(finish_1);

        //        //Openbtn.Content = "停止";

        //    }
        //}


        public void PLCConnection()
        {
            
            TcpClient tcpClient = null;
        connectcontinue:
            try
            {
                
                //tcpClient = new TcpClient("127.0.0.1", 502);
                //tcpClient = new TcpClient("192.168.0.60", 502);
                tcpClient = new TcpClient(ip, port);
                //tcpClient = new TcpClient("p490d61839.qicp.vip", 39368); 
            }
            catch (SystemException e)
            {
                System.Windows.MessageBox.Show(e.ToString());
                goto connectcontinue;
            }



            try
            {
                modbusIpMaster = ModbusIpMaster.CreateIp(tcpClient);


                //Modbusreturn = modbusIpMaster.ReadHoldingRegisters(1, 5, 16);

            }
            catch (SystemException e)
            {
                System.Windows.MessageBox.Show(e.ToString());
                tcpClient.Close();
                modbusIpMaster.Dispose();
                goto connectcontinue;
            }




        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Invoke(System.Action action)
        {
            throw new NotImplementedException();
        }

        private void Switchbtn_Unchecked(object sender, RoutedEventArgs e)
        {
            Switchbtn.Foreground = Brushes.Black;
            Openbtn.IsEnabled = false;
            thread.Abort();
            //timer.Dispose();
            //timer_2.Dispose();
            //timer_3.Dispose();
            //timer_4.Dispose();
            //timer_5.Dispose();
            //timer2.Dispose();
        }

        private void Openbtn_Click(object sender, RoutedEventArgs e)
        {

            if (run == false)
            {
                run = true;

                string sqlStr = "SELECT * FROM xinxi";
                System.Data.DataTable dt = YMysqlHelper.MysqlHelper.ExecuteDataTable(sqlStr);

                string str = dt.Rows[0][7].ToString();
                double height = Convert.ToDouble(str);
                height *= 10;
                str = Convert.ToString(height);

                ushort[] a = new ushort[2];
                int b = Convert.ToInt16(str);
                ushort[] Modbusreturn = new ushort[11];
                ushort c = (ushort)b;
                a[0] = 0;
                a[1] = c;
                modbusIpMaster.WriteMultipleRegisters(1, 0, a);

                //和a[0]同步//
                ushort[] a_4 = new ushort[1];
                a_4[0] = 1;
                modbusIpMaster.WriteMultipleRegisters(1, 60, a_4);

                //上升完成置零
                //ushort[] a_3 = new ushort[1];
                //a3[0] = 0;
                //modbusIpMaster.WriteMultipleRegisters(1, 5, a_3);

                //暂停后再次运行时使用暂停前的速度
                ushort[] a_1 = new ushort[1];
                ushort[] a_2 = new ushort[1];
                a_1[0] = cycle_1;
                a_2[0] = cycle_2;
                modbusIpMaster.WriteMultipleRegisters(1, 50, a_1);
                modbusIpMaster.WriteMultipleRegisters(1, 52, a_2);

                timer.Enabled = true;
                timer.Start();
                timer.Elapsed += new System.Timers.ElapsedEventHandler(finish_1);

                Openbtn.Content = "停止";


            }
            else
            {
                run = false;
                Openbtn.Content = "运行";


                if (ifpause)      //中途暂停
                {
                    ushort[] a = new ushort[1];
                    a[0] = 1;
                    modbusIpMaster.WriteMultipleRegisters(1, 2, a);

                    //传送带停止
                    ushort[] a_1 = new ushort[1];
                    ushort[] a_2 = new ushort[1];

                    a_1[0] = 0;
                    modbusIpMaster.WriteMultipleRegisters(1, 50, a_1);
                    a_2[0] = 0;
                    modbusIpMaster.WriteMultipleRegisters(1, 52, a_2);
                }
                else           //一批检测完成   不用
                {

                    ushort[] a_2 = new ushort[1];
                    a_2[0] = 0;
                    modbusIpMaster.WriteMultipleRegisters(1, 2, a_2);


                    ushort[] a = new ushort[1];
                    a[0] = 1;
                    modbusIpMaster.WriteMultipleRegisters(1, 4, a);

                    cycle_1 = 0;
                    cycle_2 = 0;


                    Common.InitializeMeasurementTable("select * from rectanglemeasurement", SDG);
                    Common.InitializeMeasurementTable("select * from roundmeasurement", RDG);
                }





            }


        }




        /// <summary>
        /// 上升完成
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void finish_1(object source, ElapsedEventArgs e)
        {

            //ushort[] Modbusreturn = new ushort[18];
            //Modbusreturn = modbusIpMaster.ReadHoldingRegisters(1, 17, 1);

            Modbusreturn = modbusIpMaster.ReadHoldingRegisters(0, 5, 1);
            if (Modbusreturn[0] == 1)
            {
                //上升完成置零
                ushort[] a = new ushort[1];
                a[0] = 0;
                modbusIpMaster.WriteMultipleRegisters(1, 5, a);
                //this.Dispatcher.Invoke(new System.Action(delegate
                //{

                //}));


                string sqlStr = "UPDATE `状态` SET 判断开始=1";             //重复写1
                YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr);

                timer.Stop();
                timer.Enabled = false;

                //timer.Dispose();
                timer_2.Enabled = true;
                timer_2.Start();
                timer_2.Elapsed += new System.Timers.ElapsedEventHandler(finish_2);
            }





        }
        /// <summary>
        /// 标准件检测完成 发送传送带开始信息
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void finish_2(object source, ElapsedEventArgs e)
        {
            string sqlStr = "SELECT * FROM 状态";
            System.Data.DataTable dt = YMysqlHelper.MysqlHelper.ExecuteDataTable(sqlStr);
            string str = dt.Rows[0][0].ToString();
            if (str == "0")
            {

                //传送带
                ushort[] a = new ushort[1];
                a[0] = 1;
                modbusIpMaster.WriteMultipleRegisters(1, 0, a);

                timer_2.Stop();
                timer_2.Enabled = false;
                //timer.Dispose();
                timer_3.Enabled = true;
                timer_3.Start();
                timer_3.Elapsed += new System.Timers.ElapsedEventHandler(finish_3);
            }
        }
        /// <summary>
        /// 到达检测位置 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        //private void finish_3(object source, ElapsedEventArgs e)
        //{
        //    //ushort[] Modbusreturn = new ushort[18];
        //    //Modbusreturn = modbusIpMaster.ReadHoldingRegisters(1, 17, 1);


        //    Modbusreturn = modbusIpMaster.ReadHoldingRegisters(0, 6, 1);
        //    if (Modbusreturn[0]==1)                                            //1
        //    {


        //        ushort[] a = new ushort[1];
        //        a[0] = 0;
        //        modbusIpMaster.WriteMultipleRegisters(1, 6, a);





        //        string sqlStr = "UPDATE `状态` SET 判断='1'";
        //        YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr);


        //        timer_3.Stop();
        //        timer_3.Enabled = false;
        //        //timer.Dispose();
        //        timer_4.Enabled = true;
        //        timer_4.Start();
        //        timer_4.Elapsed += new System.Timers.ElapsedEventHandler(finish_4);


        //        timer2.Enabled = true;
        //        timer2.Start();
        //        timer2.Elapsed += new System.Timers.ElapsedEventHandler(finish_5);

        //    }

        //}
        /// <summary>
        /// 单个零件检测完毕
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        //private void finish_4(object source, ElapsedEventArgs e)
        //{
        //    //ushort[] Modbusreturn = new ushort[18];
        //    //Modbusreturn = modbusIpMaster.ReadHoldingRegisters(1, 49, 1);

        //    string sqlStr = "SELECT * FROM 状态";
        //    System.Data.DataTable dt1 = YMysqlHelper.MysqlHelper.ExecuteDataTable(sqlStr);
        //    string str1 = dt1.Rows[0][1].ToString();
        //    if (str1 == "0")
        //    {

        //        ushort[] a = new ushort[1];
        //        a[0] = 1;
        //        modbusIpMaster.WriteMultipleRegisters(1, 2, a);


        //        //string sql = "UPDATE `状态` SET `判断`='2'";
        //        //YMysqlHelper.MysqlHelper.ExecuteNonQuery(sql);

        //        timer_4.Stop();
        //        timer_4.Enabled = false;
        //        //timer.Dispose();
        //        timer_5.Enabled = true;
        //        timer_5.Start();
        //        timer_5.Elapsed += new System.Timers.ElapsedEventHandler(finish_3);

        //    }





        //}

        private void finish_3(object source, ElapsedEventArgs e)
        {
            //string sqlStr_1 = "UPDATE `状态` SET `结果`='0'";
            ////YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr_1);

            //string sqlStr_2 = "SELECT * FROM 状态";
            //System.Data.DataTable dt = YMysqlHelper.MysqlHelper.ExecuteDataTable(sqlStr_2);
            //string str = dt.Rows[0][2].ToString();

            //Modbusreturn = modbusIpMaster.ReadHoldingRegisters(0, 7, 1);         //判断总体检测完成

            //if (str == "1" && Modbusreturn[0] == 0)               //2         检测到不合格，总体检测未完成
            //{

            //    YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr_1);

            //    ushort[] a = new ushort[1];
            //    a[0] = 1;
            //    modbusIpMaster.WriteMultipleRegisters(1, 3, a);

            //}
            //else if (str == "1"&&Modbusreturn[0] == 1)            //2              检测到不合格，总体检测完成
            //{
            //    YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr_1);

            //    ushort[] a = new ushort[1];
            //    a[0] = 1;
            //    modbusIpMaster.WriteMultipleRegisters(1, 3, a);

            //    //报表
            //    ushort[] ModbusreturnResh = new ushort[1];
            //    ModbusreturnResh = modbusIpMaster.ReadHoldingRegisters(0, 8, 1);
            //    ushort[] ModbusreturnRess = new ushort[1];
            //    ModbusreturnRess = modbusIpMaster.ReadHoldingRegisters(0, 9, 1);
            //    ushort[] ModbusreturnResp = new ushort[1];
            //    ModbusreturnResp = modbusIpMaster.ReadHoldingRegisters(0, 10, 1);


            //    int total = ModbusreturnResh[0] + ModbusreturnRess[0] + ModbusreturnResp[0];
            //    string rate = (ModbusreturnResp[0] * 100 / total).ToString() + '%';

            //    string sqlStr_3 = "SELECT * FROM xinxi";
            //    System.Data.DataTable dt_2 = YMysqlHelper.MysqlHelper.ExecuteDataTable(sqlStr_3);

            //    string sqlStr_4 = "INSERT INTO measurementdata (`测量时间`,`合格个数`,`高度不合格数量`,`尺寸不合格数量`,`测量个数`,`合格率`,`直径`,`形状`,`长`,`宽`,`是否打孔`,`内径`,`内径2`,`孔个数`,`厚度`,`允许误差`)" +
            //        " VALUES ('" + System.DateTime.Now + "','" + ModbusreturnResp[0].ToString() + "','" + ModbusreturnResh[0].ToString() + "','" + ModbusreturnRess[0].ToString() + "','" + total.ToString() + "'," +
            //        "'" + rate + "','" + dt_2.Rows[0][0].ToString() + "','" + dt_2.Rows[0][1].ToString() + "','" + dt_2.Rows[0][2].ToString() + "','" + dt_2.Rows[0][3].ToString() + "','" + dt_2.Rows[0][6].ToString() + "','" + dt_2.Rows[0][4].ToString() + "','" + dt_2.Rows[0][5].ToString() + "','" + dt_2.Rows[0][9].ToString() + "','" + dt_2.Rows[0][7].ToString() + "','" + dt_2.Rows[0][8].ToString() + "')";

            //    YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr_4);


            //    timer_5.Stop();
            //    timer_5.Enabled = false;
            //    //timer.Dispose();

            //    timer2.Stop();
            //    timer2.Enabled = false;
            //    //timer2.Dispose();

            //    ifpause = false;
            //    cycle_1 = 0;
            //    cycle_2 = 0;

            //}
            //else if (str == "0"&& Modbusreturn[0] == 1)        //2           未检测到不合格，总体检测完成
            //{
            //    //报表
            //    ushort[] ModbusreturnResh = new ushort[1];
            //    ModbusreturnResh = modbusIpMaster.ReadHoldingRegisters(0, 8, 1);
            //    ushort[] ModbusreturnRess = new ushort[1];
            //    ModbusreturnRess = modbusIpMaster.ReadHoldingRegisters(0, 9, 1);
            //    ushort[] ModbusreturnResp = new ushort[1];
            //    ModbusreturnResp = modbusIpMaster.ReadHoldingRegisters(0, 10, 1);


            //    int total = ModbusreturnResh[0] + ModbusreturnRess[0] + ModbusreturnResp[0];

            //    string rate = (ModbusreturnResp[0] * 100 / total).ToString() + '%';

            //    string sqlStr_3 = "SELECT * FROM xinxi";
            //    System.Data.DataTable dt_2 = YMysqlHelper.MysqlHelper.ExecuteDataTable(sqlStr_3);

            //    string sqlStr_4 = "INSERT INTO measurementdata (测量时间,合格个数,高度不合格数量,尺寸不合格数量,测量个数,合格率,直径,形状,长,宽,是否打孔,内径,内径2,孔个数,厚度,允许误差)" +
            //        " VALUES ('" + System.DateTime.Now + "','" + ModbusreturnResp[0].ToString() + "','" + ModbusreturnResh[0].ToString() + "','" + ModbusreturnRess[0].ToString() + "','" + total.ToString() + "'," +
            //        "'" + rate + "','" + dt_2.Rows[0][0].ToString() + "','" + dt_2.Rows[0][1].ToString() + "','" + dt_2.Rows[0][2].ToString() + "','" + dt_2.Rows[0][3].ToString() + "','" + dt_2.Rows[0][6].ToString() + "','" + dt_2.Rows[0][4].ToString() + "','" + dt_2.Rows[0][5].ToString() + "','" + dt_2.Rows[0][9].ToString() + "','" + dt_2.Rows[0][7].ToString() + "','" + dt_2.Rows[0][8].ToString() + "')";

            //    YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr_4);


            //    timer_5.Stop(); 
            //    timer_5.Enabled = false;
            //    //timer.Dispose();

            //    timer2.Stop();
            //    timer2.Enabled = false;
            //    //timer2.Dispose();

            //    ifpause = false;
            //}




            Modbusreturn = modbusIpMaster.ReadHoldingRegisters(0, 7, 1);         //判断总体检测完成
            if (Modbusreturn[0] == 1)
            {

                ushort[] a = new ushort[1];
                a[0] = 0;
                modbusIpMaster.WriteMultipleRegisters(0, 7, a);
                this.Dispatcher.Invoke(new System.Action(delegate
                {
                    Openbtn.Content = "运行";
                    Openbtn.IsEnabled = false;


                }));

                //报表
                ushort[] ModbusreturnResh = new ushort[1];
                ModbusreturnResh = modbusIpMaster.ReadHoldingRegisters(0, 8, 1);
                ushort[] ModbusreturnRess = new ushort[1];
                ModbusreturnRess = modbusIpMaster.ReadHoldingRegisters(0, 9, 1);
                ushort[] ModbusreturnResp = new ushort[1];
                ModbusreturnResp = modbusIpMaster.ReadHoldingRegisters(0, 10, 1);

                ushort[] ModbusreturnBroken = new ushort[1];
                ModbusreturnBroken = modbusIpMaster.ReadHoldingRegisters(0, 20, 1);

                ushort[] ModbusreturnTotal = new ushort[1];
                ModbusreturnTotal = modbusIpMaster.ReadHoldingRegisters(0, 21, 1);


                //int total = ModbusreturnResh[0] + ModbusreturnRess[0] + ModbusreturnResp[0] + ModbusreturnBroken[0];
                int total = ModbusreturnTotal[0];

                if (total != 0)
                {
                    string rate = (ModbusreturnResp[0] * 100 / total).ToString() + '%';

                    string sqlStr_3 = "SELECT * FROM xinxi";
                    System.Data.DataTable dt_2 = YMysqlHelper.MysqlHelper.ExecuteDataTable(sqlStr_3);

                    string sqlStr_4 = "INSERT INTO measurementdata (`测量时间`,`合格个数`,`高度不合格数量`,`尺寸不合格数量`,`破损数量`,`测量个数`,`合格率`,`直径`,`形状`,`长`,`宽`,`是否打孔`,`内径`,`内径2`,`孔个数`,`厚度`,`允许误差`,`PCD间距`,`PD间距`)" +
                        " VALUES ('" + System.DateTime.Now + "','" + ModbusreturnResp[0].ToString() + "','" + ModbusreturnResh[0].ToString() + "','" + ModbusreturnRess[0].ToString() + "','" + ModbusreturnBroken[0].ToString() + "','" + total.ToString() + "'," +
                        "'" + rate + "','" + dt_2.Rows[0][0].ToString() + "','" + dt_2.Rows[0][1].ToString() + "','" + dt_2.Rows[0][2].ToString() + "','" + dt_2.Rows[0][3].ToString() + "','" + dt_2.Rows[0][6].ToString() + "','" + dt_2.Rows[0][4].ToString() + "','" + dt_2.Rows[0][5].ToString() + "','" + dt_2.Rows[0][9].ToString() + "','" + dt_2.Rows[0][7].ToString() + "','" + dt_2.Rows[0][8].ToString() + "','" + dt_2.Rows[0][13].ToString() + "','" + dt_2.Rows[0][14].ToString() + "')";

                    YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr_4);
                }

                


                

                ifpause = false;
                cycle_1 = 0;
                cycle_2 = 0;




                run = false;

                


                timer_3.Stop();
                timer_3.Enabled = false;
                //timer_3.Dispose();


            }













        }





        private void SDG_Loaded(object sender, RoutedEventArgs e)
        {
            //List<string> sqlstrlist = new List<string>();
            //string sqlstr_1 = "update rectanglemeasurement set 测量个数= rectanglemeasurement.合格个数+rectanglemeasurement.不合格个数 where 合格个数 is not null";
            //string sqlstr_2 = "update rectanglemeasurement set 测量时间='" + DateTime.Now + "' where 测量时间 is null";
            //string sqlstr_3 = "select * from rectanglemeasurement";
            //sqlstrlist.Add(sqlstr_1);
            //sqlstrlist.Add(sqlstr_2);
            //Common.GetTable(sqlstrlist, sqlstr_3, SDG);

            //db
            Common.InitializeMeasurementTable("select * from rectanglemeasurement", SDG);

        }

        private void RDG_Loaded(object sender, RoutedEventArgs e)
        {
            //List<string> sqlstrlist = new List<string>();
            //string sqlstr_1 = "update roundmeasurement set 测量个数= roundmeasurement.合格个数+roundmeasurement.不合格个数 where 合格个数 is not null";
            //string sqlstr_2 = "update roundmeasurement set 测量时间='" + DateTime.Now + "' where 测量时间 is null";
            //string sqlstr_3 = "select * from roundmeasurement";
            //sqlstrlist.Add(sqlstr_1);
            //sqlstrlist.Add(sqlstr_2);
            //Common.GetTable(sqlstrlist, sqlstr_3, RDG);

            //db
            Common.InitializeMeasurementTable("select * from roundmeasurement", RDG);

        }

        private void SavePathbtn_1_Click(object sender, RoutedEventArgs e)
        {
            SavePathtb_1.Text = Common.PathPicker("矩形零件检测结果");

        }

        private void SavePathbtn_2_Click(object sender, RoutedEventArgs e)
        {
            SavePathtb_2.Text = Common.PathPicker("圆形零件检测结果");
        }

        private void SExportbtn_Checked(object sender, RoutedEventArgs e)
        {
            if (SavePathtb_1.Text == "")
            {
                SExportbtn.IsChecked = false;
                System.Windows.MessageBox.Show("请选择路径", "", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            }
            else
            {
                System.Data.DataTable dt = ((DataView)SDG.ItemsSource).Table;
                try
                {
                    Common.ExportSExcel(SavePathtb_1.Text, dt);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
                finally
                {
                    SavePathtb_1.Text = "";
                    SExportbtn.IsChecked = false;
                }
            }

        }

        private void RExportbtn_Checked(object sender, RoutedEventArgs e)
        {
            if (SavePathtb_2.Text == "")
            {
                RExportbtn.IsChecked = false;
                System.Windows.MessageBox.Show("请选择路径", "", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            }
            else
            {
                System.Data.DataTable dt = ((DataView)RDG.ItemsSource).Table;
                try
                {
                    Common.ExportRExcel(SavePathtb_2.Text, dt);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
                finally
                {
                    SavePathtb_2.Text = "";
                    RExportbtn.IsChecked = false;
                }
            }

        }

        private void TB1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TB1.Text == "0")
            {

                System.Windows.MessageBox.Show("已选择打孔，值不能为0。", "", MessageBoxButton.OK, MessageBoxImage.Error);
                TB1.Text = "";
            }

        }

        private void TB5_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TB5.Text == "0")
            {

                System.Windows.MessageBox.Show("已选择打孔，值不能为0。", "", MessageBoxButton.OK, MessageBoxImage.Error);
                TB5.Text = "";
            }
        }

        private void Btn0_Click(object sender, RoutedEventArgs e)
        {
            if (CB2.IsChecked == false)
            {
                if (TB0.Text != "" && TB7.Text != "" && TB9.Text != "")
                {
                    //cycle
                    double thickness = Convert.ToUInt16(TB7.Text), d = Convert.ToDouble(TB0.Text);

                    //暂存速度，暂停后再次运行也使用此速度
                    cycle_1 = Convert.ToUInt16(Common.VelocityMatching(thickness, d)[0]);
                    cycle_2 = Convert.ToUInt16(Common.VelocityMatching(thickness, d)[1]);

                    CB0.IsEnabled = true;
                }
                else
                {
                    System.Windows.MessageBox.Show("请输入完整参数。", "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            else
            {
                if (TB0.Text != "" && TB1.Text != "" && TB2.Text != "" && TB7.Text != "" && TB9.Text != "")
                {
                    //cycle
                    double thickness = Convert.ToUInt16(TB7.Text), d = Convert.ToDouble(TB0.Text);


                    cycle_1 = Convert.ToUInt16(Common.VelocityMatching(thickness, d)[0]);
                    cycle_2 = Convert.ToUInt16(Common.VelocityMatching(thickness, d)[1]);

                    CB0.IsEnabled = true;
                }
                else
                {
                    System.Windows.MessageBox.Show("请输入完整参数。", "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }

        }

        private void Btn1_Click(object sender, RoutedEventArgs e)
        {
            if (CB3.IsChecked == false)
            {
                if (TB3.Text != "" && TB4.Text != "" && TB8.Text != "" && TB12.Text != "")
                {
                    //cycle
                    double thickness = Convert.ToUInt16(TB8.Text), d = Convert.ToDouble(TB3.Text);

                    cycle_1 = Convert.ToUInt16(Common.VelocityMatching(thickness, d)[0]);
                    cycle_2 = Convert.ToUInt16(Common.VelocityMatching(thickness, d)[1]);

                    CB1.IsEnabled = true;

                }
                else
                {
                    System.Windows.MessageBox.Show("请输入完整参数。", "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            else
            {
                if (TB3.Text != "" && TB4.Text != "" && TB5.Text != "" && TB6.Text != "" && TB8.Text != "" && TB12.Text != "")
                {
                    //cycle
                    double thickness = Convert.ToUInt16(TB8.Text), d = Convert.ToDouble(TB3.Text);

                    cycle_1 = Convert.ToUInt16(Common.VelocityMatching(thickness, d)[0]);
                    cycle_2 = Convert.ToUInt16(Common.VelocityMatching(thickness, d)[1]);

                    CB1.IsEnabled = true;

                }
                else
                {
                    System.Windows.MessageBox.Show("请输入完整参数。", "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        private void CB0_Checked(object sender, RoutedEventArgs e)
        {
            ifpause = true;
            if (CB2.IsChecked == false)
            {
                //string sql = "insert into roundparam(上传时间,外径,打孔) values('" + DateTime.Now + "','" + TB0.Text + "',0)";

                //string sql = "insert into param(上传时间,长,宽,是否打孔) values('" + DateTime.Now + "','" + TB0.Text + "','" + TB0.Text + "',0)";
                double thickness = Convert.ToUInt16(TB7.Text), d = Convert.ToDouble(TB0.Text);
                string height = (Convert.ToInt64(TB7.Text)).ToString();
                string sqlStr = "UPDATE xinxi SET `直径`='" + TB0.Text + "',`厚度`='" + height + "',`误差`='" + TB9.Text + "',`形状`='0',`间距起始位置`='" + Common.VelocityMatching(thickness, d)[2] + "',`间距结束位置`='" + Common.VelocityMatching(thickness, d)[3] + "',`中心点`='" + Common.VelocityMatching(thickness, d)[4] + "'";
                YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr);
                string sqlStr_2 = "UPDATE `状态` SET `比值`='" + Common.VelocityMatching(thickness, d)[5] + "';";
                YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr_2);
                TB0.Text = "";
                TB7.Text = "";
                TB9.Text = "";
                TB2.Text = "";
                TB15.Text = "";
                TB11.Text = "";
                if (Switchbtn.IsChecked == true)
                {
                    Openbtn.IsEnabled = true;

                }


            }
            else
            {
                //string sql = "insert into roundparam(上传时间,外径,打孔,内径,孔个数) values('" + DateTime.Now + "','" + TB0.Text + "',1,'" + TB2.Text + "','" + TB1.Text + "')";
                //string sql = "insert into param(上传时间,长,宽,是否打孔,内径,孔个数) values('" + DateTime.Now + "','" + TB0.Text + "','" + TB0.Text + "',1,'" + TB2.Text + "','" + TB1.Text + "')";

                double thickness = Convert.ToUInt16(TB7.Text), d = Convert.ToDouble(TB0.Text);
                string height = (Convert.ToInt64(TB7.Text)).ToString();
                string sqlStr = "UPDATE xinxi SET `直径`='" + TB0.Text + "',`孔1`='" + TB2.Text + "',`是否有孔`='1',`厚度`='" + height + "',`误差`='" + TB9.Text + "',`孔个数`='" + TB1.Text + "',`形状`='0',`间距起始位置`='" + Common.VelocityMatching(thickness, d)[2] + "',`间距结束位置`='" + Common.VelocityMatching(thickness, d)[3] + "',`中心点`='" + Common.VelocityMatching(thickness, d)[4] + "'";
                YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr);
                string sqlStr_2 = "UPDATE `状态` SET `比值`='" + Common.VelocityMatching(thickness, d)[5] + "';";
                YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr_2);
                TB0.Text = "";
                TB1.Text = "";
                TB2.Text = "";
                TB7.Text = "";
                TB9.Text = "";
                TB15.Text = "";
                TB11.Text = "";
                TB1.Visibility = Visibility.Collapsed;
                TB2.Visibility = Visibility.Collapsed;
                TB_0.Visibility = Visibility.Collapsed;
                TB_1.Visibility = Visibility.Collapsed;
                TB_4.Visibility = Visibility.Collapsed;
                TB15.Visibility = Visibility.Collapsed;
                CB2.IsChecked = false;

                if (Switchbtn.IsChecked == true)
                {
                    Openbtn.IsEnabled = true;
                }

            }


        }

        private void CB1_Checked(object sender, RoutedEventArgs e)
        {
            ifpause = true;
            if (CB3.IsChecked == false)
            {
                //string sql = "insert into rectangleparam(上传时间,长,宽,打孔) values('" + DateTime.Now + "','" + TB3.Text + "','" + TB4.Text + "',0)";
                //string sql = "insert into param(上传时间,长,宽,是否打孔) values('" + DateTime.Now + "','" + TB3.Text + "','" + TB4.Text + "',0)";
                double thickness = Convert.ToUInt16(TB8.Text), d = Convert.ToDouble(TB3.Text);
                string height = (Convert.ToInt64(TB8.Text)).ToString();
                string sqlStr = "UPDATE xinxi SET `形状`='1', `长`='" + TB3.Text + "',`宽`='" + TB4.Text + "',`厚度`='" + height + "',`误差`='" + TB12.Text + "',`间距起始位置`='" + Common.VelocityMatching(thickness, d)[2] + "',`间距结束位置`='" + Common.VelocityMatching(thickness, d)[3] + "',`中心点`='" + Common.VelocityMatching(thickness, d)[4] + "'";
                YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr);
                string sqlStr_2 = "UPDATE `状态` SET `比值`='" + Common.VelocityMatching(thickness, d)[5] + "';";
                YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr_2);
                TB3.Text = "";
                TB4.Text = "";
                TB8.Text = "";
                TB12.Text = "";
                TB17.Text = "";
                TB13.Text = "";
                TB14.Text = "";
                if (Switchbtn.IsChecked == true)
                {
                    Openbtn.IsEnabled = true;
                }
            }
            else
            {
                //string sql = "insert into rectangleparam(上传时间,长,宽,打孔,内径,孔个数) values('" + DateTime.Now + "','" + TB3.Text + "','" + TB4.Text + "',1,'" + TB6.Text + "','" + TB5.Text + "')";
                //string sql = "insert into param(上传时间,长,宽,是否打孔,内径,孔个数) values('" + DateTime.Now + "','" + TB3.Text + "','" + TB4.Text + "',1,'" + TB6.Text + "','" + TB5.Text + "')";
                double thickness = Convert.ToUInt16(TB8.Text), d = Convert.ToDouble(TB3.Text);
                string height = (Convert.ToInt64(TB8.Text)+2).ToString();
                string sqlStr = "UPDATE xinxi SET `形状`='1',`长`='" + TB3.Text + "',`宽`='" + TB4.Text + "',`孔1`='" + TB6.Text + "',`是否有孔`='1',`厚度`='" + height + "',`误差`='" + TB12.Text + "',`孔个数`='" + TB5.Text + "',`间距起始位置`='" + Common.VelocityMatching(thickness, d)[2] + "',`间距结束位置`='" + Common.VelocityMatching(thickness, d)[3] + "',`中心点`='" + Common.VelocityMatching(thickness, d)[4] + "'";
                YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr);
                string sqlStr_2 = "UPDATE `状态` SET `比值`='" + Common.VelocityMatching(thickness, d)[5] + "';";
                YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr_2);
                TB3.Text = "";
                TB4.Text = "";
                TB5.Text = "";
                TB6.Text = "";
                TB8.Text = "";
                TB12.Text = "";
                TB17.Text = "";
                TB12.Text = "";
                TB13.Text = "";
                TB14.Text = "";
                TB5.Visibility = Visibility.Collapsed;
                TB_2.Visibility = Visibility.Collapsed;
                TB6.Visibility = Visibility.Collapsed;
                TB_3.Visibility = Visibility.Collapsed;
                TB_6.Visibility = Visibility.Collapsed;
                TB_6.Visibility = Visibility.Collapsed;
                TB17.Visibility = Visibility.Collapsed;
                CB3.IsChecked = false;

                if (Switchbtn.IsChecked == true)
                {
                    Openbtn.IsEnabled = true;
                }

            }


        }

        private void CB2_Checked(object sender, RoutedEventArgs e)
        {
            TB_0.Visibility = Visibility.Visible;
            TB1.Visibility = Visibility.Visible;
            TB_1.Visibility = Visibility.Visible;
            TB2.Visibility = Visibility.Visible;
            TB_4.Visibility = Visibility.Visible;
            TB15.Visibility = Visibility.Visible;

            shape1.Visibility = Visibility.Collapsed;
            shape2.Visibility = Visibility.Collapsed;
            shape3.Visibility = Visibility.Collapsed;
            shape4.Visibility = Visibility.Visible;

            gbf.Opacity = 0.1;
            Btn1.Opacity = 0.1;
            gby.Opacity = 1;
            Btn0.Opacity = 1;
        }

        private void CB3_Checked(object sender, RoutedEventArgs e)
        {
            TB_2.Visibility = Visibility.Visible;
            TB5.Visibility = Visibility.Visible;
            TB_3.Visibility = Visibility.Visible;
            TB6.Visibility = Visibility.Visible;
            TB_6.Visibility = Visibility.Visible;
            TB12.Visibility = Visibility.Visible;
            TB_6.Visibility = Visibility.Visible;
            TB17.Visibility = Visibility.Visible;

            shape1.Visibility = Visibility.Collapsed;
            shape2.Visibility = Visibility.Visible;
            shape3.Visibility = Visibility.Collapsed;
            shape4.Visibility = Visibility.Collapsed;

            gbf.Opacity = 1;
            Btn1.Opacity = 1;
            gby.Opacity = 0.1;
            Btn0.Opacity = 0.1;
        }

        private void CB2_Unchecked(object sender, RoutedEventArgs e)
        {
            TB_0.Visibility = Visibility.Collapsed;
            TB1.Visibility = Visibility.Collapsed;
            TB_1.Visibility = Visibility.Collapsed;
            TB2.Visibility = Visibility.Collapsed;
            TB_4.Visibility = Visibility.Collapsed;
            TB15.Visibility = Visibility.Collapsed;

            shape1.Visibility = Visibility.Collapsed;
            shape2.Visibility = Visibility.Collapsed;
            shape3.Visibility = Visibility.Visible;
            shape4.Visibility = Visibility.Collapsed;

        }

        private void CB3_Unchecked(object sender, RoutedEventArgs e)
        {
            TB_2.Visibility = Visibility.Collapsed;
            TB5.Visibility = Visibility.Collapsed;
            TB_3.Visibility = Visibility.Collapsed;
            TB6.Visibility = Visibility.Collapsed;
            TB_6.Visibility = Visibility.Collapsed;
            TB_6.Visibility = Visibility.Collapsed;
            TB17.Visibility = Visibility.Collapsed;

            shape1.Visibility = Visibility.Visible;
            shape2.Visibility = Visibility.Collapsed;
            shape3.Visibility = Visibility.Collapsed;
            shape4.Visibility = Visibility.Collapsed;


        }

        private void CB0_Unchecked(object sender, RoutedEventArgs e)
        {
            CB0.IsEnabled = false;
        }

        private void CB1_Unchecked(object sender, RoutedEventArgs e)
        {
            CB1.IsEnabled = false;
        }

        //private void BookMark_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        //{
        //    if (marker != 1)
        //    {
        //        Common.DAnimation(10, 0.15, TranslateTransform.YProperty, bmRender);
        //        Common.DAnimation(10, 0.15, TranslateTransform.YProperty, tbRender);

        //    }

        //}

        //private void BookMark_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        //{
        //    if (marker != 1)
        //    {
        //        Common.DAnimation(0, 0.15, TranslateTransform.YProperty, bmRender);
        //        Common.DAnimation(0, 0.15, TranslateTransform.YProperty, tbRender);
        //    }

        //}

        //private void BookMark_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    marker = 1;

        //    if (tbRender.Y == 80)
        //    {
        //        Common.DAnimation(0, 0.15, TranslateTransform.YProperty, bmRender);
        //        Common.DAnimation(0, 0.15, TranslateTransform.YProperty, tbRender);
        //        marker = 0;
        //    }
        //    else
        //    {
        //        Common.DAnimation(80, 0.15, TranslateTransform.YProperty, bmRender);
        //        Common.DAnimation(80, 0.15, TranslateTransform.YProperty, tbRender);
        //    }

        //}

        private void dp_1_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

            if (dp_1.Text != "" && dp_2.Text != "")
            {
                DateTime date1 = Convert.ToDateTime(dp_1.Text);
                DateTime date2 = Convert.ToDateTime(dp_2.Text).AddDays(1);
                Common.Filter("select * from rectanglemeasurement where 测量时间>='" + date1 + "' and 测量时间<='" + date2 + "'", SDG);
            }
            else if (dp_1.Text != "" && dp_2.Text == "")
            {
                DateTime date1 = Convert.ToDateTime(dp_1.Text);
                Common.Filter("select * from rectanglemeasurement where 测量时间>='" + date1 + "'", SDG);


            }
            else if (dp_1.Text == "" && dp_2.Text != "")
            {
                DateTime date2 = Convert.ToDateTime(dp_2.Text).AddDays(1);
                Common.Filter("select * from rectanglemeasurement where 测量时间<='" + date2 + "'", SDG);

            }
            else
            {

                Common.Filter("select * from rectanglemeasurement", SDG);
            }

        }

        private void dp_2_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dp_1.Text != "" && dp_2.Text != "")
            {
                DateTime date1 = Convert.ToDateTime(dp_1.Text);
                DateTime date2 = Convert.ToDateTime(dp_2.Text).AddDays(1);
                Common.Filter("select * from rectanglemeasurement where 测量时间>='" + date1 + "' and 测量时间<='" + date2 + "'", SDG);
            }
            else if (dp_1.Text != "" && dp_2.Text == "")
            {
                DateTime date1 = Convert.ToDateTime(dp_1.Text);
                Common.Filter("select * from rectanglemeasurement where 测量时间>='" + date1 + "'", SDG);


            }
            else if (dp_1.Text == "" && dp_2.Text != "")
            {
                DateTime date2 = Convert.ToDateTime(dp_2.Text).AddDays(1);
                Common.Filter("select * from rectanglemeasurement where 测量时间<='" + date2 + "'", SDG);

            }
            else
            {

                Common.Filter("select * from rectanglemeasurement", SDG);
            }
        }

        private void dp_3_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

            if (dp_3.Text != "" && dp_4.Text != "")
            {
                DateTime date1 = Convert.ToDateTime(dp_3.Text);
                DateTime date2 = Convert.ToDateTime(dp_4.Text).AddDays(1);
                Common.Filter("select * from roundmeasurement where 测量时间>='" + date1 + "' and 测量时间<='" + date2 + "'", RDG);
            }
            else if (dp_3.Text != "" && dp_4.Text == "")
            {
                DateTime date1 = Convert.ToDateTime(dp_3.Text);
                Common.Filter("select * from roundmeasurement where 测量时间>='" + date1 + "'", RDG);


            }
            else if (dp_3.Text == "" && dp_4.Text != "")
            {
                DateTime date2 = Convert.ToDateTime(dp_4.Text).AddDays(1);
                Common.Filter("select * from roundmeasurement where 测量时间<='" + date2 + "'", RDG);

            }
            else
            {
                Common.Filter("select * from roundmeasurement", RDG);
            }
        }

        private void dp_4_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dp_3.Text != "" && dp_4.Text != "")
            {
                DateTime date1 = Convert.ToDateTime(dp_3.Text);
                DateTime date2 = Convert.ToDateTime(dp_4.Text).AddDays(1);
                Common.Filter("select * from roundmeasurement where 测量时间>='" + date1 + "' and 测量时间<='" + date2 + "'", RDG);
            }
            else if (dp_3.Text != "" && dp_4.Text == "")
            {
                DateTime date1 = Convert.ToDateTime(dp_3.Text);
                Common.Filter("select * from roundmeasurement where 测量时间>='" + date1 + "'", RDG);


            }
            else if (dp_3.Text == "" && dp_4.Text != "")
            {
                DateTime date2 = Convert.ToDateTime(dp_4.Text).AddDays(1);
                Common.Filter("select * from roundmeasurement where 测量时间<='" + date2 + "'", RDG);

            }
            else
            {
                Common.Filter("select * from roundmeasurement", RDG);
            }
        }



        private void cameraComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //captureElement.VideoCaptureSource = (string)cameraComboBox.SelectedItem;
            //cameraindex = cameraComboBox.SelectedIndex;
        }


        private void TabItem_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            yptb.Visibility = Visibility.Collapsed;
            yp.Visibility = Visibility.Collapsed;

            //captureElement.VideoCaptureSource = null;
            //cameraComboBox.ItemsSource = MultimediaUtil.VideoInputNames;
            //if (MultimediaUtil.VideoInputNames.Length > 0)
            //{
            //    cameraComboBox.SelectedIndex = cameraindex;

            //    captureElement.VideoCaptureSource = cameraComboBox.Text;
            //}
            //else
            //{
            //    System.Windows.MessageBox.Show("未找到摄像头", "", MessageBoxButton.OK, MessageBoxImage.Information);
            //}

            ////rstp流
            //const string video_url = "rtsp://172.20.10.3:554/test";
            //LibVLC _libvlc;

            //_libvlc = new LibVLC();
            //VideoView0.MediaPlayer = new MediaPlayer(_libvlc);
            //var m = new Media(_libvlc, video_url, FromType.FromLocation);
            ////参数
            //m.AddOption(":rtsp-tcp");
            //m.AddOption(":clock-synchro=0");
            //m.AddOption(":live-caching=0");
            //m.AddOption(":network-caching=333");
            //m.AddOption(":file-caching=0");
            //m.AddOption(":grayscale");
            //m.AddOption(":reflector_buffer_size_sec=0");
            //VideoView0.MediaPlayer.Play(m);

            gby.Opacity = 1;
            gbf.Opacity = 1;
            CB2.IsChecked = false;
            CB3.IsChecked = false;

            shape1.Visibility = Visibility.Collapsed;
            shape2.Visibility = Visibility.Collapsed;
            shape3.Visibility = Visibility.Collapsed;
            shape4.Visibility = Visibility.Collapsed;
        }

        public bool IsOpen_1
        {
            get { return (bool)GetValue(IsOpen_1Property); }
            set { SetValue(IsOpen_1Property, value); }
        }
        public static readonly DependencyProperty IsOpen_1Property =
            DependencyProperty.Register("IsOpen_1", typeof(bool), typeof(ManagementSystemView), new PropertyMetadata(false));
        private void addresstb_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            IsOpen_1 = true;
        }

        private void equipmentlist_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            IsOpen_1 = false;
        }

        private void Addbtn_Click(object sender, RoutedEventArgs e)
        {
            Common.AddEquipment(UC_ipaddress, sv_1);
            wp_1.Children.Clear();
            Common.ReadEquipment(wp_1, 294, 245, 18);
            wp_2.Children.Clear();
            Common.ReadEquipment(wp_2, 170, 125, 12);

        }

        private void Deletebtn_Click(object sender, RoutedEventArgs e)
        {
            Common.DeleteEquipment(UC_ipaddress);
            wp_1.Children.Clear();
            Common.ReadEquipment(wp_1, 294, 245, 18);
            wp_2.Children.Clear();
            Common.ReadEquipment(wp_2, 170, 125, 12);
        }

        private void WDG_Loaded(object sender, RoutedEventArgs e)
        {
            //db
            Common.InitializeMeasurementTable("select * from manager", WDG);
        }




        private void Resultti_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            CB2.IsChecked = false;
            CB3.IsChecked = false;
            shape1.Visibility = Visibility.Collapsed;
            shape2.Visibility = Visibility.Collapsed;
            shape3.Visibility = Visibility.Collapsed;
            shape4.Visibility = Visibility.Collapsed;

            gbf.Opacity = 1;
            Btn1.Opacity = 1;
            gby.Opacity = 1;
            Btn0.Opacity = 1;
            gbmpf.Opacity = 1;
            gbmpy.Opacity = 1;

            shape1.Visibility = Visibility.Collapsed;
            shape2.Visibility = Visibility.Collapsed;
            shape3.Visibility = Visibility.Collapsed;
            shape4.Visibility = Visibility.Collapsed;
            shape5.Visibility = Visibility.Collapsed;
            shape6.Visibility = Visibility.Collapsed;
            shape7.Visibility = Visibility.Collapsed;

            yptb.Visibility = Visibility.Collapsed;
            yp.Visibility = Visibility.Collapsed;


            //去重（取出行数n，对1到n-1行进行查询，距第n行时间或当前时间30秒内的行进行删除）
            string sqlStr_1 = "SELECT COUNT(* ) FROM measurementdata;";
            System.Data.DataTable dt = YMysqlHelper.MysqlHelper.ExecuteDataTable(sqlStr_1);
            int n = Convert.ToUInt16(dt.Rows[0][0]) - 1;   //索引从0开始

            string sqlStr_2 = "SELECT * FROM measurementdata;";
            System.Data.DataTable dt_3 = YMysqlHelper.MysqlHelper.ExecuteDataTable(sqlStr_2);
            object lrt = dt_3.Rows[n][1];
            int m = Convert.ToUInt16(dt_3.Rows[n][0]) - 1;

            string sqlStr_4 = "DELETE FROM measurementdata WHERE 测量时间 > DATE_SUB('" + lrt + "',INTERVAL 30 SECOND) AND 序列号 BETWEEN 0 AND '" + m + "';";
            YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr_4);

            Common.InitializeMeasurementTable("select * from rectanglemeasurement", SDG);
            Common.InitializeMeasurementTable("select * from roundmeasurement", RDG);

        }

        private void Deviceti_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            yptb.Visibility = Visibility.Collapsed;
            yp.Visibility = Visibility.Collapsed;

            shape1.Visibility = Visibility.Collapsed;
            shape2.Visibility = Visibility.Collapsed;
            shape3.Visibility = Visibility.Collapsed;
            shape4.Visibility = Visibility.Collapsed;

            gbf.Opacity = 1;
            Btn1.Opacity = 1;
            gby.Opacity = 1;
            Btn0.Opacity = 1;
        }

        private void Managerti_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            yptb.Visibility = Visibility.Collapsed;
            yp.Visibility = Visibility.Collapsed;

            shape1.Visibility = Visibility.Collapsed;
            shape2.Visibility = Visibility.Collapsed;
            shape3.Visibility = Visibility.Collapsed;
            shape4.Visibility = Visibility.Collapsed;

            gbf.Opacity = 1;
            Btn1.Opacity = 1;
            gby.Opacity = 1;
            Btn0.Opacity = 1;
        }

        private void TB0_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {


            gbf.Opacity = 0.1;
            Btn1.Opacity = 0.1;
            gby.Opacity = 1;
            Btn0.Opacity = 1;

            CB3.IsChecked = false;


            if (CB2.IsChecked == false)
            {
                shape1.Visibility = Visibility.Collapsed;
                shape2.Visibility = Visibility.Collapsed;
                shape3.Visibility = Visibility.Visible;
                shape4.Visibility = Visibility.Collapsed;
            }

        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            gby.Visibility = Visibility.Visible;
            gbf.Visibility = Visibility.Visible;
            gbmpy.Visibility = Visibility.Collapsed;
            gbmpf.Visibility = Visibility.Collapsed;

            Btn4.Visibility = Visibility.Collapsed;
            CB4.Visibility = Visibility.Collapsed;
            Btn5.Visibility = Visibility.Collapsed;
            CB5.Visibility = Visibility.Collapsed;

            Btn0.Visibility = Visibility.Visible;
            CB0.Visibility = Visibility.Visible;
            Btn1.Visibility = Visibility.Visible;
            CB1.Visibility = Visibility.Visible;

            wcsif.Visibility = Visibility.Collapsed;
            wcslb.Visibility = Visibility.Collapsed;

            shape1.Visibility = Visibility.Collapsed;
            shape2.Visibility = Visibility.Collapsed;
            shape3.Visibility = Visibility.Collapsed;
            shape4.Visibility = Visibility.Collapsed;
            shape5.Visibility = Visibility.Collapsed;
            shape6.Visibility = Visibility.Collapsed;
            shape7.Visibility = Visibility.Collapsed;


            //db
            string sqlStr1 = "UPDATE 状态 SET `质检类型`=1";
            YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr1);

            gbmpf.Opacity = 1;
            Btn5.Opacity = 1;
            gbmpy.Opacity = 1;
            Btn4.Opacity = 1;
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            gbmpy.Visibility = Visibility.Visible;
            gbmpf.Visibility = Visibility.Visible;
            gby.Visibility = Visibility.Collapsed;
            gbf.Visibility = Visibility.Collapsed;

            Btn4.Visibility = Visibility.Visible;
            CB4.Visibility = Visibility.Visible;
            Btn5.Visibility = Visibility.Visible;
            CB5.Visibility = Visibility.Visible;

            Btn0.Visibility = Visibility.Collapsed;
            CB0.Visibility = Visibility.Collapsed;
            Btn1.Visibility = Visibility.Collapsed;
            CB1.Visibility = Visibility.Collapsed;

            wcsif.Visibility = Visibility.Collapsed;
            wcslb.Visibility = Visibility.Collapsed;

            CB2.IsChecked = false;
            CB3.IsChecked = false;

            shape1.Visibility = Visibility.Collapsed;
            shape2.Visibility = Visibility.Collapsed;
            shape3.Visibility = Visibility.Collapsed;
            shape4.Visibility = Visibility.Collapsed;

            gbf.Opacity = 1;
            gby.Opacity = 1;

            //db
            string sqlStr1 = "UPDATE 状态 SET `质检类型`=1";
            YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr1);
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            gby.Visibility = Visibility.Collapsed;
            gbf.Visibility = Visibility.Collapsed;
            gbmpy.Visibility = Visibility.Collapsed;
            gbmpf.Visibility = Visibility.Collapsed;

            Btn4.Visibility = Visibility.Collapsed;
            CB4.Visibility = Visibility.Collapsed;
            Btn5.Visibility = Visibility.Collapsed;
            CB5.Visibility = Visibility.Collapsed;

            Btn0.Visibility = Visibility.Collapsed;
            CB0.Visibility = Visibility.Collapsed;
            Btn1.Visibility = Visibility.Collapsed;
            CB1.Visibility = Visibility.Collapsed;

            wcsif.Visibility = Visibility.Visible;
            wcslb.Visibility = Visibility.Visible;

            CB2.IsChecked = false;
            CB3.IsChecked = false;

            shape1.Visibility = Visibility.Collapsed;
            shape2.Visibility = Visibility.Collapsed;
            shape3.Visibility = Visibility.Collapsed;
            shape4.Visibility = Visibility.Collapsed;

            shape5.Visibility = Visibility.Collapsed;
            shape6.Visibility = Visibility.Collapsed;
            shape7.Visibility = Visibility.Collapsed;

            gbf.Opacity = 1;
            gby.Opacity = 1;

            //db
            string sqlStr1 = "UPDATE 状态 SET `质检类型`=0";
            YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr1);

            gbmpf.Opacity = 1;
            Btn5.Opacity = 1;
            gbmpy.Opacity = 1;
            Btn4.Opacity = 1;
        }

        private void CB4_Checked(object sender, RoutedEventArgs e)
        {
            ifpause = true;

            //if (ykgs.Text == "4")
            //{
            //    ypcd.Text = ypd.Text;
            //}

            //string sql = "insert into roundparam(上传时间,外径,打孔,内径,孔个数) values('" + DateTime.Now + "','" + TB0.Text + "',1,'" + TB2.Text + "','" + TB1.Text + "')";
            //string sql = "insert into param(上传时间,长,宽,是否打孔,内径,孔个数) values('" + DateTime.Now + "','" + TB0.Text + "','" + TB0.Text + "',1,'" + TB2.Text + "','" + TB1.Text + "')";

            //double thickness = Convert.ToUInt16(yhd.Text), d = Convert.ToDouble(ywj.Text);
            double thickness = Convert.ToDouble(yhd.Text), d = Convert.ToDouble(ywj.Text);
            //string height = (Convert.ToInt64(yhd.Text)).ToString();
            string height = (Convert.ToDouble(yhd.Text)).ToString();
            string sqlStr = "UPDATE xinxi SET `直径`='" + ywj.Text + "',`孔1`='" + ykj1.Text + "',`孔2`='" + ykj2.Text + "',`是否有孔`='1',`厚度`='" + height + "',`误差`='" + ywc.Text + "',`孔个数`='" + ykgs.Text + "',`形状`='0',`间距起始位置`='" + Common.VelocityMatching(thickness, d)[2] + "',`间距结束位置`='" + Common.VelocityMatching(thickness, d)[3] + "',`中心点`='" + Common.VelocityMatching(thickness, d)[4] + "',`PCD间距`='" + ypcd.Text + "',`PD间距`='" + ypd.Text + "'";
            YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr);
            string sqlStr_2 = "UPDATE `状态` SET `比值`='" + Common.VelocityMatching(thickness, d)[5] + "';";
            YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr_2);
            ywj.Text = "";
            yhd.Text = "";
            ykgs.Text = "";
            ykj1.Text = "";
            ykj2.Text = "";
            ypcd.Text = "";
            ypd.Text = "";
            ywc.Text = "";


            if (Switchbtn.IsChecked == true)
            {
                Openbtn.IsEnabled = true;
            }
        }

        private void CB4_Unchecked(object sender, RoutedEventArgs e)
        {
            CB4.IsEnabled = false;
        }

        private void CB5_Checked(object sender, RoutedEventArgs e)
        {
            ifpause = true;

            //string sql = "insert into rectangleparam(上传时间,长,宽,打孔,内径,孔个数) values('" + DateTime.Now + "','" + TB3.Text + "','" + TB4.Text + "',1,'" + TB6.Text + "','" + TB5.Text + "')";
            //string sql = "insert into param(上传时间,长,宽,是否打孔,内径,孔个数) values('" + DateTime.Now + "','" + TB3.Text + "','" + TB4.Text + "',1,'" + TB6.Text + "','" + TB5.Text + "')";
            double thickness = Convert.ToDouble(fhd.Text), d = Convert.ToDouble(fcd.Text);
            string height = (Convert.ToDouble(fhd.Text)).ToString();
            string sqlStr = "UPDATE xinxi SET `形状`='1',`长`='" + fcd.Text + "',`宽`='" + fkd.Text + "',`孔1`='" + fkj1.Text + "',`孔2`='" + fkj2.Text + "',`是否有孔`='1',`厚度`='" + height + "',`误差`='" + fwc.Text + "',`孔个数`='" + fkgs.Text + "',`间距起始位置`='" + Common.VelocityMatching(thickness, d)[2] + "',`间距结束位置`='" + Common.VelocityMatching(thickness, d)[3] + "',`中心点`='" + Common.VelocityMatching(thickness, d)[4] + "',`PCD间距`='" + fpcd.Text + "',`PD间距`='" + fpd.Text + "'";
            YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr);
            string sqlStr_2 = "UPDATE `状态` SET `比值`='" + Common.VelocityMatching(thickness, d)[5] + "';";
            YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr_2);
            fcd.Text = "";
            fkd.Text = "";
            fhd.Text = "";
            fkgs.Text = "";
            fkj1.Text = "";
            fkj2.Text = "";
            fpcd.Text = "";
            fpd.Text = "";
            fwc.Text = "";


            if (Switchbtn.IsChecked == true)
            {
                Openbtn.IsEnabled = true;
            }
        }

        private void CB5_Unchecked(object sender, RoutedEventArgs e)
        {
            CB5.IsEnabled = false;
        }

        private void Btn4_Click(object sender, RoutedEventArgs e)
        {
            if (ykgs.Text == "5")
            {
                if (ywj.Text != "" && yhd.Text != "" && ykgs.Text != "" && ykj1.Text != "" && ykj2.Text != "" && ypcd.Text != "" && ypd.Text != "" && ywc.Text != "")
                {
                    //cycle
                    //double thickness = Convert.ToUInt16(yhd.Text), d = Convert.ToDouble(ywj.Text);
                    double thickness = Convert.ToDouble(yhd.Text), d = Convert.ToDouble(ywj.Text);


                    cycle_1 = Convert.ToUInt16(Common.VelocityMatching(thickness, d)[0]);
                    cycle_2 = Convert.ToUInt16(Common.VelocityMatching(thickness, d)[1]);

                    CB4.IsEnabled = true;
                }
                else
                {
                    System.Windows.MessageBox.Show("请输入完整参数。", "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            else if (ykgs.Text == "4")
            {
                if (ywj.Text != "" && yhd.Text != "" && ykgs.Text != "" && ykj1.Text != "" && ykj2.Text != "" && ypd.Text != "" && ywc.Text != "")
                {
                    //cycle
                    //double thickness = Convert.ToUInt16(yhd.Text), d = Convert.ToDouble(ywj.Text);
                    double thickness = Convert.ToDouble(yhd.Text), d = Convert.ToDouble(ywj.Text);


                    cycle_1 = Convert.ToUInt16(Common.VelocityMatching(thickness, d)[0]);
                    cycle_2 = Convert.ToUInt16(Common.VelocityMatching(thickness, d)[1]);

                    CB4.IsEnabled = true;
                }
                else
                {
                    System.Windows.MessageBox.Show("请输入完整参数。", "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }

            
        }

        private void Btn5_Click(object sender, RoutedEventArgs e)
        {
            if (fcd.Text != "" && fkd.Text != "" && fhd.Text != "" && fkgs.Text != "" && fkj1.Text != "" && fkj2.Text != "" && fpcd.Text != "" && fpd.Text != "" && fwc.Text != "")
            {
                //cycle
                double thickness = Convert.ToUInt16(fhd.Text), d = Convert.ToDouble(fcd.Text);

                cycle_1 = Convert.ToUInt16(Common.VelocityMatching(thickness, d)[0]);
                cycle_2 = Convert.ToUInt16(Common.VelocityMatching(thickness, d)[1]);

                CB5.IsEnabled = true;

            }
            else
            {
                System.Windows.MessageBox.Show("请输入完整参数。", "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void ywj_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            gbmpf.Opacity = 0.1;
            Btn5.Opacity = 0.1;
            gbmpy.Opacity = 1;
            Btn4.Opacity = 1;

            shape6.Visibility = Visibility.Visible;
            shape5.Visibility = Visibility.Collapsed;
            shape7.Visibility = Visibility.Collapsed;

        }

        private void fcd_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            gbmpy.Opacity = 0.1;
            Btn4.Opacity = 0.1;
            gbmpf.Opacity = 1;
            Btn5.Opacity = 1;

            shape5.Visibility = Visibility.Visible;
            shape6.Visibility = Visibility.Collapsed;
            shape7.Visibility = Visibility.Collapsed;
        }

        private void ykgs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ykgs.Text == "4")
            {
                shape7.Visibility = Visibility.Collapsed;
                shape6.Visibility = Visibility.Visible;

                //ypcd.Visibility = Visibility.Collapsed;
                //tbypcd.Visibility = Visibility.Collapsed;
            }
            else if (ykgs.Text == "5")
            {
                shape6.Visibility = Visibility.Collapsed;
                shape7.Visibility = Visibility.Visible;

                //ypcd.Visibility = Visibility.Visible;
                //tbypcd.Visibility = Visibility.Visible;
            }
        }

        private void Paramti_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            yptb.Visibility = Visibility.Visible;
            yp.Visibility = Visibility.Visible;
        }

        private void yp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            int selectedIndex = yp.SelectedIndex;
            
            switch (selectedIndex)
            {
                case 0:
                    if (rbdp.IsChecked == false)
                    {
                        rbdp.IsChecked = true;
                    }

                    TB0_PreviewMouseUp(null, null);
                    CB2.IsChecked = true;

                    //外径
                    TB0.Text = "15.9";
                    //厚度
                    TB7.Text = "1";
                    //孔个数
                    TB1.Text = "1";
                    //外径误差
                    TB9.Text = "0.3";
                    //厚度误差
                    TB11.Text = "0.3";
                    //内径
                    TB2.Text = "5.4";
                    //内径误差
                    TB15.Text = "0.3";

                    break;
                case 1:
                    if (rbdp.IsChecked == false)
                    {
                        rbdp.IsChecked = true;
                    }

                    TB3_PreviewMouseUp(null, null);
                    CB3.IsChecked = true;

                    //长度
                    TB3.Text = "21";
                    //宽度
                    TB4.Text = "21";
                    //厚度
                    TB8.Text = "1";
                    //孔个数
                    TB5.Text = "1";
                    //长度误差
                    TB12.Text = "0.3";
                    //宽度误差
                    TB13.Text = "0.3";
                    //厚度误差
                    TB14.Text = "0.3";
                    //内径
                    TB6.Text = "4.2";
                    //内径误差
                    TB17.Text = "0.3";

                    break;
                case 2:
                    if (rbmp.IsChecked == false)
                    {
                        rbmp.IsChecked = true;
                    }

                    ywj_PreviewMouseUp(null, null);
                    ykgs.Text = "4";

                    //外径
                    ywj.Text = "38";
                    //厚度
                    yhd.Text = "3";
                    //孔个数
                    ykgs.Text = "4";
                    //孔径1
                    ykj1.Text = "13";
                    //孔径2
                    ykj2.Text = "5";
                    //tbypcd
                    ypcd.Text = "28.3";
                    //孔距
                    ypd.Text = "24.5";
                    //误差
                    ywc.Text = "0.3";

                    break;
                case 3:
                    if (rbmp.IsChecked == false)
                    {
                        rbmp.IsChecked = true;
                    }

                    ywj_PreviewMouseUp(null, null);
                    ykgs.Text = "5";

                    //外径
                    ywj.Text = "20";
                    //厚度
                    yhd.Text = "0.3";
                    //孔个数
                    ykgs.Text = "5";
                    //孔径1
                    ykj1.Text = "10";
                    //孔径2
                    ykj2.Text = "2.5";
                    //tbypcd
                    ypcd.Text = "15";
                    //孔距
                    ypd.Text = "10.5";
                    //误差
                    ywc.Text = "0.3";

                    break;
                case 4:
                    if (rbmp.IsChecked == false)
                    {
                        rbmp.IsChecked = true;
                    }

                    fcd_PreviewMouseUp(null, null);

                    //长度
                    fcd.Text = "19.5";
                    //宽度
                    fkd.Text = "19.5";
                    //厚度
                    fhd.Text = "0.3";
                    //孔个数
                    fkgs.Text = "5";
                    //孔径1
                    fkj1.Text = "9";
                    //孔径2
                    fkj2.Text = "2";
                    //tbypcd
                    fpcd.Text = "14";
                    //孔距
                    fpd.Text = "10";
                    //误差
                    fwc.Text = "0.3";

                    break;
            }
        }

        private void TB3_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {


            gbf.Opacity = 1;
            Btn1.Opacity = 1;
            gby.Opacity = 0.1;
            Btn0.Opacity = 0.1;

            CB2.IsChecked = false;


            if (CB3.IsChecked == false)
            {
                shape1.Visibility = Visibility.Visible;
                shape2.Visibility = Visibility.Collapsed;
                shape3.Visibility = Visibility.Collapsed;
                shape4.Visibility = Visibility.Collapsed;
            }

        }


    }
}
