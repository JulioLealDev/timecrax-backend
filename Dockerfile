FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/Timecrax.Api/*.csproj ./Timecrax.Api/
RUN dotnet restore ./Timecrax.Api/Timecrax.Api.csproj
COPY src/Timecrax.Api/. ./Timecrax.Api/
RUN dotnet publish ./Timecrax.Api/Timecrax.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Timecrax.Api.dll"]
