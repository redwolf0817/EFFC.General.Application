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
    public class ViewBusinessModule:AssemblyLoadBusinessModule<WebParameter,WMvcData,ViewLogic>
    {
        public override string LogicAssemblyPath
        {
            get { return GlobalCommon.WMvcCommon.LogicAssemblyPath; }
        }

        public override string LogicName
        {
            get { return this.Parameter.RequestResourceName; }
        }

        public override string Description
        {
            get { return "業務處理模塊"; }
        }

        public override string Name
        {
            get { return "ViewBusiness"; }
        }

        protected override void OnError(Exception ex, WebParameter p, WMvcData d)
        {
            throw ex;
        }

        public override string Version
        {
            get { return "0.0.1"; }
        }
    }
}
