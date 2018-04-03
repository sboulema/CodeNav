Public Class Class1

    Public Sub New(ByVal Value As String)
        mstrLine = Value
    End Sub

    Function Area(ByVal radius As Double) As Double
        Return Math.PI * Math.Pow(radius, 2)
    End Function

    Friend Sub SubInsteadOfFunction(ByVal diam As Integer)

    End Sub

End Class