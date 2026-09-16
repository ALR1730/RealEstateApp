import React from 'react';
import { Link } from 'react-router-dom';
import { MailCheck, ArrowLeft } from 'lucide-react';

export const PendingActivationPage: React.FC = () => {
  return (
    <div className="min-h-[60vh] flex flex-col items-center justify-center text-center px-4 py-20 space-y-4">
      <div className="w-16 h-16 rounded-full bg-emerald-100 flex items-center justify-center">
        <MailCheck className="w-8 h-8 text-emerald-600" />
      </div>
      <h2 className="text-2xl font-extrabold text-slate-900">Cuenta creada</h2>
      <p className="text-xs sm:text-sm text-slate-500 max-w-sm leading-relaxed">
        Tu cuenta ha sido creada exitosamente. Revisa tu correo electrónico para confirmar tu cuenta.
      </p>
      <Link
        to="/login"
        className="inline-flex items-center gap-2 px-5 py-2.5 bg-brand-600 hover:bg-brand-700 text-white font-bold text-xs rounded-xl shadow-md transition-all"
      >
        <ArrowLeft className="w-4 h-4" />
        <span>Volver al Login</span>
      </Link>
    </div>
  );
};
