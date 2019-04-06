# HikVision-Arcsoft
# 简陋的程序, 只适合做参考。
# 另外，需要注意的是，由于采用了海康威视摄像头的人俩抓拍功能，所以本项目只能处理已经截取到的人脸图片！！！图片默认保存在设备上。
服创大赛人脸识别部分

海康威视的摄像头 虹软的人脸识别 用到了摄像头的人脸抓拍功能

需要C#版的SDK支持：

  因为项目的需要，摄像头部分只实现了查找和下载图片，虹软的部分只用来做人脸对比和检测人脸角度。
  
  CHNetSDK.dll：https://github.com/ashortname/CHNetSDK/tree/master
  
  HAF_2_0：https://github.com/dayAndnight2018/HRFace2_0

可配合web端实现图片展示：
 
  程序会向web端返回扫描识别结果。
  
可以通过UDP实现功能调用：
  
  例如当有新用户加入时，需要向程序发送给定格式的json数据；还可通过发送指令给程序完成一次指定时间段的扫描识别。
  
  新用户注册，发送：
  
  {
    "opt":"reg",
    "id":"114",
    "name":"ZhangSan",
    "extension":"jpeg"
  }

  发起一次扫描，发送：
  
  {
    "opt":"scan",
    "CourseId":"01",
    "CourseName":"math",
    "ClassId":"01",
    "ClassName":"class1",
    "TeacherId":"00",
    "TeacherName":"teacher",
    "College":"collee",
    "StartTime":"2019-03-22 22:55:00",
    "StopTime":"2019-03-22 22:57:00"
  }
