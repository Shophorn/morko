import os
import sys

if "--dll" in sys.argv:
	target = "-target:library"
	print("Building .dll")
else:
	target = ""
	print("Building .exe")

files = [
	"Server2.cs",
	"Logger.cs",
	"ProtocolFormat.cs",
	"NetworkCommand.cs"
]

call = "csc {} {}".format(target, " ".join(files))

os.system(call)