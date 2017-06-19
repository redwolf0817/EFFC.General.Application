using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using EFFC.Frame.Net.Web.Core;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using EFFC.Frame.Net.Global;
using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Module;
using Builder.Web.Proxy;
using EFFC.Frame.Net.Base.Data.Base;
using Builder.Web.Global;
using Builder.Web.Helper;
using System.Net.WebSockets;
using System.Threading;
using System.Web.SessionState;
using Noesis.Javascript;
using EFFC.Frame.Net.Business.Engine;


namespace Builder.Web.Handler
{
    public class GoHandler : GoBaseHandler<WebParameter, GoData>, IReadOnlySessionState
    {
        protected override void OnError(Exception ex, WebParameter p, GoData d)
        {
            GlobalCommon.ExceptionProcessor.ProcessException(this, ex, p, d);
            string errorCode = "E-" + ComFunc.nvl(p[DomainKey.CONFIG, "Machine_No"]) + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string errlog = "";
            if (ex is JavascriptException)
            {
                var jex = (JavascriptException)ex;
                if (ex.InnerException != null)
                {
                    if (ex.InnerException is JavascriptException)
                    {
                        var ijex = (JavascriptException)ex.InnerException;
                        errlog = string.Format("错误编号：{0}，\n{1}\n{2}\n出错代码行数{3}\n出错代码列数{4}\n出错代码位置{5}\nInnerException:{6}\n{7}\n出错代码行数{8}\n出错代码列数{9}\n出错代码位置{10}", errorCode, ex.Message, ex.StackTrace,
                            jex.Line, jex.StartColumn + "~" + jex.EndColumn, jex.V8SourceLine.Replace("\"","'"),
                            ex.InnerException.Message, ex.InnerException.StackTrace,
                            ijex.Line, ijex.StartColumn + "~" + ijex.EndColumn, ijex.V8SourceLine.Replace("\"", "'"));
                    }
                    else
                    {
                        errlog = string.Format("错误编号：{0}，\n{1}\n{2}\n出错代码行数{3}\n出错代码列数{4}\n出错代码位置{5}\nInnerException:{6}\n{7}", errorCode, ex.Message, ex.StackTrace, jex.Line, jex.StartColumn + "~" + jex.EndColumn, jex.V8SourceLine, ex.InnerException.Message, ex.InnerException.StackTrace);
                    }
                }
                else
                {
                    errlog = string.Format("错误编号：{0}，\n{1}\n{2}\n出错代码行数{3}\n出错代码列数{4}\n出错代码位置{5}", errorCode, ex.Message, ex.StackTrace,
                        jex.Line, jex.StartColumn + "~" + jex.EndColumn, jex.V8SourceLine.Replace("\"", "'"));
                }
            }
            else
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException is JavascriptException)
                    {
                        var ijex = (JavascriptException)ex.InnerException;
                        errlog = string.Format("错误编号：{0}，\n{1}\n{2}\nInnerException:{3}\n{4}\n\n出错代码行数{5}\n出错代码列数{6}\n出错代码位置{7}", errorCode, ex.Message, ex.StackTrace,
                            ex.InnerException.Message, ex.InnerException.StackTrace,
                            ijex.Line, ijex.StartColumn + "~" + ijex.EndColumn, ijex.V8SourceLine.Replace("\"", "'"));
                    }
                    else
                    {
                        errlog = string.Format("错误编号：{0}，\n{1}\n{2}\nInnerException:{3}\n{4}", errorCode, ex.Message, ex.StackTrace, ex.InnerException.Message, ex.InnerException.StackTrace);
                    }
                }
                else
                {
                    errlog = string.Format("错误编号：{0}，\n{1}\n{2}", errorCode, ex.Message, ex.StackTrace);
                }
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
                errormsg = string.Format("系统出错了，请联系相关人员帮助处理，并告知其错误编号。谢谢！（错误编号：{0}）", errorCode);
            }

            p.Resources.RollbackTransaction(p.CurrentTransToken);
            p.Resources.ReleaseAll();

            if (d.ContentType == GoResponseDataType.Json)
            {
                if (IsWebSocket)
                {
                    if (CurrentSocket.State == WebSocketState.Open)
                    {
                        var b = Encoding.UTF8.GetBytes(ComFunc.FormatJSON(errorCode, errlog, "").ToJSONString());
                        var buffer = new ArraySegment<byte>(b);
                        CurrentSocket.SendAsync(buffer, WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
                    }
                }
                else
                {
                    CurrentContext.Response.Write(ComFunc.FormatJSON(errorCode, errlog, "").ToJSONString());
                }
            }
            else if (d.ContentType == GoResponseDataType.RazorView)
            {
                if (IsWebSocket)
                {
                    if (CurrentSocket.State == WebSocketState.Open)
                    {
                        var b = Encoding.UTF8.GetBytes(ComFunc.FormatJSON(errorCode, errlog, "").ToJSONString());
                        var buffer = new ArraySegment<byte>(b);
                        CurrentSocket.SendAsync(buffer, WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
                    }
                    else
                    {
                        if (IsAjaxAsync)
                        {
                            d.ViewPath = "~/Views/Shared/Error_Frame_NoLayout.cshtml";
                        }
                        else
                        {
                            d.ViewPath = "~/Views/Shared/Error_Frame.cshtml";
                        }
                        if (File.Exists(d.ViewPath.Replace("~", p.ServerRootPath)))
                        {
                            d[DomainKey.VIEW_LIST, "ErrorTitle"] = "系统出错了";
                            d[DomainKey.VIEW_LIST, "ErrorMsg"] = errormsg;

                            CurrentContext.Response.Charset = "UTF-8";
                            CurrentContext.Response.ContentType = ResponseHeader_ContentType.html;
                            WMvcView.RenderView(p, d, CurrentContext, CurrentContext.Response.Output);
                        }
                        else
                        {
                            CurrentContext.Response.Charset = "UTF-8";
                            CurrentContext.Response.ContentType = ResponseHeader_ContentType.json;
                            CurrentContext.Response.Write(ComFunc.FormatJSON(errorCode, errlog, "").ToJSONString());
                        }
                    }
                }
                else
                {
                    CurrentContext.Response.Write(ComFunc.FormatJSON(errorCode, errlog, "").ToJSONString());
                }
            }
            else if (d.ContentType == GoResponseDataType.HostView)
            {
                var viewpath = "~/error.hjs".Replace("~", GlobalCommon.HostCommon.RootPath + HostJsConstants.COMPILED_VIEW_PATH);
                if (File.Exists(viewpath))
                {
                    //调用hostview引擎进行渲染
                    HostJsView hjv = (HostJsView)p.ExtentionObj.hostviewengine;
                    hjv.CurrentContext.SetDataModel(FrameDLRObject.CreateInstanceFromat(@"{ErrorTitle:{0},ErrorMsg:{1}}", "系统出错了", errormsg).ToDictionary());
                    var html = hjv.Render(File.ReadAllText(viewpath, Encoding.UTF8));

                    CurrentContext.Response.Charset = "UTF-8";
                    CurrentContext.Response.ContentType = ResponseHeader_ContentType.html;
                    CurrentContext.Response.Write(html);
                }
                else
                {
                    CurrentContext.Response.Charset = "UTF-8";
                    CurrentContext.Response.ContentType = ResponseHeader_ContentType.json;
                    CurrentContext.Response.Write(ComFunc.FormatJSON(errorCode, errlog, "").ToJSONString());
                }
            }
            else
            {
                CurrentContext.Response.Charset = "UTF-8";
                CurrentContext.Response.ContentType = ResponseHeader_ContentType.json;
                CurrentContext.Response.Write(ComFunc.FormatJSON(errorCode, errlog, "").ToJSONString());
            }
        }

        protected override bool RunMe(WebParameter p, GoData d)
        {
            try
            {
                //1.进行预处理
                p.CanContinue = true;
                bool isSuccess = ModuleProxyManager<WebParameter, GoData>.Call<PreProcessGoProxy>(p, d);
                //2.业务逻辑处理
                if (isSuccess & p.CanContinue)
                {
                    isSuccess = isSuccess & ModuleProxyManager<WebParameter, GoData>.Call<GoBusinessProxy>(p, d);
                }

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
        }

        protected override void AfterProcess(HttpContext context, WebParameter p, GoData d)
        {
            base.AfterProcess(context, p, d);
            //if (!IsWebSocket)
            //{
            //    CurrentContext.Response.AddHeader("Access-Control-Allow-Origin", ComFunc.nvl(p[DomainKey.CONFIG, "Access-Control-Allow-Origin"]));
            //}
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
            get { return "Go类型的Request的处理"; }
        }
    }
}
