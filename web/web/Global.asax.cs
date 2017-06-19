using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using web.AppCode;

namespace web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //進行Log注冊
            GlobalCommon.Logger = new FrameLog();
            //進行異常處理注冊
            GlobalCommon.ExceptionProcessor = new ExceptionProcess();
            //進行WMvc請求下的Business Assembly的注冊
            GlobalCommon.WMvcCommon.LogicAssemblyPath = "Web.Business";
            //進行Go請求下的Business Assembly的注冊
            GlobalCommon.GoCommon.LogicAssemblyPath = "Web.Business";
            //Host Path
            GlobalCommon.HostCommon.RootPath = MyConfig.GetConfiguration("HostJs_Path");

            GlobalCommon.WebSocketCommon.MaxConnectionMinutes = 1;
            //站点起始页
            GlobalCommon.WebCommon.StartPage = MyConfig.GetConfiguration("FrameStartPage");
            //站点的缓存器
            GlobalCommon.ApplicationCache = new WebCache();
        }
    }
}
