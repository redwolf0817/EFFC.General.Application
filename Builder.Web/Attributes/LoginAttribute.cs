using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Builder.Web.Attributes
{
    public class LoginAttribute:Attribute
    {
        public LoginAttribute(bool isneedlogin)
        {
            IsNeedLogin = isneedlogin;
            LoginUrl = "";
        }

        public LoginAttribute(bool isneedlogin,string loginurl)
        {
            IsNeedLogin = isneedlogin;
            LoginUrl = loginurl;
        }

        /// <summary>
        /// 是否需要登录检核
        /// </summary>
        public bool IsNeedLogin
        {
            get;
            set;
        }

        public string LoginUrl
        {
            get;
            set;
        }
    }
}
