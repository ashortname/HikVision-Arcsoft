using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainPro.DataModel
{
    class myEventArgs
    {
        /// <summary>
        /// 用于响应注册的事件参数
        /// </summary>
        public class RegistArgs : EventArgs
        {
            public ResultReceiveMod.Regist Regist { set; get; }
        }

        /// <summary>
        /// 用于响应扫描的事件参数
        /// </summary>
        public class ScanArgs : EventArgs
        {
            public ResultReceiveMod.ScanApplication Scan { set; get; }
        }        
    }
}
