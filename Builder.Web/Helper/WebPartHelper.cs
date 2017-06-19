using Builder.Web.Handler;
using Builder.Web.Proxy;
using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Module;
using EFFC.Frame.Net.Base.ResouceManage;
using EFFC.Frame.Net.Base.Token;
using EFFC.Frame.Net.Business.Engine;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Builder.Web.Helper
{
    public static class WebPartHelper
    {
        /// <summary>
        /// 加载一个ViewPart,沿用当前的数据集
        /// </summary>
        /// <param name="viewpath"></param>
        /// <param name="viewdata"></param>
        /// <returns></returns>
        public static MvcHtmlString LoadPartView(string viewpath, ViewDataDictionary viewdata)
        {
            StringWriter sw = new StringWriter();
            ControllerContext cc = new ControllerContext();
            cc.HttpContext = new HttpContextWrapper(HttpContext.Current);
            cc.RouteData = new RouteData();
            cc.RouteData.Values.Add("controller", "webpart");
            cc.RouteData.Values.Add("action", "Process");
            TempDataDictionary tdd = new TempDataDictionary();
            if (string.IsNullOrEmpty(viewpath))
            {
                throw new Exception("没有获得ViewPath，无法展现页面");
            }
            WMvcView rv = new WMvcView(viewpath);
            ViewContext vc = new ViewContext(cc, rv, viewdata, tdd, sw);
            rv.Render(vc, sw);

            return MvcHtmlString.Create(sw.ToString());
        }
        /// <summary>
        /// 加载一个ViewPart，沿用当前的View上下文
        /// </summary>
        /// <param name="viewpath"></param>
        /// <param name="viewcontext"></param>
        /// <returns></returns>
        public static MvcHtmlString LoadPartView(string viewpath, ViewContext viewcontext)
        {
            StringWriter sw = new StringWriter();
            if (string.IsNullOrEmpty(viewpath))
            {
                throw new Exception("没有获得ViewPath，无法展现页面");
            }
            WMvcView rv = new WMvcView(viewpath);
            rv.Render(viewcontext, sw);

            return MvcHtmlString.Create(sw.ToString());
        }
        /// <summary>
        /// 根据Logic.action加载一个PartView
        /// </summary>
        /// <param name="logic"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static MvcHtmlString LoadPartView(string logic, string action, params KeyValuePair<string, object>[] otherParameters)
        {
            ResourceManage rema = new ResourceManage();
            WebParameter p = new WebParameter();
            WMvcData d = new WMvcData();
            StringWriter sw = new StringWriter();
            #region 进行参数和数据准备
            p[DomainKey.SESSION, "SessionID"] = HttpContext.Current.Session.SessionID;
            p.SetValue("ServerRootPath", HttpContext.Current.Server.MapPath("~"));
            Global.GlobalPrepare.ConfigPrepare(ref p);

            p.SetValue<ResourceManage>(ParameterKey.RESOURCE_MANAGER, rema);
            p.SetValue<TransactionToken>(ParameterKey.TOKEN, TransactionToken.NewToken());

            p.Action = action;
            p.RequestResourceName = logic;


            foreach (string s in HttpContext.Current.Request.QueryString.Keys)
            {
                p[DomainKey.QUERY_STRING, s] = HttpContext.Current.Request.QueryString[s];
            }

            foreach (string s in HttpContext.Current.Request.Form.AllKeys)
            {
                d[DomainKey.POST_DATA, s] = HttpContext.Current.Request.Form[s];
            }

            foreach (var item in otherParameters)
            {
                d[DomainKey.POST_DATA, item.Key] = item.Value;
            }

            //获取上传文件的二进制流
            foreach (string s in HttpContext.Current.Request.Files.AllKeys)
            {
                d[DomainKey.UPDATE_FILE, s] = HttpContext.Current.Request.Files[s];
            }

            foreach (string s in HttpContext.Current.Session.Keys)
            {
                p[DomainKey.SESSION, s] = HttpContext.Current.Session[s];
            }
            if (HttpContext.Current.Session["uid"] != null)
            {
                p.LoginInfo = new EFFC.Frame.Net.Data.LoginUserData() { UserID = ComFunc.nvl(HttpContext.Current.Session["uid"]) };
                p.LoginInfo.AccountID = ComFunc.nvl(HttpContext.Current.Session["account"]);
                p.LoginInfo.ExtentionObj.phone = ComFunc.nvl(HttpContext.Current.Session["phone"]);
            }


            #endregion

            #region 呼叫Business Module
            ModuleProxyManager<WebParameter, WMvcData>.Call<BusinessProxy>(p, d);
            #endregion

            #region After Process
            foreach (var s in p.Domain(DomainKey.SESSION))
            {
                HttpContext.Current.Session[s.Key] = s.Value;
            }
            #endregion

            try
            {
                WMvcView.RenderView(p, d, HttpContext.Current, sw);
                return new MvcHtmlString(sw.GetStringBuilder().ToString());
            }
            finally
            {
                sw.Close();
                rema.ReleaseAll();
            }
        }
        /// <summary>
        /// 根据Logic.action加载一个go
        /// </summary>
        /// <param name="logic"></param>
        /// <param name="action"></param>
        /// <param name="otherParameters"></param>
        /// <returns></returns>
        public static object LoadPartGo(string logic, string action, params KeyValuePair<string, object>[] otherParameters)
        {
            ResourceManage rema = new ResourceManage();
            WebParameter p = new WebParameter();
            GoData d = new GoData();
            StringWriter sw = new StringWriter();
            #region 进行参数和数据准备
            p[DomainKey.SESSION, "SessionID"] = HttpContext.Current.Session.SessionID;
            p.SetValue("ServerRootPath", HttpContext.Current.Server.MapPath("~"));
            Global.GlobalPrepare.ConfigPrepare(ref p);

            p.SetValue<ResourceManage>(ParameterKey.RESOURCE_MANAGER, rema);
            p.SetValue<TransactionToken>(ParameterKey.TOKEN, TransactionToken.NewToken());
            try
            {



                p.Action = action;
                p.RequestResourceName = logic;


                foreach (string s in HttpContext.Current.Request.QueryString.Keys)
                {
                    p[DomainKey.QUERY_STRING, s] = HttpContext.Current.Request.QueryString[s];
                }

                foreach (string s in HttpContext.Current.Request.Form.AllKeys)
                {
                    d[DomainKey.POST_DATA, s] = HttpContext.Current.Request.Form[s];
                }

                foreach (var item in otherParameters)
                {
                    d[DomainKey.POST_DATA, item.Key] = item.Value;
                }

                //获取上传文件的二进制流
                foreach (string s in HttpContext.Current.Request.Files.AllKeys)
                {
                    d[DomainKey.UPDATE_FILE, s] = HttpContext.Current.Request.Files[s];
                }

                foreach (string s in HttpContext.Current.Session.Keys)
                {
                    p[DomainKey.SESSION, s] = HttpContext.Current.Session[s];
                }



            #endregion

                #region 呼叫Business Module
                ModuleProxyManager<WebParameter, GoData>.Call<GoBusinessProxy>(p, d);
                #endregion

                #region After Process
                foreach (var s in p.Domain(DomainKey.SESSION))
                {
                    HttpContext.Current.Session[s.Key] = s.Value;
                }
                #endregion

                rema.CommitTransaction(p.CurrentTransToken);
                return d.ResponseData;
            }
            catch (Exception ex)
            {
                rema.RollbackTransaction(p.CurrentTransToken);
                throw ex;
            }
            finally
            {
                rema.ReleaseAll();
            }
        }
        /// <summary>
        /// 加载指定的html做part
        /// </summary>
        /// <param name="htmlpath"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static MvcHtmlString LoadPartHtml(string htmlpath, Encoding encoding)
        {
            WebParameter wp = new WebParameter();
            WMvcData wd = new WMvcData();
            ViewHandler.Prepare(HttpContext.Current, ref wp, ref wd);

            string path = htmlpath.Replace("~", wp.ServerRootPath);
            if (File.Exists(path))
            {
                string htmlstring = File.ReadAllText(path, encoding);
                return new MvcHtmlString(htmlstring);
            }
            else
            {
                throw new FileNotFoundException(string.Format("Not HMTL referenced found,please make sure the path \"{0}\"exits!", htmlpath));
            }

        }

        /// <summary>
        /// 加載翻頁器
        /// </summary>
        /// <param name="viewpath"></param>
        /// <param name="viewcontext"></param>
        /// <returns></returns>
        public static MvcHtmlString LoadPageNavigation(string viewpath, string scriptfunc, ViewContext viewcontext)
        {
            viewcontext.ViewData["_client_submit_func_"] = scriptfunc;
            return LoadPartView(viewpath, viewcontext);
        }
        /// <summary>
        /// 加載默認路徑下的翻頁器
        /// </summary>
        /// <param name="viewcontext"></param>
        /// <returns></returns>
        public static MvcHtmlString LoadPageNavigation(string scriptfunc, ViewContext viewcontext)
        {
            return LoadPageNavigation("~/Views/Shared/PageNavigation_SM.cshtml", scriptfunc, viewcontext);
        }

    }
}
