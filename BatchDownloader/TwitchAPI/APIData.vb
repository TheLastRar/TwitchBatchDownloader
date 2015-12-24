''Data Types sent out by API for the downloader to use
Public Class VideoList
    Public _total As Integer
    Public Videos As IList(Of VideoData)
    Public _links As Dictionary(Of String, String)
End Class

Public Class VideoData
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
''JSON error
Public Class APIError
    Public message As String
    Public status As Integer
    'Public error as string
End Class
''Old FLV vods
Public Class VideoFLVStreamInfo
    Public api_id As String
    Public start_offset As Long
    Public end_offset As Long
    Public play_offset As Long
    Public increment_view_count_url As String
    Public path As String
    Public duration As Long
    Public broadcaster_software As String
    Public channel As String
    Public chunks As Dictionary(Of String, List(Of VideoFLVPart))
    'Public restrictions As List(Of String)
    Public preview_small As String
    Public preview As String
    Public vod_ad_frequency As Integer
    Public vod_ad_length As Integer
    Public redirect_api_id As String 'Redirects for v urls when this is an b/c link
    Public muted_segments As String() 'is this used
    Public can_highlight As Boolean
End Class

Public Class VideoFLVPart
    Public url As String
    Public vod_count_url As String
    Public length As Long
    Public upkeep As String
End Class

''AccessToken for new TS Vods
Public Class AccessToken
    Public token As String
    Public sig As String
End Class