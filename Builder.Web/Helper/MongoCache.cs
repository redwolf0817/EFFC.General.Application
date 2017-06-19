using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data;
using EFFC.Frame.Net.Base.Data.Base;
using EFFC.Frame.Net.Base.Interfaces.System;
using EFFC.Frame.Net.Base.ResouceManage.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Builder.Web.Helper
{
    public class MongoCache : IFrameCache
    {
        //static Dictionary<string, CacheEntity> _d = new Dictionary<string, CacheEntity>();
        static object _cache_lockobj = new object();
        /// <summary>
        /// 下次清理缓存的时间
        /// </summary>
        static DateTime nextcleardatetime = DateTime.Now;
        /// <summary>
        /// 每个多少分钟清理一次缓存
        /// </summary>
        static int clearminutes = 1;
        string mongoconn = "";
        string appid = "";
        string dbname = "EFFCFrame";
        string collectionname = "cache";
        public MongoCache()
        {
            mongoconn = MyConfig.GetConfiguration("mongodb");
            appid = MyConfig.GetConfiguration("appid");
        }
        /// <summary>
        /// 新增数据，如果存在则更新,线程安全，到指定超时
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="expira"></param>
        public void Set(string key, object obj, DateTime expira)
        {
            lock (_cache_lockobj)
            {
                new CacheEntity(obj, expira, CacheExpiraType.DateTime, TimeSpan.Zero);
                //将数据缓存到mongo中
                SaveData2Mongo(key, new CacheEntity(obj, expira, CacheExpiraType.DateTime, TimeSpan.Zero));
            }
        }
        /// <summary>
        /// 新增数据，如果存在则更新,线程安全,多长时间不用则超时
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="slide"></param>
        public void Set(string key, object obj, TimeSpan slide)
        {
            lock (_cache_lockobj)
            {
                //将数据缓存到mongo中
                SaveData2Mongo(key, new CacheEntity(obj, DateTime.Now.AddTicks(slide.Ticks), CacheExpiraType.Slide, slide));
            }
        }
        /// <summary>
        /// 根据key获取数据，如果超时则返回null,线程安全
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(string key)
        {
            lock (_cache_lockobj)
            {
                var rtn = QueryDataFromMongo(key);
                AutoRemoveValue();
                return rtn;
            }
        }
        /// <summary>
        /// 超时自动清理
        /// </summary>
        private void AutoRemoveValue()
        {

            var t = Task.Run(() =>
            {
                lock (_cache_lockobj)
                {
                    //删除mongo中过期的数据
                    MongoAccess26 mongo = new MongoAccess26(mongoconn, dbname);
                    mongo.Delete(collectionname, @"{
appid:'" + appid + @"',
expira:{$lte:new Date()}
}");
                }
            });
        }

        private void SaveData2Mongo(string key, CacheEntity data)
        {
            MongoAccess26 mongo = new MongoAccess26(mongoconn, dbname);
            mongo.Delete(collectionname, @"{
appid:'" + appid + @"',
key:'" + key + @"'
}");
            var dobj = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
            dobj.appid = appid;
            dobj.key = key;
            dobj.value = ParseTo(data.Value);
            dobj.expira = data.Expira;
            dobj.expiratype = ComFunc.Enum2String<CacheExpiraType>(data.ExpiraType);
            dobj.timespan = data.Slide.Ticks;

            mongo.Insert(collectionname, dobj);
            mongo.Close();
        }

        private object QueryDataFromMongo(string key)
        {
            MongoAccess26 mongo = new MongoAccess26(mongoconn, dbname);
            var list = mongo.Query(collectionname, @"{
appid:'" + appid + @"',
key:'" + key + @"'
}");
            mongo.Close();

            object rtn = null;
            if (list.Count > 0)
            {
                var expira = list[0].GetValue("expira") == null ? DateTime.MinValue : (DateTime)list[0].GetValue("expira");
                var expiratype = ComFunc.nvl(list[0].GetValue("expiratype")) == "" ? CacheExpiraType.DateTime : ComFunc.EnumParse<CacheExpiraType>(ComFunc.nvl(list[0].GetValue("expiratype")));
                var timespan = list[0].GetValue("timespan") == null ? 0 : (Int64)list[0].GetValue("timespan");
                if (expiratype == CacheExpiraType.DateTime)
                {
                    if (expira > DateTime.Now)
                    {
                        rtn = ConvertTo(list[0].GetValue("value"));
                    }
                }
                else if (expiratype == CacheExpiraType.Slide)
                {
                    if (expira > DateTime.Now)
                    {
                        rtn = ConvertTo(list[0].GetValue("value"));
                        SaveData2Mongo(key, new CacheEntity(rtn, DateTime.Now.AddTicks(timespan), CacheExpiraType.Slide, new TimeSpan(timespan)));
                    }
                }
                
            }

            

            return rtn;
        }

        /// <summary>
        /// 移除缓存,线程安全
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            lock (_cache_lockobj)
            {
                MongoAccess26 mongo = new MongoAccess26(mongoconn, dbname);
                mongo.Delete(collectionname, @"{
appid:'" + appid + @"',
key:'" + key + @"'
}");

                mongo.Close();
            }
        }

        private object ParseTo(object obj)
        {
            if (obj is FrameDLRObject)
            {
                return "#Type:" + typeof(FrameDLRObject).Name + " " + ComFunc.Base64Code(((FrameDLRObject)obj).ToJSONString());
            }
            else if (obj is Dictionary<string, object>)
            {
                var dobj = FrameDLRObject.CreateInstance((Dictionary<string, object>)obj);
                return "#Type:" + typeof(Dictionary<string, object>).Name + " " + ComFunc.Base64Code(((FrameDLRObject)obj).ToJSONString());
            }
            else
            {
                return obj;
            }
        }

        private object ConvertTo(object obj)
        {
            if (obj is string)
            {
                var s = ComFunc.nvl(obj);
                if (s.StartsWith("#Type:" + typeof(FrameDLRObject).Name + " "))
                {
                    var content = s.Replace("#Type:" + typeof(FrameDLRObject).Name + " ", "");
                    return FrameDLRObject.CreateInstance(ComFunc.Base64DeCode(content), FrameDLRFlags.SensitiveCase);
                }
                else if (s.StartsWith("#Type:" + typeof(Dictionary<string, object>).Name + " "))
                {
                    var content = s.Replace("#Type:" + typeof(Dictionary<string, object>).Name + " ", "");
                    FrameDLRObject dobj = FrameDLRObject.CreateInstance(ComFunc.Base64DeCode(content), FrameDLRFlags.SensitiveCase);
                    return dobj.ToDictionary();
                }
                else
                {
                    return obj;
                }
            }
            else
            {
                return obj;
            }
        }
        private enum CacheExpiraType
        {
            /// <summary>
            /// 绝对超时方式
            /// </summary>
            DateTime,
            /// <summary>
            /// 过多长时间不用则超时
            /// </summary>
            Slide
        }
        private class CacheEntity
        {
            public CacheEntity(object value, DateTime expira, CacheExpiraType type, TimeSpan slide)
            {
                Value = value;
                Expira = expira;
                ExpiraType = type;
                Slide = slide;
            }
            public object Value
            {
                get;
                set;
            }
            public DateTime Expira
            {
                get;
                set;
            }

            public CacheExpiraType ExpiraType
            {
                get;
                set;
            }
            public TimeSpan Slide
            {
                get;
                set;
            }

        }
    }
}
