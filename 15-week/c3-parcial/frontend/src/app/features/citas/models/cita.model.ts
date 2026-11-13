export type EstadoCita = 'Pendiente' | 'Atendida' | 'Cancelada';

export interface Cita {
  id: number;
  /** Fecha y hora local de la cita (ISO sin zona, ej. 2026-11-14T09:00:00). */
  fechaHora: string;
  motivo: string;
  estado: EstadoCita;
  medicoId: number;
  medico: string;
  especialidad: string;
  pacienteId: number;
  paciente: string;
}

export interface CrearCitaRequest {
  medicoId: number;
  fechaHora: string;
  motivo: string;
}
