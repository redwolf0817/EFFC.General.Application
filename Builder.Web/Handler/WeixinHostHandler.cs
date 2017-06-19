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
    public class WeixinHostHandler:WeixinHandler
    {
        protected override bool RunMe(EFFC.Frame.Net.Data.Parameters.WebParameter p, EFFC.Frame.Net.Data.WebData.GoData d)
        {
            try
            {
                p.CanContinue = true;
                bool isSuccess = ModuleProxyManager<WebParameter, GoData>.Call<PreWeixinProcessProxy>(p, d);
                if (isSuccess & p.CanContinue)
                    isSuccess = ModuleProxyManager<WebParameter, GoData>.Call<HostBusinessProxy>(p, d);

                return isSuccess;
            }
            finally
            {
                p.Resources.ReleaseAll();
            }
        }
    }
}
