
Public Class StringEventArgs
    Inherits EventArgs
    Public Value As String
    Public Sub New(str As String)
        Value = str
    End Sub
End Class