using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WcfService1
{
    // 注意: 如果更改此处的接口名称 "IService1"，也必须更新 Web.config 中对 "IService1" 的引用。
    [ServiceContract]
    public interface IService1
    {

        [OperationContract]
        string GetData(int value);

        // 任务: 在此处添加服务操作
    }
}
