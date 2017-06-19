using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFFC.Frame.Net.Base.Module;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using Builder.Web.Business;

namespace Builder.Web.Proxy
{
    public class PreProcessProxy:LocalModuleProxy<WebParameter,WMvcData>
    {
        protected override BaseModule<WebParameter, WMvcData> GetModule(WebParameter p, WMvcData data)
        {
            return new PreProcessModule();
        }

        public override void OnError(Exception ex, WebParameter p, WMvcData data)
        {
            throw ex;
        }
    }
}
