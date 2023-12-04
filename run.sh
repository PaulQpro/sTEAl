#!/usr/bin/env bash
# mono only
echo "===========" && echo "Compiling StealLang.cs:" && echo "===========" &&
mcs -target:library StealLang.cs -r:System.Core -r:System.Text.Json &&
echo "===========" && echo "Compiling Steal.cs:" && echo "===========" &&
mcs Steal.cs -r:StealLang &&
echo "===========" && echo "Executing Steal.exe:" && echo "===========" &&
./Steal.exe
