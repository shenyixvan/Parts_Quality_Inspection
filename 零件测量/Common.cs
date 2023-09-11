using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Windows.Controls;
using YF.Utility;
using System.Timers;
using S7.Net.Types;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows;

namespace 零件测量
{
    class Common
    {
        //public static S7.Net.Plc plc= new S7.Net.Plc(S7.Net.CpuType.S7200Smart, "500f21s937.qicp.vip", 0,1);     //0,1:机架号 槽号
        

        /// <summary>
        /// 导出矩形excel
        /// </summary>
        /// <param name="path"></param>
        /// <param name="dt"></param>
        public static void ExportSExcel(string path, System.Data.DataTable dt)
        {
            Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
            Workbook excelWB = excelApp.Workbooks.Add(System.Type.Missing);
            Worksheet excelWS = (Worksheet)excelWB.Worksheets[1];

            Range range = excelWS.get_Range("A1", "R1"); 
            range.Merge(0);
            range.get_Offset(0, 0).Cells.Value = "矩形零件检测结果";
            range.HorizontalAlignment = XlHAlign.xlHAlignCenter;

            range = excelWS.get_Range("A2", Type.Missing);
            range.get_Offset(0, 0).Cells.Value = "序列号";
            range.get_Offset(0, 1).Cells.Value = "测量时间";
            range.get_Offset(0, 1).ColumnWidth = 15;
            range.get_Offset(0, 2).Cells.Value = "合格个数";
            range.get_Offset(0, 3).Cells.Value = "高度不合格个数";
            range.get_Offset(0, 3).ColumnWidth = 15;
            range.get_Offset(0, 4).Cells.Value = "尺寸不合格个数";
            range.get_Offset(0, 4).ColumnWidth = 15;
            range.get_Offset(0, 5).Cells.Value = "破损个数";
            range.get_Offset(0, 6).Cells.Value = "测量个数";
            range.get_Offset(0, 7).Cells.Value = "合格率";
            range.get_Offset(0, 8).Cells.Value = "长";
            range.get_Offset(0, 9).Cells.Value = "宽";
            range.get_Offset(0, 10).Cells.Value = "是否打孔";
            range.get_Offset(0, 11).Cells.Value = "PCD间距";
            range.get_Offset(0, 12).Cells.Value = "PD间距";
            range.get_Offset(0, 13).Cells.Value = "内径";
            range.get_Offset(0, 14).Cells.Value = "内径2";
            range.get_Offset(0, 15).Cells.Value = "孔个数";
            range.get_Offset(0, 16).Cells.Value = "厚度";
            range.get_Offset(0, 17).Cells.Value = "误差";



            range = excelWS.get_Range("A3", Type.Missing);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    range.get_Offset(i, j).Cells.Value = dt.Rows[i][j].ToString();
                }
            }


