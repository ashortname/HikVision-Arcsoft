using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainPro.DataModel
{
    /// <summary>
    /// 本地用户信息 --- 对应保存特征值的 xml 文件
    /// </summary>
    class LocalUserMod
    {
        public String Name { set; get; }
        public String Num { set; get; }
        /// <summary>
        /// 特征值
        /// </summary>
        public String Freature { set; get; }
    }
}
