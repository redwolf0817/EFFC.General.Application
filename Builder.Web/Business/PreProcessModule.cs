using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using EFFC.Frame.Net.Base.Module;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using Builder.Web.Global;
using EFFC.Frame.Net.Base.Constants;

namespace Builder.Web.Business
{
    public class PreProcessModule:BaseModule<WebParameter,WMvcData>
    {
        public override string Description
        {
            get { return "預處理模塊"; }
        }

        public override string Name
        {
            get { return "PreProcess"; }
        }

        protected override void OnError(Exception ex, WebParameter p, WMvcData d)
        {
            throw ex;
        }

        protected override void Run(WebParameter p, WMvcData d)
        {

            WebParameter wp = (WebParameter)p;
            WMvcData wd = (WMvcData)d;

            //如果为后台系统
        }

        public override string Version
        {
            get { return "0.0.1"; }
        }
    }
}
