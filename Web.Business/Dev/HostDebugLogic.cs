using Builder.Web.Logic;
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

namespace Web.Business.Dev
{
    public class HostDebugLogic:HostDebugBaseLogic
    {
        string current_debug_code = "";
        DebugState _state = DebugState.None;
        int _threadid = -1;
        protected override Func<LogicData, object> GetFunction(string actionName)
        {
            current_debug_code = DateTime.Now.ToString("yyyyMMddHHmmss");
            switch (actionName.ToLower())
            {
                default:
                    return Load;
            }
        }

        private object Load(LogicData arg)
        {
            _state = DebugState.Open;
            try
            {
                if (IsDebug && IsWebSocket)
                {
                    _threadid = Thread.CurrentThread.ManagedThreadId;
                    //启用监控器
                    AddMonitor(Current_Debug_Code, this);
                    _state = DebugState.Debuging;
                    //挂起进行指令接收
                    WartForProcessing();

                    return FrameDLRObject.CreateInstance(@"{
issuceess:true,
msg:'Debug process complete!!!'
}");
                }
                else
                {
                    return FrameDLRObject.CreateInstance(@"{
issuceess:true,
msg:'Debug process complete!!!'
}");
                }
            }
            finally
            {
                _state = DebugState.Closed;
                //避免目标线程挂起
                if (GetTarget(current_debug_code) != null)
                {
                    GetTarget(current_debug_code).Resume();
                    
                    //让子线程先释放
                    Thread.Sleep(2000);
                }
                //Debug模式下需要手动释放js引擎资源
                if (IsDebug && IsWebSocket)
                {
                    ReleaseDebugEntity(current_debug_code);
                }
            }
        }

        public override string Name
        {
            get { return "hostdebug"; }
        }


        protected override string Current_Debug_Code
        {
            get { return current_debug_code; }
        }

        public override HostDebugBaseLogic.DebugState CurrentState
        {
            get { return _state; }
        }

        public override int MyThreadID
        {
            get
            {
                return base.MyThreadID;
            }
        }
    }
}
