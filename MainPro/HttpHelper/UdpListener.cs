using MainPro.DataModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MainPro.HttpHelper
{
    class UdpListener
    {
        #region ===地址、端口、Task===
        /// <summary>
        /// Udp监听的端口
        /// </summary>
        public static int UdpListen_Port { set; get; }

        /// <summary>
        /// 用以监听的udpclient对象实例
        /// </summary>
        private static UdpClient ReceiveClient;
        
        /// <summary>
        /// 用于监听广播消息
        /// </summary>
        public static Task task_ListenMsg = new Task(() => { });

        /// <summary>
        /// 与任务相关联的令牌
        /// </summary>
        private static CancellationTokenSource Cts;
        #endregion
        
        #region ===定义事件===
        /// <summary>
        /// 事件 - 用户注册
        /// </summary>
        public static event EventHandler<myEventArgs.RegistArgs> NewUserRegist;
        
        /// <summary>
        /// 事件 - 扫描申请
        /// </summary>
        public static event EventHandler<myEventArgs.ScanArgs> ScanApply;
        #endregion

        #region===构造函数===
        /// <summary>
        /// 默认构造函数 默认监听 45534 端口
        /// </summary>
        public UdpListener()
        {
            UdpListen_Port = 45534;
        }

        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="port"></param>
        public UdpListener(int port)
        {
            UdpListen_Port = port;
        }
        #endregion

        /// <summary>
        /// 开启监听
        /// </summary>
        public static void startUdp()
        {
            try
            {                
                if(task_ListenMsg.Status != TaskStatus.Running)
                {
                    //初始化监听实例
                    ReceiveClient = new UdpClient(new IPEndPoint(IPAddress.Any, UdpListen_Port));
                    //初始化任务token
                    Cts = new CancellationTokenSource();
                    //建立任务
                    task_ListenMsg = new Task(() => { ReceiveData(); }, Cts.Token);
                    //开始监听
                    task_ListenMsg.Start();

                    Console.WriteLine("Start listen......");
                    return;
                }
                Console.WriteLine("Udp is already start!");   
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to start listen!");
                throw;
            }
        }

        /// <summary>
        /// 关闭监听
        /// </summary>
        public static void stopUdp()
        {
            try
            {
                if(task_ListenMsg.Status == TaskStatus.Running)
                {
                    Cts.Cancel();
                    ReceiveClient.Close();
                    ReceiveClient = null;
                    Console.WriteLine("Stop listen......");
                    return;
                }
                Console.WriteLine("Udp is not opened yet!");   
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to stop listen!");
                throw;
            }
        }

        /// <summary>
        /// 用于接收数据
        /// </summary>
        private static void ReceiveData()
        {
            IPEndPoint tmp = new IPEndPoint(IPAddress.Any, 0);
            String msg = "";
            while(true)
            {
                try
                {
                    if (Cts.IsCancellationRequested)
                        break;
                    if(ReceiveClient.Available > 0)
                    {
                        byte[] buffer = ReceiveClient.Receive(ref tmp);
                        msg = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                        dynamic data = JsonConvert.DeserializeObject(msg);
                        if(data.opt == ("reg"))
                        {
                            myEventArgs.RegistArgs args = new myEventArgs.RegistArgs()
                            {
                                Regist = JsonConvert.DeserializeObject<ResultReceiveMod.Regist>(msg)
                            };
                            //触发事件
                            NewUserRegist(new UdpClient(), args);
                        }else if(data.opt == ("scan"))
                        {
                            myEventArgs.ScanArgs args = new myEventArgs.ScanArgs()
                            {
                                Scan = JsonConvert.DeserializeObject<ResultReceiveMod.ScanApplication>(msg)
                            };
                            //触发事件
                            ScanApply(new UdpClient(), args);
                        }
                        else
                        {
                            //do nothing
                        }
                    }
                }catch(JsonException ex)
                {
                    Console.WriteLine("\n[********\n\tJson parse error : {0}\n\tdata : {1}\n********]\n", ex.Message, msg);
                    continue;
                }
                catch (Exception)
                {
                    Console.WriteLine("An error occured while receiving data!");
                    stopUdp();
                    break;
                    throw;
                }
            }
        }
    }
}
