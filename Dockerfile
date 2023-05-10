FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

COPY src .
RUN dotnet restore
RUN dotnet publish -c Release -o out MdReactionist/MdReactionist.csproj

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "MdReactionist.dll"]

