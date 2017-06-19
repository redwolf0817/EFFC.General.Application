using Builder.Web.Global;
using Builder.Web.Helper;
using Builder.Web.Proxy;
using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data.Base;
using EFFC.Frame.Net.Base.Module;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using EFFC.Frame.Net.Global;
using EFFC.Frame.Net.Web.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.SessionState;
using System.Xml;

namespace Builder.Web.Handler
{
    public class WeixinHandler : GoBaseHandler<WebParameter, GoData>, IReadOnlySessionState
    {
        protected override void OnError(Exception ex, WebParameter p, GoData d)
        {
            GlobalCommon.ExceptionProcessor.ProcessException(this, ex, p, d);
            string errorCode = "E-" + ComFunc.nvl(p[DomainKey.CONFIG, "Machine_No"]) + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string errlog = "";
            if (ex.InnerException != null)
            {
                errlog = string.Format("错误编号：{0}，\n{1}\n{2}\nInnerException:{3}\n{4}", errorCode, ex.Message, ex.StackTrace, ex.InnerException.Message, ex.InnerException.StackTrace);
            }
            else
            {
                errlog = string.Format("错误编号：{0}，\n{1}\n{2}", errorCode, ex.Message, ex.StackTrace);
            }
            GlobalCommon.Logger.WriteLog(LoggerLevel.ERROR, errlog);

            var errormsg = "";
            var isdebug = p[DomainKey.CONFIG, "DebugMode"] == null ? false : (bool)p[DomainKey.CONFIG, "DebugMode"];
            if (isdebug)
            {
                errormsg = string.Format("出错了，{0}", errlog); ;
            }
            else
            {
                errormsg = string.Format("系统出错了，亲，请将错误编号（{0}）告知我们，我们会帮亲处理的哦！", errorCode);
            }

            p.Resources.RollbackTransaction(p.CurrentTransToken);
            p.Resources.ReleaseAll();

            var dobj = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
            dobj.ToUserName = p[DomainKey.POST_DATA, "FromUserName"];
            dobj.FromUserName = p[DomainKey.POST_DATA, "ToUserName"];
            dobj.CreateTime = DateTime.Now;
            dobj.MsgType = "text";
            dobj.Content = errormsg;
            dobj.FuncFlag = 0;
            var content = ToXml(dobj);
            //如果内容为aes加密
            if (p.ExtentionObj.weixin.encrypt_type == "aes")
            {
                DateTime createTime = dobj.CreateTime;
                int timeStamp = ToWeixinTime(createTime);
                Random random = new Random();
                string nonce = random.Next().ToString();

                WXBizMsgCrypt wxcpt = new WXBizMsgCrypt(p.ExtentionObj.weixin.token, p.ExtentionObj.weixin.encrypt_key, p.ExtentionObj.weixin.appid);
                string xmlEncrypt = "";
                //加密消息
                if (wxcpt.EncryptMsg(content, timeStamp.ToString(), nonce, ref xmlEncrypt) == WXBizMsgCrypt.WXBizMsgCryptErrorCode.WXBizMsgCrypt_OK)
                    content = xmlEncrypt;
            }

            CurrentContext.Response.Write(content);
        }

        protected override bool RunMe(WebParameter p, GoData d)
        {
            try
            {
                p.CanContinue = true;
                bool isSuccess = ModuleProxyManager<WebParameter, GoData>.Call<PreWeixinProcessProxy>(p, d);
                if (isSuccess & p.CanContinue)
                    isSuccess = ModuleProxyManager<WebParameter, GoData>.Call<GoBusinessProxy>(p, d);

                return isSuccess;
            }
            finally
            {
                p.Resources.ReleaseAll();
            }
        }

