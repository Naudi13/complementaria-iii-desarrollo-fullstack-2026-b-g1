// Producción (Docker): el frontend y la API se sirven bajo el mismo origen.
// nginx redirige /api → contenedor de la API, así que no hay petición cross-origin (sin CORS).
export const environment = {
  production: true,
  apiUrl: '/api',
};
