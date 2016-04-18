' 注意: 如果更改此处的类名 "Calculator"，也必须更新 Web.config 和关联的 .svc 文件中对 "Calculator" 的引用。
Public Class Calculator
    Implements ICalculator

    Public Sub DoWork() Implements ICalculator.DoWork
    End Sub

End Class
