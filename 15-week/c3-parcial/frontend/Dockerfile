# =============================================================
# CitasMed frontend — imagen multi-stage
#   1) build: Node compila Angular en modo producción
#   2) web:   nginx sirve los archivos estáticos y reenvía /api a la API
# Construir (desde la carpeta frontend):  docker build -t citasmed-web .
# =============================================================

# ---------- Etapa 1: build ----------
FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

# ---------- Etapa 2: web ----------
FROM nginx:1.27-alpine AS web

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html

EXPOSE 80
