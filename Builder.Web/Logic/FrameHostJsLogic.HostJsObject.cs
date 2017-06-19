using Builder.Web.Proxy;
using EFFC.Frame.Net.Base.AttributeDefine;
using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data;
using EFFC.Frame.Net.Base.Data.Base;
using EFFC.Frame.Net.Base.Module;
using EFFC.Frame.Net.Base.ResouceManage.JsEngine;
using EFFC.Frame.Net.Business.Logic;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using EFFC.Frame.Net.Global;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Builder.Web.Logic
{
    #region outerinterface
    public class OuterInterfaceObject : BaseHostJsObject
    {
        WebBaseLogic<WebParameter, GoData> _logic = null;

        public OuterInterfaceObject(WebBaseLogic<WebParameter, GoData> logic)
        {
            _logic = logic;
        }
        [Desc("呼叫微信服务")]
        public Dictionary<string, object> CallWeixinServer(string url, Dictionary<string, object> postdata)
        {
            var copyp = _logic.CallContext_Parameter.Clone<WebParameter>();
            var copyd = _logic.CallContext_DataCollection.Clone<GoData>();
            copyd.ExtentionObj.OuterHttpUrl = url;
            if (postdata != null)
            {
                FrameDLRObject dobj = FrameDLRObject.CreateInstance(postdata, FrameDLRFlags.SensitiveCase);

                copyd.ExtentionObj.OuterHttpPostData = dobj;
            }
            ModuleProxyManager.Call<WeixinHttpProxy, WebParameter, GoData>(copyp, copyd);
            var result = copyd.ExtentionObj.OuterHttpResult;
            var rtn = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
            if (result is FrameDLRObject)
            {
                rtn.result = (FrameDLRObject)result;
            }
            else
            {
                rtn.result = ComFunc.nvl(result);
            }
            return ((FrameDLRObject)rtn).ToDictionary();
        }
        [Desc("上传二进制文件到微信服务器,name为post请求的名称，随意输入；filename为带扩展名的文件名称，filecontenttype为Http请求的contenttype中的application类型，file为文件内容")]
        public Dictionary<string, object> UploadWeixinServer(string url, string name, string filename, string filecontenttype, byte[] file)
        {
            var copyp = _logic.CallContext_Parameter.Clone<WebParameter>();
            var copyd = _logic.CallContext_DataCollection.Clone<GoData>();
            copyd.ExtentionObj.OuterHttpUrl = url;

            copyd.ExtentionObj.filename = filename;
            copyd.ExtentionObj.name = name;
            copyd.ExtentionObj.filecontenttype = filecontenttype;
            copyd.ExtentionObj.filecontent = file;

            ModuleProxyManager.Call<WeixinHttpProxy, WebParameter, GoData>(copyp, copyd);
            var result = copyd.ExtentionObj.OuterHttpResult;
            var rtn = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
            if (result is FrameDLRObject)
            {
                rtn.result = (FrameDLRObject)result;
            }
            else
            {
                rtn.result = ComFunc.nvl(result);
            }
            return ((FrameDLRObject)rtn).ToDictionary();
        }
        public override string Description
        {
            get { return "外部接口呼叫"; }
        }

        public override string Name
        {
            get { return "OuterAPI"; }
        }
    }
    #endregion
    #region Weixin
    public class WeixinObject : BaseHostJsObject
    {
        WebBaseLogic<WebParameter, GoData> _logic = null;

        public WeixinObject(WebBaseLogic<WebParameter, GoData> logic)
        {
            _logic = logic;
        }
        public override string Description
        {
            get { return "微信相关，EFFC框架针对微信服务器请求过来的Msg或Event转化成对应action(相关的MsgType或Event可以参阅微信的开发文档-接收消息部分)，action为空的时候则表示是微信的验证请求，需要做验证处理"; }
        }

        public override string Name
        {
            get { return "WeiXin"; }
        }
        [Desc("微信的APPID")]
        public string AppID
        {
            get
            {
                return ComFunc.nvl(_logic.CallContext_Parameter.ExtentionObj.weixin.appid);
            }
        }
        [Desc("微信的APPSecret")]
        public string AppSecret
        {
            get
            {
                return ComFunc.nvl(_logic.CallContext_Parameter.ExtentionObj.weixin.appsecret);
            }
        }
        [Desc("请求的FromUserName")]
        public string FromUserName
        {
            get
            {
                return ComFunc.nvl(_logic.CallContext_Parameter[DomainKey.POST_DATA, "FromUserName"]);
            }
        }
        [Desc("请求的ToUserName")]
        public string ToUserName
        {
            get
            {
                return ComFunc.nvl(_logic.CallContext_Parameter[DomainKey.POST_DATA, "ToUserName"]);
            }
        }
        [Desc("请求的CreateTime")]
        public DateTime CreateTime
        {
            get
            {
                return (DateTime)_logic.CallContext_Parameter[DomainKey.POST_DATA, "CreateTime"];
            }
        }
        [Desc("请求的MsgType")]
        public string MsgType
        {
            get
            {
                return ComFunc.nvl(_logic.CallContext_Parameter[DomainKey.POST_DATA, "MsgType"]);
            }
        }
        [Desc("请求的Content")]
        public string Content
        {
            get
            {
                return ComFunc.nvl(_logic.CallContext_Parameter[DomainKey.POST_DATA, "Content"]);
            }
        }
        [Desc("请求的MsgId")]
        public string MsgId
        {
            get
            {
                return ComFunc.nvl(_logic.CallContext_Parameter[DomainKey.POST_DATA, "MsgId"]);
            }
        }
        [Desc("请求的PicUrl")]
        public string PicUrl
        {
            get
            {
                return ComFunc.nvl(_logic.CallContext_Parameter[DomainKey.POST_DATA, "PicUrl"]);
            }
        }
        [Desc("请求的MediaId")]
        public string MediaId
        {
            get
            {
                return ComFunc.nvl(_logic.CallContext_Parameter[DomainKey.POST_DATA, "MediaId"]);
            }
        }
        [Desc("请求的语音Format，语音格式，如amr，speex等")]
        public string Format
        {
            get
            {
                return ComFunc.nvl(_logic.CallContext_Parameter[DomainKey.POST_DATA, "Format"]);
            }
        }
        [Desc("请求的语音识别结果，使用UTF8编码")]
        public string Recognition
        {
            get
            {
                return ComFunc.nvl(_logic.CallContext_Parameter[DomainKey.POST_DATA, "Recognition"]);
            }
        }
        [Desc("视频消息缩略图的媒体id，可以调用多媒体文件下载接口拉取数据。")]
        public string ThumbMediaId
        {
            get
            {
                return ComFunc.nvl(_logic.CallContext_Parameter[DomainKey.POST_DATA, "ThumbMediaId"]);
            }
        }
        [Desc("地理位置维度")]
        public double Location_X
        {
            get
            {
                return DoubleStd.ParseStd(_logic.CallContext_Parameter[DomainKey.POST_DATA, "Location_X"]).Value;
            }
        }
        [Desc("地理位置经度")]
        public double Location_Y
        {
            get
            {
                return DoubleStd.ParseStd(_logic.CallContext_Parameter[DomainKey.POST_DATA, "Location_Y"]).Value;
            }
        }
        [Desc("地图缩放大小")]
        public double Scale
        {
            get
            {
                return DoubleStd.ParseStd(_logic.CallContext_Parameter[DomainKey.POST_DATA, "Scale"]).Value;
            }
        }
        [Desc("地理位置信息")]
        public string Label
        {
            get
            {
                return ComFunc.nvl(_logic.CallContext_Parameter[DomainKey.POST_DATA, "Label"]);
            }
        }
        [Desc("消息标题")]
        public string MessageTitle
        {
            get
            {
                return ComFunc.nvl(_logic.CallContext_Parameter[DomainKey.POST_DATA, "Title"]);
            }
        }
        [Desc("消息描述")]
        public string MessageDescription
        {
            get
            {
                return ComFunc.nvl(_logic.CallContext_Parameter[DomainKey.POST_DATA, "Description"]);
            }
        }
        [Desc("消息链接")]
        public string MessageUrl
        {
            get
            {
                return ComFunc.nvl(_logic.CallContext_Parameter[DomainKey.POST_DATA, "Url"]);
            }
        }
        [Desc(@"事件类型，subscribe(订阅)、unsubscribe(取消订阅)、SCAN（取消关注）、LOCATION（上报地理位置）、CLICK（点击菜单）")]
        public string Event
        {
            get
            {
                return ComFunc.nvl(_logic.CallContext_Parameter[DomainKey.POST_DATA, "Event"]);
            }
        }
        [Desc(@"事件KEY值，qrscene_为前缀，后面为二维码的参数值")]
        public string EventKey
        {
            get
            {
                return ComFunc.nvl(_logic.CallContext_Parameter[DomainKey.POST_DATA, "EventKey"]);
            }
        }
        [Desc(@"二维码的ticket，可用来换取二维码图片")]
        public string Ticket
        {
            get
            {
                return ComFunc.nvl(_logic.CallContext_Parameter[DomainKey.POST_DATA, "Ticket"]);
            }
        }
        [Desc(@"上报事件中的地理位置纬度")]
        public double Latitude
        {
            get
            {
                return DoubleStd.ParseStd(_logic.CallContext_Parameter[DomainKey.POST_DATA, "Latitude"]).Value;
            }
        }
        [Desc(@"上报事件中的地理位置经度")]
        public double Longitude
        {
            get
            {
                return DoubleStd.ParseStd(_logic.CallContext_Parameter[DomainKey.POST_DATA, "Longitude"]).Value;
            }
        }
        [Desc(@"上报事件中的地理位置精度")]
        public double Precision
        {
            get
            {
                return DoubleStd.ParseStd(_logic.CallContext_Parameter[DomainKey.POST_DATA, "Precision"]).Value;
            }
        }
        [Desc(@"获取微信的Access Token，用于与微信服务器进行信息交互")]
        public string Access_Token
        {
            get
            {
                if (_logic.CacheHelper.GetCache("weixin_access_token") == null)
                {
                    var copyp = _logic.CallContext_Parameter.Clone<WebParameter>();
                    var copyd = _logic.CallContext_DataCollection.Clone<GoData>();
                    copyd.ExtentionObj.OuterHttpUrl = string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}", AppID, AppSecret);
                    ModuleProxyManager.Call<WeixinHttpProxy, WebParameter, GoData>(copyp, copyd);
                    var result = copyd.ExtentionObj.OuterHttpResult;
                    if (result is FrameDLRObject)
                    {
                        dynamic dobj = (FrameDLRObject)result;
                        var token = ComFunc.nvl(dobj.access_token);
                        var expireseconds = ComFunc.nvl(dobj.expires_in);
                        if (token != "")
                        {
                            //获取之后将超时时间缩短10秒，微信默认超时时间为7200秒，每获取一次就会重置该token
                            _logic.CacheHelper.SetCache("weixin_access_token", token, DateTime.Now.AddSeconds(IntStd.ParseStd(expireseconds).Value - 10));
                        }
                    }
                }

                return ComFunc.nvl(_logic.CacheHelper.GetCache("weixin_access_token"));
            }
        }

        private dynamic GenResponseObject()
        {
            var rtn = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
            rtn.ToUserName = FromUserName;
            rtn.FromUserName = ToUserName;
            rtn.CreateTime = DateTime.Now;
            return rtn;
        }
        [Desc("生成一个Text类型的回复数据")]
        public Dictionary<string, object> GenResponseText(string content)
        {
            var rtn = GenResponseObject();
            rtn.MsgType = "text";
            rtn.Content = content;
            return ((FrameDLRObject)rtn).ToDictionary();
        }
        [Desc(@"生成一个image类型的回复数据,
mediaid:通过素材管理接口上传多媒体文件，得到的id。")]
        public Dictionary<string, object> GenResponseImage(string mediaid)
        {
            var rtn = GenResponseObject();
            rtn.MsgType = "image";
            rtn.Image = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
            rtn.Image.MediaId = mediaid;
            return ((FrameDLRObject)rtn).ToDictionary();
        }
        [Desc(@"生成一个image类型的回复数据,
mediaid:通过素材管理接口上传多媒体文件，得到的id。")]
        public Dictionary<string, object> GenResponseVoice(string mediaid)
        {
            var rtn = GenResponseObject();
            rtn.MsgType = "voice";
            rtn.Voice = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
            rtn.Voice.MediaId = mediaid;
            return ((FrameDLRObject)rtn).ToDictionary();
        }
        [Desc(@"生成一个video类型的回复数据,
mediaid:通过素材管理接口上传多媒体文件，得到的id。
title:视频消息的标题
description:视频消息的描述")]
        public Dictionary<string, object> GenResponseVideo(string mediaid, string title, string description)
        {
            var rtn = GenResponseObject();
            rtn.MsgType = "video";
            rtn.Video = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
            rtn.Video.MediaId = mediaid;
            rtn.Video.Title = title;
            rtn.Video.Description = description;
            return ((FrameDLRObject)rtn).ToDictionary();
        }
        [Desc(@"生成一个音乐消息类型的回复数据,
mediaid:通过素材管理接口上传多媒体文件，得到的id。
title:音乐标题
description:音乐描述
MusicUrl:音乐链接
HQMusicUrl:高质量音乐链接，WIFI环境优先使用该链接播放音乐")]
        public Dictionary<string, object> GenResponseMusic(string mediaid, string title, string description, string MusicUrl, string HQMusicUrl)
        {
            var rtn = GenResponseObject();
            rtn.MsgType = "music";
            rtn.Music = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
            rtn.Music.Title = title;
            rtn.Music.Description = description;
            rtn.Music.MusicUrl = MusicUrl;
            rtn.Music.HQMusicUrl = HQMusicUrl;
            rtn.Music.ThumbMediaId = mediaid;
            return ((FrameDLRObject)rtn).ToDictionary();
        }
        [Desc(@"生成一个图文消息类型的回复数据,
items:数组，动态对象为敏感大小写
     构成如下：
     Title：图文消息标题
     Description：图文消息描述
     PicUrl：图片链接，支持JPG、PNG格式，较好的效果为大图360*200，小图200*200
     Url：点击图文消息跳转链接")]
        public FrameDLRObject GenResponseNews(object[] items)
        {
            var rtn = GenResponseObject();
            rtn.MsgType = "news";
            rtn.ArticleCount = items.Length;
            rtn.Articles = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
            rtn.Articles.item = items;
            return rtn;
        }

    }
    #endregion
    #region Razor
    public class RazorObject : BaseHostJsObject
    {
        WebBaseLogic<WebParameter, GoData> _logic = null;

        public RazorObject(WebBaseLogic<WebParameter, GoData> logic)
        {
            _logic = logic;
        }

        [Desc("填入ViewPath，格式为“~/Views/xxxx/xxxx.cshtml”")]
        public void SetViewPath(string viewpath)
        {
            _logic.CallContext_DataCollection.ViewPath = viewpath;
        }
        [Desc("向ViewData中新增或更新一个参数，用于View页面的ViewData使用")]
        public void SetViewData(string key, object value)
        {
            _logic.CallContext_DataCollection[DomainKey.VIEW_LIST, key] = value;
        }
        [Desc("填入起始view文件的名称，默认为_ViewStart")]
        public void SetStartView(string startviewname)
        {
            _logic.CallContext_DataCollection.StartViewName = startviewname;
        }
        [Desc("写入一个moduledata，供View使用")]
        public void SetMvcModuleData(object obj)
        {
            if (obj != null)
            {
                if (obj is Dictionary<string, object>)
                {
                    _logic.CallContext_DataCollection.MvcModuleData = FrameDLRObject.CreateInstance((Dictionary<string, object>)obj, FrameDLRFlags.SensitiveCase);
                }
                else
                {
                    _logic.CallContext_DataCollection.MvcModuleData = obj;
                }
            }
        }

        public override string Description
        {
            get { return "可以调用Razor进行页面渲染"; }
        }

        public override string Name
        {
            get { return "Razor"; }
        }
    }
    #endregion
    #region Debug
    public class DebugObject : BaseHostJsObject
    {
        WebBaseLogic<WebParameter, GoData> _logic = null;
        bool isDebug = false;

        public DebugObject(WebBaseLogic<WebParameter, GoData> logic)
        {
            _logic = logic;
            isDebug = bool.Parse(ComFunc.nvl(_logic.Configs["DebugMode"]));
        }

        public override string Description
        {
            get { return "Debug运行的对象，只有在DebugMode=true下才能执行"; }
        }
        [Desc("设置断点，该断点会通过WebSocket来阻断当前线程的运行,该方法只有在DebugMode=true和请求中Debug=1&DebugCode不为空时才有效")]
        public void bp()
        {
            var fl = (FrameHostJsLogic)_logic;
            //如果为debug才执行
            if (fl.IsDebug && fl.IsDebugMode && fl.DebugCode != "")
            {
                
                var monitor = HostDebugBaseLogic.GetMonitor(fl.DebugCode);


                if (monitor != null)
                {
                    monitor.ProcessDebugCommand(FrameDLRObject.CreateInstanceFromat(@"{
issuccess:true, 
command:'pendflag'
}"));
                    fl.Suspend();
                }

                //object obj = null;
                //                    obj = _logic.WS.Recieve();
                //                    if (obj is FrameDLRObject)
                //                    {
                //                        dynamic dobj = obj;
                //                        if (dobj.command == "resume")
                //                        {
                //                            _logic.WS.Send(FrameDLRObject.CreateInstanceFromat(@"{
                //issuccess:true,
                //msg:'当前运行恢复',    
                //command:'resume',
                //express:'',
                //result:null 
                //}"));
                //                            break;
                //                        }
                //                        else
                //                        {
                //                            if (_logic is FrameHostJsLogic)
                //                            {
                //                                ((FrameHostJsLogic)_logic).ProcessCommand(obj);
                //                            }
                //                        }
                //                    }
                //                    else if (ComFunc.nvl(obj) == "resume")
                //                    {
                //                        _logic.WS.Send(FrameDLRObject.CreateInstanceFromat(@"{
                //issuccess:true,
                //msg:'当前运行恢复',    
                //command:'resume',
                //express:'',
                //result:null 
                //}"));
                //                        break;
                //                    }

            }
        }

        public override string Name
        {
            get { return "debug"; }
        }
    }

    #endregion
}
