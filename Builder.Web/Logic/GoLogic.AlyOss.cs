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
using System.Globalization;

namespace Builder.Web.Logic
{
    public abstract partial class GoLogic
    {
        AlyOssHelper _alyoss = null;
        /// <summary>
        /// 阿里云OSS集成API
        /// </summary>
        public AlyOssHelper AlyOss
        {
            get
            {
                if (_alyoss == null) _alyoss = new AlyOssHelper(this);
                return _alyoss;
            }


        }
        /// <summary>
        /// 阿里云OSS集成API
        /// </summary>
        public class AlyOssHelper
        {
            GoLogic _logic;
            public AlyOssHelper(GoLogic logic)
            {
                _logic = logic;
            }
            /// <summary>
            /// AppID
            /// </summary>
            public string AccessKey
            {
                get
                {
                    return ComFunc.nvl(_logic.Configs["aly_access_key"]);
                }
            }
            /// <summary>
            /// APP秘钥
            /// </summary>
            public string AccessSecret
            {
                get
                {
                    return ComFunc.nvl(_logic.Configs["aly_access_secret"]);
                }
            }
            /// <summary>
            /// 当前默认的Oss的BucketName
            /// </summary>
            public string CurrentBucket
            {
                get
                {
                    return ComFunc.nvl(_logic.Configs["oss_bucket"]);
                }
            }
            /// <summary>
            /// 当前默认的Oss服务器的EndPoint
            /// </summary>
            public string ServerEndPoint
            {
                get
                {
                    return ComFunc.nvl(_logic.Configs["oss_endpoint"]);
                }
            }
            /// <summary>
            /// 当前默认oss服务器的URI
            /// </summary>
            public string CurrentURI
            {
                get
                {
                    return string.Format("http://{0}.{1}/", CurrentBucket, ServerEndPoint);
                }
            }
            /// <summary>
            /// 获取数字签名
            /// </summary>
            /// <param name="verb"></param>
            /// <param name="content_md5"></param>
            /// <param name="content_type"></param>
            /// <param name="date"></param>
            /// <param name="canonicalizedOSSHeaders"></param>
            /// <param name="canonicalizedResource"></param>
            /// <returns></returns>
            private string GetSign(string verb, string content_md5, string content_type, DateTime date, string canonicalizedOSSHeaders, string canonicalizedResource)
            {
                var sb = new StringBuilder();
                sb.Append(verb.ToUpperInvariant());
                sb.Append("\n");
                sb.Append(ComFunc.nvl(content_md5));
                sb.Append("\n");
                sb.Append(ComFunc.nvl(content_type));
                sb.Append("\n");
                //日期为GMT格式
                sb.Append(date.ToUniversalTime().ToString("r"));
                sb.Append("\n");
                if (!string.IsNullOrEmpty(canonicalizedOSSHeaders))
                {
                    sb.Append(canonicalizedOSSHeaders);
                    sb.Append("\n");
                }
                sb.Append(ComFunc.nvl(canonicalizedResource));

                using (var algorithm = KeyedHashAlgorithm.Create("HMACSHA1"))
                {
                    algorithm.Key = Encoding.UTF8.GetBytes(AccessSecret.ToCharArray());
                    return Convert.ToBase64String(
                        algorithm.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString().ToCharArray())));
                }

                
            }
            /// <summary>
            /// 获取上传资源的MD5签名
            /// </summary>
            /// <param name="input"></param>
            /// <param name="partSize"></param>
            /// <returns></returns>
            private string GetContentMD5(Stream input, long partSize)
            {
                using (var md5 = MD5.Create())
                {
                    int readSize = (int)partSize;
                    long pos = input.Position;
                    byte[] buffer = new byte[readSize];
                    readSize = input.Read(buffer, 0, readSize);

                    var data = md5.ComputeHash(buffer, 0, readSize);

                    input.Seek(pos, SeekOrigin.Begin);
                    return Convert.ToBase64String(data);
                }
            }
            /// <summary>
            /// 上传数据对象
            /// </summary>
            /// <param name="serverurl">oss访问的url</param>
            /// <param name="filename">上传文件名称</param>
            /// <param name="s">文件流</param>
            /// <param name="relativepath">相对路径，根路径使用~表示，如，~/myfolder/</param>
            /// <param name="new_name">新文件名称</param>
            /// <returns></returns>
            public dynamic PutObject(string serverurl,string filename, Stream s,string relativepath,string new_name)
            {
                var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:false,
msg:''
}");
                if (s == null) return rtn;

