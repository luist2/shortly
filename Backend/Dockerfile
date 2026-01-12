# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar todo el backend
COPY . ./

# Restaurar y compilar el proyecto
RUN dotnet restore Shortly_API/Shortly_API.csproj
RUN dotnet publish Shortly_API/Shortly_API.csproj -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Shortly_API.dll"]
