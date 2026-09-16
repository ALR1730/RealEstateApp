import React, { useState } from 'react';
import { Property } from '../../types';
import { appointmentsService } from '../../api/services';
import { Modal } from '../common/Modal';
import { Calendar, Clock, CheckCircle, Send } from 'lucide-react';

interface AppointmentModalProps {
  isOpen: boolean;
  onClose: () => void;
  property: Property;
  onSuccess?: () => void;
}

export const AppointmentModal: React.FC<AppointmentModalProps> = ({
  isOpen,
  onClose,
  property,
  onSuccess,
}) => {
  const [date, setDate] = useState<string>('');
  const [timeSlot, setTimeSlot] = useState<string>('10:00 AM - 11:00 AM');
  const [notes, setNotes] = useState<string>('');
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [success, setSuccess] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const timeSlots = [
    '09:00 AM - 10:00 AM',
    '10:00 AM - 11:00 AM',
    '11:00 AM - 12:00 PM',
    '02:00 PM - 03:00 PM',
    '03:00 PM - 04:00 PM',
    '04:00 PM - 05:00 PM',
  ];

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!date) {
      setError("Por favor seleccione una fecha.");
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);
      await appointmentsService.requestAppointment({
        propertyId: property.id,
        date,
        timeSlot,
        clientNotes: notes,
      });

      setSuccess(true);
      setTimeout(() => {
        onSuccess?.();
        onClose();
        setSuccess(false);
      }, 2000);
    } catch (err: any) {
      console.error("Error booking appointment:", err);
      setError(err.response?.data?.error || "No se pudo agendar la cita. Intente de nuevo.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Agendar Visita Inmobiliaria">
      {success ? (
        <div className="py-8 text-center space-y-3 animate-in zoom-in-95">
          <CheckCircle className="w-12 h-12 text-emerald-500 mx-auto" />
          <h4 className="font-extrabold text-lg text-slate-900">¡Solicitud de Visita Agendada!</h4>
          <p className="text-xs text-slate-500">
            El agente asignado ({property.agentName || 'Corredor'}) revisará tu horario y confirmará la cita.
          </p>
        </div>
      ) : (
        <form onSubmit={handleSubmit} className="space-y-4">
          
          <div className="p-3 bg-slate-50 rounded-2xl border border-slate-100 flex items-center justify-between text-xs">
            <div>
              <span className="font-bold text-slate-800">Propiedad #{property.code}</span>
              <p className="text-slate-500 truncate max-w-xs">{property.description}</p>
            </div>
            <div className="text-right">
              <span className="text-[10px] text-slate-400 uppercase font-bold">Agente</span>
              <p className="font-bold text-brand-600">{property.agentName || 'Agente Inmobiliario'}</p>
            </div>
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5 flex items-center gap-1.5">
              <Calendar className="w-3.5 h-3.5 text-brand-600" />
              Fecha de la Visita
            </label>
            <input
              type="date"
              required
              min={new Date().toISOString().split('T')[0]}
              value={date}
              onChange={(e) => setDate(e.target.value)}
              className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500 bg-white"
            />
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5 flex items-center gap-1.5">
              <Clock className="w-3.5 h-3.5 text-brand-600" />
              Horario Preferido
            </label>
            <select
              value={timeSlot}
              onChange={(e) => setTimeSlot(e.target.value)}
              className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500 bg-white"
            >
              {timeSlots.map((ts) => (
                <option key={ts} value={ts}>
                  {ts}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5">
              Notas Adicionales (Opcional)
            </label>
            <textarea
              rows={2}
              placeholder="Ej: Visita con acompañante, requerimiento de tour virtual previo..."
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500"
            />
          </div>

          {error && <p className="text-xs text-rose-600 font-semibold">{error}</p>}

          <div className="pt-3 flex justify-end gap-2">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-xs font-bold text-slate-600 hover:bg-slate-100 rounded-xl transition-colors"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="flex items-center gap-1.5 px-5 py-2 text-xs font-bold text-white bg-brand-600 hover:bg-brand-700 disabled:opacity-50 rounded-xl shadow-md shadow-brand-600/20 transition-all"
            >
              <Send className="w-3.5 h-3.5" />
              <span>{isSubmitting ? 'Agendando...' : 'Solicitar Cita'}</span>
            </button>
          </div>
        </form>
      )}
    </Modal>
  );
};
