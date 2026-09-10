import React, { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { CheckCircle, XCircle, ArrowLeft, Loader2, MailX } from 'lucide-react';
import { authService } from '../../api/services';

type ConfirmState = 'loading' | 'success' | 'error' | 'missing';

export const ConfirmEmailPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const userId = searchParams.get('userId');
  const token = searchParams.get('token');
  const [state, setState] = useState<ConfirmState>('loading');
  const [message, setMessage] = useState('');

  useEffect(() => {
    let active = true;

    const confirm = async () => {
      if (!userId || !token) {
        if (active) setState('missing');
        return;
      }
      try {
        const res = await authService.confirmEmail(userId, token);
        if (active) {
          setMessage(res?.message || 'Tu correo electrónico ha sido confirmado exitosamente.');
          setState('success');
        }
      } catch (err: any) {
        if (active) {
          setMessage(err.response?.data?.error || err.response?.data?.message || 'No se pudo confirmar tu correo electrónico.');
          setState('error');
        }
      }
    };

    confirm();
    return () => { active = false; };
  }, [userId, token]);

  return (
    <div className="min-h-[60vh] flex flex-col items-center justify-center text-center px-4 py-20 space-y-4">
      {state === 'loading' && (
        <>
          <div className="w-16 h-16 rounded-full bg-brand-100 flex items-center justify-center">
            <Loader2 className="w-8 h-8 text-brand-600 animate-spin" />
          </div>
          <h2 className="text-2xl font-extrabold text-slate-900">Confirmando correo</h2>
          <p className="text-xs sm:text-sm text-slate-500 max-w-sm leading-relaxed">
            Estamos validando tu enlace de activación, espera un momento...
          </p>
        </>
      )}

      {state === 'success' && (
        <>
          <div className="w-16 h-16 rounded-full bg-emerald-100 flex items-center justify-center">
            <CheckCircle className="w-8 h-8 text-emerald-600" />
          </div>
          <h2 className="text-2xl font-extrabold text-slate-900">Correo confirmado</h2>
          <p className="text-xs sm:text-sm text-slate-500 max-w-sm leading-relaxed">
            {message}
          </p>
        </>
      )}

      {state === 'error' && (
        <>
          <div className="w-16 h-16 rounded-full bg-rose-100 flex items-center justify-center">
            <XCircle className="w-8 h-8 text-rose-600" />
          </div>
          <h2 className="text-2xl font-extrabold text-slate-900">Error de confirmación</h2>
          <p className="text-xs sm:text-sm text-slate-500 max-w-sm leading-relaxed">
            {message}
          </p>
        </>
      )}

      {state === 'missing' && (
        <>
          <div className="w-16 h-16 rounded-full bg-amber-100 flex items-center justify-center">
            <MailX className="w-8 h-8 text-amber-600" />
          </div>
          <h2 className="text-2xl font-extrabold text-slate-900">Enlace incompleto</h2>
          <p className="text-xs sm:text-sm text-slate-500 max-w-sm leading-relaxed">
            El enlace de confirmación no es válido. Solicita uno nuevo desde la página de inicio de sesión o vuelve a registrarte.
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