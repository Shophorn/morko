# Leo Tamminen, shophorn@protonmail.com
# Usb copy helper to work on 2 computers with server development

import shutil

src = "W:/metropolia/morko/MorkoUnityProject/Assets/"
dst = "D:/morko/Assets/"

print ("Copying files from {} to {}".format(src, dst))

# Note(Leo): on python 3.7 copytree dst must not exits, so we must remove it first
shutil.rmtree(dst)
shutil.copytree(src, dst)

print ("Done!")