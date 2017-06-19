using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data.Base;
using EFFC.Frame.Net.Base.Module;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Builder.Web.Proxy
{
    public class HTMLHttpProxy: HttpRemoteProxy<WebParameter, GoData>
    {
        protected override void ProcessAfterRequest(EFFC.Frame.Net.Base.Data.Base.FrameDLRObject responseobj, WebParameter p, GoData d)
        {
            var responsestring = responseobj.GetValue("content");
            var contenttype = ComFunc.nvl(responseobj.GetValue("contenttype"));
            if (contenttype.ToLower().IndexOf("/xml") > 0)
            {
                var xd = new XmlDocument();
                xd.LoadXml(ComFunc.nvl(responsestring));
                var root = xd.FirstChild;
                var dobj = FrameDLRObject.CreateInstance(ComFunc.nvl(responsestring), FrameDLRFlags.SensitiveCase);

                d.ExtentionObj.OuterHttpResult = dobj;
            }
            else if (contenttype.ToLower().StartsWith("image")
                || contenttype.ToLower().StartsWith("audio")
                || contenttype.ToLower().StartsWith("video"))
            {
                d.ExtentionObj.OuterHttpResult = responseobj;
            }
            else
            {
                if (FrameDLRObject.IsJson(ComFunc.nvl(responsestring)))
                {
                    d.ExtentionObj.OuterHttpResult = FrameDLRObject.CreateInstance(responsestring, FrameDLRFlags.SensitiveCase);
                }
                else
                {
                    d.ExtentionObj.OuterHttpResult = responsestring;
                }
            }
        }

        protected override void ProcessBeforeRequest(WebParameter p, GoData d)
        {
            string url = ComFunc.nvl(d.ExtentionObj.OuterHttpUrl);
            SetContentType("text/html");
            SetRequestMethod("Get");
            SetRequestURL(url);
        }
    }
}
