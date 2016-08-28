rem
echo off
"%WIX%bin"\candle disfr.wxs
"%WIX%bin"\light -b . -b ..\disfr\bin\Release -b ..\SHChangeNotify\bin\Release -sloc disfr.wixobj
