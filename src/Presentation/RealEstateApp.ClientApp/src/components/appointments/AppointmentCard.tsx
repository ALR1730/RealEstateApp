import React from 'react';
import { Link } from 'react-router-dom';
import { Appointment } from '../../types';
import { formatDate } from '../../utils/formatters';
import { Badge } from '../common/Badge';
import { Calendar, Clock, Check, X, User, Phone, Mail } from 'lucide-react';

interface AppointmentCardProps {
  appointment: Appointment;
  onConfirm?: (id: number) => void;
  onCancel?: (id: number) => void;
  onComplete?: (id: number) => void;
}

export const AppointmentCard: React.FC<AppointmentCardProps> = ({
  appointment,
  onConfirm,
  onCancel,
  onComplete,
}) => {
  const appt = appointment;

  return (
    <div className="bg-white dark:bg-slate-900 rounded-3xl p-6 border border-slate-200 dark:border-slate-800 shadow-xs space-y-4">
      <div className="flex justify-between items-start">
        <div>
          <span className="font-bold text-sm text-slate-900 dark:text-white">
            Propiedad #{appt.propertyCode || appt.propertyId}
          </span>
          <p className="text-xs text-slate-500 dark:text-slate-400 truncate max-w-xs">
            {appt.propertyDescription}
          </p>
        </div>
        <Badge status={appt.status} />
      </div>

      {/* Client Info */}
      <div className="p-3.5 bg-slate-50 dark:bg-slate-800/60 rounded-2xl border border-slate-100 dark:border-slate-700 space-y-1.5 text-xs text-slate-700 dark:text-slate-300">
        <div className="flex items-center gap-2 font-bold text-slate-900 dark:text-white">
          <User className="w-4 h-4 text-slate-400" />
          <span>{appt.clientName || 'Cliente Comprador'}</span>
        </div>
        {appt.clientPhone && (
          <div className="flex items-center gap-2 text-slate-600 dark:text-slate-400">
            <Phone className="w-4 h-4 text-brand-600 dark:text-brand-400" />
            <span>{appt.clientPhone}</span>
          </div>
        )}
        {appt.clientEmail && (
          <div className="flex items-center gap-2 text-slate-600 dark:text-slate-400">
            <Mail className="w-4 h-4 text-brand-600 dark:text-brand-400" />
            <span>{appt.clientEmail}</span>
          </div>
        )}
      </div>

      {/* Schedule Info */}
      <div className="flex items-center justify-between text-xs font-semibold text-slate-700 dark:text-slate-300 px-1">
        <div className="flex items-center gap-1.5">
          <Calendar className="w-4 h-4 text-royal-600 dark:text-indigo-400" />
          <span>{formatDate(appt.date)}</span>
        </div>
        <div className="flex items-center gap-1.5">
          <Clock className="w-4 h-4 text-royal-600 dark:text-indigo-400" />
          <span>{appt.timeSlot}</span>
        </div>
      </div>

      {/* Actions */}
      <div className="pt-2 border-t border-slate-100 dark:border-slate-800 flex items-center justify-between">
        <Link
          to={`/property/${appt.propertyId}`}
          className="text-xs font-bold text-brand-600 dark:text-brand-400 hover:text-brand-700 dark:hover:text-brand-300"
        >
          Ver Ficha Técnica
        </Link>

        <div className="flex gap-2">
          {appt.status === 'Pending' && onConfirm && (
            <button
              onClick={() => onConfirm(appt.id)}
              className="flex items-center gap-1 px-3 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl font-bold text-xs shadow-xs cursor-pointer"
            >
              <Check className="w-3.5 h-3.5" />
              <span>Confirmar Cita</span>
            </button>
          )}
          {appt.status === 'Confirmed' && onComplete && (
            <button
              onClick={() => onComplete(appt.id)}
              className="flex items-center gap-1 px-3 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl font-bold text-xs shadow-xs cursor-pointer"
            >
              <Check className="w-3.5 h-3.5" />
              <span>Completar</span>
            </button>
          )}
          {appt.status !== 'Cancelled' && appt.status !== 'Completed' && onCancel && (
            <button
              onClick={() => onCancel(appt.id)}
              className="flex items-center gap-1 px-3 py-1.5 bg-rose-50 dark:bg-rose-950/40 hover:bg-rose-100 dark:hover:bg-rose-900/50 text-rose-700 dark:text-rose-400 rounded-xl font-bold text-xs border border-rose-200 dark:border-rose-800 cursor-pointer"
            >
              <X className="w-3.5 h-3.5" />
              <span>Cancelar</span>
            </button>
          )}
        </div>
      </div>
    </div>
  );
};
