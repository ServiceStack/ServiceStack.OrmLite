#!/bin/bash

declare -A projects=( \
 ["ServiceStack.OrmLite"]="1.3" \
 ["ServiceStack.OrmLite.Sqlite"]="1.3" \
 ["ServiceStack.OrmLite.SqlServer"]="1.3" \
 ["ServiceStack.OrmLite.PostgreSQL"]="1.3" \
 ["ServiceStack.OrmLite.MySql"]="1.6" \
)

#for each project copy files to Nuget.Core/$project/lib folder
#and build nuget package
for proj in "${!projects[@]}"; do
  echo "$proj - ${projects[$proj]}";
  rm -r NuGet.Core/$proj.Core/lib/*

  for ver in ${projects[$proj]}; do
    mkdir -p NuGet.Core/$proj.Core/lib/netstandard$ver
    cp src/$proj/bin/Release/netstandard$ver/$proj.dll NuGet.Core/$proj.Core/lib/netstandard$ver
    cp src/$proj/bin/Release/netstandard$ver/$proj.pdb NuGet.Core/$proj.Core/lib/netstandard$ver
    cp src/$proj/bin/Release/netstandard$ver/$proj.deps.json NuGet.Core/$proj.Core/lib/netstandard$ver
  done

  (cd ./NuGet.Core && mono ./nuget.exe pack $proj.Core/$proj.Core.nuspec -symbols)

done
