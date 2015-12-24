Public Class DescendingComparator
    Implements IComparer(Of String)

    Public Function Compare(x As String, y As String) As Integer Implements IComparer(Of String).Compare
        Return y.CompareTo(x)
    End Function
End Class
