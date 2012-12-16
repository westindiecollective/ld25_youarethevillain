#!/bin/bash

InputFile="$1"
OutputFile="$2"
TmpFile="$2.tmp"
echo "converting $InputFile to $OutputFile"

#sed s/eth0/eth1/g file1 >  file2

if [ $3 -eq 1 ]; then
    sed 's/var \(.*\) : \(.*\) = \(.*\);/\2 \1 = \3;/g' $InputFile > $TmpFile
else
if [ $3 -eq 2 ]; then
    sed 's/var \(.*\) : \(.*\);/\2 \1;/g' $InputFile > $TmpFile
else
if [ $3 -eq 3 ]; then
    sed 's/public static function \(.*\)(\(.*\)) : \(.*\) \(.*\)/public static \3 \1(\2) \4/g' $InputFile > $TmpFile
else
if [ $3 -eq 4 ]; then
    sed 's/function \(.*\)(\(.*\)) : \(.*\) \(.*\)/public \3 \1(\2) \4/g' $InputFile > $TmpFile
else
if [ $3 -eq 5 ]; then
    sed 's/\([a-zA-Z0-9_\-]*\) : \([a-zA-Z0-9_\-]*\)\([,)]\)/\2 \1\3/g' $InputFile > $TmpFile
else
if [ $3 -eq 6 ]; then
    sed 's/function \(.*\)/public void \1/g' $InputFile > $TmpFile
else
if [ $3 -eq 7 ]; then
    sed 's/boolean/bool/g' $InputFile > $TmpFile
else
if [ $3 -eq 8 ]; then
    echo "using UnityEngine;" > $TmpFile
    echo "using System.Collections;" >> $TmpFile
    echo "" >> $TmpFile
    echo "public class PlayerMovementMotor_new : MonoBehaviour {" >> $TmpFile
    sed 's/\(.*\)/    \1/g' $InputFile | sed 's/#pragma strict//g' >> $TmpFile
    echo "}" >> $TmpFile
fi  #8
fi  #7
fi  #6
fi  #5
fi  #4
fi  #3
fi  #2
fi  #1

mv $TmpFile $OutputFile

