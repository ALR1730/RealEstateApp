import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { adminService } from '../../api/services';
import { 
  ShieldCheck, 
  ArrowLeft, 
  CheckCircle2, 
  AlertCircle 
} from 'lucide-react';

export const CreateAdminPage: React.FC = () => {
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
      setError("Las contraseñas no coinciden.");
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
    } catch (err: any) {
      console.error("Error creating admin:", err);
      setError(err.response?.data?.error || "Error al crear el administrador.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      
      <div className="flex items-center gap-4 pb-4 border-b border-slate-200">
        <button
          onClick={() => navigate('/admin/admins')}
          className="p-2 rounded-xl hover:bg-slate-100 text-slate-500 hover:text-slate-900 transition-colors"
        >
          <ArrowLeft className="w-5 h-5" />
        </button>
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <ShieldCheck className="w-6 h-6 text-royal-600" />
            Crear Nuevo Administrador
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Registra una nueva cuenta de administrador para el panel de control.
          </p>
        </div>
      </div>

      {success ? (
        <div className="bg-white p-8 rounded-3xl border border-slate-200 text-center space-y-3 shadow-xl">
          <CheckCircle2 className="w-14 h-14 text-emerald-500 mx-auto" />
          <h3 className="text-xl font-bold text-slate-900">¡Administrador Creado!</h3>
          <p className="text-xs text-slate-500">
            Redirigiendo al listado de administradores...
          </p>
        </div>
      ) : (
        <form onSubmit={handleSubmit} className="bg-white p-6 sm:p-8 rounded-3xl border border-slate-200 shadow-sm space-y-4 max-w-lg">
          
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1">Nombre *</label>
              <input
                type="text"
                name="firstName"
                required
                placeholder="Juan"
                value={form.firstName}
                onChange={handleChange}
                className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500 bg-slate-50/50"
              />
            </div>
            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1">Apellido *</label>
              <input
                type="text"
                name="lastName"
                required
                placeholder="Pérez"
                value={form.lastName}
                onChange={handleChange}
                className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500 bg-slate-50/50"
              />
            </div>
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1">Nombre de Usuario *</label>
            <input
              type="text"
              name="userName"
              required
              placeholder="admin_juan"
              value={form.userName}
              onChange={handleChange}
              className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500 bg-slate-50/50"
            />
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1">Correo Electrónico *</label>
            <input
              type="email"
              name="email"
              required
              placeholder="admin@empresa.com"
              value={form.email}
              onChange={handleChange}
              className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500 bg-slate-50/50"
            />
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1">Contraseña *</label>
            <input
              type="password"
              name="password"
              required
              placeholder="••••••••"
              value={form.password}
              onChange={handleChange}
              className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500 bg-slate-50/50"
            />
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1">Confirmar Contraseña *</label>
            <input
              type="password"
              name="confirmPassword"
              required
              placeholder="••••••••"
              value={form.confirmPassword}
              onChange={handleChange}
              className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500 bg-slate-50/50"
            />
          </div>

          {error && (
            <div className="flex items-center gap-2 p-3 bg-rose-50 rounded-xl border border-rose-200">
              <AlertCircle className="w-4 h-4 text-rose-600 shrink-0" />
              <p className="text-xs text-rose-600 font-semibold">{error}</p>
            </div>
          )}

          <div className="pt-3 flex justify-end gap-2">
            <button
              type="button"
              onClick={() => navigate('/admin/admins')}
              className="px-4 py-2 rounded-xl text-xs font-bold text-slate-600 hover:bg-slate-100"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="px-5 py-2 bg-slate-900 hover:bg-slate-800 disabled:opacity-50 text-white font-extrabold text-xs rounded-xl shadow-md"
            >
              {isSubmitting ? 'Creando...' : 'Crear Administrador'}
            </button>
          </div>
        </form>
      )}
    </div>
  );
};
