Public Class DownloadVideo
    Public accessLock As New Object
    Protected _Downloading As Boolean
    Protected _DoMerge As Boolean
    Protected _TargetFormat As Format
    Protected _Verify As Boolean

    Public Property isDownloading As Boolean
        Get
            SyncLock accessLock
                Return _Downloading
            End SyncLock
        End Get
        Set(value As Boolean)
            SyncLock accessLock
                _Downloading = value
            End SyncLock
        End Set
    End Property

    Public Property Merge As Boolean
        Get
            SyncLock accessLock
                Return _DoMerge
            End SyncLock
        End Get
        Set(value As Boolean)
            SyncLock accessLock
                _DoMerge = value
            End SyncLock
        End Set
    End Property

    Public Property TargetFormat As Format
        Get
            SyncLock accessLock
                Return _TargetFormat
            End SyncLock
        End Get
        Set(value As Format)
            SyncLock accessLock
                _TargetFormat = value
            End SyncLock
        End Set
    End Property

    Public Property VerifyOldFiles As Boolean 'ToDo make this actully do things
        Get
            SyncLock accessLock
                Return _Verify
            End SyncLock
        End Get
        Set(value As Boolean)
            SyncLock accessLock
                _Verify = value
            End SyncLock
        End Set
    End Property

#Region "Events + Proxy Functions"
    Public Event DownloadDone()
    'Public Event DownloadStoped()
    Public Event TotalProgress(value As Integer, max As Integer)
    Public Event TotalTextUpdate(str As String)

    Public Event VODProgress(value As Integer, max As Integer)
    Public Event VODProgressTextUpdate(str As String)

    Public Event FileProgress(value As Integer, max As Integer) 'SetProgress2(0)
    Public Event FileProgressTextUpdate(str As String)

    Public Event StatusUpdate(str As String)

    Public Event NewUI(ui As Form)

    Protected Sub SetStatsText(str As String)
        RaiseEvent StatusUpdate(str)
    End Sub
    Sub SetTotalProgress(value As Integer, max As Integer)
        RaiseEvent TotalProgress(value, max)
    End Sub
    Sub SetTotalText(str As String)
        RaiseEvent TotalTextUpdate(str)
    End Sub

    Sub SetVODProgress(value As Integer, max As Integer)
        RaiseEvent VODProgress(value, max)
    End Sub
    Sub SetVODText(str As String)
        RaiseEvent VODProgressTextUpdate(str)
    End Sub

    Sub SetFileProgress(value As Integer, max As Integer)
        RaiseEvent FileProgress(value, max)
    End Sub
    Sub SetFileText(str As String)
        RaiseEvent FileProgressTextUpdate(str)
    End Sub

    Sub LogWebError(sender As Object, e As FileDownloadCompletedEventArgs)
        If Not IsNothing(e.ErrorMessage) Then
            Debug.Print(e.ErrorMessage.Message)
        End If
    End Sub

    Dim BarID3Text As String
    Sub SetTotalProgressViaEvent(sender As Object, e As FileDownloadProgressChangedEventArgs)
        RaiseEvent TotalProgress(e.ProgressPercentage, 100)
        RaiseEvent TotalTextUpdate(BarID3Text & " (" &
                                   (e.DownloadSpeedBytesPerSec \ (1024)).ToString & "KB/s)")
    End Sub
    Dim BarID2Text As String
    Sub SetVODProgressViaEvent(sender As Object, e As FileDownloadProgressChangedEventArgs)
        RaiseEvent VODProgress(e.ProgressPercentage, 100)
        RaiseEvent VODProgressTextUpdate(BarID2Text & " (" &
                                    (e.DownloadSpeedBytesPerSec \ (1024)).ToString & "KB/s)")
    End Sub
    Dim BarID1Text As String
    Sub SetFileProgressViaEvent(sender As Object, e As FileDownloadProgressChangedEventArgs)
        RaiseEvent FileProgress(e.ProgressPercentage, 100)
        RaiseEvent FileProgressTextUpdate(BarID1Text & " (" &
                                    (e.DownloadSpeedBytesPerSec \ (1024)).ToString & "KB/s)")
    End Sub

