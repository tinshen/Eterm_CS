using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
//using SevenZSharp;
using System.Runtime.InteropServices;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.Security.Permissions;
using Microsoft.Win32;



namespace Eterm_CS
{
    public partial class FrmMain : Form
    {
        bool isopen;
        public static Guid IID_IAuthenticate = new Guid("79eac9d0-baf9-11ce-8c82-00aa004ba90b");
        public const int INET_E_DEFAULT_ACTION = unchecked((int)0x800C0011);
        public const int S_OK = unchecked((int)0x00000000);
        string path = string.Empty;
        string dir = string.Empty;//文件路径
        string name = string.Empty;//文件名
        DataTable dt2 = new DataTable();//取订座指令
        DataTable dt1 = new DataTable();//取文件名和文件路径
        string ftp_type = string.Empty, id_code = string.Empty, iss_co = string.Empty;
        WriteFile writeFile = new WriteFile();
        string ls_status = string.Empty;
        string ls_flag = string.Empty;
        string str_content = string.Empty;
        int time_out = 300;
        int il_loop_count = 0;

        [DefaultValue(false)]
        public bool IsOpen
        {
            get { return isopen; }
            set { isopen = value; }
        }
        public FrmMain()
        {
            InitializeComponent();
        }
        string ls_id_code = string.Empty, ls_file_type = string.Empty;
        private void FrmMain_Load(object sender, EventArgs e)
        {
            string ls_message = string.Empty;
            string time_str = ConfigurationManager.AppSettings["TimeOut"];
            ls_id_code = ConfigurationManager.AppSettings["ID_CODE"];
            ls_file_type = ConfigurationManager.AppSettings["FILE_TYPE"];
            if (!string.IsNullOrEmpty(time_str))
            {
                time_out = Int32.Parse(time_str);
            }
            this.startcheck.Checked = isstartup();
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// 获取计划项
        /// </summary>
        private void InitPlan()
        {
            try
            {
                var sql = "select plan_time,act_code,days,last_time,next_time from p_background_plan where act_code='GDS' and job='" + bga.gl_job + "'";
                bga.backgroup_plan = bga.db.ExecuteDataSet(sql).Tables[0];
            }
            catch (Exception ex)
            {
                writeFile.Write("获取执行计划表出错!" + ex.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            InitPlan();
            DateTime lt_date, lt_plan_time, lt_next_time, lt_date1;
            DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);

            int ll_days = 0, ll_1 = 0;
            string ls_act_code;

            if (bga.backgroup_plan == null)
            {
                textBox_status.Text = "系统未配置" + bga.gl_job + "的自动运行计划！";
                return;
            }

            lt_date = DateTime.Now;
            #region
            for (int i = 0; i < bga.backgroup_plan.Rows.Count; i++)
            {
                lt_plan_time = Convert.ToDateTime(bga.backgroup_plan.Rows[i]["plan_time"].ToString());
                if (bga.backgroup_plan.Rows[i]["next_time"].ToString() != string.Empty)
                {
                    lt_next_time = Convert.ToDateTime(bga.backgroup_plan.Rows[i]["next_time"].ToString());
                }
                else
                    lt_next_time = Convert.ToDateTime("2000-01-01");
                ls_act_code = bga.backgroup_plan.Rows[i]["act_code"].ToString();
                if (bga.backgroup_plan.Rows[i]["days"].ToString() != string.Empty)
                {
                    ll_days = Convert.ToInt32(bga.backgroup_plan.Rows[i]["days"].ToString());
                }
                lt_date = DateTime.Now;
                //判断当前时间是否大于下次执行时间，是则执行
                if (lt_date > lt_next_time)
                {
                    bga.backgroup_plan.Rows[i]["last_time"] = lt_date;
                    lt_date1 = Convert.ToDateTime(DateTime.Now.AddDays(ll_days).ToString("yyyy-MM-dd") + " " + lt_plan_time.ToString("HH:mm:ss"));
                    bga.backgroup_plan.Rows[i]["next_time"] = lt_date1;
                    var sql = "update p_background_plan set last_time=to_date('" + lt_date.ToString() + "','yyyy-mm-dd hh24:mi:ss') , next_time=to_date('" + lt_date1.ToString() + "','yyyy-mm-dd hh24:mi:ss') where act_code='" + ls_act_code + "' and plan_time=to_date('" + bga.backgroup_plan.Rows[i]["plan_time"].ToString() + "','yyyy-mm-dd hh24:mi:ss')" +
                        "  and  job='" + bga.gl_job + "' ";
                    try
                    {
                        bga.db.ExecuteNonQuery(sql);
                    }
                    catch (Exception ex)
                    {
                        writeFile.Write("更新后台执行计划时间错误" + ex.Message);
                        return;
                    }
                    if (ls_act_code == "GDS")
                    {
                        button1_Click(sender, e);
                    }
                }
            }
            #endregion
        }

        private void StreamWriter(String dir, string name, string str)
        {
            try
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                FileStream fsFile = new FileStream(dir + "\\" + name, FileMode.OpenOrCreate);
                StreamWriter swWriter = new StreamWriter(fsFile, Encoding.Default);
                //写入数据
                swWriter.WriteLine(str);

                swWriter.Close();

            }
            catch (Exception e)
            {
                writeFile.Write("写文件出错：" + e.Message);
            }
        }

        int num_2 = 0;
        /// <summary>
        /// 单个文件下已执行的订座指令数量
        /// </summary>
        int num_1 = 0;
        /// <summary>
        /// 单个文件下的订座指令数量
        /// </summary>
        int count_1 = 0;
        /// <summary>
        /// 已经处理过的GDS文件数量
        /// </summary>
        int num_3 = 0;
        int num_4 = 0;
        /// <summary>
        /// 此次需要处理的GDS文件数量
        /// </summary>
        int count_2 = 0;
        bool ib_flag = true;
        string cmd = string.Empty;
        int page_count = 0;
        DateTime time = DateTime.Now;
        int pn_num = 0;
        private void button1_Click(object sender, EventArgs e)
        {
            if (ib_flag == false)
                return;
            try
            {
                string content = string.Empty, sql1 = string.Empty;//写入的文件内容             
                sql1 = "select a.ftp_type,a.id_code,a.iss_co, a.d_filename,b.down_file_dir from p_ftp_deal_record a,p_ftp_down_config b where a.ftp_type=b.ftp_type and a.id_code=b.id_code and a.iss_co=b.iss_co AND  a.ftp_type LIKE 'GDS%' AND a.load_flag='B' AND a.d_fileName>=b.bef_str||to_char(sysdate-7,'yyyymmdd') ";
                if (!string.IsNullOrEmpty(ls_id_code))
                    sql1 = sql1 + "  and  instr('" + ls_id_code + "',a.id_code)>0 ";
                if (!string.IsNullOrEmpty(ls_file_type))
                    sql1 = sql1 + "  and  instr('" + ls_file_type + "',a.ftp_type)>0 ";
                sql1 = sql1 + " order by a.d_filename asc";
                dt1 = bga.db.ExecuteDataSet(sql1).Tables[0];//取文件名和文件路径
                num_3 = 0;
                num_4 = -1;
                count_2 = dt1.Rows.Count;
                if (count_2 > 0)
                {
                    ib_flag = false;
                    timer1.Enabled = false;//这时先将timer1关闭。将这次处理的文件处理完之后再启动。             
                    timer4.Enabled = true;//这个时候将timer4开启。
                }
                else
                {
                    textBox_status.Text = "暂时没有需要执行的计划！";
                    timer1.Enabled = true;
                    timer4.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                writeFile.Write(ex.Message);
            }
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer4_Tick(object sender, EventArgs e)
        {
            if (num_3 == count_2 && count_2 > 0)
            {
                timer4.Enabled = false;
                timer1.Enabled = true;
                ib_flag = true;
                textBox_status.Text = "休闲时间,关闭订座窗口";
                eterm_bga.ib_connect_status = false;
                eterm_bga.ib_disconnect = true;
            }
            if (num_3 < count_2 && num_3 != num_4)
            {
                #region  根据订座指令取文件
                dir = dt1.Rows[num_3]["down_file_dir"].ToString();
                name = dt1.Rows[num_3]["d_filename"].ToString();
                ftp_type = dt1.Rows[num_3]["ftp_type"].ToString();
                id_code = dt1.Rows[num_3]["id_code"].ToString();
                iss_co = dt1.Rows[num_3]["iss_co"].ToString();
                string sql2 = "select CODE_STR from table(select a.ARR_STR from p_ftp_deal_record a,p_ftp_down_config b where a.ftp_type=b.ftp_type and a.id_code=b.id_code and a.iss_co=b.iss_co  AND a.load_flag='B' AND a.ftp_type='" + ftp_type + "' AND a.id_code='" + id_code + "' AND a.iss_co='" + iss_co + "' AND a.d_filename='" + name + "')";
                try
                {
                    dt2 = bga.db.ExecuteDataSet(sql2).Tables[0];//获取一条p_deal_record中的订座指令集合
                }
                catch (Exception ex)
                {
                    writeFile.Write(ex.Message);
                }
                count_1 = dt2.Rows.Count;
                num_1 = 0;
                num_2 = -1;
                if (count_1 > 0)//如果这次的有需要处理的，则启动timer3来处理
                {
                    this.textBox1.Text += DateTime.Now.ToString() + "(" + name.ToUpper() + ") 开始取订座指令  " + System.Environment.NewLine;
                    num_4 = num_3;
                    str_content = "";
                    timer3.Enabled = true;
                    timer4.Enabled = false;
                }
                else//如果没有，则将这条p_deal_record计划的标志设置一下
                {
                    #region 修改标志
                    string sql3 = "update p_ftp_deal_record set DOWN_TIME=sysdate,FILE_LENGTH=0, load_flag='U',file_status='FINISH' where ftp_type='" + ftp_type + "' AND id_code='" + id_code + "' AND iss_co='" + iss_co + "' AND d_filename='" + name + "'";
                    try
                    {
                        bga.db.ExecuteNonQuery(sql3);
                    }
                    catch (Exception ex)
                    {
                        writeFile.Write("修改文件标示出错：" + ex.Message);
                    }

                    #endregion
                    num_3 = num_3 + 1;
                }

                #endregion
            }

        }

        /// <summary>
        /// 请求订座数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer3_Tick(object sender, EventArgs e)
        {
            #region  订座取数据
            if (num_1 == count_1 && count_1 > 0)
            {
                long size = 0;
                this.textBox1.Text += DateTime.Now.ToString() + "   订座指令取完" + System.Environment.NewLine;
                this.textBox1.Text += DateTime.Now.ToString() + "   开始写入文件" + System.Environment.NewLine;
                StreamWriter(dir, name, str_content);//写数据                            
                this.textBox1.Text += DateTime.Now.ToString() + "   文件写完" + System.Environment.NewLine;
                str_content = string.Empty;//清空数据
                FileInfo file_info = new FileInfo(dir + "\\" + name);
                size = file_info.Length;
                #region 修改标示
                string sql3 = "update p_ftp_deal_record set DOWN_TIME=sysdate,FILE_LENGTH=" + size + ", load_flag='U',file_status='FINISH' where ftp_type='" + ftp_type + "' AND id_code='" + id_code + "' AND iss_co='" + iss_co + "' AND d_filename='" + name + "'";
                try
                {
                    bga.db.ExecuteNonQuery(sql3);
                }
                catch (Exception ex)
                {
                    writeFile.Write("修改文件标示出错：" + ex.Message);
                }
                #endregion
                if (!string.IsNullOrEmpty(textBox1.Text))
                {
                    if (textBox1.Text.Length > 30000)
                        textBox1.Text = string.Empty;//长度超过30000，清空
                }
                timer3.Enabled = false;
                num_1 = 0;
                num_2 = 0;
                num_3 += 1;
                timer4.Enabled = true;
            }

            if (num_1 < count_1 && (num_2 != num_1 || getDiffSend(DateTime.Now, time) > 20))
            {

                #region 循环取数据

                string com = string.Empty;
                cmd = dt2.Rows[num_1]["CODE_STR"].ToString();//取出每条指令
                timer2.Enabled = false;
                num_2 = num_1;
                pn_num = 0;
                if (time_out > 0)
                    Thread.Sleep(time_out);
                time = DateTime.Now;
                il_loop_count = il_loop_count + 1;
                if (il_loop_count >= 10)
                {
                    textBox_status.Text = cmd;
                    if (il_loop_count >= 20)
                        il_loop_count = 0;
                }
                eterm_bga.ib_dataflag = false;
                com = eterm_fun.Eterm_comman(cmd);//取出每条指令后开始进入下一步，区分出各种情况
                //1、没有连接上时应该直接
                timer3.Enabled = false;
                timer2.Enabled = true;
                //Debug.WriteLine(num_1.ToString() + "  " + count_1.ToString());


                #endregion
            }

            #endregion
        }


        /// <summary>
        /// 获取订座指令返回字符串
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                string ls_show_txt = string.Empty, con_str = string.Empty;
                int ll_num;
                if (num_2 == num_1 && count_1 > 0 && getDiffSend(DateTime.Now, time) > 20 && timer3.Enabled == false)
                {
                    timer3.Enabled = true;
                }
                if (eterm_bga.ib_flag)
                {
                    eterm_bga.ib_flag = false;
                }
                if (((eterm_bga.is_eterm_status != "Start") && (eterm_bga.is_eterm_status != "disconnect")) || eterm_bga.is_eterm_result.Length > 0)
                {
                    if ((eterm_bga.is_eterm_status != "connection successed") && (eterm_bga.is_eterm_status != "Start"))
                    {
                        eterm_bga.il_error_count = eterm_bga.il_error_count + 1;
                        if (eterm_bga.il_error_count <= 2)
                        {
                            con_str = con_str + eterm_bga.is_eterm_status + "\r\n";

                        }
                        eterm_bga.is_eterm_result = "";
                        if (eterm_bga.ib_connect_status == true)//当前窗口打开，但系统错误可以设置关闭窗口，然后重新计算
                        {
                            eterm_bga.il_retry_count = eterm_bga.il_retry_count + 1;
                            if (eterm_bga.il_retry_count <= 2) //试两次关闭窗口重新打开，如果还是没办法正常连接就放弃。
                            {
                                textBox_status.Text = "关闭订座窗口";
                                eterm_bga.ib_connect_status = false;
                                eterm_bga.ib_disconnect = true;
                                num_1 -= 1;
                            }
                        }
                    }
                    if (eterm_bga.ib_dataflag)
                    {

                        if (eterm_bga.is_eterm_result.IndexOf("ET PASSENGER DATA NOT FOUND") > -1 && cmd.IndexOf(",P") < 0)
                        {
                            cmd = cmd + ",P";
                            eterm_bga.is_eterm_status = "Start";
                            eterm_bga.is_eterm_result = "";
                            eterm_bga.ib_dataflag = false;
                            eterm_fun.Eterm_comman(cmd);
                            return;
                        }

                        if (eterm_bga.is_eterm_result.IndexOf("SI") > -1 && eterm_bga.is_eterm_result.Trim().Length <= 4)
                        {
                            textBox_status.Text = "关闭订座窗口";
                            eterm_bga.ib_connect_status = false;
                            eterm_bga.ib_disconnect = true;
                            num_1 -= 1;
                        }

                        str_content = str_content + eterm_bga.is_Command_str + eterm_bga.is_eterm_result.Trim();

                        if (il_loop_count >= 10)
                        {
                            textBox_status.Text = eterm_bga.is_eterm_result.Trim();
                            il_loop_count = 0;
                        }
                        if (eterm_bga.is_eterm_result.IndexOf("PAGE ") > -1 && cmd != "PN")
                        {
                            if (eterm_bga.is_eterm_result.LastIndexOf("/") > -1)
                            {
                                int length1 = eterm_bga.is_eterm_result.LastIndexOf("/") - eterm_bga.is_eterm_result.LastIndexOf("PAGE ") - 4;
                                //string sss = com.Substring(com.LastIndexOf("PAGE") + 4, length1);
                                int pages = Convert.ToInt32(eterm_bga.is_eterm_result.Substring(eterm_bga.is_eterm_result.LastIndexOf("PAGE ") + 4, length1));//当前页数
                                int b = eterm_bga.is_eterm_result.LastIndexOf("/") + 1;
                                string aaa = eterm_bga.is_eterm_result.Substring(b, 3);
                                int page1 = Convert.ToInt32(aaa);//总页数
                                if (pages != page1)//有分页
                                {
                                    page_count = page1 - pages;
                                }
                            }
                        }
                        if (eterm_bga.is_eterm_result.IndexOf("PAGE ") > -1 && pn_num < page_count)
                        {
                            cmd = "PN";
                            eterm_bga.is_eterm_status = "Start";
                            eterm_bga.is_eterm_result = "";
                            eterm_bga.ib_dataflag = false;
                            eterm_fun.Eterm_comman(cmd);
                            pn_num += 1;
                            return;
                        }
                        else
                        {
                            page_count = 0;
                            if (pn_num <= 3 && ((eterm_bga.is_eterm_result.IndexOf(" +") >= 0) || (eterm_bga.is_eterm_result.IndexOf("+ ") >= 0)) && (eterm_bga.is_eterm_result.IndexOf(" + ") < 0))
                            {
                                eterm_bga.is_eterm_status = "Start";
                                eterm_bga.is_eterm_result = "";
                                eterm_bga.ib_dataflag = false;
                                eterm_fun.Eterm_comman("PN");
                                pn_num += 1;
                                return;
                            }
                        }
                        num_1 += 1;
                        eterm_bga.is_eterm_status = "Start";
                        eterm_bga.is_eterm_result = "";
                        eterm_bga.ib_dataflag = false;
                        timer2.Enabled = false;
                        timer3.Enabled = true;
                        return;
                    }

                }
            }
            catch (Exception)
            {


            }
        }

        /// <summary>
        /// 返回时间差（秒数）
        /// </summary>
        /// <param name="t1">当前时间</param>
        /// <param name="t2">给定时间</param>
        /// <returns></returns>
        private int getDiffSend(DateTime t1, DateTime t2)
        {
            TimeSpan ts1 = new TimeSpan(t1.Ticks);
            TimeSpan ts2 = new TimeSpan(t2.Ticks);
            TimeSpan ts = ts1.Subtract(ts2).Duration();
            return ts.Seconds;
        }

        /// <summary>
        /// 是否开机启动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startcheck_CheckedChanged(object sender, EventArgs e)
        {
            if (this.startcheck.Checked)
            {
                startup(true);
            }
            else
            {
                startup(false);
            }
        }

        /// <summary>
        /// 添加或移除注册表项
        /// </summary>
        /// <param name="add"></param>
        [RegistryPermissionAttribute(SecurityAction.LinkDemand, Write = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run")]
        private void startup(bool add)
        {
            try
            {
                string reg_name = ConfigurationManager.AppSettings["RegName"];
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (add)
                {
                    if (key.GetValue(reg_name) == null)
                    {
                        key.SetValue(reg_name, "\"" + Application.ExecutablePath + "\"");
                    }
                }
                else
                {
                    key.DeleteValue(reg_name);
                }
                key.Close();
            }
            catch (Exception ex)
            {
                writeFile.Write("注册表的启动增加或删除失败！" + ex.Message);
                return;
            }
        }

        private bool isstartup()
        {
            bool result = false;
            try
            {
                string reg_name = ConfigurationManager.AppSettings["RegName"];
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                result = key.GetValue(reg_name) != null;
                key.Close();
            }
            catch (Exception ex)
            {
                writeFile.Write("读取注册表失败！" + ex.Message);
                //MessageBox.Show("读取注册表失败！" + ex.Message, "提示");
            }
            return result;
        }
    }

}