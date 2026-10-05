import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { adminService } from '../../api/services';
import { Badge } from '../../components/common/Badge';
import { Loader } from '../../components/common/Loader';
import { User } from '../../types';
import { getApiErrorMessage } from '../../utils/formatters';
import { 
  Code, 
  CheckCircle, 
  XCircle, 
  Plus, 
  Pencil, 
  Search 
} from 'lucide-react';
import { useLanguage } from '../../context/LanguageContext';

interface DevUser extends User {
  isActive?: boolean;
}

export const ManageDevelopersPage: React.FC = () => {
  const { t } = useLanguage();
  const navigate = useNavigate();
  const [developers, setDevelopers] = useState<DevUser[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');

  const loadDevelopers = async () => {
    try {
      const data = await adminService.getDevelopers();
      setDevelopers(data || []);
    } catch (err: unknown) {
      console.error("Error loading developers:", getApiErrorMessage(err));
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    Promise.resolve().then(() => loadDevelopers()).catch(console.error);
  }, []);

  const handleToggleStatus = async (userId: string, currentStatus: boolean) => {
    try {
      await adminService.toggleDeveloperStatus(userId, !currentStatus);
      loadDevelopers();
    } catch (err: unknown) {
      console.error("Error toggling developer status:", getApiErrorMessage(err));
    }
  };

  const filtered = developers.filter((d) => {
    const term = search.toLowerCase();
    return (
      (d.userName && d.userName.toLowerCase().includes(term)) ||
      (d.email && d.email.toLowerCase().includes(term))
    );
  });

  return (
    <div className="space-y-6">
      
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 pb-4 border-b border-slate-200 dark:border-slate-800">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 dark:text-white tracking-tight flex items-center gap-2">
            <Code className="w-6 h-6 text-royal-600 dark:text-indigo-400" />
            {t('admin.devs.title', "Administración de Desarrolladores")}
          </h2>
          <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">
            {t('admin.devs.subtitle', "Gestión de cuentas de desarrolladores de la API, activación/inactivación y creación.")}
          </p>
        </div>

        <div className="flex items-center gap-3">
          <div className="w-56">
            <div className="relative">
              <Search className="w-4 h-4 absolute left-3 top-2.5 text-slate-400 dark:text-slate-500" />
              <input
                type="text"
                placeholder={t('admin.devs.searchPlaceholder', "Buscar desarrollador...")}
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                className="w-full pl-9 pr-3 py-2 text-xs rounded-xl border border-slate-200 dark:border-slate-700 focus:ring-2 focus:ring-brand-500 bg-white dark:bg-slate-800 text-slate-900 dark:text-white placeholder:text-slate-400 dark:placeholder:text-slate-500"
              />
            </div>
          </div>

          <button
            onClick={() => navigate('/admin/developers/create')}
            className="flex items-center gap-1.5 px-4 py-2 bg-royal-600 hover:bg-royal-700 text-white font-bold text-xs rounded-xl shadow-md transition-all cursor-pointer"
          >
            <Plus className="w-4 h-4" />
            <span>{t('admin.devs.newDev', "Nuevo Developer")}</span>
          </button>
        </div>
      </div>

      {isLoading ? (
        <Loader text={t('admin.devs.loading', "Cargando listado de desarrolladores...")} />
      ) : (
        <div className="bg-white dark:bg-slate-900 rounded-3xl border border-slate-200 dark:border-slate-800 overflow-hidden shadow-xs">
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse text-xs">
              <thead>
                <tr className="bg-slate-50 dark:bg-slate-800/70 text-slate-700 dark:text-slate-300 font-extrabold uppercase tracking-wider border-b border-slate-200 dark:border-slate-800">
                  <th className="py-3.5 px-4">{t('admin.devs.colUser', "Usuario")}</th>
                  <th className="py-3.5 px-4">{t('admin.devs.colEmail', "Correo")}</th>
                  <th className="py-3.5 px-4">{t('admin.devs.colPhone', "Teléfono")}</th>
                  <th className="py-3.5 px-4">{t('admin.devs.colStatus', "Estado")}</th>
                  <th className="py-3.5 px-4 text-right">{t('admin.devs.colActions', "Acciones")}</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 dark:divide-slate-800">
                {filtered.map((dev) => (
                  <tr key={dev.id} className="hover:bg-slate-50/80 dark:hover:bg-slate-800/40 transition-colors">
                    <td className="py-3 px-4 font-bold text-slate-900 dark:text-white">
                      @{dev.userName}
                    </td>
                    <td className="py-3 px-4 text-slate-600 dark:text-slate-300">
                      {dev.email}
                    </td>
                    <td className="py-3 px-4 text-slate-500 dark:text-slate-400 font-mono">
                      {dev.phoneNumber || 'N/A'}
                    </td>
                    <td className="py-3 px-4">
                      <Badge status={dev.isActive ? 'Active' : 'Inactive'} />
                    </td>
                    <td className="py-3 px-4 text-right space-x-1.5">
                      
                      <button
                        onClick={() => handleToggleStatus(dev.id, dev.isActive ?? false)}
                        className={`inline-flex items-center gap-1 px-2.5 py-1.5 rounded-xl font-bold text-xs transition-all cursor-pointer ${
                          dev.isActive
                            ? 'bg-amber-50 dark:bg-amber-950/40 text-amber-700 dark:text-amber-300 hover:bg-amber-100 dark:hover:bg-amber-900/50 border border-amber-200 dark:border-amber-800/50'
                            : 'bg-emerald-50 dark:bg-emerald-950/40 text-emerald-700 dark:text-emerald-300 hover:bg-emerald-100 dark:hover:bg-emerald-900/50 border border-emerald-200 dark:border-emerald-800/50'
                        }`}
                        title={dev.isActive ? t('admin.devs.deactivate', 'Inactivar Desarrollador') : t('admin.devs.activate', 'Activar Desarrollador')}
                      >
                        {dev.isActive ? <XCircle className="w-3.5 h-3.5" /> : <CheckCircle className="w-3.5 h-3.5" />}
                        <span>{dev.isActive ? t('admin.devs.deactivate', 'Inactivar') : t('admin.devs.activate', 'Activar')}</span>
                      </button>

                      <button
                        onClick={() => navigate(`/admin/developers/edit/${dev.id}`)}
                        className="inline-flex items-center gap-1 px-2.5 py-1.5 bg-indigo-50 dark:bg-indigo-950/40 hover:bg-indigo-100 dark:hover:bg-indigo-900/50 text-royal-700 dark:text-indigo-300 rounded-xl font-bold text-xs border border-indigo-200 dark:border-indigo-800/50 transition-all cursor-pointer"
                        title={t('admin.devs.edit', "Editar Desarrollador")}
                      >
                        <Pencil className="w-3.5 h-3.5" />
                        <span>{t('admin.devs.edit', "Editar")}</span>
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
};