#End Region

    Public Sub StopDownload()
        isDownloading = False
    End Sub
    Public Sub StartDownload(Source As String)
        Dim DownloadThread As New Threading.Thread(AddressOf Me.NewDownload)
        isDownloading = True
        DownloadThread.IsBackground = True
        DownloadThread.Start(Source)
    End Sub

    Protected Sub NewDownload(Source As String) 'video ID
        Net.ServicePointManager.DefaultConnectionLimit = 1000
        'SetProgress2(0)
        RaiseEvent FileProgress(0, 100)
        'SetTotalProgress(0, 10)
        RaiseEvent TotalProgress(0, 10)
        'SetVODProgress(0, 0)
        RaiseEvent VODProgress(0, 10)
        SetStatsText("Loading")
        If (System.IO.Directory.Exists("Temp")) Then
            System.IO.Directory.Delete("Temp", True)
        End If
        System.IO.Directory.CreateDirectory("Temp")

        Download(Source)
        'RaiseEvent TotalTextUpdate("Total")
        'RaiseEvent TotalProgress(100, 100)

        SetStatsText("Done")
        isDownloading = False
        RaiseEvent DownloadDone()
    End Sub

    Protected Function GetFolderName(Video As Videos)
        'The TwitchAPI and the TwitchAPI had diffrent ideas about spaces
        Dim FolderName As String = Video.recorded_at & "_" & Video.title
        FolderName = FolderName.Replace("\\", "_").Replace("/", "_").Replace("""", "_").Replace("*", "_").Replace(":", "_").Replace("?", "_").Replace("<", "_").Replace(">", "_").Replace("|", "_").Trim().TrimEnd(".")
        'Dim Directorys As String() = IO.Directory.GetDirectories(IO.Directory.GetCurrentDirectory)
        'For Each Dir As String In Directorys
        '    Dim DirComponents As String() = Dir.Split("\")
        '    Dim DirEnd As String = DirComponents(DirComponents.Count - 1)
        '    If DirEnd.Replace(" ", "") = FolderName.Replace(" ", "") Then
        '        Return DirEnd
        '    End If
        'Next
        Return FolderName
    End Function

#Region "Verify"
    'Sub Verify(url As String, url2 As String, title As String, FolderName As String, PartNo As Integer)
    Protected Sub Verify(url As String, title As String, FolderName As String, PartNo As Integer, BarID As Integer)
        Dim FileFormat As String = IO.Path.GetExtension(url).Split("?")(0) '=.flv"
        If VerifyOldFiles Then
            Dim ExpectedSize As Long = GetFileSize(url)
            'If ExpectedSize = -1 Then
            '    ExpectedSize = GetFileSize(url2)
            'End If
            If ExpectedSize > 0 Then
                Dim ActuralSize As Long = My.Computer.FileSystem.GetFileInfo(FolderName + "\Part " & (PartNo + 1).ToString() & FileFormat).Length
                If Not (ExpectedSize <= ActuralSize) Then
                    '<= works around muted VOD files if they are muted after download (has this happend yet?)
                    SetStatsText("Redownloading " & title & " Part: " & (PartNo + 1))

                    Select Case BarID
                        Case 1
                            RaiseEvent FileProgressTextUpdate("Part: " & (PartNo + 1))
                            BarID1Text = "Part: " & (PartNo + 1)
                        Case 2
                            RaiseEvent VODProgressTextUpdate("Part: " & (PartNo + 1))
                            BarID2Text = "Part: " & (PartNo + 1)
                        Case 3
                            RaiseEvent TotalTextUpdate("Part: " & (PartNo + 1))
                            BarID3Text = "Part: " & (PartNo + 1)
                    End Select

                    IO.File.Delete(FolderName + "\Part " & (PartNo + 1).ToString() & FileFormat)
                    StartFileDownloadWrapper(url, title, FolderName, PartNo, BarID)
                Else
                    SetStatsText("File is expected size")
                    If Not (ExpectedSize = My.Computer.FileSystem.GetFileInfo(FolderName + "\Part " & (PartNo + 1).ToString() & FileFormat).Length) Then
                        LogToFile(FolderName, PartNo, "Downloaded file larger then online version")
                    End If
                    Threading.Thread.Sleep(500)
                End If
            Else
                SetStatsText("Could not connect")
                LogToFile(FolderName, PartNo, "Couldn't Verify File")
                Threading.Thread.Sleep(500)
            End If
        End If

        CheckMuted(FolderName, PartNo, FileFormat)
    End Sub

    Protected Sub CheckMuted(FolderName As String, PartNo As Integer, FileFormat As String)

        Dim MI As MediaInfoLib.MediaInfo = New MediaInfoLib.MediaInfo
        MI.Open(FolderName + "\Part " & (PartNo + 1).ToString() & FileFormat)
        Dim AudioTrack As String = MI.Get_(MediaInfoLib.StreamKind.Audio, 0, "Format")
        If AudioTrack = "" Then
            System.IO.File.Create(FolderName + "\Part " & (PartNo + 1).ToString() & "_Muted").Close()
            LogToFile(FolderName, PartNo, "File is Muted")
            LogToFile(FolderName, PartNo, "File Muted By Twitch")
            Threading.Thread.Sleep(500)
        End If
    End Sub

    Protected Shared Function GetFileSize(url As String) As Long
        Dim Tries As Integer = 1
        Do
            Try
                Dim req As System.Net.WebRequest = System.Net.HttpWebRequest.Create(url)
                req.Method = "HEAD"
                Using resp As System.Net.WebResponse = req.GetResponse()
                    Dim ContentLength As Long
                    If (Long.TryParse(resp.Headers.Get("Content-Length"), ContentLength)) Then
                        Tries = 3
                        Return ContentLength
                    End If
                End Using
            Catch
                Tries += 1
            End Try
        Loop Until Tries = 3
        Return -1
    End Function
#End Region

#Region "Downloading"
    Private PartsList As List(Of SortedDictionary(Of String, String))
    Private CompleatedParts As Boolean()

    Protected Overridable Sub Download(source As String) 'single video
        PartsList = TwitchAPI.GetVideoParts(source, (AddressOf SetStatsText))
        Dim nArray(PartsList.Count - 1) As Boolean
        CompleatedParts = nArray

        Dim videos As Videos = TwitchAPI.GetSingleVideo(source, (AddressOf SetStatsText))

        Dim FolderName As String = GetFolderName(videos)
        Dim FileFormat As String = ".flv"
        'Guess Format
        If source.StartsWith("v") Then
            FileFormat = ".ts"
        End If

        'check if stream has been downloaded
        If (Not System.IO.File.Exists(FolderName & "\Done.Download")) Then
            If (Not System.IO.Directory.Exists(FolderName)) Then
                System.IO.Directory.CreateDirectory(FolderName)
            End If
            'app txt file to Stream
            IO.File.WriteAllText(FolderName & "\_StreamURL.txt", videos.url)

            'SetVODProgress(0, PartsList.Count)
            RaiseEvent VODProgress(0, PartsList.Count)

            StartPartDownloaderThreads(videos, FolderName)

            'Actully Get the File Format
            FileFormat = IO.Path.GetExtension(PartsList(0)("source")).Split("?")(0) '=.flv"

            If isDownloading = False Then
                Return
            End If
            System.IO.File.Create(FolderName & "\Done.Download").Close()
        End If
        'Start this as own thread?
        If Merge = True Then
            SetStatsText("Converting Video") ' (Background)")
            Dim VidMerger As New VideoConvertLogger
            RaiseEvent NewUI(VidMerger)
            Threading.Thread.Sleep(10)
            VidMerger.StartConvert(FolderName, FileFormat, TargetFormat)
            Threading.Thread.Sleep(500)
            VidMerger.MtClose()
        End If
    End Sub

    Private Structure DLTreadParam
        Dim videos As Videos
        Dim FolderName As String
        Dim BarID As Integer
    End Structure
    Protected Const NoOfDownloads As Integer = 2 '3
    Private Sub StartPartDownloaderThreads(videos As Videos, FolderName As String)
        Dim DLThreads(NoOfDownloads - 1) As Threading.Thread
        For x As Integer = 0 To NoOfDownloads - 1
            DLThreads(x) = New Threading.Thread(AddressOf Me.DownloadParts)
            Dim param As New DLTreadParam
            param.videos = videos
            param.FolderName = FolderName
            param.BarID = x + 1
            DLThreads(x).IsBackground = True
            DLThreads(x).Start(param)
        Next

        'wait for all threads
        Dim Alive As Boolean = False
        Do
            Alive = False
            Threading.Thread.Sleep(500)
            For x As Integer = 0 To NoOfDownloads - 1
                If DLThreads(x).IsAlive Then
                    Alive = True
                End If
            Next
        Loop While Alive
    End Sub

    Private Sub DownloadParts(params As DLTreadParam)
        Dim videos As Videos = params.videos
        Dim FolderName As String = params.FolderName
        Dim BarID As Integer = params.BarID
        Dim CurrentPart As SortedDictionary(Of String, String) = Nothing
        Do
            Select Case BarID
                Case 1
                    RaiseEvent FileProgress(0, 100)
                    RaiseEvent FileProgressTextUpdate("N/A")
                Case 2
                    RaiseEvent VODProgress(0, 100)
                    RaiseEvent VODProgressTextUpdate("N/A")
                Case 3
                    RaiseEvent TotalProgress(0, 100)
                    RaiseEvent TotalTextUpdate("N/A")
            End Select
            If isDownloading = False Then
                Exit Do
            End If

            Dim AvalibleFile As Integer = -1
            SyncLock PartsList
                For x As Integer = 0 To PartsList.Count - 1
                    If CompleatedParts(x) = False Then
                        AvalibleFile = x
                        CompleatedParts(x) = True
                        Exit For
                    End If
                Next

                If AvalibleFile = -1 Then
                    Exit Do
                End If
                CurrentPart = PartsList(AvalibleFile)
            End SyncLock
            DownLoadSinglePart(videos, AvalibleFile, CurrentPart("source"), FolderName, BarID)
            UpdateVODProgress(AvalibleFile + 1, PartsList.Count)
        Loop
    End Sub

    Protected Overridable Sub UpdateVODProgress(progress As Integer, total As Integer)
        If NoOfDownloads = 1 Then
            RaiseEvent VODProgressTextUpdate("Current VOD Progress")
            RaiseEvent VODProgress(progress, total)
        End If
        If NoOfDownloads = 2 Then
            RaiseEvent TotalTextUpdate("Current VOD Progress")
            RaiseEvent TotalProgress(progress, total)
        End If
    End Sub

    Private Sub DownLoadSinglePart(Videos As Videos, PartNo As Integer, PartURL As String, FolderName As String, BarID As Integer) 'PartNo,PartURL,FolderName,BarID
        Select Case BarID
            Case 1
                RaiseEvent FileProgress(0, 100)
            Case 2
                RaiseEvent VODProgress(0, 100)
            Case 3
                RaiseEvent TotalProgress(0, 100)
        End Select
        If (System.IO.File.Exists(FolderName + "\Part " & (PartNo + 1).ToString() & "_Failed")) Then
            System.IO.File.Delete(FolderName + "\Part " & (PartNo + 1).ToString() & "_Failed")
        End If
        If (System.IO.File.Exists(FolderName + "\Part " & (PartNo + 1).ToString() & "_Muted")) Then
            System.IO.File.Delete(FolderName + "\Part " & (PartNo + 1).ToString() & "_Muted")
        End If

        Dim FileFormat As String = IO.Path.GetExtension(PartURL).Split("?")(0) '=.flv"

        If (Not System.IO.File.Exists(FolderName & "\Part " & (PartNo + 1).ToString() & FileFormat)) Then
            SetStatsText("Starting download of " & Videos.title & " Part: " & (PartNo + 1) & "/" & PartsList.Count)
            Select Case BarID
                Case 1
                    RaiseEvent FileProgressTextUpdate("Part: " & (PartNo + 1))
                    BarID1Text = "Part: " & (PartNo + 1)
                Case 2
                    RaiseEvent VODProgressTextUpdate("Part: " & (PartNo + 1))
                    BarID2Text = "Part: " & (PartNo + 1)
                Case 3
                    RaiseEvent TotalTextUpdate("Part: " & (PartNo + 1))
                    BarID3Text = "Part: " & (PartNo + 1)
            End Select
            StartFileDownloadWrapper(PartURL, Videos.title, FolderName, PartNo, BarID)
        Else
            If VerifyOldFiles Then
                SetStatsText("Starting Verification of " & Videos.title & " Part: " & (PartNo + 1))
                Verify(PartURL, Videos.title, FolderName, PartNo, BarID)
                'should verify block the dl threads?
            End If
        End If
    End Sub

    'Public Sub StartDownloadWrapper(url As String, url2 As String, title As String, FolderName As String, PartNo As Integer)
    Protected Sub StartFileDownloadWrapper(url As String, title As String, FolderName As String, PartNo As Integer, BarID As Integer)
        Dim FileFormat As String = IO.Path.GetExtension(url).Split("?")(0) '=.flv"

        If StartFileDownload(url, FolderName, PartNo, BarID) = False Then
            'If StartDownload(url2, FolderName, PartNo) = False Then
            SetStatsText("Failed to Download " & title & " Part: " & (PartNo + 1))
            Threading.Thread.Sleep(500)
            System.IO.File.Create(FolderName + "\Part " & (PartNo + 1).ToString() & "_Failed").Close()
            LogToFile(FolderName, PartNo, "Couldn't Download File")
        Else
            SetStatsText("Completed " & title & " Part: " & (PartNo + 1))
            CheckMuted(FolderName, PartNo, FileFormat)
            Threading.Thread.Sleep(500)
        End If
        'End If
    End Sub

    Protected Sub AddProgressChangedHandler(Downloader As DownloadFileAsyncExtended, BarID As Integer)
        Select Case BarID
            Case 1
                AddHandler Downloader.DownloadProgressChanged, AddressOf SetFileProgressViaEvent
            Case 2
                AddHandler Downloader.DownloadProgressChanged, AddressOf SetVODProgressViaEvent
            Case 3
                AddHandler Downloader.DownloadProgressChanged, AddressOf SetTotalProgressViaEvent
        End Select
        AddHandler Downloader.DownloadCompleted, AddressOf LogWebError
    End Sub

    Protected Function StartFileDownload(url As String, FolderName As String, PartNo As Integer, BarID As Integer) As Boolean
        Dim Downloader As New DownloadFileAsyncExtended()
        Dim FileFormat As String = IO.Path.GetExtension(url).Split("?")(0) '=.flv"

        AddProgressChangedHandler(Downloader, BarID)

        Dim Tries As Integer = 0

        Do
            Tries += 1
            Downloader.DowloadFileAsync(url, "Temp\Part " & (PartNo + 1).ToString() & FileFormat, Nothing)
            Do
                Threading.Thread.Sleep(500)
            Loop Until (Not IsNothing(Downloader.ResponseHeaders)) Or Downloader.IsBusy = False
        Loop Until (Not IsNothing(Downloader.ResponseHeaders)) Or Tries = 3

        If IsNothing(Downloader.ResponseHeaders) Then
            Return False
        Else
            Dim Headers As System.Net.WebHeaderCollection = Downloader.ResponseHeaders
            Dim TargetSize As Long = Headers.GetValues("Content-Length")(0)

            Dim PartDownloaded As Boolean = False
            Do
                Do
                    If isDownloading = False Then
                        Downloader.CancelAsync()
                        Return False
                    End If

                    Threading.Thread.Sleep(500)
                Loop Until Downloader.IsBusy = False

                Dim CompleatedSize As Long = My.Computer.FileSystem.GetFileInfo("Temp\Part " & (PartNo + 1).ToString() & FileFormat).Length
                If Not (TargetSize = My.Computer.FileSystem.GetFileInfo("Temp\Part " & (PartNo + 1).ToString() & FileFormat).Length) Then

                    If FileFormat = ".ts" Then 'Resume support seemed buggy for the new vod file system, start the part over
                        IO.File.Delete("Temp\Part " & (PartNo + 1).ToString() & FileFormat)

                        Downloader = New DownloadFileAsyncExtended()
                        AddProgressChangedHandler(Downloader, BarID)
                        'Restart Part from scratch
                        Do
                            Downloader.DowloadFileAsync(url, "Temp\Part " & (PartNo + 1).ToString() & FileFormat, Nothing)
                            Do
                                Threading.Thread.Sleep(500)
                            Loop Until (Not IsNothing(Downloader.ResponseHeaders)) Or Downloader.IsBusy = False
                        Loop Until (Not IsNothing(Downloader.ResponseHeaders))
                    Else
                        Downloader.ResumeAsync()
                    End If

                Else
                    PartDownloaded = True
                End If
            Loop Until PartDownloaded = True

            System.IO.File.Move("Temp\Part " & (PartNo + 1).ToString() & FileFormat, FolderName + "\Part " & (PartNo + 1).ToString() & FileFormat)
            Return True
        End If
    End Function
#End Region

    Protected Sub LogToFile(FolderName As String, PartNo As String, Log As String)
        SyncLock accessLock
            If Not (IO.File.Exists("Log.txt")) Then
                IO.File.Create("Log.txt").Close()
            End If

            Dim FileStream As System.IO.StreamWriter = IO.File.AppendText("Log.txt")
            FileStream.WriteLine(FolderName & " Part: " & (PartNo + 1) & ": " & Log)
            FileStream.Close()
        End SyncLock
    End Sub
End Class
