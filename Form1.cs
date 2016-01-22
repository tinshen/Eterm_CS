using System;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Security.Permissions;
using System.Diagnostics;

namespace Eterm_CS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            winapi.statusbar = winapi.FindWindow("Shell_TrayWnd", "");
        }
        WriteFile writefile = new WriteFile();
        Random random;
        string WebURL, NetSet, startpath, user, pwd, connectionString;

        [DllImport("Iphlpapi.dll")]
        private static extern int SendARP(Int32 dest, Int32 host, ref Int32 mac, ref Int32 length);
        [DllImport("Ws2_32.dll")]
        private static extern Int32 inet_addr(string ip);

        private void Form1_Load(object sender, EventArgs e)
        {
            string str_encrypt, result, statu, str_ramdon, isWebConnection, rsakeypath;
            try
            {
                try
                {
                    WebURL = ConfigurationManager.AppSettings["WebUrl"];
                    NetSet = ConfigurationManager.AppSettings["NetSet"];
                    user = ConfigurationManager.AppSettings["UserName"];
                    pwd = ConfigurationManager.AppSettings["PassWord"];
                    isWebConnection = ConfigurationManager.AppSettings["WebC2"];
                    bga.gl_job = ConfigurationManager.AppSettings["JOB"];
                    connectionString = ConfigurationManager.AppSettings["ConnectionString"];
                    if (!string.IsNullOrEmpty(connectionString) && connectionString.IndexOf(";") > -1)
                    {
                        string db_name = string.Empty, db_user = string.Empty, db_pwb = string.Empty;
                        string db_name1 = string.Empty, db_user1 = string.Empty, db_pwb1 = string.Empty;
                        string[] strs = connectionString.Split(';');
                        db_name1 = strs[0].Substring(strs[0].IndexOf("=") + 1);
                        db_name = bga.DESDecrypt(db_name1, "hna2013_rams");//对数据库名解密
                        db_user1 = strs[1].Substring(strs[1].IndexOf("=") + 1);
                        db_user = bga.DESDecrypt(db_user1, "hna2013_rams");//对用户名解密
                        db_pwb1 = strs[2].Substring(strs[2].IndexOf("=") + 1);
                        db_pwb = bga.DESDecrypt(db_pwb1, "hna2013_rams");//对密码解密
                        connectionString = connectionString.Replace(db_name1, db_name);
                        connectionString = connectionString.Replace(db_user1, db_user);
                        connectionString = connectionString.Replace(db_pwb1, db_pwb);
                    }
                }
                catch
                {
                    writefile.Write("应用程序配置文件出错");
                    //MessageBox.Show("应用程序配置文件出错，请重新配置！", "提示");
                    Application.Exit();
                    return;
                }

                startpath = Application.StartupPath;
                //startpath = startpath.Substring(0, startpath.LastIndexOf('\\') - 3);
                rsakeypath = startpath + "publickey.pke";
                random = new Random();
                str_ramdon = random.Next(10000000, 99999999).ToString();

                DynService web = new DynService(WebURL);

                #region 网络配置
                if (string.IsNullOrEmpty(NetSet))
                {
                    writefile.Write("请配制网络连接内网或外网");
                    //MessageBox.Show("请配制网络连接内网或外网", "提示");
                    this.Close();
                    return;
                }
                else
                {
                    if (NetSet != "内网" && NetSet != "外网")
                    {
                        writefile.Write("请配制网络连接内网或外网");
                        //MessageBox.Show("请配制网络连接内网或外网", "提示");
                        this.Close();
                        return;
                    }
                    #region 内网连接
                    if (NetSet == "内网")
                    {

                        DataAccess.Path = "C2_DBHelp";
                        DataAccess.ClassName = "Rams.C2_DBHelp";
                        try
                        {
                            bga.db = DataAccess.Create();
                        }
                        catch (Exception ex)
                        {
                            writefile.Write("C2_DBHelp程序集错误：" + ex.Message);
                            return;
                        }
                        //返回数据连接结果
                        if (!string.IsNullOrEmpty(connectionString))
                        {
                            if (bga.db.GetConnection(connectionString) != "OK")
                            {
                                this.Close();
                                return;
                            }
                        }
                        else
                        {

                            #region  通过Web服务器返回连接字符串
                            if (!string.IsNullOrEmpty(isWebConnection))
                            {
                                //用RSA对字符串进行加密
                                string connstr;
                                str_encrypt = "<user>" + user.ToUpper() + "</user><pwd>" + pwd.ToUpper() + "</pwd><random>" + str_ramdon + "</random><flag>C2</flag><DoMain>FALSE</DoMain>";
                                str_encrypt = bga.RSAEncrypt(str_encrypt, rsakeypath);
                                try
                                {
                                    connstr = web.UserValidate(str_encrypt);
                                }
                                catch (Exception ex)
                                {
                                    writefile.Write("Web服务错误：" + ex.Message);
                                    //MessageBox.Show("Web服务错误：" + ex.Message, "提示");
                                    return;
                                }

                                statu = bga.GetString(connstr, "stu");
                                if (statu != "OK")
                                {
                                    writefile.Write(statu);
                                    //MessageBox.Show(statu, "提示");
                                    return;
                                }
                                connstr = bga.GetString(connstr, "str");
                                connstr = bga.DESDecrypt(connstr, str_ramdon);
                                connstr = connstr.Substring(connstr.IndexOf(';') + 1);
                                connstr = "Data Source=" + isWebConnection + ";" + connstr;
                                if (bga.db.GetConnection(connstr) != "OK")
                                    return;
                            }

                            #endregion
                        }
                    }
                    #endregion
                    #region 外网连接
                    else if (NetSet == "外网")
                    {
                        string sessionid, newrandom;
                        #region 数据库连接初始化
                        DataAccess.Path = "C3_DBHelp";
                        DataAccess.ClassName = "Rams.C3_DBHelp";
                        try
                        {
                            bga.db = DataAccess.Create();
                        }
                        catch (Exception ex)
                        {
                            writefile.Write("C3_DBHelp程序集错误:" + ex.Message);
                            //MessageBox.Show("C3_DBHelp程序集错误:" + ex.Message, "提示");
                            return;
                        }
                        #endregion
                        str_encrypt = "<user>" + user.ToUpper() + "</user><pwd>" + pwd.ToUpper() + "</pwd><random>" + str_ramdon + "</random><ip>" + bga.IP + "</ip><mac>" + bga.MAC + "</mac><flag>C3</flag><DoMain>FALSE</DoMain>";
                        //用RSA对字符串进行加密
                        str_encrypt = bga.RSAEncrypt(str_encrypt, rsakeypath);
                        try
                        {
                            result = web.UserValidate(str_encrypt);
                            statu = bga.GetString(result, "stu");
                        }
                        catch (Exception ex)
                        {
                            writefile.Write("Web服务错误:" + ex.Message);
                            //MessageBox.Show("Web服务错误：" + ex.Message, "提示");
                            return;
                        }

                        if (statu != "OK")
                        {
                            writefile.Write(statu);
                            //MessageBox.Show(statu);
                            return;
                        }
                        result = bga.GetString(result, "str");
                        //对字符串进行解密
                        //用DES,str_ramdom进行解密                
                        string sDec = bga.DESDecrypt(result, str_ramdon);
                        sessionid = bga.GetString(sDec, "sessionid");
                        newrandom = bga.GetString(sDec, "random");
                        newrandom = bga.DESEncrypt(newrandom, str_ramdon);
                        bga.SessionID = sessionid;
                        bga.Secret = "<sessionid>" + bga.SessionID + "</sessionid><random>" + newrandom + "</random>";
                        bga.db.GetConnection(str_ramdon + ";" + bga.Secret);

                    }
                    #endregion
                }
                #endregion

                var sql = "select id_code from d_sub_base_info";
                DataTable dt = bga.db.ExecuteDataSet(sql).Tables[0];
                if (dt.Rows.Count > 0)
                    bga.id_code = dt.Rows[0][0].ToString();
                bga.stat_code = "D";
                bga.logid = user;

                this.Hide();
                Tray.ShowBalloonTip(5);
                this.Tray.MouseDoubleClick += new MouseEventHandler(Tray_MouseDoubleClick);
                this.Hide();
                Tray_MouseDoubleClick(sender, e);
            }
            catch (Exception ex)
            {
                writefile.Write("Load错误信息：" + ex.Message);
            }

        }

        /// <summary>
        /// 通过IP获取MAC地址
        /// </summary>
        /// <returns></returns>
        private string GetMACFromIP(string A_strIP)
        {
            string strRet = "Unknown";

            string strIPPattern = @"^\d+\.\d+\.\d+\.\d+$";

            Regex objRex = new Regex(strIPPattern);

            if (objRex.IsMatch(A_strIP) == true)
            {
                Int32 intDest = inet_addr(A_strIP);
                Int32[] arrMAC = new Int32[2];
                Int32 intLen = 6;

                int intResult = SendARP(intDest, 0, ref   arrMAC[0], ref   intLen);

                if (intResult == 0)
                {
                    Byte[] arrbyte = new Byte[8];
                    arrbyte[5] = (Byte)(arrMAC[1] >> 8);
                    arrbyte[4] = (Byte)arrMAC[1];
                    arrbyte[3] = (Byte)(arrMAC[0] >> 24);
                    arrbyte[2] = (Byte)(arrMAC[0] >> 16);
                    arrbyte[1] = (Byte)(arrMAC[0] >> 8);
                    arrbyte[0] = (Byte)arrMAC[0];

                    StringBuilder strbMAC = new StringBuilder();

                    for (int intIndex = 0; intIndex < 6; intIndex++)
                    {
                        if (intIndex > 0) strbMAC.Append("-");
                        strbMAC.Append(arrbyte[intIndex].ToString("X2"));
                    }
                    strRet = strbMAC.ToString();
                }
            }

            return strRet;
        }

        /// <summary>
        /// 双击通知区域图标,打开FrmMain窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tray_MouseDoubleClick(object sender, EventArgs e)
        {
            //打开窗体时，取一下主机名和Eterm账号信息
            string ls_eterm = ConfigurationManager.AppSettings["Eterm_STR"];
            string Eterm_Host = string.Empty, Eterm_User = string.Empty;
            if (string.IsNullOrEmpty(ls_eterm))
            {
                if (DateTime.Now.Day > 15)
                    ls_eterm = ConfigurationManager.AppSettings["Eterm_STR1"];
                else
                    ls_eterm = ConfigurationManager.AppSettings["Eterm_STR2"];
            }
            if (!string.IsNullOrEmpty(ls_eterm) && ls_eterm.IndexOf(";") > -1)
            {
                string eterm_host1 = string.Empty, eterm_host = string.Empty;
                string eterm_user1 = string.Empty, eterm_user = string.Empty;
                String[] strs = ls_eterm.Split(';');
                eterm_host1 = strs[0].Substring(strs[0].IndexOf("=") + 1);
                Eterm_Host = bga.DESDecrypt(eterm_host1, "hna2013_rams");//对服务器名解密
                eterm_user1 = strs[2].Substring(strs[2].IndexOf("=") + 1);
                Eterm_User = bga.DESDecrypt(eterm_user1, "hna2013_rams");//对密码解密
            }
            string ls_message = "订座系统(" + bga.gl_job + "[" + Eterm_Host + "  -  " + Eterm_User + "])";
            if (bga.frm_main == null)
            {
                FrmMain opt = new FrmMain();
                opt.Text = ls_message;
                opt.SetFrmText("系统启动，初始化中...");
                if (opt.IsOpen)
                    return;
                opt.Icon = Properties.Resources._10;
                bga.frm_main = opt;
                opt.ShowDialog();
            }
            else
            {
                try
                {
                    if(!bga.frm_main.Visible)
                    {
                        bga.frm_main.WindowState = System.Windows.Forms.FormWindowState.Normal;
                        bga.frm_main.Text = ls_message;
                        bga.frm_main.ShowDialog();
                    }     
                }
                catch (Exception ex)
                {
                    writefile.Write("MouseDoubleClick错误信息：" + ex.Message);
                }
            }
        }

        private void Exititem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要退出订座系统吗？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            Tray.Visible = false;
            foreach (Process pro in Process.GetProcesses())
            {
                if (pro.ProcessName.IndexOf("Eterm_CS") > -1)
                {
                    pro.Kill();
                }
            }
            Application.Exit();
        }
    }
}
