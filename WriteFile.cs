using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;

namespace Eterm_CS
{
    public class WriteFile
    {
        public  void Write(string str)
        {
            string path=ConfigurationManager.AppSettings["PATH_STR"];
            if (!string.IsNullOrEmpty(path))
            {
                if(!Directory.Exists(path))
                    Directory.CreateDirectory(path);

            }
            FileStream fs = new FileStream(path+"\\error.txt", FileMode.Append,FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(DateTime.Now.ToString()+"     "+ str);
            sw.Close();
            fs.Close();
        }
    }
}
