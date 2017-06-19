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
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Builder.Web.Proxy
{
    class AliHttpProxy : HttpRemoteProxy<WebParameter, GoData>
    {
        protected override void ProcessAfterRequest(EFFC.Frame.Net.Base.Data.Base.FrameDLRObject responseobj, WebParameter p, GoData d)
        {
            var responsestring = responseobj.GetValue("content");
            var contenttype = ComFunc.nvl(responseobj.GetValue("contenttype"));
            d.ExtentionObj.OuterHttpResult = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
            if (contenttype.ToLower().IndexOf("/xml") > 0)
            {
                var xmlstr = "";
                if (responsestring is byte[])
                {
                    xmlstr = ComFunc.ByteToString((byte[])responsestring, Encoding.UTF8);
                }
                else if (responsestring is Stream)
                {
                    xmlstr = ComFunc.StreamToString((Stream)responsestring);
                }
                else
                {
                    xmlstr = ComFunc.nvl(responsestring);
                }

                var dobj = FrameDLRObject.CreateInstance(xmlstr, FrameDLRFlags.SensitiveCase);

                d.ExtentionObj.OuterHttpResult = dobj;
            }
            else if (contenttype.ToLower().StartsWith("application/"))
            {
                d.ExtentionObj.OuterHttpResult = responseobj;
            }
            else
            {
                var str = "";
                if (responsestring is byte[])
                {
                    str = ComFunc.ByteToString((byte[])responsestring, Encoding.UTF8);
                }
                else if (responsestring is Stream)
                {
                    str = ComFunc.StreamToString((Stream)responsestring);
                }
                else
                {
                    str = ComFunc.nvl(responsestring);
                }

                if (FrameDLRObject.IsJson(ComFunc.nvl(str)))
                {
                    d.ExtentionObj.OuterHttpResult = FrameDLRObject.CreateInstance(str, FrameDLRFlags.SensitiveCase);
                }
                else if (str == "")
                {
                    d.ExtentionObj.OuterHttpResult = FrameDLRObject.CreateInstance("", FrameDLRFlags.SensitiveCase);
                }
                else
                {
                    d.ExtentionObj.OuterHttpResult = str;
                }
            }
            d.ExtentionObj.OuterHttpResult.header = responseobj.GetValue("header");
            d.ExtentionObj.OuterHttpResult.statuscode = responseobj.GetValue("statuscode");
        }

        protected override void ProcessBeforeRequest(WebParameter p, GoData d)
        {
            string url = ComFunc.nvl(d.ExtentionObj.OuterHttpUrl);

            SetRequestURL(url);
            string contenttype = ComFunc.nvl(d.ExtentionObj.type);
            string verb = ComFunc.nvl(d.ExtentionObj.method);

            if (contenttype != "")
            {
                if (verb.ToLower() == "put")
                {
                    SetContentType("PUT/" + contenttype);
                }
                else
                {
                    SetContentType(contenttype);
                }
            }
            else
            {
                SetContentType("");
            }

            if (verb != "")
            {
                SetRequestMethod(verb);
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
                            if (dobj.GetValue(k) is string)
                            {
                                AddPostData(k, ComFunc.UrlEncode(dobj.GetValue(k)));
                            }
                            else
                            {
                                AddPostData(k, dobj.GetValue(k));
                            }
                        }
                    }
                }
            }
        }
    }
}
