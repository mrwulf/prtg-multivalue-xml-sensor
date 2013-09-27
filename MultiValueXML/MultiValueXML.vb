Imports System.Xml.Linq
Imports System.Xml.XPath
Imports System.Console
Imports System.Web
Imports System.Xml
Imports System.Linq
Imports Gnu.Getopt

Module MultiValueXML

    Sub Main(ByVal cmdArgs() As String)
        Dim url As String = "http://172.16.12.92:8081/mbean?objectname=org.apache.cassandra.db:type=ColumnFamilies,keyspace=commons,columnfamily=items_&template=identity"
        'Dim xmlReader As XmlTextReader = New XmlTextReader("http://" & HttpUtility.UrlEncode(url))
        Dim xpath As String = "/MBean/Attribute[contains(@availability,'R')]"
        Dim key As String = "//@name"
        Dim value As String = "//@value"
        Dim type As String = "//@type"
        Dim floaters As String = "long,double"

        Dim LongOptions() As LongOpt = {
            New LongOpt("url", Argument.Required, Nothing, Asc("u")),
            New LongOpt("xpath", Argument.Required, Nothing, Asc("x")),
            New LongOpt("key", Argument.Required, Nothing, Asc("k")),
            New LongOpt("value", Argument.Required, Nothing, Asc("v")),
            New LongOpt("type", Argument.Required, Nothing, Asc("t")),
            New LongOpt("float", Argument.Required, Nothing, Asc("f")),
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
                Case Asc("t")
                    type = g.Optarg()
                Case Asc("f")
                    floaters = g.Optarg()
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
                    WriteLine(vbNewLine & vbTab & "t, --type")
                    WriteLine(vbTab & vbTab & "What to append to XPath to get the type")
                    WriteLine(vbNewLine & vbTab & "f, --float")
                    WriteLine(vbTab & vbTab & "Types (returned by --type query) that should be considered float")

                    Return
            End Select
        End While

        WriteLine(Process(url, xpath, key, value, type, floaters))
    End Sub

    Private Function Process(url As String, x_xpath As String, x_key As String, x_value As String, x_type As String, a_floaters As String) As String
        Dim resultXML As XElement = New XElement("prtg")
        Dim BeforeTime, AfterTime As DateTime
        Try
            BeforeTime = Now
            Dim xml As XDocument = XDocument.Load(url)
            AfterTime = Now

            For Each node As XElement In xml.XPathSelectElements(x_xpath)
                Dim channelName As String = GetValue(node, x_key)
                Dim channelValue As String = GetValue(node, x_value)
                Dim isFloat As Boolean = a_floaters.Split(",").Contains(GetValue(node, x_type))

                resultXML.Add(NewResult(channelName, channelValue, isFloat))
            Next

            If resultXML.Elements.Count = 0 Then
                resultXML.Add(NewResult("Error", 1, False))
                resultXML.Add(NewResult("Parameters", "URL:" & url & vbCrLf &
                                                      "XPath:" & x_xpath & vbCrLf &
                                                      "KEY:" & x_key & vbCrLf &
                                                      "VALUE:" & x_value & vbCrLf, False))
            End If

            ' Record Query Time
            resultXML.Add(NewResult("Query Execution Time", CInt(AfterTime.Subtract(BeforeTime).TotalMilliseconds), False))
        Catch ex As Exception
            resultXML.Add(NewResult("error", 1, False))
            resultXML.Add(NewResult("text", "ERROR:" & ex.Message & vbCrLf & "StackTrace:" & ex.StackTrace, False))
        End Try

        Return resultXML.ToString(SaveOptions.DisableFormatting)
    End Function

    Private Function NewResult(name As String, value As String, float As Boolean) As XElement
        Return New XElement("result",
                        New XElement("channel", name),
                        New XElement("value", New XCData(value)),
                        IIf(float, New XElement("float", 1), New XElement("float", 0)))
    End Function

    Private Function InlineAssignHelper(Of T)(ByRef target As T, ByVal value As T) As T
        target = value
        Return value
    End Function

    Private Function GetValue(aNode As XElement, xquery As String) As String
        Dim returnValue As String
        Dim newDoc As XDocument = New XDocument(aNode.DescendantNodesAndSelf)
        Dim result As IEnumerable = CType(newDoc.XPathEvaluate(xquery), IEnumerable)

        If xquery.Substring(Math.Max(0, xquery.LastIndexOf("/") + 1), 1) = "@" Then
            returnValue = result.Cast(Of XAttribute).FirstOrDefault.Value.ToString
        Else
            returnValue = result.Cast(Of XElement).FirstOrDefault.Value.ToString
        End If

        Return returnValue
    End Function

End Module