        protected override void Init(System.Web.HttpContext context, WebParameter p, GoData d)
        {
            base.Init(context, p, d);
            GlobalPrepare.ConfigPrepare(ref p);
            p.SetValue("logkey", DateTime.Now.ToString("yyyyMMddHHmmssfff"));
            string content = string.Empty;
            if (context.Request.HttpMethod == WebRequestMethods.Http.Post)
            {

                byte[] bytes = context.Request.BinaryRead(context.Request.ContentLength);
                if (bytes != null && bytes.Length > 0)
                    content = context.Request.ContentEncoding.GetString(bytes);
                //如果内容为aes加密
                if (p.ExtentionObj.weixin.encrypt_type == "aes")
                {
                    WXBizMsgCrypt wxcpt = new WXBizMsgCrypt(p.ExtentionObj.weixin.token, p.ExtentionObj.weixin.encrypt_key, p.ExtentionObj.weixin.appid);
                    string msg = "";
                    var result = wxcpt.DecryptMsg(p.ExtentionObj.weixin.signature, p.ExtentionObj.weixin.timestamp, p.ExtentionObj.weixin.nonce, content, ref msg);
                    content = msg;
                }
                AddRequestContentLog(p, content);
                if (content != "")
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(content);
                    var root = doc.FirstChild;
                    foreach (XmlNode node in root.ChildNodes)
                    {
                        if (node.Name == "CreateTime")
                        {
                            p[DomainKey.POST_DATA, node.Name] = new DateTime(1970, 1, 1).AddSeconds(int.Parse(node.InnerText));
                        }
                        else
                        {
                            p[DomainKey.POST_DATA, node.Name] = node.InnerText;
                        }

                    }
                }
                //事件触发时的action处理
                if (p[DomainKey.POST_DATA, "Event"] != null)
                {
                    p.Action = ComFunc.nvl(p[DomainKey.POST_DATA, "Event"]);
                }
                else
                {
                    //普通消息处理，action为消息类型
                    p.Action = ComFunc.nvl(p[DomainKey.POST_DATA, "MsgType"]);
                }
            }
            else
            {
                AddRequestContentLog(p, content);
                //action为空的时候为微信服务器的验证请求
                p.Action = "";
            }
        }

        private void AddRequestContentLog(WebParameter p, string content)
        {
            Dictionary<string, FrameDLRObject> d = null;
            if (GlobalCommon.ApplicationCache.Get("weixinlog") == null)
            {
                d = new Dictionary<string, FrameDLRObject>();
                GlobalCommon.ApplicationCache.Set("weixinlog", d, DateTime.Now.AddDays(1));
            }
            else
            {
                d = (Dictionary<string, FrameDLRObject>)GlobalCommon.ApplicationCache.Get("weixinlog");
            }
            var logkey = ComFunc.nvl(p.GetValue("logkey"));
            var dobj = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
            dobj.Request = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
            dobj.Request.Head = p.ExtentionObj.weixin;
            dobj.Request.Content = content;
            d.Add(logkey, dobj);
        }
        private void AddResponseContentLog(WebParameter p, string content)
        {
            Dictionary<string, FrameDLRObject> d = null;
            if (GlobalCommon.ApplicationCache.Get("weixinlog") == null)
            {
                d = new Dictionary<string, FrameDLRObject>();
                GlobalCommon.ApplicationCache.Set("weixinlog", d, DateTime.Now.AddDays(1));
            }
            else
            {
                d = (Dictionary<string, FrameDLRObject>)GlobalCommon.ApplicationCache.Get("weixinlog");
            }
            var logkey = ComFunc.nvl(p.GetValue("logkey"));

            if (d.ContainsKey(logkey))
            {
                dynamic dobj = d[logkey];
                dobj.Response = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
                dobj.Response.Head = p.ExtentionObj.weixin;
                dobj.Response.Content = content;
            }
            else
            {
                dynamic dobj = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
                dobj.Response = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
                dobj.Response.Head = p.ExtentionObj.weixin;
                dobj.Response.Content = content;
                d.Add(logkey, dobj);
            }
        }

