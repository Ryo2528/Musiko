Option Explicit
Dim shell, fso, appFolder, edge, appUri, command
Set shell = CreateObject("WScript.Shell")
Set fso = CreateObject("Scripting.FileSystemObject")
appFolder = fso.GetParentFolderName(WScript.ScriptFullName)
edge = shell.ExpandEnvironmentStrings("%ProgramFiles(x86)%") & "\Microsoft\Edge\Application\msedge.exe"
If Not fso.FileExists(edge) Then edge = shell.ExpandEnvironmentStrings("%ProgramFiles%") & "\Microsoft\Edge\Application\msedge.exe"
If Not fso.FileExists(edge) Then
  MsgBox "Microsoft Edge was not found. Install or repair Microsoft Edge, then launch Musiko again.", vbExclamation, "Musiko"
  WScript.Quit 1
End If
appUri = "file:///" & Replace(Replace(appFolder, "\", "/"), " ", "%20") & "/index.html"
command = Chr(34) & edge & Chr(34) & " --app=" & Chr(34) & appUri & Chr(34) & " --new-window"
shell.Run command, 1, False
