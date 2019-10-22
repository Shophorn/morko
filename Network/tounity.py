import shutil

src = "W:/metropolia/morko/Network/MorkoNetwork"
dst = "W:/metropolia/morko/MorkoUnityProject/Assets/Plugins/MorkoNetwork"

shutil.copy(src + ".dll", dst + ".dll")
shutil.copy(src + ".pdb", dst + ".pdb")
