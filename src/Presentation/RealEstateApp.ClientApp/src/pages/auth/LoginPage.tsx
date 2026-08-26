import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { Building2, LogIn, Key, Mail, ShieldAlert, Sparkles, User, UserCheck, Code } from 'lucide-react';

export const LoginPage: React.FC = () => {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email || !password) return;

    try {
      setIsLoading(true);
      setError(null);
      const res = await login(email, password);

      if (res.hasError) {
        setError(res.error || 'Credenciales inválidas.');
        return;
      }

      // Redirigir según rol
      const roles = res.roles || [];
      if (roles.includes('Admin')) navigate('/admin');
      else if (roles.includes('Agent')) navigate('/agent');
      else if (roles.includes('Developer')) navigate('/admin');
      else navigate('/client');
    } catch (err: any) {
      console.error("Login error:", err);
      setError(err.response?.data?.error || 'Error al conectar con el servidor.');
    } finally {
      setIsLoading(false);
    }
  };

  // One-click demo login helper
  const handleQuickLogin = (demoEmail: string, demoPass: string) => {
    setEmail(demoEmail);
    setPassword(demoPass);
  };

  return (
    <div className="min-h-[80vh] flex items-center justify-center px-4 py-12">
      <div className="max-w-md w-full space-y-6">
        
        {/* Header Logo */}
        <div className="text-center space-y-2">
          <Link to="/" className="inline-flex items-center gap-2 group">
            <div className="w-12 h-12 rounded-2xl bg-gradient-to-tr from-brand-600 to-emerald-400 flex items-center justify-center text-white shadow-lg shadow-brand-500/20 group-hover:scale-105 transition-transform">
              <Building2 className="w-7 h-7" />
            </div>
          </Link>
          <h2 className="text-2xl sm:text-3xl font-extrabold text-slate-900 tracking-tight">
            Iniciar Sesión
          </h2>
          <p className="text-xs text-slate-500">
            Ingresa tus credenciales para acceder a la plataforma inmobiliaria.
          </p>
        </div>

        {/* Demo Fast Login Buttons Panel */}
        <div className="p-4 bg-gradient-to-br from-slate-900 to-navy-950 rounded-2xl text-white shadow-xl space-y-3">
          <div className="flex items-center justify-between">
            <span className="text-[11px] font-extrabold uppercase tracking-wider text-brand-400 flex items-center gap-1">
              <Sparkles className="w-3.5 h-3.5" />
              Acceso Rápido para Demostración
            </span>
            <span className="text-[10px] text-slate-400 font-mono">1 Clic</span>
          </div>

          <div className="grid grid-cols-2 gap-2">
            <button
              type="button"
              onClick={() => handleQuickLogin('adminuser@realestate.com', 'Admin123!')}
              className="p-2.5 rounded-xl bg-white/10 hover:bg-white/20 text-left transition-all border border-white/10 text-xs flex items-center gap-2"
            >
              <UserCheck className="w-4 h-4 text-emerald-400 shrink-0" />
              <div className="truncate">
                <p className="font-bold text-[11px] text-white">Administrador</p>
                <p className="text-[9px] text-slate-300 font-mono">adminuser</p>
              </div>
            </button>

            <button
              type="button"
              onClick={() => handleQuickLogin('agentuser@realestate.com', 'Agent123!')}
              className="p-2.5 rounded-xl bg-white/10 hover:bg-white/20 text-left transition-all border border-white/10 text-xs flex items-center gap-2"
            >
              <Building2 className="w-4 h-4 text-sky-400 shrink-0" />
              <div className="truncate">
                <p className="font-bold text-[11px] text-white">Agente</p>
                <p className="text-[9px] text-slate-300 font-mono">agentuser</p>
              </div>
            </button>

            <button
              type="button"
              onClick={() => handleQuickLogin('clientuser@realestate.com', 'Client123!')}
              className="p-2.5 rounded-xl bg-white/10 hover:bg-white/20 text-left transition-all border border-white/10 text-xs flex items-center gap-2"
            >
              <User className="w-4 h-4 text-amber-400 shrink-0" />
              <div className="truncate">
                <p className="font-bold text-[11px] text-white">Cliente Comprador</p>
                <p className="text-[9px] text-slate-300 font-mono">clientuser</p>
              </div>
            </button>

            <button
              type="button"
              onClick={() => handleQuickLogin('developeruser@realestate.com', 'Developer123!')}
              className="p-2.5 rounded-xl bg-white/10 hover:bg-white/20 text-left transition-all border border-white/10 text-xs flex items-center gap-2"
            >
              <Code className="w-4 h-4 text-purple-400 shrink-0" />
              <div className="truncate">
                <p className="font-bold text-[11px] text-white">Desarrollador</p>
                <p className="text-[9px] text-slate-300 font-mono">developeruser</p>
              </div>
            </button>
          </div>
        </div>

        {/* Login Form */}
        <form onSubmit={handleSubmit} className="bg-white p-6 sm:p-8 rounded-3xl border border-slate-200 shadow-sm space-y-4">
          
          <div>
            <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5 flex items-center gap-1.5">
              <Mail className="w-3.5 h-3.5 text-brand-600" />
              Correo Electrónico
            </label>
            <input
              type="email"
              required
              placeholder="ejemplo@realestate.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full px-3.5 py-2.5 text-xs sm:text-sm rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500 bg-slate-50/50"
            />
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5 flex items-center gap-1.5">
              <Key className="w-3.5 h-3.5 text-brand-600" />
              Contraseña
            </label>
            <input
              type="password"
              required
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full px-3.5 py-2.5 text-xs sm:text-sm rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500 bg-slate-50/50"
            />
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
            className="w-full py-3 bg-brand-600 hover:bg-brand-700 disabled:opacity-50 text-white font-extrabold text-xs sm:text-sm rounded-xl shadow-md shadow-brand-600/20 transition-all flex items-center justify-center gap-2"
          >
            <LogIn className="w-4 h-4" />
            <span>{isLoading ? 'Iniciando sesión...' : 'Entrar a la Plataforma'}</span>
          </button>

          <div className="pt-2 text-center text-xs text-slate-500 space-y-1">
            <p>
              ¿No tienes una cuenta aún?{' '}
              <Link to="/register" className="font-bold text-brand-600 hover:underline">
                Regístrate como Cliente
              </Link>
            </p>
            <p>
              ¿Eres agente inmobiliario?{' '}
              <Link to="/register-agent" className="font-bold text-emerald-600 hover:underline">
                Únete como Corredor
              </Link>
            </p>
          </div>
        </form>
      </div>
    </div>
  );
};
