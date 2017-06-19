using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data.Base;
using EFFC.Frame.Net.Base.Module;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Builder.Web.Business
{
    public class PreWeixinProcessModule : BaseModule<WebParameter, GoData>
    {
        public override string Description
        {
            get { return "微信預處理模塊"; }
        }

        public override string Name
        {
            get { return "PreProcess"; }
        }

        protected override void OnError(Exception ex, WebParameter p, GoData d)
        {
            throw ex;
        }

        protected override void Run(WebParameter p, GoData d)
        {
            WebParameter wp = (WebParameter)p;
            GoData wd = (GoData)d;
            p.CanContinue = true;
            string token = p.ExtentionObj.weixin.token;
            string signature = p.ExtentionObj.weixin.signature;
            string timestamp = p.ExtentionObj.weixin.timestamp;
            string nonce = p.ExtentionObj.weixin.nonce;
            if (string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(timestamp) || string.IsNullOrWhiteSpace(nonce))
            {
                p.CanContinue = false;
                var dobj = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
                dobj.ToUserName = p[DomainKey.POST_DATA, "FromUserName"];
                dobj.FromUserName = p[DomainKey.POST_DATA, "ToUserName"];
                dobj.CreateTime = DateTime.Now;
                dobj.MsgType = "text";
                dobj.Content = "检验无效，微信请求参数不正确";
                dobj.FuncFlag = 0;
                d.ResponseData = dobj;
            }
            else
            {
                string[] infos = new string[] { token, timestamp, nonce };
                Array.Sort<string>(infos);
                string info = string.Format("{0}{1}{2}", infos[0], infos[1], infos[2]);
                p.CanContinue = string.Compare(signature, GetSha1Hash(info, Encoding.ASCII), true) == 0;
                if (!p.CanContinue)
                {
                    var dobj = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
                    dobj.ToUserName = p[DomainKey.POST_DATA, "FromUserName"];
                    dobj.FromUserName = p[DomainKey.POST_DATA, "ToUserName"];
                    dobj.CreateTime = DateTime.Now;
                    dobj.MsgType = "text";
                    dobj.Content = "检验无效，不是微信访问接口";
                    dobj.FuncFlag = 0;
                    d.ResponseData = dobj;
                }

            }
        }

        /// <returns>返回SHA1哈希字符串</returns>
        public static string GetSha1Hash(string input, Encoding encoding)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(input) && encoding != null)
            {
                byte[] bytes = encoding.GetBytes(input);
                SHA1 sha1 = new SHA1CryptoServiceProvider();
                byte[] result = sha1.ComputeHash(bytes);
                foreach (byte b in result)
                    sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }

        public override string Version
        {
            get { return "0.0.1"; }
        }
    }
}
