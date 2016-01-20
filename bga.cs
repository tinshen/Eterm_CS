using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rams;
using System.Security.Cryptography;
using System.IO;
using System.Windows.Forms;
using System.Data;

namespace Eterm_CS
{
    public static class bga
    {
        public static IDBHelp db;
        public static string IP;
        public static string MAC;
        public static string SessionID;
        public static string Secret;
        public static string id_code;
        public static string stat_code;
        public static string logid;
        public static DataTable backgroup_plan;
        public static string gl_job = string.Empty;
        public static Form frm_main = null;
        public static System.Windows.Forms.WebBrowser webbrowser = null;        

        #region 加密
        /// <summary>
        /// 由于RSA的rgb参数不能超过字符串长度为117，进行分截加密,获取公钥
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns></returns>
        public static string RSAEncrypt(string str, string publcikeypath)
        {
            string ls_str = string.Empty;
            string ls_temp = string.Empty;
            string ls_encrypt_str = string.Empty;
            string publickey;
            byte[] byte_encrypt;
            int ll_num = 0;
            ls_str = str;
            System.Text.Encoding utf8 = new System.Text.UTF8Encoding();
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            try
            {
                StreamReader reader = new StreamReader(publcikeypath);
                publickey = reader.ReadToEnd();
                RSA.FromXmlString(publickey);
            }
            catch (Exception ex)
            {
                MessageBox.Show("加密文件没有在当前执行目录：" + ex.Message);
                return string.Empty;
            }

            if (ls_str == string.Empty || ls_str == "")
                return ls_encrypt_str;
            do
            {
                if (ls_str.Length > 100)
                {
                    ls_temp = ls_str.Substring(0, 100);
                    ls_str = ls_str.Substring(100);
                    byte_encrypt = utf8.GetBytes(ls_temp);
                    byte_encrypt = RSA.Encrypt(byte_encrypt, false);
                    ls_temp = System.Convert.ToBase64String(byte_encrypt);
                    ll_num = ll_num + 1;
                    ls_encrypt_str = ls_encrypt_str + "<" + ll_num.ToString() + ">" + ls_temp + "</" + ll_num.ToString() + ">";
                }
                else
                {
                    ls_temp = ls_str;
                    byte_encrypt = utf8.GetBytes(ls_temp);
                    byte_encrypt = RSA.Encrypt(byte_encrypt, false);
                    ls_temp = System.Convert.ToBase64String(byte_encrypt);
                    ll_num = ll_num + 1;
                    ls_encrypt_str = ls_encrypt_str + "<" + ll_num.ToString() + ">" + ls_temp + "</" + ll_num.ToString() + ">";
                    break;
                }

            } while (2 > 1);
            return ls_encrypt_str;

        }

        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string DESEncrypt(string str, string key)
        {
            string value = string.Empty;
            if (key.Length < 8)
                key = key.PadRight(8, '0');
            else if (key.Length > 8)
                key = key.Substring(0, 8);
            System.Text.Encoding utf8 = new System.Text.UTF8Encoding();
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            if (str == string.Empty || str == null)
                return value;
            byte[] byte_key = utf8.GetBytes(key);
            byte[] iv = utf8.GetBytes(key);
            try
            {
                ICryptoTransform decryptor = des.CreateEncryptor(byte_key, iv);
                byte[] byte_str = utf8.GetBytes(str);
                byte_str = decryptor.TransformFinalBlock(byte_str, 0, byte_str.Length);
                value = System.Convert.ToBase64String(byte_str);
            }
            catch
            {
            }

            return value;
        }
        /// <summary>
        /// DES解密.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string DESDecrypt(string str, string key)
        {
            string value = string.Empty;
            if (key.Length < 8)
                key = key.PadRight(8, '0');
            else if (key.Length > 8)
                key = key.Substring(0, 8);
            System.Text.Encoding utf8 = new System.Text.UTF8Encoding();
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            if (str == string.Empty || str == null)
                return value;
            byte[] byte_key = utf8.GetBytes(key);
            byte[] iv = utf8.GetBytes(key);
            try
            {
                ICryptoTransform decryptor = des.CreateDecryptor(byte_key, iv);
                byte[] byte_str = System.Convert.FromBase64String(str);
                byte_str = decryptor.TransformFinalBlock(byte_str, 0, byte_str.Length);
                value = utf8.GetString(byte_str);
            }
            catch
            {

            }

            return value;
        }

