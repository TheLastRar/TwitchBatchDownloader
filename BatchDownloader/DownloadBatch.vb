Public Class DownloadBatch
    Inherits DownloadVideo

    Protected Overrides Sub Download(Source As String) 'ChannelName

        Dim videos As List(Of VideoData) = TwitchAPI.GetVideos(Source, True, (AddressOf SetStatsText))

        'Dim Abort As Boolean = False

        For VideoNumber As Integer = 0 To videos.Count - 1
            'create folder if missing
            MyBase.Download(videos.Item(VideoNumber)._id)
            'Dim FolderName As String = GetFolderName(videos.Item(VideoNumber))

            If isDownloading = False Then
                Exit For
            End If

            SetTotalText("Total")
            SetTotalProgress(VideoNumber + 1, videos.Count)
            SetMidText("N/A")
            SetProgressMid(0, 100)
            SetTopText("N/A")
            SetTopProgress(0, 100)
        Next
    End Sub

    Protected Overrides Sub UpdateVODProgress(progress As Integer, total As Integer)
        If NoOfDownloads = 1 Then
            SetMidText("Current VOD Progress")
            SetProgressMid(progress, total)
        End If
    End Sub
End Class
