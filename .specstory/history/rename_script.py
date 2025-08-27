import os
import glob

# 获取当前目录所有文件
files = os.listdir('.')
for file in files:
    if "udp信号" in file:
        os.rename(file, "UDP喷气列车信号处理说明.md")
        print(f"成功重命名: {file} -> UDP喷气列车信号处理说明.md")
        break
