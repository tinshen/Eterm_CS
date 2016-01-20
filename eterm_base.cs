using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DrveTerm;
using OleBase;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Configuration;
using Eterm_CS;

namespace eterm_base
{
    public partial class eterm_bases : Form
    {
        public const int WM_DATA = 0x0401;
        public const int WM_CTX = 0x0402;
        public const int WM_MESSAGE = 0x0403;
        public IntPtr hWnd;
        public string Eterm_host;
        public int Eterm_port;
        public string Eterm_user;
        public string Eterm_pwd;
        public string Eterm_code;
        public string Eterm_first_comm;
        //public string is_status;
        //public string is_connect_status;
        //public string is_show_txt;

        public struct COPYDATASTRUCT
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string bstrCategory;
            [MarshalAs(UnmanagedType.LPStr)]
            public string bstrToken;
            [MarshalAs(UnmanagedType.LPStr)]
            public string bstrData;
        }

        [DllImport("User32.dll", EntryPoint = "SendMessageA")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, object lParam);
        [DllImport("User32.dll", EntryPoint = "SendMessageA")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, ref  COPYDATASTRUCT lParam);

        protected override void DefWndProc(ref System.Windows.Forms.Message m)
        {
            string token = "";
            COPYDATASTRUCT mystr;
            Type mytype = null;
            //MessageBox.Show(m.Msg.ToString());
            switch (m.Msg)
            {
                case WM_MESSAGE:
                    mystr = new COPYDATASTRUCT();
                    mytype = mystr.GetType();
                    mystr = (COPYDATASTRUCT)m.GetLParam(mytype);
                    token = mystr.bstrData;

                    eterm_bga.is_eterm_status = mystr.bstrData;
                    eterm_bga.ib_connect_status = false;
                    eterm_bga.ib_dataflag = false;
                    //    MessageBox.Show(this, mystr.bstrData, mystr.bstrCategory);
                    break;

                case WM_DATA:
                    mystr = new COPYDATASTRUCT();
                    mytype = mystr.GetType();
                    mystr = (COPYDATASTRUCT)m.GetLParam(mytype);
                    token = mystr.bstrData;

                    ShowData(token);
                    break;
                case WM_CTX:
                    mystr = new COPYDATASTRUCT();
                    mytype = mystr.GetType();
                    mystr = (COPYDATASTRUCT)m.GetLParam(mytype);
                    token = mystr.bstrToken;
                    CreatingCtx((int)m.WParam, token);
                    break;
                default:
                   base.DefWndProc(ref m);
                   if (eterm_bga.ib_connect_status)
                   {
                       eterm_bga.is_eterm_status = "Host system down";
                       eterm_bga.ib_connect_status = false;
                       eterm_bga.ib_dataflag = false;
                   }
                    
                    break;
            }
        }
        public eterm_bases()
        {
            InitializeComponent();
        }

