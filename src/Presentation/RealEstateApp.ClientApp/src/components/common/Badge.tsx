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
    success: 'bg-emerald-50 text-emerald-700 border-emerald-200/60',
    warning: 'bg-amber-50 text-amber-700 border-amber-200/60',
    danger: 'bg-rose-50 text-rose-700 border-rose-200/60',
    info: 'bg-sky-50 text-sky-700 border-sky-200/60',
    neutral: 'bg-slate-100 text-slate-700 border-slate-200',
  }[v || 'neutral'];

  const sizeStyles = {
    sm: 'text-[11px] px-2.5 py-0.5 font-bold',
    md: 'text-xs px-3 py-1 font-extrabold',
  }[size];

  return (
    <span className={`inline-flex items-center gap-1 rounded-full border tracking-wide uppercase shadow-2xs ${colorStyles} ${sizeStyles}`}>
      <span className="w-1.5 h-1.5 rounded-full bg-current opacity-75" />
      {text}
    </span>
  );
};
