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

namespace Builder.Web.Proxy
{
    public class TradeProxy : HttpRemoteProxy<WebParameter, GoData>
    {
        protected override void ProcessAfterRequest(EFFC.Frame.Net.Base.Data.Base.FrameDLRObject responseobj, WebParameter p, GoData d)
        {
            var responsestring = responseobj.GetValue("content");
            var contenttype = ComFunc.nvl(responseobj.GetValue("contenttype"));

            if (FrameDLRObject.IsJson(ComFunc.nvl(responsestring)))
            {
                d.ExtentionObj.OuterHttpResult = FrameDLRObject.CreateInstance(responsestring, FrameDLRFlags.SensitiveCase);
            }
            else
            {
                d.ExtentionObj.OuterHttpResult = responsestring;
            }
        }

        protected override void ProcessBeforeRequest(WebParameter p, GoData d)
        {
            string url = ComFunc.nvl(d.ExtentionObj.OuterHttpUrl);

            SetRequestURL(url);
            SetContentType(ResponseHeader_ContentType.json);

            if (d.ExtentionObj.OuterHttpPostData != null)
            {
                if (d.ExtentionObj.OuterHttpPostData is FrameDLRObject)
                {
                    var dobj = (FrameDLRObject)d.ExtentionObj.OuterHttpPostData;
                    foreach (var k in dobj.Keys)
                    {
                        AddPostData(k, dobj.GetValue(k));
                    }
                }
                else if (d.ExtentionObj.OuterHttpPostData is KeyValuePair<string, object>[])
                {
                    var dobj = (KeyValuePair<string, object>[])d.ExtentionObj.OuterHttpPostData;
                    foreach (var k in dobj)
                    {
                        AddPostData(k.Key, k.Value);
                    }
                }
            }
        }
    }

    public class TradeAsyncProxy : HttpRemoteAsyncProxy<WebParameter, GoData>
    {

        protected override void ProcessAfterRequest(FrameDLRObject responseobj, WebParameter p, GoData d, Action<WebParameter, GoData> callback)
        {
            var responsestring = responseobj.GetValue("content");
            var contenttype = ComFunc.nvl(responseobj.GetValue("contenttype"));

            if (FrameDLRObject.IsJson(ComFunc.nvl(responsestring)))
            {
                d.ExtentionObj.OuterHttpResult = FrameDLRObject.CreateInstance(responsestring, FrameDLRFlags.SensitiveCase);
            }
            else
            {
                d.ExtentionObj.OuterHttpResult = responsestring;
            }
        }

        protected override void ProcessBeforeRequest(WebParameter p, GoData d)
        {
            string url = ComFunc.nvl(d.ExtentionObj.OuterHttpUrl);

            SetRequestURL(url);
            SetContentType(ResponseHeader_ContentType.json);

            if (d.ExtentionObj.OuterHttpPostData != null)
            {
                if (d.ExtentionObj.OuterHttpPostData is FrameDLRObject)
                {
                    var dobj = (FrameDLRObject)d.ExtentionObj.OuterHttpPostData;
                    foreach (var k in dobj.Keys)
                    {
                        AddPostData(k, dobj.GetValue(k));
                    }
                }
                else if (d.ExtentionObj.OuterHttpPostData is KeyValuePair<string, object>[])
                {
                    var dobj = (KeyValuePair<string, object>[])d.ExtentionObj.OuterHttpPostData;
                    foreach (var k in dobj)
                    {
                        AddPostData(k.Key, k.Value);
                    }
                }
            }
        }
    }
}
