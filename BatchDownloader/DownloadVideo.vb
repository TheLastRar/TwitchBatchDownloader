Imports System.IO
Imports System.Net
Imports BatchDownloader.MediaInfoLib

Public Class ProgressEventArgs
    Inherits EventArgs
    Public Value As Integer
    Public Max As Integer

    Public Sub New(val As Integer, mx As Integer)
        Value = val
        Max = mx
    End Sub
End Class

Public Class FormShowRequestEvent
    Inherits EventArgs
    Public Value As Form
    Public Sub New(frm As Form)
        Value = frm
    End Sub
End Class

Public Class DownloadVideo
    Public accessLock As New Object
    ''TODO Pass init args in thread
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

    Private Property Merge As Boolean
        Get
            Return _DoMerge
        End Get
        Set(value As Boolean)
            _DoMerge = value
        End Set
    End Property

    Private Property TargetFormat As Format
        Get
            Return _TargetFormat
        End Get
        Set(value As Format)
            _TargetFormat = value
        End Set
    End Property

    Private Property VerifyOldFiles As Boolean
        Get
            Return _Verify
        End Get
        Set(value As Boolean)
            _Verify = value
        End Set
    End Property

#Region "Events + Proxy Functions"

    Public Event DownloadDone(sender As Object, e As EventArgs)
    'Public Event DownloadStoped()
    Public Event TotalProgress(sender As Object, e As ProgressEventArgs)
    Public Event TotalTextUpdate(sender As Object, e As StringEventArgs)

    Public Event ProgressMid(sender As Object, e As ProgressEventArgs)
    Public Event ProgressMidTextUpdate(sender As Object, e As StringEventArgs)

    Public Event ProgressTop(sender As Object, e As ProgressEventArgs) 'SetProgress2(0)
    Public Event ProgressTopTextUpdate(sender As Object, e As StringEventArgs)

    Public Event StatusUpdate(sender As Object, e As StringEventArgs)

    Public Event NewUI(sender As Object, e As FormShowRequestEvent)

    Protected Sub SetStatsText(str As String)
        RaiseEvent StatusUpdate(Me, New StringEventArgs(str))
    End Sub
    Sub SetTotalProgress(value As Integer, max As Integer)
        RaiseEvent TotalProgress(Me, New ProgressEventArgs(value, max))
    End Sub
    Sub SetTotalText(str As String)
        RaiseEvent TotalTextUpdate(Me, New StringEventArgs(str))
    End Sub

    Sub SetProgressMid(value As Integer, max As Integer)
        RaiseEvent ProgressMid(Me, New ProgressEventArgs(value, max))
    End Sub
    Sub SetMidText(str As String)
        RaiseEvent ProgressMidTextUpdate(Me, New StringEventArgs(str))
    End Sub

    Sub SetTopProgress(value As Integer, max As Integer)
        RaiseEvent ProgressTop(Me, New ProgressEventArgs(value, max))
    End Sub
    Sub SetTopText(str As String)
        RaiseEvent ProgressTopTextUpdate(Me, New StringEventArgs(str))
    End Sub

    Sub LogWebError(sender As Object, e As FileDownloadCompletedEventArgs)
        If Not IsNothing(e.ErrorMessage) Then
            Debug.Print(e.ErrorMessage.Message)
        End If
    End Sub

    Dim BarID3Text As String
    Sub SetTotalProgressViaEvent(sender As Object, e As FileDownloadProgressChangedEventArgs)
        RaiseEvent TotalProgress(sender, New ProgressEventArgs(e.ProgressPercentage, 100))
        RaiseEvent TotalTextUpdate(sender,
            New StringEventArgs(BarID3Text & " (" &
                                   (e.DownloadSpeedBytesPerSec \ (1024)).ToString & "KB/s)"))
    End Sub
    Dim BarID2Text As String
    Sub SetMidProgressViaEvent(sender As Object, e As FileDownloadProgressChangedEventArgs)
        RaiseEvent ProgressMid(sender, New ProgressEventArgs(e.ProgressPercentage, 100))
        RaiseEvent ProgressMidTextUpdate(sender,
            New StringEventArgs(BarID2Text & " (" &
                                    (e.DownloadSpeedBytesPerSec \ (1024)).ToString & "KB/s)"))
    End Sub
    Dim BarID1Text As String
    Sub SetTopProgressViaEvent(sender As Object, e As FileDownloadProgressChangedEventArgs)
        RaiseEvent ProgressTop(sender, New ProgressEventArgs(e.ProgressPercentage, 100))
        RaiseEvent ProgressTopTextUpdate(sender,
            New StringEventArgs(BarID1Text & " (" &
                                    (e.DownloadSpeedBytesPerSec \ (1024)).ToString & "KB/s)"))
    End Sub

