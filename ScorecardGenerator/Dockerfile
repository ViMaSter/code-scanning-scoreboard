FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0.101 AS build
WORKDIR /src
COPY ["ScorecardGenerator.csproj", "./"]
RUN dotnet restore "ScorecardGenerator.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "ScorecardGenerator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ScorecardGenerator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ScorecardGenerator.dll"]
