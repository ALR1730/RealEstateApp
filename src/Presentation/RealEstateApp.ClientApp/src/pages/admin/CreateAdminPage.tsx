import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { adminService } from '../../api/services';
import { getApiErrorMessage } from '../../utils/formatters';
import { 
  ShieldCheck, 
  ArrowLeft, 
  CheckCircle2, 
  AlertCircle 
} from 'lucide-react';
import { useLanguage } from '../../context/LanguageContext';

export const CreateAdminPage: React.FC = () => {
  const { t } = useLanguage();
  const navigate = useNavigate();

  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    userName: '',
    email: '',
    password: '',
    confirmPassword: '',
  });

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (form.password !== form.confirmPassword) {
      setError(t('admin.admins.passwordMismatch', "Las contraseñas no coinciden."));
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);
      await adminService.createAdmin({
        firstName: form.firstName,
        lastName: form.lastName,
        userName: form.userName,
        email: form.email,
        password: form.password,
        confirmPassword: form.confirmPassword,
      });

      setSuccess(true);
      setTimeout(() => {
        navigate('/admin/admins');
      }, 1500);
    } catch (err: unknown) {
      console.error("Error creating admin:", err);
      setError(getApiErrorMessage(err, t('admin.common.error', "Error al crear el administrador.")));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      
      <div className="flex items-center gap-4 pb-4 border-b border-slate-200 dark:border-slate-800">
        <button
          onClick={() => navigate('/admin/admins')}
          className="p-2 rounded-xl hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-500 dark:text-slate-400 hover:text-slate-900 dark:hover:text-white transition-colors cursor-pointer"
        >
          <ArrowLeft className="w-5 h-5" />
        </button>
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 dark:text-white tracking-tight flex items-center gap-2">
            <ShieldCheck className="w-6 h-6 text-royal-600 dark:text-royal-400" />
            {t('admin.admins.createTitle', "Crear Nuevo Administrador")}
          </h2>
          <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">
            {t('admin.admins.createSubtitle', "Registra una nueva cuenta de administrador para el panel de control.")}
          </p>
        </div>
      </div>

      {success ? (
        <div className="bg-white dark:bg-slate-900 p-8 rounded-3xl border border-slate-200 dark:border-slate-800 text-center space-y-3 shadow-xl">
          <CheckCircle2 className="w-14 h-14 text-emerald-500 mx-auto" />
          <h3 className="text-xl font-bold text-slate-900 dark:text-white">{t('admin.admins.createdSuccess', "¡Administrador Creado!")}</h3>
          <p className="text-xs text-slate-500 dark:text-slate-400">
            {t('admin.admins.redirecting', "Redirigiendo al listado de administradores...")}
          </p>
        </div>
      ) : (
        <form onSubmit={handleSubmit} className="bg-white dark:bg-slate-900 p-6 sm:p-8 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-sm space-y-4 max-w-lg">
          
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-bold text-slate-700 dark:text-slate-300 uppercase tracking-wider mb-1">{t('admin.admins.firstName', "Nombre")} *</label>
              <input
                type="text"
                name="firstName"
                required
                placeholder="Juan"
                value={form.firstName}
                onChange={handleChange}
                className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 dark:border-slate-700 focus:ring-2 focus:ring-brand-500 bg-slate-50/50 dark:bg-slate-800 text-slate-900 dark:text-white"
              />
            </div>
            <div>
              <label className="block text-xs font-bold text-slate-700 dark:text-slate-300 uppercase tracking-wider mb-1">{t('admin.admins.lastName', "Apellido")} *</label>
              <input
                type="text"
                name="lastName"
                required
                placeholder="Pérez"
                value={form.lastName}
                onChange={handleChange}
                className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 dark:border-slate-700 focus:ring-2 focus:ring-brand-500 bg-slate-50/50 dark:bg-slate-800 text-slate-900 dark:text-white"
              />
            </div>
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-700 dark:text-slate-300 uppercase tracking-wider mb-1">{t('admin.admins.userName', "Nombre de Usuario")} *</label>
            <input
              type="text"
              name="userName"
              required
              placeholder="admin_juan"
              value={form.userName}
              onChange={handleChange}
              className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 dark:border-slate-700 focus:ring-2 focus:ring-brand-500 bg-slate-50/50 dark:bg-slate-800 text-slate-900 dark:text-white"
            />
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-700 dark:text-slate-300 uppercase tracking-wider mb-1">{t('admin.admins.email', "Correo Electrónico")} *</label>
            <input
              type="email"
              name="email"
              required
              placeholder="admin@empresa.com"
              value={form.email}
              onChange={handleChange}
              className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 dark:border-slate-700 focus:ring-2 focus:ring-brand-500 bg-slate-50/50 dark:bg-slate-800 text-slate-900 dark:text-white"
            />
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-700 dark:text-slate-300 uppercase tracking-wider mb-1">{t('admin.admins.password', "Contraseña")} *</label>
            <input
              type="password"
              name="password"
              required
              placeholder="••••••••"
              value={form.password}
              onChange={handleChange}
              className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 dark:border-slate-700 focus:ring-2 focus:ring-brand-500 bg-slate-50/50 dark:bg-slate-800 text-slate-900 dark:text-white"
            />
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-700 dark:text-slate-300 uppercase tracking-wider mb-1">{t('admin.admins.confirmPassword', "Confirmar Contraseña")} *</label>
            <input
              type="password"
              name="confirmPassword"
              required
              placeholder="••••••••"
              value={form.confirmPassword}
              onChange={handleChange}
              className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 dark:border-slate-700 focus:ring-2 focus:ring-brand-500 bg-slate-50/50 dark:bg-slate-800 text-slate-900 dark:text-white"
            />
          </div>

          {error && (
            <div className="flex items-center gap-2 p-3 bg-rose-50 dark:bg-rose-950/40 rounded-xl border border-rose-200 dark:border-rose-900/50">
              <AlertCircle className="w-4 h-4 text-rose-600 dark:text-rose-400 shrink-0" />
              <p className="text-xs text-rose-600 dark:text-rose-400 font-semibold">{error}</p>
            </div>
          )}

          <div className="pt-3 flex justify-end gap-2">
            <button
              type="button"
              onClick={() => navigate('/admin/admins')}
              className="px-4 py-2 rounded-xl text-xs font-bold text-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 cursor-pointer"
            >
              {t('admin.common.cancel', "Cancelar")}
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="px-5 py-2 bg-slate-900 hover:bg-slate-800 dark:bg-brand-600 dark:hover:bg-brand-700 disabled:opacity-50 text-white font-extrabold text-xs rounded-xl shadow-md cursor-pointer"
            >
              {isSubmitting ? t('admin.admins.creating', 'Creando...') : t('admin.admins.btnCreate', 'Crear Administrador')}
            </button>
          </div>
        </form>
      )}
    </div>
  );
};
