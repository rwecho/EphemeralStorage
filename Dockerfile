FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . .

RUN dotnet restore -nowarn:msb3202,nu1503
RUN dotnet build --no-restore -c Release -o /app ./src/EphemeralStorage.HttpApi.Host/EphemeralStorage.HttpApi.Host.csproj


FROM build AS publish
RUN ls -lh
RUN dotnet publish --no-restore -c Release -o /app ./src/EphemeralStorage.HttpApi.Host/EphemeralStorage.HttpApi.Host.csproj

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
CMD ["dotnet", "EphemeralStorage.HttpApi.Host.dll"]
