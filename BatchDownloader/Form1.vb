Imports System.IO


Public Class Form1

    Dim WithEvents DownloadM As DownloadVideo

    ''Dim DoVerify As Boolean = False

#Region "Progress Event Handling"
    Private Sub SetProgressTop(sender As Object, e As ProgressEventArgs) Handles DownloadM.ProgressTop
        If ProgressBarTop.InvokeRequired Then
            ProgressBarTop.Invoke(New Action(Of Object, ProgressEventArgs)(AddressOf SetProgressTop), sender, e)
        Else
            If e.Value >= 0 Then ''Why?
                If Not (ProgressBarTop.Maximum = e.Max) Then
                    ProgressBarTop.Maximum = e.Max
                End If
                ProgressBarTop.Value = Math.Max(e.Value, 0)
            End If
        End If
    End Sub
    Private Sub SetProgressTopText(sender As Object, e As StringEventArgs) Handles DownloadM.ProgressTopTextUpdate
        If PBTopLabel.InvokeRequired Then
            PBTopLabel.Invoke(New Action(Of Object, StringEventArgs)(AddressOf SetProgressTopText), sender, e)
        Else
            PBTopLabel.Text = e.Value
        End If
    End Sub

    Private Sub SetProgressMid(sender As Object, e As ProgressEventArgs) Handles DownloadM.ProgressMid
        If ProgressBarMid.InvokeRequired Then
            ProgressBarMid.Invoke(New Action(Of Object, ProgressEventArgs)(AddressOf SetProgressMid), sender, e)
        Else
            If Not (ProgressBarMid.Maximum = e.Max) Then
                ProgressBarMid.Maximum = e.Max
            End If
            ProgressBarMid.Value = e.Value
        End If
    End Sub
    Private Sub SetProgressMid(sender As Object, e As StringEventArgs) Handles DownloadM.ProgressMidTextUpdate
        If PBMidLabel.InvokeRequired Then
            PBMidLabel.Invoke(New Action(Of Object, StringEventArgs)(AddressOf SetProgressMid), sender, e)
        Else
            PBMidLabel.Text = e.Value
        End If
    End Sub

    Private Sub SetTotalProgress(sender As Object, e As ProgressEventArgs) Handles DownloadM.TotalProgress
        If TotalProgress.InvokeRequired Then
            TotalProgress.Invoke(New Action(Of Object, ProgressEventArgs)(AddressOf SetTotalProgress), sender, e)
        Else
            If Not (TotalProgress.Maximum = e.Max) Then
                TotalProgress.Maximum = e.Max
            End If
            TotalProgress.Value = e.Value
        End If
    End Sub
    Private Sub SetTotalProgressText(sender As Object, e As StringEventArgs) Handles DownloadM.TotalTextUpdate
        If TPLabel.InvokeRequired Then
            TPLabel.Invoke(New Action(Of Object, StringEventArgs)(AddressOf SetTotalProgressText), sender, e)
        Else
            TPLabel.Text = e.Value
        End If
    End Sub
    Private Sub DownloadStoped() Handles DownloadM.DownloadDone
        EnableStuff()
    End Sub
#End Region

    Private Sub ButtonGoStop_Click(sender As Object, e As EventArgs) Handles ButtonGoStop.Click
        ''Is Download Manager Present?
        If IsNothing(DownloadM) Then
            StartDownload(False)
        Else
            ''Download Manager Present
            ''Is it downloading?
            If DownloadM.isDownloading = False Then
                ''Nope, Clear The Download Manager
                DownloadM = Nothing
                StartDownload(False)
                Return
            Else
                ''Yes, Stop the download
                DownloadM.StopDownload()
                ButtonGoStop.Text = "Stopping"
                ButtonGoStop.Enabled = False
            End If
        End If
    End Sub

    Private Sub ButtonVerify_Click(sender As Object, e As EventArgs) Handles ButtonVerify.Click
        'Get a list of folders and delete Download.Done in all of them
        Dim DirList As IEnumerable(Of String) = Directory.EnumerateDirectories(Directory.GetCurrentDirectory)
        For Each Dir As String In DirList
            If File.Exists(Dir & "\Done.Download") Then
                File.Delete(Dir & "\Done.Download")
            End If
            If File.Exists(Dir & "\Done.Convert") Then
                File.Delete(Dir & "\Done.Convert")
            End If
        Next
        If File.Exists("Log.txt") Then 'reset log
            File.Delete("Log.txt")
        End If

        StartDownload(True)
    End Sub

    Private Sub StartDownload(DoVerify As Boolean)
        Dim input As String = inputName.Text

        ''No Download Manager, We can start setting up
        Dim Splt() As String = inputName.Text.Split("/"c)
        ''Is User name of VOD file?
        If Splt.Count > 1 Then
            ''Is a VOD, Detect Which VOD Type
            If Splt(Splt.Count - 2).Length = 1 Then 'Got video ID
                input = "a" & Splt(Splt.Count - 1) 'Past Broadcast
                If Splt(Splt.Count - 2) = "v" Then
                    input = "v" & Splt(Splt.Count - 1) 'New video system
                End If
                If Splt(Splt.Count - 2) = "c" Then 'Highlight
                    input = "c" & Splt(Splt.Count - 1)
                End If
                ''Download single VOD
                DownloadM = New DownloadVideo()
            Else
                Throw New Exception("Invalid input")
            End If
        Else
            ''Is user name
            ''Download Eveything we can
            DownloadM = New DownloadBatch()
        End If

        'DownloadM.VerifyOldFiles = DoVerify
        Dim Merge As Boolean
        If MergeBox.SelectedIndex = 1 Then '1 = No
            Merge = False
        Else
            Merge = True
        End If

        Dim TarFormat As Format

        Select Case CopyToBox.SelectedIndex
            Case Is = 0 'Just Merge
                TarFormat = Format.AsSource
            Case Is = 1 'MKV
                TarFormat = Format.MKV
            Case Is = 2 'MP4
                TarFormat = Format.MP4
        End Select

        DownloadM.StartDownload(input, DoVerify, Merge, TarFormat)

        ''Disable Buttons
        inputName.Enabled = False
        ButtonGoStop.Text = "Stop"
        ButtonVerify.Enabled = False
        MergeBox.Enabled = False
        CopyToBox.Enabled = False
    End Sub

    Private Sub SetStatsText(sender As Object, e As StringEventArgs) Handles DownloadM.StatusUpdate
        If Stats.InvokeRequired Then
            Stats.Invoke(New Action(Of Object, StringEventArgs)(AddressOf SetStatsText), sender, e)
        Else
            Stats.Text = e.Value
        End If
    End Sub

    Private Sub EnableStuff()
        If Stats.InvokeRequired Then
            Stats.Invoke(Sub() EnableStuff())
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

    Private Sub CreateNewForm(sender As Object, e As FormShowRequestEvent) Handles DownloadM.NewUI
        If InvokeRequired Then
            Invoke(New Action(Of Object, FormShowRequestEvent)(AddressOf CreateNewForm), sender, e)
        Else
            e.Value.Show()
        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        MergeBox.SelectedIndex = 0
        CopyToBox.SelectedIndex = 2
    End Sub

    Private Sub MergeBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles MergeBox.SelectedIndexChanged
        If MergeBox.SelectedIndex = 1 Then
            CopyToBox.Enabled = False
        Else
            CopyToBox.Enabled = True
        End If
    End Sub

End Class
