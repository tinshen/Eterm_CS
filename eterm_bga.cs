using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eterm_CS
{
    public class eterm_bga
    {
        public static string is_eterm_status = "Start"; //隐藏窗口关闭默认为disconnect，指令开始或开始连接时为Start
        /// <summary>
        /// 指令返回结果
        /// </summary>
        public static string is_eterm_result = "";
        public static bool ib_dataflag = false;
        /// <summary>
        /// 断开连接
        /// </summary>
        public static Boolean ib_disconnect = false;
        /// <summary>
        /// 是否连接
        /// </summary>
        public static Boolean ib_connect_status = false;
        /// <summary>
        /// 指令
        /// </summary>
        public static string is_Command_str = "";
        public static int il_retry_count = 0;
        public static int il_error_count = 0;
        public static Boolean ib_flag = false;
    }
}
