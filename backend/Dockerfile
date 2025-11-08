FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5089

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/NotificaPix.Api/NotificaPix.Api.csproj", "src/NotificaPix.Api/"]
COPY ["src/NotificaPix.Core/NotificaPix.Core.csproj", "src/NotificaPix.Core/"]
COPY ["src/NotificaPix.Infrastructure/NotificaPix.Infrastructure.csproj", "src/NotificaPix.Infrastructure/"]
RUN dotnet restore "src/NotificaPix.Api/NotificaPix.Api.csproj"
COPY . .
RUN dotnet publish "src/NotificaPix.Api/NotificaPix.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificaPix.Api.dll"]
