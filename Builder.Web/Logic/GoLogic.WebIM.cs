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
using System.Web;
using System.Drawing;

namespace Builder.Web.Logic
{
    public abstract partial class GoLogic
    {
        WebIMHelper _webim = null;
        /// <summary>
        /// 环信集成API
        /// </summary>
        public WebIMHelper WebIM
        {
            get
            {
                if (_webim == null) _webim = new WebIMHelper(this);
                return _webim;
            }


        }
        /// <summary>
        /// 环信
        /// </summary>
        public class WebIMHelper
        {
            GoLogic _logic;

            public WebIMHelper(GoLogic logic)
            {
                _logic = logic;
            }
            /// <summary>
            /// 环信下的OrgName
            /// </summary>
            public string OrgName
            {
                get
                {
                    return ComFunc.nvl(_logic.Configs["webim_orgname"]);
                }
            }
            public string APPName
            {
                get
                {
                    return ComFunc.nvl(_logic.Configs["webim_appname"]);
                }
            }
            public string Client_ID
            {
                get
                {
                    return ComFunc.nvl(_logic.Configs["webim_clientid"]);
                }
            }
            public string Client_Secret
            {
                get
                {
                    return ComFunc.nvl(_logic.Configs["webim_clientsecret"]);
                }
            }

            /// <summary>
            /// 获取环信的Access Token，用于与微信服务器进行信息交互
            /// </summary>
            public string Access_Token
            {
                get
                {
                    var key = "webim_access_token_" + OrgName + "#" + APPName;
                    if (_logic.CacheHelper.GetCache(key) == null)
                    {
                        var data = FrameDLRObject.CreateInstance(@"{
grant_type:'client_credentials',
client_id:'" + Client_ID + @"',
client_secret:'" + Client_Secret + @"'
}", FrameDLRFlags.SensitiveCase);

                        var result = _logic.OuterInterface.CallWebIMServer(string.Format("https://a1.easemob.com/{0}/{1}/token", OrgName, APPName), "", "", null, data);
                        dynamic dobj = (FrameDLRObject)result;
                        var token = ComFunc.nvl(dobj.access_token);
                        var expireseconds = "7200";//ComFunc.nvl(dobj.expires_in);
                        if (token != "")
                        {
                            //获取之后将超时时间缩短10秒，微信默认超时时间为7200秒，每获取一次就会重置该token
                            _logic.CacheHelper.SetCache(key, token, DateTime.Now.AddSeconds(IntStd.ParseStd(expireseconds).Value - 10));
                        }
                    }

                    return ComFunc.nvl(_logic.CacheHelper.GetCache(key));
                }
            }
            /// <summary>
            /// 开放注册一个用户
            /// </summary>
            /// <param name="userid"></param>
            /// <param name="pass"></param>
            /// <param name="nickname"></param>
            /// <returns></returns>
            public dynamic RegisterOpen(string userid, string pass, string nickname)
            {
                var data = FrameDLRObject.CreateInstance(@"{
username:'" + userid + @"',
password:'" + pass + @"',
nickname:'" + nickname + @"'
}", FrameDLRFlags.SensitiveCase);

                var result = _logic.OuterInterface.CallWebIMServer(string.Format("https://a1.easemob.com/{0}/{1}/users", OrgName, APPName), "", "", null, data);
                dynamic dobj = (FrameDLRObject)result;
                var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:false,
code:'',
msg:''
}");
                if (dobj.statuscode == 400)
                {
                    rtn.issuccess = false;
                    rtn.code = "Notexits";
                    rtn.msg = "用户已存在、用户名或密码为空、用户名不合法";
                }
                else if (dobj.statuscode == 200)
                {
                    rtn.issuccess = true;
                    rtn.msg = "";
                }
                else
                {
                    rtn.issuccess = false;
                    rtn.code = "Error";
                    rtn.msg = "远程服务器错误";
                }

