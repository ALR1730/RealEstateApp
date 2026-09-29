import React, { useState } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { authService } from '../../api/services';
import { Lock, CheckCircle2 } from 'lucide-react';

export const ResetPasswordPage: React.FC = () => {
  const [searchParams] = useSearchParams();

  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const email = searchParams.get('email') || '';
  const token = searchParams.get('token') || '';

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!password || password !== confirmPassword) {
      setError("Las contraseñas no coinciden.");
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);
      await authService.resetPassword({
        email,
        token,
        password,
        confirmPassword,
      });
      setIsSuccess(true);
    } catch (err: unknown) {
      console.error("Error resetting password:", err);
      const apiError = (err as { response?: { data?: { error?: string } } })?.response?.data?.error;
      setError(apiError || "Error al restablecer la contraseña.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-[80vh] flex items-center justify-center px-4 py-12">
      <div className="max-w-md w-full bg-white dark:bg-slate-900 rounded-3xl p-8 border border-slate-200 dark:border-slate-800 shadow-xl space-y-6">
        
        <div className="text-center space-y-2">
          <div className="w-12 h-12 rounded-2xl bg-brand-50 dark:bg-slate-800 text-brand-600 dark:text-brand-400 flex items-center justify-center mx-auto border border-transparent dark:border-slate-700">
            <Lock className="w-6 h-6" />
          </div>
          <h2 className="text-2xl font-extrabold text-slate-900 dark:text-white tracking-tight">
            Restablecer Contraseña
          </h2>
          <p className="text-xs text-slate-500 dark:text-slate-400">
            Crea una nueva contraseña segura para tu cuenta.
          </p>
        </div>

        {isSuccess ? (
          <div className="p-6 bg-emerald-50 dark:bg-emerald-950/40 rounded-2xl border border-emerald-200 dark:border-emerald-800/60 text-center space-y-3">
            <CheckCircle2 className="w-8 h-8 text-emerald-600 dark:text-emerald-400 mx-auto" />
            <h4 className="text-sm font-extrabold text-emerald-900 dark:text-emerald-300">¡Contraseña Actualizada!</h4>
            <p className="text-xs text-emerald-700 dark:text-emerald-400">
              Tu contraseña ha sido cambiada exitosamente. Ya puedes iniciar sesión.
            </p>
            <Link
              to="/login"
              className="inline-block mt-2 px-6 py-2.5 bg-brand-600 text-white font-extrabold text-xs rounded-xl shadow-md cursor-pointer"
            >
              Iniciar Sesión Ahora
            </Link>
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-xs font-bold text-slate-700 dark:text-slate-300 uppercase mb-1">
                Nueva Contraseña *
              </label>
              <input
                type="password"
                required
                minLength={8}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="w-full px-3.5 py-2.5 text-xs rounded-xl border border-slate-200 dark:border-slate-700 focus:ring-2 focus:ring-brand-500 bg-white dark:bg-slate-800 text-slate-900 dark:text-white"
              />
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-700 dark:text-slate-300 uppercase mb-1">
                Confirmar Contraseña *
              </label>
              <input
                type="password"
                required
                minLength={8}
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                className="w-full px-3.5 py-2.5 text-xs rounded-xl border border-slate-200 dark:border-slate-700 focus:ring-2 focus:ring-brand-500 bg-white dark:bg-slate-800 text-slate-900 dark:text-white"
              />
            </div>

            {error && <p className="text-xs font-bold text-rose-600 dark:text-rose-400 p-2.5 bg-rose-50 dark:bg-rose-950/40 border border-transparent dark:border-rose-900/50 rounded-xl">{error}</p>}

            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full py-3 bg-brand-600 hover:bg-brand-700 text-white rounded-xl font-extrabold text-xs shadow-md transition-all disabled:opacity-50 cursor-pointer"
            >
              {isSubmitting ? 'Guardando...' : 'Cambiar Contraseña'}
            </button>
          </form>
        )}
      </div>
    </div>
  );
};
