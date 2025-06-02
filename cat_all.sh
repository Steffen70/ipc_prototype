#!/bin/bash

WATCHED_EXTENSIONS=("cs" "ps1" "csproj" "md")
EXT_PATTERN=$(IFS="|" ; echo "${WATCHED_EXTENSIONS[*]}")

# Loop through and print filename + contents
for file in $(git ls-files | grep -E "\.(${EXT_PATTERN})$"); do
    echo -e "\n===== FILE: $file ====="
    cat "$file"
done
