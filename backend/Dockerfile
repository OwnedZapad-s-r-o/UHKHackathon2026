FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Nejdřív jen projekty — restore se cachuje, dokud se nezmění závislosti
COPY SmartAudit.slnx ./
COPY SmartAudit.Api/SmartAudit.Api.csproj SmartAudit.Api/
COPY SmartAudit.Core/SmartAudit.Core.csproj SmartAudit.Core/
RUN dotnet restore SmartAudit.Api/SmartAudit.Api.csproj

COPY . .
RUN dotnet publish SmartAudit.Api/SmartAudit.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SmartAudit.Api.dll"]
