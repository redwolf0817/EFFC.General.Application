using Builder.Web.Logic;
using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data;
using EFFC.Frame.Net.Base.Data.Base;
using EFFC.Frame.Net.Base.ResouceManage.Files;
using EFFC.Frame.Net.Business.Engine;
using EFFC.Frame.Net.Business.Logic;
using EFFC.Frame.Net.Data.LogicData;
using EFFC.Frame.Net.Global;
using Noesis.Javascript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Web.Business.Dev
{
    public class HostDevLogic : DevLogic
    {
        static string CodeDir = "";
        protected override Func<LogicData, object> GetFunction(string actionName)
        {
            switch (actionName.ToLower())
            {
                case "compile":
                    return DoCompile;
                case "allfiles":
                    return AllFiles;
                case "save":
                    return DoSave;
                case "getcode":
                    return GetCode;
                case "help":
                    return GetHelper;
                case "del":
                    return DoDel;
                case "compileall":
                    return DoCompileAll;
                case "publish":
                    return DoPublish;
                case "uploadresource":
                    return DoUploadResource;
                case "uptemplate":
                    return DoUploadTemplate;
                case "newhost":
                    return CreateNewHostFile;
                case "debug":
                    return DoDebug;
                case "listvl":
                    return ListViewLogic;
                case "listre":
                    return ListResouces;
                case "getre":
                    return GetResources;
                case "delre":
                    return DelResources;
                case "savere":
                    return SaveResource;
                default:
                    return null;
            }

        }

        private object SaveResource(LogicData arg)
        {
            var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:true,
msg:'保存成功'
}");

            var path = ComFunc.nvl(arg["repath"]);
            var content = ComFunc.nvlNotrim(arg["content"]);
            if (path == "")
            {
                rtn.issuccess = false;
                rtn.msg = "缺少资源路径";
            }
            else
            {
                path = ServerInfo.ServerRootPath + path;
                if (!File.Exists(path))
                {
                    rtn.issuccess = false;
                    rtn.msg = "资源不存在";
                }
                else
                {
                    var filetype = Path.GetExtension(path).Replace(".", "");
                    File.WriteAllText(path, ComFunc.Base64DeCode(content.Replace(" ", "+")), Encoding.UTF8);
                }
            }

            return rtn;
        }

        private object DelResources(LogicData arg)
        {
            var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:true,
msg:'删除成功'
}");
            var files = ComFunc.nvl(arg["deletedlist"]);
            if (files != "")
            {
                var arr = files.Split(',');
                foreach (var s in arr)
                {
                    if ("images,scripts,css" != s.Replace("/", "").Replace("\\", ""))
                    {
                        File.Delete(ServerInfo.ServerRootPath + s);
                    }
                }
            }

            return rtn;
        }

        private object GetResources(LogicData arg)
        {
            var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:true,
msg:''
}");
            var path = ComFunc.nvl(arg["repath"]);
            if (path == "")
            {
                rtn.issuccess = false;
                rtn.msg = "缺少资源路径";
            }
            else
            {
                path = ServerInfo.ServerRootPath + path;
                if (!File.Exists(path))
                {
                    rtn.issuccess = false;
                    rtn.msg = "资源不存在";
                }
                else
                {
                    string content = null;
                    var filetype = Path.GetExtension(path).Replace(".", "");
                    if ("js,map,css,txt,html".Contains(filetype) || filetype == "")
                    {
                        content = ComFunc.Base64Code(File.ReadAllText(path, Encoding.UTF8));
                    }
                    else
                    {
                        content = "data:" + ResponseHeader_ContentType.Map(filetype) + ";base64," + ComFunc.StreamToBase64String(new MemoryStream(File.ReadAllBytes(path)));
                    }

                    rtn.file = FrameDLRObject.CreateInstanceFromat(@"{
name:{0},
type:{1},
content:{2}
}", Path.GetFileNameWithoutExtension(path), filetype, content);
                }
            }
            return rtn;
        }

        private object ListResouces(LogicData arg)
        {
            var rtn = FrameDLRObject.CreateInstance();
            var path = ServerInfo.ServerRootPath;

            //image
            var imgpath = path + "\\images\\";

            rtn.image = FrameDLRObject.CreateInstance();
            rtn.image.text = "Image";
            LoopBuildTree(imgpath, rtn.image, ".bmp|.jpg|.png|.gif|.ico", "image");
            //css
            var csspath = path + "\\css\\";

            rtn.css = FrameDLRObject.CreateInstance();
            rtn.css.text = "Css";
            LoopBuildTree(csspath, rtn.css, ".css|.map|.bmp|.jpg|.png|.gif|.ico", "css");
            //scripts
            var scriptspath = path + "\\scripts\\";

            rtn.script = FrameDLRObject.CreateInstance();
            rtn.script.text = "Script";
            LoopBuildTree(scriptspath, rtn.script, ".js|.map|.bmp|.jpg|.png|.gif|.ico", "script");
            //html
            var htmlpath = path + "\\HTML\\";

            rtn.html = FrameDLRObject.CreateInstance();
            rtn.html.text = "HTML";
            LoopBuildTree(htmlpath, rtn.html, ".html|.htm", "html");

            return rtn;
        }

        private void LoopBuildTree(string path, dynamic parent, string filterfile, string type)
        {
            if (!Directory.Exists(path))
            {
                parent.nodes = null;
                return;
            }



            var files = Directory.GetFiles(path);
            var list = new List<object>();
            foreach (var f in files)
            {
                if (string.IsNullOrEmpty(filterfile) || filterfile.Contains(Path.GetExtension(f)))
                {
                    var item = FrameDLRObject.CreateInstance();
                    item.text = Path.GetFileName(f);
                    item.relativepath = f.Replace(ServerInfo.ServerRootPath, "");
                    item.type = type;
                    list.Add(item);
                }
            }

            var dirs = Directory.GetDirectories(path);

            foreach (var d in dirs)
            {
                var item = FrameDLRObject.CreateInstance();
                item.text = Path.GetFileName(d);
                LoopBuildTree(d, item, filterfile, type);
                list.Add(item);
            }

            parent.nodes = list;
        }

        private object ListViewLogic(LogicData arg)
        {
            var rtn = FrameDLRObject.CreateInstance();
            var path = GlobalCommon.HostCommon.RootPath;

            rtn.views = FrameDLRObject.CreateInstance();
            rtn.views.text = "Views";
            var viewpath = path + HostJsConstants.VIEW_PATH;
            if (Directory.Exists(viewpath))
            {
                var files = Directory.GetFiles(viewpath);
                var filelist = new List<object>();
                foreach (var f in files)
                {
                    var item = FrameDLRObject.CreateInstance();
                    item.text = Path.GetFileName(f);
                    item.relativepath = Path.GetFileName(f);
                    item.type = "view";
                    filelist.Add(item);
                }
                rtn.views.nodes = filelist;
            }
            var logicpath = path + HostJsConstants.LOGIC_PATH;
            rtn.logics = FrameDLRObject.CreateInstance();
            rtn.logics.text = "Logics";
            if (Directory.Exists(logicpath))
            {
                var files = Directory.GetFiles(logicpath);
                var filelist = new List<object>();
                foreach (var f in files)
                {
                    var item = FrameDLRObject.CreateInstance();
                    item.text = Path.GetFileName(f);
                    item.relativepath = Path.GetFileName(f);
                    item.type = "logic";
                    filelist.Add(item);

                }
                rtn.logics.nodes = filelist;
            }

            return rtn;
        }

        private object DoDebug(LogicData arg)
        {
            var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:true,
msg:''
}");

            if (!IsWebSocket) return rtn;

            var re = WS.Recieve();


            return rtn;
        }

        private object CreateNewHostFile(LogicData arg)
        {
            var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:true,
msg:''
}");
            CacheHelper.SetCache("mykey", rtn);
            var filetype = ComFunc.nvl(arg["filetype"]);
            if (filetype == "")
            {
                return FrameDLRObject.CreateInstance(@"{
issuccess:false,
msg:'要选择文件类型'
}");
            }

            var path = GlobalCommon.HostCommon.RootPath + "/Template/";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (filetype == "view")
            {
                path = path + "t_view.html";

            }
            else if (filetype == "logic")
            {
                path = path + "t_logic.js";
            }

            if (File.Exists(path))
            {
                var content = File.ReadAllText(path, Encoding.UTF8);
                rtn.content = ComFunc.UrlEncode(content);
            }
            else
            {
                rtn.content = "";
            }

            return rtn;
        }

        private object DoUploadTemplate(LogicData arg)
        {
            var filetype = ComFunc.nvl(arg["filetype"]);
            var file = arg["file"];
            if (filetype == "" || file == null)
            {
                return FrameDLRObject.CreateInstance(@"{
issuccess:false,
msg:'缺少必要的参数'
}");
            }

            var path = GlobalCommon.HostCommon.RootPath + "/Template/";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }


            if (file is FrameUploadFile)
            {
                var fuf = (FrameUploadFile)file;
                if (filetype == "view")
                {
                    if (File.Exists(path + "t_view.html"))
                    {
                        File.Delete(path + "t_view.html");
                    }
                    fuf.SaveAs(path + "t_view.html", Encoding.UTF8);
                }
                else if (filetype == "logic")
                {
                    if (File.Exists(path + "t_logic.js"))
                    {
                        File.Delete(path + "t_logic.js");
                    }
                    fuf.SaveAs(path + "t_logic.js", Encoding.UTF8);
                }
            }

            return FrameDLRObject.CreateInstance(@"{
issuccess:true,
msg:'上传成功'
}");
        }

        private object DoCompile(LogicData arg)
        {
            var type = ComFunc.nvl(arg["filetype"]);
            if (type == "view")
            {
                return DoCompileView(arg);
            }
            else if (type == "logic")
            {
                return DoCompileLogic(arg);
            }
            else
            {
                return FrameDLRObject.CreateInstance(@"{
issuccess:false,
msg:'无法识别的文件类型'
}");
            }
        }

        private object DoUploadResource(LogicData arg)
        {
            var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:true,
msg:'上传成功'
}");
            var path = ComFunc.nvl(arg["path"]);
            if (path == "")
            {
                rtn.issuccess = false;
                rtn.msg = "请确定路径和上传文件正确！";
                return rtn;
            }
            var filecount = ComFunc.nvl(arg["filecount"]);
            var icount = IntStd.IsInt(filecount) ? IntStd.ParseStd(filecount).Value : 0;
            if (icount <= 0)
            {
                rtn.issuccess = false;
                rtn.msg = "请选择要上传的文件！";
                return rtn;
            }

            path = path.Replace("~", ServerInfo.ServerRootPath).Replace("%web%", ServerInfo.ServerRootPath).Replace("%host%", GlobalCommon.HostCommon.RootPath);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var isuccess = 0;
            var ifail = 0;
            for (int i = 0; i < icount; i++)
            {
                var myfile = arg["myfile" + i];
                if (myfile != null)
                {
                    var fuf = (FrameUploadFile)myfile;
                    var ext = Path.GetExtension(fuf.FileName).Replace(".", "");

                    if (ext == "zip")
                    {
                        var zip = CreateResource<CompressFile>();
                        zip.UnZipFile(fuf.InputStream, path);
                    }
                    else
                    {
                        fuf.SaveAs(path + fuf.FileName);
                    }

                    isuccess++;
                }
                else
                {
                    ifail++;
                }
            }

            rtn.msg = string.Format("{0}份上传成功,{1}上传失败！", isuccess, ifail);

            return rtn;
        }

        private object DoCompileAll(LogicData arg)
        {
            var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:true,
msg:''
}");
            var projectname = ComFunc.nvl(arg["projectname"]);
            var path = GlobalCommon.HostCommon.RootPath + HostJsConstants.LOGIC_PATH;
            if (IsWebSocket)
            {
                WS.Send("开始执行Logic编译");
            }
            if (Directory.Exists(path))
            {
                var fs = Directory.GetFiles(path, "*.js", SearchOption.AllDirectories);
                var hle = FrameHostJsLogic.GetCurrentLogicEngine();
                hle.CurrentContext.RootPath = GlobalCommon.HostCommon.RootPath;
                hle.CurrentContext.RunTimeLibPath = GlobalCommon.HostCommon.RootPath + "/" + HostJsConstants.COMPILED_ROOT_PATH;
                hle.CurrentContext.CommonLibPath = GlobalCommon.HostCommon.RootPath;
                if (IsWebSocket)
                {
                    WS.Send("删除所有已编译的Logic文件");
                }
                hle.DeleteAllCompiledFile();
                if (IsWebSocket)
                {
                    WS.Send("开始执行编译操作，共计" + fs.Length + "份文件");
                }
                foreach (var f in fs)
                {
                    var text = File.ReadAllText(f, Encoding.UTF8);
                    hle.Compile(Path.GetFileNameWithoutExtension(f), text, true);
                    if (IsWebSocket)
                    {
                        WS.Send(string.Format("编译{0}文件完成", Path.GetFileNameWithoutExtension(f)));
                    }
                }
            }
            if (IsWebSocket)
            {
                WS.Send("结束Logic编译");
            }

            path = GlobalCommon.HostCommon.RootPath + HostJsConstants.VIEW_PATH;
            if (IsWebSocket)
            {
                WS.Send("开始执行View编译");
            }
            if (Directory.Exists(path))
            {
                var fs = Directory.GetFiles(path, "*.html", SearchOption.AllDirectories);
                var hve = CurrentHostViewEngine;
                hve.CurrentContext.RootPath = GlobalCommon.HostCommon.RootPath;
                hve.CurrentContext.RunTimeLibPath = GlobalCommon.HostCommon.RootPath + "/" + HostJsConstants.COMPILED_ROOT_PATH;
                hve.CurrentContext.CommonLibPath = GlobalCommon.HostCommon.RootPath;
                if (IsWebSocket)
                {
                    WS.Send("删除所有已编译的View文件");
                }
                hve.DeleteAllCompiledFile();
                if (IsWebSocket)
                {
                    WS.Send("开始执行编译操作，共计" + fs.Length + "份文件");
                }
                foreach (var f in fs)
                {
                    var text = File.ReadAllText(f, Encoding.UTF8);
                    hve.Compile(Path.GetFileNameWithoutExtension(f), text, true);
                    if (IsWebSocket)
                    {
                        WS.Send(string.Format("编译{0}文件完成", Path.GetFileNameWithoutExtension(f)));
                    }
                }
            }
            if (IsWebSocket)
            {
                WS.Send("结束View编译");
            }

            return rtn;
        }

        private object DoPublish(LogicData arg)
        {
            var rtn = DoCompileAll(arg);
            var path = GlobalCommon.HostCommon.RootPath + "/Publish/";
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                Directory.CreateDirectory(path);
            }

            ComFunc.CopyDirectory(GlobalCommon.HostCommon.RootPath + "/" + HostJsConstants.COMPILED_ROOT_PATH, path + HostJsConstants.VIEW_PATH, ".html");
            ComFunc.CopyDirectory(GlobalCommon.HostCommon.RootPath + HostJsConstants.VIEW_PATH, path + HostJsConstants.VIEW_PATH, ".html");
            ComFunc.CopyDirectory(GlobalCommon.HostCommon.RootPath + HostJsConstants.LOGIC_PATH, path + HostJsConstants.LOGIC_PATH, ".js");
            //拷贝公共js文件
            var fs = Directory.GetFiles(GlobalCommon.HostCommon.RootPath);
            foreach (var f in fs)
            {
                if (Path.GetExtension(f) == ".js")
                {
                    File.Copy(f, path + Path.GetFileName(f));
                }
            }
            var zip = CreateResource<CompressFile>();
            zip.ZipParameter = new ZipParameter();
            zip.ZipParameter.Zip_Name = GlobalCommon.HostCommon.RootPath + "/" + "publish.zip";
            zip.ZipParameter.DirectoryName = path;
            var s = zip.CompressReturnStream();
            s.Close();
            var zipstream = new FileStream(zip.ZipParameter.Zip_Name, FileMode.Open);
            var ms = new MemoryStream();
            zipstream.CopyTo(ms);
            zipstream.Flush();
            zipstream.Close();
            WS.Send(ms);
            return rtn;
        }

        private object DoDel(LogicData arg)
        {
            var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:true,
msg:''
}");

            string filename = ComFunc.nvl(arg["filename"]);
            string jstype = ComFunc.nvl(arg["jstype"]);
            if (filename == "")
            {
                rtn.msg = "请输入文件名称";
                rtn.issuccess = false;
                return rtn;
            }

            var filepath = "";
            var path = "";
            if (jstype == "view")
            {
                path = GlobalCommon.HostCommon.RootPath + HostJsConstants.VIEW_PATH;
                filepath = path + filename + ".html";
            }
            else
            {
                path = GlobalCommon.HostCommon.RootPath + HostJsConstants.LOGIC_PATH;
                filepath = path + filename + ".js";
            }
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }
            else
            {
                rtn.msg = "文件不存在";
                rtn.issuccess = false;
            }

            return rtn;
        }

        private object GetHelper(LogicData arg)
        {
            var jstype = ComFunc.nvl(arg["jstype"]);
            var type = ComFunc.nvl(arg["type"]);
            if (jstype == "view")
            {
                var hve = CurrentHostViewEngine;
                if (type == "serverobj")
                {
                    return hve.ServerReserverObjectKey;
                }
                else
                {
                    return hve.ReserverTags;
                }
            }
            else
            {
                var hle = FrameHostJsLogic.GetCurrentLogicEngine();
                if (type == "serverobj")
                {
                    return hle.ServerReserverObjectKey;
                }
                else
                {
                    return hle.ReserverTags;
                }
            }
        }

        private object GetCode(LogicData arg)
        {
            var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:true,
msg:''
}");
            var file = ComFunc.nvl(arg["file"]);
            var jstype = ComFunc.nvl(arg["filetype"]);
            var path = "";
            if (jstype == "view")
            {
                path = GlobalCommon.HostCommon.RootPath + HostJsConstants.VIEW_PATH + file;
            }
            else if (jstype == "logic")
            {
                path = GlobalCommon.HostCommon.RootPath + HostJsConstants.LOGIC_PATH + file;
            }

            if (File.Exists(path))
            {
                var content = File.ReadAllText(path, Encoding.UTF8);
                rtn.content = ComFunc.UrlEncode(content);
            }
            else
            {
                rtn.issuccess = false;
                rtn.msg = "文件不存在";
            }

            return rtn;
        }

        private object DoSave(LogicData arg)
        {
            var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:true,
msg:''
}");
            string filename = ComFunc.nvl(arg["filename"]);
            string code = ComFunc.nvl(arg["code"]);
            string jstype = ComFunc.nvl(arg["jstype"]);
            string relativepath = ComFunc.nvl(arg["filepath"]);
            if (filename == "")
            {
                rtn.msg = "请输入文件名称";
                rtn.issuccess = false;
                return rtn;
            }
            code = ComFunc.UrlDecode(code);
            var filepath = "";
            var path = "";
            if (jstype == "view")
            {
                path = GlobalCommon.HostCommon.RootPath + HostJsConstants.VIEW_PATH;
                filepath = path + filename + ".html";
            }
            else if (jstype == "logic")
            {
                path = GlobalCommon.HostCommon.RootPath + HostJsConstants.LOGIC_PATH;
                filepath = path + filename + ".js";
            }


            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            File.WriteAllText(filepath, code, Encoding.UTF8);

            rtn.issuccess = true;


            return rtn;

        }


        private object AllFiles(LogicData arg)
        {
            var filetype = ComFunc.nvl(arg["filetype"]);
            var rtn = FrameDLRObject.CreateInstance();
            var path = GlobalCommon.HostCommon.RootPath;
            if (filetype == "view")
            {
                path += HostJsConstants.VIEW_PATH;
            }
            else if (filetype == "logic")
            {
                path += HostJsConstants.LOGIC_PATH;
            }
            else
            {
                rtn.issuccess = false;
                rtn.msg = "无法识别的文件类型";
                return rtn;
            }

            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path);
                var filelist = new List<string>();
                foreach (var f in files)
                {
                    filelist.Add(Path.GetFileName(f));

                }
                rtn.files = filelist.ToArray();
            }
            return rtn;
        }

        private object DoCompileView(LogicData arg)
        {
            var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:true,
msg:''
}");
            var file = ComFunc.nvl(arg["file"]);
            var path = GlobalCommon.HostCommon.RootPath + HostJsConstants.VIEW_PATH + file;
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path, Encoding.UTF8);
                var hve = CurrentHostViewEngine;
                var filename = Path.GetFileNameWithoutExtension(path);
                hve.CurrentContext.RootPath = GlobalCommon.HostCommon.RootPath;
                hve.CurrentContext.CommonLibPath = GlobalCommon.HostCommon.RootPath;
                hve.CurrentContext.RunTimeLibPath = GlobalCommon.HostCommon.RootPath + "/" + HostJsConstants.COMPILED_ROOT_PATH;
                var js = hve.Compile(filename, content, true);
            }
            else
            {
                rtn.issuccess = false;
                rtn.msg = "文件不存在";
            }

            return rtn;
        }

        private object DoCompileLogic(LogicData arg)
        {
            var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:true,
msg:''
}");
            var file = ComFunc.nvl(arg["file"]);
            var path = GlobalCommon.HostCommon.RootPath + HostJsConstants.LOGIC_PATH + file;
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path, Encoding.UTF8);
                var hle = FrameHostJsLogic.GetCurrentLogicEngine();
                hle.CurrentContext.RootPath = GlobalCommon.HostCommon.RootPath;
                hle.CurrentContext.CommonLibPath = GlobalCommon.HostCommon.RootPath;
                hle.CurrentContext.RunTimeLibPath = GlobalCommon.HostCommon.RootPath + "/" + HostJsConstants.COMPILED_ROOT_PATH;
                var filename = Path.GetFileNameWithoutExtension(path);
                var d = hle.Compile(filename, content, true);

                StringBuilder sbstr = new StringBuilder();
                sbstr.AppendLine("执行代码......");
                bool issuccess = true;
                foreach (var item in d)
                {
                    sbstr.AppendLine("执行代码:" + item.Key);
                    try
                    {
                        //每个js文件都是独立的上下文结构
                        var context = FrameHostJsLogic.GetCurrentLogicEngine().CurrentContext;
                        context.RootPath = GlobalCommon.HostCommon.RootPath;
                        context.CommonLibPath = GlobalCommon.HostCommon.RootPath;
                        context.RunTimeLibPath = GlobalCommon.HostCommon.RootPath + "/" + HostJsConstants.COMPILED_ROOT_PATH;
                        HostLogicEngine.InitContext(context, this);

                        //执行的根目录环境
                        var obj = hle.RunJs(item.Value, context);

                        sbstr.AppendLine("执行成功！");
                    }
                    catch (JavascriptException ex)
                    {
                        issuccess = false;
                        sbstr.AppendLine("执行代码:" + item.Key + "失败！");
                        sbstr.AppendLine("错误信息:" + ex.Message);
                        sbstr.AppendLine("出错代码行数:" + ex.Line);
                        sbstr.AppendLine("出错代码列数:" + ex.StartColumn + "~" + ex.EndColumn);
                        sbstr.AppendLine("出错代码位置:" + ex.V8SourceLine);
                        string[] sarr = item.Value.Split('\n');
                        if (ex.Line < sarr.Length)
                        {
                            sbstr.AppendLine("出错源代码:" + sarr[ex.Line]);
                        }

                        CallContext_ResourceManage.Release(CallContext_CurrentToken);
                    }
                    sbstr.AppendLine("执行代码:" + item.Key + "完成！");
                }
                sbstr.AppendLine("执行代码完成");
                sbstr.AppendLine("控制台输出信息:" + hle.GetConsoleMsg());
                rtn.issuccess = issuccess;
                rtn.msg = sbstr.ToString().Replace("\"", @"'");
            }
            else
            {
                rtn.issuccess = false;
                rtn.msg = "文件不存在";
            }
            //防止脚本在运行时对本logic的contenttype做修改
            SetContentType(GoResponseDataType.Json);
            return rtn;
        }

        public override string Name
        {
            get { return "hostdev"; }
        }
    }
}
