@echo off
cls
:start

RustDedicated -batchmode -nographics +server.ip 192.168.0.107 +rcon.ip 192.168.0.107 +server.port 28015 +rcon.port 28016 +rcon.password "changeme" +server.maxplayers 10 +server.hostname "[BR] DEFENDERS RUST INSTANT/TP/CLAN/X5" +server.identity "my_server_identity" +server.level "Procedural Map" +server.seed 12345 +server.worldsize 4000 +server.saveinterval 300 +server.globalchat true +server.description "Powered by Oxide" +server.headerimage "http://oxidemod.org/styles/oxide/logo.png" +server.url "http://oxidemod.org"

@echo.
@echo Restarting server...
@echo.
goto start
