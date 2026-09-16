import React, { useState, useEffect, useCallback } from 'react';
import FullCalendar from '@fullcalendar/react';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';
import esLocale from '@fullcalendar/core/locales/es';
import { appointmentsService } from '../../api/services';
import { Appointment } from '../../types';
import { Loader } from '../../components/common/Loader';
import { AppointmentCard } from '../../components/appointments/AppointmentCard';
import { Calendar, CalendarDays, ListChecks, X } from 'lucide-react';

interface CalendarEvent {
  id: string;
  title: string;
  start: string;
  end?: string;
  allDay?: boolean;
  backgroundColor: string;
  borderColor: string;
  textColor: string;
  extendedProps: { appointment: Appointment };
}

const STATUS_STYLE: Record<string, { bg: string; border: string; text: string }> = {
  Pending: { bg: '#f59e0b', border: '#d97706', text: '#ffffff' },
  Confirmed: { bg: '#10b981', border: '#059669', text: '#ffffff' },
  Completed: { bg: '#64748b', border: '#475569', text: '#ffffff' },
  Cancelled: { bg: '#e11d48', border: '#be123c', text: '#ffffff' },
};

function parseSlotTime(timeSlot: string, isStart: boolean): string | null {
  const match = timeSlot.match(/(\d{1,2}):(\d{2})\s*([AP]M)/gi);
  if (!match || match.length < 2) return null;
  const raw = isStart ? match[0] : match[match.length - 1];
  const parts = raw.match(/(\d{1,2}):(\d{2})\s*([AP]M)/i);
  if (!parts) return null;
  let hours = parseInt(parts[1], 10);
  const minutes = parseInt(parts[2], 10);
  const ampm = parts[3].toUpperCase();
  if (ampm === 'PM' && hours !== 12) hours += 12;
  if (ampm === 'AM' && hours === 12) hours = 0;
  return `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:00`;
}

function buildIsoDateTime(date: string, time: string): string {
  const day = (date || '').slice(0, 10);
  return `${day}T${time}`;
}

function toCalendarEvents(appointments: Appointment[]): CalendarEvent[] {
  return appointments.map((appt) => {
    const startTime = parseSlotTime(appt.timeSlot, true);
    const endTime = parseSlotTime(appt.timeSlot, false);
    const style = STATUS_STYLE[appt.status] || STATUS_STYLE.Pending;
    return {
      id: String(appt.id),
      title: `${appt.clientName || 'Cliente'} · ${appt.propertyCode || `#${appt.propertyId}`}`,
      start: startTime ? buildIsoDateTime(appt.date, startTime) : (appt.date as string),
      end: endTime && startTime ? buildIsoDateTime(appt.date, endTime) : undefined,
      backgroundColor: style.bg,
      borderColor: style.border,
      textColor: style.text,
      extendedProps: { appointment: appt },
    };
  });
}

export const AgentAppointmentsPage: React.FC = () => {
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [view, setView] = useState<'calendar' | 'list'>('calendar');
  const [selected, setSelected] = useState<Appointment | null>(null);

  const loadAppointments = useCallback(async () => {
    try {
      const data = await appointmentsService.getMyAppointments();
      setAppointments(data || []);
    } catch (err) {
      console.error("Error loading agent appointments:", err);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    Promise.resolve().then(() => loadAppointments()).catch(console.error);
  }, [loadAppointments]);

  const handleConfirm = async (id: number) => {
    try {
      await appointmentsService.confirmAppointment(id, "Cita confirmada por el agente.");
      setSelected(null);
      loadAppointments();
    } catch (err) {
      console.error("Error confirming appointment:", err);
    }
  };

  const handleCancel = async (id: number) => {
    if (!window.confirm("¿Seguro que deseas cancelar esta cita?")) return;
    try {
      await appointmentsService.cancelAppointment(id, "Cancelada por el agente.");
      setSelected(null);
      loadAppointments();
    } catch (err) {
      console.error("Error cancelling appointment:", err);
    }
  };

  const handleComplete = async (id: number) => {
    try {
      await appointmentsService.confirmAppointment(id, "Cita completada por el agente.");
      setSelected(null);
      loadAppointments();
    } catch (err) {
      console.error("Error completing appointment:", err);
    }
  };

  const handleEventClick = (arg: any) => {
    setSelected(arg.event.extendedProps.appointment as Appointment);
  };

  const events = toCalendarEvents(appointments);

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3 pb-4 border-b border-slate-200">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <Calendar className="w-6 h-6 text-royal-600" />
            Gestión de Citas y Visitas Recibidas
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Visualiza en calendario y gestiona las visitas solicitadas por los clientes compradores.
          </p>
        </div>
        <div className="flex items-center gap-1 bg-white border border-slate-200 rounded-xl p-1 shadow-xs">
          <button
            onClick={() => { setView('calendar'); setSelected(null); }}
            className={`px-3 py-1.5 text-xs font-bold rounded-lg transition-colors flex items-center gap-1.5 ${
              view === 'calendar' ? 'bg-royal-600 text-white' : 'text-slate-500 hover:bg-slate-100'
            }`}
          >
            <CalendarDays className="w-3.5 h-3.5" />
            Calendario
          </button>
          <button
            onClick={() => { setView('list'); setSelected(null); }}
            className={`px-3 py-1.5 text-xs font-bold rounded-lg transition-colors flex items-center gap-1.5 ${
              view === 'list' ? 'bg-royal-600 text-white' : 'text-slate-500 hover:bg-slate-100'
            }`}
          >
            <ListChecks className="w-3.5 h-3.5" />
            Lista
          </button>
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
        <>
          {view === 'calendar' && (
            <div className="bg-white rounded-3xl border border-slate-200 shadow-xs p-4 sm:p-6">
              <FullCalendar
                plugins={[dayGridPlugin, timeGridPlugin, interactionPlugin]}
                initialView="dayGridMonth"
                locale={esLocale}
                headerToolbar={{
                  left: 'prev,next today',
                  center: 'title',
                  right: 'dayGridMonth,timeGridWeek,timeGridDay',
                }}
                buttonText={{
                  today: 'Hoy',
                  month: 'Mes',
                  week: 'Semana',
                  day: 'Día',
                }}
                height="auto"
                events={events}
                eventClick={handleEventClick}
                eventTimeFormat={{ hour: '2-digit', minute: '2-digit', meridiem: 'short' }}
                slotMinTime="08:00:00"
                slotMaxTime="20:00:00"
                allDaySlot={false}
                nowIndicator
                dayMaxEvents={3}
              />

              {selected && (
                <div className="mt-5 border-t border-slate-200 pt-5">
                  <div className="flex items-center justify-between mb-3">
                    <h3 className="text-sm font-bold text-slate-800">Detalle de la visita seleccionada</h3>
                    <button
                      onClick={() => setSelected(null)}
                      className="p-1.5 text-slate-400 hover:text-slate-600 hover:bg-slate-100 rounded-lg transition-colors"
                      title="Cerrar"
                    >
                      <X className="w-4 h-4" />
                    </button>
                  </div>
                  <AppointmentCard
                    appointment={selected}
                    onConfirm={handleConfirm}
                    onCancel={handleCancel}
                    onComplete={handleComplete}
                  />
                </div>
              )}
            </div>
          )}

          {view === 'list' && (
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
        </>
      )}
    </div>
  );
};