        #endregion
        /// <summary>
        ///字符串截取
        /// </summary>
        /// <param name="str">要截取字符串.</param>
        /// <param name="condition">截取条件.</param>
        /// <returns></returns>
        public static string GetString(string str, string condition)
        {
            string result = string.Empty;
            if (str != string.Empty && condition != string.Empty)
            {
                int first1 = str.IndexOf("<" + condition + ">");
                int first2 = str.LastIndexOf("</" + condition + ">");

                if (first1 > -1)
                {
                    if (first2 > -1)
                    {
                        result = str.Substring(first1 + 2 + condition.Length, first2 - (first1 + 2 + condition.Length));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 得到指定日期前多少天或后多少天的日期
        /// </summary>
        /// <param name="dt">指定日期</param>
        /// <param name="i">-+天数</param>
        /// <returns>指定日期前多少天或后多少天的日期</returns>
        public static DateTime RelativeDate(DateTime dt, int i)
        {
            int DaysInMonth = DateTime.DaysInMonth(dt.Year, dt.Month);
            if (i == 0)
                return dt;
            else
                return dt.AddDays(i);
        }

        public static bool IsNumber(string str)
        {
            bool result = false;
            foreach (Char c in str)
            {
                if (char.IsNumber(c) == true)
                    result = true;
                else
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        public static string File_Clob_Transfer(string a_user_code, string a_file_name, string a_id_code, string a_iss_co, string a_deal_type, DateTime a_deal_time, string a_txt_encode, string a_oth_str, string a_flag)
        {
            string ls_status, ls_file_name, ls_co_2c = string.Empty, ls_input, ls_output;

            int ll_1 = 0, max_int = 2147483647;
            //int ll_1 = 0, count = 10;

            string trans_str = string.Empty, ls_update_str = string.Empty;
            FileStream fs;
            DataTable dt_temp = null;

            ls_status = "OK";
            ls_file_name = a_file_name.ToUpper();
            Encoding encoding = Encoding.GetEncoding(936);
            var sql = "select count(*)  from d_sys_task_manage where ip_address='" + bga.IP + "'||'-'||'" + bga.SessionID + "' and  submit_user='" + bga.logid + "' and id_code='" + bga.id_code + "' and (status='等待' or status='运行')";
            ll_1 = Int32.Parse(bga.db.ExecuteDataSet(sql).Tables[0].Rows[0][0].ToString());
            if (ll_1 > 0)
            {
                ls_status = "当前进程还有" + ll_1.ToString() + "个事务再处理,请等待!";
                return ls_status;
            }
            sql = "select co_2c from d_sub_base_info where id_code='" + a_id_code + "' and co_3c='" + a_iss_co + "'";
            dt_temp = bga.db.ExecuteDataSet(sql).Tables[0];
            if (dt_temp != null && dt_temp.Rows.Count > 0)
                ls_co_2c = dt_temp.Rows[0][0].ToString();

            if (a_txt_encode == "UTF8")
            {
                ls_file_name = a_file_name + ".ASCII";
                FileEncodeConvert(a_file_name, Encoding.UTF8, Encoding.ASCII, ls_file_name);
            }
            try
            {
                //a_file_name = @"D:\temp\agreement_fare_HU.xml";
                fs = File.OpenRead(ls_file_name);
            }
            catch (Exception ex)
            {
                ls_status = ex.Message + "打开文件失败";
                return ls_status;
            }

            if (fs.Length > max_int)
            {
                return "超出系统传输文件的最大值：1.5G！";
            }

            sql = "delete dm_blob_tmp where user_session='" + bga.logid + "'and use_type='" + ls_file_name + "' and serial_no='" + Fill_0("1", 3) + "'";
            bga.db.ExecuteNonQuery(sql);
            sql = "insert into dm_blob_tmp values('" + bga.logid + "','" + ls_file_name + "','" + Fill_0("1", 3) + "','','')";
            bga.db.ExecuteNonQuery(sql);
            sql = "select * from dm_blob_tmp where user_session='" + bga.logid + "'and use_type='" + ls_file_name + "' and serial_no='" + Fill_0("1", 3) + "'";
            DataSet ds = bga.db.ExecuteDataSet(sql);
            if (ds == null && ds.Tables.Count == 0 && ds.Tables[0].Rows.Count == 0)
                return "文件传输出错：插入错误";
            byte[] buttfly = new byte[fs.Length];
            fs.Read(buttfly, 0, (int)fs.Length);
            fs.Close();
            ds.Tables[0].Rows[0]["blob_str"] = buttfly;
            ls_update_str = bga.db.UpdateDataSet(ds, "select * from dm_blob_tmp", "Table");
            if (ls_update_str == "OK")
                ls_status = "OK";
            else
                ls_status = "文件传输出错：" + ls_update_str;


            if (ls_status != "OK")
                return "文件传输出错：" + ls_status;
            ls_input = "<1><N>a_user_code</N><T>S</T><V>" + a_user_code + "</V></1>" +
                   "<2><N>a_file_name</N><T>S</T><V>" + a_file_name + "</V></2>" +
                   "<3><N>a_run_type</N><T>S</T><V>" + a_flag + "</V></3>" +
                   "<4><N>a_file_date</N><T>D</T><V>" + a_deal_time + "</V></4>" +
                   "<5><N>a_deal_type</N><T>S</T><V>" + a_deal_type + "</V></5>" +
                   "<6><N>a_id_code</N><T>S</T><V>" + a_id_code + "</V></6>" +
                   "<7><N>a_co_2c</N><T>S</T><V>" + ls_co_2c + "</V></7>" +
                   "<8><N>a_co_3c</N><T>S</T><V>" + a_iss_co + "</V></8>" +
                   "<9><N>a_oth_str</N><T>S</T><V>" + a_oth_str + "</V></9>";

            ls_output = "<1><N>b_status</N></1>";
            ls_status = GetString(bga.db.ExecutePRC("pack_file_clob_transfer.file_parse", ls_output, ls_input), "1");
            return ls_status;
        }

        public static string FileEncodeConvert(string srcpath, Encoding srcEncoding, Encoding dstEncoding, string dstpath)
        {
            string result = "OK", input;
            if (!File.Exists(srcpath))
            {
                result = "文件不存在！";
                return result;
            }
            try
            {
                StreamReader reader = new StreamReader(srcpath, srcEncoding);
                input = reader.ReadToEnd();
                reader.Close();
                StreamWriter writer = new StreamWriter(dstpath, false, dstEncoding);
                writer.Write(input);
                writer.Close();
            }
            catch (Exception ex)
            {
                result = "编码转换失败" + ex.Message;
            }
            return result;
        }

        public static string Fill_0(string source, int length)
        {
            int ll_1 = source.Length;
            for (int i = 0; i < length - ll_1; i++)
            {
                source = "0" + source;
            }
            return source;
        }
    }
}
