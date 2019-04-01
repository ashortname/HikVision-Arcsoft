using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MainPro.HttpHelper
{
    class DldImg
    {
         /// <summary>
       /// Http方式下载文件
       /// </summary>
       /// <param name="url">http地址</param>
       /// <param name="localfile">本地文件</param>
       /// <returns></returns>
        public bool Download(string url,string localfile)
        {
            bool flag = false;
            long startPosition = 0; // 上次下载的文件起始位置
            FileStream writeStream; // 写入本地文件流对象

            long remoteFileLength = GetHttpLength(url);// 取得远程文件长度
            System.Console.WriteLine(" remoteFileLength = " + remoteFileLength);
            if (remoteFileLength == 745)
            {
                System.Console.WriteLine("远程文件不存在.");
                return false;
            }
            if(remoteFileLength == 0)
            {
                Console.WriteLine("Error...");
                return false;
            }

            // 判断要下载的文件夹是否存在
            if (File.Exists(localfile))
            {
                writeStream = File.OpenWrite(localfile);             // 存在则打开要下载的文件
                startPosition = writeStream.Length;                  // 获取已经下载的长度

                if (startPosition >= remoteFileLength)
                {
                    System.Console.WriteLine("文件已下载！");
                    writeStream.Close();
                    return false;
                }
                else
                {
                    writeStream.Seek(startPosition, SeekOrigin.Current); // 本地文件写入位置定位
                }
            }
            else
            {
                writeStream = new FileStream(localfile, FileMode.Create);// 文件不保存创建一个文件
                startPosition = 0;
            }            
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)HttpWebRequest.Create(url);// 打开网络连接

                if (startPosition > 0)
                {
                    myRequest.AddRange((int)startPosition);// 设置Range值,与上面的writeStream.Seek用意相同,是为了定义远程文件读取位置
                }
                Stream readStream = myRequest.GetResponse().GetResponseStream();// 向服务器请求,获得服务器的回应数据流
                byte[] btArray = new byte[512];// 定义一个字节数据,用来向readStream读取内容和向writeStream写入内容
                
                int contentSize = readStream.Read(btArray, 0, btArray.Length);// 向远程文件读第一次
                long currPostion=startPosition;

                while (contentSize > 0)// 如果读取长度大于零则继续读
                {
                    currPostion+=contentSize;
                    int percent = (int)(currPostion*100 / remoteFileLength);
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write("File downloading : {0}%", percent);

                    writeStream.Write(btArray, 0, contentSize);// 写入本地文件
                    contentSize = readStream.Read(btArray, 0, btArray.Length);// 继续向远程文件读取
                }
                //换行
                Console.WriteLine();
                //关闭流
                writeStream.Close();
                readStream.Close();

                flag = true;        //返回true下载成功
            }
            catch (Exception)
            {
                writeStream.Close();
                flag = false;       //返回false下载失败
            }

            return flag;
        }

        // 从文件头得到远程文件的长度
        private static long GetHttpLength(string url)
        {
            long length = 0;
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);// 打开网络连接
                HttpWebResponse rsp = (HttpWebResponse)req.GetResponse();

                if (rsp.StatusCode == HttpStatusCode.OK)
                {
                    length = rsp.ContentLength;// 从文件头得到远程文件的长度
                }

                rsp.Close();
                return length;
            }
            catch (Exception e)
            {
                return length;
            }
        }
    }
}
