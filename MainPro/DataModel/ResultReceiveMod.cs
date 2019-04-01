using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainPro.DataModel
{
    /// <summary>
    /// 收到的数据模型
    /// </summary>
    class ResultReceiveMod
    {
        /// <summary>
        /// 收到的注册信息
        /// </summary>
        public class Regist
        {
            /// <summary>
            /// 注册的学号
            /// </summary>
            public String Id { set; get; }
            public String Name { set; get; }
            /// <summary>
            /// 图片的后缀名
            /// </summary>
            public String Extension { set; get; }
        }

        /// <summary>
        /// 收到的扫描请求
        /// </summary>
        public class ScanApplication
        {
            /// <summary>
            /// 课程ID
            /// </summary>
            public String CourseId { set; get; }
            public String CourseName { set; get; }
            /// <summary>
            /// 班级ID
            /// </summary>
            public String ClassId { set; get; }
            public String ClassName { set; get; }
            /// <summary>
            /// 教师ID
            /// </summary>
            public String TeacherId { set; get; }
            public String TeacherName { set; get; }
            /// <summary>
            /// 学院
            /// </summary>
            public String College { set; get; }
            /// <summary>
            /// 要求开始扫描的时间（start -- end 时间段内的信息）
            /// </summary>
            public DateTime StartTime { set; get; }
            /// <summary>
            /// 要求的截至时间
            /// </summary>
            public DateTime StopTime { set; get; }
        }
    }
}
