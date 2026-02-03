FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["ATMChallenge.API/ATMChallenge.API.csproj", "ATMChallenge.API/"]
COPY ["ATMChallenge.Application/ATMChallenge.Application.csproj", "ATMChallenge.Application/"]
COPY ["ATMChallenge.Domain/ATMChallenge.Domain.csproj", "ATMChallenge.Domain/"]
COPY ["ATMChallenge.Infrastructure/ATMChallenge.Infrastructure.csproj", "ATMChallenge.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "ATMChallenge.API/ATMChallenge.API.csproj"

# Copy source code
COPY . .

# Build
WORKDIR "/src/ATMChallenge.API"
RUN dotnet build "ATMChallenge.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 2: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ATMChallenge.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ATMChallenge.API.dll"]
