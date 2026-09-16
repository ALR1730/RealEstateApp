import React from 'react';
import { Loader2 } from 'lucide-react';

interface LoaderProps {
  text?: string;
  size?: 'sm' | 'md' | 'lg';
}

export const Loader: React.FC<LoaderProps> = ({ text = 'Cargando información...', size = 'md' }) => {
  const iconSize = {
    sm: 'w-5 h-5',
    md: 'w-8 h-8',
    lg: 'w-12 h-12',
  }[size];

  return (
    <div className="flex flex-col items-center justify-center py-12 px-4 space-y-3">
      <Loader2 className={`${iconSize} animate-spin text-brand-600`} />
      {text && <p className="text-xs sm:text-sm font-semibold text-slate-500">{text}</p>}
    </div>
  );
};
