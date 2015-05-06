Imports System.Text.RegularExpressions

Public Class Form1

    Dim WithEvents DownloadM As DownloadVideo

    Dim DoVerify As Boolean = False
    Private Sub Button_Click(sender As System.Object, e As System.EventArgs) Handles ButtonGoStop.Click
        Dim input As String = inputName.Text
        If IsNothing(DownloadM) Then
            Dim Splt() As String = inputName.Text.Split("/")
            If Splt.Count > 1 Then
                If Splt(Splt.Count - 2).Length = 1 Then 'Got video ID7
                    input = "a" & Splt(Splt.Count - 1) 'Past Broadcast
                    If Splt(Splt.Count - 2) = "v" Then
                        input = "v" & Splt(Splt.Count - 1) 'New video system
                    End If
                    If Splt(Splt.Count - 2) = "c" Then 'Highlight
                        input = "c" & Splt(Splt.Count - 1)
                    End If
                    DownloadM = New DownloadVideo
                Else
                    Throw New Exception("Invalid input")
                End If
            Else
                DownloadM = New DownloadBatch
            End If
        Else
            If DownloadM.isDownloading = False Then
                DownloadM = Nothing
                Button_Click(Nothing, Nothing)
                Return
            End If
        End If

        DownloadM.VerifyOldFiles = DoVerify
        DoVerify = False

        If MergeBox.SelectedIndex = 1 Then '1 = No
            DownloadM.Merge = False
        Else
            DownloadM.Merge = True
        End If

        Select Case CopyToBox.SelectedIndex
            Case Is = 0 'Just Merge
                DownloadM.TargetFormat = Format.AsSource
            Case Is = 1 'MKV
                DownloadM.TargetFormat = Format.MKV
            Case Is = 2 'MP4
                DownloadM.TargetFormat = Format.MP4
        End Select

        If DownloadM.isDownloading = False Then
            DownloadM.StartDownload(input)
            inputName.Enabled = False
            ButtonGoStop.Text = "Stop"
            ButtonVerify.Enabled = False
            MergeBox.Enabled = False
            CopyToBox.Enabled = False
        Else
            DownloadM.StopDownload()
            ButtonGoStop.Text = "Stopping"
            ButtonGoStop.Enabled = False
        End If
    End Sub

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles ButtonVerify.Click
        'Get a list of folders and delete Download.Done in all of them
        Dim DirList As IEnumerable(Of String) = IO.Directory.EnumerateDirectories(IO.Directory.GetCurrentDirectory)
        For Each Dir As String In DirList
            If System.IO.File.Exists(Dir & "\Done.Download") Then
                System.IO.File.Delete(Dir & "\Done.Download")
            End If
        Next
        If IO.File.Exists("Log.txt") Then 'reset log
            IO.File.Delete("Log.txt")
        End If
        DoVerify = True
        Button_Click(Nothing, Nothing)
    End Sub

    Private Sub SetStatsText(ByVal text As String) Handles DownloadM.StatusUpdate
        If Stats.InvokeRequired Then
            Stats.Invoke(New Action(Of String)(AddressOf SetStatsText), text)
        Else
            Stats.Text = text
        End If
    End Sub

#Region "Progress Event Handling"
    Private Sub SetProgress2(ByVal int As Integer, ByVal max As Integer) Handles DownloadM.FileProgress
        If ProgressBar1.InvokeRequired Then
            ProgressBar1.Invoke(New Action(Of Integer, Integer)(AddressOf SetProgress2), int, max)
        Else
            If int >= 0 Then
                ProgressBar1.Value = int
                ProgressBar1.Maximum = max
            End If
        End If
    End Sub
    Private Sub SetProgress2Text(ByVal str As String) Handles DownloadM.FileProgressTextUpdate
        If PB1Label.InvokeRequired Then
            PB1Label.Invoke(New Action(Of String)(AddressOf SetProgress2Text), str)
        Else
            PB1Label.Text = str
        End If
    End Sub

    Private Sub SetVODProgress(ByVal completed As Integer, ByVal total As Integer) Handles DownloadM.VODProgress
        If VODProgress.InvokeRequired Then
            VODProgress.Invoke(New Action(Of Integer, Integer)(AddressOf SetVODProgress), completed, total)
        Else
            VODProgress.Maximum = total
            VODProgress.Value = completed
        End If
    End Sub
    Private Sub SetVODProgressText(ByVal str As String) Handles DownloadM.VODProgressTextUpdate
        If VPLabel.InvokeRequired Then
            VPLabel.Invoke(New Action(Of String)(AddressOf SetVODProgressText), str)
        Else
            VPLabel.Text = str
        End If
    End Sub

    Private Sub SetTotalProgress(ByVal completed As Integer, ByVal total As Integer) Handles DownloadM.TotalProgress
        If TotalProgress.InvokeRequired Then
            TotalProgress.Invoke(New Action(Of Integer, Integer)(AddressOf SetTotalProgress), completed, total)
        Else
            TotalProgress.Maximum = total
            TotalProgress.Value = completed
        End If
    End Sub
    Private Sub SetTotalProgressText(ByVal str As String) Handles DownloadM.TotalTextUpdate
        If TPLabel.InvokeRequired Then
            TPLabel.Invoke(New Action(Of String)(AddressOf SetTotalProgressText), str)
        Else
            TPLabel.Text = str
        End If
    End Sub
    Private Sub DownloadStoped() Handles DownloadM.DownloadDone
        EnableStuff("t")
    End Sub
