using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data.Base;
using EFFC.Frame.Net.Base.Module;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Builder.Web.Proxy
{
    public class WeixinHttpProxy : HttpRemoteProxy<WebParameter, GoData>
    {
        protected override void ProcessAfterRequest(FrameDLRObject responseobj, WebParameter p, GoData d)
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
            if (d.ExtentionObj.x509cert != null)
            {
                SetX509Certificate2((X509Certificate2)d.ExtentionObj.x509cert);
            }

            SetRequestURL(url);
            if (ComFunc.nvl(d.ExtentionObj.type) == "text/xml")
            {
                SetContentType("text/xml");
            }
            if (ComFunc.nvl(d.ExtentionObj.method) != "")
            {
                SetRequestMethod(ComFunc.nvl(d.ExtentionObj.method));
            }

            if (d.ExtentionObj.OuterHttpPostData != null)
            {
                if (ComFunc.nvl(d.ExtentionObj.type) == "binary" || ComFunc.nvl(d.ExtentionObj.type) == "multipart/form-data")
                {
                    string filename = d.ExtentionObj.filename != null ? ComFunc.nvl(d.ExtentionObj.filename) : ComFunc.nvl(d.ExtentionObj.OuterHttpPostData.filename);
                    string name = d.ExtentionObj.name != null ? ComFunc.nvl(d.ExtentionObj.name) : ComFunc.nvl(d.ExtentionObj.OuterHttpPostData.name);
                    string filecontenttype = d.ExtentionObj.filecontenttype != null ? ComFunc.nvl(d.ExtentionObj.filecontenttype) : ComFunc.nvl(d.ExtentionObj.OuterHttpPostData.filecontenttype);
                    byte[] filecontent = d.ExtentionObj.filecontent != null ? d.ExtentionObj.filecontent : d.ExtentionObj.OuterHttpPostData.filecontent;
                    SetContentType("multipart/form-data");

                    var item = FrameDLRObject.CreateInstance();
                    item.name = name;
                    item.filename = filename;
                    item.contenttype = "application/octet-stream";
                    item.formitem = filecontent;

                    AddPostData(name, item);
                }
                else
                {
                    if (d.ExtentionObj.OuterHttpPostData is FrameDLRObject)
                    {
                        var dobj = (FrameDLRObject)d.ExtentionObj.OuterHttpPostData;
                        foreach (var k in dobj.Keys)
                        {
                            AddPostData(k, dobj.GetValue(k));
                        }
                    }
                }
            }
        }
    }
    public class WeixinAsyncHttpProxy : HttpRemoteAsyncProxy<WebParameter, GoData>
    {

        protected override void ProcessAfterRequest(FrameDLRObject responseobj, WebParameter p, GoData d, Action<WebParameter, GoData> callback)
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

            if (callback != null)
            {
                callback.Invoke(p, d);
            }
        }

        protected override void ProcessBeforeRequest(WebParameter p, GoData d)
        {
            string url = ComFunc.nvl(d.ExtentionObj.OuterHttpUrl);
            if (d.ExtentionObj.x509cert != null)
            {
                SetX509Certificate2((X509Certificate2)d.ExtentionObj.x509cert);
            }

            SetRequestURL(url);
            if (ComFunc.nvl(d.ExtentionObj.type) == "text/xml")
            {
                SetContentType("text/xml");
            }
            SetProxy(null);

            if (d.ExtentionObj.OuterHttpPostData != null)
            {
                if (ComFunc.nvl(d.ExtentionObj.type) == "binary" || ComFunc.nvl(d.ExtentionObj.type) == "multipart/form-data")
                {
                    string filename = d.ExtentionObj.filename != null ? ComFunc.nvl(d.ExtentionObj.filename) : ComFunc.nvl(d.ExtentionObj.OuterHttpPostData.filename);
                    string name = d.ExtentionObj.name != null ? ComFunc.nvl(d.ExtentionObj.name) : ComFunc.nvl(d.ExtentionObj.OuterHttpPostData.name);
                    string filecontenttype = d.ExtentionObj.filecontenttype != null ? ComFunc.nvl(d.ExtentionObj.filecontenttype) : ComFunc.nvl(d.ExtentionObj.OuterHttpPostData.filecontenttype);
                    byte[] filecontent = d.ExtentionObj.filecontent != null ? d.ExtentionObj.filecontent : d.ExtentionObj.OuterHttpPostData.filecontent;
                    SetContentType("multipart/form-data");

                    var item = FrameDLRObject.CreateInstance();
                    item.name = name;
                    item.filename = filename;
                    item.contenttype = "application/octet-stream";
                    item.formitem = filecontent;

                    AddPostData(name, item);
                }
                else
                {
                    if (d.ExtentionObj.OuterHttpPostData is FrameDLRObject)
                    {
                        var dobj = (FrameDLRObject)d.ExtentionObj.OuterHttpPostData;
                        foreach (var k in dobj.Keys)
                        {
                            AddPostData(k, dobj.GetValue(k));
                        }
                    }
                }
            }
        }
    }
}

    