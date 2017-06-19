using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Builder.Web.Logic
{
    public abstract partial class ViewLogic
    {
        OuterInterfaceHelper _oifh = null;
        public new OuterInterfaceHelper OuterInterface
        {
            get
            {
                if (_oifh == null) _oifh = new OuterInterfaceHelper(this);
                return _oifh;
            }


        }
        public class OuterInterfaceHelper
        {
            ViewLogic _logic = null;

            public OuterInterfaceHelper(ViewLogic logic)
            {
                _logic = logic;
            }
        }

    }
}

