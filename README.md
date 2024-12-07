# yin-csharp-implementation
the implementation of YIN - The algorithm for audio frequency detection
Based on the python scirpts:
https://github.com/patriceguyot/Yin
Based on the article:
[1] De Cheveigné, A., & Kawahara, H. (2002). YIN, a fundamental frequency estimator for speech and music. The Journal of the Acoustical Society of America, 111(4), 1917-1930.

All the functions in the code correspond to steps in the article [1]. Meanwhile, the difference function has been modify substantially in order to improve speed. Finally, speed has been improved by more than 1000x.
# Tutorial
normalize audio file to wav 16bits without compression.
code ffmpeg:
ffmpeg -i input.mp4 -ar 48000 -ac 1 -acodec pcm_s16le out.wav

this code was optimized for short audio below 1s in duration. 
#author
Thanhday