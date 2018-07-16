pushd %~dp0\src\Voidwell.Internal.Data
set ASPNETCORE_ENVIRONMENT=Development
dotnet ef migrations add VoidwellDbContext.release.1 -v ^
    -c Voidwell.Internal.Data.VoidwellDbContext ^
    -o ./Migrations ^
    --msbuildprojectextensionspath ./../../build/Voidwell.Internal.Data/Debug/obj
popd