                var fileext = Path.GetExtension(filename).Replace(".", "");
                var name = Path.GetFileNameWithoutExtension(filename);
                var contenttype = ResponseHeader_ContentType.Map(fileext);
                var canonicalizedResource = relativepath.Replace("~", "").Replace("\\", "/") + new_name;
                var url = serverurl + canonicalizedResource;
                var contentmd5 = GetContentMD5(s, s.Length);
                var date = DateTime.Now;
                var sign = GetSign("PUT", contentmd5, contenttype, date, "", "/" + CurrentBucket + canonicalizedResource);

                FrameDLRObject header = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
                header.SetValue("Date", date.ToString("r"));
                header.SetValue("Content-MD5", contentmd5);
                header.SetValue("Content-length", s.Length);
                header.SetValue("Authorization", " OSS " + AccessKey + ":" + sign);
                //header.SetValue("User-Agent", "aliyun-sdk-dotnet/2.2.0.0(windows 6.2/6.2.9200.0/x86_64;2.0.50727.8689)");

                FrameDLRObject data = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
                byte[] barr = ComFunc.StreamToBytes(s);;
                data.SetValue(name, barr);

                dynamic result = _logic.OuterInterface.CallAliService(url, "PUT", contenttype, header, data);
                if (result.statuscode == 200)
                {
                    rtn.issuccess = true;
                    rtn.etag = result.header.ETag;
                }
                else
                {
                    rtn.issuccess = false;
                    rtn.msg = result.Message;
                    rtn.content = result;
                }

                return rtn;

            }
            /// <summary>
            /// 上传数据对象
            /// </summary>
            /// <param name="filename">上传文件名称</param>
            /// <param name="s">文件流</param>
            /// <param name="relativepath">相对路径，根路径使用~表示，如，~/myfolder/</param>
            /// <param name="new_name">新文件名称</param>
            /// <returns></returns>
            public dynamic PutObject(string filename, Stream s, string relativepath, string new_name)
            {
                return PutObject(CurrentURI, filename, s, relativepath, new_name);
            }
            /// <summary>
            /// 下载数据文件
            /// </summary>
            /// <param name="serverurl"></param>
            /// <param name="resourcepath">相对路径，根路径使用~表示，如，~/myfolder/text.xlsx</param>
            /// <returns></returns>
            public dynamic GetObject(string serverurl, string resourcepath)
            {
                var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:false,
msg:''
}");

                var canonicalizedResource = resourcepath.Replace("~", "").Replace("\\", "/");
                var url = serverurl + canonicalizedResource;
                var contentmd5 = "";
                var date = DateTime.Now;
                var sign = GetSign("GET", contentmd5, "", date, "", "/" + CurrentBucket + canonicalizedResource);

                FrameDLRObject header = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
                header.SetValue("Date", date.ToString("r"));
                header.SetValue("Authorization", " OSS " + AccessKey + ":" + sign);

                dynamic result = _logic.OuterInterface.CallAliService(url, "GET", "", header, null);
                if (result.statuscode == 200)
                {
                    rtn.issuccess = true;
                    if (result.content is byte[])
                    {
                        rtn.content = (byte[])result.content;
                    }
                    else if (result.content is Stream)
                    {
                        rtn.content = ComFunc.StreamToBytes((Stream)result.content);
                    }
                    else
                    {
                        rtn.content = result.content;
                    }
                }
                else
                {
                    rtn.issuccess = false;
                    rtn.msg = result.Message;
                    rtn.content = result;
                }

                return rtn;
            }
            /// <summary>
            /// 下载数据文件
            /// </summary>
            /// <param name="resourcepath">相对路径，根路径使用~表示，如，~/myfolder/text.xlsx</param>
            /// <returns></returns>
            public dynamic GetObject(string resourcepath)
            {
                return GetObject(CurrentURI, resourcepath);
            }
        }

    }
}
