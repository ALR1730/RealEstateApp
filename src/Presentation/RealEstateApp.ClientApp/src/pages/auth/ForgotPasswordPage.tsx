import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { authService } from '../../api/services';
import { KeyRound, ArrowLeft, Mail, CheckCircle2 } from 'lucide-react';

export const ForgotPasswordPage: React.FC = () => {
  const [email, setEmail] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSubmitted, setIsSubmitted] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email) return;

    try {
      setIsSubmitting(true);
      setError(null);
      await authService.forgotPassword(email);
      setIsSubmitted(true);
    } catch (err: any) {
      console.error("Error sending reset password link:", err);
      setError(err.response?.data?.error || "Error al solicitar restablecimiento de contraseña.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-[80vh] flex items-center justify-center px-4 py-12">
      <div className="max-w-md w-full bg-white rounded-3xl p-8 border border-slate-200 shadow-xl space-y-6">
        
        <div className="text-center space-y-2">
          <div className="w-12 h-12 rounded-2xl bg-brand-50 text-brand-600 flex items-center justify-center mx-auto">
            <KeyRound className="w-6 h-6" />
          </div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight">
            ¿Olvidaste tu contraseña?
          </h2>
          <p className="text-xs text-slate-500">
            Ingresa tu correo electrónico registrado y te enviaremos un enlace para restablecerla.
          </p>
        </div>

        {isSubmitted ? (
          <div className="p-6 bg-emerald-50 rounded-2xl border border-emerald-200 text-center space-y-3">
            <CheckCircle2 className="w-8 h-8 text-emerald-600 mx-auto" />
            <h4 className="text-sm font-extrabold text-emerald-900">Enlace Enviado</h4>
            <p className="text-xs text-emerald-700">
              Si tu correo coincide con una cuenta activa, recibirás instrucciones para restablecer tu contraseña.
            </p>
            <Link
              to="/login"
              className="inline-block mt-2 text-xs font-bold text-emerald-800 underline"
            >
              Volver a Iniciar Sesión
            </Link>
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase mb-1">
                Correo Electrónico
              </label>
              <div className="relative">
                <Mail className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  type="email"
                  required
                  placeholder="ejemplo@correo.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  className="w-full pl-9 pr-3.5 py-2.5 text-xs rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500"
                />
              </div>
            </div>

            {error && <p className="text-xs font-bold text-rose-600 p-2.5 bg-rose-50 rounded-xl">{error}</p>}

            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full py-3 bg-brand-600 hover:bg-brand-700 text-white rounded-xl font-extrabold text-xs shadow-md transition-all disabled:opacity-50"
            >
              {isSubmitting ? 'Enviando enlace...' : 'Enviar Instrucciones'}
            </button>

            <div className="text-center pt-2">
              <Link
                to="/login"
                className="inline-flex items-center gap-1 text-xs font-bold text-slate-500 hover:text-slate-900"
              >
                <ArrowLeft className="w-3.5 h-3.5" />
                <span>Volver al Login</span>
              </Link>
            </div>
          </form>
        )}
      </div>
    </div>
  );
};
