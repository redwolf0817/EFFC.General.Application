using Builder.Web.Business;
using EFFC.Frame.Net.Base.Module;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Builder.Web.Proxy
{
    public class PreProcessGoProxy : LocalModuleProxy<WebParameter, GoData>
    {
        protected override BaseModule<WebParameter, GoData> GetModule(WebParameter p, GoData GoData)
        {
            return new PreProcessGoModule();
        }

        public override void OnError(Exception ex, WebParameter p, GoData data)
        {
            throw ex;
        }
    }
}
