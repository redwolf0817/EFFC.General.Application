using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data.Base;
using EFFC.Frame.Net.Data.LogicData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Builder.Web.Logic
{
    public abstract class HostDebugBaseLogic:GoLogic
    {
        private static Dictionary<string, HostDebugEntity> _l = new Dictionary<string, HostDebugEntity>();
        protected static object lockobj = new object();
        int mythreadid = -1;
        /// <summary>
        /// Debug监控器进行的状态
        /// </summary>
        public enum DebugState
        {
            Open,
            Closed,
            Debuging,
            None
        }

        /// <summary>
        /// 当前的DebugCode
        /// </summary>
        protected abstract string Current_Debug_Code
        {
            get;
        }
        /// <summary>
        /// 获取当前debug的状态
        /// </summary>
        public abstract DebugState CurrentState
        {
            get;
        }
        /// <summary>
        /// 获取自身主线程号
        /// </summary>
        public virtual int MyThreadID
        {
            get
            {
                return Thread.CurrentThread.ManagedThreadId;
            }
        }
        public void WartForProcessing()
        {
            var obj = FrameDLRObject.CreateInstanceFromat(@"{
                command:{0},
                express:{1}
            }", CallContext_Parameter[DomainKey.POST_DATA, "command"], CallContext_Parameter[DomainKey.POST_DATA, "express"]);
            do
            {
                ProcessDebugCommand(obj);
                obj = WS.Recieve();
            } while (!WS.IsClose);
        }

        public void ProcessDebugCommand(object obj)
        {
            var rtn = FrameDLRObject.CreateInstanceFromat(@"
{
issuccess:true,
msg:'',    
command:'',
express:'',
result:null  
}");
            if (obj is FrameDLRObject)
            {
                dynamic dobj = obj;
                if (dobj.command == "debugcode")
                {
                    var code = Current_Debug_Code;

                    rtn.command = dobj.command;
                    rtn.express = "";
                    rtn.result = code;
                }
                else if (dobj.command == "source")
                {
                    rtn.command = dobj.command;
                    rtn.express = "";
                    var target = GetTarget(Current_Debug_Code);
                    if (target != null)
                    {
                        rtn.result = ComFunc.Base64Code(target.Current_Logic_Code);
                        rtn.msg = ComFunc.Base64Code("获取logic代码成功");
                    }
                    else
                    {
                        rtn.issuccess = false;
                        rtn.msg = ComFunc.Base64Code("缺少被监控对象");
                    }

                }
                else if (dobj.command == "viewsource")
                {
                    rtn.command = dobj.command;
                    rtn.express = "";
                    var target = GetTarget(Current_Debug_Code);
                    if (target != null)
                    {
                        rtn.result = ComFunc.Base64Code(target.Current_View_Code);
                        rtn.msg = ComFunc.Base64Code("获取view代码成功");
                    }
                    else
                    {
                        rtn.issuccess = false;
                        rtn.msg = ComFunc.Base64Code("缺少被监控对象");
                    }

                }
                else if (dobj.command == "set_source")
                {
                    rtn.command = dobj.command;
                    rtn.express = dobj.express;

                    var target = GetTarget(Current_Debug_Code);
                    if (target != null)
                    {
                        if (rtn.express != null)
                        {
                            target.Current_Logic_Code = ComFunc.Base64DeCode(ComFunc.nvl(rtn.express).Replace(" ", "+"));

                            rtn.msg = ComFunc.Base64Code("写入新代码成功");
                        }
                        else
                        {
                            rtn.issuccess = false;
                            rtn.msg = ComFunc.Base64Code("缺少更新的代码");
                        }
                    }
                    else
                    {
                        rtn.issuccess = false;
                        rtn.msg = ComFunc.Base64Code("缺少被监控对象");
                    }
                }
                else if (dobj.command == "set_viewsource")
                {
                    rtn.command = dobj.command;
                    rtn.express = dobj.express;

                    var target = GetTarget(Current_Debug_Code);
                    if (target != null)
                    {
                        if (rtn.express != null)
                        {
                            target.Current_View_Code = ComFunc.Base64DeCode(ComFunc.nvl(rtn.express).Replace(" ", "+"));

                            rtn.msg = ComFunc.Base64Code("写入新代码成功");
                        }
                        else
                        {
                            rtn.issuccess = false;
                            rtn.msg = ComFunc.Base64Code("缺少更新的代码");
                        }
                    }
                    else
                    {
                        rtn.issuccess = false;
                        rtn.msg = ComFunc.Base64Code("缺少被监控对象");
                    }
                }
                else if (dobj.command == "checkobj")
                {
                    rtn.command = dobj.command;
                    rtn.express = dobj.express;

                    var target = GetTarget(Current_Debug_Code);
                    if (target != null)
                    {
                        var express = ComFunc.Base64DeCode(ComFunc.nvl(dobj.express).Replace(" ", "+"));
                        var re = target.CheckObject(express);
                        rtn.issuccess = re.issuccess;
                        rtn.result = re.result;

                        rtn.msg = ComFunc.Base64Code(re.msg);
                    }
                    else
                    {
                        rtn.issuccess = false;
                        rtn.msg = ComFunc.Base64Code("缺少被监控对象");
                    }
                }
                else if (dobj.command == "console")
                {
                    rtn.command = dobj.command;
                    rtn.express = dobj.express;

                    var target = GetTarget(Current_Debug_Code);
                    if (target != null)
                    {
                        var express = ComFunc.Base64DeCode(ComFunc.nvl(dobj.express).Replace(" ", "+"));
                        var re = target.GetConsole();
                        rtn.issuccess = re.issuccess;
                        rtn.result = ComFunc.Base64Code(re.result);
                        rtn.msg = ComFunc.Base64Code(re.msg);
                    }
                    else
                    {
                        rtn.issuccess = false;
                        rtn.msg = ComFunc.Base64Code("缺少被监控对象");
                    }
                }
                else if (dobj.command == "resume")
                {
                    rtn.command = dobj.command;
                    rtn.express = dobj.express;
                    var target = GetTarget(Current_Debug_Code);
                    if (target != null)
                    {
                        target.Resume();
                    }
                    else
                    {
                        rtn.issuccess = false;
                        rtn.msg = ComFunc.Base64Code("缺少被监控对象");
                    }

                }
                else if (dobj.command == "pendflag")
                {
                    rtn.command = "pend";
                    rtn.express = dobj.express;

                }
            }
            if (!WS.IsClose)
            {
                WS.Send(rtn);
            }
        }


        #region debug模式下监控器与监控目标控制
        /// <summary>
        /// 获取监视目标
        /// </summary>
        /// <param name="debugcode"></param>
        /// <returns></returns>
        public static FrameHostJsLogic GetTarget(string debugcode)
        {
            if (_l.ContainsKey(debugcode))
            {
                return _l[debugcode].Target;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 获取监视者
        /// </summary>
        /// <param name="debugcode"></param>
        /// <returns></returns>
        public static HostDebugBaseLogic GetMonitor(string debugcode)
        {
            if (_l.ContainsKey(debugcode))
            {
                return _l[debugcode].Monitor;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 添加监控者
        /// </summary>
        /// <param name="debugcode"></param>
        /// <param name="monitor"></param>
        public static void AddMonitor(string debugcode, HostDebugBaseLogic monitor)
        {
            if (string.IsNullOrEmpty(debugcode) || monitor == null) return;

            lock (lockobj)
            {
                if (!_l.ContainsKey(debugcode))
                {
                    _l[debugcode] = new HostDebugEntity();
                }
                if (_l[debugcode].Monitor == null)
                {
                    _l[debugcode].Monitor = monitor;
                }
                else
                {
                    throw new Exception("Monitor exits!!!");
                }
            }
        }
        /// <summary>
        /// 添加新的被监视对象
        /// </summary>
        /// <param name="debugcode"></param>
        /// <param name="target"></param>
        public static void AddTarget(string debugcode, FrameHostJsLogic target)
        {
            if (string.IsNullOrEmpty(debugcode) || target == null) return;

            lock (lockobj)
            {
                if (!_l.ContainsKey(debugcode))
                {
                    _l[debugcode] = new HostDebugEntity();
                }
                if (_l[debugcode].Target == null)
                {
                    _l[debugcode].Target = target;
                }
                else
                {
                    throw new Exception("Target exits!!!If you want to add target,you should remove the one exits!");
                }
            }
        }
        /// <summary>
        /// 移除现有的被监视对象
        /// </summary>
        /// <param name="debugcode"></param>
        public static void RemoveTarget(string debugcode)
        {
            lock (lockobj)
            {
                if (_l.ContainsKey(debugcode))
                {
                    _l[debugcode].Target = null;
                }
            }
        }
        /// <summary>
        /// 释放debug entity
        /// </summary>
        /// <param name="debugcode"></param>
        public static void ReleaseDebugEntity(string debugcode)
        {
            lock (lockobj)
            {
                if (_l.ContainsKey(debugcode))
                {
                    _l[debugcode].Release();
                    _l.Remove(debugcode);
                }
            }
        }

        private class HostDebugEntity
        {
            HostDebugBaseLogic _monitor;
            FrameHostJsLogic _target;
            string _debugcode = "";

            /// <summary>
            /// 监视者
            /// </summary>
            public HostDebugBaseLogic Monitor
            {
                get
                {
                    return _monitor;
                }
                set
                {
                    _monitor = value;
                }
            }
            /// <summary>
            /// 被监视目标
            /// </summary>
            public FrameHostJsLogic Target
            {
                get
                {
                    return _target;
                }
                set
                {
                    _target = value;
                }
            }
            /// <summary>
            /// debug编码
            /// </summary>
            public string DebugCode
            {
                get
                {
                    return _debugcode;
                }
                set
                {
                    _debugcode = value;
                }
            }
            /// <summary>
            /// 释放资源，切断对象的联系
            /// </summary>
            public void Release()
            {
                _monitor = null;
                _target = null;
                _debugcode = null;
            }
        }
        #endregion
    }
}
