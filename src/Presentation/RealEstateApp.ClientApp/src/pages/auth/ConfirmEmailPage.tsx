import React from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { CheckCircle, XCircle, ArrowLeft } from 'lucide-react';

export const ConfirmEmailPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const email = searchParams.get('email');
  const token = searchParams.get('token');
  const hasParams = !!email && !!token;

  return (
    <div className="min-h-[60vh] flex flex-col items-center justify-center text-center px-4 py-20 space-y-4">
      {hasParams ? (
        <>
          <div className="w-16 h-16 rounded-full bg-emerald-100 flex items-center justify-center">
            <CheckCircle className="w-8 h-8 text-emerald-600" />
          </div>
          <h2 className="text-2xl font-extrabold text-slate-900">Correo confirmado</h2>
          <p className="text-xs sm:text-sm text-slate-500 max-w-sm leading-relaxed">
            Tu correo electrónico ha sido confirmado exitosamente. Ya puedes iniciar sesión en tu cuenta.
          </p>
        </>
      ) : (
        <>
          <div className="w-16 h-16 rounded-full bg-rose-100 flex items-center justify-center">
            <XCircle className="w-8 h-8 text-rose-600" />
          </div>
          <h2 className="text-2xl font-extrabold text-slate-900">Error de confirmación</h2>
          <p className="text-xs sm:text-sm text-slate-500 max-w-sm leading-relaxed">
            No se pudo confirmar tu correo electrónico. El enlace puede haber expirado o ser inválido. Solicita uno nuevo desde la página de inicio de sesión.
          </p>
        </>
      )}
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
