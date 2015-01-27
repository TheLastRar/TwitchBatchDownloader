Imports System.Net
Imports Newtonsoft.Json
Imports System.Text.RegularExpressions

Public Class TwitchApps
    Shared Function GetVideos(ChannelName As String, SetStatsText As Action(Of String)) As List(Of Videos)
        Dim PageNumber As Integer = 1
        Dim AllVideos As New List(Of Videos)


        Dim Done As Boolean = False
        Do
            SetStatsText("Downloading File List")
            Dim url As String = "http://download.twitchapps.com/?stream=" & ChannelName & "&limit=100&page=" & PageNumber.ToString() & "&vodType=a"
            Dim PageResponse As New List(Of String)
            Dim request As HttpWebRequest = DirectCast(HttpWebRequest.Create(url), HttpWebRequest)

            Dim response As WebResponse = request.GetResponse()
            Dim stream As IO.Stream = response.GetResponseStream()

            Using StreamReader As New IO.StreamReader(stream)
                While Not StreamReader.EndOfStream
                    PageResponse.Add(StreamReader.ReadLine())
                End While
            End Using

            'Dim FileListDownloaded As Boolean = False
            'Do While FileListDownloaded = False
            '    'If FileList(58) = "<hr/><div class=""alert alert-error"">Unable to access API.</div>" Then
            '    '    Threading.Thread.Sleep(5000)
            '    'Else
            '    FileListDownloaded = True
            '    'End If
            'Loop
            SetStatsText("Reading File List")

            Dim Index As Integer = 63
            If PageResponse(59).Contains("next") Then
                PageNumber += 1
            Else
                Done = True
            End If
            If PageResponse(Index) = "</div>" Then
                Done = True
                Exit Do
            End If

            Dim CompleatedPage As Boolean = False

            Do While CompleatedPage = False

                Dim Title As String
                If PageResponse(Index).Contains("</td>") Then
                    Title = PageResponse(Index).Substring(4, PageResponse(Index).Length - (5 + 4)) 'need to detect multiline titles
                Else
                    Title = PageResponse(Index).Substring(4, PageResponse(Index).Length - 4)
                    Index += 1
                    Do Until PageResponse(Index).Contains("</td>")
                        Title += " " + PageResponse(Index)
                        Index += 1
                    Loop
                    Title += " " + PageResponse(Index).Substring(0, PageResponse(Index).Length - 5)
                End If
                Index += 1
                Dim Id As String = PageResponse(Index).Substring(4, PageResponse(Index).Length - (5 + 4))
                Index += 3
                Dim StreamDate As String = PageResponse(Index).Substring(4, PageResponse(Index).Length - (5 + 4))
                Index += 5

                Dim URLID As String = Id.Substring(1, Id.Length - 1)

                Dim Videoinfo As New Videos
                Videoinfo.title = Title
                Videoinfo._id = Id
                Videoinfo.recorded_at = StreamDate
                Videoinfo.url = "http://www.twitch.tv/" & ChannelName & "/b/" & URLID
                AllVideos.Add(Videoinfo)

                If PageResponse(Index).Contains("</div>") Then
                    CompleatedPage = True
                End If
            Loop

        Loop Until Done

        Return AllVideos
    End Function

    Shared Function GetVideoParts(_id As String, SetStatsText As Action(Of String)) As List(Of SortedDictionary(Of String, String))
        SetStatsText("Downloading Part List")

        Dim url As String = "http://download.twitchapps.com/listparts.php?id=" & _id
        Dim PageResponse As New List(Of String)

        Dim PartListDownloadDone As Boolean = False
        Do
            Dim request As HttpWebRequest = DirectCast(HttpWebRequest.Create(url), HttpWebRequest)
            Dim response As WebResponse = request.GetResponse()
            Dim stream As IO.Stream = response.GetResponseStream()

            Using StreamReader As New IO.StreamReader(stream)
                While Not StreamReader.EndOfStream
                    PageResponse.Add(StreamReader.ReadLine())
                End While
            End Using

            If PageResponse(0) = "Error: Unable to access API" Then
                Threading.Thread.Sleep(5000)
                PageResponse.Clear()
            Else
                PartListDownloadDone = True
            End If
        Loop Until PartListDownloadDone = True

        Dim URLlist As New List(Of SortedDictionary(Of String, String))
        For PartNo As Integer = 0 To PageResponse.Count - 1
            Dim LineParts As String() = Regex.Split(PageResponse(PartNo), """")
            Dim PartURL As New SortedDictionary(Of String, String)
            PartURL.Add("source", LineParts(1))
            URLlist.Add(PartURL)
        Next
        Return URLlist
    End Function
End Class
