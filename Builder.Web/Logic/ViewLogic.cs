using Builder.Web.Constant;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data;
using EFFC.Frame.Net.Business.Logic;
using EFFC.Frame.Net.Data.LogicData;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Builder.Web.Logic
{
    public abstract partial class ViewLogic : WebBaseLogic<WebParameter, WMvcData>
    {

        
        /// <summary>
        /// 初始化执行部分
        /// </summary>
        /// <param name="actionname"></param>
        public abstract Action<LogicData> GetAction(string actionname);

        /// <summary>
        /// Response跳转
        /// </summary>
        /// <param name="touri"></param>
        public void RedirectTo(string touri)
        {
            this.CallContext_DataCollection.RedirectUri = touri;
        }

        /// <summary>
        /// Response跳转
        /// </summary>
        /// <param name="touri"></param>
        /// <param name="encoder"></param>
        public void RedirectTo(string touri, Encoding encoder)
        {
            this.CallContext_DataCollection.RedirectUri = HttpUtility.UrlEncode(touri, encoder);
        }
        /// <summary>
        /// 填入ViewPath，格式为“~/Views/xxxx/xxxx.cshtml”
        /// </summary>
        /// <param name="viewpath"></param>
        public void SetViewPath(string viewpath)
        {
            this.CallContext_DataCollection.ViewPath = viewpath;
        }
        /// <summary>
        /// 向ViewData中新增或更新一个参数，用于View页面的ViewData使用
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetViewData(string key, object value)
        {
            this.CallContext_DataCollection[DomainKey.VIEW_LIST, key] = value;
        }
        /// <summary>
        /// 填入起始view文件的名称，默认为_ViewStart
        /// </summary>
        /// <param name="startviewname"></param>
        public void SetStartView(string startviewname)
        {
            this.CallContext_DataCollection.StartViewName = startviewname;
        }
        /// <summary>
        /// 写入一个moduledata，供View使用
        /// </summary>
        /// <param name="obj"></param>
        public void SetMvcModuleData(object obj)
        {
            this.CallContext_DataCollection.MvcModuleData = obj;
        }
        /// <summary>
        /// 設定是否需要緩存本次請求的html
        /// </summary>
        /// <param name="isCache"></param>
        public void SetCacheHTML(bool isCache)
        {
            this.CallContext_DataCollection.SetValue("IsCacheHTML", isCache);
        }
        protected override void DoInvoke(WebParameter p, WMvcData d, LogicData ld)
        {
            //添加翻页信息
            if (d[DomainKey.POST_DATA, KeyDics.QueryByPage.ToPage] != null)
            {
                ld.ToPage = IntStd.ParseStd(d[DomainKey.POST_DATA, KeyDics.QueryByPage.ToPage]);
            }
            else
            {
                ld.ToPage = 1;
            }
            if (d[DomainKey.POST_DATA, KeyDics.QueryByPage.Count_per_Page] != null)
            {
                ld.Count_Per_Page = IntStd.ParseStd(d[DomainKey.POST_DATA, KeyDics.QueryByPage.Count_per_Page]);
            }
            else
            {
                ld.Count_Per_Page = p[DomainKey.CONFIG, KeyDics.QueryByPage.Count_per_Page] != null ? IntStd.ParseStd(p[DomainKey.CONFIG, KeyDics.QueryByPage.Count_per_Page]).Value : 10;
            }
            if (d[DomainKey.POST_DATA, KeyDics.QueryByPage.CurrentPage] != null)
            {
                ld.CurrentPage = IntStd.ParseStd(d[DomainKey.POST_DATA, KeyDics.QueryByPage.CurrentPage]);
            }
            else
            {
                ld.CurrentPage = 1;
            }

            SetCacheHTML(false);
            //根据Action呼叫不同的function
            Action<LogicData> delegateA = GetAction(Action);
            if (delegateA != null)
            {
                delegateA(ld);
            }
        }

    }
}