#End Region

    Private Sub EnableStuff(dummy As String)
        If Stats.InvokeRequired Then
            Stats.Invoke(New Action(Of String)(AddressOf EnableStuff), dummy)
        Else
            inputName.Enabled = True
            ButtonGoStop.Text = "Download"
            ButtonGoStop.Enabled = True
            DownloadM.StopDownload()
            ButtonVerify.Enabled = True
            MergeBox.Enabled = True
            CopyToBox.Enabled = True
            MergeBox_SelectedIndexChanged(Nothing, Nothing)
        End If
    End Sub

    Private Sub CreateNewForm(cForm As Form) Handles DownloadM.NewUI
        If Me.InvokeRequired Then
            Me.Invoke(New Action(Of Form)(AddressOf CreateNewForm), cForm)
        Else
            cForm.Show()
        End If
    End Sub


    'Private Sub ProgressBar1_Click(sender As System.Object, e As System.EventArgs) Handles ProgressBar1.Click
    '    If (Not System.IO.Directory.Exists("down")) Then
    '        System.IO.Directory.CreateDirectory("down")
    '    End If
    '    Dim stubbenVideo As String = "http://hqwallbase.com/images/big/cirno_touhou_simple_background_wallpaper-40197.jpg"
    '    Dim DownloadThread As New Threading.Thread(AddressOf StartDownloadTW)
    '    Downloading = True
    '    DownloadThread.IsBackground = True
    '    Dim param_obj(3) As Object
    '    param_obj(0) = stubbenVideo
    '    param_obj(1) = "down"
    '    param_obj(2) = 3 - 1
    '    DownloadThread.Start(param_obj)
    '    'StartDownload(stubbenVideo, "down", 2)
    '    'Dim videos As List(Of Videos) = TwitchAPI.GetVideos("monotonetim", (AddressOf SetStatsText))

    '    'Dim Abort As Boolean = False

    '    'For VideoNumber As Integer = 0 To videos.Count - 1
    '    '    Dim parts As List(Of SortedDictionary(Of String, String)) = TwitchApps.GetVideoParts(videos(VideoNumber)._id, (AddressOf SetStatsText))
    '    '    Dim partsapi As List(Of SortedDictionary(Of String, String)) = TwitchAPI.GetVideoParts(videos(VideoNumber)._id, (AddressOf SetStatsText))
    '    '    Dim partcount As Integer = parts.Count
    '    '    For Int As Integer = 0 To partcount - 1
    '    '        If parts(Int)("source") = partsapi(Int)("source") Then

    '    '        Else
    '    '            MsgBox("missmatch")
    '    '        End If
    '    '    Next
    '    'Next
    'End Sub
    'Public Sub StartDownloadTW(ByVal param_objx As Object)
    '    Dim url As String = param_objx(0)
    '    Dim foldername As String = param_objx(1)
    '    Dim partno As Integer = param_objx(2)
    '    StartDownload(url, foldername, partno)
    'End Sub

    Private Sub Form1_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        MergeBox.SelectedIndex = 0
        CopyToBox.SelectedIndex = 2
    End Sub

    Private Sub MergeBox_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles MergeBox.SelectedIndexChanged
        If MergeBox.SelectedIndex = 1 Then
            CopyToBox.Enabled = False
        Else
            CopyToBox.Enabled = True
        End If
        'Update 
    End Sub

    Private Sub CopyToBox_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles CopyToBox.SelectedIndexChanged

    End Sub

End Class
