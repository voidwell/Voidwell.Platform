pushd %~dp0\src\Voidwell.Platform.Data
set ASPNETCORE_ENVIRONMENT=Development
dotnet ef database update -v ^
    -c Voidwell.Platform.Data.VoidwellDbContext
popd
