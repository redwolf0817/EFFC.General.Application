using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Builder.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class BindDB : Attribute
    {
        string _dbname = "";
        public BindDB(string dbname)
        {
            _dbname = dbname;
        }

        public string DBName
        {
            get
            {
                return _dbname;
            }
        }
    }
}
