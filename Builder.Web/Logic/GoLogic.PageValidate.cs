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
using System.Text.RegularExpressions;

namespace Builder.Web.Logic
{
    public abstract partial class GoLogic
    {
        PageValidate _v = null;
        public PageValidate validate
        {
            get
            {
                if (_t == null) _v = new PageValidate(this);
                return _v;
            }
        }
        public class PageValidate
        {
            GoLogic _logic;

            public PageValidate(GoLogic logic)
            {
                _logic = logic;

            }

            /// <summary>
            /// 匹配非负整数   >=0   
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static bool IsNotNagtive(string input)
            {
                Regex regex = new Regex(@"^\d+$");
                return regex.IsMatch(input);
            }

            /// <summary>
            /// 匹配正整数 >0
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static bool IsUint(string input)
            {
                Regex regex = new Regex("^[0-9]*[1-9][0-9]*$");
                return regex.IsMatch(input);
            }

            /// <summary>
            /// 判断输入的字符串是否是一个合法的Email地址
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static bool IsEmail(string input)
            {
                string pattern = @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
                Regex regex = new Regex(pattern);
                return regex.IsMatch(input);
            }

            /// <summary>
            /// 验证手机号和固定电话号码
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static bool IsMobilePhoneOrPhone(string input)
            {
                //(\d{3,4}\)|\d{3,4}-|\s)?\d{7,14}
                string pattern = @"(\d{3,4}\)|\d{3,4}-|\s)?\d{7,14}";
                return new Regex(pattern).IsMatch(input);
            }

            /// <summary>
            /// 判断输入的字符串是否是一个合法的手机号
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static bool IsMobilePhone(string input)
            {
                //Regex regex = new Regex("^13\\d{9}$");
                Regex regex = new Regex("^1[358]\\d{9}$");
                return regex.IsMatch(input);
            }

            /// <summary>
            /// 匹配3位或4位区号的电话号码，其中区号可以用小括号括起来，
            /// 也可以不用，区号与本地号间可以用连字号或空格间隔，
            /// 也可以没有间隔
            /// \(0\d{2}\)[- ]?\d{8}|0\d{2}[- ]?\d{8}|\(0\d{3}\)[- ]?\d{7}|0\d{3}[- ]?\d{7}
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static bool IsPhone(string input)
            {
                string pattern = "^\\(0\\d{2}\\)[- ]?\\d{8}$|^0\\d{2}[- ]?\\d{8}$|^\\(0\\d{3}\\)[- ]?\\d{7}$|^0\\d{3}[- ]?\\d{7}$";
                Regex regex = new Regex(pattern);
                return regex.IsMatch(input);
            }

            /// <summary>
            /// 判断一个字符串是否为ID格式
            /// </summary>
            /// <param name="source"></param>
            /// <returns></returns>
            public static bool IsIDCard(string _value)
            {
                Regex regex;
                string[] strArray;
                DateTime time;
                if ((_value.Length != 15) && (_value.Length != 0x12))
                {
                    return false;
                }
                if (_value.Length == 15)
                {
                    regex = new Regex(@"^(\d{6})(\d{2})(\d{2})(\d{2})(\d{3})$");
                    if (!regex.Match(_value).Success)
                    {
                        return false;
                    }
                    strArray = regex.Split(_value);
                    try
                    {
                        time = new DateTime(int.Parse("19" + strArray[2]), int.Parse(strArray[3]), int.Parse(strArray[4]));
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
                regex = new Regex(@"^(\d{6})(\d{4})(\d{2})(\d{2})(\d{3})([0-9Xx])$");
                if (!regex.Match(_value).Success)
                {
                    return false;
                }
                strArray = regex.Split(_value);
                try
                {
                    time = new DateTime(int.Parse(strArray[2]), int.Parse(strArray[3]), int.Parse(strArray[4]));
                    return true;
                }
                catch
                {
                    return false;
                }
            }

        }

    }
}

