@echo off
cls
:start

RustDedicated -batchmode -nographics +server.ip 192.168.0.107 +rcon.ip 192.168.0.107 +server.port 28015 +rcon.port 28016 +rcon.password "15021" +server.hostname "[BR] DEFENDERS TEST SERVER!" +server.identity "my_server_identity" +server.level "Procedural Map" +server.seed 12345 +server.globalchat true +server.description "Servidor Teste - Ajudem sua opnião conta aqui!" +server.headerimage "http://oxidemod.org/styles/oxide/logo.png" +server.url "http://oxidemod.org"

@echo.
@echo Restarting server...
@echo.
goto start
