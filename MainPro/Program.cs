﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using MainPro.HttpHelper;
using MainPro.DataModel;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using Newtonsoft.Json;
using HAF_2_0; //自编译的调用ArcSoft的dll
using CHNetSDK; //自编译的调用HikVision的dll
using MainPro.FileUtil;
using MainPro.DataModel;
using System.IO;

namespace MainPro
{
    class Program
    {
        #region ===本机端口===
        /// <summary>
        /// Udp监听的端口
        /// </summary>
        private static int UdpListen_Port;
        #endregion

        #region===客户端接口URL===
        /// <summary>
        /// 下载图片的url地址
        /// </summary>
        private const String Url_DldImg = "Url to download images.";
        /// <summary>
        /// 上传数据的url地址
        /// </summary>
        private const String Url_PostData = "Url to upload datas.";
        /// <summary>
        /// 上传图片的ur地址
        /// </summary>
        private const String Url_PostImg = "Url to upload images";
        #endregion

        #region===本地保存的根目录===
        /// <summary>
        /// 用于保存注册的用户图片的根目录 eg. "D:\\Imgs"
        /// </summary>
        private const String ImgRootPath_Regist = "Your local path";

        /// <summary>
        /// 用于保存提取后的用户的特征值的根目录 eg. "D:\\featurses.xml" 实际写在 xmlUtil.cs 里面
        /// </summary>
        private const String ImgRootPath_FaceFeatures = "Your local path";

        /// <summary>
        /// 用于保存每次扫描图片的根目录
        /// </summary>
        private const String ImgRootPath_Scan = "Your local path";
        #endregion

        #region===虹软API相关===
        private const String APPID = "APPID";
        private const String APPKEY = "APPKEY";
        #endregion

        #region===海康威视相关===
        private const String IP = "IP";
        private const String Account = "ACCOUNT";
        private const String PassWord = "PASSWORD";
        private const int Port = 8000;
        #endregion

        #region===扫描相关===
        /// <summary>
        /// 所有本地用户信息
        /// </summary>
        private static List<LocalUserMod> allRegUsers = new List<LocalUserMod>();
        /// <summary>
        /// 用来返回数据的真正模型
        /// </summary>
        private static Dictionary<String, ResultSendMod.ScanResult> resultSend = new Dictionary<String, ResultSendMod.ScanResult>();
        /// <summary>
        /// 扫描定时器
        /// </summary>
        private static System.Timers.Timer ScanTimer;
        /// <summary>
        /// 扫描周期（毫秒）
        /// </summary>
        private static Int32 ScanPeriod = 30000;
        /// <summary>
        /// 相似度
        /// </summary>
        private const float Similarity = 0.8F;
        #endregion

        /// <summary>
        /// 主函数
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            ////获取输入的端口号
            if (!getInput(ref UdpListen_Port))
            {
                Console.WriteLine("Input error,quit!");
                return;
            }
            try
            {
                UdpListener.UdpListen_Port = UdpListen_Port;
                //事件绑定
                UdpListener.NewUserRegist += new EventHandler<myEventArgs.RegistArgs>(NewUserRegist);
                UdpListener.ScanApply += new EventHandler<myEventArgs.ScanArgs>(ScanApply);
                //开启监听
                UdpListener.startUdp();
                //告知监听端口
                Console.WriteLine("Start Udp at port: {0}", UdpListen_Port);
                ScanTimer = new System.Timers.Timer(ScanPeriod); //设置时间间隔为30秒
                //绑定tick时间
                ScanTimer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_TimesUp);
                ScanTimer.AutoReset = true; //每到指定时间Elapsed事件是触发一次（false），还是一直触发（true）
                //System.Threading.Thread.Sleep(1000);                
                //定时开始
                ScanTimer.Start();
                //阻塞主线程
                Task.WaitAll(UdpListener.task_ListenMsg);