            excelWB.SaveAs(path);
            excelWB.Close();
            excelApp.Quit();
            System.Windows.MessageBox.Show("导出Excel成功");
        }


        /// <summary>
        /// 导出圆形excel
        /// </summary>
        /// <param name="path"></param>
        /// <param name="dt"></param>
        public static void ExportRExcel(string path, System.Data.DataTable dt)
        {
            Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
            Workbook excelWB = excelApp.Workbooks.Add(System.Type.Missing);
            Worksheet excelWS = (Worksheet)excelWB.Worksheets[1];

            Range range = excelWS.get_Range("A1", "Q1");
            range.Merge(0);
            range.get_Offset(0, 0).Cells.Value = "圆形零件检测结果";
            range.HorizontalAlignment = XlHAlign.xlHAlignCenter;

            range = excelWS.get_Range("A2", Type.Missing);
            range.get_Offset(0, 0).Cells.Value = "序列号";
            range.get_Offset(0, 1).Cells.Value = "测量时间";
            range.get_Offset(0, 1).ColumnWidth = 15;
            range.get_Offset(0, 2).Cells.Value = "合格个数";
            range.get_Offset(0, 3).Cells.Value = "高度不合格个数";
            range.get_Offset(0, 3).ColumnWidth = 15;
            range.get_Offset(0, 4).Cells.Value = "尺寸不合格个数";
            range.get_Offset(0, 4).ColumnWidth = 15;
            range.get_Offset(0, 5).Cells.Value = "破损个数";
            range.get_Offset(0, 6).Cells.Value = "测量个数";
            range.get_Offset(0, 7).Cells.Value = "合格率";
            range.get_Offset(0, 8).Cells.Value = "外径";
            range.get_Offset(0, 9).Cells.Value = "是否打孔";
            range.get_Offset(0, 10).Cells.Value = "PCD间距";
            range.get_Offset(0, 11).Cells.Value = "PD间距";
            range.get_Offset(0, 12).Cells.Value = "内径";
            range.get_Offset(0, 13).Cells.Value = "内径2";
            range.get_Offset(0, 14).Cells.Value = "孔个数";
            range.get_Offset(0, 15).Cells.Value = "厚度";
            range.get_Offset(0, 16).Cells.Value = "误差";



            range = excelWS.get_Range("A3", Type.Missing);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    range.get_Offset(i, j).Cells.Value = dt.Rows[i][j].ToString();
                }
            }


            excelWB.SaveAs(path);
            excelWB.Close();
            excelApp.Quit();
            System.Windows.MessageBox.Show("导出Excel成功");
        }



        /// <summary>
        /// 选择文件夹
        /// </summary>
        /// <param name="name">文件名</param>
        /// <returns>路径</returns>
        public static string PathPicker(string name)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "请选择或新建一个文件夹。";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fbd.SelectedPath += '\\' + name + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
                
            }
            return fbd.SelectedPath;
        }

        /// <summary>
        /// 处理表
        /// </summary>
        public static void GetTable()
        {
            List<string> sqlstrlist = new List<string>();
            
            string sqlstr_1 = "truncate table rectanglemeasurement";

            string sqlstr_2 = "insert into rectanglemeasurement(测量时间,合格个数,高度不合格个数,尺寸不合格个数,破损个数,测量个数,合格率,长,宽,是否打孔,内径,内径2,孔个数,厚度,误差,PCD间距,PD间距) " +
                "select 测量时间,合格个数,高度不合格数量,尺寸不合格数量,破损数量,测量个数,合格率,长,宽,是否打孔,内径,内径2,孔个数,厚度,允许误差,PCD间距,PD间距 from measurementdata where 形状='1'";

            string sqlstr_3 = "truncate table roundmeasurement";

            
            string sqlstr_4 = "insert into roundmeasurement(测量时间,合格个数,高度不合格个数,尺寸不合格个数,破损个数,测量个数,合格率,外径,是否打孔,内径,内径2,孔个数,厚度,误差,PCD间距,PD间距) " +
                "select 测量时间,合格个数,高度不合格数量,尺寸不合格数量,破损数量,测量个数,合格率,直径,是否打孔,内径,内径2,孔个数,厚度,允许误差,PCD间距,PD间距 from measurementdata where 形状='0'";

            //最后一条数据会重复执行两次
            string sqlstr_5 = "flush tables";




            sqlstrlist.Add(sqlstr_1);
            sqlstrlist.Add(sqlstr_2);
            sqlstrlist.Add(sqlstr_3);
            sqlstrlist.Add(sqlstr_4);
            sqlstrlist.Add(sqlstr_5);




            YMysqlHelper.MysqlHelper.ExecuteNoQueryTran(sqlstrlist);


        }

        /// <summary>
        /// 初始化datagrid
        /// </summary>
        /// <param name="sqlstr"></param>
        /// <param name="grid"></param>
        public static void InitializeMeasurementTable(string sqlstr, System.Windows.Controls.DataGrid grid)
        {

            GetTable();

            MySqlConnection conn = new MySqlConnection(YMysqlHelper.MysqlHelper.connectionString);                 
            try
            {
                conn.Open();

                MySqlCommand cmd = new MySqlCommand(sqlstr, conn);
                MySqlDataAdapter msda = new MySqlDataAdapter(cmd);
                MySqlCommandBuilder cb = new MySqlCommandBuilder(msda);
                System.Data.DataTable dt = new System.Data.DataTable();
                msda.Fill(dt);
                grid.ItemsSource = null;
                grid.Items.Clear();
                grid.ItemsSource = dt.DefaultView;



            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());

            }
            finally
            {
                conn.Close();
            }


        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="location">末位置</param>
        /// <param name="interval">时间间隔</param>
        /// <param name="property">X/Y方向</param>
        /// <param name="render">对象</param>
        public static void DAnimation(int location,double interval , DependencyProperty property, TranslateTransform render)
        {
            DoubleAnimation da = new DoubleAnimation
            {
                To = location
            };
            da.Duration = new Duration(TimeSpan.FromSeconds(interval));
            render.BeginAnimation(property, da);
        }


        /// <summary>
        /// 筛选日期
        /// </summary>
        /// <param name="sqlstr"></param>
        /// <param name="dg"></param>
        public static void Filter(string sqlstr, System.Windows.Controls.DataGrid dg)
        {
            MySqlConnection conn = new MySqlConnection(YMysqlHelper.MysqlHelper.connectionString);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sqlstr, conn);
                MySqlDataAdapter msda = new MySqlDataAdapter(cmd);
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    reader.Close();
                    System.Data.DataTable dt = new System.Data.DataTable();
                    msda.Fill(dt);
                    dg.ItemsSource = null;
                    dg.Items.Clear();
                    dg.ItemsSource = dt.DefaultView;
                }
                else
                {
                    System.Windows.MessageBox.Show("该时段无记录");
                    

                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }
        /// <summary>
        /// 显示所有设备到面板中
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="fontsize"></param>
        public static void ReadEquipment(WrapPanel panel,int height,int width,int fontsize)
        {
            List<string> iplist = new List<string>();
            List<string> ecnamelist = new List<string>();
            string ecname = null;
            string sqlStr = "select inet_ntoa(ip) from equipment";
            System.Data.DataTable table = YMysqlHelper.MysqlHelper.ExecuteDataTable(sqlStr);
            foreach (DataRow row in table.Rows)
            {
                iplist.Add(row[0].ToString());
            }
            for(int i = 0; i<iplist.Count; i++)
            {
                ecname = "ec_" + i.ToString();
                EquipmentCardUserControl ec  = new EquipmentCardUserControl();
                ec.Name = ecname;
                ec.box.Header = iplist[i];
                ec.Height = height;
                ec.Width = width;
                ec.equipmentimage.Foreground = Brushes.Gray;
                ec.status.Text = "未连接";
                ec.FontSize = fontsize;
                panel.Children.Add(ec);
            }
            
        }


        /// <summary>
        /// 添加设备
        /// </summary>
        /// <param name="iPBox"></param>
        /// <param name="panel"></param>
        /// <param name="viewer"></param>
        public static void AddEquipment(IPBoxUserControl iPBox,ScrollViewer viewer)
        {         
            if (iPBox.TbxIP1.Text != "" && iPBox.TbxIP2.Text != "" && iPBox.TbxIP3.Text != "" && iPBox.TbxIP4.Text != "")
            {
                object objCount;
                string strCount;
                string sqlStr_1 = "select count(*) from equipment";
                string ip;

                MySqlConnection conn = new MySqlConnection(YMysqlHelper.MysqlHelper.connectionString);
                MySqlDataAdapter da = new MySqlDataAdapter(sqlStr_1, conn);
                System.Data.DataTable dt = new System.Data.DataTable();
                da.Fill(dt);
                objCount = dt.Rows[0][0];

                strCount = (Convert.ToInt32(objCount) + 1).ToString();

                ip = iPBox.TbxIP1.Text + '.' + iPBox.TbxIP2.Text + '.' + iPBox.TbxIP3.Text + '.' + iPBox.TbxIP4.Text;
                string sqlStr_2 = "insert into equipment(编号,ip) values ('"+ strCount + "',inet_aton('"+ip+"'))";
                YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr_2);

                
                //viewer.ScrollToEnd();

            }
            iPBox.TbxIP1.Text = "";
            iPBox.TbxIP2.Text = "";
            iPBox.TbxIP3.Text = "";
            iPBox.TbxIP4.Text = "";
        }
        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="iPBox"></param>
        /// <param name="panel"></param>
        public static void DeleteEquipment(IPBoxUserControl iPBox)
        {
            if (iPBox.TbxIP1.Text != "" && iPBox.TbxIP2.Text != "" && iPBox.TbxIP3.Text != "" && iPBox.TbxIP4.Text != "")
            {
                string ip;
                ip = iPBox.TbxIP1.Text + '.' + iPBox.TbxIP2.Text + '.' + iPBox.TbxIP3.Text + '.' + iPBox.TbxIP4.Text;


                string sqlStr_1 = "DELETE FROM equipment WHERE ip=(inet_aton('" + ip + "'))";
                string sqlStr_2 = "UPDATE equipment SET `编号`=NULL";        
                string sqlStr_3 = "SET @rownum :=0";
                string sqlStr_4 = "UPDATE equipment SET `编号`=(@rownum := @rownum+1)";
                
                YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr_1);
                YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr_2);
                YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr_3);
                YMysqlHelper.MysqlHelper.ExecuteNonQuery(sqlStr_4);

            }
            iPBox.TbxIP1.Text = "";
            iPBox.TbxIP2.Text = "";
            iPBox.TbxIP3.Text = "";
            iPBox.TbxIP4.Text = "";
        }

        //判断重名
        public static void IfRepeat()
        {

        }
        /// <summary>
        /// 传送带速度匹配
        /// </summary>
        /// <param name="thickness">零件厚度</param>
        /// <param name="d">零件直径或长度</param>
        /// <returns></returns>
        public static double[] VelocityMatching(double thickness,double d)
        {
            double cycle_1=0,cycle_2=0;

            //WD = 67.23768115942029;
            double A = 0, t_v = 2.7, WD = 67.5843413, f = 2.9, R_V = 720, t = 0.00065, v_1 = 0, v_2 = 0, l = 0, Lmax = 60.517, lp = 0, begin = 0, end = 0, Tred = 0.1056158144462509;

            //分拣
            A = t_v * (WD - thickness) / f / R_V * 1;
            v_1 = A / t; 
            cycle_1 = 30 * Math.PI * 1.8 * 1000000 / 360 / 8 / v_1;

            //排列
            l = d + Tred * v_1;
            v_2 = d * v_1 / 2 / l;
            cycle_2 = cycle_1 * 5 / 3 * v_1 / v_2 / 4;                     //8 32 

            //间距换算 宽方向
            lp = 720 / 60.517 * l;
            begin = (720 - lp) / 2;
            end = lp + begin;

            //零件中心坐标
            double p = 720 / 60.517 * d / 2;

            //单个零件检测时间(s)
            double v;
            v = l / v_1;

            double[] w = new double[] { cycle_1, cycle_2, begin, end, p, A, v };

            //调试用
            //double q = w[3] - w[2];
            //System.Windows.MessageBox.Show(v_1.ToString() + "\r\n" + w[0].ToString() + "\r\n" + w[1].ToString() + "\r\n" + w[2].ToString() + "\r\n" + w[3].ToString() + "\r\n" + l.ToString() + "\r\n" + q.ToString() + "\r\n" + v.ToString());
            System.Windows.MessageBox.Show(v.ToString());
            //调试用

            return w;
            
        }


    }
}
