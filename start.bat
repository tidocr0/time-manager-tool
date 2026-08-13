@echo off
echo Starting Time Manager Application...

echo Starting Backend API...
start "Time Manager API" cmd /k "cd TimeManager.Api && dotnet run"

echo Starting Frontend Vue App...
start "Time Manager Frontend" cmd /k "cd time-manager-clone && npm run dev"

echo Both servers are starting in separate windows.
