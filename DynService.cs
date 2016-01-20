using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Services;

namespace Eterm_CS
{
    [System.Diagnostics.DebuggerStepThrough(), System.ComponentModel.DesignerCategory("code"), System.Web.Services.WebServiceBinding(Name = "WebDBHelp", Namespace = "WebDBHelp")]
    [ToolboxItem(false)]
    public class DynService : DBHelp.DBHelp
    {
        public DynService()
            : base()
        {
        }

        public DynService(string webUrl)
            : base()
        {
            if (string.IsNullOrEmpty(webUrl))
                this.Url = "http://localhost/rams/DBHelp.asmx";
            else
                this.Url = webUrl;
        }
    }
}
