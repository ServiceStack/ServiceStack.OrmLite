SET MSBUILD="C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"

PUSHD ..\src
dotnet restore ServiceStack.OrmLite.Signed.sln
POPD

%MSBUILD% build-sn.proj /target:NuGetPack /property:PatchVersion=0

PUSHD ..\src
dotnet restore ServiceStack.OrmLite.sln
POPD

%MSBUILD% build.proj /target:NuGetPack /property:Configuration=Release;RELEASE=true;PatchVersion=0;PatchCoreVersion=0
