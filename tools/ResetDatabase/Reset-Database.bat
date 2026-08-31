@echo off
:: Double-click this file to reset the ASID Edge database.
:: It will open a PowerShell window and guide you through the reset.
powershell -ExecutionPolicy Bypass -File "%~dp0Reset-Database.ps1"
pause
