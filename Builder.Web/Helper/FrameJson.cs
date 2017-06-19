using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Builder.Web.Helper
{
    public class FrameJson
    {
        public static string JsonFormat(string errorcode, string errormsg, string content)
        {
            StringBuilder sbrtn = new StringBuilder();
            sbrtn.Append(@"{""ErrorCode"":""" + errorcode + @""",""ErrorMsg"":""" + errormsg + @""",""Content"":""" + content + @"""}");

            return sbrtn.ToString();
        }
        public static string JsonFormat(string json)
        {
            StringBuilder sbrtn = new StringBuilder();
            sbrtn.Append(@"{""ErrorCode"":"""",""ErrorMessage"":"""",""Content"":" + json + "}");

            return sbrtn.ToString();
        }

        public static string JsonFormat(Exception ex)
        {
            string errorcode = "E-" + ComFunc.nvl(ComFunc.nvl(MyConfig.GetConfiguration("Machine_No"))) + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            var rtn = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
            rtn.ErrorCode = errorcode;
            rtn.ErrorMessage = ex.Message;

            return ((FrameDLRObject)rtn).ToJSONString();
        }
    }
}
