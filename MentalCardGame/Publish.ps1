$build = $env:windir+'\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe'
&$build .\MentalCardGame.csproj /p:Configuration=Release 
NuGet.exe update -self
NuGet.exe pack .\MentalCardGame.csproj -Symbols -Prop Configuration=Release
NuGet.exe push .\MentalCardGame.1.0.0.0.nupkg
NuGet.exe push .\MentalCardGame.1.0.0.0.symbols.nupkg