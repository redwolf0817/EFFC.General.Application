using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unit
{
    public class SampleUnit:BaseDBUnit
    {
        protected override Action<EFFC.Frame.Net.Data.Parameters.UnitParameter, dynamic> SqlFunc(string flag)
        {
            switch (flag.ToLower())
            {
                case "query":
                    return Query;
                default:
                    return null;
            }
        }

        private void Query(EFFC.Frame.Net.Data.Parameters.UnitParameter arg1, dynamic arg2)
        {
            arg2.sql = "select * from otc_dz_dbf where dzghrq=@dzghrq0 order by createtime desc";
        }
    }
}
