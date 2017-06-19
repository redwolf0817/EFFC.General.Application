using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data;
using EFFC.Frame.Net.Base.Interfaces.System;
using EFFC.Frame.Net.Base.Parameter;
using EFFC.Frame.Net.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace web.AppCode
{
    public class ExceptionProcess : IExceptionProcess
    {
        public void ProcessException(object sender, Exception ex, ParameterStd p, DataCollection d)
        {
            GlobalCommon.Logger.WriteLog(LoggerLevel.ERROR, ex.Message + "\n" + ex.StackTrace);
        }
    }
}