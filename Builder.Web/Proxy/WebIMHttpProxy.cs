using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data.Base;
using EFFC.Frame.Net.Base.Module;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Builder.Web.Proxy
{
    public class WebIMHttpProxy : HttpRemoteProxy<WebParameter, GoData>
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

            d.ExtentionObj.OuterHttpResult.statuscode = responseobj.GetValue("statuscode");
        }

        protected override void ProcessBeforeRequest(WebParameter p, GoData d)
        {
            string url = ComFunc.nvl(d.ExtentionObj.OuterHttpUrl);

            SetRequestURL(url);
            if (ComFunc.nvl(d.ExtentionObj.type) == "")
            {
                SetContentType("application/json");
            }
            else if (ComFunc.nvl(d.ExtentionObj.type) == "binary" || ComFunc.nvl(d.ExtentionObj.type) == "multipart/form-data")
            {
                SetContentType("multipart/form-data");
            }
            else
            {
                SetContentType(ComFunc.nvl(d.ExtentionObj.type));
            }
            

            if (ComFunc.nvl(d.ExtentionObj.type) == "text/xml")
            {
                SetContentType("text/xml");
            }

            if (ComFunc.nvl(d.ExtentionObj.method) != "")
            {
                SetRequestMethod(ComFunc.nvl(d.ExtentionObj.method));
            }


            if (d.ExtentionObj.HeadData != null)
            {
                if (d.ExtentionObj.HeadData is FrameDLRObject)
                {
                    var dobj = (FrameDLRObject)d.ExtentionObj.HeadData;
                    foreach (var k in dobj.Keys)
                    {
                        AddHeader(k, ComFunc.nvl(dobj.GetValue(k)));
                    }
                }
            }
            

            if (d.ExtentionObj.OuterHttpPostData != null)
            {
                if (ComFunc.nvl(d.ExtentionObj.type) == "binary" || ComFunc.nvl(d.ExtentionObj.type) == "multipart/form-data")
                {
                    string filename = ComFunc.nvl(d.ExtentionObj.OuterHttpPostData.filename);
                    string name = ComFunc.nvl(d.ExtentionObj.OuterHttpPostData.name);
                    string filecontenttype = ComFunc.nvl(d.ExtentionObj.OuterHttpPostData.filecontenttype);
                    byte[] filecontent = d.ExtentionObj.OuterHttpPostData.filecontent;

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
