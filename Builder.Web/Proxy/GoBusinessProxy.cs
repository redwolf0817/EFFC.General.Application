using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFFC.Frame.Net.Base.Module;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using Builder.Business.Module;

namespace Builder.Web.Proxy
{
    public class GoBusinessProxy : LocalModuleProxy<WebParameter, GoData>
    {
        protected override BaseModule<WebParameter, GoData> GetModule(WebParameter p, GoData data)
        {
            return new GoBusinessModule();
        }

        public override void OnError(Exception ex, WebParameter p, GoData data)
        {
            throw ex;
        }
    }
}
