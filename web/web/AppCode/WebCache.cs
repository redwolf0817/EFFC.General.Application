using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Interfaces.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace web.AppCode
{
    public class WebCache:IFrameCache
    {
        private static object lockobj = new object();
        public object Get(string key)
        {
            lock (lockobj)
            {
                return HttpRuntime.Cache.Get(ComFunc.nvl(key));
            }
        }

        public void Remove(string key)
        {
            lock (lockobj)
            {
                HttpRuntime.Cache.Remove(ComFunc.nvl(key));
            }
        }

        public void Set(string key, object obj, TimeSpan slide)
        {
            lock (lockobj)
            {
                var ckey = ComFunc.nvl(key);
                if (HttpRuntime.Cache.Get(ckey) != null)
                {
                    HttpRuntime.Cache.Remove(ckey);
                    
                }
                HttpRuntime.Cache.Insert(ckey, obj, null, Cache.NoAbsoluteExpiration, slide);
            }
        }

        public void Set(string key, object obj, DateTime expira)
        {
            lock (lockobj)
            {
                var ckey = ComFunc.nvl(key);
                if (HttpRuntime.Cache.Get(ckey) != null)
                {
                    HttpRuntime.Cache.Remove(ckey);

                }
                HttpRuntime.Cache.Insert(ckey, obj, null, expira, Cache.NoSlidingExpiration);
            }
        }
    }
}