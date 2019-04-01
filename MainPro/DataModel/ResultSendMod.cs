using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainPro.DataModel
{
    /// <summary>
    /// 发送的数据模型
    /// </summary>
    class ResultSendMod
    {
        /// <summary>
        /// 扫描的结果
        /// </summary>
        public class ScanResult
        {
            /// <summary>
            /// 学号
            /// </summary>
            public String Id { set; get; }
            /// <summary>
            /// 学生姓名
            /// </summary>
            public String Name { set; get; }
            /// <summary>
            /// 学生的状态
            /// </summary>
            public int Status { set; get; }
            /// <summary>
            /// 地点
            /// </summary>
            public String Location { set; get; }
            /// <summary>
            /// （如果有）合成的Gif图像名称
            /// </summary>
            public String Photo { set; get; }
            /// <summary>
            /// 查找结束的时间
            /// </summary>
            public String ScanTime { set; get; }

            public bool isScaned { set; get; }

            public ScanResult()
            {
                Location = "ClassRoom";
                Photo = "gif";
                isScaned = false;
                Status = 0;
            }
        }
    }
}
