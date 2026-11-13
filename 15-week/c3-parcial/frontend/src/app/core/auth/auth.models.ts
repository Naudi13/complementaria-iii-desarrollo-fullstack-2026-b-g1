export type Rol = 'Admin' | 'Medico' | 'Paciente';

export interface Usuario {
  id: number;
  nombre: string;
  email: string;
  rol: Rol;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  tokenType: string;
  expiresAt: string;
  usuario: Usuario;
}
