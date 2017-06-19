using Builder.Web.Attributes;
using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data.Base;
using EFFC.Frame.Net.Business.Logic;
using EFFC.Frame.Net.Data;
using EFFC.Frame.Net.Data.LogicData;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using EFFC.Frame.Net.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Builder.Web.Logic
{
    public abstract partial class GoLogic : WebBaseLogic<WebParameter, GoData>
    {
        protected abstract Func<LogicData, object> GetFunction(string actionName);
        private LoginUserData _loginInfo;

        protected override void DoInvoke(WebParameter p, GoData d, LogicData ld)
        {
            var func = GetFunction(p.Action);
            var isneedlogin = false;
            var loginurl = "";
            foreach (var attr in this.GetType().GetMethod(func.Method.Name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetCustomAttributes(false))
            {
                if (attr is LoginAttribute)
                {
                    isneedlogin = ((LoginAttribute)attr).IsNeedLogin;
                    loginurl = ((LoginAttribute)attr).LoginUrl;
                    break;
                }
                Console.WriteLine(attr);
            }

            if (isneedlogin && LoginInfo == null)
            {
                d.ResponseData = FrameDLRObject.CreateInstance(@"{
__isneedlogin__:true,
__loginurl__:'" + loginurl + @"',
}");
            }
            else
            {
                if (GetFunction(p.Action) != null)
                {
                    d.ResponseData = GetFunction(p.Action)(ld);

                    if (d.ContentType == GoResponseDataType.RazorView)
                    {
                        if (d.ResponseData == null) d.ResponseData = FrameDLRObject.CreateInstance();
                        if (d.MvcModuleData == null) d.MvcModuleData = d.ResponseData;
                    }
                }
            }
        }

        public LoginUserData CurrentLoginUser
        {
            get
            {
                if (_loginInfo == null)
                    _loginInfo = this.CallContext_Parameter.LoginInfo.Clone<LoginUserData>();

                return _loginInfo;
            }
        }

        /// <summary>
        /// 设定responsedata的数据类型
        /// </summary>
        /// <param name="type"></param>
        public void SetContentType(GoResponseDataType type)
        {
            this.CallContext_DataCollection.ContentType = type;
        }

        /// <summary>
        /// Response跳转
        /// </summary>
        /// <param name="touri"></param>
        public void RedirectTo(string touri)
        {
            this.CallContext_DataCollection.RedirectUri = touri;//HttpUtility.UrlEncode(touri, Encoding.UTF8);
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
        /// 设定下载文件的名称
        /// </summary>
        /// <param name="filename"></param>
        public void SetDownLoadFileName(string filename)
        {
            this.CallContext_DataCollection["__download_filename__"] = filename;
        }
        /// <summary>
        /// 判断当前系统是否处于Debug开发模式
        /// </summary>
        public bool IsDebug
        {
            get
            {
                if (ComFunc.nvl(Configs["DebugMode"]) == "")
                {
                    return false;
                }
                else
                {
                    return bool.Parse(ComFunc.nvl(Configs["DebugMode"]));
                }
            }
        }
    }
}
