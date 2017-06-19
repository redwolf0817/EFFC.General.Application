using Builder.Web.Proxy;
using EFFC.Frame.Net.Base.Module;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Builder.Web.Handler
{
    public class HostHandler : GoHandler
    {
        protected override bool RunMe(WebParameter p, GoData d)
        {
            bool isSuccess = ModuleProxyManager<WebParameter, GoData>.Call<HostBusinessProxy>(p, d);

            return isSuccess;
        }
    }
}
