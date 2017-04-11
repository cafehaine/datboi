Set fso = CreateObject("Scripting.FileSystemObject")
pwd = Replace(Wscript.ScriptFullName, Wscript.ScriptName, "")

If not fso.FolderExists(pwd & "screenshots") Then
	fso.CreateFolder(pwd & "screenshots")
End If

function downloadFile()
	strLink = "http://127.0.0.1:6699/screen.png"
	strSaveName = getFileName()
	strSaveTo = pwd & "screenshots\" & strSaveName
	Set objHTTP = CreateObject( "WinHttp.WinHttpRequest.5.1" )
	objHTTP.Open "GET", strLink, False
	objHTTP.Send()
	If fso.FileExists(strSaveTo) Then
		fso.DeleteFile(strSaveTo)
	End If
	If objHTTP.Status = 200 Then
		Dim objStream
		Set objStream = CreateObject("ADODB.Stream")
		With objStream
			.Type = 1 'adTypeBinary
			.Open()
			.Write(objHTTP.ResponseBody)
			.SaveToFile(strSaveTo)
			.Close()
		End With
		set objStream = Nothing
	End If
end function

function add0(num)
	If num < 10 Then
		add0 = "0" & num
	Else
		add0 = num
	End If
end function

function getFileName()
	n = Now()
	t = Time()
	getFileName = Year(n) & "-" & add0(Month(n)) & "-" & add0(Day(n)) & "-" & add0(Hour(t)) & "_" & add0(Minute(t)) & "_" & add0(Second(t)) & ".png"
end function

WScript.Echo("Auto-fetcher started running. To close it you must kill the scripting environment process.")

while true
	downloadFile()
	WScript.Sleep(300000)
wend

