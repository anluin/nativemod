#!/bin/bash
set -e

./build.sh
mono ~/ApplicationData/vintagestory/Vintagestory.exe \
  --playStyle preset-surviveandbuild \
  --openWorld modding test world \
  --addModPath "$(pwd)/dest"