#End Region
    Public Sub StopDownload()
        ''Synclock / ManualResetEvent?
        isDownloading = False
    End Sub

    Public Sub StartDownload(Source As String, Verify As Boolean, MergeFiles As Boolean, MergedFormat As Format)
        VerifyOldFiles = Verify
        Merge = MergeFiles
        TargetFormat = MergedFormat

        Dim DownloadThread As New Threading.Thread(Sub() NewDownload(Source))
        isDownloading = True
        DownloadThread.IsBackground = True
        DownloadThread.Start()
    End Sub

    Protected Sub NewDownload(Source As String) 'video ID
        ServicePointManager.DefaultConnectionLimit = 1000
        'SetProgress2(0)
        RaiseEvent ProgressTop(Me, New ProgressEventArgs(0, 10))
        'SetTotalProgress(0, 10)
        RaiseEvent TotalProgress(Me, New ProgressEventArgs(0, 10))
        'SetVODProgress(0, 0)
        RaiseEvent ProgressMid(Me, New ProgressEventArgs(0, 10))
        SetStatsText("Loading")
        If (Directory.Exists("Temp")) Then
            Directory.Delete("Temp", True)
        End If
        Directory.CreateDirectory("Temp")

        Download(Source)
        'RaiseEvent TotalTextUpdate("Total")
        'RaiseEvent TotalProgress(100, 100)

        SetStatsText("Done")
        isDownloading = False
        RaiseEvent DownloadDone(Me, New EventArgs())
    End Sub

    Protected Function GetFolderName(Video As VideoData) As String
        'The TwitchAPI and the TwitchAPI had diffrent ideas about spaces
        Dim FolderName As String = Video.recorded_at & "_" & Video.title
        FolderName = FolderName.Replace("\\", "_").Replace("/", "_").Replace("""", "_").Replace("*", "_").Replace(":", "_").Replace("?", "_").Replace("<", "_").Replace(">", "_").Replace("|", "_").Trim().TrimEnd("."c)

        'Deal with streams that have stupid long titles
        Const MaxFullName As Integer = 260 - 1
        Dim MaxDirLen As Integer = 248 - 1
        '                           "Merged.xyz
        '                           "Merged.tmp2
        '                           "Done.Convert
        '                           "Done.Download"
        '                           "Part 1000.ts"
        '                           "Part 1000_Muted.ts"
        '                           "Part 1000_Failed.ts"
        '                           "_StreamURL.txt"
        '                           "Temp/Part 100.mkv" 'FLV convert
        Dim MaxFileLen As Integer = "Part 1000_Failed.ts".Length() + 1 '+1 to Account for /
        Dim MaxFullNameRemaining As Integer = MaxFullName - MaxFileLen
        If MaxFullNameRemaining < MaxDirLen Then
            MaxDirLen = MaxFullNameRemaining
        End If
        MaxDirLen -= (Directory.GetCurrentDirectory().Length + 1) '+1 to Account for /

        If (FolderName.Length) > MaxDirLen Then
            FolderName = FolderName.Substring(0, MaxDirLen)
        End If
        Return FolderName
    End Function

#Region "Verify"
    Protected Sub Verify(url As String, title As String, FolderName As String, PartNo As Integer, BarID As Integer)
        Dim FileFormat As String = Path.GetExtension(url).Split("?"c)(0) '=.flv"
        If VerifyOldFiles Then
            Dim ExpectedSize As Long = GetFileSize(url)

            If ExpectedSize > 0 Then
                Dim ActuralSize As Long = My.Computer.FileSystem.GetFileInfo(FolderName + "\Part " & (PartNo + 1).ToString() & FileFormat).Length
                If Not (ExpectedSize <= ActuralSize) Then
                    '<= works around muted VOD files if they are muted after download (has this happend yet?)
                    SetStatsText("Redownloading " & title & " Part: " & (PartNo + 1))

                    Select Case BarID
                        Case 1
                            RaiseEvent ProgressTopTextUpdate(Me, New StringEventArgs("Part: " & (PartNo + 1)))
                            BarID1Text = "Part: " & (PartNo + 1)
                        Case 2
                            RaiseEvent ProgressMidTextUpdate(Me, New StringEventArgs("Part: " & (PartNo + 1)))
                            BarID2Text = "Part: " & (PartNo + 1)
                        Case 3
                            RaiseEvent TotalTextUpdate(Me, New StringEventArgs("Part: " & (PartNo + 1)))
                            BarID3Text = "Part: " & (PartNo + 1)
                    End Select

                    File.Delete(FolderName + "\Part " & (PartNo + 1).ToString() & FileFormat)
                    StartFileDownloadWrapper(url, title, FolderName, PartNo, BarID)
                Else
                    SetStatsText("File is expected size")
                    If Not (ExpectedSize = My.Computer.FileSystem.GetFileInfo(FolderName + "\Part " & (PartNo + 1).ToString() & FileFormat).Length) Then
                        LogToFile(FolderName, PartNo.ToString(), "Downloaded file larger then online version")
                    End If
                    Threading.Thread.Sleep(500)
                End If
            Else
                SetStatsText("Could not connect")
                LogToFile(FolderName, PartNo.ToString(), "Couldn't Verify File")
                Threading.Thread.Sleep(500)
            End If
        End If

        CheckMuted(FolderName, PartNo, FileFormat)
    End Sub

    Protected Sub CheckMuted(FolderName As String, PartNo As Integer, FileFormat As String)
        Dim MI As MediaInfo = New MediaInfo
        Dim ret As Integer = MI.Open(FolderName + "\Part " & (PartNo + 1).ToString() & FileFormat)
        If ret = 0 Then
            Throw New FieldAccessException("MI Failed To Open File")
        End If
        MI.Close()
        Dim AudioTrack As String = MI.Get(StreamKind.Audio, 0, "Format")
        If AudioTrack = "" Then
            File.Create(FolderName + "\Part " & (PartNo + 1).ToString() & "_Muted").Close()
            LogToFile(FolderName, PartNo.ToString(), "File is Muted")
            LogToFile(FolderName, PartNo.ToString(), "File Muted By Twitch")
            Threading.Thread.Sleep(500)
        End If
        MI.Dispose()
    End Sub

    Protected Shared Function GetFileSize(url As String) As Long
        Dim Tries As Integer = 1
        Do
            Try
                Dim req As WebRequest = WebRequest.Create(url)
                req.Method = "HEAD"
                Using resp As WebResponse = req.GetResponse()
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

        Dim videos As VideoData = TwitchAPI.GetSingleVideo(source, (AddressOf SetStatsText))

        Dim FolderName As String = GetFolderName(videos)
        Dim FileFormat As String = ".flv"
        'Guess Format
        If source.StartsWith("v") Then
            FileFormat = ".ts"
        End If

        'Actully Get the File Format
        FileFormat = Path.GetExtension(PartsList(0)("source")).Split("?"c)(0) '=.flv"

        'check if stream has been downloaded
        If (Not File.Exists(FolderName & "\Done.Download")) Then
            If (Not Directory.Exists(FolderName)) Then
                Directory.CreateDirectory(FolderName)
            End If
            'app txt file to Stream
            File.WriteAllText(FolderName & "\_StreamURL.txt", videos.url)

            'SetVODProgress(0, PartsList.Count)
            RaiseEvent ProgressMid(Me, New ProgressEventArgs(0, PartsList.Count))

            StartPartDownloaderThreads(videos, FolderName)

            If isDownloading = False Then
                Return
            End If

            RaiseEvent TotalProgress(Me, New ProgressEventArgs(PartsList.Count, PartsList.Count))

            File.Create(FolderName & "\Done.Download").Close()
        End If
        'Start this as own thread?
        If Merge = True Then
            SetStatsText("Converting Video") ' (Background)")
            Dim VidMerger As New VideoConvertLogger
            RaiseEvent NewUI(Me, New FormShowRequestEvent(VidMerger))
            Threading.Thread.Sleep(10)
            VidMerger.StartConvert(FolderName, FileFormat, TargetFormat)
            Threading.Thread.Sleep(500)
            VidMerger.MtClose()
        End If
        RaiseEvent ProgressTop(Me, New ProgressEventArgs(0, 10))
        RaiseEvent ProgressMid(Me, New ProgressEventArgs(0, 10))
        RaiseEvent TotalProgress(Me, New ProgressEventArgs(0, 10))
    End Sub


    Protected Const NoOfDownloads As Integer = 2 '3
    Private Sub StartPartDownloaderThreads(videos As VideoData, FolderName As String)
        Dim DLThreads(NoOfDownloads - 1) As Threading.Thread
        For x As Integer = 0 To NoOfDownloads - 1

            Dim BarID As Integer = x + 1
            DLThreads(x) = New Threading.Thread(Sub() DownloadParts(videos, FolderName, BarID))
            DLThreads(x).IsBackground = True
            DLThreads(x).Start()
        Next

        'wait for all threads
        'Dim Alive As Boolean = False
        'Do
        ''Alive = False
        'Threading.Thread.Sleep(500)
        For x As Integer = 0 To NoOfDownloads - 1
            '    If DLThreads(x).IsAlive Then
            '    ''Alive = True
            'End If
            DLThreads(x).Join()
        Next
        ''Loop While Alive
    End Sub

    Private Sub DownloadParts(videos As VideoData, FolderName As String, BarID As Integer)
        ''Dim videos As Videos = params.videos
        ''Dim FolderName As String = params.FolderName
        ''Dim BarID As Integer = params.BarID
        Dim CurrentPart As SortedDictionary(Of String, String) = Nothing
        Do
            Select Case BarID
                Case 1
                    RaiseEvent ProgressTop(Me, New ProgressEventArgs(0, 100))
                    RaiseEvent ProgressTopTextUpdate(Me, New StringEventArgs("N/A"))
                Case 2
                    RaiseEvent ProgressMid(Me, New ProgressEventArgs(0, 100))
                    RaiseEvent ProgressMidTextUpdate(Me, New StringEventArgs("N/A"))
                Case 3
                    RaiseEvent TotalProgress(Me, New ProgressEventArgs(0, 100))
                    RaiseEvent TotalTextUpdate(Me, New StringEventArgs("N/A"))
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
            ''Thread race condition here
            UpdateVODProgress(AvalibleFile + 1, PartsList.Count)
        Loop
    End Sub

    Protected Overridable Sub UpdateVODProgress(progress As Integer, total As Integer)
        If NoOfDownloads = 1 Then
            RaiseEvent ProgressMidTextUpdate(Me, New StringEventArgs("Current VOD Progress"))
            RaiseEvent ProgressMid(Me, New ProgressEventArgs(progress, total))
        End If
        If NoOfDownloads = 2 Then
            RaiseEvent TotalTextUpdate(Me, New StringEventArgs("Current VOD Progress"))
            RaiseEvent TotalProgress(Me, New ProgressEventArgs(progress, total))
        End If
    End Sub

    Private Sub DownLoadSinglePart(Videos As VideoData, PartNo As Integer, PartURL As String, FolderName As String, BarID As Integer) 'PartNo,PartURL,FolderName,BarID
        Select Case BarID
            Case 1
                RaiseEvent ProgressTop(Me, New ProgressEventArgs(0, 100))
            Case 2
                RaiseEvent ProgressMid(Me, New ProgressEventArgs(0, 100))
            Case 3
                RaiseEvent TotalProgress(Me, New ProgressEventArgs(0, 100))
        End Select
        If (File.Exists(FolderName + "\Part " & (PartNo + 1).ToString() & "_Failed")) Then
            File.Delete(FolderName + "\Part " & (PartNo + 1).ToString() & "_Failed")
        End If
        If (File.Exists(FolderName + "\Part " & (PartNo + 1).ToString() & "_Muted")) Then
            File.Delete(FolderName + "\Part " & (PartNo + 1).ToString() & "_Muted")
        End If

        Dim FileFormat As String = Path.GetExtension(PartURL).Split("?"c)(0) '=.flv"

        If (Not File.Exists(FolderName & "\Part " & (PartNo + 1).ToString() & FileFormat)) Then
            SetStatsText("Starting download of " & Videos.title & " Part: " & (PartNo + 1) & "/" & PartsList.Count)
            Select Case BarID
                Case 1
                    RaiseEvent ProgressTopTextUpdate(Me, New StringEventArgs("Part: " & (PartNo + 1)))
                    BarID1Text = "Part: " & (PartNo + 1)
                Case 2
                    RaiseEvent ProgressMidTextUpdate(Me, New StringEventArgs("Part: " & (PartNo + 1)))
                    BarID2Text = "Part: " & (PartNo + 1)
                Case 3
                    RaiseEvent TotalTextUpdate(Me, New StringEventArgs("Part: " & (PartNo + 1)))
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
        Dim FileFormat As String = Path.GetExtension(url).Split("?"c)(0) '=.flv"

        If StartFileDownload(url, FolderName, PartNo, BarID) = False Then
            'If StartDownload(url2, FolderName, PartNo) = False Then
            SetStatsText("Failed to Download " & title & " Part: " & (PartNo + 1))
            File.Create(FolderName + "\Part " & (PartNo + 1).ToString() & "_Failed").Close()
            LogToFile(FolderName, PartNo.ToString(), "Couldn't Download File")
        Else
            SetStatsText("Completed " & title & " Part: " & (PartNo + 1))
            CheckMuted(FolderName, PartNo, FileFormat)
        End If
        Threading.Thread.Sleep(500)
        'End If
    End Sub

    Protected Sub AddProgressChangedHandler(Downloader As DownloadFileAsyncExtended, BarID As Integer)
        Downloader.ProgressUpdateFrequency = DownloadFileAsyncExtended.UpdateFrequency.HalfSecond
        Select Case BarID
            Case 1
                AddHandler Downloader.DownloadProgressChanged, AddressOf SetTopProgressViaEvent
            Case 2
                AddHandler Downloader.DownloadProgressChanged, AddressOf SetMidProgressViaEvent
            Case 3
                AddHandler Downloader.DownloadProgressChanged, AddressOf SetTotalProgressViaEvent
        End Select
        AddHandler Downloader.DownloadCompleted, AddressOf LogWebError
    End Sub

    Protected Function StartFileDownload(url As String, FolderName As String, PartNo As Integer, BarID As Integer) As Boolean
        Dim Downloader As New DownloadFileAsyncExtended()

        Dim FileFormat As String = Path.GetExtension(url).Split("?"c)(0) '=.flv"

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
            Dim Headers As WebHeaderCollection = Downloader.ResponseHeaders
            Dim TargetSize As Long = Long.Parse(Headers.GetValues("Content-Length")(0))

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
                        File.Delete("Temp\Part " & (PartNo + 1).ToString() & FileFormat)

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

            File.Move("Temp\Part " & (PartNo + 1).ToString() & FileFormat, FolderName + "\Part " & (PartNo + 1).ToString() & FileFormat)
            Return True
        End If
    End Function
#End Region

    Protected Sub LogToFile(FolderName As String, PartNo As String, Log As String)
        SyncLock accessLock
            If Not (File.Exists("Log.txt")) Then
                File.Create("Log.txt").Close()
            End If

            Dim FileStream As StreamWriter = File.AppendText("Log.txt")
            FileStream.WriteLine(FolderName & " Part: " & (Integer.Parse(PartNo) + 1).ToString() & ": " & Log)
            FileStream.Close()
        End SyncLock
    End Sub
End Class
