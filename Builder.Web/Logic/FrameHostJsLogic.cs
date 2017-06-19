using Builder.Web.Global;
using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data.Base;
using EFFC.Frame.Net.Base.ResouceManage;
using EFFC.Frame.Net.Base.ResouceManage.JsEngine;
using EFFC.Frame.Net.Base.Token;
using EFFC.Frame.Net.Business.Engine;
using EFFC.Frame.Net.Business.Logic;
using EFFC.Frame.Net.Data.LogicData;
using EFFC.Frame.Net.Data.Parameters;
using EFFC.Frame.Net.Data.WebData;
using EFFC.Frame.Net.Global;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Builder.Web.Logic
{
    public class FrameHostJsLogic : HostJsLogic
    {
        HostLogicEngine _curhle = null;

        string current_logic_code = "";
        string current_view_code = "";
        string _debug_code = "";
        string _is_debugstr = "";
        string _debugmode = "";
        static object lockobj = new object();
        static ManualResetEvent _mre = new ManualResetEvent(false);
        public FrameHostJsLogic()
            : base()
        {
            
        }
        public string Current_Logic_Code
        {
            get { return current_logic_code; }
            set { current_logic_code = value; }
        }
        public string Current_View_Code
        {
            get { return current_view_code; }
            set { current_view_code = value; }
        }
        /// <summary>
        /// 恢复中断的线程,仅适用于debug模式下
        /// </summary>
        public void Resume()
        {
            _mre.Set();
        }
        /// <summary>
        /// 将当前线程中断,仅适用于debug模式下
        /// </summary>
        public void Suspend()
        {
            if (!IsDebug || !IsDebugMode) return;

            _mre.WaitOne();
            _mre.Reset();
        }
        protected override string GetLogicRootPath()
        {
            return GlobalCommon.HostCommon.RootPath + HostJsConstants.COMPILED_LOGIC_PATH;
        }
        protected override void DoInvoke(WebParameter p, GoData d, LogicData ld)
        {
            if (_curhle == null) _curhle = GetHostLogicEngine();
            //在本实例主线程中先保存DebugCode和Debug的值，用于多线程调用
            _debug_code = ComFunc.nvl(p[DomainKey.QUERY_STRING, "DebugCode"]);
            _debug_code = _debug_code == "" ? ComFunc.nvl(p[DomainKey.POST_DATA, "DebugCode"]) : _debug_code;
            _is_debugstr = ComFunc.nvl(CallContext_Parameter[DomainKey.QUERY_STRING, "Debug"]);
            _is_debugstr = _is_debugstr == "" ? ComFunc.nvl(CallContext_Parameter[DomainKey.POST_DATA, "Debug"]) : _is_debugstr;
            _debugmode = ComFunc.nvl(Configs["DebugMode"]);

            var logicfile = p.RequestResourceName + (ComFunc.nvl(p.Action) != "" ? "." + p.Action : "") + ".hjs";
            var path = GetLogicRootPath() + @"/" + logicfile;
            var hle = _curhle;
            var js = File.ReadAllText(path);

            object result = null;
            try
            {

                    //debug模式
                    if (IsDebug && IsDebugMode)
                    {
                        //有debug码
                        if (DebugCode != "")
                        {
                            var monitor = HostDebugBaseLogic.GetMonitor(DebugCode);
                            if (monitor != null)
                            {
                                //添加被监控对象
                                HostDebugBaseLogic.AddTarget(DebugCode, this);
                                if (current_logic_code == "")
                                {
                                    current_logic_code = js;
                                }
                                //呼叫监控器向远端发送logic的代码
                                monitor.ProcessDebugCommand(FrameDLRObject.CreateInstanceFromat(@"{
command:{0},
express:{1}
}", "source", ""));

                                //先挂起，等待监控器的指令
                                if (monitor.CurrentState == HostDebugBaseLogic.DebugState.Debuging)
                                {
                                    monitor.ProcessDebugCommand(FrameDLRObject.CreateInstanceFromat(@"{
command:{0},
express:{1}
}", "pendflag", ""));
                                    Suspend();
                                    
                                }
                                var tmpre = hle.DebugRunJs(current_logic_code);
                                if (current_view_code == "" && d.ContentType == GoResponseDataType.HostView)
                                {
                                    //获取view路径
                                    string viewpath = ComFunc.nvl(d.ExtentionObj.hostviewpath);
                                    viewpath = viewpath.Replace("~", GlobalCommon.HostCommon.RootPath + HostJsConstants.COMPILED_VIEW_PATH);
                                    current_view_code = File.ReadAllText(viewpath, Encoding.UTF8);
                                }
                                //呼叫监控器向远端发送view的代码
                                monitor.ProcessDebugCommand(FrameDLRObject.CreateInstanceFromat(@"{
command:{0},
express:{1}
}", "viewsource", ""));
                                //挂起，等待监视器指令
                                if (monitor.CurrentState == HostDebugBaseLogic.DebugState.Debuging)
                                {
                                    monitor.ProcessDebugCommand(FrameDLRObject.CreateInstanceFromat(@"{
command:{0},
express:{1}
}", "pendflag", ""));
                                    Suspend();
                                }
                                if (current_view_code != "")
                                {
                                    //调用hostview引擎进行渲染
                                    HostJsView hjv = (HostJsView)p.ExtentionObj.hostviewengine;
                                    hjv.CurrentContext.SetDataModel(tmpre);
                                    var html = hjv.Render(current_view_code);
                                    result = html;
                                    d.ContentType = GoResponseDataType.HTML;
                                }
                                else
                                {
                                    result = tmpre;
                                }
                            }
                            else
                            {
                                result = hle.RunJs(js);
                            }
                        }
                        else
                        {
                            result = hle.RunJs(js);
                        }

                    }
                    else
                    {
                        result = hle.RunJs(js);
                    }
            }
            finally
            {
                if (IsDebug && DebugCode != "")
                {
                    hle.CurrentContext.CurrentJsEngine.Release();
                    //清除被监控对象
                    HostDebugBaseLogic.RemoveTarget(DebugCode);
                }
                
            }
            d.ResponseData = result;
        }
        protected override HostLogicEngine GetHostLogicEngine()
        {
            var hle = base.GetHostLogicEngine();
            hle.CurrentContext.AddHostJsObject(new OuterInterfaceObject(this));
            hle.CurrentContext.AddHostJsObject(new WeixinObject(this));
            hle.CurrentContext.AddHostJsObject(new DebugObject(this));
            return hle;
        }
        /// <summary>
        /// 获取FrameLogic用到的HostLogicEngine
        /// </summary>
        /// <returns></returns>
        public static HostLogicEngine GetCurrentLogicEngine()
        {
            FrameHostJsLogic l = new FrameHostJsLogic();
            var p = new WebParameter();
            var d = new GoData();
            var _rm = p.GetValue<ResourceManage>(ParameterKey.RESOURCE_MANAGER);
            var _token = p.GetValue<TransactionToken>(ParameterKey.TOKEN);
            GlobalPrepare.ConfigPrepare(ref p);

            CallContext.SetData(l.Name + CALL_CONTEXT_PARAMETER, p);
            CallContext.SetData(l.Name + CALL_CONTEXT_DATACOLLECTION, d);
            CallContext.SetData(l.Name + CALL_CONTEXT_RESOURCEMANAGER, _rm);
            CallContext.SetData(l.Name + CALL_CONTEXT_TRANSTOKEN, _token);
            if (l._curhle == null) l._curhle = l.GetHostLogicEngine();
            return l._curhle;
        }

        /// <summary>
        /// 判断当前系统是否处于Debug开发模式
        /// </summary>
        public bool IsDebugMode
        {
            get
            {
                if (_debugmode == "")
                {
                    return false;
                }
                else
                {
                    return bool.Parse(_debugmode);
                }
            }
        }
        /// <summary>
        /// 判断当前的请求是否为Debug请求
        /// </summary>
        public bool IsDebug
        {
            get
            {
                if (_is_debugstr == "1" || _is_debugstr.ToLower() == "true")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        /// <summary>
        /// Debug序列码
        /// </summary>
        public string DebugCode
        {
            get
            {
                return _debug_code;
            }
        }
        /// <summary>
        /// debug模式下执行对象检查，根据表达式获取对象结果
        /// </summary>
        /// <param name="express"></param>
        /// <returns></returns>
        public dynamic CheckObject(string express)
        {
            var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:false,
msg:''
}");
            if (!IsDebug || !IsDebugMode) return rtn;

            if (ComFunc.nvl(express) != "")
            {
                var execjs = express;

                try
                {
                    _curhle.DebugRunJs("var __tmp_out__=" + execjs);
                    var re = _curhle.CurrentContext.CurrentJsEngine.GetOutObject("__tmp_out__");
                    rtn.issuccess = true;
                    rtn.result = re;
                }
                catch (Noesis.Javascript.JavascriptException ex)
                {
                    var sbstr = new StringBuilder();
                    sbstr.AppendLine("执行代码失败！");
                    sbstr.AppendLine("错误信息:" + ex.Message);
                    sbstr.AppendLine("出错代码行数:" + ex.Line);
                    sbstr.AppendLine("出错代码列数:" + ex.StartColumn + "~" + ex.EndColumn);
                    sbstr.AppendLine("出错代码位置:" + ex.V8SourceLine);
                    sbstr.AppendLine("出错源代码:" + execjs);
                    rtn.issuccess = false;
                    rtn.msg = sbstr.ToString();
                }
                catch (Exception ex)
                {
                    var sbstr = new StringBuilder();
                    sbstr.AppendLine("执行代码失败！");
                    sbstr.AppendLine("错误信息:" + ex.Message);
                    rtn.issuccess = false;
                    rtn.msg = sbstr.ToString();
                }
            }
            else
            {
                rtn.issuccess = false;
                rtn.msg = "缺少表达式";
            }

            return rtn;
        }
        /// <summary>
        /// 获取console控制台信息，仅适用于debug模式下
        /// </summary>
        /// <returns></returns>
        public dynamic GetConsole()
        {
            var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:false,
msg:''
}");
            if (!IsDebug || !IsDebugMode) return rtn;

            try
            {
                var re = ((ConsoleObject)_curhle.CurrentContext.GetHostJsObject("ConsoleS")).OutMsg;
                rtn.issuccess = true;
                rtn.result = re;
                WS.Send(rtn);
            }
            catch (Noesis.Javascript.JavascriptException ex)
            {
                var sbstr = new StringBuilder();
                sbstr.AppendLine("执行代码失败！");
                sbstr.AppendLine("错误信息:" + ex.Message);
                sbstr.AppendLine("出错代码行数:" + ex.Line);
                sbstr.AppendLine("出错代码列数:" + ex.StartColumn + "~" + ex.EndColumn);
                sbstr.AppendLine("出错代码位置:" + ex.V8SourceLine);
                sbstr.AppendLine("出错源代码:" + current_logic_code);
                rtn.issuccess = false;
                rtn.msg = sbstr.ToString();
            }
            catch (Exception ex)
            {
                var sbstr = new StringBuilder();
                sbstr.AppendLine("执行代码失败！");
                sbstr.AppendLine("错误信息:" + ex.Message);
                rtn.issuccess = false;
                rtn.msg = sbstr.ToString();
            }

            return rtn;
        }
    }
}

