pushd %~dp0\src\Voidwell.Platform.Data
set ASPNETCORE_ENVIRONMENT=Development
dotnet ef migrations add %1 -v ^
    -c Voidwell.Platform.Data.VoidwellDbContext ^
    -o ./Migrations
popd
