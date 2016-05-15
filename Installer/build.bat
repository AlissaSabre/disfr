rem

PATH %WIX%bin;%PATH%
candle disfr.wxs
light -b . -b ..\disfr\bin\Release -sloc disfr.wixobj
