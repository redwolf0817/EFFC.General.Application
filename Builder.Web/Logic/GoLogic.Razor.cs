﻿using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.ResouceManage.FTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Builder.Web.Logic
{
    public abstract partial class GoLogic
    {
        RazorHelper _razor = null;
        public RazorHelper Razor
        {
            get
            {
                if (_razor == null) _razor = new RazorHelper(this);
                return _razor;
            }


        }
        public class RazorHelper
        {
            GoLogic _logic;

            public RazorHelper(GoLogic logic)
            {
                _logic = logic;
            }

            /// <summary>
            /// 填入ViewPath，格式为“~/Views/xxxx/xxxx.cshtml”
            /// </summary>
            /// <param name="viewpath"></param>
            public void SetViewPath(string viewpath)
            {
                _logic.CallContext_DataCollection.ViewPath = viewpath;
            }
            /// <summary>
            /// 向ViewData中新增或更新一个参数，用于View页面的ViewData使用
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            public void SetViewData(string key, object value)
            {
                _logic.CallContext_DataCollection[DomainKey.VIEW_LIST, key] = value;
            }
            /// <summary>
            /// 填入起始view文件的名称，默认为_ViewStart
            /// </summary>
            /// <param name="startviewname"></param>
            public void SetStartView(string startviewname)
            {
                _logic.CallContext_DataCollection.StartViewName = startviewname;
            }
            /// <summary>
            /// 写入一个moduledata，供View使用
            /// </summary>
            /// <param name="obj"></param>
            public void SetMvcModuleData(object obj)
            {
                if (obj != null)
                    _logic.CallContext_DataCollection.MvcModuleData = obj;
            }
        }
    }
}
