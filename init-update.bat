pushd %~dp0\src\Voidwell.Internal.Data
set ASPNETCORE_ENVIRONMENT=Development
dotnet ef database update -v ^
    -c Voidwell.Internal.Data.VoidwellDbContext ^
    --msbuildprojectextensionspath ./../../build/Voidwell.Internal.Data/Debug/obj
popd