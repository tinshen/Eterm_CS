using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eterm_CS
{
    public class eterm_bga
    {
        /// <summary>
        /// Eterm窗口状态，有Start，disconnect
        /// </summary>
        public static string is_eterm_status = "Start"; //隐藏窗口关闭默认为disconnect，指令开始或开始连接时为Start
        /// <summary>
        /// 指令返回结果
        /// </summary>
        public static string is_eterm_result = "";
        public static bool ib_dataflag = false;
        /// <summary>
        /// 是否需要断开连接，设置为true,则Eterm窗口会关闭。
        /// </summary>
        public static Boolean ib_disconnect = false;
        /// <summary>
        /// 是否已连接
        /// </summary>
        public static Boolean ib_connect_status = false;
        /// <summary>
        /// 指令
        /// </summary>
        public static string is_Command_str = "";
        /// <summary>
        /// 重复尝试次数
        /// </summary>
        public static int il_retry_count = 0;
        /// <summary>
        /// 错误计数
        /// </summary>
        public static int il_error_count = 0;
        public static Boolean ib_flag = false;
    }
}
