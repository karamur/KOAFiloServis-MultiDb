@echo off
REM Ollama servisini baslat (AI asistan ozelligi icin)
net start ollama 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo Ollama servisi baslatilamadi. Kurulu oldugundan emin olun.
    echo https://ollama.com
)
