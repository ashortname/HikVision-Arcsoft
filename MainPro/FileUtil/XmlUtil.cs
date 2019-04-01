using MainPro.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MainPro.FileUtil
{
    class XmlUtil
    {
        //默认全局文件路径
        public static String xmlPath = "D:\\Files\\服创\\Test\\Feature\\feature.xml";


        /// <summary>
        /// 创建文件
        /// </summary>
        public static void Create(String path)
        {
            xmlPath = path;
            if (!File.Exists(@xmlPath))
            {
                //File.Create() 不会自动关闭，所以要在末尾加上Close()
                File.Create(@xmlPath).Close();

                XmlDocument doc = new XmlDocument();
                XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.AppendChild(dec);

                XmlElement root = doc.CreateElement("AllUserInfo");
                XmlElement male = doc.CreateElement("Male");
                XmlElement female = doc.CreateElement("Female");

                root.AppendChild(male);
                root.AppendChild(female);
                doc.AppendChild(root);
                doc.Save(@xmlPath);
            }
        }

        /// <summary>
        /// 添加一条记录
        /// </summary>
        /// <param name="ip">所属ip</param>
        /// <param name="nacc">账号</param>
        /// <param name="npass">密码</param>
        public static void AddOneData(LocalUserMod user, String gender)
        {
            //先判断是否存在文件
            Create(@xmlPath);

            //装载文件
            XmlDocument doc = new XmlDocument();
            doc.Load(@xmlPath);
            XmlElement root = doc.DocumentElement;
            var nodes = root.GetElementsByTagName(gender); //male
            foreach(XmlNode node in nodes)
            {
                if(SearchOrUpdate(user, gender))
                {                   
                    return;
                }
            }
            
            //新增连接、账号信息
            XmlElement newCon = doc.CreateElement("ID");
            newCon.SetAttribute("id", user.Num);

            XmlElement temp1 = doc.CreateElement("Name");
            temp1.InnerText = user.Name;
            XmlElement temp2 = doc.CreateElement("Feature");
            temp2.InnerText = user.Freature;

            newCon.AppendChild(temp1);
            newCon.AppendChild(temp2);
            nodes.Item(0).AppendChild(newCon);
            //root.AppendChild(newCon);

            doc.Save(@xmlPath);
        }

        /// <summary>
        /// 查找或者更新数据
        /// </summary>
        /// <param name="ip">所属ip</param>
        /// <param name="acc">账号</param>
        /// <param name="newpass">新密码</param>
        /// <returns></returns>
        public static bool SearchOrUpdate(LocalUserMod user, String gender)
        {
            //先判断是否存在文件
            Create(@xmlPath);

            //装载文件
            XmlDocument doc = new XmlDocument();
            doc.Load(@xmlPath);
            XmlElement root = doc.DocumentElement;
            var nodes = root.GetElementsByTagName(gender);
            foreach (XmlNode node in nodes) //male
            {
                foreach(XmlNode inode in node.ChildNodes) //ID
                {
                    if (inode.Attributes[0].Value.Equals(user.Num)) //如果该学号已经存在，更新数据
                    {
                        inode.FirstChild.InnerText = user.Name;
                        inode.LastChild.InnerText = user.Freature;
                        //保存结果
                        doc.Save(@xmlPath);
                        return true; //存在，且更新了
                    }
                }                
            }
            //查找的IP不存在
            return false;
        }

        /// <summary>
        /// 获取、填充指定连接下的所有账号信息
        /// </summary>
        /// <param name="ip">指定的连接地址</param>
        public static void ReadAllDatas(String gender, ref List<LocalUserMod> datas)
        {
            //先判断是否存在文件
            Create(@xmlPath);

            //装载文件
            XmlDocument doc = new XmlDocument();
            doc.Load(@xmlPath);
            XmlElement root = doc.DocumentElement;
            var nodes = root.GetElementsByTagName(gender);
            foreach (XmlNode node in nodes) //male
            {
                foreach (XmlNode inode in node.ChildNodes) //ID
                {
                    datas.Add(new LocalUserMod()
                    {
                        Num = inode.Attributes[0].Value,
                        Name = inode.FirstChild.InnerText,
                        Freature = inode.LastChild.InnerText
                    });
                }
            }
        }
    }
}
