using EFFC.Frame.Net.Base.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Builder.Web.Helper
{
    public class HostJsServer
    {
        DateTimeHelper _sd = new DateTimeHelper();
        object _console;
        public HostJsServer()
        {
        }

        public DateTimeHelper Date
        {
            get
            {
                return _sd;
            }
        }

        public Object Console
        {
            get
            {
                if (_console == null)
                {
                    System.Console.OutputEncoding = Encoding.Unicode;
                    _console = System.Console.Out;
                }
                return _console;
            }
            set
            {

            }
        }

        public sealed class DateTimeHelper
        {
            public DateTime Now
            {
                get
                {
                    return DateTime.Now;
                }
            }

            public DateTime ParseDateTime(object obj)
            {
                return DateTimeStd.ParseStd(obj).Value;
            }

            public string ToString(DateTime dt, string formater)
            {
                return dt.ToString(formater);
            }

            public int CompareTo(DateTime dt1, DateTime dt2)
            {
                return dt1.CompareTo(dt2);
            }

            public int DateDiff(object dt1, object dt2, string mode)
            {
                int rtn = 0;
                var dts1 = DateTimeStd.ParseStd(dt1);
                var dts2 = DateTimeStd.ParseStd(dt2);
                TimeSpan ts1 = new TimeSpan(dts1.Value.Ticks);
                TimeSpan ts2 = new TimeSpan(dts2.Value.Ticks);

                TimeSpan ts = ts1.Subtract(ts2).Duration();
                var modetrans = mode.ToLower();
                if (modetrans == "day" || modetrans == "days" || modetrans == "dd" || modetrans == "d")
                {
                    modetrans = "day";
                }
                else if (modetrans == "hour" || modetrans == "hours" || mode == "HH" || mode == "H")
                {
                    modetrans = "hour";
                }
                else if (modetrans == "minute" || modetrans == "minutes" || mode == "mm" || mode == "m")
                {
                    modetrans = "minute";
                }
                else if (modetrans == "second" || modetrans == "seconds" || mode == "ss" || mode == "s")
                {
                    modetrans = "second";
                }
                else if (modetrans == "millisecond" || modetrans == "milliseconds" || mode == "fff" || mode == "f")
                {
                    modetrans = "millisecond";
                }

                switch (modetrans)
                {
                    case "day":
                        rtn = ts.Days;
                        break;
                    case "hour":
                        rtn = ts.Hours;
                        break;
                    case "minute":
                        rtn = ts.Minutes;
                        break;
                    case "second":
                        rtn = ts.Seconds;
                        break;
                    case "millisecond":
                        rtn = ts.Milliseconds;
                        break;
                }

                return rtn;
            }
        }
    }
}
