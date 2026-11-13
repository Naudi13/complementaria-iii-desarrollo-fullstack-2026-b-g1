# =============================================================
# CitasMed.Api — imagen multi-stage
#   1) build:   SDK completo para restaurar, compilar y publicar
#   2) runtime: solo el runtime de ASP.NET (imagen pequeña y segura)
# Construir (desde la carpeta backend):  docker build -t citasmed-api .
# =============================================================

# ---------- Etapa 1: build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero solo los .csproj: si no cambian las dependencias,
# Docker reutiliza la capa de "restore" (builds mucho más rápidos).
COPY src/CitasMed.Domain/CitasMed.Domain.csproj                 src/CitasMed.Domain/
COPY src/CitasMed.Application/CitasMed.Application.csproj       src/CitasMed.Application/
COPY src/CitasMed.Infrastructure/CitasMed.Infrastructure.csproj src/CitasMed.Infrastructure/
COPY src/CitasMed.Api/CitasMed.Api.csproj                       src/CitasMed.Api/
RUN dotnet restore src/CitasMed.Api/CitasMed.Api.csproj

# Luego el código fuente y la publicación en modo Release.
COPY src/ src/
RUN dotnet publish src/CitasMed.Api/CitasMed.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- Etapa 2: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Carpeta para la base SQLite (se monta como volumen para no perder datos).
RUN mkdir -p /app/data && chown app:app /app/data

COPY --from=build /app/publish .

# Valores por defecto NO sensibles. Los secretos (Jwt__Key) se pasan al ejecutar.
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__Default="Data Source=/app/data/citasmed.db"

EXPOSE 8080

# Usuario sin privilegios (incluido en las imágenes oficiales de .NET).
USER app

ENTRYPOINT ["dotnet", "CitasMed.Api.dll"]
