using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data;
using EFFC.Frame.Net.Base.Data.Base;
using System.Security.Cryptography;
using EFFC.Frame.Net.Global;
using Builder.Web.Helper;
using System.IO;
using System.Net;

namespace Builder.Web.Logic
{
    public abstract partial class GoLogic
    {
        AlyOpenIMHelper _alyopenim = null;
        /// <summary>
        /// 环信集成API
        /// </summary>
        public AlyOpenIMHelper AlyOpenIM
        {
            get
            {
                if (_alyopenim == null) _alyopenim = new AlyOpenIMHelper(this);
                return _alyopenim;
            }


        }
        /// <summary>
        /// 阿里OpenIM
        /// </summary>
        public class AlyOpenIMHelper
        {
            GoLogic _logic;

            public AlyOpenIMHelper(GoLogic logic)
            {
                _logic = logic;
            }
            /// <summary>
            /// AppID
            /// </summary>
            public string AppID
            {
                get
                {
                    return ComFunc.nvl(_logic.Configs["aly_openim_appid"]);
                }
            }
            /// <summary>
            /// APP秘钥
            /// </summary>
            public string AppSecret
            {
                get
                {
                    return ComFunc.nvl(_logic.Configs["aly_openim_secret"]);
                }
            }
            /// <summary>
            /// 添加一个用户
            /// </summary>
            /// <param name="userid"></param>
            /// <param name="password"></param>
            /// <returns></returns>
            public dynamic AddUser(string userid, string password, string name,string nick, string headpic)
            {
                var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:false,
code:'',
msg:''
}");
                var data = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
                data.method = "taobao.openim.users.add";
                data.app_key = AppID;
                data.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                data.format = "json";
                data.v = "2.0";
                data.sign_method = "md5";

                var userinfos = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
                userinfos.userid = userid;
                userinfos.password = password;
                userinfos.name = name;
                userinfos.icon_url = headpic;
                userinfos.nick = nick;
                data.userinfos = ((FrameDLRObject)userinfos).ToJSONString();

                data.sign = GetSign(data, true);


                var url = "http://gw.api.taobao.com/router/rest";
                var result = _logic.OuterInterface.CallAlyOpenIMServer(url, "Post", "", data);
                dynamic dobj = (FrameDLRObject)result;

                if (dobj.statuscode == 200)
                {
                    if (dobj.openim_users_add_response != null)
                    {
                        rtn.issuccess = true;
                        rtn.msg = "";
                    }
                    else
                    {
                        rtn.issuccess = true;
                        if (dobj.error_response != null)
                        {
                            rtn.code = dobj.error_response.code;
                            rtn.msg = dobj.error_response.msg;
                        }
                        else
                        {
                            rtn.msg = "unknown error";
                        }
                    }
                }
                else
                {
                    rtn.issuccess = false;
                    rtn.code = "Error";
                    rtn.msg = "远程服务器错误";
                }

                return rtn;
            }
            private string GetSign(FrameDLRObject p,bool qhs)
            {
                Dictionary<string, string> sd = new Dictionary<string, string>();
                foreach (var k in p.Keys)
                {
                    sd.Add(k, ComFunc.nvl(p.GetValue(k)));
                }
                return GetSign(sd, qhs);
            }
            /// <summary>
            /// 生成数字签名
            /// </summary>
            /// <param name="parameters">要传递的参数</param>
            /// <param name="qhs">是否前后都加密钥进行签名</param>
            /// <returns></returns>
            private string GetSign(IDictionary<string, string> parameters,bool qhs)
            {
                // 第一步：把字典按Key的字母顺序排序
                IDictionary<string, string> sortedParams = new SortedDictionary<string, string>(parameters, StringComparer.Ordinal);
                IEnumerator<KeyValuePair<string, string>> dem = sortedParams.GetEnumerator();

                // 第二步：把所有参数名和参数值串在一起
                StringBuilder query = new StringBuilder(AppSecret);
                while (dem.MoveNext())
                {
                    string key = dem.Current.Key;
                    string value = dem.Current.Value;
                    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                    {
                        query.Append(key).Append(value);
                    }
                }
                if (qhs)
                {
                    query.Append(AppSecret);
                }

                // 第三步：使用MD5加密
                MD5 md5 = MD5.Create();
                byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(query.ToString()));

                // 第四步：把二进制转化为大写的十六进制
                StringBuilder result = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    result.Append(bytes[i].ToString("X2"));
                }

                return result.ToString();
            }
        }

    }
}
