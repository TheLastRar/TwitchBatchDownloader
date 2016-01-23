Imports BatchDownloader.MediaInfoLib
Imports System.IO

Public Enum Format
    TS
    FLV
    AsSource
    MKV
    MP4
End Enum

Public Class VideoMerger

    Public Event ProcOutput(sender As Object, e As StringEventArgs)
    Public Event ManOutput(sender As Object, e As StringEventArgs)

    Private Sub OutputHandler(sendingProcess As Object, outLine As DataReceivedEventArgs)
        ' Collect the sort command output.
        If Not String.IsNullOrEmpty(outLine.Data) Then
            ' Add the text to the collected output.
            If outLine.Data.StartsWith("[") Then
                'Console.ForegroundColor = ConsoleColor.Yellow
                RaiseEvent ProcOutput(Me, New StringEventArgs("!!!" + outLine.Data))
            Else
                RaiseEvent ProcOutput(Me, New StringEventArgs(outLine.Data))
            End If
            'LogConverision(outLine.Data)
            'Console.ForegroundColor = ConsoleColor.Gray
        End If
    End Sub

#Region "FromTS"
    Private ProbeSizeM As Integer = 32
    Private AnalyzeDurationM As Integer = 60
    Public Sub AdjustAnalysisParams(SourceFiles As List(Of String), FileDirectory As String, MutedFiles As List(Of String))
        'EditZP stream fix V2
        Dim MI As MediaInfo = New MediaInfo
        'We take the second part, as the 1st part may
        'be smaller than following parts due to a large
        'audio delay, an accurate file size is needed
        'for steams that start with muted parts
        Dim FileID As Integer = 1
        'If Stream is very short, take part size /
        'duration form 1st file to avoid an out of 
        'bounds error
        If (SourceFiles.Count <= 2) Then
            'less than too is used to avoid any
            'issues with using a small last part
            'size/duration for probesize/duration
            FileID = 0
        End If
        MI.Open(FileDirectory & "\" & SourceFiles(FileID))
        ProbeSizeM = CType(Math.Ceiling(My.Computer.FileSystem.GetFileInfo(FileDirectory & "\" & SourceFiles(FileID)).Length / (1000000)), Integer)
        AnalyzeDurationM = CType(Math.Ceiling(Double.Parse(MI.Get(StreamKind.General, 0, "Duration")) / 1000.0), Integer)
        MI.Close()
        MI.Dispose()
        'Detect VODs starting with muted files
        Dim NumStartingMutedFiles As Integer = 0
        Do While (MutedFiles.Contains(Path.GetFileNameWithoutExtension(SourceFiles(NumStartingMutedFiles)) & "_Muted"))
            NumStartingMutedFiles += 1
        Loop

        'Ensure We detect the audio channel even when VOD starts with muted files
        ProbeSizeM = ProbeSizeM * (NumStartingMutedFiles + 1)
        AnalyzeDurationM = AnalyzeDurationM * (NumStartingMutedFiles + 1)

    End Sub

    Private Function GenerateParamsTSProtocol(SourceFiles As List(Of String)) As String
        'Only seems to work when going to MKV
        '-probesize, -analyzeduration handle VODs with large (6s) video delay
        'or when the VOD starts with muted files
        Dim argsProtocol As String = "-probesize " & ProbeSizeM & "M -analyzeduration " & AnalyzeDurationM & "M -f mpegts -i ""concat:"
        For x As Integer = 0 To SourceFiles.Count - 1
            Dim FileName As String = SourceFiles(x)
            argsProtocol = argsProtocol & FileName & "|"
        Next
        'convert the audio
        argsProtocol = argsProtocol.TrimEnd("|"c) & """ -copyinkf -c:v copy -copyts -c:a copy -y " '"" & FileDirectory & "\Merged.mkv"""
        Return argsProtocol
    End Function

    Public Sub JoinTSToMPEGTS(SourceFiles As List(Of String), FileDirectory As String, Optional Extension As String = ".ts")
        RaiseEvent ManOutput(Me, New StringEventArgs("Merging TS"))
        Dim watch As Stopwatch = Stopwatch.StartNew()
        If File.Exists(FileDirectory & "\Merged" & Extension) Then File.Delete(FileDirectory & "\Merged" & Extension)

        'Buch Reads and writes into 10 iterations each (test)

        Dim FileS As New FileStream(FileDirectory & "\Merged" & Extension, FileMode.Append, FileAccess.Write)

        Dim FileHandles As New List(Of FileStream)

        For x As Integer = 0 To SourceFiles.Count - 1
            FileHandles.Add(New FileStream(FileDirectory & "\" & SourceFiles(x), FileMode.Open, FileAccess.Read))
        Next
        For x As Integer = 0 To SourceFiles.Count - 1
            RaiseEvent ProcOutput(Me, New StringEventArgs("Appending Part " & x))
            FileHandles(x).CopyTo(FileS)
        Next
        Debug.Print("Save Compleate")
        For x As Integer = 0 To SourceFiles.Count - 1
            FileHandles(x).Close()
        Next
        FileS.Flush()
        FileS.Close()
        watch.Stop()

        'Console.ForegroundColor = ConsoleColor.Cyan
        RaiseEvent ProcOutput(Me, New StringEventArgs(""))
        RaiseEvent ProcOutput(Me, New StringEventArgs("Merging TS took " & (watch.ElapsedMilliseconds \ 1000L) & "s"))
        RaiseEvent ProcOutput(Me, New StringEventArgs(""))
        'Console.ForegroundColor = ConsoleColor.Gray

    End Sub

    Public Sub JoinTSToMKV(SourceFiles As List(Of String), FileDirectory As String, Optional Extension As String = ".mkv")
        RaiseEvent ManOutput(Me, New StringEventArgs("Converting TS to MKV"))
        Dim argsProtocol As String = GenerateParamsTSProtocol(SourceFiles) & "-f matroska ""Merged" & Extension & """"
        Const ProtocolLimit As Integer = 32768

        Dim watch As Stopwatch = Stopwatch.StartNew()

        If argsProtocol.Length + 1 >= ProtocolLimit Then
            RaiseEvent ProcOutput(Me, New StringEventArgs("File To Large to use FFMPEG concat, doing it ourselves (Buggy)"))
            JoinTSToMPEGTS(SourceFiles, FileDirectory, ".tmp")
            argsProtocol = "-f mpegts -i ""Merged.tmp"" -copyinkf -c:v copy -copyts -c:a copy -y -f matroska ""Merged" & Extension & """"
        End If

        StartFFMPEG(FileDirectory, argsProtocol)
        watch.Stop()

        'Console.ForegroundColor = ConsoleColor.Cyan
        RaiseEvent ProcOutput(Me, New StringEventArgs(""))
        RaiseEvent ProcOutput(Me, New StringEventArgs("Converting TS to MKV took " & (watch.ElapsedMilliseconds \ 1000L) & "s"))
        RaiseEvent ProcOutput(Me, New StringEventArgs(""))
        'Console.ForegroundColor = ConsoleColor.Gray


        If argsProtocol.Length + 1 >= ProtocolLimit Then
            File.Delete(FileDirectory & "\Merged.tmp")
        End If
    End Sub
    Public Sub JoinTSToMP4(SourceFiles As List(Of String), FileDirectory As String)
        RaiseEvent ManOutput(Me, New StringEventArgs("Converting TS to MP4"))
        RaiseEvent ManOutput(Me, New StringEventArgs("Managing Audio Sync"))
        JoinTSToMKV(SourceFiles, FileDirectory, ".tmp2")
        RaiseEvent ManOutput(Me, New StringEventArgs("Done Managing Audio Sync"))
        RaiseEvent ManOutput(Me, New StringEventArgs("Starting Final Convert"))

        'Then convert to MP4 (A direct convert will cause desync)
        'Note: in the TS-MKV-MP4 convert, AAC-LC gets converted to AAC-LTP
        'That may be the key to a direct convert without midderling convert
        'At this point, I don't care enough to investigate
        Dim args As String = "-f matroska -i """ & FileDirectory & "\Merged.tmp2"" -copyinkf -c:v copy -copyts -c:a copy -bsf:a aac_adtstoasc -y """ & FileDirectory & "\Merged.mp4"""

        Dim watch As Stopwatch = Stopwatch.StartNew()
        StartFFMPEG(FileDirectory, args)
        watch.Stop()

        'Console.ForegroundColor = ConsoleColor.Cyan
        RaiseEvent ProcOutput(Me, New StringEventArgs(""))
        RaiseEvent ProcOutput(Me, New StringEventArgs("Converting MKV to MP4 took " & (watch.ElapsedMilliseconds \ 1000L) & "s"))
        RaiseEvent ProcOutput(Me, New StringEventArgs(""))
        'Console.ForegroundColor = ConsoleColor.Gray

        File.Delete(FileDirectory & "\Merged.tmp2")
    End Sub
#End Region

#Region "FromFLV"
    '-1 No Convert Needed
    '-2 Lossless
    '-100 BitRate Not Found
    Dim VideoRate As Integer = -1
    Dim AudioRate As Integer = -1
    Dim AudioSample As Integer = -1
    Dim AudioC As Integer = -1
    Public Function CanCopyCodecToMP4(SourceFiles As List(Of String), FileDirectory As String) As Boolean
        Dim MI As MediaInfo = New MediaInfo
        Dim FileID As Integer = 0
        MI.Open(FileDirectory & "\" & SourceFiles(FileID))

        Dim AudioTrack As String = MI.Get(StreamKind.Audio, 0, "Format")
        Dim AudioBRateStr As String = MI.Get(StreamKind.Audio, 0, "BitRate_Maximum")
        Dim AudioBRate As Integer
        If (AudioBRateStr = "") Then
            If Not Integer.TryParse(MI.Get(StreamKind.Audio, 0, "BitRate"), AudioBRate) Then
                AudioBRate = -100
            End If
        Else
            AudioBRate = CInt(AudioBRateStr)
        End If
        AudioSample = CInt(MI.Get(StreamKind.Audio, 0, "SamplingRate"))
        AudioC = CInt(MI.Get(StreamKind.Audio, 0, "Channels"))

        Dim VideoTrack As String = MI.Get(StreamKind.Video, 0, "Format")
        Dim VideoBRateStr As String = MI.Get(StreamKind.Video, 0, "BitRate_Maximum")
        Dim VideoBRate As Integer
        If (VideoBRateStr = "") Then
            If Not Integer.TryParse(MI.Get(StreamKind.Video, 0, "BitRate"), VideoBRate) Then
                VideoBRate = -100
            End If
        Else
            VideoBRate = CInt(AudioBRateStr)
        End If

        MI.Close()
        MI.Dispose()

        Dim ret As Boolean = True

        Select Case AudioTrack
            'Case "AAC"
            Case "ADPCM"
                AudioRate = -2
            Case "Speex"
                AudioRate = AudioBRate
                ret = False
        End Select

        Select Case VideoTrack
           ' Case "AVC"
            Case "Sorenson Spark", "VP6"
                VideoRate = VideoBRate
                ret = False
        End Select

        Return ret
    End Function

    Private Shared Function GenerateParamsAnyDemux(SourceFiles As List(Of String), FileDirectory As String) As String
        'Sometimes gives stuttery audio (re-check)
        Dim InputArray(SourceFiles.Count - 1) As String
        For x As Integer = 0 To SourceFiles.Count - 1
            Dim FileName As String = SourceFiles(x)
            InputArray(x) = "file '" & FileDirectory & "\" & FileName & "'"
        Next
        File.WriteAllLines(FileDirectory & "\list.txt", InputArray) 'just leave the file there

        Dim argsDemux As String = "-f concat -i """ & FileDirectory & "\list.txt" & """ -copyinkf -c:v copy -copyts -c:a copy -y " '""" & FileDirectory & "\Merged.mp4"""
        '-copyinkf has been removed
        Return argsDemux
    End Function

    Private Function PartsFLVToMKV(SourceFiles As List(Of String), FileDirectory As String) As List(Of String)
        ''Convert each part to MKV
        Dim watch As Stopwatch = Stopwatch.StartNew()
        If Not Directory.Exists(FileDirectory & "\Temp") Then
            Directory.CreateDirectory(FileDirectory & "\Temp")
        End If

        Dim TempFiles As New List(Of String)
        For x As Integer = 0 To SourceFiles.Count - 1
            Dim args As String = "-i """ & FileDirectory & "\" & SourceFiles(x) & """ -copyinkf -c:v copy -copyts -c:a copy -y " & """" & FileDirectory & "\Temp\" & SourceFiles(x) & ".mkv"""
            StartFFMPEG(FileDirectory, args)
            TempFiles.Add(SourceFiles(x) & ".mkv")
        Next
        watch.Stop()

        Console.ForegroundColor = ConsoleColor.Cyan
        RaiseEvent ProcOutput(Me, New StringEventArgs(""))
        RaiseEvent ProcOutput(Me, New StringEventArgs("Converting parts took " & (watch.ElapsedMilliseconds \ 1000L) & "s"))
        RaiseEvent ProcOutput(Me, New StringEventArgs(""))
        Console.ForegroundColor = ConsoleColor.Gray

        Return TempFiles
    End Function

    Public Sub JoinFLVToFLV(SourceFiles As List(Of String), FileDirectory As String)
        RaiseEvent ManOutput(Me, New StringEventArgs("Merging FLV"))
        'Errors produced
        'Unkown stream (id 2)
        'Stream found after parsing head
        'Weird Desync issue, fast catching up effect

        Dim argsDemux As String = GenerateParamsAnyDemux(SourceFiles, FileDirectory) & """" & FileDirectory & "\Merged.flv"""

        Dim watch As Stopwatch = Stopwatch.StartNew()
        StartFFMPEG(FileDirectory, argsDemux)
        watch.Stop()

        'Console.ForegroundColor = ConsoleColor.Cyan
        RaiseEvent ProcOutput(Me, New StringEventArgs(""))
        RaiseEvent ProcOutput(Me, New StringEventArgs("Merging FLV took " & (watch.ElapsedMilliseconds \ 1000L) & "s"))
        RaiseEvent ProcOutput(Me, New StringEventArgs(""))
        'Console.ForegroundColor = ConsoleColor.Gray
    End Sub

    Public Sub JoinFLVToMKV(SourceFiles As List(Of String), FileDirectory As String)
        RaiseEvent ManOutput(Me, New StringEventArgs("Converting FLV to MKV"))
        'Errors produced
        'Unkown stream (id 2)
        'Stream found after parsing head
        'Sometimes Desyncs

        Dim TempFiles As List(Of String) = PartsFLVToMKV(SourceFiles, FileDirectory)

        'Dim argsDemux As String = GenerateParamsAnyDemux(SourceFiles, FileDirectory) & """" & FileDirectory & "\Merged.mkv"""
        Dim argsDemux As String = GenerateParamsAnyDemux(TempFiles, FileDirectory & "\Temp") & """" & FileDirectory & "\Merged.mkv""" 'should be mkv

        Dim watch As Stopwatch = Stopwatch.StartNew()
        StartFFMPEG(FileDirectory, argsDemux)
        watch.Stop()

        'Console.ForegroundColor = ConsoleColor.Cyan
        RaiseEvent ProcOutput(Me, New StringEventArgs(""))
        RaiseEvent ProcOutput(Me, New StringEventArgs("Converting Parts to MKV took " & (watch.ElapsedMilliseconds \ 1000L) & "s"))
        RaiseEvent ProcOutput(Me, New StringEventArgs(""))
        'Console.ForegroundColor = ConsoleColor.Gray

        Directory.Delete(FileDirectory & "\Temp", True)
    End Sub

    Public Sub JoinFLVToMP4_FFMPEG(SourceFiles As List(Of String), FileDirectory As String)
        RaiseEvent ManOutput(Me, New StringEventArgs("Converting FLV to MP4"))
        'Errors produced
        'Unkown stream (2)
        'Stream found after parsing head
        'Non-monotonous DTS in output stream 0:0 (video);
        '               Previous: <value>;
        '               current: <value>;
        '               changing to <value+1>. 
        '               This may result in incorrect timestamps in the output file
        'Sometimes get Audio/Visual Corruption (Fixed?)

        Dim TempFiles As List(Of String) = PartsFLVToMKV(SourceFiles, FileDirectory)

        Dim argsDemux As String = GenerateParamsAnyDemux(TempFiles, FileDirectory & "\Temp") & """" & FileDirectory & "\Merged.mp4"""

        If Not (AudioRate = -1) Then
            If (AudioRate = -2) Then
                'argsDemux = argsDemux.Replace("-c:a copy", "-c:a mp4als")
                'argsDemux = argsDemux.Replace("-c:a copy", "-c:a alac")
                argsDemux = argsDemux.Replace("-c:a copy", "-strict -2 -c:a aac -b:a " & (320 * 100).ToString())
            Else
                'For Low Bitrates (I.e. Speex) Increase Bitrate to 128Kb/s
                AudioRate = Math.Max(AudioC, 128 * 1000)

                'aac encoder does not support high bitrates with low channels / sample rate
                'I.e. Mono audio at Sample Rate 11.025 KHz will max out at 66kb/s
                'It sounds worse then the source Speex (27.1 Kb/s) at this level
                'So use another aac encoder. 
                If (128 * 1000 > 6144 * AudioC * AudioSample / 1024.0) Then
                    'Better for lower bitrates
                    argsDemux = argsDemux.Replace("-c:a copy", "-c:a libvo_aacenc -b:a " & (AudioRate).ToString())
                Else
                    'Better for 128+
                    argsDemux = argsDemux.Replace("-c:a copy", "-strict -2 -c:a aac -b:a " & (AudioRate).ToString())
                End If
            End If
        End If
            If Not (VideoRate = -1) Then
            argsDemux = argsDemux.Replace("-c:v copy", "-c:v libx264 -preset medium -crf 18")
        End If

        Dim watch As Stopwatch = Stopwatch.StartNew()
        StartFFMPEG(FileDirectory, argsDemux)
        watch.Stop()

        'Console.ForegroundColor = ConsoleColor.Cyan
        RaiseEvent ProcOutput(Me, New StringEventArgs(""))
        RaiseEvent ProcOutput(Me, New StringEventArgs("Converting Parts to MP4 took " & (watch.ElapsedMilliseconds \ 1000L) & "s"))
        RaiseEvent ProcOutput(Me, New StringEventArgs(""))
        'Console.ForegroundColor = ConsoleColor.Gray

        Directory.Delete(FileDirectory & "\Temp", True)
    End Sub
#End Region

    Private Sub StartFFMPEG(WorkingDirectory As String, Args As String)
        RaiseEvent ProcOutput(Me, New StringEventArgs("Starting FFMPEG"))
        Dim ffSI As New ProcessStartInfo(My.Application.Info.DirectoryPath & "\utils\ffmpeg.exe", Args)
        ffSI.WorkingDirectory = WorkingDirectory
        ffSI.RedirectStandardError = True
        ffSI.RedirectStandardOutput = True
        ffSI.RedirectStandardInput = True
        ffSI.UseShellExecute = False
        ffSI.CreateNoWindow = True

        Dim ff As New Process()
        ff.StartInfo = ffSI
        AddHandler ff.OutputDataReceived, AddressOf OutputHandler
        AddHandler ff.ErrorDataReceived, AddressOf OutputHandler

        ff.Start()
        ff.BeginOutputReadLine()
        ff.BeginErrorReadLine()
        ff.WaitForExit()
        RaiseEvent ProcOutput(Me, New StringEventArgs("FFMPEG Done"))
    End Sub

    'Private Function StartFFprobe(WorkingDirectory As String, Args As String) As String
    '    RaiseEvent ProcOutput("Starting Probe")
    '    Dim ffSI As New ProcessStartInfo(My.Application.Info.DirectoryPath & "\utils\ffprobe.exe", Args)
    '    ffSI.WorkingDirectory = WorkingDirectory
    '    ffSI.RedirectStandardError = True
    '    ffSI.RedirectStandardOutput = False
    '    ''ffSI.RedirectStandardInput = True
    '    ffSI.UseShellExecute = False
    '    ffSI.CreateNoWindow = True

    '    Dim ff As New Process()
    '    ff.StartInfo = ffSI
    '    ff.Start()

    '    Dim sOutput As String = ""
    '    'Using oStreamReader As System.IO.StreamReader = ff.StandardOutput
    '    '    sOutput = oStreamReader.ReadToEnd()
    '    'End Using
    '    Using oStreamReader As System.IO.StreamReader = ff.StandardError
    '        sOutput += oStreamReader.ReadToEnd()
    '    End Using
    '    ff.WaitForExit()
    '    RaiseEvent ProcOutput("Probe Done")
    '    Return sOutput
    'End Function

End Class

Public Class AlphanumComparator 'http://www.dotnetperls.com/alphanumeric-sorting-vbnet
    Implements IComparer(Of String)

    Public Function Compare(ByVal x As String,
       ByVal y As String) As Integer Implements IComparer(Of String).Compare

        ' [1] Validate the arguments.
        Dim s1 As String = x
        If s1 = Nothing Then
            Return 0
        End If

        Dim s2 As String = y
        If s2 = Nothing Then
            Return 0
        End If

        Dim len1 As Integer = s1.Length
        Dim len2 As Integer = s2.Length
        Dim marker1 As Integer = 0
        Dim marker2 As Integer = 0

        ' [2] Loop over both Strings.
        While marker1 < len1 And marker2 < len2

            ' [3] Get Chars.
            Dim ch1 As Char = s1(marker1)
            Dim ch2 As Char = s2(marker2)

            Dim space1(len1) As Char
            Dim loc1 As Integer = 0
            Dim space2(len2) As Char
            Dim loc2 As Integer = 0

            ' [4] Collect digits for String one.
            Do
                space1(loc1) = ch1
                loc1 += 1
                marker1 += 1

                If marker1 < len1 Then
                    ch1 = s1(marker1)
                Else
                    Exit Do
                End If
            Loop While Char.IsDigit(ch1) = Char.IsDigit(space1(0))

            ' [5] Collect digits for String two.
            Do
                space2(loc2) = ch2
                loc2 += 1
                marker2 += 1

                If marker2 < len2 Then
                    ch2 = s2(marker2)
                Else
                    Exit Do
                End If
            Loop While Char.IsDigit(ch2) = Char.IsDigit(space2(0))

            ' [6] Convert to Strings.
            Dim str1 As String = New String(space1)
            Dim str2 As String = New String(space2)

            ' [7] Parse Strings into Integers.
            Dim result As Integer
            If Char.IsDigit(space1(0)) And Char.IsDigit(space2(0)) Then
                Dim thisNumericChunk As Integer = Integer.Parse(str1)
                Dim thatNumericChunk As Integer = Integer.Parse(str2)
                result = thisNumericChunk.CompareTo(thatNumericChunk)
            Else
                result = str1.CompareTo(str2)
            End If

            ' [8] Return result if not equal.
            If Not result = 0 Then
                Return result
            End If
        End While

        ' [9] Compare lengths.
        Return len1 - len2
    End Function
End Class
