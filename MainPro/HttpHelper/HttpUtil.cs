using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MainPro.HttpHelper
{
    class HttpUtil
    {
        /// <summary>
        /// post请求上传数据
        /// </summary>
        /// <param name="url">url地址</param>
        /// <param name="datas">json数据</param>
        /// <returns></returns>
        public static bool postData(String url, String datas)
        {
            if (url.StartsWith("https"))
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            HttpContent httpContent = new StringContent(datas);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", @"Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            httpClient.DefaultRequestHeaders.Add("Accept", @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            try
            {                
                HttpResponseMessage response = httpClient.PostAsync(url, httpContent).Result;                
                return (response.IsSuccessStatusCode ? true : false);
            }
            catch (Exception)
            {
                
                throw;
            }
            finally
            {
                httpContent.Dispose();
                httpClient.Dispose();                
            }
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="url">目标Url</param>
        /// <param name="path">本地文件</param>
        /// <returns></returns>
        public static bool postFile(String url, String path)
        {
            if (url.StartsWith("https"))
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            String boundary = String.Format("------WebKitFormBoundary{0}", DateTime.Now.Ticks.ToString("x"));
            
            //不手动设置header，否额报错
            MultipartFormDataContent content = new MultipartFormDataContent(boundary);
            HttpClient client = new HttpClient();

            String filename = Path.GetFileName(path);         
            try
            {
                #region Stream请求
                byte[] buf = System.IO.File.ReadAllBytes(@path);
                content.Add(new ByteArrayContent(buf, 0, buf.Length), "file", filename);
                #endregion

                var result = client.PostAsync(url, content).Result;
                if (result.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                
                throw;
            }
            finally
            {
                client.Dispose();
                content.Dispose();
            }
           
            return false;
    }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool downFile(String url, String path)
        {
            long startPosition = 0; // 上次下载的文件起始位置
            
            if (url.StartsWith("https"))
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", @"Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            client.DefaultRequestHeaders.Add("Accept", @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            Stream sr = null;
            FileStream sw = null;
            
            try
            {
                long contentSize; //文件大大小
                contentSize = GetHttpLength(url); //获取文件大小
                if(contentSize == 0)
                {
                    Console.WriteLine("Find file Error!");
                    return false;
                }
                sr = client.GetStreamAsync(url).Result; //获取文件流
                
                if(File.Exists("path"))
                {
                    sw = File.OpenWrite(path);
                    startPosition = sw.Length;
                    if(startPosition >= contentSize)
                    {
                        Console.WriteLine("文件已存在！");                        
                        return false;
                    }
                    else
                    {
                        sw.Seek(startPosition, SeekOrigin.Current);
                    }
                }
                else
                {
                    sw = File.Open(path, FileMode.Create);
                    startPosition = 0;
                }

                byte[] buf = new byte[1024];
                int leng = 0;
                long currPostion = startPosition;

                while((leng = sr.Read(buf, 0, buf.Length)) > 0)
                {
                    currPostion += leng;
                    int percent = (int)(currPostion * 100 / contentSize);
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write("File downloading : {0}%", percent);
                    sw.Write(buf, 0, leng);
                }

                Console.WriteLine();
            }
            catch (Exception)
            {
                
                throw;
            }
            finally
            {
                if (sr != null)
                    sr.Close();
                if (sw != null)
                    sw.Close();
                client.Dispose();
            }

            return true;
        }

        /// <summary>
        /// 获取文件的大小
        /// </summary>
        /// <param name="url">文件路径</param>
        /// <returns></returns>
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
