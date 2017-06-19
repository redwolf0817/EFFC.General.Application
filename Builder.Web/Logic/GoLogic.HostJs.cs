using Builder.Web.Helper;
using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Data.Base;
using EFFC.Frame.Net.Global;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Builder.Web.Logic
{
    public abstract partial class GoLogic
    {
        GoHostJsHelper _hjh = null;
        public new GoHostJsHelper Js
        {
            get
            {
                if (_hjh == null) _hjh = new GoHostJsHelper(this);
                return _hjh;
            }


        }
        public class GoHostJsHelper : HostJsHelper
        {
            string jsrootpath = "";
            public GoHostJsHelper(GoLogic logic)
                : base(logic)
            {
                jsrootpath = ComFunc.nvl(logic.Configs["HostJs_Path"]);
            }
            /// <summary>
            /// 呼叫js脚本
            /// </summary>
            /// <param name="scriptpath">脚本路径，根路径用~表示</param>
            /// <param name="includes">需要include进来的脚本，根路径用~表示</param>
            /// <param name="p"></param>
            /// <returns></returns>
            public FrameDLRObject CallByPath(string scriptpath, string[] includes, params KeyValuePair<string, object>[] p)
            {
                return CallLocal(scriptpath, includes, FrameDLRObject.CreateInstance(), p.ToList());
            }
            private FrameDLRObject CallLocal(string scriptpath, string[] includes, FrameDLRObject input, List<KeyValuePair<string, object>> p)
            {
                return CallLocal(scriptpath, includes, input, "output", p);
            }
            private FrameDLRObject CallLocal(string scriptpath, string[] includes, FrameDLRObject input,string outkey, List<KeyValuePair<string, object>> p)
            {
                if (!File.Exists(scriptpath.Replace("~", jsrootpath))) return FrameDLRObject.CreateInstance();

                var obj = CallScriptString(File.ReadAllText(scriptpath.Replace("~", jsrootpath), Encoding.Unicode), includes, input, outkey, p);
                return (FrameDLRObject)obj;
            }

            public object CallScriptString(string jsstr, string[] includes, FrameDLRObject input, string outputkey, List<KeyValuePair<string, object>> p)
            {

                var scriptstr = new StringBuilder();
                if (includes != null)
                {
                    foreach (var s in includes)
                    {
                        var path = s.Replace("~", jsrootpath);
                        if (File.Exists(path))
                        {
                            scriptstr.AppendLine(File.ReadAllText(path, Encoding.Unicode));
                            scriptstr.AppendLine("");
                        }
                    }
                }

                scriptstr.AppendLine(jsstr);
                p = p == null ? new List<KeyValuePair<string, object>>() : p;
                p.Add(new KeyValuePair<string, object>("server", new HostJsServer()));
                p.Add(new KeyValuePair<string, object>("log", GlobalCommon.Logger));

                return base.Call(scriptstr.ToString(), input, outputkey, p.ToArray());
            }

            public FrameDLRObject CallByPath(string scriptpath, FrameDLRObject input, List<KeyValuePair<string, object>> p, params string[] includes)
            {
                var lpincludes = includes.ToList();
                if (!lpincludes.Contains("~/common.min.js")
                    && !lpincludes.Contains("~/common.js"))
                {
                    lpincludes.Add("~/common.js");
                }
                return CallLocal(scriptpath, lpincludes.ToArray(), input, p);

            }

            public FrameDLRObject CallByPath(string scriptpath, FrameDLRObject input, params KeyValuePair<string, object>[] p)
            {
                var lp = p.ToList();
                lp.Add(new KeyValuePair<string, object>("input", input.ToDictionary()));
                return CallLocal(scriptpath, new string[] { "~/common.js" }, input, lp);

            }

            public FrameDLRObject CallByPath(string scriptpath, FrameDLRObject input, string outkey, params KeyValuePair<string, object>[] p)
            {
                var lp = p.ToList();
                lp.Add(new KeyValuePair<string, object>("input", input.ToDictionary()));
                return CallLocal(scriptpath, new string[] { "~/common.js" }, input, outkey, lp);

            }

            /// <summary>
            /// 呼叫规则js，生成满足规则的sql where条件
            /// </summary>
            /// <param name="scriptpath">脚本路径，~为根路径</param>
            /// <param name="rulename">规则名称</param>
            /// <param name="input">输入数据对象</param>
            /// <param name="resolver">sql规则解读器,用于解读一段规则表达式，将其转化成sql
            /// 基本的规则解读器可以参考"~/resolver.js"中的OracleResolve
            /// </param>
            /// <param name="keymap">关键字映射，当resolvemap的关键字与规则约定的不同时，则通过keymap来映射成正确的key，如：
            /// 规则需要的map为{
            /// period:6,
            /// nowtime:sysdate,
            /// }
            /// 但提供的的resolvemap结构为{
            /// c_period:6,
            /// c_nowtime:sysdate,
            /// },
            /// 此时需要keymap，经行矫正映射，如下
            /// {
            /// c_period:"period",
            /// c_nowtime:"nowtime",
            /// },
            /// 如果提供的resolvemap是满足要求的，则该参数可以为null</param>
            /// <returns></returns>
            public T MapRules<T>(string scriptpath, string rulename, FrameDLRObject input, Dictionary<string, object> resolver, FrameDLRObject keymap)
            {

                string spath = scriptpath.Replace("~", jsrootpath);
                string rulesframepath = jsrootpath + @"/rules_frame.js";

                string rulejsstr = File.ReadAllText(spath, Encoding.Unicode);
                string ruleframe = File.ReadAllText(rulesframepath, Encoding.Unicode);
                var scriptstr = new StringBuilder();

                scriptstr.AppendLine(ruleframe.Replace("/*#out#*/", rulejsstr));

                List<KeyValuePair<string, object>> lp = new List<KeyValuePair<string, object>>();
                lp.Add(new KeyValuePair<string, object>("resolveropts", resolver));
                lp.Add(new KeyValuePair<string, object>("input", input));
                lp.Add(new KeyValuePair<string, object>("keymap", keymap));
                lp.Add(new KeyValuePair<string, object>("rulename", rulename));


                var result = CallScriptString(scriptstr.ToString(), new string[] { "~/common.js" }, input, "outrule", lp);
                if (typeof(T).FullName == typeof(FrameDLRObject).FullName && result is Dictionary<string, object>)
                {
                    return FrameDLRObject.CreateInstance((Dictionary<string, object>)result);
                }
                else
                {
                    return (T)result;
                }
            }
            /// <summary>
            /// 呼叫规则js，生成满足规则的sql where条件
            /// </summary>
            /// <param name="scriptpath">脚本路径，~为根路径</param>
            /// <param name="rulename">规则名称</param>
            /// <param name="resolvemap">规则解读器的map</param>
            /// <param name="keymap">关键字映射，当resolvemap的关键字与规则约定的不同时，则通过keymap来映射成正确的key，如：
            /// 规则需要的map为{
            /// period:6,
            /// nowtime:sysdate,
            /// }
            /// 但提供的的resolvemap结构为{
            /// c_period:6,
            /// c_nowtime:sysdate,
            /// },
            /// 此时需要keymap，经行矫正映射，如下
            /// {
            /// c_period:"period",
            /// c_nowtime:"nowtime",
            /// },
            /// 如果提供的resolvemap是满足要求的，则该参数可以为null
            /// </param>
            /// <returns></returns>
            public T MapRules<T>(string scriptpath, string rulename, FrameDLRObject resolvemap, FrameDLRObject keymap)
            {

                string resolepath = jsrootpath + @"/resolver.js";
                string commonpath = jsrootpath + @"/common.js";

                var js = new StringBuilder();
                js.AppendLine(File.ReadAllText(commonpath, Encoding.Unicode));
                js.AppendLine(File.ReadAllText(resolepath, Encoding.Unicode));

                var resolver = CallScriptString(js.ToString(), new string[] { }, FrameDLRObject.CreateInstance(), "JsResolve", new List<KeyValuePair<string, object>>());


                FrameDLRObject dresolver = (FrameDLRObject)resolver;
                dresolver.SetValue("Map", resolvemap);
                var input = FrameDLRObject.CreateInstance();
                if (keymap == null) keymap = FrameDLRObject.CreateInstance();

                return MapRules<T>(scriptpath, rulename, input, dresolver.ToDictionary(), keymap);

            }
            /// <summary>
            /// 呼叫规则js，生成满足规则的sql where条件,规则解读器使用默认的
            /// </summary>
            /// <param name="scriptpath">>脚本路径，~为根路径</param>
            /// <param name="rulename">规则名称</param>
            /// <param name="resolvemap">规则解读器的map</param>
            /// <returns></returns>
            public T MapRules<T>(string scriptpath, string rulename, FrameDLRObject resolvemap)
            {
                return MapRules<T>(scriptpath, rulename, resolvemap, null);
            }

        }
    }
}

