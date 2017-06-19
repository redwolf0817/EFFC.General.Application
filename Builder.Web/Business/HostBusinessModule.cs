using Builder.Web.Logic;
using EFFC.Frame.Net.Business.Module;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Builder.Web.Business
{
    public class HostBusinessModule : LocalLoadHostBusinessModule<FrameHostJsLogic, WebParameter, GoData>
    {

        public override string Description
        {
            get { return "Host引擎架构"; }
        }

        public override string Name
        {
            get { return "hostbusiness"; }
        }

        protected override void OnError(Exception ex, WebParameter p, GoData d)
        {
            throw ex;
        }

        public override string Version
        {
            get { return "0.0.1"; }
        }
    }
}
