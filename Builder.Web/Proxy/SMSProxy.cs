using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Module;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using EFFC.Frame.Net.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace Builder.Web.Proxy
{
    public class SMSProxy : HttpRemoteProxy<WebParameter, WebBaseData>
    {
        protected override void ProcessAfterRequest(EFFC.Frame.Net.Base.Data.Base.FrameDLRObject responseobj, WebParameter p, WebBaseData d)
        {
            var xmlstr = ComFunc.nvl(responseobj.GetValue("content"));
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlstr);
            XmlNode codeNode = xmlDoc.SelectSingleNode("sendresult/errorcode");
            XmlNode msgNode = xmlDoc.SelectSingleNode("sendresult/message");
            XmlNode infoNode = xmlDoc.SelectSingleNode("sendresult/SMSID");
            if (codeNode.InnerText == "1")
            {
                d.ExtentionObj.issuccess = true;
            }
            else
            {
                d.ExtentionObj.issuccess = false;
                GlobalCommon.Logger.WriteLog(LoggerLevel.ERROR, string.Format("发送短信失败,错误代码:{0};错误信息:{1}", codeNode.InnerText, msgNode.InnerText));
            }
        }

        protected override void ProcessBeforeRequest(WebParameter p, WebBaseData d)
        {
            var url = "http://sms.ue35.net/sms/interface/sendmess.htm";
            SetRequestURL(url);
            var username = "yhy_ff";
            var userpwd = "123456";
            var mobiles = d.ExtentionObj.mobiles;
            var content = ComFunc.UrlEncode(ComFunc.nvl(d.ExtentionObj.message));

            SetContentType("application/x-www-form-urlencoded");
            SetRequestMethod("POST");
            AddPostData("username",username);
            AddPostData("userpwd",userpwd);
            AddPostData("mobiles", mobiles);
            AddPostData("content",content);
        }
    }
}
