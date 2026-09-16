import React from 'react';

interface BadgeProps {
  status?: string;
  children?: React.ReactNode;
  variant?: 'success' | 'warning' | 'danger' | 'info' | 'neutral';
  size?: 'sm' | 'md';
}

export const Badge: React.FC<BadgeProps> = ({ status, children, variant, size = 'sm' }) => {
  let v = variant;
  let text = children || status;

  if (status) {
    switch (status.toLowerCase()) {
      case 'available':
      case 'disponible':
      case 'accepted':
      case 'aceptada':
      case 'confirmed':
      case 'confirmada':
      case 'active':
      case 'activo':
      case 'paid':
      case 'pagada':
        v = 'success';
        text = text || (status === 'Available' ? 'Disponible' : status);
        break;
      case 'reserved':
      case 'reservada':
      case 'pending':
      case 'pendiente':
      case 'counteroffered':
        v = 'warning';
        text = text || (status === 'Reserved' ? 'Reservada' : status);
        break;
      case 'sold':
      case 'vendida':
      case 'rejected':
      case 'rechazada':
      case 'cancelled':
      case 'cancelada':
      case 'inactive':
      case 'inactivo':
        v = 'danger';
        text = text || (status === 'Sold' ? 'Vendida' : status);
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
