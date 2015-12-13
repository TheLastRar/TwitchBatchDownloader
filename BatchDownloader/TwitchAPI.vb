Imports Newtonsoft.Json
Imports System.Net
Imports System.Text.RegularExpressions

Public Class TwitchAPI
    Shared Function GetVideos(ChannelName As String, isHighLight As Boolean, SetStatsText As Action(Of String)) As List(Of Videos)
        SetStatsText("Downloading File List")
        Dim url As String = "https://api.twitch.tv/kraken/channels/" & ChannelName & "/videos?limit=100&offset=0&broadcasts=true"

        Dim AllVideos As New List(Of Videos)
        Dim Done As Boolean = False
        Do

            Dim JSONResponse As String = ""
            Dim request As HttpWebRequest = DirectCast(HttpWebRequest.Create(url), HttpWebRequest)
            request.Accept = "application/vnd.twitchtv.v2+json"
            request.Method = "GET"
            Dim CP As New Cache.RequestCachePolicy(Cache.RequestCacheLevel.BypassCache)
            request.CachePolicy = CP

            Dim response As WebResponse = request.GetResponse()
            Dim stream As IO.Stream = response.GetResponseStream()

            Using StreamReader As New IO.StreamReader(stream)
                While Not StreamReader.EndOfStream
                    JSONResponse += StreamReader.ReadLine()
                End While
            End Using

            SetStatsText("Reading File List")
            ''TODO Check Link is valid
            Dim APIReply = JsonConvert.DeserializeObject(Of APIError)(JSONResponse)
            If APIReply.status = 0 Then
                Dim VideoReply = JsonConvert.DeserializeObject(Of VideoList)(JSONResponse)
                AllVideos.AddRange(VideoReply.Videos)
                If isHighLight Then
                    url = VideoReply._links("next") & "&broadcasts=true" 'twitch bug, brodcasts param is not kept
                Else
                    url = VideoReply._links("next") & "&broadcasts=false" 'twitch bug, brodcasts param is not kept
                End If
                If VideoReply.Videos.Count <> 100 Then
                    Done = True
                End If
            Else
                Threading.Thread.Sleep(1000)
            End If
        Loop Until Done

        For index As Integer = 0 To AllVideos.Count - 1
            AllVideos.Item(index).title = Regex.Replace(AllVideos.Item(index).title, "\s+", " ")
        Next

        Return AllVideos
    End Function

    Shared Function GetSingleVideo(_id As String, SetStatsText As Action(Of String)) As Videos
        SetStatsText("Downloading Stream Info")
        Dim url As String = "https://api.twitch.tv/kraken/videos/" & _id & ".json"
        Dim JSONResponse As String = GetTwitchFile(url)

        Dim VideoReply As Videos = JsonConvert.DeserializeObject(Of Videos)(JSONResponse)
        Return VideoReply
    End Function

    Shared Function GetVideoParts(_id As String, SetStatsText As Action(Of String)) As List(Of SortedDictionary(Of String, String))
        SetStatsText("Downloading Part List")

        If _id.StartsWith("v") Then
            Return GetUsherVideoParts(_id, SetStatsText)
        End If

        Dim url As String = "https://api.twitch.tv/api/videos/" & _id & ".json"

        Dim JSONResponse As String = GetTwitchFile(url)

        Dim VideoReply As VideoAPIParts = JsonConvert.DeserializeObject(Of VideoAPIParts)(JSONResponse)

        Dim LiveQ As List(Of VideoAPIChunk) = VideoReply.chunks("live")


        Dim URLlist As New List(Of SortedDictionary(Of String, String))
        For part As Integer = 0 To LiveQ.Count - 1
            'parts should be in order
            Dim PartURL As New SortedDictionary(Of String, String)(New DescendingComparer())
            'Dim Keys As Collections.Generic.Dictionary(Of String, String).KeyCollection = JTVReply(part).transcode_file_urls.Keys
            'For Each Key As String In Keys
            '    Dim newKey As String = Key.Split("_")(1)
            '    PartURL.Add(newKey, JTVReply(part).transcode_file_urls(Key))
            'Next
            PartURL.Add("source", LiveQ(part).url)
            URLlist.Add(PartURL)
        Next

        Return URLlist
    End Function

    Shared Function GetUsherVideoParts(_id As String, SetStatsText As Action(Of String)) As List(Of SortedDictionary(Of String, String))
        SetStatsText("Downloading Part List")

        Dim AT As AccessToken = GetAccessToken(_id, SetStatsText)
        Dim lowerBound As Integer = 1
        Dim UpperBound As Integer = 999999
        'remove 'v' from id"
        Dim url As String = "http://usher.twitch.tv/vod/" & _id.Substring(1) &
            "?player=twitchweb&p=" &
            CInt(Math.Floor((UpperBound - lowerBound + 1) * Rnd())) + lowerBound &
            "&type=any&allow_source=true&allow_audio_only=true&nauthsig=" & AT.sig & "&nauth=" & AT.token

        Dim HLSResponse As List(Of String) = GetM3UFile(url)

        'Find Source Q
        Dim SourceGroupID As String = ""
        Dim SourceM3UURL As String = ""
        For x As Integer = 0 To HLSResponse.Count - 1
            Dim ext_X_Split As String() = HLSResponse(x).Split(":")
            If ext_X_Split.Count > 1 Then
                Dim MediaStreamSplit As String() = ext_X_Split(1).Split(",")
                Dim Params As New Dictionary(Of String, String)
                For pID As Integer = 0 To MediaStreamSplit.Count - 1
                    Dim pSplit As String() = MediaStreamSplit(pID).Split("=")
                    If pSplit.Count = 1 Then
                        Continue For
                    End If
                    If pSplit(1).StartsWith("""") And Not pSplit(1).EndsWith("""") Then
                        'deal with CODEC entry
                        Dim str As String = pSplit(1)
                        str = str & MediaStreamSplit(x + 1)
                        Params.Add(pSplit(0), str)
                    Else
                        Params.Add(pSplit(0), pSplit(1))
                    End If
                Next

                If ext_X_Split(0) = "#EXT-X-MEDIA" Then
                    If Params("NAME").ToLower = """source""" Then
                        SourceGroupID = Params("GROUP-ID")
                    End If
                End If
                If ext_X_Split(0) = "#EXT-X-STREAM-INF" Then
                    If Params("VIDEO").ToLower = SourceGroupID Then
                        SourceM3UURL = HLSResponse(x + 1)
                        Exit For
                    End If
                End If
            End If
        Next

        Dim SURLSplit As String() = SourceM3UURL.Split("/")

        Dim BaseSourceURL As String = SourceM3UURL.Substring(0, SourceM3UURL.Length - SURLSplit(SURLSplit.Count - 1).Length)

        HLSResponse = GetM3UFile(SourceM3UURL)

        Dim LiveQ As New List(Of String)
        Dim CurrentURL As String = ""
        Dim CURLSOffset As Long = 0
        Dim CURLEOffset As Long = 0
        For x As Integer = 0 To HLSResponse.Count - 1
            If HLSResponse(x) = "#EXT-X-ENDLIST" Then
                LiveQ.Add(BaseSourceURL & CurrentURL & "?start_offset=" & CURLSOffset & "&end_offset=" & CURLEOffset)
                Exit For
            End If

            If HLSResponse(x).StartsWith("#") Then Continue For
            If HLSResponse(x) = "" Then Continue For

            Dim URLSplit As String() = HLSResponse(x).Split("?")
            Dim ParamSplit As String() = URLSplit(1).Split("&")
            Dim Params As New Dictionary(Of String, String)
            For pID As Integer = 0 To ParamSplit.Count - 1
                Dim pSplit As String() = ParamSplit(pID).Split("=")
                Params.Add(pSplit(0), pSplit(1))
            Next


            If CurrentURL = "" Or CurrentURL <> URLSplit(0) Then
                If CurrentURL <> "" Then
                    LiveQ.Add(BaseSourceURL & CurrentURL & "?start_offset=" & CURLSOffset & "&end_offset=" & CURLEOffset)
                End If
                CurrentURL = URLSplit(0)
                CURLSOffset = Params("start_offset")
                CURLEOffset = Params("end_offset")
            Else
                CURLEOffset = Params("end_offset")
            End If

            'LiveQ.Add(BaseSourceURL & HLSResponse(x))
        Next

        Dim URLlist As New List(Of SortedDictionary(Of String, String))
        For part As Integer = 0 To LiveQ.Count - 1
            'parts should be in order
            Dim PartURL As New SortedDictionary(Of String, String)(New DescendingComparer())
            PartURL.Add("source", LiveQ(part))
            URLlist.Add(PartURL)
        Next

        Return URLlist
    End Function

    Shared Function GetAccessToken(_id As String, SetStatsText As Action(Of String)) As AccessToken
        SetStatsText("Downloading Token")

        Dim url As String = "https://api.twitch.tv/api/vods/" & _id.Substring(1) & "/access_token.json" 'remove 'v' from id

        Dim JSONResponse As String = GetTwitchFile(url)

        Dim TokenReply As AccessToken = JsonConvert.DeserializeObject(Of AccessToken)(JSONResponse)

        Return TokenReply
    End Function

    Private Shared Function GetTwitchFile(url As String) As String
        Dim JSONResponse As String = ""

        Dim PartListDownloadDone As Boolean = False
        Do
            JSONResponse = ""
            Dim request As HttpWebRequest = DirectCast(HttpWebRequest.Create(url), HttpWebRequest)
            request.Accept = "application/vnd.twitchtv.v2+json"
            request.Method = "GET"
            Dim CP As New Cache.RequestCachePolicy(Cache.RequestCacheLevel.BypassCache)
            request.CachePolicy = CP

            Dim response As WebResponse = request.GetResponse()
            Dim stream As IO.Stream = response.GetResponseStream()


            Using StreamReader As New IO.StreamReader(stream)
                While Not StreamReader.EndOfStream
                    JSONResponse += StreamReader.ReadLine()
                End While
            End Using

            Dim APIReply As APIError
            Try
                APIReply = JsonConvert.DeserializeObject(Of APIError)(JSONResponse)
            Catch
                APIReply = New APIError
                APIReply.status = 0
            End Try
            If APIReply.status = 0 Then
                PartListDownloadDone = True
            Else
                Threading.Thread.Sleep(1000)
            End If

        Loop Until PartListDownloadDone = True
        Return JSONResponse
    End Function

    Private Shared Function GetM3UFile(url As String) As List(Of String)
        Dim HLSResponse As New List(Of String)
        Dim request As HttpWebRequest = DirectCast(HttpWebRequest.Create(url), HttpWebRequest)
        request.Accept = "application/x-mpegurl"
        request.Method = "GET"
        Dim CP As New Cache.RequestCachePolicy(Cache.RequestCacheLevel.BypassCache)
        request.CachePolicy = CP
        Dim response As HttpWebResponse = request.GetResponse()
        Dim StreamNet As IO.Stream = response.GetResponseStream()

        'response.ContentLength
        Dim StreamTemp As New IO.MemoryStream()

        While StreamTemp.Length < response.ContentLength
            StreamNet.CopyTo(StreamTemp)
        End While

        StreamTemp.Seek(0, IO.SeekOrigin.Begin)

        Using StreamReader As New IO.StreamReader(StreamTemp)
            While Not StreamReader.EndOfStream
                HLSResponse.Add(StreamReader.ReadLine())
            End While
        End Using

        StreamTemp.Dispose()
        StreamNet.Dispose()

        Return HLSResponse
    End Function
End Class

Public Class DescendingComparer
    Implements IComparer(Of String)

    Public Function Compare(x As String, y As String) As Integer Implements System.Collections.Generic.IComparer(Of String).Compare
        Return y.CompareTo(x)
    End Function
End Class

Public Class VideoList
    Public _total As Integer
    Public Videos As IList(Of Videos)
    Public _links As Dictionary(Of String, String)
End Class

Public Class APIError
    Public message As String
    Public status As Integer
    'Public error as string
End Class

Public Class Videos
    Public title As String
    Public channel As Dictionary(Of String, String) ' Name and display name
    Public recorded_at As String
    Public broadcast_id As String 'what is this?
    Public _id As String
    Public _links As Dictionary(Of String, String)
    Public embed As String
    Public url As String
    Public views As Integer
    Public preview As String
    Public length As Long
    Public game As String
    Public description As String
End Class

Public Class VideoAPIParts
    Public api_id As String
    Public start_offset As Long
    Public end_offset As Long
    Public play_offset As Long
    Public increment_view_count_url As String
    Public path As String
    Public duration As Long
    Public broadcaster_software As String
    Public channel As String
    Public chunks As Dictionary(Of String, List(Of VideoAPIChunk))
    'Public restrictions As List(Of String)
    Public preview_small As String
    Public preview As String
    Public vod_ad_frequency As Integer
    Public vod_ad_length As Integer
    Public redirect_api_id As String 'Redirects for v urls when this is an b/c link
    Public muted_segments As String() 'is this used
    Public can_highlight As Boolean
End Class

Public Class VideoAPIChunk
    Public url As String
    Public vod_count_url As String
    Public length As Long
    Public upkeep As String
End Class

Public Class AccessToken
    Public token As String
    Public sig As String
End Class

'Public Class JTVVideos
'    Public title As String
'    Public start_time As String
'    Public broadcast_id As String 'what is this?
'    Public id As String 'this is a diffrent id then Twitch.tv
'    Public video_file_url As String
'    Public transcode_file_urls As Dictionary(Of String, String)
'    Public start_timestamp As Long
'End Class

'Public Class VideoPartsJTV
'    Public PartsList As List(Of String)
'End Class