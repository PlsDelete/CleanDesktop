@echo off
chcp 65001 >nul
:: 65001 - UTF-8

:: Admin rights check
echo Данный файл должен быть запущен с правами администратора (ПКМ - Запустить от имени администратора).
echo Нажмите любую клавишу, чтобы продолжить создание сервиса.
pause

set SRVCNAME=CleanDesktop
set FILEPATH=%~dp0\CleanDesktop.WindowsService.exe

net stop %SRVCNAME%
sc delete %SRVCNAME%
sc create %SRVCNAME% binPath= "%FILEPATH%" DisplayName= "%SRVCNAME%" start= auto
sc description %SRVCNAME% "Service for automatically hiding shortcuts and taskbar on Windows 10"
sc start %SRVCNAME%

pause