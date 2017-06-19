using Builder.Web.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Business.Weixin
{
    public class WeixinHome:GoLogic
    {
        protected override Func<EFFC.Frame.Net.Data.LogicData.LogicData, object> GetFunction(string actionName)
        {
            switch (actionName.ToLower())
            {
                case "subscribe":
                    return DoSubscribe;
                case "scan":
                    return DoScan;
                case "click":
                    return DoClick;
                case "text":
                    return DoText;
                default:
                    return Load;

            }
        }

        private object DoSubscribe(EFFC.Frame.Net.Data.LogicData.LogicData arg)
        {
            var rtn = Weixin.GenResponseText("关注测试，谢谢！");

            return rtn;
        }

        private object DoScan(EFFC.Frame.Net.Data.LogicData.LogicData arg)
        {
            var rtn = Weixin.GenResponseText("关注测试，谢谢！");

            return rtn;
        }

        private object DoClick(EFFC.Frame.Net.Data.LogicData.LogicData arg)
        {
            var rtn = Weixin.GenResponseText("点击测试，谢谢！");

            return rtn;
        }

        private object DoText(EFFC.Frame.Net.Data.LogicData.LogicData arg)
        {
            var rtn = Weixin.GenResponseText("文本测试，谢谢！");

            return rtn;
        }

        private object Load(EFFC.Frame.Net.Data.LogicData.LogicData arg)
        {
            return arg["echostr"];
        }

        public override string Name
        {
            get { return "weixinhome"; }
        }
    }
}
