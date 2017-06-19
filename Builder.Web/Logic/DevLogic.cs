using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data.Base;
using EFFC.Frame.Net.Business.Logic;
using EFFC.Frame.Net.Data.LogicData;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Builder.Web.Logic
{
    /// <summary>
    /// 用于识别HostJs开发使用的接口
    /// </summary>
    public abstract partial class DevLogic : GoLogic
    {

        protected override void DoInvoke(WebParameter p, GoData d, LogicData ld)
        {
            var isdebug = bool.Parse(ComFunc.nvl(this.Configs["DebugMode"]));
            //只有debug模式才能进行开发
            if (isdebug)
            {
                var func = GetFunction(p.Action);

                if (GetFunction(p.Action) != null)
                {
                    d.ResponseData = GetFunction(p.Action)(ld);
                }
            }
            else
            {
                d.ResponseData = FrameDLRObject.CreateInstanceFromat(@"{
issuccess:false,
msg:{0}
}", "You can't do programming when DebugMode is false");
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
    }
}
