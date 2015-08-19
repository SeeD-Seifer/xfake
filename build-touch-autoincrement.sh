#!/bin/bash
INFOPLIST=$1
bN=$(/usr/libexec/PlistBuddy -c "Print :CFBundleVersion" "${INFOPLIST}")
bN=$(($bN))
bN=$(($bN + 1))
bN=$(printf "%d" $bN)
/usr/libexec/PlistBuddy -c "Set :CFBundleVersion $bN" "${INFOPLIST}"

# git commit -m "** iOS build version auto-incremented to $bN" -- ${INFOPLIST}
