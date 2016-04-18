using System;
using System.Collections;
using Infecon.Common.COM;
using Infecon.Common.Utility;
using log4net;
using Quartz;
using System.Diagnostics;
namespace Infecon.CSSD.Monitor.Belimed
{
    /// <summary>
    /// 读取传感器数据
    /// </summary>
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class DataReadJob : IJob
    {
        public const string PARAM_NAME_PIPE_HANDLE = "PipeHandle";
        public const string PARAM_NAME_PIPE_NAME = "PipeName";
        
        //日志记录
        protected readonly ILog logger;

        private readonly ILog logOutput;

        /// <summary>
        /// 命名管道句柄
        /// </summary>
        private IntPtr mPipeHandle;

        
        public DataReadJob()
        {
            logger = LogManager.GetLogger(GetType());

            logOutput = LogManager.GetLogger("SensorData.Belimed");
        }

        /// <summary>
        /// 获取接收数据的实际长度
        /// </summary>
        /// <param name="numReadWritten"></param>
        /// <returns></returns>
        private int GetReceiveLength(byte[] numReadWritten)
        {
            try
            {
                //暂时只处理前2位,numReadWritten是以整型数值保存在byte[]数组中,大于256进1位
                int l1 = ParseHelper.ParseToInt(numReadWritten[0].ToString());
                int l2 = ParseHelper.ParseToInt(numReadWritten[1].ToString()) * 256;
                return l1 + l2;
            }
            catch (Exception ex)
            {
                
                throw new Exception("GetReceiveLength:" + ex.Message);
            }
        }

        #region IJob 成员

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                logger.Info("DataReadJob 任务开始运行");
               
                JobDataMap dataMap = context.MergedJobDataMap;

                mPipeHandle = (IntPtr)dataMap.Get(PARAM_NAME_PIPE_HANDLE);//"PipeHandle"
                string pipeName = dataMap.GetString(PARAM_NAME_PIPE_NAME);//"PipeName"
                ArrayList readedBuf = (ArrayList)dataMap.Get(Consts.C_READED_BUFFER_KEY);//"ReadedBuffer"

                byte[] buf = new byte[512];
                byte[] numReadWritten = new byte[4];
                int readedCount = 0;

                if (NamedPipeHelper.ReadFile(mPipeHandle, buf, (uint)buf.Length, numReadWritten, 0))
                {
                    readedCount = GetReceiveLength(numReadWritten);
                    if (readedCount > 0)
                    {
                        logOutput.Info(ToolHelper.ByteArrayToHexString(buf));

                        lock (readedBuf.SyncRoot)
                        {
                            readedBuf.Add(buf);
                        }
                    }
                }
                else
                {
                    NamedPipeHelper.CloseNamedPipe(mPipeHandle);
                    mPipeHandle = NamedPipeHelper.OpenNamedPipe(pipeName);
                    if (mPipeHandle.ToInt32() > -1)
                    {
                        dataMap.Put(PARAM_NAME_PIPE_HANDLE, mPipeHandle);
                        logger.Warn("成功重新打开命名管道连接。");
                        

                        lock (readedBuf.SyncRoot)
                        {
                            if (readedBuf.Count > 0)
                            {
                                logger.WarnFormat("重新打开命名管道连接，舍弃已接收的数据：[{0}]", ToolHelper.ByteArrayToHexString((byte[])readedBuf[0]));
                                 
                                readedBuf.Clear();
                            }
                        }
                    }
                    else
                    {
                        logger.Error("重新打开命名管道连接失败。");
                    
                    }
                }

                logger.Info("DataReadJob 任务运行结束");
                
            }
            catch (Exception ex)
            {
                  
                logger.Error("DataReadJob 运行异常", ex);
            }
        }

        #endregion
    }
}
