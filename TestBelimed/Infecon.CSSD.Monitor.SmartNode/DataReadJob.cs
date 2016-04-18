using System;
using System.Collections;
using log4net;
using Quartz;
using System.Net.Sockets;
using Infecon.Common.Utility;
using System.IO;

namespace Infecon.CSSD.Monitor.SmartNode
{
    /// <summary>
    /// 读取传感器数据
    /// </summary>
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    class DataReadJob : IJob
    {
        private const int cBufferSize = 256;


        //日志记录
        protected readonly ILog logger;

        private readonly ILog logOutput;

        private TcpListener mServer;


        
        public DataReadJob()
        {
            logger = LogManager.GetLogger(GetType());

            logOutput = LogManager.GetLogger("SensorData.SmartNode");
        }

        #region IJob 成员

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                logger.Info("DataReadJob 任务开始运行");

                JobDataMap dataMap = context.MergedJobDataMap;

                mServer = (TcpListener)dataMap.Get(Consts.C_TCP_LISTENER_KEY);
                ArrayList readedBuf = (ArrayList)dataMap.Get(Consts.C_READED_BUFFER_KEY);

                TcpClient client = mServer.AcceptTcpClient();
                NetworkStream clientStream = client.GetStream();

                byte[] buffer = new byte[cBufferSize];

                int readBytes = 0;

                try
                {
                    readBytes = clientStream.Read(buffer, 0, buffer.Length);
                }
                catch (IOException ex)
                {
                    logger.Error(ex);

                    mServer.Stop();
                    mServer.Start();

                    if (readedBuf.Count > 0)
                    {
                        lock (readedBuf.SyncRoot)
                        {
                            logger.WarnFormat("重新打开侦听器，舍弃已接收的数据：[{0}]", ToolHelper.ByteArrayToHexString((byte[])readedBuf[0]));
                            readedBuf.Clear();
                        }
                    }

                    logger.Error("重新打开侦听器。");
                }
                catch (ObjectDisposedException ex)
                {
                    logger.Error(ex);
                }

                if (readBytes > 0)
                {
                    Array.Resize<byte>(ref buffer, readBytes);

                    logOutput.Info(ToolHelper.ByteArrayToHexString(buffer));


                    lock (readedBuf.SyncRoot)
                    {
                        readedBuf.Add(buffer);
                    }
                }


                clientStream.Close();
                client.Close();

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
