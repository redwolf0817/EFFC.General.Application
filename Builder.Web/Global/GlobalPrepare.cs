using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.IO;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data;
using EFFC.Frame.Net.Data.WebData;
using EFFC.Frame.Net.Base.ResouceManage;
using EFFC.Frame.Net.Base.Token;
using EFFC.Frame.Net.Base.Module;
using Builder.Web.Proxy;
using EFFC.Frame.Net.Global;
using EFFC.Frame.Net.Base.Data.Base;

namespace Builder.Web.Global
{
    public class GlobalPrepare
    {
        public static void ConfigPrepare(ref WebParameter p)
        {
            p.DBConnectionString = MyConfig.GetConfiguration("dbconn");
            p[DomainKey.CONFIG, ParameterKey.NONSQL_DBCONNECT_STRING] = MyConfig.GetConfiguration("mongodb");
            bool bvalue = true;
            foreach (var item in MyConfig.GetConfigurationList())
            {
                if (bool.TryParse(ComFunc.nvl(item.Value), out bvalue))
                {
                    p[DomainKey.CONFIG, item.Key] = bool.Parse(ComFunc.nvl(item.Value));
                }
                else if (DateTimeStd.IsDateTime(item.Value))
                {
                    p[DomainKey.CONFIG, item.Key] = DateTimeStd.ParseStd(item.Value).Value;
                }
                else
                {
                    p[DomainKey.CONFIG, item.Key] = ComFunc.nvl(item.Value);
                }
            }
            //微信相关信息
            p.ExtentionObj.weixin = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
            p.ExtentionObj.weixin.signature = ComFunc.nvl(p[DomainKey.QUERY_STRING, "signature"]);
            p.ExtentionObj.weixin.timestamp = ComFunc.nvl(p[DomainKey.QUERY_STRING, "timestamp"]);
            p.ExtentionObj.weixin.nonce = ComFunc.nvl(p[DomainKey.QUERY_STRING, "nonce"]);
            p.ExtentionObj.weixin.token = ComFunc.nvl(p[DomainKey.CONFIG, "weixin_token"]);
            p.ExtentionObj.weixin.encrypt_type = ComFunc.nvl(p[DomainKey.QUERY_STRING, "encrypt_type"]);
            p.ExtentionObj.weixin.encrypt_key = ComFunc.nvl(p[DomainKey.CONFIG, "weixin_encry_key"]);
            p.ExtentionObj.weixin.appid = ComFunc.nvl(p[DomainKey.CONFIG, "weixin_Appid"]);
            p.ExtentionObj.weixin.appsecret = ComFunc.nvl(p[DomainKey.CONFIG, "weixin_Appsecret"]);
            
        }
        static List<string> _ingore_auth_list = null;
        /// <summary>
        /// 排除不需要登录的请求
        /// </summary>
        /// <returns></returns>
        public static List<string> LoginExcept()
        {
            if (_ingore_auth_list == null)
            {
                _ingore_auth_list = new List<string>();
                _ingore_auth_list.Add("sys_setting.*");
                _ingore_auth_list.Add("login.*");
                _ingore_auth_list.Add("entry.*.view");
                _ingore_auth_list.Add("sessionout.*.view");
                _ingore_auth_list.Add("debtservice.unitbind.go");
                _ingore_auth_list.Add("oplog.*.go");
                _ingore_auth_list.Add("editpwd.*");
                _ingore_auth_list.Add("goods.getcost.go");
                _ingore_auth_list.Add("hostdev.*.go");
            }
            return _ingore_auth_list;
        }
        static Dictionary<string, string> _map_url = null;
        /// <summary>
        /// 跳转映射
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string RedirectMap(string url)
        {
            return url;
        }
    }
    
}