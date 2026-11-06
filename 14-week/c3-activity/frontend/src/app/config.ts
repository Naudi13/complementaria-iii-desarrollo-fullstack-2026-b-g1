// config.ts
// URL base de la API.
//  - Desarrollo (ng serve, puerto 4200): la API vive en otro origen -> http://localhost:5080
//    y por eso el backend habilita CORS para http://localhost:4200.
//  - Producción (Docker): Angular lo sirve el propio backend -> mismo origen, ruta relativa.
import { isDevMode } from '@angular/core';

export const API_URL = isDevMode() ? 'http://localhost:5080/api' : '/api';
