# =============================================================
# Stage 1: Base Runtime (লাইটওয়েট প্রোডাকশন রানটাইম)
# =============================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# =============================================================
# Stage 2: SDK Build & Restore (কোড কম্পাইল করার জন্য)
# =============================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# ১. ক্যাশিং অপটিমাইজেশনের জন্য আগে সব csproj ফাইল কপি করে ডিপেন্ডেন্সি রিস্টোর করা
COPY ["src/Core/Omnicart.Domain/OmniCart.Domain.csproj", "Core/Omnicart.Domain/"]
COPY ["src/Core/Omnicart.Application/Omnicart.Application.csproj", "Core/Omnicart.Application/"]
COPY ["src/Core/Omnicart.Infrustructure/Omnicart.Infrustructure.csproj", "Core/Omnicart.Infrustructure/"]
COPY ["src/Presentation/OmniCart.API/OmniCart.API.csproj", "Presentation/OmniCart.API/"]

RUN dotnet restore "Presentation/OmniCart.API/OmniCart.API.csproj"

# ২. বাকি সব সোর্স কোড কপি করা
COPY src/ .

# ৩. প্রোডাকশন রিলিজ বিল্ড করা
WORKDIR "/src/Presentation/OmniCart.API"
RUN dotnet build "OmniCart.API.csproj" -c Release -o /app/build

# =============================================================
# Stage 3: Publish (বাইনারি ও অ্যাসেট তৈরি)
# =============================================================
FROM build AS publish
RUN dotnet publish "OmniCart.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# =============================================================
# Stage 4: Final Image (শুধুমাত্র ফাইনাল বাইনারি নিয়ে সুপার-ফাস্ট কন্টেইনার)
# =============================================================
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OmniCart.API.dll"]