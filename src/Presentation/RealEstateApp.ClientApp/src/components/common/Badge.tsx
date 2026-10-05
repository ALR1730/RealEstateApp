import React from 'react';
import { useLanguage } from '../../context/LanguageContext';

interface BadgeProps {
  status?: string;
  children?: React.ReactNode;
  variant?: 'success' | 'warning' | 'danger' | 'info' | 'neutral';
  size?: 'sm' | 'md';
}

export const Badge: React.FC<BadgeProps> = ({ status, children, variant, size = 'sm' }) => {
  const { t } = useLanguage();
  let v = variant;
  let text = children || status;

  if (status) {
    switch (status.toLowerCase()) {
      case 'available':
      case 'disponible':
        v = 'success';
        text = children || t('status.available', 'Disponible');
        break;
      case 'accepted':
      case 'aceptada':
        v = 'success';
        text = children || t('status.accepted', 'Aceptada');
        break;
      case 'confirmed':
      case 'confirmada':
        v = 'success';
        text = children || t('status.confirmed', 'Confirmada');
        break;
      case 'completed':
      case 'completada':
        v = 'success';
        text = children || t('status.completed', 'Completada');
        break;
      case 'active':
      case 'activo':
        v = 'success';
        text = children || (status.toLowerCase() === 'activo' || status.toLowerCase() === 'active' ? t('common.yes', 'Activo') : status);
        break;
      case 'paid':
      case 'pagada':
        v = 'success';
        text = children || t('status.paid', 'Pagada');
        break;
      case 'reserved':
      case 'reservada':
        v = 'warning';
        text = children || t('status.reserved', 'Reservada');
        break;
      case 'pending':
      case 'pendiente':
        v = 'warning';
        text = children || t('status.pending', 'Pendiente');
        break;
      case 'counteroffered':
      case 'contraofertada':
        v = 'warning';
        text = children || t('status.counteroffered', 'Contraofertada');
        break;
      case 'sold':
      case 'vendida':
        v = 'danger';
        text = children || t('status.sold', 'Vendida');
        break;
      case 'rejected':
      case 'rechazada':
        v = 'danger';
        text = children || t('status.rejected', 'Rechazada');
        break;
      case 'cancelled':
      case 'cancelada':
        v = 'danger';
        text = children || t('status.cancelled', 'Cancelada');
        break;
      case 'inactive':
      case 'inactivo':
        v = 'danger';
        text = children || (status.toLowerCase() === 'inactivo' || status.toLowerCase() === 'inactive' ? t('common.no', 'Inactivo') : status);
        break;
      default:
        v = 'neutral';
    }
  }

  const colorStyles = {
    success: 'bg-emerald-50 dark:bg-emerald-950/40 text-emerald-700 dark:text-emerald-300 border-emerald-200/60 dark:border-emerald-800/60',
    warning: 'bg-amber-50 dark:bg-amber-950/40 text-amber-700 dark:text-amber-300 border-amber-200/60 dark:border-amber-800/60',
    danger: 'bg-rose-50 dark:bg-rose-950/40 text-rose-700 dark:text-rose-300 border-rose-200/60 dark:border-rose-800/60',
    info: 'bg-sky-50 dark:bg-sky-950/40 text-sky-700 dark:text-sky-300 border-sky-200/60 dark:border-sky-800/60',
    neutral: 'bg-slate-100 dark:bg-slate-800 text-slate-700 dark:text-slate-300 border-slate-200 dark:border-slate-700',
  }[v || 'neutral'];

  const sizeStyles = {
    sm: 'text-[11px] px-2.5 py-0.5 font-bold',
    md: 'text-xs px-3 py-1 font-extrabold',
  }[size];

  return (
    <span className={`inline-flex items-center gap-1 rounded-full border tracking-wide uppercase shadow-sm ${colorStyles} ${sizeStyles}`}>
      <span className="w-1.5 h-1.5 rounded-full bg-current opacity-75" />
      {text}
    </span>
  );
};
