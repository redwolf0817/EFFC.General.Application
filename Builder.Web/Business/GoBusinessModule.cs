using Builder.Web.Logic;
using EFFC.Frame.Net.Business.Module;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using EFFC.Frame.Net.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Builder.Business.Module
{
    public class GoBusinessModule : AssemblyLoadBusinessModule<WebParameter, GoData, GoLogic>
    {
        public override string LogicAssemblyPath
        {
            get { return GlobalCommon.GoCommon.LogicAssemblyPath; }
        }

        public override string LogicName
        {
            get { return Parameter.RequestResourceName; }
        }

        public override string Description
        {
            get { return "Go請求模式下的業務處理邏輯"; }
        }

        public override string Name
        {
            get { return "GoBusiness"; }
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
