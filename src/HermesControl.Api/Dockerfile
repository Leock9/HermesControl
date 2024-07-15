# Etapa base do ASP.NET para a aplicação
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Etapa base do SDK do .NET para compilar e restaurar pacotes
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar o arquivo .csproj e restaurar as dependências
COPY ["src/HermesControl.Api/HermesControl.Api.csproj", "src/HermesControl.Api/"]
RUN dotnet restore "src/HermesControl.Api/HermesControl.Api.csproj"

# Copiar o restante dos arquivos e compilar a aplicação
COPY . .
WORKDIR "/src/src/HermesControl.Api"
RUN dotnet build "HermesControl.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publicar a aplicação
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "HermesControl.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Etapa final para executar a aplicação
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HermesControl.Api.dll"]
