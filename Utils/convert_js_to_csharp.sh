#!/bin/bash

InputFile="$1"
OutputFile="$2"
TmpFileBase="$2"
echo "converting $InputFile to $OutputFile"

#sed s/eth0/eth1/g file1 >  file2

 TmpInputFile="$InputFile"

if [ $# -eq 2 ] || [ $3 -eq 1 ]; then
    TmpOutputFile="${TmpFileBase}_1"
    sed 's/var \(.*\) : \(.*\) = \(.*\);/\2 \1 = \3;/g' $TmpInputFile > $TmpOutputFile
    TmpInputFile="$TmpOutputFile"
fi
if [ $# -eq 2 ] || [ $3 -eq 2 ]; then
    TmpOutputFile="${TmpFileBase}_2"
    sed 's/var \(.*\) : \(.*\);/\2 \1;/g' $TmpInputFile > $TmpOutputFile
    TmpInputFile="$TmpOutputFile"
fi
if [ $# -eq 2 ] || [ $3 -eq 3 ]; then
    TmpOutputFile="${TmpFileBase}_3"
    sed 's/public static function \(.*\)(\(.*\)) : \(.*\) \(.*\)/public static \3 \1(\2) \4/g' $TmpInputFile > $TmpOutputFile
    TmpInputFile="$TmpOutputFile"
fi
if [ $# -eq 2 ] || [ $3 -eq 4 ]; then
    TmpOutputFile="${TmpFileBase}_4"
    sed 's/function \(.*\)(\(.*\)) : \(.*\) \(.*\)/public \3 \1(\2) \4/g' $TmpInputFile > $TmpOutputFile
    TmpInputFile="$TmpOutputFile"
fi
if [ $# -eq 2 ] || [ $3 -eq 5 ]; then
    TmpOutputFile="${TmpFileBase}_5"
    sed 's/\([a-zA-Z0-9_\-]*\) : \([a-zA-Z0-9_\-]*\)\([,)]\)/\2 \1\3/g' $TmpInputFile > $TmpOutputFile
    TmpInputFile="$TmpOutputFile"
fi
if [ $# -eq 2 ] || [ $3 -eq 6 ]; then
    TmpOutputFile="${TmpFileBase}_6"
    sed 's/function \(.*\)/public void \1/g' $TmpInputFile > $TmpOutputFile
    TmpInputFile="$TmpOutputFile"
fi
if [ $# -eq 2 ] || [ $3 -eq 7 ]; then
    TmpOutputFile="${TmpFileBase}_7"
    sed 's/boolean/bool/g' $TmpInputFile > $TmpOutputFile
    TmpInputFile="$TmpOutputFile"
fi
if [ $# -eq 2 ] || [ $3 -eq 8 ]; then
    TmpOutputFile="${TmpFileBase}_8"
    echo "using UnityEngine;" > $TmpOutputFile
    echo "using System.Collections;" >> $TmpOutputFile
    echo "" >> $TmpOutputFile
    echo "public class PlayerMovementMotor_new : MonoBehaviour {" >> $TmpOutputFile
    sed 's/\(.*\)/    \1/g' $TmpInputFile | sed 's/#pragma strict//g' >> $TmpOutputFile
    echo "}" >> $TmpOutputFile
fi

mv $TmpOutputFile $OutputFile

