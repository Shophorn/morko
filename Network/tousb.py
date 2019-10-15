# Leo Tamminen, shophorn@protonmail.com
# Usb copy helper to work on 2 computers with server development

import shutil

# dll_src = "W:/metropolia/morko/MorkoUnityProject/Assets/Plugins/MorkoNetwork.dll"
# dll_dst = "D:/morko/MorkoNetwork.dll"
# shutil.copy(dll_src, dll_dst)

# Note(Leo): on python 3.7 copytree dst must not exits, so we must remove it first
code_src = "W:/metropolia/morko/MorkoUnityProject/Assets/"
code_dst = "D:/morko/Assets/"

shutil.rmtree(code_dst)
shutil.copytree(code_src, code_dst)
