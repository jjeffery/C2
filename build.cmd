@echo off
set NANT_DIR=\build\programs\nant-0.91\bin\
pushd buildscripts
%NANT_DIR%\nant clean
%NANT_DIR%\nant %1 %2 %3 %4
popd
