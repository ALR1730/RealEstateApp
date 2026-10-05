import React, { useState, useEffect } from 'react';
import { adminService } from '../../api/services';
import { Badge } from '../../components/common/Badge';
import { Loader } from '../../components/common/Loader';
import { Modal } from '../../components/common/Modal';
import { User } from '../../types';
import { getApiErrorMessage } from '../../utils/formatters';
import { Shield, Code, UserPlus } from 'lucide-react';
import { useLanguage } from '../../context/LanguageContext';

export const ManageUsersPage: React.FC = () => {
  const { t } = useLanguage();
  const [admins, setAdmins] = useState<User[]>([]);
  const [developers, setDevelopers] = useState<User[]>([]);
  const [activeTab, setActiveTab] = useState<'admins' | 'developers'>('admins');
  const [isLoading, setIsLoading] = useState(true);

  // Create User Modal
  const [createModalOpen, setCreateModalOpen] = useState(false);
  const [createRole, setCreateRole] = useState<'Admin' | 'Developer'>('Admin');
  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    email: '',
    userName: '',
    password: '',
    confirmPassword: '',
    phone: '',
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadUsers = async () => {
    try {
      const [adm, dev] = await Promise.all([
        adminService.getAdmins(),
        adminService.getDevelopers(),
      ]);
      setAdmins(adm || []);
      setDevelopers(dev || []);
    } catch (err) {
      console.error("Error loading users:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    Promise.resolve().then(() => loadUsers()).catch(console.error);
  }, []);

  const handleToggleAdmin = async (userId: string, current: boolean) => {
    try {
      await adminService.toggleAdminStatus(userId, !current);
      loadUsers();
    } catch (err) {
      console.error("Error toggling admin:", err);
    }
  };

  const handleToggleDev = async (userId: string, current: boolean) => {
    try {
      await adminService.toggleDeveloperStatus(userId, !current);
      loadUsers();
    } catch (err) {
      console.error("Error toggling dev:", err);
    }
  };

  const handleCreateSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (form.password !== form.confirmPassword) {
      setError(t('admin.users.passwordMismatch', "Las contraseñas no coinciden."));
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);
      if (createRole === 'Admin') {
        await adminService.createAdmin(form);
      } else {
        await adminService.createDeveloper(form);
      }
      setCreateModalOpen(false);
      setForm({ firstName: '', lastName: '', email: '', userName: '', password: '', confirmPassword: '', phone: '' });
      await loadUsers();
      alert(`Usuario ${createRole} ${t('admin.users.createSuccess', 'creado exitosamente.')}`);
    } catch (err: unknown) {
      console.error("Error creating user:", err);
      setError(getApiErrorMessage(err, t('admin.users.createError', "Error al registrar usuario.")));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 pb-4 border-b border-slate-200 dark:border-slate-800">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 dark:text-white tracking-tight flex items-center gap-2">
            <Shield className="w-6 h-6 text-royal-600 dark:text-indigo-400" />
            {t('admin.users.title', 'Administradores & Desarrolladores API')}
          </h2>
          <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">
            {t('admin.users.subtitle', 'Gestión de credenciales de gobierno del sistema y accesos técnicos para consumo REST.')}
          </p>
        </div>

        <div className="flex gap-2">
          <button
            onClick={() => { setCreateRole('Admin'); setCreateModalOpen(true); }}
            className="flex items-center gap-1.5 px-4 py-2 bg-slate-900 hover:bg-slate-800 dark:bg-slate-800 dark:hover:bg-slate-700 text-white font-bold text-xs rounded-xl shadow-md transition-all"
          >
            <UserPlus className="w-4 h-4" />
            <span>{t('admin.users.newAdmin', 'Nuevo Admin')}</span>
          </button>
          <button
            onClick={() => { setCreateRole('Developer'); setCreateModalOpen(true); }}
            className="flex items-center gap-1.5 px-4 py-2 bg-royal-600 hover:bg-royal-700 text-white font-bold text-xs rounded-xl shadow-md transition-all"
          >
            <Code className="w-4 h-4" />
            <span>{t('admin.users.newDev', 'Nuevo Developer')}</span>
          </button>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex gap-2 border-b border-slate-200 dark:border-slate-800">
        <button
          onClick={() => setActiveTab('admins')}
          className={`pb-3 px-4 text-xs sm:text-sm font-bold border-b-2 transition-all ${
            activeTab === 'admins'
              ? 'border-brand-600 text-brand-700 dark:text-brand-400'
              : 'border-transparent text-slate-500 hover:text-slate-900 dark:hover:text-slate-200'
          }`}
        >
          {t('admin.users.adminsTab', 'Administradores')} ({admins.length})
        </button>
        <button
          onClick={() => setActiveTab('developers')}
          className={`pb-3 px-4 text-xs sm:text-sm font-bold border-b-2 transition-all ${
            activeTab === 'developers'
              ? 'border-royal-600 text-royal-700 dark:text-indigo-400'
              : 'border-transparent text-slate-500 hover:text-slate-900 dark:hover:text-slate-200'
          }`}
        >
          {t('admin.users.devsTab', 'Desarrolladores Web API')} ({developers.length})
        </button>
      </div>

      {isLoading ? (
        <Loader text={t('admin.users.loading', 'Cargando usuarios del sistema...')} />
      ) : (
        <div className="bg-white dark:bg-slate-900 rounded-3xl border border-slate-200 dark:border-slate-800 overflow-hidden shadow-xs">
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse text-xs">
              <thead>
                <tr className="bg-slate-50 dark:bg-slate-800/70 text-slate-700 dark:text-slate-300 font-extrabold uppercase tracking-wider border-b border-slate-200 dark:border-slate-800">
                  <th className="py-3.5 px-4">{t('admin.users.colUser', 'Usuario')}</th>
                  <th className="py-3.5 px-4">{t('admin.users.colEmail', 'Correo')}</th>
                  <th className="py-3.5 px-4">{t('admin.users.colPhone', 'Teléfono')}</th>
                  <th className="py-3.5 px-4">{t('admin.users.colStatus', 'Estado')}</th>
                  <th className="py-3.5 px-4 text-right">{t('admin.users.colAction', 'Acción')}</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 dark:divide-slate-800">
                {(activeTab === 'admins' ? admins : developers).map((u) => (
                  <tr key={u.id} className="hover:bg-slate-50/80 dark:hover:bg-slate-800/40 transition-colors">
                    <td className="py-3 px-4 font-bold text-slate-900 dark:text-white">
                      @{u.userName}
                    </td>
                    <td className="py-3 px-4 text-slate-600 dark:text-slate-300">
                      {u.email}
                    </td>
                    <td className="py-3 px-4 text-slate-500 dark:text-slate-400 font-mono">
                      {u.phoneNumber || 'N/A'}
                    </td>
                    <td className="py-3 px-4">
                      <Badge status={u.isActive ? 'Active' : 'Inactive'} />
                    </td>
                    <td className="py-3 px-4 text-right">
                      <button
                        onClick={() => activeTab === 'admins' ? handleToggleAdmin(u.id, u.isActive) : handleToggleDev(u.id, u.isActive)}
                        className={`inline-flex items-center gap-1 px-3 py-1.5 rounded-xl font-bold text-xs transition-all ${
                          u.isActive
                            ? 'bg-amber-50 dark:bg-amber-950/40 text-amber-700 dark:text-amber-300 hover:bg-amber-100 dark:hover:bg-amber-900/50 border border-amber-200 dark:border-amber-800/50'
                            : 'bg-emerald-50 dark:bg-emerald-950/40 text-emerald-700 dark:text-emerald-300 hover:bg-emerald-100 dark:hover:bg-emerald-900/50 border border-emerald-200 dark:border-emerald-800/50'
                        }`}
                      >
                        {u.isActive ? t('admin.agents.deactivate', 'Inactivar') : t('admin.agents.activate', 'Activar')}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Create User Modal */}
      {createModalOpen && (
        <Modal
          isOpen={createModalOpen}
          onClose={() => setCreateModalOpen(false)}
          title={`${t('admin.users.modalCreateTitle', 'Crear Nuevo Usuario')} ${createRole}`}
        >
          <form onSubmit={handleCreateSubmit} className="space-y-4">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-bold text-slate-700 dark:text-slate-300 uppercase mb-1">{t('admin.users.firstName', 'Nombre')}</label>
                <input
                  type="text"
                  required
                  value={form.firstName}
                  onChange={(e) => setForm({ ...form, firstName: e.target.value })}
                  className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:ring-2 focus:ring-brand-500"
                />
              </div>
              <div>
                <label className="block text-xs font-bold text-slate-700 dark:text-slate-300 uppercase mb-1">{t('admin.users.lastName', 'Apellido')}</label>
                <input
                  type="text"
                  required
                  value={form.lastName}
                  onChange={(e) => setForm({ ...form, lastName: e.target.value })}
                  className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:ring-2 focus:ring-brand-500"
                />
              </div>
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-bold text-slate-700 dark:text-slate-300 uppercase mb-1">{t('admin.users.userName', 'Usuario')}</label>
                <input
                  type="text"
                  required
                  value={form.userName}
                  onChange={(e) => setForm({ ...form, userName: e.target.value })}
                  className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:ring-2 focus:ring-brand-500"
                />
              </div>
              <div>
                <label className="block text-xs font-bold text-slate-700 dark:text-slate-300 uppercase mb-1">{t('admin.users.phone', 'Teléfono')}</label>
                <input
                  type="tel"
                  value={form.phone}
                  onChange={(e) => setForm({ ...form, phone: e.target.value })}
                  className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:ring-2 focus:ring-brand-500"
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-700 dark:text-slate-300 uppercase mb-1">{t('admin.users.email', 'Correo Electrónico')}</label>
              <input
                type="email"
                required
                value={form.email}
                onChange={(e) => setForm({ ...form, email: e.target.value })}
                className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:ring-2 focus:ring-brand-500"
              />
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-bold text-slate-700 dark:text-slate-300 uppercase mb-1">{t('admin.users.password', 'Contraseña')}</label>
                <input
                  type="password"
                  required
                  value={form.password}
                  onChange={(e) => setForm({ ...form, password: e.target.value })}
                  className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:ring-2 focus:ring-brand-500"
                />
              </div>
              <div>
                <label className="block text-xs font-bold text-slate-700 dark:text-slate-300 uppercase mb-1">{t('admin.users.confirmPassword', 'Confirmar')}</label>
                <input
                  type="password"
                  required
                  value={form.confirmPassword}
                  onChange={(e) => setForm({ ...form, confirmPassword: e.target.value })}
                  className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:ring-2 focus:ring-brand-500"
                />
              </div>
            </div>

            {error && <p className="text-xs text-rose-600 font-semibold">{error}</p>}

            <div className="pt-3 flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setCreateModalOpen(false)}
                className="px-4 py-2 rounded-xl text-xs font-bold text-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800"
              >
                {t('admin.common.cancel', 'Cancelar')}
              </button>
              <button
                type="submit"
                disabled={isSubmitting}
                className="px-5 py-2 bg-brand-600 hover:bg-brand-700 disabled:opacity-50 text-white font-extrabold text-xs rounded-xl shadow-md"
              >
                {isSubmitting ? t('admin.users.creating', 'Registrando...') : t('admin.users.createUser', 'Crear Usuario')}
              </button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  );
};
