@ECHO OFF

SET GAME_DIR=e:\SteamLibrary\steamapps\common\Subnautica
SET MOD_DIR=BepInEx\plugins\ResourceFabricator
SET MOD_FULL_PATH=%GAME_DIR%\%MOD_DIR%

copy ".\bin\Release\net472\SubnauticaResourceFabricatorMod.dll" %MOD_FULL_PATH%"
copy ".\assets\recipes\*" "%MOD_FULL_PATH%\assets\recipes\"