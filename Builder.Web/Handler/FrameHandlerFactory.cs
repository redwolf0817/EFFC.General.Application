using Builder.Web.Global;
using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Data.Base;
using EFFC.Frame.Net.Global;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Builder.Web.Handler
{
    public class FrameHandlerFactory : IHttpHandlerFactory
    {
        static IHttpHandler sataichandler = null;
        static Dictionary<string,IHttpHandler> _pool = new Dictionary<string,IHttpHandler>();
        public FrameHandlerFactory()
        {
            Type type = typeof(HttpApplication).Assembly.GetType("System.Web.StaticFileHandler", true);
            sataichandler = (IHttpHandler)Activator.CreateInstance(type, true);
        }
        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            //提供CORS的Prelight认证
            if (context.Request.HttpMethod.ToUpper().Equals("OPTIONS"))
            {
                //通知客户端允许预检请求。并设置缓存时间
                context.Response.ClearContent();
                context.Response.AddHeader("Access-Control-Allow-Origin", ComFunc.nvl(MyConfig.GetConfiguration("Access-Control-Allow-Origin")));
                context.Response.AddHeader("Access-Control-Allow-Methods", ComFunc.nvl(MyConfig.GetConfiguration("Access-Control-Allow-Methods")));
                context.Response.AddHeader("Access-Control-Allow-Headers", ComFunc.nvl(MyConfig.GetConfiguration("Access-Control-Allow-Headers")));
                context.Response.AddHeader("Access-Control-Max-Age", ComFunc.nvl(MyConfig.GetConfiguration("Access-Control-Max-Age")));
                //此过程无需返回数据
                context.Response.End();

                return null;
            }
            else if ("POST,GET".Contains(context.Request.HttpMethod.ToUpper()))
            {
                context.Response.AddHeader("Access-Control-Allow-Origin", ComFunc.nvl(MyConfig.GetConfiguration("Access-Control-Allow-Origin")));
                context.Response.AddHeader("Access-Control-Allow-Methods", ComFunc.nvl(MyConfig.GetConfiguration("Access-Control-Allow-Methods")));
                context.Response.AddHeader("Access-Control-Allow-Headers", ComFunc.nvl(MyConfig.GetConfiguration("Access-Control-Allow-Headers")));
                context.Response.AddHeader("Access-Control-Max-Age", ComFunc.nvl(MyConfig.GetConfiguration("Access-Control-Max-Age")));
            }

            var tourl = url;
            if (url == "" || url == "/")
            {
                tourl = url + (ComFunc.nvl(GlobalCommon.WebCommon.StartPage) == "" ? "index.html" : GlobalCommon.WebCommon.StartPage.Replace("~",""));
            }
            else
            {
                tourl = GlobalPrepare.RedirectMap(url);
            }

            var ext = Path.GetExtension(tourl).Replace(".", "").ToLower();
            //没有扩展名则默认为go请求
            if (ext == "")
            {
                ext = "go";
            }
            context.RewritePath("~" + tourl);

            IHttpHandler hanlder = _pool.ContainsKey(ext) ? _pool[ext] : null;
            if (hanlder == null)
            {
                if (ext == "go")
                {
                    hanlder = new GoHandler();
                }
                else if (ext == "view")
                {
                    hanlder = new ViewHandler();
                }
                else if (ext == "hgo")
                {
                    hanlder = new HostHandler();
                }
                else
                {
                    hanlder = sataichandler;
                }

                if (hanlder.IsReusable
                    && hanlder != sataichandler)
                {
                    _pool.Add(ext, hanlder);
                }
            }

            return hanlder;
        }

        public void ReleaseHandler(IHttpHandler handler)
        {
            if (handler is IDisposable)
            {
                ((IDisposable)handler).Dispose();
            }
            //如果使用的内存超过500MB，则强制释放
            if (ComFunc.GetProcessUsedMemory() > 500)
            {
                ComFunc.MemoryCollect();
            }
        }
    }
}
