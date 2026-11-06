# ---------- Etapa 1: compilar el backend .NET ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src
COPY backend/Backend.Api.csproj backend/
RUN dotnet restore backend/Backend.Api.csproj
COPY backend/ backend/
RUN dotnet publish backend/Backend.Api.csproj -c Release -o /out --no-restore

# ---------- Etapa 2: compilar el frontend Angular ----------
FROM node:22-alpine AS frontend-build
WORKDIR /web
COPY frontend/package*.json ./
RUN npm ci
COPY frontend/ ./
RUN npm run build -- --configuration production

# ---------- Etapa 3: imagen final (API + frontend servido por la misma app) ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=backend-build /out ./
# Angular genera dist/frontend/browser; ASP.NET lo sirve como archivos estáticos.
COPY --from=frontend-build /web/dist/frontend/browser ./wwwroot

ENV ASPNETCORE_URLS=http://+:8080
# La clave real se entrega al ejecutar el contenedor: -e Jwt__Key="..."
# (si no se entrega, se usa la clave de ejemplo de appsettings.json: solo para demo).

# Usuario sin privilegios (buena práctica de seguridad)
RUN useradd -m appuser
USER appuser

EXPOSE 8080
ENTRYPOINT ["dotnet", "Backend.Api.dll"]
