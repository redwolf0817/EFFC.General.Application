using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data.Base;
using EFFC.Frame.Net.Base.Module;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;

namespace Builder.Web.Proxy
{
    public class WeixinPlatformProxy : HttpRemoteProxy<WebParameter, WebBaseData>
    {
        protected override void ProcessAfterRequest(FrameDLRObject responseobj, WebParameter p, WebBaseData d)
        {
            var responsestring = responseobj.GetValue("content");
            //var contenttype = ComFunc.nvl(responseobj.GetValue("contenttype"));
            d.ExtentionObj.OuterHttpResult = FrameDLRObject.IsJson(ComFunc.nvl(responsestring)) ?
                                                FrameDLRObject.CreateInstance(responsestring, FrameDLRFlags.SensitiveCase)
                                                : FrameDLRObject.CreateInstance(@"{ issuccess : false, msg:'" + responsestring + "',token:'' }");
        }

        protected override void ProcessBeforeRequest(WebParameter p, WebBaseData d)
        {
            string url = ComFunc.nvl(d.ExtentionObj.OuterHttpUrl);
            SetRequestURL(url);
            SetContentType("application/x-www-form-urlencoded");
            SetRequestMethod("POST");
            AddPostData("password", d.ExtentionObj.password);
            AddPostData("action", d.ExtentionObj.action);
            AddPostData("appid", d.ExtentionObj.appid);
        }
    }
}
