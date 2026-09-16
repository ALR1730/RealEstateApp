import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { authService } from '../../api/services';
import { Building2, UserPlus, CheckCircle, ShieldAlert } from 'lucide-react';

export const RegisterAgentPage: React.FC = () => {
  const navigate = useNavigate();
  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    email: '',
    userName: '',
    password: '',
    confirmPassword: '',
    phone: '',
  });

  const [isLoading, setIsLoading] = useState(false);
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
      setIsLoading(true);
      setError(null);
      const res = await authService.registerAgent(form);

      if (res.hasError) {
        setError(res.error || "Error al registrar agente inmobiliario.");
        return;
      }

      setSuccess(true);
      setTimeout(() => {
        navigate('/login');
      }, 2500);
    } catch (err: any) {
      console.error("Register agent error:", err);
      setError(err.response?.data?.error || "Error al conectar con el servidor.");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-[80vh] flex items-center justify-center px-4 py-12">
      <div className="max-w-lg w-full space-y-6">
        
        <div className="text-center space-y-2">
          <Link to="/" className="inline-flex items-center gap-2 group">
            <div className="w-12 h-12 rounded-2xl bg-gradient-to-tr from-brand-600 to-emerald-400 flex items-center justify-center text-white shadow-lg shadow-brand-500/20">
              <Building2 className="w-7 h-7" />
            </div>
          </Link>
          <h2 className="text-2xl sm:text-3xl font-extrabold text-slate-900 tracking-tight">
            Únete como Agente Inmobiliario
          </h2>
          <p className="text-xs text-slate-500">
            Publica tu portafolio, gestiona ofertas en tiempo real y chatea directamente con prospectos calificados.
          </p>
        </div>

        {success ? (
          <div className="bg-white p-8 rounded-3xl border border-slate-200 text-center space-y-3 shadow-xl animate-in zoom-in-95">
            <CheckCircle className="w-14 h-14 text-emerald-500 mx-auto" />
            <h3 className="text-xl font-bold text-slate-900">¡Registro de Agente Exitoso!</h3>
            <p className="text-xs text-slate-500">
              Tu cuenta de agente está lista. Redirigiendo al login...
            </p>
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="bg-white p-6 sm:p-8 rounded-3xl border border-slate-200 shadow-sm space-y-4">
            
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1">Nombre</label>
                <input
                  type="text"
                  name="firstName"
                  required
                  placeholder="Carlos"
                  value={form.firstName}
                  onChange={handleChange}
                  className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500 bg-slate-50/50"
                />
              </div>

              <div>
                <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1">Apellido</label>
                <input
                  type="text"
                  name="lastName"
                  required
                  placeholder="Gómez"
                  value={form.lastName}
                  onChange={handleChange}
                  className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500 bg-slate-50/50"
                />
              </div>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1">Usuario</label>
                <input
                  type="text"
                  name="userName"
                  required
                  placeholder="carlosagente"
                  value={form.userName}
                  onChange={handleChange}
                  className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500 bg-slate-50/50"
                />
              </div>

              <div>
                <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1">Teléfono Directo</label>
                <input
                  type="tel"
                  name="phone"
                  required
                  placeholder="809-555-4321"
                  value={form.phone}
                  onChange={handleChange}
                  className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500 bg-slate-50/50"
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1">Correo Electrónico Corporativo</label>
              <input
                type="email"
                name="email"
                required
                placeholder="carlos.gomez@inmobiliaria.com"
                value={form.email}
                onChange={handleChange}
                className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500 bg-slate-50/50"
              />
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1">Contraseña</label>
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
                <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1">Confirmar Contraseña</label>
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
            </div>

            {error && (
              <div className="p-3 bg-rose-50 rounded-xl border border-rose-200 text-xs text-rose-700 flex items-center gap-2">
                <ShieldAlert className="w-4 h-4 shrink-0" />
                <span>{error}</span>
              </div>
            )}

            <button
              type="submit"
              disabled={isLoading}
              className="w-full py-3 bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 text-white font-extrabold text-xs sm:text-sm rounded-xl shadow-md shadow-emerald-600/20 transition-all flex items-center justify-center gap-2"
            >
              <UserPlus className="w-4 h-4" />
              <span>{isLoading ? 'Creando perfil de agente...' : 'Registrarse como Agente Inmobiliario'}</span>
            </button>

            <div className="pt-2 text-center text-xs text-slate-500">
              ¿Ya tienes cuenta?{' '}
              <Link to="/login" className="font-bold text-brand-600 hover:underline">
                Inicia Sesión aquí
              </Link>
            </div>
          </form>
        )}
      </div>
    </div>
  );
};
