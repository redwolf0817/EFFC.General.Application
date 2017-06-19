
using Builder.Web.Business;
using EFFC.Frame.Net.Base.Module;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Builder.Web.Proxy
{
    public class ViewAfterProcessProxy : LocalModuleProxy<WebParameter, WMvcData>
    {
        protected override BaseModule<WebParameter, WMvcData> GetModule(WebParameter p, WMvcData data)
        {
            return new ViewAfterProcessModule();
        }

        public override void OnError(Exception ex, WebParameter p, WMvcData data)
        {
            throw ex;
        }
    }
}
