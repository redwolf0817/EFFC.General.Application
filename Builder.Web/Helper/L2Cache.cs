using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data;
using EFFC.Frame.Net.Base.Data.Base;
using EFFC.Frame.Net.Base.Interfaces.System;
using EFFC.Frame.Net.Base.ResouceManage.DB;
using EFFC.Frame.Net.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Builder.Web.Helper
{
    public class L2Cache : IFrameCache
    {
        static Dictionary<string, CacheEntity> _d = new Dictionary<string, CacheEntity>();
        static object _cache_lockobj = new object();
        /// <summary>
        /// 下次清理缓存的时间
        /// </summary>
        static DateTime nextcleardatetime = DateTime.Now;
        /// <summary>
        /// 每个多少分钟同步一次缓存
        /// </summary>
        static int clearminutes = 1;
        string mongoconn = "";
        string appid = "";
        string dbname = "EFFCFrame";
        string collectionname = "cache";
        public L2Cache()
        {
            mongoconn = MyConfig.GetConfiguration("mongodb");
            appid = MyConfig.GetConfiguration("appid");
            MongoAccess26 mongo = new MongoAccess26(mongoconn, dbname);
            var list = mongo.Query(collectionname, @"{
appid:'" + appid + @"'
}");
            foreach (dynamic item in list)
            {
                var entity = new CacheEntity(ConvertTo(item.value), item.expira, ComFunc.EnumParse<CacheExpiraType>(item.expiratype), new TimeSpan(item.timespan));
                _d.Add(item.key, entity);
            }
            //启动一个定时处理器
            Task t = Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(clearminutes * 60 * 1000);

                    lock (_cache_lockobj)
                    {
                        try
                        {
                            //删除mongo中过期的数据
                            MongoAccess26 mongoasyn = new MongoAccess26(mongoconn, dbname);
                            mongo.Delete(collectionname, @"{
appid:'" + appid + @"',
expira:{$lte:new Date()}
}");
                            //mongoasyn.Close();

                            foreach (var key in _d.Keys)
                            {
                                SaveData2Mongo(key, _d[key]);
                            }
                        }
                        catch (Exception ex)
                        {
                            GlobalCommon.Logger.WriteLog(LoggerLevel.ERROR, "L2Cache出错:" + ex.Message + "\n" + ex.StackTrace);
                        }
                    }

                    
                }
            });
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
                if (!_d.ContainsKey(key))
                {
                    _d.Add(key, new CacheEntity(obj, expira, CacheExpiraType.DateTime, TimeSpan.Zero));
                }
                else
                {
                    _d[key].Value = obj;
                }
                //将数据缓存到mongo中
                //SaveData2Mongo(key, _d[key]);
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
                if (!_d.ContainsKey(key))
                {
                    _d.Add(key, new CacheEntity(obj, DateTime.Now.AddTicks(slide.Ticks), CacheExpiraType.Slide, slide));
                }
                else
                {
                    _d[key].Value = obj;
                }
                //将数据缓存到mongo中
                //SaveData2Mongo(key, _d[key]);
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
                if (_d.ContainsKey(key))
                {
                    var e = _d[key];
                    if (e.Expira > DateTime.Now)
                    {
                        if (e.ExpiraType == CacheExpiraType.Slide)
                        {
                            e.Expira = DateTime.Now.AddTicks(e.Slide.Ticks);
                        }
                        //AutoRemoveValue();
                        return e.Value;
                    }
                    else
                    {
                        _d.Remove(key);
                        //AutoRemoveValue();
                        return null;
                    }
                }
                else
                {
                    //AutoRemoveValue();
                    return null;
                }
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
                    if (nextcleardatetime <= DateTime.Now)
                    {
                        var strarr = new string[_d.Keys.Count];
                        _d.Keys.CopyTo(strarr, 0);
                        foreach (var key in strarr)
                        {
                            if (_d[key].Expira > DateTime.Now)
                            {
                                _d.Remove(key);
                            }
                        }

                        nextcleardatetime = nextcleardatetime.AddMinutes(clearminutes);
                    }
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
            //如果缓存过期时间超过24小时则做2级缓存放入mongo
            if (DateTime.Now.AddHours(24) <= data.Expira)
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

                _d.Remove(key);
            }
        }
        private object ParseTo(object obj)
        {
            if (obj is FrameDLRObject)
            {
                return "#Type:" + typeof(FrameDLRObject).FullName + " " + ComFunc.Base64Code(((FrameDLRObject)obj).ToJSONString());
            }
            else if (obj is Dictionary<string, object>)
            {
                FrameDLRObject dobj = FrameDLRObject.CreateInstance((Dictionary<string, object>)obj);
                return "#Type:" + typeof(Dictionary<string, object>).FullName + " " + ComFunc.Base64Code(dobj.ToJSONString());
            }
            else if (obj is Dictionary<string, FrameDLRObject>)
            {
                var ddobj = (Dictionary<string, FrameDLRObject>)obj;
                FrameDLRObject dobj = FrameDLRObject.CreateInstance(ddobj);
                return "#Type:" + typeof(Dictionary<string, FrameDLRObject>).FullName + " " + ComFunc.Base64Code(dobj.ToJSONString());
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
                if (s.StartsWith("#Type:" + typeof(FrameDLRObject).FullName + " "))
                {
                    var content = s.Replace("#Type:" + typeof(FrameDLRObject).FullName + " ", "");
                    return FrameDLRObject.CreateInstance(ComFunc.Base64DeCode(content), FrameDLRFlags.SensitiveCase);
                }
                else if (s.StartsWith("#Type:" + typeof(Dictionary<string, object>).FullName + " "))
                {
                    var content = s.Replace("#Type:" + typeof(Dictionary<string, object>).FullName + " ", "");
                    FrameDLRObject dobj = FrameDLRObject.CreateInstance(ComFunc.Base64DeCode(content), FrameDLRFlags.SensitiveCase);
                    return dobj.ToDictionary();
                }
                else if (s.StartsWith("#Type:" + typeof(Dictionary<string, FrameDLRObject>).FullName + " "))
                {
                    var content = s.Replace("#Type:" + typeof(Dictionary<string, FrameDLRObject>).FullName + " ", "");
                    FrameDLRObject dobj = FrameDLRObject.CreateInstance(ComFunc.Base64DeCode(content), FrameDLRFlags.SensitiveCase);
                    var r = new Dictionary<string, FrameDLRObject>();
                    foreach (var k in dobj.Keys)
                    {
                        r.Add(k, (FrameDLRObject)dobj.GetValue(k));
                    }
                    return r;
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