                return rtn;
            }
            /// <summary>
            /// 授权注册一个用户
            /// </summary>
            /// <param name="userid"></param>
            /// <param name="pass"></param>
            /// <param name="nickname"></param>
            /// <returns></returns>
            public dynamic RegisterAuth(string userid, string pass, string nickname)
            {
                var data = FrameDLRObject.CreateInstance(@"{
username:'" + userid + @"',
password:'" + pass + @"',
nickname:'" + nickname + @"'
}", FrameDLRFlags.SensitiveCase);
                var header = FrameDLRObject.CreateInstance(@"{
Authorization:'Bearer " + Access_Token + @"'
}", FrameDLRFlags.SensitiveCase);

                var result = _logic.OuterInterface.CallWebIMServer(string.Format("https://a1.easemob.com/{0}/{1}/users", OrgName, APPName), "", "", header, data);
                dynamic dobj = (FrameDLRObject)result;
                var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:false,
code:'',
msg:''
}");
                if (dobj.statuscode == 400)
                {
                    rtn.issuccess = false;
                    rtn.code = "Notexits";
                    rtn.msg = "用户已存在、用户名或密码为空、用户名不合法";
                }
                else if (dobj.statuscode == 200)
                {
                    rtn.issuccess = true;
                    rtn.msg = "";
                }
                else
                {
                    rtn.issuccess = true;
                    rtn.code = "Error";
                    rtn.msg = "远程服务器错误";
                }

                return rtn;
            }

            public dynamic GetUser(string userid)
            {
                var header = FrameDLRObject.CreateInstance(@"{
Authorization:'Bearer " + Access_Token + @"'
}", FrameDLRFlags.SensitiveCase);

                var result = _logic.OuterInterface.CallWebIMServer(string.Format("https://a1.easemob.com/{0}/{1}/users/{2}", OrgName, APPName, userid), "", "", header, null);
                dynamic dobj = (FrameDLRObject)result;
                var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:false,
code:'',
msg:''
}");
                if (dobj.statuscode == 404)
                {
                    rtn.issuccess = false;
                    rtn.code = "Notexits";
                    rtn.msg = "用户不存在";
                }
                else if (dobj.statuscode == 200)
                {
                    rtn.issuccess = true;
                    rtn.msg = "";
                    rtn.content = dobj.content;
                }
                else
                {
                    rtn.issuccess = true;
                    rtn.code = "Error";
                    rtn.msg = "远程服务器错误";
                }