        protected override void SetContent(WebParameter p, GoData d)
        {
            if (d.ResponseData is FrameDLRObject)
            {
                var re = (FrameDLRObject)d.ResponseData;

                var content = ToXml(re);
                if (p.ExtentionObj.weixin.encrypt_type == "aes")
                {
                    var createTime = re.GetValue("CreateTime") == null ? DateTime.Now : (DateTime)re.GetValue("CreateTime");
                    int timeStamp = ToWeixinTime(createTime);
                    Random random = new Random();
                    string nonce = random.Next().ToString();

                    WXBizMsgCrypt wxcpt = new WXBizMsgCrypt(p.ExtentionObj.weixin.token, p.ExtentionObj.weixin.encrypt_key, p.ExtentionObj.weixin.appid);
                    string xmlEncrypt = "";
                    //加密消息
                    if (wxcpt.EncryptMsg(content, timeStamp.ToString(), nonce, ref xmlEncrypt) == WXBizMsgCrypt.WXBizMsgCryptErrorCode.WXBizMsgCrypt_OK)
                        content = xmlEncrypt;

                }
                AddResponseContentLog(p, content);
                CurrentContext.Response.Write(content);
            }
            else
            {
                AddResponseContentLog(p, ComFunc.nvl(d.ResponseData));
                CurrentContext.Response.Write(d.ResponseData);
            }
        }
        /// <summary>
        /// 微信无需websocket支持
        /// </summary>
        /// <param name="p"></param>
        /// <param name="d"></param>
        protected override void SetContent4WebSocket(WebParameter p, GoData d)
        {
            CurrentSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, "微信没有websocket协议", CancellationToken.None);
            SetContent(p, d);
        }

        private string ToXml(FrameDLRObject obj)
        {

            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("xml");
            doc.AppendChild(root);
            foreach (var k in obj.Keys)
            {
                ToXmlElem(doc,root, k, obj.GetValue(k));
            }
            return doc.InnerXml;
        }

        private void ToXmlElem(XmlDocument doc, XmlElement parent, string name, object obj)
        {
            

            if (obj is int
                    || obj is long
                    || obj is double
                    || obj is float
                    || obj is decimal)
            {
                XmlElement elem = doc.CreateElement(name);
                elem.AppendChild(doc.CreateTextNode(obj.ToString()));
                parent.AppendChild(elem);
            }
            else if (obj is DateTime)
            {
                var dt = (DateTime)obj;
                XmlElement elem = doc.CreateElement(name);
                elem.AppendChild(doc.CreateTextNode(ToWeixinTime(dt).ToString()));
                 parent.AppendChild(elem);
            }
            else if (obj is string)
            {
                XmlElement elem = doc.CreateElement(name);
                elem.AppendChild(doc.CreateCDataSection(obj.ToString()));
                parent.AppendChild(elem);
            }
            else if (obj is FrameDLRObject)
            {
                var dobj = (FrameDLRObject)obj;
                XmlElement elem = doc.CreateElement(name);
                foreach (var k in dobj.Keys)
                {
                    ToXmlElem(doc,elem, k, dobj.GetValue(k));
                }
                parent.AppendChild(elem);
            }
            else if (obj is Dictionary<string, object>)
            {
                var dobj = (Dictionary<string, object>)obj;
                 XmlElement elem = doc.CreateElement(name);
                foreach (var k in dobj)
                {
                    ToXmlElem(doc,elem, k.Key, k.Value);
                }
                parent.AppendChild(elem);
            }
            else if (obj is object[])
            {
                var arr = (object[])obj;
                foreach (var item in arr)
                {
                    var elemitem = doc.CreateElement(name);
                    if (item is FrameDLRObject)
                    {
                        var dobj = (FrameDLRObject)item;
                        foreach (var k in dobj.Keys)
                        {
                            ToXmlElem(doc, elemitem, k, dobj.GetValue(k));
                        }
                    }
                    else if (item is Dictionary<string, object>)
                    {
                        var dobj = (Dictionary<string, object>)item;
                        foreach (var k in dobj)
                        {
                            ToXmlElem(doc, elemitem, k.Key, k.Value);
                        }
                    }
                    else
                    {
                        elemitem.AppendChild(doc.CreateCDataSection(ComFunc.nvl(item)));
                    }
                    parent.AppendChild(elemitem);
                }

            }
            else
            {
                XmlElement elem = doc.CreateElement(name);
                elem.AppendChild(doc.CreateCDataSection(ComFunc.nvl(obj)));
                parent.AppendChild(elem);
            }
        }

        /// <summary>
        /// 返回微信时间（距1970年1月1日0点的秒数）
        /// </summary>
        /// <param name="dt">时间</param>
        /// <returns>返回微信时间</returns>
        public static int ToWeixinTime(DateTime dt)
        {
            DateTime baseTime = new DateTime(1970, 1, 1);
            return (int)(dt - baseTime).TotalSeconds;
        }

        public override string Name
        {
            get { return "Other"; }
        }

        public override string Version
        {
            get { return "0.0.1"; }
        }

        public override string Description
        {
            get { return "微信的Request的处理"; }
        }
    }
}
