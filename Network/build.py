import os
import sys


files = [
	"Server.cs",
	"Logger.cs",
	"ProtocolFormat.cs",
	"NetworkCommand.cs",
	"Constants.cs"
]

build_exe = "--exe" in sys.argv

if "--exe" in sys.argv:
	print ("Building standalone server")
	call = "csc -out:Server2.exe Program.cs -r:MorkoNetwork.dll"
else:
	print ("Building MorkoNetwork library")
	call = "csc -out:MorkoNetwork.dll -target:library {}".format(" ".join(files))

os.system(call)
