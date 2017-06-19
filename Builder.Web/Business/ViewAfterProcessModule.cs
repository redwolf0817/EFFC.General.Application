
using Builder.Web.Global;
using Builder.Web.Helper;
using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Module;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Builder.Web.Business
{
    public class ViewAfterProcessModule : BaseModule<WebParameter, WMvcData>
    {
        public override string Description
        {
            get { return "View的收尾處理"; }
        }

        public override string Name
        {
            get { return "viewafterprocess"; }
        }

        protected override void OnError(Exception ex, WebParameter p, WMvcData d)
        {
            throw ex;
        }

        protected override void Run(WebParameter p, WMvcData d)
        {
            var loginexcept = GlobalPrepare.LoginExcept();
            if (!loginexcept.Contains((p.RequestResourceName + (p.Action == "" ? "" : "." + p.Action) + ".view").ToLower())
                && !loginexcept.Contains((p.RequestResourceName + ".*" + ".view").ToLower())
                && !loginexcept.Contains((p.RequestResourceName + (p.Action == "" ? "" : "." + p.Action) + ".*").ToLower())
                && !loginexcept.Contains((p.RequestResourceName + ".*" + ".*").ToLower())
                && !loginexcept.Contains((p.RequestResourceName + ".*").ToLower()))
            {
                var logic = p.RequestResourceName.ToLower();
                var action = p.Action.ToLower();
                var html = ComFunc.nvl(d["ViewHtmlCode"]);
                //input
                var inputs = HtmlParseHelper.MatchSingleHasAttr("input", "action", html);
                //img
                var imgs = HtmlParseHelper.MatchSingleHasAttr("img", "action", html);
                //link
                var links = HtmlParseHelper.MatchHasAttr("a", "action", html);

                var requestla = logic + (action == "" ? "" : "." + action) + ".view";
                var actions = p.LoginInfo.Actions.GetActionIDsByPageUrl(requestla);
                List<string> removed = new List<string>();
                JudgeProcess(inputs, "input", actions, ref removed);
                JudgeProcess(imgs, "img", actions, ref removed);
                JudgeProcess(links, "a", actions, ref removed);

                foreach (var s in removed)
                {
                    html = html.Replace(s, "");
                }
                var filds = p.LoginInfo.Actions.GetFiledsByPageUrl(requestla);
                //屏蔽未授权的字段
                //th
                var ths = HtmlParseHelper.MatchHasAttr("th", "filedid", html);
                JudgeProcess(ths, "th", filds, "filedid", ref removed);
                //th
                var tds1 = HtmlParseHelper.MatchHasAttr("td", "filedid", html);
                JudgeProcess(tds1, "td", filds, "filedid", ref removed);
                foreach (var s in removed)
                {
                    html = html.Replace(s, "");
                }
                d["ViewHtmlCode"] = html;
            }
        }

        private void JudgeProcess(List<string> l, string tag, string actions, ref List<string> removed)
        {
            foreach (var s in l)
            {
                var a = HtmlParseHelper.AttrValInControl(tag, "action", s);
                var aary = a.Split(',');
                var isauth = false;
                foreach (var aa in aary)
                {
                    if (actions.IndexOf(aa) >= 0)
                    {
                        isauth = true;
                    }
                }
                if (!isauth)
                {
                    removed.Add(s);
                }
            }
        }
        private void JudgeProcess(List<string> l, string tag, string actions,string replacetype, ref List<string> removed)
        {
            foreach (var s in l)
            {
                var a = HtmlParseHelper.AttrValInControl(tag, replacetype, s);
                var aary = a.Split(',');
                var isauth = false;
                foreach (var aa in aary)
                {
                    if (actions.IndexOf(aa) >= 0)
                    {
                        isauth = true;
                    }
                }
                if (!isauth)
                {
                    removed.Add(s);
                }
            }
        }


        public override string Version
        {
            get { return "0.0.1"; }
        }
    }
}
