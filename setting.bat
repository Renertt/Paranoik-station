@echo off
echo Pulling latest changes...
git pull
git submodule update --init --recursive

echo Pre-building resources...
python ./RUN_THIS.py

echo Building the game...
dotnet build --configuration Release

echo Done! Use bin/Content.Client/Content.Client.exe
pause