        private void eterm_base_Load(object sender, EventArgs e)
        {

            int ll_1;
            int ll_2;
            int ll_3;
            string ls_temp_str, ls_temp;
            hWnd = this.Handle;
            Eterm_host = "";
            Eterm_port = 0;
            Eterm_user = "";
            Eterm_pwd = "";
            Eterm_code = "";
            Eterm_first_comm = "";

            eterm_bga.is_eterm_status = "Start";
            ls_temp_str = ConfigurationManager.AppSettings["Eterm_STR"];
            if (string.IsNullOrEmpty(ls_temp_str))
            {
                if (DateTime.Now.Day > 15)
                    ls_temp_str = ConfigurationManager.AppSettings["Eterm_STR1"];
                else
                    ls_temp_str = ConfigurationManager.AppSettings["Eterm_STR2"];
            }
            if (!string.IsNullOrEmpty(ls_temp_str) && ls_temp_str.IndexOf(";") > -1)
            {
                string eterm_host1 = string.Empty, eterm_host = string.Empty, eterm_port1 = string.Empty, eterm_port = string.Empty;
                string eterm_user1 = string.Empty, eterm_user = string.Empty, eterm_pwd1 = string.Empty, eterm_pwd = string.Empty;
                string eterm_first_comm1 = string.Empty, eterm_first_comm = string.Empty;
                String[] strs = ls_temp_str.Split(';');
                eterm_host1 = strs[0].Substring(strs[0].IndexOf("=") + 1);
                eterm_host = bga.DESDecrypt(eterm_host1, "hna2013_rams");//对数据库名解密
                eterm_port1 = strs[1].Substring(strs[1].IndexOf("=") + 1);
                eterm_port = bga.DESDecrypt(eterm_port1, "hna2013_rams");//对用户名解密
                eterm_user1 = strs[2].Substring(strs[2].IndexOf("=") + 1);
                eterm_user = bga.DESDecrypt(eterm_user1, "hna2013_rams");//对密码解密
                eterm_pwd1 = strs[3].Substring(strs[3].IndexOf("=") + 1);
                eterm_pwd = bga.DESDecrypt(eterm_pwd1, "hna2013_rams");//对密码解密
                eterm_first_comm1 = strs[4].Substring(strs[4].IndexOf("=") + 1);
                eterm_first_comm = bga.DESDecrypt(eterm_first_comm1, "hna2013_rams");//对密码解密
                ls_temp_str = ls_temp_str.Replace(eterm_host1, eterm_host);
                ls_temp_str = ls_temp_str.Replace(eterm_port1, eterm_port);
                ls_temp_str = ls_temp_str.Replace(eterm_user1, eterm_user);
                ls_temp_str = ls_temp_str.Replace(eterm_pwd1, eterm_pwd);
                ls_temp_str = ls_temp_str.Replace(eterm_first_comm1, eterm_first_comm);
            }
            ll_1 = ls_temp_str.IndexOf("host", 0); //找HOST
            if (ll_1 >= 0)
            {
                ll_2 = ls_temp_str.IndexOf("=", ll_1 + 1);
                if (ll_2 > 0)
                {
                    ll_3 = ls_temp_str.IndexOf(";", ll_2 + 1);
                    if (ll_3 > ll_2)
                    {
                        Eterm_host = ls_temp_str.Substring(ll_2 + 1, ll_3 - ll_2 - 1).Trim();
                    }
                    else
                    {
                        Eterm_host = ls_temp_str.Substring(ll_2 + 1).Trim();
                    }
                }
            }

            ll_1 = ls_temp_str.IndexOf("port", 0); //找PORT
            if (ll_1 >= 0)
            {
                ll_2 = ls_temp_str.IndexOf("=", ll_1 + 1);
                if (ll_2 > 0)
                {
                    ll_3 = ls_temp_str.IndexOf(";", ll_2 + 1);
                    if (ll_3 > ll_2)
                    {
                        Eterm_port = int.Parse(ls_temp_str.Substring(ll_2 + 1, ll_3 - ll_2 - 1).Trim());
                    }
                    else
                    {
                        Eterm_port = int.Parse(ls_temp_str.Substring(ll_2 + 1).Trim());
                    }
                }
            }

            ll_1 = ls_temp_str.IndexOf("user", 0); //找user
            if (ll_1 >= 0)
            {
                ll_2 = ls_temp_str.IndexOf("=", ll_1 + 1);
                if (ll_2 > 0)
                {
                    ll_3 = ls_temp_str.IndexOf(";", ll_2 + 1);
                    if (ll_3 > ll_2)
                    {
                        Eterm_user = ls_temp_str.Substring(ll_2 + 1, ll_3 - ll_2 - 1).Trim();
                    }
                    else
                    {
                        Eterm_user = ls_temp_str.Substring(ll_2 + 1).Trim();
                    }
                }
            }

            ll_1 = ls_temp_str.IndexOf("pwd", 0); //找pwd
            if (ll_1 >= 0)
            {
                ll_2 = ls_temp_str.IndexOf("=", ll_1 + 1);
                if (ll_2 > 0)
                {
                    ll_3 = ls_temp_str.IndexOf(";", ll_2 + 1);
                    if (ll_3 > ll_2)
                    {
                        Eterm_pwd = ls_temp_str.Substring(ll_2 + 1, ll_3 - ll_2 - 1).Trim();
                    }
                    else
                    {
                        Eterm_pwd = ls_temp_str.Substring(ll_2 + 1).Trim();
                    }
                }
            }

            ll_1 = ls_temp_str.IndexOf("code", 0); //找code
            if (ll_1 >= 0)
            {
                ll_2 = ls_temp_str.IndexOf("=", ll_1 + 1);
                if (ll_2 > 0)
                {
                    ll_3 = ls_temp_str.IndexOf(";", ll_2 + 1);
                    if (ll_3 > ll_2)
                    {
                        Eterm_code = ls_temp_str.Substring(ll_2 + 1, ll_3 - ll_2 - 1).Trim();
                    }
                    else
                    {
                        Eterm_code = ls_temp_str.Substring(ll_2 + 1).Trim();
                    }
                }
            }


            ll_1 = ls_temp_str.IndexOf("first_comm", 0); //找first_comm
            if (ll_1 >= 0)
            {
                ll_2 = ls_temp_str.IndexOf("=", ll_1 + 1);
                if (ll_2 > 0)
                {
                    ll_3 = ls_temp_str.IndexOf(";", ll_2 + 1);
                    if (ll_3 > ll_2)
                    {
                        Eterm_first_comm = ls_temp_str.Substring(ll_2 + 1, ll_3 - ll_2 - 1).Trim();
                    }
                    else
                    {
                        Eterm_first_comm = ls_temp_str.Substring(ll_2 + 1).Trim();
                    }
                }
            }


            // FormConnection cc = new FormConnection();
            // cc.textBox1.Text = "eterm.hnair.com";
            // cc.textBox2.Text = "350";

            if ((Eterm_host.Length < 1) || (Eterm_port == 0) || (Eterm_user.Length < 1) || (Eterm_pwd.Length < 1))
            {
                eterm_connect dlg = new eterm_connect();
                dlg.Eterm_host = Eterm_host;
                dlg.Eterm_port = Eterm_port.ToString();
                dlg.Eterm_user = Eterm_user;
                dlg.Eterm_pwd = Eterm_pwd;
                if (dlg.ShowDialog() != DialogResult.OK)
                {
                    Close();
                    return;
                }
                Eterm_host = dlg.Eterm_host;
                Eterm_port = int.Parse(dlg.Eterm_port.Trim());
                Eterm_user = dlg.Eterm_user;
                Eterm_pwd = dlg.Eterm_pwd;

                dlg.Dispose();

            }
            try
            {
                eTermFactory = new DrveTerm.MatipFactoryClass();
                eTermFactory._IDrvFactoryEvents_Event_OnConnected += new _IDrvFactoryEvents_OnConnectedEventHandler(Factory_OnConnected);
                eTermFactory._IDrvFactoryEvents_Event_OnConnectParameters += new _IDrvFactoryEvents_OnConnectParametersEventHandler(Factory_OnConnectParameters);
                eTermFactory._IDrvFactoryEvents_Event_OnCtxCreated += new _IDrvFactoryEvents_OnCtxCreatedEventHandler(Factory_OnCtxCreated);
                eTermFactory._IDrvFactoryEvents_Event_OnDisconnected += new _IDrvFactoryEvents_OnDisconnectedEventHandler(Factory_OnDisconnected);
                eTermFactory.Connect(Eterm_host, Eterm_port, "");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

           

        }

        public void ShowMessageBox(string a1, string a2)
        {
            COPYDATASTRUCT cds = new COPYDATASTRUCT();
            cds.bstrCategory = a2;
            cds.bstrData = a1;
            SendMessage(hWnd, WM_MESSAGE, 0, ref cds);
        }

        public void Factory_OnConnected(int nCode, string bstrStatusMessage)
        {
            if (nCode != 0)
            {

                eterm_bga.is_eterm_status = "connection error:" + bstrStatusMessage;
                Close();
            }
            else
            {

                eterm_bga.is_eterm_status = "connection successed";
                eterm_bga.ib_connect_status = true;
                eterm_bga.il_error_count = 0;
                eterm_bga.il_retry_count = 0;

            }
        }

        public void Factory_OnConnectParameters(object pVal)
        {
            OleBase.Collection coll = (OleBase.Collection)pVal;


            coll.set_StringItem("USER", Eterm_user);
            coll.set_StringItem("PWD", Eterm_pwd);
            coll.set_StringItem("VALIDATECODE", Eterm_code);


        }

        public void CreatingCtx(int type, string bstrToken)
        {
            if (type == 0)
            {
                if (eTermCrsCtx != null)
                {
                    return;
                }

                eTermCrsCtx = new DrvCtxWrap(eTermFactory.CtxList[bstrToken]);

                eTermCrsCtx._IDrvCtxEvents_Event_OnAsyncData += new _IDrvCtxEvents_OnAsyncDataEventHandler(CRS_Ctx_OnAsyncData);
                eTermCrsCtx._IDrvCtxEvents_Event_OnNotify += new _IDrvCtxEvents_OnNotifyEventHandler(CRS_Ctx_OnNotify);
                eTermCrsCtx._IDrvCtxEvents_Event_OnStatus += new _IDrvCtxEvents_OnStatusEventHandler(CRS_Ctx_OnStatus);
            }
        }

        public void Factory_OnCtxCreated(string bstrCategory, string bstrToken)
        {
            COPYDATASTRUCT cds = new COPYDATASTRUCT();
            cds.bstrCategory = bstrCategory;
            cds.bstrToken = bstrToken;

            if (bstrCategory == "0000")
            {
                if (eTermCrsCtx != null)
                {
                    return;
                }

                SendMessage(hWnd, WM_CTX, 0, ref cds);
            }
        }

        public void Factory_OnDisconnected(string bstrStatusMessage)
        {


            eterm_bga.is_eterm_status = "Disconnect";

            //  MessageBox.Show("Disconnect");
            Close();
        }

        public void ShowData(string bstrData)
        {


            if (eterm_bga.is_eterm_result.Length != 0)
            {
                eterm_bga.is_eterm_result = eterm_bga.is_eterm_result + "\r\n";

            }
            bstrData = bstrData.Replace("\n", "\r");
            bstrData = bstrData.Replace("\r", "\r\n");


            eterm_bga.is_eterm_result = eterm_bga.is_eterm_result + bstrData;
            eterm_bga.ib_dataflag = true;
        }

        public void CRS_Ctx_OnAsyncData(string bstrData, string bstrProperties)
        {
            COPYDATASTRUCT cds = new COPYDATASTRUCT();
            cds.bstrData = bstrData;
            SendMessage(hWnd, WM_DATA, 0, ref cds);
        }

        public void CRS_Ctx_OnNotify(int nNotify, string bstrNotifyMessage)
        {
            ShowMessageBox(bstrNotifyMessage, "Notify :" + nNotify);
        }

        public void CRS_Ctx_OnStatus(int nStatus, string bstrStatusMessage)
        {
            ShowMessageBox(bstrStatusMessage, "Status :" + nStatus);
        }

        private DrveTerm.MatipFactoryClass eTermFactory = null;
        private DrvCtxWrap eTermCrsCtx = null;

        public void command_exe(string Command_text)
        {
            try
            {
                if (eTermCrsCtx != null)
                {
                    eTermCrsCtx.GetCtx().AsyncSend(Command_text);
                }
            }
            catch 
            {
                
            }
            
        }


        private void eterm_base_FormClosing(object sender, FormClosingEventArgs e)
        {
            eterm_bga.ib_connect_status = false;
            eterm_bga.is_eterm_status = "disconnect";
            eTermFactory.Disconnect();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (eterm_bga.ib_disconnect)
            {
                eterm_bga.ib_disconnect = false;
                this.Close();
            }
        }
    }
}
