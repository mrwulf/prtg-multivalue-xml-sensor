Imports System.Xml.Linq
Imports System.Xml.XPath
Imports System.Console
Imports System.Web
Imports System.Xml
Imports System.Linq
Imports Gnu.Getopt

Module MultiValueXML

    Sub Main(ByVal cmdArgs() As String)
        Dim url As String = "http://172.16.12.92:8081/mbean?objectname=org.apache.cassandra.request:type=ReadStage&template=identity"
        'Dim xmlReader As XmlTextReader = New XmlTextReader("http://" & HttpUtility.UrlEncode(url))
        Dim xpath As String = "/MBean/Attribute"
        Dim key As String = "@name"
        Dim value As String = "@value"


        Dim LongOptions() As LongOpt = {
            New LongOpt("url", Argument.Required, Nothing, Asc("u")),
            New LongOpt("xpath", Argument.Required, Nothing, Asc("x")),
            New LongOpt("key", Argument.Required, Nothing, Asc("k")),
            New LongOpt("value", Argument.Required, Nothing, Asc("v")),
            New LongOpt("help", Argument.No, Nothing, Asc("?"))
            }
        Dim g As New Getopt("MultiValueXML.exe", cmdArgs, Getopt.digest(LongOptions), LongOptions)
        g.Opterr = False
        Dim c As Integer
        While (InlineAssignHelper(c, g.getopt()) <> -1)
            Select Case c
                Case -1
                    Exit While
                Case Asc("u")
                    url = g.Optarg()
                Case Asc("x")
                    xpath = g.Optarg()
                Case Asc("k")
                    key = g.Optarg()
                Case Asc("v")
                    value = g.Optarg()
                Case Asc("?")
                    WriteLine("Available Options:")
                    WriteLine(vbNewLine & vbTab & "u, --url")
                    WriteLine(vbTab & vbTab & "URL to load XML from")
                    WriteLine(vbNewLine & vbTab & "x, --xpath")
                    WriteLine(vbTab & vbTab & "XPath to collection")
                    WriteLine(vbNewLine & vbTab & "k, --key")
                    WriteLine(vbTab & vbTab & "What to append to XPath to get the key")
                    WriteLine(vbNewLine & vbTab & "v, --value")
                    WriteLine(vbTab & vbTab & "What to append to XPath to get the value")

                    Return
            End Select
        End While

        Dim resultXML As XElement = New XElement("prtg")
        Try
            Dim xml As XDocument = XDocument.Load(url)

            For Each node As XElement In xml.XPathSelectElements(xpath)
                Dim channelName As String = GetValue(node, key)
                Dim channelValue As String = GetValue(node, value)
                resultXML.Add(New XElement("result", New XElement("channel", channelName), New XElement("value", channelValue)))
            Next

            If resultXML.Elements.Count = 0 Then
                resultXML.Add(New XCData("URL:" & url & vbCrLf & "XPath:" & xpath & vbCrLf & "KEY:" & key & vbCrLf & "VALUE:" & value & vbCrLf))

            End If

        Catch ex As Exception
            resultXML.Add(New XElement("error", 1))
            resultXML.Add(New XElement("text", "ERROR:" & ex.Message & vbCrLf & "StackTrace:" & ex.StackTrace))
        End Try

        WriteLine(resultXML.ToString(SaveOptions.DisableFormatting))
    End Sub

    Private Function InlineAssignHelper(Of T)(ByRef target As T, ByVal value As T) As T
        target = value
        Return value
    End Function

    Private Function GetValue(node As XElement, xquery As String) As String
        Dim returnValue As String
        Dim result As IEnumerable = CType(node.XPathEvaluate(xquery), IEnumerable)

        If xquery.Substring(Math.Max(0, xquery.LastIndexOf("/")), 1) = "@" Then
            returnValue = result.Cast(Of XAttribute).FirstOrDefault.Value.ToString
        Else
            returnValue = result.Cast(Of XElement).FirstOrDefault.Value.ToString
        End If

        Return returnValue
    End Function

End Module
