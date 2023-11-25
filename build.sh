#!/usr/bin/env sh

# Where to put the build result
target=./build/

# Which build configuration to use (Typically "Debug"/"Release")
configuration=Release

# Allowed values are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic].
verbosity=m

# common configuration parameters for all commands
common="--nologo --verbosity $verbosity"

dotnet clean $common -c $configuration
dotnet restore $common --force-evaluate --use-lock-file -f
dotnet build $common --no-restore --self-contained -c $configuration

## A "hacky" way to make sure we have our token present next to the app
cp ./TWITCH.SECRET ./app/src/bin/Release/net8.0/linux-x64/