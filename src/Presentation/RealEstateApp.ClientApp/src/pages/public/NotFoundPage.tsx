import React from 'react';
import { Link } from 'react-router-dom';
import { Home } from 'lucide-react';

export const NotFoundPage: React.FC = () => {
  return (
    <div className="min-h-[60vh] flex flex-col items-center justify-center text-center px-4 py-20 space-y-4">
      <h1 className="text-7xl font-extrabold text-slate-900 font-mono">404</h1>
      <h2 className="text-xl sm:text-2xl font-bold text-slate-800">Página No Encontrada</h2>
      <p className="text-xs sm:text-sm text-slate-500 max-w-md">
        La ruta a la que intentas acceder no existe o fue movida.
      </p>
      <div className="pt-4 flex gap-3">
        <Link
          to="/"
          className="flex items-center gap-2 px-5 py-2.5 bg-brand-600 hover:bg-brand-700 text-white font-bold text-xs rounded-xl shadow-md transition-all"
        >
          <Home className="w-4 h-4" />
          <span>Ir al Inicio</span>
        </Link>
      </div>
    </div>
  );
};
