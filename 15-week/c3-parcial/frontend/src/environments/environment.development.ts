// Desarrollo: Angular (localhost:4200) llama a la API (localhost:5142).
// Son orígenes distintos → la API debe permitir este origen con CORS.
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5142/api',
};
