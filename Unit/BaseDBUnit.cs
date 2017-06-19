using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data.Base;
using EFFC.Frame.Net.Base.Interfaces.Unit;
using EFFC.Frame.Net.Base.ResouceManage;
using EFFC.Frame.Net.Base.ResouceManage.DB;
using EFFC.Frame.Net.Data.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unit
{
    public abstract partial class BaseDBUnit : IDBUnit<UnitParameter>
    {
        private UnitParameter _up = null;
        private string _flag = "";
        private string _connstring = "";
        private ResourceManage _rm;

        public void BindCurrentDBO(string configname)
        {
            _connstring = ComFunc.nvl(_up[DomainKey.CONFIG, configname]);
        }

        protected abstract Action<UnitParameter, dynamic> SqlFunc(string flag);

        public Func<UnitParameter, dynamic> GetSqlFunc(string flag)
        {
            _flag = flag;
            return LoadInvoke;
        }

        private dynamic LoadInvoke(UnitParameter arg)
        {
            Init(arg);

            var m = SqlFunc(_flag);
            var rtn = new FrameDLRObject();
            m(_up, rtn);

            if (_up.Dao == null)
            {
                var token = arg.CurrentTransToken;
                var rm = arg.Resources;
                _up.Dao = rm.CreateInstance<SQLServerAccess>(token);
                if (_connstring == "")
                {
                    BindCurrentDBO("dbconn");
                }
                _up.Dao.Open(_connstring);
            }

            return rtn;
        }

        private void Init(UnitParameter arg)
        {
            _rm = (ResourceManage)arg.GetValue(ParameterKey.RESOURCE_MANAGER);
            _up = arg;
        }



    }
}
