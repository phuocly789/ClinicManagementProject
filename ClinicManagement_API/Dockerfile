# Sử dụng image SDK để build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy file csproj và restore dependencies
COPY ["ClinicManagement_API/ClinicManagement_API.csproj", "ClinicManagement_API/"]
COPY ["ClinicManagement_Infrastructure/ClinicManagement_Infrastructure.csproj", "ClinicManagement_Infrastructure/"]

RUN dotnet restore "ClinicManagement_API/ClinicManagement_API.csproj"

# Copy toàn bộ source code
COPY . .

# Build và Publish
WORKDIR "/src/ClinicManagement_API"
# Thêm flag bỏ qua lỗi trùng file output
RUN dotnet publish "ClinicManagement_API.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:ErrorOnDuplicatePublishOutputFiles=false

# Tạo image runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ClinicManagement_API.dll"]