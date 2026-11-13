export interface Especialidad {
  id: number;
  nombre: string;
}

export interface Medico {
  id: number;
  nombre: string;
  email: string;
  registroProfesional: string;
  especialidadId: number;
  especialidad: string;
}

export interface CrearMedicoRequest {
  nombre: string;
  email: string;
  password: string;
  registroProfesional: string;
  especialidadId: number;
}
