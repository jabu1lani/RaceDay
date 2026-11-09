# 1. Base image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# 2. Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copy project files
COPY ["RaceDay.Mvc/RaceDay.Mvc.csproj", "RaceDay.Mvc/"]
COPY ["RaceDay.Contracts/RaceDay.Contracts.csproj", "RaceDay.Contracts/"]

# Restore
RUN dotnet restore "RaceDay.Mvc/RaceDay.Mvc.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/RaceDay.Mvc"
RUN dotnet build "RaceDay.Mvc.csproj" -c Release -o /app/build

# 3. Publish
FROM build AS publish
RUN dotnet publish "RaceDay.Mvc.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RaceDay.Mvc.dll"]