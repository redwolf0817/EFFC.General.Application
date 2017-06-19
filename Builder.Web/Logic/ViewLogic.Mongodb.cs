using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.ResouceManage.DB;

namespace Builder.Web.Logic
{
    public abstract partial class ViewLogic
    {
        MongoAccess26 _mh = null;
        public MongoAccess26 Mongo
        {
            get
            {
                var config = ComFunc.nvl(Configs["mongodb"]);
                var dbname = config == "" ? "" : config.Substring(config.LastIndexOf("/"));
                if (_mh == null) _mh = new MongoAccess26(ComFunc.nvl(Configs["mongodb"]), dbname);
                return _mh;
            }
        }
    }
}

