using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.ResouceManage.FTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Builder.Web.Logic
{
    public abstract partial class GoLogic
    {
        Extentions _ext = null;
        public Extentions ExtFunc
        {
            get
            {
                if (_ext == null) _ext = new Extentions(this);
                return _ext;
            }


        }
        public class Extentions
        {
            GoLogic _logic;

            public Extentions(GoLogic logic)
            {
                _logic = logic;
            }

            /// <summary>
            /// 检查是否含有敏感词
            /// </summary>
            /// <param name="path"></param>
            /// <param name="content"></param>
            /// <returns></returns>
            public bool HasSensitiveWords(string content)
            {
                bool has = true;
                var path = string.Format("{0}/Config/sensitive.txt", _logic.ServerInfo.ServerRootPath);
                if (!File.Exists(path))
                {
                    has = false;
                }
                else
                {
                    StreamReader sr = new StreamReader(path, System.Text.Encoding.UTF8);

                    //int length = 0;
                    for (int i = 0; i == 0; )
                    {
                        var sb = sr.ReadLine();
                        if (string.IsNullOrWhiteSpace(sb))
                            break;
                        if (content.ToLower().Contains(sb.ToLower()))
                        {
                            has = false;
                            break;
                        }
                    }

                    sr.Close();
                    sr.Dispose();
                }
                return has;
            }
        }
    }
}
