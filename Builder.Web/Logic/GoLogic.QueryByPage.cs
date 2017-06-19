using Builder.Web.Constant;
using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data;
using EFFC.Frame.Net.Base.ResouceManage.DB;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.UnitData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Builder.Web.Logic {
	public abstract partial class GoLogic {
        JoJoDBHelper _db;
        /// <summary>
        /// db操作相关
        /// </summary>
		public override DBHelper DB {
			get {
                if (_db == null)
                    _db = new JoJoDBHelper(this);

                return _db;
            }
        }

		public class JoJoDBHelper : DBHelper {
            GoLogic _logic = null;

            public JoJoDBHelper(GoLogic logic)
				: base(logic) {
                _logic = logic;
            }

			public override UnitDataCollection QueryByPage<T>(UnitParameter p, string actionflag) {
				if (_logic.CallContext_Parameter[DomainKey.POST_DATA, KeyDics.QueryByPage.ToPage] != null) {
                    p.ToPage = IntStd.ParseStd(_logic.CallContext_Parameter[DomainKey.POST_DATA, KeyDics.QueryByPage.ToPage]);
				} else if (_logic.CallContext_Parameter[DomainKey.QUERY_STRING, KeyDics.QueryByPage.ToPage] != null) {
                    p.ToPage = IntStd.ParseStd(_logic.CallContext_Parameter[DomainKey.QUERY_STRING, KeyDics.QueryByPage.ToPage]);
                }
                //easyui使用的参数
                else if (_logic.CallContext_Parameter[DomainKey.QUERY_STRING, "page"] != null)
                {
                    p.ToPage = IntStd.ParseStd(_logic.CallContext_Parameter[DomainKey.QUERY_STRING, "page"]);
                }
                else if (_logic.CallContext_Parameter[DomainKey.POST_DATA, "page"] != null)
                {
                    p.ToPage = IntStd.ParseStd(_logic.CallContext_Parameter[DomainKey.POST_DATA, "page"]);
                }
                else
                {
                    p.ToPage = 1;
                }
				if (_logic.CallContext_Parameter[DomainKey.POST_DATA, KeyDics.QueryByPage.Count_per_Page] != null) {
                    p.Count_Of_OnePage = IntStd.ParseStd(_logic.CallContext_Parameter[DomainKey.POST_DATA, KeyDics.QueryByPage.Count_per_Page]);
				} else if (_logic.CallContext_Parameter[DomainKey.QUERY_STRING, KeyDics.QueryByPage.Count_per_Page] != null) {
                    p.Count_Of_OnePage = IntStd.ParseStd(_logic.CallContext_Parameter[DomainKey.QUERY_STRING, KeyDics.QueryByPage.Count_per_Page]);
                }
                //easyui使用的参数
                else if (_logic.CallContext_Parameter[DomainKey.QUERY_STRING, "rows"] != null)
                {
                    p.Count_Of_OnePage = IntStd.ParseStd(_logic.CallContext_Parameter[DomainKey.QUERY_STRING, "rows"]);
                }
                else if (_logic.CallContext_Parameter[DomainKey.POST_DATA, "rows"] != null)
                {
                    p.Count_Of_OnePage = IntStd.ParseStd(_logic.CallContext_Parameter[DomainKey.POST_DATA, "rows"]);
                }
                else {
                    p.Count_Of_OnePage = _logic.CallContext_Parameter[DomainKey.CONFIG, KeyDics.QueryByPage.Count_per_Page] != null ? IntStd.ParseStd(_logic.CallContext_Parameter[DomainKey.CONFIG, KeyDics.QueryByPage.Count_per_Page]).Value : 10;
                }
                UnitDataCollection rtn = base.QueryByPage<T>(p, actionflag);

                _logic.CallContext_DataCollection.SetValue(KeyDics.QueryByPage.Count_per_Page, rtn.Count_Of_OnePage);
                _logic.CallContext_DataCollection.SetValue(KeyDics.QueryByPage.CurrentPage, rtn.CurrentPage);
                _logic.CallContext_DataCollection.SetValue(KeyDics.QueryByPage.Total_Page, rtn.TotalPage);
                _logic.CallContext_DataCollection.SetValue(KeyDics.QueryByPage.Total_Row, rtn.TotalRow);
                return rtn;
            }

            public override UnitParameter NewDBUnitParameter()
            {
                var rtn = base.NewDBUnitParameter<SQLServerAccess>();
                rtn.Dao.Open(rtn.DBConnString);
                return rtn;
            }
            /// <summary>
            /// 创建默认的mongodb连接参数
            /// </summary>
            /// <returns></returns>
            public new UnitParameter NewMongoUnitParameter()
            {
                var rtn = base.NewDBUnitParameter<MongoAccess26>();
                rtn.Dao.Open(ComFunc.nvl(_logic.Configs["mongodb"]),"jumeiyi");
                return rtn;
            }
        }
    }
}
