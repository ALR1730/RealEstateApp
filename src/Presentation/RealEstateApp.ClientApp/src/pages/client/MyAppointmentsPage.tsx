import React, { useState, useEffect } from 'react';
import { appointmentsService } from '../../api/services';
import { Appointment } from '../../types';
import { formatDate } from '../../utils/formatters';
import { Badge } from '../../components/common/Badge';
import { Loader } from '../../components/common/Loader';
import { Calendar, Clock, XCircle, ExternalLink } from 'lucide-react';
import { Link } from 'react-router-dom';

export const MyAppointmentsPage: React.FC = () => {
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const loadAppointments = async () => {
    try {
      setIsLoading(true);
      const data = await appointmentsService.getMyAppointments();
      setAppointments(data || []);
    } catch (err) {
      console.error("Error loading appointments:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadAppointments();
  }, []);

  const handleCancel = async (id: number) => {
    if (!window.confirm("¿Seguro que deseas cancelar esta cita?")) return;
    try {
      await appointmentsService.cancelAppointment(id, "Cancelado por el cliente");
      loadAppointments();
    } catch (err) {
      console.error("Error cancelling appointment:", err);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center pb-4 border-b border-slate-200">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <Calendar className="w-6 h-6 text-royal-600" />
            Mis Citas de Visita
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Horarios agendados para visitas presenciales con los agentes inmobiliarios.
          </p>
        </div>
      </div>

      {isLoading ? (
        <Loader text="Consultando tus citas agendadas..." />
      ) : appointments.length === 0 ? (
        <div className="bg-white rounded-3xl p-12 text-center border border-slate-200 space-y-4">
          <Calendar className="w-12 h-12 text-slate-300 mx-auto" />
          <h3 className="text-base font-bold text-slate-800">No tienes citas de visita agendadas</h3>
          <p className="text-xs text-slate-500 max-w-sm mx-auto">
            Puedes agendar una visita presencial desde la ficha técnica de cualquier propiedad.
          </p>
          <Link
            to="/catalog"
            className="inline-block px-5 py-2.5 bg-brand-600 hover:bg-brand-700 text-white font-bold text-xs rounded-xl shadow-md"
          >
            Explorar Catálogo
          </Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {appointments.map((appt) => (
            <div key={appt.id} className="bg-white rounded-3xl p-6 border border-slate-200 shadow-xs space-y-4">
              <div className="flex justify-between items-start">
                <div>
                  <span className="font-bold text-sm text-slate-900">Propiedad #{appt.propertyCode || appt.propertyId}</span>
                  <p className="text-xs text-slate-500 truncate max-w-xs">{appt.propertyDescription}</p>
                </div>
                <Badge status={appt.status} />
              </div>

              <div className="p-3.5 bg-slate-50 rounded-2xl space-y-2 text-xs">
                <div className="flex items-center gap-2 font-semibold text-slate-700">
                  <Calendar className="w-4 h-4 text-royal-600" />
                  <span>Fecha: {formatDate(appt.date)}</span>
                </div>
                <div className="flex items-center gap-2 font-semibold text-slate-700">
                  <Clock className="w-4 h-4 text-royal-600" />
                  <span>Horario: {appt.timeSlot}</span>
                </div>
                {appt.clientNotes && (
                  <p className="text-[11px] text-slate-500 pt-1 border-t border-slate-200/60">
                    Notas: "{appt.clientNotes}"
                  </p>
                )}
              </div>

              <div className="flex items-center justify-between pt-2">
                <Link
                  to={`/property/${appt.propertyId}`}
                  className="inline-flex items-center gap-1 text-xs font-bold text-brand-600 hover:text-brand-700"
                >
                  <span>Ver Propiedad</span>
                  <ExternalLink className="w-3.5 h-3.5" />
                </Link>

                {appt.status !== 'Cancelled' && (
                  <button
                    onClick={() => handleCancel(appt.id)}
                    className="flex items-center gap-1 px-3 py-1.5 text-rose-600 hover:bg-rose-50 rounded-lg text-xs font-bold transition-colors"
                  >
                    <XCircle className="w-4 h-4" />
                    <span>Cancelar Cita</span>
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
