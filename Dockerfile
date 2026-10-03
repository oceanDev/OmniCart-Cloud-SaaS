# =============================================================
# Stage 1: Base Runtime
# =============================================================

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# =============================================================
# Stage 1: SDK Build & Restore
# =============================================================

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy .csproj

COPY ["src/Core/Omnicart.Domain/OmniCart.Domain.csproj", "Core/Omnicart.Domain/"]
COPY ["src/Core/Omnicart.Application/OmniCart.Application.csproj", "Core/Omnicart.Application/"]
COPY ["src/Core/Omnicart.Infrustructure/Omnicart.Infrustructure.csproj", "Core/Omnicart.Infrustructure/"]
COPY ["src/Presentation/OmniCart.API/OmniCart.API.csproj", "Presentation/OmniCart.API/"]

RUN dotnet restore "Presentation/OmniCart.API/OmniCart.API.csproj"

# Copy Remaining Files

COPY src/ .

# Release Build

WORKDIR "/src/Presentation/OmniCart.API"
RUN dotnet build "OmniCart.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OmniCart.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OmniCart.API.dll"]