                return rtn;
            }
            /// <summary>
            /// 获取历史聊天记录
            /// </summary>
            /// <param name="from">发出人员id</param>
            /// <param name="to">接收人员id</param>
            /// <returns></returns>
            public dynamic GetHistoryMsg(string from, string to)
            {
                return GetHistoryMsg(string.Format("select * where (from='{0}' and to='{1}') or (from='{1}' and to='{0}')", from, to), 10);
            }
            /// <summary>
            /// 获取历史聊天记录
            /// </summary>
            /// <param name="from">发出人员id</param>
            /// <param name="to">接收人员id</param>
            /// <returns></returns>
            public dynamic GetHistoryMsgDesc(string from, string to)
            {
                return GetHistoryMsgDesc(from, to, 10);
            }
            /// <summary>
            /// 获取历史聊天记录
            /// </summary>
            /// <param name="from">发出人员id</param>
            /// <param name="to">接收人员id</param>
            /// <returns></returns>
            public dynamic GetHistoryMsgDesc(string from, string to,int limit)
            {
                return GetHistoryMsg(string.Format("select * where (from='{0}' and to='{1}') or (from='{1}' and to='{0}') order by timestamp desc", from, to), limit);
            }
            /// <summary>
            /// 获取接收方的历史记录
            /// </summary>
            /// <param name="to"></param>
            /// <returns></returns>
            public dynamic GetToHistoryMsg(string to,string cursor)
            {
                return GetToHistoryMsg(to, cursor, 1000);
            }
            /// <summary>
            /// 获取接收方的历史记录
            /// </summary>
            /// <param name="to"></param>
            /// <returns></returns>
            public dynamic GetToHistoryMsg(string to, string cursor,int limit)
            {
                return GetHistoryMsg(string.Format("select * where to='{0}'", to), ComFunc.nvl(cursor), limit);
            }
            /// <summary>
            /// 获取接收方的历史记录
            /// </summary>
            /// <param name="to"></param>
            /// <returns></returns>
            public dynamic GetToHistoryMsgDesc(string to, string cursor, int limit)
            {
                return GetHistoryMsg(string.Format("select * where to='{0}' order by timestamp desc", to), ComFunc.nvl(cursor), limit);
            }
            /// <summary>
            /// 根据查询条件获取历史聊天记录
            /// </summary>
            /// <param name="ql">查询条件</param>
            /// <param name="limit">每页限制笔数</param>
            /// <returns></returns>
            public dynamic GetHistoryMsg(string ql,int limit)
            {
                return GetHistoryMsg(ql, "", limit);
            }
            /// <summary>
            /// 获取历史聊天记录
            /// </summary>
            /// <param name="cursor">上次查询时返回的cursor</param>
            /// <param name="limit">分页笔数</param>
            /// <returns></returns>
            public dynamic GetHistoryMsg(string ql, string cursor, int limit)
            {
                var header = FrameDLRObject.CreateInstance(@"{
Authorization:'Bearer " + Access_Token + @"'
}", FrameDLRFlags.SensitiveCase);

                var result = _logic.OuterInterface.CallWebIMServer(string.Format("https://a1.easemob.com/{0}/{1}/chatmessages?ql={2}&limit={3}&cursor={4}", OrgName, APPName, ComFunc.UrlEncode(ql), limit, cursor), "Get", "", header, null);
                dynamic dobj = (FrameDLRObject)result;
                var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:false,
code:'',
msg:''
}");

                if (dobj.statuscode == 401)
                {
                    rtn.issuccess = false;
                    rtn.code = "noauth";
                    rtn.msg = "未授权";
                }
                else if (dobj.statuscode == 200)
                {
                    rtn.issuccess = true;
                    rtn.msg = "";
                    rtn.content = dobj;
                }
                else
                {
                    rtn.issuccess = true;
                    rtn.code = "Error";
                    rtn.msg = "远程服务器错误";
                }


                return rtn;
            }
            /// <summary>
            /// 发送图片消息
            /// </summary>
            /// <param name="from"></param>
            /// <param name="to"></param>
            /// <param name="file"></param>
            /// <param name="filename"></param>
            /// <returns></returns>
            public dynamic SendPicMessage(string from, string to, byte[] file, string filename)
            {
                var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:false,
code:'',
msg:''
}");
                //Image img = Image.FromStream(file.InputStream);
                //var width = img.Width;
                //var height = img.Height;
                var upre = UploadFile(filename, file);

                if (upre.issuccess)
                {
                    FrameDLRObject entity = upre.content.entities[0];
                    var uuid = ComFunc.nvl(entity.GetValue("uuid"));
                    var secret = ComFunc.nvl(entity.GetValue("share-secret"));


                    var header = FrameDLRObject.CreateInstance(@"{
Authorization:'Bearer " + Access_Token + @"'
}", FrameDLRFlags.SensitiveCase);

                    var data = FrameDLRObject.CreateInstance(@"{
    target_type : 'users',   //users 给用户发消息, chatgroups 给群发消息, chatrooms 给聊天室发消息
    target : ['" + to + @"'],// 注意这里需要用数组,数组长度建议不大于20, 即使只有一个用户,   
                                  // 也要用数组 ['u1'], 给用户发送时数组元素是用户名,给群组发送时  
                                  // 数组元素是groupid
    msg : {  //消息内容
        type : 'img',   // 消息类型
	    url: 'https://a1.easemob.com/" + OrgName + @"/" + APPName + @"/chatfiles/" + uuid + @"',  //成功上传文件返回的uuid
	    filename: '" + filename + @"', // 指定一个文件名
	    secret: '" + secret + @"', // 成功上传文件后返回的secret
	    size : {
          
      }
     },
    from : '" + from + @"', //表示消息发送者, 无此字段Server会默认设置为'from':'admin',有from字段但值为空串('')时请求失败 
}");

                    var result = _logic.OuterInterface.CallWebIMServer(string.Format("https://a1.easemob.com/{0}/{1}/messages", OrgName, APPName), "Post", "", header, data);
                    dynamic dobj = (FrameDLRObject)result;

                    if (dobj.statuscode == 401)
                    {
                        rtn.issuccess = false;
                        rtn.code = "noauth";
                        rtn.msg = "未授权";
                    }
                    else if (dobj.statuscode == 200)
                    {
                        rtn.issuccess = true;
                        rtn.msg = "";
                        rtn.content = dobj;
                    }
                    else
                    {
                        rtn.issuccess = true;
                        rtn.code = "Error";
                        rtn.msg = "远程服务器错误";
                    }

                }
                else
                {
                    rtn.issuccess = false;
                    rtn.msg = "上传失败";
                }

                return rtn;
            }
            /// <summary>
            /// 发送图片消息
            /// </summary>
            /// <param name="from"></param>
            /// <param name="to"></param>
            /// <param name="file"></param>
            /// <returns></returns>
            public dynamic SendPicMessage(string from, string to, HttpPostedFile file)
            {
                byte[] bfile = ComFunc.StreamToBytes(file.InputStream);
                return SendPicMessage(from, to, bfile, file.FileName);
            }
            /// <summary>
            /// 上传文件
            /// </summary>
            /// <param name="file"></param>
            /// <returns></returns>
            public dynamic UploadFile(HttpPostedFile file)
            {
                var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:false,
code:'',
msg:''
}");
                if (file.ContentLength > 10 * 1024 * 1024)
                {
                    rtn.issuccess = false;
                    rtn.msg = "上传文件超过服务器大小限制（10M）";
                    return rtn;
                }
                if (file.ContentLength <= 0)
                {
                    rtn.issuccess = false;
                    rtn.msg = "上传文件大小为0";
                    return rtn;
                }

                var filename = file.FileName;
                

                var fileStream =file.InputStream;
                byte[] bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, bytes.Length);
                // 设置当前流的位置为流的开始
                fileStream.Seek(0, SeekOrigin.Begin);

                return UploadFile(filename, bytes);
            }
            /// <summary>
            /// 上传文件
            /// </summary>
            /// <param name="name"></param>
            /// <param name="filename"></param>
            /// <param name="filecontenttype"></param>
            /// <param name="file"></param>
            /// <returns></returns>
            public dynamic UploadFile(string filename,byte[] file)
            {
                var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:false,
code:'',
msg:''
}");
                var name = Path.GetFileNameWithoutExtension(filename);
                var ext = Path.GetExtension(filename);
                var filecontenttype = "";
                switch (ext.Replace(".", ""))
                {
                    case "jpg":
                        filecontenttype = ResponseHeader_ContentType.jpg;
                        break;
                    case "jpeg":
                        filecontenttype = ResponseHeader_ContentType.jpeg;
                        break;
                    case "gif":
                        filecontenttype = ResponseHeader_ContentType.gif;
                        break;
                    case "bmp":
                        filecontenttype = ResponseHeader_ContentType.bmp;
                        break;
                    case "png":
                        filecontenttype = ResponseHeader_ContentType.png;
                        break;
                }


                var header = FrameDLRObject.CreateInstance(@"{
'restrict-access':true,
Authorization:'Bearer " + Access_Token + @"'
}", FrameDLRFlags.SensitiveCase);

                
                if (filecontenttype == "")
                {
                    rtn.issuccess = false;
                    rtn.msg = "不支持的文件类型（" + ext.Replace(".", "") + "）";
                    return rtn;
                }

                var data = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
                data.name = "file";
                data.filename = filename;
                data.filecontenttype = "application/octet-stream";
                data.filecontent = file;
                //data.file = Encoding.UTF8.GetString(file);

                var result = _logic.OuterInterface.CallWebIMServer(string.Format("https://a1.easemob.com/{0}/{1}/chatfiles", OrgName, APPName), "Post", "multipart/form-data", header, data);
                dynamic dobj = (FrameDLRObject)result;
                

                if (dobj.statuscode == 401)
                {
                    rtn.issuccess = false;
                    rtn.code = "noauth";
                    rtn.msg = "未授权";
                }
                else if (dobj.statuscode == 200)
                {
                    rtn.issuccess = true;
                    rtn.msg = "";
                    rtn.content = dobj;
                }
                else
                {
                    rtn.issuccess = true;
                    rtn.code = "Error";
                    rtn.msg = "远程服务器错误";
                }


                return rtn;
            }
        }
    }
}
