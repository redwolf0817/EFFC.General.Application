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
    public class PreWeixinProcessProxy : LocalModuleProxy<WebParameter, GoData>
    {
        protected override BaseModule<WebParameter, GoData> GetModule(WebParameter p, GoData data)
        {
            return new PreWeixinProcessModule();
        }

        public override void OnError(Exception ex, WebParameter p, GoData data)
        {
            throw ex;
        }
    }
}
