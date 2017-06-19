using Builder.Web.Proxy;
using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data.Base;
using EFFC.Frame.Net.Base.Module;
using EFFC.Frame.Net.Base.ResouceManage;
using EFFC.Frame.Net.Base.Token;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using EFFC.Frame.Net.Global;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Builder.Web.Logic
{
    public abstract partial class GoLogic
    {
        OuterInterfaceHelper _oifh = null;
        public new OuterInterfaceHelper OuterInterface
        {
            get
            {
                if (_oifh == null) _oifh = new OuterInterfaceHelper(this);
                return _oifh;
            }


        }
        public class OuterInterfaceHelper
        {
            GoLogic _logic = null;

            public OuterInterfaceHelper(GoLogic logic)
            {
                _logic = logic;
            }
            /// <summary>
            /// 呼叫本地logic
            /// </summary>
            /// <param name="logic"></param>
            /// <param name="action"></param>
            /// <param name="p"></param>
            /// <returns></returns>
            public object CallLocalLogic(string logic, string action, params KeyValuePair<string, object>[] p)
            {
                FrameDLRObject dp = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
                foreach (var item in p)
                {
                    dp.SetValue(item.Key, item.Value);
                }
                return CallLocalLogic(logic, action, dp);
            }

            /// <summary>
            /// 呼叫本地logic
            /// </summary>
            /// <param name="logic"></param>
            /// <param name="action"></param>
            /// <param name="p"></param>
            /// <returns></returns>
            public object CallLocalLogic(string logic, string action, FrameDLRObject p)
            {
                var copyp = _logic.CallContext_Parameter.Clone<WebParameter>();
                var copyd = _logic.CallContext_DataCollection.Clone<GoData>();
                copyp.RequestResourceName = logic;
                copyp.Action = action;
                ResourceManage rema = new ResourceManage();
                copyp.SetValue<ResourceManage>(ParameterKey.RESOURCE_MANAGER, rema);
                var defaulttoken = TransactionToken.NewToken();
                copyp.TransTokenList.Add(defaulttoken);
                copyp.SetValue<TransactionToken>(ParameterKey.TOKEN, defaulttoken);
                copyp.SetValue("IsAjaxAsync", false);
                if (p != null)
                {
                    foreach (var item in p.Keys)
                    {
                        copyp.SetValue(DomainKey.POST_DATA, item, p.GetValue(item));
                    }
                }
                return CallLocalLogic(logic, action, copyp, copyd);
            }
            /// <summary>
            /// 呼叫本地logic
            /// </summary>
            /// <param name="logic"></param>
            /// <param name="action"></param>
            /// <param name="p"></param>
            /// <param name="d"></param>
            /// <returns></returns>
            private object CallLocalLogic(string logic, string action,WebParameter p,GoData d)
            {
                ModuleProxyManager<WebParameter, GoData>.Call<GoBusinessProxy>(p, d);
                return d.ResponseData;
            }

            

            /// <summary>
            /// 呼叫交易服务
            /// </summary>
            /// <param name="tradename"></param>
            /// <param name="instanceid"></param>
            /// <param name="action"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            public object CallTradeService(string tradename, string instanceid, string action, params KeyValuePair<string, object>[] data)
            {
                var copyp = _logic.CallContext_Parameter.Clone<WebParameter>();
                var copyd = _logic.CallContext_DataCollection.Clone<GoData>();
                copyd.ExtentionObj.OuterHttpUrl = ComFunc.nvl(copyp[DomainKey.CONFIG, "trade_server_url"]) + tradename + "." + instanceid + (action == "" ? "" : "." + action) + ".tra";
                if (data != null)
                {
                    copyd.ExtentionObj.OuterHttpPostData = data;
                }
                
                ModuleProxyManager.Call<TradeProxy, WebParameter, GoData>(copyp, copyd);
                if (copyd.ExtentionObj.OuterHttpResult is FrameDLRObject)
                {
                    if (copyd.ExtentionObj.OuterHttpResult.ErrorCode != "")
                    {
                        throw new Exception("Call Trade Service Error:" + copyd.ExtentionObj.OuterHttpResult.ErrorMessage);
                    }
                }
                return copyd.ExtentionObj.OuterHttpResult.Content;
            }
            /// <summary>
            /// 异步调用交易服务
            /// </summary>
            /// <param name="tradename"></param>
            /// <param name="instanceid"></param>
            /// <param name="action"></param>
            /// <param name="callback"></param>
            /// <param name="data"></param>
            public void CallTradeServiceAsync(string tradename, string instanceid, string action, Action<FrameDLRObject> callback, params KeyValuePair<string, object>[] data)
            {
                var copyp = _logic.CallContext_Parameter.Clone<WebParameter>();
                var copyd = _logic.CallContext_DataCollection.Clone<GoData>();
                copyd.ExtentionObj.OuterHttpUrl = ComFunc.nvl(copyp[DomainKey.CONFIG, "trade_server_url"]) + tradename + "." + instanceid + (action == "" ? "" : "." + action) + ".tra";
                if (data != null)
                {
                    copyd.ExtentionObj.OuterHttpPostData = data;
                }
                copyd.SetValue("callback", callback);
                var proxy = ModuleProxyManager.BeginCall<TradeAsyncProxy, WebParameter, GoData>(copyp, copyd, TradeServiceCallBack);
            }

            private void TradeServiceCallBack(WebParameter arg1, GoData arg2)
            {
                var callback = arg2.GetValue("callback") == null ? null : (Action<FrameDLRObject>)arg2.GetValue("callback");
                dynamic result = (FrameDLRObject)arg2.ExtentionObj.OuterHttpResult;
                if (result.ErrorCode != "")
                {
                    GlobalCommon.Logger.WriteLog(LoggerLevel.ERROR, "Call Trade Service Error:" + result.ErrorMessage);
                    //throw new Exception("Call Trade Service Error:" + result.errormsg);
                }
                else
                {
                    if (callback != null)
                    {
                        callback(result.Content);
                    }
                }
            }
            /// <summary>
            /// 呼叫微信服务
            /// </summary>
            /// <param name="url"></param>
            /// <param name="contenttype"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            public object CallWeixinServer(string url, string contenttype, FrameDLRObject data)
            {
                return CallWeixinServer(url, contenttype, data, false);
            }
            /// <summary>
            /// 呼叫微信服务
            /// </summary>
            /// <param name="url"></param>
            /// <param name="contenttype"></param>
            /// <param name="data"></param>
            /// <param name="isneedcert">是否需要使用数字证书访问</param>
            /// <returns></returns>
            public object CallWeixinServer(string url, string contenttype, FrameDLRObject data,bool isneedcert)
            {
                return CallWeixinServer(url, "", contenttype, data, isneedcert);
            }
            /// <summary>
            /// 呼叫微信服务
            /// </summary>
            /// <param name="url"></param>
            /// <param name="method"></param>
            /// <param name="contenttype"></param>
            /// <param name="data"></param>
            /// <param name="isneedcert"></param>
            /// <returns></returns>
            public object CallWeixinServer(string url,string method, string contenttype, FrameDLRObject data, bool isneedcert)
            {
                var copyp = _logic.CallContext_Parameter.Clone<WebParameter>();
                var copyd = _logic.CallContext_DataCollection.Clone<GoData>();
                copyd.ExtentionObj.OuterHttpUrl = url;
                copyd.ExtentionObj.type = contenttype;
                copyd.ExtentionObj.method = method;
                if (data != null)
                {
                    copyd.ExtentionObj.OuterHttpPostData = data;
                }
                if (isneedcert)
                {
                    copyd.ExtentionObj.x509cert = new X509Certificate2(_logic.CallContext_Parameter.ExtentionObj.weixin.weixin_mch_ssl_path, "1269527601", X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet);
                }
                ModuleProxyManager.Call<WeixinHttpProxy, WebParameter, GoData>(copyp, copyd);
                return copyd.ExtentionObj.OuterHttpResult;
            }

            public void CallWeixinServerAsync(string url, string contenttype, FrameDLRObject data, bool isneedcert, string callbackLA,FrameDLRObject recorddata)
            {
                var copyp = _logic.CallContext_Parameter.Clone<WebParameter>();
                var copyd = _logic.CallContext_DataCollection.Clone<GoData>();
                copyd.ExtentionObj.OuterHttpUrl = url;
                copyd.ExtentionObj.type = contenttype;
                if (data != null)
                {
                    copyd.ExtentionObj.OuterHttpPostData = data;
                }
                if (isneedcert)
                {
                    copyd.ExtentionObj.x509cert = new X509Certificate2(_logic.CallContext_Parameter.ExtentionObj.weixin.weixin_mch_ssl_path, "1269527601", X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet);
                }


                ResourceManage rema = new ResourceManage();
                copyp.SetValue<ResourceManage>(ParameterKey.RESOURCE_MANAGER, rema);
                var defaulttoken = TransactionToken.NewToken();
                copyp.TransTokenList.Add(defaulttoken);
                copyp.SetValue<TransactionToken>(ParameterKey.TOKEN, defaulttoken);
                copyp.SetValue("IsAjaxAsync", false);
                copyd.SetValue("recorddata", recorddata == null ? FrameDLRObject.CreateInstance() : recorddata);
                copyd.SetValue("callback", callbackLA);
                var dt1 = DateTime.Now;
                ModuleProxyManager.BeginCall<WeixinAsyncHttpProxy, WebParameter, GoData>(copyp, copyd,WeixinCallback);
                var dt2 = DateTime.Now;
                GlobalCommon.Logger.WriteLog(LoggerLevel.DEBUG, "CallWeixinServerAsync cost:" + ComFunc.nvl(copyp.ExtentionObj.asynccallcost));
                
            }

            private void WeixinCallback(WebParameter arg1, GoData arg2)
            {
                var callback = ComFunc.nvl(arg2.GetValue("callback"));
                var recorddata = arg2.GetValue("recorddata") == null ? null : (FrameDLRObject)arg2.GetValue("recorddata");
                FrameDLRObject result = (FrameDLRObject)arg2.ExtentionObj.OuterHttpResult;
                GlobalCommon.Logger.WriteLog(LoggerLevel.DEBUG, "WeixinCallBack result:" + result.ToJSONString());
                arg1.SetValue(DomainKey.POST_DATA, "weixincallbackresult", result);
                if (callback != "")
                {
                    string[] la = callback.Split('.');
                    var logic = "";
                    var action = "";
                    if (la.Length > 0)
                    {
                        logic = la[0];
                    }
                    if (la.Length > 1)
                    {
                        action = la[1];
                    }

                    arg1.RequestResourceName = logic;
                    arg1.Action = action;

                    if (recorddata != null)
                    {
                        foreach (var item in recorddata.Keys)
                        {
                            arg1.SetValue(DomainKey.POST_DATA, item, recorddata.GetValue(item));
                        }
                    }
                    CallLocalLogic(logic, action, arg1, arg2);
                }

            }
            /// <summary>
            /// 呼叫微信服务
            /// </summary>
            /// <param name="url"></param>
            /// <param name="contenttype"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            public object CallWeixinServer(string url, string contenttype="", params KeyValuePair<string, object>[] data)
            {
                FrameDLRObject dobj = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        dobj.SetValue(item.Key, item.Value);
                    }
                    
                }
                return CallWeixinServer(url, contenttype, dobj);
                
            }
            public object CallWeixinServer(string url, params KeyValuePair<string, object>[] data)
            {
                return CallWeixinServer(url, "", data);
            }
            /// <summary>
            /// 呼叫环信服务
            /// </summary>
            /// <param name="url"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            public object CallWebIMServer(string url,string method,string contenttype, FrameDLRObject header,FrameDLRObject data)
            {
                var copyp = _logic.CallContext_Parameter.Clone<WebParameter>();
                var copyd = _logic.CallContext_DataCollection.Clone<GoData>();
                copyd.ExtentionObj.OuterHttpUrl = url;
                copyd.ExtentionObj.type = contenttype;
                copyd.ExtentionObj.method = method;

                if (header != null)
                {
                    copyd.ExtentionObj.HeadData = header;
                }
                if (data != null)
                {
                    copyd.ExtentionObj.OuterHttpPostData = data;
                }
                
                ModuleProxyManager.Call<WebIMHttpProxy, WebParameter, GoData>(copyp, copyd);
                return copyd.ExtentionObj.OuterHttpResult;
            }
            /// <summary>
            /// 调用微信服务
            /// </summary>
            /// <param name="url"></param>
            /// <param name="json"></param>
            /// <returns></returns>
            public object CallWeixinServer(string url, string json)
            {
                var copyp = _logic.CallContext_Parameter.Clone<WebParameter>();
                var copyd = _logic.CallContext_DataCollection.Clone<GoData>();
                copyd.ExtentionObj.OuterHttpUrl = url;

                if (!string.IsNullOrEmpty(json))
                {
                    FrameDLRObject dobj = FrameDLRObject.CreateInstance(json, FrameDLRFlags.SensitiveCase);
                    copyd.ExtentionObj.OuterHttpPostData = dobj;
                }
                ModuleProxyManager.Call<WeixinHttpProxy, WebParameter, GoData>(copyp, copyd);
                return copyd.ExtentionObj.OuterHttpResult;
            }
            /// <summary>
            /// 呼叫阿里OpenIM服务
            /// </summary>
            /// <param name="url"></param>
            /// <param name="method"></param>
            /// <param name="contenttype"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            public object CallAlyOpenIMServer(string url, string method, string contenttype, FrameDLRObject data)
            {
                var copyp = _logic.CallContext_Parameter.Clone<WebParameter>();
                var copyd = _logic.CallContext_DataCollection.Clone<GoData>();
                copyd.ExtentionObj.OuterHttpUrl = url;
                copyd.ExtentionObj.type = contenttype;
                copyd.ExtentionObj.method = method;

                if (data != null)
                {
                    copyd.ExtentionObj.OuterHttpPostData = data;
                }

                ModuleProxyManager.Call<AliOpenIMHttpProxy, WebParameter, GoData>(copyp, copyd);
                return copyd.ExtentionObj.OuterHttpResult;
            }

            /// <summary>
            /// 呼叫阿里云服务
            /// </summary>
            /// <param name="url"></param>
            /// <param name="method"></param>
            /// <param name="contenttype"></param>
            /// <param name="headdata"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            public object CallAliService(string url, string method, string contenttype, FrameDLRObject headdata, FrameDLRObject data)
            {
                var copyp = _logic.CallContext_Parameter.Clone<WebParameter>();
                var copyd = _logic.CallContext_DataCollection.Clone<GoData>();
                copyd.ExtentionObj.OuterHttpUrl = url;
                copyd.ExtentionObj.type = contenttype;
                copyd.ExtentionObj.method = method;

                if (headdata != null)
                {
                    copyd.ExtentionObj.HeadData = headdata;
                }

                if (data != null)
                {
                    copyd.ExtentionObj.OuterHttpPostData = data;
                }

                ModuleProxyManager.Call<AliHttpProxy, WebParameter, GoData>(copyp, copyd);
                return copyd.ExtentionObj.OuterHttpResult;
            }

            /// <summary>
            /// 调用接口发送短信
            /// </summary>
            /// <param name="message"></param>
            /// <param name="phoneNo"></param>
            /// <returns></returns>
            public bool SendMessage(string message, string phoneNo)
            {
                var copyp = _logic.CallContext_Parameter.Clone<WebParameter>();
                var copyd = _logic.CallContext_DataCollection.Clone<GoData>();
                //* 如果手机号为空则不发短信
                if (string.IsNullOrEmpty(phoneNo) || phoneNo.Length != 11)
                    return true;
                copyd.ExtentionObj.mobiles = phoneNo;
                copyd.ExtentionObj.message = message;
                ModuleProxyManager.Call<SMSProxy, WebParameter, WebBaseData>(copyp, copyd);
                try
                {
                    var rtn = (bool)copyd.ExtentionObj.issuccess;
                    return rtn;
                }
                catch (Exception ex)
                {
                    GlobalCommon.Logger.WriteLog(LoggerLevel.ERROR, "发送短信失败，原因：" + ex.Message);
                    return false;
                }
            }

            /// <summary>
            /// 发送短信验证码
            /// </summary>
            /// <param name="msg"></param>
            /// <param name="phone"></param>
            /// <param name="uid"></param>
            /// <returns></returns>
            public bool SendSMSValidCode(string phone,out string uid)
            {
                var r = ComFunc.Random(6);
                uid = Guid.NewGuid().ToString();
                if (SendMessage(string.Format("亲爱的聚美医用户，您的短信验证码为：{0}，有效时间为2分钟。", r), phone))
                {
                    GlobalCommon.Logger.WriteLog(LoggerLevel.DEBUG, string.Format("发送给{0}验证码：{1}", phone, r));
                    _logic.CacheHelper.SetCache(uid, r, DateTime.Now.AddMinutes(2));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            /// <summary>
            /// 进行验证码的校验
            /// </summary>
            /// <param name="uid"></param>
            /// <param name="code"></param>
            /// <returns></returns>
            public bool ValidSMSCode(string uid, string code)
            {
                var oricode = ComFunc.nvl(_logic.CacheHelper.GetCache(uid));
                if (oricode == code)
                {
                    _logic.CacheHelper.RemoveCache(uid);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}