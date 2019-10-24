# Leo Tamminen, shophorn@protonmail.com
# Usb copy helper to work on 2 computers with server development

import shutil
import os
import subprocess

class Copy:
	def __init__(self, src, dst):
		self.src = src
		self.dst = dst

	def __repr__(self):
		return "Copying files from {} to {}".format(self.src, self.dst)		

	def execute(self):
		# Note(Leo): on python 3.7 copytree dst must not exists, so we must remove it first
		if os.path.exists(self.dst):
			shutil.rmtree(self.dst)
		shutil.copytree(self.src, self.dst)

class Eject:
	def __init__(self, disk):
		self.disk = disk

	def __repr__(self):
		return "Ejecting usb disk {}".format(self.disk)

	def execute(self):
		tempFilename = 'tmp.ps1'
		tmpFile = open(tempFilename,'w')
		tmpFile.write('$driveEject = New-Object -comObject Shell.Application\n')
		tmpFile.write('$driveEject.Namespace(17).ParseName("'+self.disk+':").InvokeVerb("Eject")')
		tmpFile.close()
		process = subprocess.Popen(['powershell.exe', '-ExecutionPolicy','Unrestricted','./tmp.ps1'])
		process.communicate()
		os.remove(tempFilename)

operations = [
	Copy("W:/metropolia/morko/MorkoUnityProject/Assets/", "D:/morko/Assets/"),
	Copy("W:/metropolia/morko/MorkoUnityProject/ProjectSettings/", "D:/morko/ProjectSettings/"),
	Eject("D")
]

count = len(operations)
for i in range(count):
	print ("{}/{}: {}".format(i + 1, count, operations[i]))
	operations[i].execute()

print ("Done!")