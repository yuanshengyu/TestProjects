Imports System.ServiceModel

' 注意: 如果更改此处的类名 "ICalculator"，也必须更新 Web.config 中对 "ICalculator" 的引用。
<ServiceContract()> _
Public Interface ICalculator

    <OperationContract()> _
    Sub DoWork()

End Interface
