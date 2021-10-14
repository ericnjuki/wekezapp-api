FROM mcr.microsoft.com/dotnet/core/aspnet:2.2.3
COPY ./bin/Release/netcoreapp2.2/publish/ App/
WORKDIR /App
ENTRYPOINT ["dotnet", "wekezapp.core.dll"]