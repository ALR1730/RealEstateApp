import React, { useState, useEffect } from 'react';
import { appointmentsService } from '../../api/services';
import { Appointment } from '../../types';
import { Loader } from '../../components/common/Loader';
import { AppointmentCard } from '../../components/appointments/AppointmentCard';
import { Calendar } from 'lucide-react';

export const AgentAppointmentsPage: React.FC = () => {
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const loadAppointments = async () => {
    try {
      setIsLoading(true);
      const data = await appointmentsService.getMyAppointments();
      setAppointments(data || []);
    } catch (err) {
      console.error("Error loading agent appointments:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadAppointments();
  }, []);

  const handleConfirm = async (id: number) => {
    try {
      await appointmentsService.confirmAppointment(id, "Cita confirmada por el agente.");
      loadAppointments();
    } catch (err) {
      console.error("Error confirming appointment:", err);
    }
  };

  const handleCancel = async (id: number) => {
    if (!window.confirm("¿Seguro que deseas cancelar esta cita?")) return;
    try {
      await appointmentsService.cancelAppointment(id, "Cancelada por el agente.");
      loadAppointments();
    } catch (err) {
      console.error("Error cancelling appointment:", err);
    }
  };

  const handleComplete = async (id: number) => {
    try {
      await appointmentsService.confirmAppointment(id, "Cita completada por el agente.");
      loadAppointments();
    } catch (err) {
      console.error("Error completing appointment:", err);
    }
  };

  return (
    <div className="space-y-6">
      
      <div className="flex justify-between items-center pb-4 border-b border-slate-200">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <Calendar className="w-6 h-6 text-royal-600" />
            Gestión de Citas y Visitas Recibidas
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Confirma o reprograma visitas inmobiliarias solicitadas por los clientes compradores.
          </p>
        </div>
      </div>

      {isLoading ? (
        <Loader text="Consultando tus citas asignadas..." />
      ) : appointments.length === 0 ? (
        <div className="bg-white rounded-3xl p-12 text-center border border-slate-200 space-y-4">
          <Calendar className="w-12 h-12 text-slate-300 mx-auto" />
          <h3 className="text-base font-bold text-slate-800">No tienes visitas programadas</h3>
          <p className="text-xs text-slate-500 max-w-sm mx-auto">
            Las solicitudes de visita que agenden los clientes en tus propiedades se listarán aquí.
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {appointments.map((appt) => (
            <AppointmentCard
              key={appt.id}
              appointment={appt}
              onConfirm={handleConfirm}
              onCancel={handleCancel}
              onComplete={handleComplete}
            />
          ))}
        </div>
      )}
    </div>
  );
};
