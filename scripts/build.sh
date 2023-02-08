#!/bin/bash
set -e

cargo build --release
mkdir -p target/release
csc -t:library \
    -r:"$HOME/ApplicationData/vintagestory/VintagestoryAPI.dll" \
    -out:target/release/NativeMod.dll \
    /res:target/release/libNativeMod.so \
    src/mod.cs

pushd target/release
zip -r ../../dest/NativeMod.zip NativeMod.dll
popd

pushd resources
zip -ur ../dest/NativeMod.zip ./**
popd
