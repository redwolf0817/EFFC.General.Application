using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Data.SqlClient;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using EFFC.Frame.Net.Web.Core;
using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Global;
using Builder.Web.Global;
using EFFC.Frame.Net.Base.Module;
using Builder.Web.Proxy;
using System.Net.WebSockets;
using EFFC.Frame.Net.Base.Data.Base;
using EFFC.Frame.Net.Data;
using EFFC.Frame.Net.Business.Engine;

namespace Builder.Web.Handler
{
    public class ViewHandler : WMcvBaseHandler<WebParameter, WMvcData>
    {
        public override bool IsReusable
        {
            get { return false; }
        }

        protected override void OnError(Exception ex, WebParameter p, WMvcData d)
        {
            if (ex is ThreadAbortException) return;

            if (IsAjaxAsync)
            {
                d.ViewPath = "~/Views/Shared/Error_Frame_NoLayout.cshtml";
            }
            else
            {
                d.ViewPath = "~/Views/Shared/Error_Frame.cshtml";
            }
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
            //此處error頁面的跳轉處理不用轉向error.view，以免發生死循環
            d[DomainKey.VIEW_LIST, "ErrorTitle"] = "系统出错了";
            if ((bool)p[DomainKey.CONFIG, "DebugMode"])
            {
                d[DomainKey.VIEW_LIST, "ErrorMsg"] = string.Format("出錯了，{0}", errlog);
            }
            else
            {
                d[DomainKey.VIEW_LIST, "ErrorMsg"] = string.Format("系统出错了，请联系相关人员帮助处理，并告知其错误编号。谢谢！（错误编号：{0}）", errorCode);
            }
            p.Resources.RollbackTransaction(p.CurrentTransToken);
            p.Resources.ReleaseAll();
            GlobalCommon.Logger.WriteLog(LoggerLevel.ERROR, errlog);
            //寫入DB
            //ExceptionCategory.ExceptionToDB(p, d, ex);

            WMvcView.RenderView(p, d, CurrentContext, CurrentContext.Response.Output);
        }

        protected override void Init(System.Web.HttpContext context, WebParameter p, WMvcData d)
        {
            base.Init(context, p, d);
            if (context.Session["LoginInfo"] != null)
            {
                p.LoginInfo = (LoginUserData)context.Session["LoginInfo"];
            }
            else
            {
                p.LoginInfo = null;
            }
            if (context.Application["sysinfo"] != null)
            {
                var sysinfo = (FrameDLRObject)context.Application["sysinfo"];
                var title = sysinfo.GetValue("sys_name") + " " + sysinfo.GetValue("sys_version");
                d.SetValue(DomainKey.VIEW_LIST, "sysname", title);
            }
            GlobalPrepare.ConfigPrepare(ref p);
        }

        protected override bool RunMe(WebParameter p, WMvcData d)
        {
            try
            {
                //1.进行预处理
                p.CanContinue = true;
                bool isSuccess = ModuleProxyManager<WebParameter, WMvcData>.Call<PreProcessProxy>(p, d);
                //2.业务逻辑处理
                if (isSuccess && p.CanContinue)
                    isSuccess = isSuccess & ModuleProxyManager<WebParameter, WMvcData>.Call<BusinessProxy>(p, d);
                

                return isSuccess;
            }
            finally
            {
                p.Resources.ReleaseAll();
            }
        }

        private void ProcessHTML(HttpContext context, WebParameter p, WMvcData d)
        {
            ModuleProxyManager<WebParameter, WMvcData>.Call<ViewAfterProcessProxy>(p, d);
            var html = ComFunc.nvl(d.GetValue("ViewHtmlCode"));
            d["ViewHtmlCode"] = html;
        }

        protected override void AfterProcess(HttpContext context, WebParameter p, WMvcData d)
        {
            base.AfterProcess(context, p, d);
            ////Cache生成的HTML
            //string htmlcode = ComFunc.nvl(d["ViewHtmlCode"]);
            //bool isCache = bool.Parse(ComFunc.nvl(d["IsCacheHTML"]) == "" ? "false" : ComFunc.nvl(d["IsCacheHTML"]));
            //if (isCache)
            //{
            //    //if (htmlcode != "" && !string.IsNullOrEmpty(p.SessionID))
            //    //{
            //    //    HttpRuntime.Cache.Insert("ViewHistoryHTML_" + p.SessionID, d.GetValue("ViewHtmlCode"));
            //    //}
            //    if (htmlcode != "" && !string.IsNullOrEmpty(ComFunc.nvl(context.Session["MemoryCar_SessionGUID"])))
            //    {
            //        HttpRuntime.Cache.Insert("ViewHistoryHTML_" + ComFunc.nvl(context.Session["MemoryCar_SessionGUID"]), d.GetValue("ViewHtmlCode"));
            //    }
            //}
        }

        public override string Name
        {
            get { return "WMvcHandler"; }
        }

        public override string Version
        {
            get { return "0.0.1"; }
        }

        public override string Description
        {
            get { return "WMvc资源请求处理器"; }
        }
    }
}
