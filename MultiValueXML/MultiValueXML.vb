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
        Dim x_xpath As String = "/MBean/Attribute[contains(@availability,'R')]"
        Dim x_key As String = "//@name"
        Dim x_value As String = "//@value"
        Dim x_type As String = "//@type"
        Dim a_floaters As String = "long,double"

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
                    x_xpath = g.Optarg()
                Case Asc("k")
                    x_key = g.Optarg()
                Case Asc("v")
                    x_value = g.Optarg()
                Case Asc("t")
                    x_type = g.Optarg()
                Case Asc("f")
                    a_floaters = g.Optarg()
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

        Dim resultXML As XElement = New XElement("prtg")
        Try
            Dim xml As XDocument = XDocument.Load(url)

            For Each node As XElement In xml.XPathSelectElements(x_xpath)
                Dim channelName As String = GetValue(node, x_key)
                Dim channelValue As String = GetValue(node, x_value)
                Dim isFloat As Boolean = GetFloat(node, x_type, a_floaters.Split(","))

                resultXML.Add(New XElement("result", New XElement("channel", channelName), New XElement("value", channelValue), IIf(isFloat, New XElement("float", 1), New XElement("float", 0))))
            Next

            If resultXML.Elements.Count = 0 Then
                resultXML.Add(New XCData("URL:" & url & vbCrLf & "XPath:" & x_xpath & vbCrLf & "KEY:" & x_key & vbCrLf & "VALUE:" & x_value & vbCrLf))
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

    Private Function GetFloat(aNode As XElement, xquery As String, floaters() As String) As Boolean
        Dim returnValue As String
        Dim newDoc As XDocument = New XDocument(aNode.DescendantNodesAndSelf)
        Dim result As IEnumerable = CType(newDoc.XPathEvaluate(xquery), IEnumerable)

        If xquery.Substring(Math.Max(0, xquery.LastIndexOf("/")+1), 1) = "@" Then
            returnValue = result.Cast(Of XAttribute).FirstOrDefault.Value.ToString
        Else
            returnValue = result.Cast(Of XElement).FirstOrDefault.Value.ToString
        End If

        Return floaters.Contains(returnValue)
    End Function


End Module