                Console.WriteLine("Quit!!!!");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in main function: {0}", ex.Message);
                UdpListener.stopUdp();
                ScanTimer.Stop();
            }
        }

        /// <summary>
        /// 获取输入的端口号
        /// </summary>
        /// <param name="portNum">端口号</param>
        /// <returns></returns>
        private static bool getInput(ref int portNum)
        {
            while (true)
            {
                try
                {
                    Console.Write("Please enter a port number: ");
                    String num = Console.ReadLine();
                    if (num.ToUpper().Equals("QUIT"))
                    {
                        return false;
                    }
                    //使用默认端口号：45535
                    if (num.Equals(string.Empty))
                    {
                        portNum = 45534;
                        return true;
                    }

                    portNum = int.Parse(num);
                    return true;
                }
                catch (Exception)
                {
                    Console.WriteLine("Input error, please retry...");
                    continue;
                }
            }
        }

        #region===注册触发的一系列操作===
        /// <summary>
        /// 注册事件触发函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">变量参数</param>
        public static void NewUserRegist(object sender, myEventArgs.RegistArgs args)
        {
            /****************************
             * 保存特征值时才区分性别
             * **************************/
            //下载图片到本地
            String url = String.Format("{0}/{1}_{2}.{3}",
                Url_DldImg, args.Regist.Id, args.Regist.Name, args.Regist.Extension);
            String pathImg = String.Format("{0}\\{1}_{2}.{3}",
                ImgRootPath_Regist, args.Regist.Id, args.Regist.Name, args.Regist.Extension);
            HttpUtil.downFile(url, pathImg);
            //判断性别（假设性别判断 100% 准确）
            try
            {
                ResultCode result = EngineActivate.ActivateEngine(APPID, APPKEY);
                if (result != ResultCode.SDK已激活)
                {
                    Console.WriteLine(result.ToString());
                    return;
                }
                IntPtr hengine = EngineFactory.GetEngineInstance(EngineFactory.Image,
                    DetectionOrientPriority.ASF_OP_0_ONLY, 16); //检测角度指人脸在照片中的角度
                Bitmap img1 = new Bitmap(pathImg);

                var face = new FaceDetection(hengine, img1);
                var r = face.GetGender();
                LocalUserMod temp1 = new LocalUserMod();
                temp1.Name = args.Regist.Name;
                temp1.Num = args.Regist.Id;
                //用Base64转码
                temp1.Freature = Convert.ToBase64String(face.getFaceFeature(img1));
                //销毁图片
                img1.Dispose();
                //提取特征值保存
                switch (r)
                {
                    case "男":
                        XmlUtil.AddOneData(temp1, "Male");
                        break;
                    case "女":
                        XmlUtil.AddOneData(temp1, "Female");
                        break;
                    default:
                        Console.WriteLine("获取性别失败！用户：{0} , 学号：{1}，注册时间：{2}",
                            args.Regist.Name, args.Regist.Id, DateTime.Now.ToString());
                        return;
                }
                //提示用户注册
                Console.WriteLine("User registed: {0} at {1}", args.Regist.Name, DateTime.Now.ToString());
                //更新用户信息
                //令写一个函数
                getAllUserInfo();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Regist Error: {0}", ex.Message);
            }
            finally
            {
                EngineFactory.DisposeEngine();
            }
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        private static void getAllUserInfo()
        {
            resultSend.Clear();
            allRegUsers.Clear(); //清空
            XmlUtil.ReadAllDatas("Male", ref allRegUsers);
            XmlUtil.ReadAllDatas("Female", ref allRegUsers);
            //初始化学生状态
            foreach (LocalUserMod mod in allRegUsers)
            {
                resultSend.Add(mod.Name, new DataModel.ResultSendMod.ScanResult()
                {
                    Id = mod.Num,
                    Name = mod.Name
                });
            }
        }
        #endregion

        #region===扫描触发的一系列操作===
        /// <summary>
        /// 调用扫描的函数
        /// </summary>
        /// <param name="start">开始时间</param>
        /// <param name="stop">结束时间</param>
        public static void callScan(DateTime start, DateTime stop)
        {
            //取出所有的用户信息
            getAllUserInfo();
            try
            {
                //从设备download时段内的图片，保存到本地根目录下
                //以"starttime_stoptime"命名的文件夹下
                String savepath = String.Format("{0}\\{1}_{2}\\",
                    ImgRootPath_Scan, start.ToString("yyyyMMdd-HHmmss"), stop.ToString("yyyyMMdd-HHmmss"));
                //初始化下载类并开始下载，自动释放资源
                ImgHelper imgHelper = new ImgHelper(IP, Port, Account, PassWord, savepath);
                imgHelper.SearchAndDown(start, stop);
                //开始逐个判断：先判断性别，识别用户状态，再与本地特征值逐个进行比较，
                //明确用户身份，最后结果保存到dictonary中。
                new Task(() => { doScan(savepath); }).Start();
                Console.WriteLine("Start scan {0} to {1} ......",
                    start.ToString(), stop.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Call scan error! {0}", ex.Message);
                return;
            }
        }
        /// <summary>
        /// 定时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void Timer_TimesUp(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                //设定时间
                DateTime ts = DateTime.Now;
                DateTime te = ts.AddSeconds(30); //当前时间向后推30秒
                //唤起扫描
                callScan(ts, te);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Timer tick Error! {0}", ex.Message);
                return;
            }
        }
        /// <summary>
        /// 扫描触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">变量</param>
        public static void ScanApply(object sender, myEventArgs.ScanArgs args)
        {
            try
            {
                //唤起扫描
                callScan(args.Scan.StartTime, args.Scan.StopTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Timer tick Error! {0}", ex.Message);
                return;
            }
        }

        /// <summary>
        /// 做扫描识别
        /// </summary>
        /// <param name="path"></param>
        private static void doScan(String path)
        {
            ResultCode result = EngineActivate.ActivateEngine(APPID, APPKEY);
            if (result != ResultCode.SDK已激活)
            {
                Console.WriteLine(result.ToString());
                return;
            }
            IntPtr hengine = EngineFactory.GetEngineInstance(EngineFactory.Image,
            DetectionOrientPriority.ASF_OP_0_ONLY, 16);
            FaceDetection face = new FaceDetection(hengine);

            DirectoryInfo root = new DirectoryInfo(path);

            //获取所有下载的图片开始识别
            List<FileInfo> Filelist = root.GetFiles("*.jpeg").ToList();
            Bitmap img = null;
            foreach (FileInfo file in Filelist) //外层是文件
            {
                try
                {
                    img = new Bitmap(file.FullName);
                    foreach (LocalUserMod mod in allRegUsers) //内层是用户
                    {
                        try
                        {
                            //此人已被识别，不做识别
                            if (resultSend[mod.Name].isScaned) //如果已经查找了这个人
                                continue;
                            byte[] f1 = Convert.FromBase64String(mod.Freature);
                            byte[] f2 = face.getFaceFeature(img);
                            float sim = face.Compare(f1, f2);
                            if (sim >= Similarity)
                            {
                                //获取3D角度
                                FaceDetection.TDAResult tdr = face.GetThreeDAngle(img);
                                Console.WriteLine("Name: {0}\nThreeDAngle: {1}", mod.Name, tdr.ToString());
                                //判断角度
                                //更改状态
                                if (tdr.Pitch <= -13) //低头角度
                                {
                                    resultSend[mod.Name].Status = 1; //低头
                                }
                                else if (tdr.Yaw <= -26 || tdr.Yaw >= 26)
                                {
                                    resultSend[mod.Name].Status = 2; //左右
                                }
                                else
                                {
                                    resultSend[mod.Name].Status = 3; //认真
                                }
                                //标识已扫描
                                resultSend[mod.Name].isScaned = true;
                                //上传文件到前端
                                HttpUtil.postFile(Url_PostImg, file.FullName);
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error in scanning! {0}", ex.Message);
                            continue;
                        }
                        //未识别出
                        Console.WriteLine("Cannot distinguish : {0}", file.Name);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }
                finally
                {
                    //释放图片资源
                    if (img != null)
                        img.Dispose();
                }
            }
            try
            {
                String datas = JsonConvert.SerializeObject(resultSend);
                //post数据到前端
                HttpUtil.postData(Url_PostData, datas);
                //销毁引擎
                EngineFactory.DisposeEngine();
                Console.WriteLine("Scan over......");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }
        #endregion
    }
}