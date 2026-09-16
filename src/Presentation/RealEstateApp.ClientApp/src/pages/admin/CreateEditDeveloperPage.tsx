import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { adminService } from '../../api/services';
import { Loader } from '../../components/common/Loader';
import { 
  Code, 
  ArrowLeft, 
  CheckCircle2, 
  AlertCircle 
} from 'lucide-react';

export const CreateEditDeveloperPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const isEditing = Boolean(id);
  const navigate = useNavigate();

  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    userName: '',
    email: '',
    password: '',
    confirmPassword: '',
  });

  const [isLoading, setIsLoading] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    if (isEditing && id) {
      const loadDeveloper = async () => {
        try {
          setIsLoading(true);
          const developers = await adminService.getDevelopers();
          const dev = developers.find((d: any) => d.id === id);
          if (dev) {
            setForm({
              firstName: '',
              lastName: '',
              userName: dev.userName || '',
              email: dev.email || '',
              password: '',
              confirmPassword: '',
            });
          }
        } catch (err) {
          console.error("Error loading developer:", err);
        } finally {
          setIsLoading(false);
        }
      };
      loadDeveloper();
    }
  }, [id, isEditing]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setIsSubmitting(true);
      setError(null);

      if (isEditing && id) {
        const payload: any = {
          userName: form.userName,
          email: form.email,
        };
        if (form.password) {
          payload.password = form.password;
        }
        await adminService.updateDeveloper(id, payload);
      } else {
        await adminService.createDeveloper({
          firstName: form.firstName,
          lastName: form.lastName,
          userName: form.userName,
          email: form.email,
          password: form.password,
          confirmPassword: form.confirmPassword,
        });
      }

      setSuccess(true);
      setTimeout(() => {
        navigate('/admin/developers');
      }, 1500);
    } catch (err: any) {
      console.error("Error saving developer:", err);
      setError(err.response?.data?.error || "Error al guardar el desarrollador.");
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) {
    return <Loader text="Cargando datos del desarrollador..." />;
  }

  return (
    <div className="space-y-6">
      
      <div className="flex items-center gap-4 pb-4 border-b border-slate-200">
        <button
          onClick={() => navigate('/admin/developers')}
          className="p-2 rounded-xl hover:bg-slate-100 text-slate-500 hover:text-slate-900 transition-colors"
        >
          <ArrowLeft className="w-5 h-5" />
        </button>
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <Code className="w-6 h-6 text-royal-600" />
            {isEditing ? 'Editar Desarrollador' : 'Crear Nuevo Desarrollador'}
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            {isEditing
              ? 'Actualiza la información del desarrollador. Deja la contraseña vacía para no cambiarla.'
              : 'Registra una nueva cuenta de desarrollador para acceso a la API.'}
          </p>
        </div>
      </div>

      {success ? (
        <div className="bg-white p-8 rounded-3xl border border-slate-200 text-center space-y-3 shadow-xl">
          <CheckCircle2 className="w-14 h-14 text-emerald-500 mx-auto" />
          <h3 className="text-xl font-bold text-slate-900">
            {isEditing ? '¡Desarrollador Actualizado!' : '¡Desarrollador Creado!'}
          </h3>
          <p className="text-xs text-slate-500">
            Redirigiendo al listado de desarrolladores...
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
                required={!isEditing}
                placeholder="José"
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
                required={!isEditing}
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
              placeholder="dev_jose"
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
              placeholder="dev@empresa.com"
              value={form.email}
              onChange={handleChange}
              className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500 bg-slate-50/50"
            />
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1">
                Contraseña {isEditing ? '(dejar vacío para no cambiar)' : '*'}
              </label>
              <input
                type="password"
                name="password"
                required={!isEditing}
                placeholder="••••••••"
                value={form.password}
                onChange={handleChange}
                className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500 bg-slate-50/50"
              />
            </div>
            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1">
                Confirmar Contraseña {isEditing ? '(dejar vacío para no cambiar)' : '*'}
              </label>
              <input
                type="password"
                name="confirmPassword"
                required={!isEditing}
                placeholder="••••••••"
                value={form.confirmPassword}
                onChange={handleChange}
                className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500 bg-slate-50/50"
              />
            </div>
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
              onClick={() => navigate('/admin/developers')}
              className="px-4 py-2 rounded-xl text-xs font-bold text-slate-600 hover:bg-slate-100"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="px-5 py-2 bg-royal-600 hover:bg-royal-700 disabled:opacity-50 text-white font-extrabold text-xs rounded-xl shadow-md"
            >
              {isSubmitting
                ? 'Guardando...'
                : isEditing
                  ? 'Actualizar Desarrollador'
                  : 'Crear Desarrollador'}
            </button>
          </div>
        </form>
      )}
    </div>
  );
};
