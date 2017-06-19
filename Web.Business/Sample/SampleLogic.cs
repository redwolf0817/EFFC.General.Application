using Builder.Web.Logic;
using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Data.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFFC.Frame.Net.Data.LogicData;
using Unit;

namespace Business
{
    public class SampleLogic:GoLogic
    {
        protected override Func<LogicData, object> GetFunction(string actionName)
        {
            switch (actionName.ToLower())
            {
                case "razor":
                    return RazorSample;
                default:
                    return Load;
            }
        }

        private object RazorSample(LogicData arg)
        {
            SetContentType(EFFC.Frame.Net.Base.Constants.GoResponseDataType.RazorView);
            Razor.SetViewData("name", "ych");
            Razor.SetViewPath("~/Views/sample/sample.cshtml");

            return null;
        }

        private object Load(EFFC.Frame.Net.Data.LogicData.LogicData obj)
        {
            var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:true,
msg:'成功',
}");
            SetContentType(EFFC.Frame.Net.Base.Constants.GoResponseDataType.RazorView);
            Razor.SetViewData("key", "");
            //var up = DB.NewDBUnitParameter();
            //up.SetValue("dzghrq0", "20141127");

            //var udc = DB.Excute(up, @"{
            //             $acttype : 'Query',
            //         	$table : 'sjhm_repast_corder',
            //             $where:{
            //                 id:1
            //             }
            //     }");
            //udc = DB.Query<SampleUnit>(up, "query");
            ////访问数据的方式
            //var ds = udc.QueryDatas;
            //var dt = udc.QueryTable;
            //var list = udc.QueryData<FrameDLRObject>();
            //dynamic item = list[0];

            // var id = ds[0][0, "id"];
            // id = dt[0, "id"];
            // id = item.id;

            //Files.Write2TxtWith(list, "d:/mytext.txt");
            //Files.Write2TxtByLine(list, ",", "d:/mytext.txt", true, Encoding.UTF8);
            //Files.UploadFile("~/", "mytext.txt", File.Open("d:/mytext.txt",FileMode.Open));
            //OuterInterface.SendEmail("2069067@qq.com", "发送邮件测试", "测试成功", "d:/dt_article.txt", "d:/dt_article.xls");
            //Files.Zip("d:/test.zip", "d:/dt_article.txt", "d:/dt_article.xls");

            return rtn;
        }

        

        public override string Name
        {
            get { return "sample"; }
        }
    }
}
