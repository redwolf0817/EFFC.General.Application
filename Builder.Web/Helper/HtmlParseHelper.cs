using EFFC.Frame.Net.Base.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Builder.Web.Helper
{
    public class HtmlParseHelper
    {
        /// <summary>
        /// 匹配含有属性attrname=attrval的嵌套标签
        /// </summary>
        /// <param name="tag">标签名称</param>
        /// <param name="attrname">属性名称</param>
        /// <param name="attrval">属性值</param>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<string> Match(string tag, string attrname, string attrval, string html)
        {
            string regexpress = @"(?isx)
                                <({0})\b(?:(?!(?:{1}=|</?\1\b)).)*{1}=(['""]?)" + attrval + @"\2[^>]*>  #开始标记“<tag...>”
                                (?>                                                                  #分组构造，用来限定量词“*”修饰范围
                                <\1[^>]*>  (?<Open>)                                                 #命名捕获组，遇到开始标记，入栈，Open计数加1
                                |</\1>  (?<-Open>)                                                   #狭义平衡组，遇到结束标记，出栈，Open计数减1
                                |(?:(?!</?\1\b).)*                                                   #右侧不为开始或结束标记的任意字符
                                )
                                (?(Open)(?!))                                                        #判断是否还有'OPEN'，有则说明不配对，什么都不匹配
                                </\1>                                                                #结束标记“</tag>”
                     ";
            Regex reg = new Regex(string.Format(regexpress, Regex.Escape(tag)));
            MatchCollection mc = reg.Matches(html);
            List<string> rtn = new List<string>();
            foreach (Match m in mc)
            {
                rtn.Add(m.Value);
            }

            return rtn;
        }

        public static List<string> MatchHasAttr(string tag, string attrname, string html)
        {
            string regexpress = @"(?isx)
                                <({0})\b(?:(?!(?:{1}=|</?\1\b)).)*{1}=(['""]?)[^>'""]*\2[^>]*>  #开始标记“<tag...>”
                                (?>                                                                  #分组构造，用来限定量词“*”修饰范围
                                <\1[^>]*>  (?<Open>)                                                 #命名捕获组，遇到开始标记，入栈，Open计数加1
                                |</\1>  (?<-Open>)                                                   #狭义平衡组，遇到结束标记，出栈，Open计数减1
                                |(?:(?!</?\1\b).)*                                                   #右侧不为开始或结束标记的任意字符
                                )
                                (?(Open)(?!))                                                        #判断是否还有'OPEN'，有则说明不配对，什么都不匹配
                                </\1>                                                                #结束标记“</tag>”
                     ";
            Regex reg = new Regex(string.Format(regexpress, Regex.Escape(tag), Regex.Escape(attrname)), RegexOptions.IgnorePatternWhitespace);
            MatchCollection mc = reg.Matches(html);
            List<string> rtn = new List<string>();
            foreach (Match m in mc)
            {
                rtn.Add(m.Value);
            }

            return rtn;
        }

        public static List<string> MatchSingle(string tag, string attrname, string attrval, string html)
        {
            string regexpress = @"(?isx)

                      <{0}[^>]+?" + attrname + @"=(['""]?)" + attrval + @"\1[^>]*        #开始标记“<xxx...”

                      />                          #结束标记“/>”
                     ";
            Regex reg = new Regex(string.Format(regexpress, tag));
            MatchCollection mc = reg.Matches(html);
            List<string> rtn = new List<string>();
            foreach (Match m in mc)
            {
                rtn.Add(m.Value);
            }

            return rtn;
        }

        public static List<string> MatchSingleHasAttr(string tag, string attrname, string html)
        {
            string regexpress = @"(?isx)

                      <{0}[^>]+?\s+" + attrname + @"=(['""])[^>'""]*\1[^>]*        #开始标记“<xxx...”

                      />                          #结束标记“/>”
                     ";
            Regex reg = new Regex(string.Format(regexpress, tag));
            MatchCollection mc = reg.Matches(html);
            List<string> rtn = new List<string>();
            foreach (Match m in mc)
            {
                rtn.Add(m.Value);
            }

            return rtn;
        }

        public static string AttrVal(string tag, string id, string attrname, string html)
        {
            string regexpress = @"(?isx)
                                    (?<=<{0}[^>]*{2}=(['""]))
                                    [^>'""]*
                                    (?=\1[^>]*id=['""]{1}['""])
                                    |
                                    (?isx)
                                    (?<=<{0}[^>]*id=['""]{1}['""][^>]*{2}=(['""]))
                                    [^>'""]*
                                    (?=\2)";

            Regex reg = new Regex(string.Format(regexpress, Regex.Escape(tag), Regex.Escape(id), Regex.Escape(attrname)));
            Match mc = reg.Match(html);

            return mc.Value;
        }

        public static string AttrValInControl(string tag, string attrname, string html)
        {
            string regexpress = @"(?isx)
                                    (?<=<{0}[^>]*{1}=(['""]))
                                    [^>'""]*
                                    (?=\1)";

            Regex reg = new Regex(string.Format(regexpress, Regex.Escape(tag), Regex.Escape(attrname)));
            Match mc = reg.Match(html);

            return ComFunc.nvl(mc.Value);
        }
    }
}
