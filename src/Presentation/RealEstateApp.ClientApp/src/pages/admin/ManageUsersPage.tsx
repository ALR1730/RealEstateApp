import React, { useState, useEffect } from 'react';
import { adminService, authService } from '../../api/services';
import { Badge } from '../../components/common/Badge';
import { Loader } from '../../components/common/Loader';
import { Modal } from '../../components/common/Modal';
import { Shield, Code, UserPlus, CheckCircle, XCircle, Key, Mail, User } from 'lucide-react';

export const ManageUsersPage: React.FC = () => {
  const [admins, setAdmins] = useState<any[]>([]);
  const [developers, setDevelopers] = useState<any[]>([]);
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
      setError("Las contraseñas no coinciden.");
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);
      if (createRole === 'Admin') {
        await authService.registerClient({ ...form, role: 'Admin' }); // WebApi register-admin
      } else {
        await authService.registerClient({ ...form, role: 'Developer' }); // WebApi register-developer
      }
      setCreateModalOpen(false);
      setForm({ firstName: '', lastName: '', email: '', userName: '', password: '', confirmPassword: '', phone: '' });
      await loadUsers();
      alert(`Usuario ${createRole} creado exitosamente.`);
    } catch (err: any) {
      console.error("Error creating user:", err);
      setError(err.response?.data?.error || "Error al registrar usuario.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 pb-4 border-b border-slate-200">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <Shield className="w-6 h-6 text-royal-600" />
            Administradores & Desarrolladores API
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Gestión de credenciales de gobierno del sistema y accesos técnicos para consumo REST.
          </p>
        </div>

        <div className="flex gap-2">
          <button
            onClick={() => { setCreateRole('Admin'); setCreateModalOpen(true); }}
            className="flex items-center gap-1.5 px-4 py-2 bg-slate-900 hover:bg-slate-800 text-white font-bold text-xs rounded-xl shadow-md transition-all"
          >
            <UserPlus className="w-4 h-4" />
            <span>Nuevo Admin</span>
          </button>
          <button
            onClick={() => { setCreateRole('Developer'); setCreateModalOpen(true); }}
            className="flex items-center gap-1.5 px-4 py-2 bg-royal-600 hover:bg-royal-700 text-white font-bold text-xs rounded-xl shadow-md transition-all"
          >
            <Code className="w-4 h-4" />
            <span>Nuevo Developer</span>
          </button>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex gap-2 border-b border-slate-200">
        <button
          onClick={() => setActiveTab('admins')}
          className={`pb-3 px-4 text-xs sm:text-sm font-bold border-b-2 transition-all ${
            activeTab === 'admins'
              ? 'border-brand-600 text-brand-700'
              : 'border-transparent text-slate-500 hover:text-slate-900'
          }`}
        >
          Administradores ({admins.length})
        </button>
        <button
          onClick={() => setActiveTab('developers')}
          className={`pb-3 px-4 text-xs sm:text-sm font-bold border-b-2 transition-all ${
            activeTab === 'developers'
              ? 'border-royal-600 text-royal-700'
              : 'border-transparent text-slate-500 hover:text-slate-900'
          }`}
        >
          Desarrolladores Web API ({developers.length})
        </button>
      </div>

      {isLoading ? (
        <Loader text="Cargando usuarios del sistema..." />
      ) : (
        <div className="bg-white rounded-3xl border border-slate-200 overflow-hidden shadow-xs">
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse text-xs">
              <thead>
                <tr className="bg-slate-50 text-slate-700 font-extrabold uppercase tracking-wider border-b border-slate-200">
                  <th className="py-3.5 px-4">Usuario</th>
                  <th className="py-3.5 px-4">Correo</th>
                  <th className="py-3.5 px-4">Teléfono</th>
                  <th className="py-3.5 px-4">Estado</th>
                  <th className="py-3.5 px-4 text-right">Acción</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {(activeTab === 'admins' ? admins : developers).map((u) => (
                  <tr key={u.id} className="hover:bg-slate-50/80 transition-colors">
                    <td className="py-3 px-4 font-bold text-slate-900">
                      @{u.userName}
                    </td>
                    <td className="py-3 px-4 text-slate-600">
                      {u.email}
                    </td>
                    <td className="py-3 px-4 text-slate-500 font-mono">
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
                            ? 'bg-amber-50 text-amber-700 hover:bg-amber-100 border border-amber-200'
                            : 'bg-emerald-50 text-emerald-700 hover:bg-emerald-100 border border-emerald-200'
                        }`}
                      >
                        {u.isActive ? 'Inactivar' : 'Activar'}
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
          title={`Crear Nuevo Usuario ${createRole}`}
        >
          <form onSubmit={handleCreateSubmit} className="space-y-4">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-bold text-slate-700 uppercase mb-1">Nombre</label>
                <input
                  type="text"
                  required
                  value={form.firstName}
                  onChange={(e) => setForm({ ...form, firstName: e.target.value })}
                  className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500"
                />
              </div>
              <div>
                <label className="block text-xs font-bold text-slate-700 uppercase mb-1">Apellido</label>
                <input
                  type="text"
                  required
                  value={form.lastName}
                  onChange={(e) => setForm({ ...form, lastName: e.target.value })}
                  className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500"
                />
              </div>
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-bold text-slate-700 uppercase mb-1">Usuario</label>
                <input
                  type="text"
                  required
                  value={form.userName}
                  onChange={(e) => setForm({ ...form, userName: e.target.value })}
                  className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500"
                />
              </div>
              <div>
                <label className="block text-xs font-bold text-slate-700 uppercase mb-1">Teléfono</label>
                <input
                  type="tel"
                  value={form.phone}
                  onChange={(e) => setForm({ ...form, phone: e.target.value })}
                  className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500"
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase mb-1">Correo Electrónico</label>
              <input
                type="email"
                required
                value={form.email}
                onChange={(e) => setForm({ ...form, email: e.target.value })}
                className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500"
              />
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-bold text-slate-700 uppercase mb-1">Contraseña</label>
                <input
                  type="password"
                  required
                  value={form.password}
                  onChange={(e) => setForm({ ...form, password: e.target.value })}
                  className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500"
                />
              </div>
              <div>
                <label className="block text-xs font-bold text-slate-700 uppercase mb-1">Confirmar</label>
                <input
                  type="password"
                  required
                  value={form.confirmPassword}
                  onChange={(e) => setForm({ ...form, confirmPassword: e.target.value })}
                  className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500"
                />
              </div>
            </div>

            {error && <p className="text-xs text-rose-600 font-semibold">{error}</p>}

            <div className="pt-3 flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setCreateModalOpen(false)}
                className="px-4 py-2 rounded-xl text-xs font-bold text-slate-600 hover:bg-slate-100"
              >
                Cancelar
              </button>
              <button
                type="submit"
                disabled={isSubmitting}
                className="px-5 py-2 bg-brand-600 hover:bg-brand-700 disabled:opacity-50 text-white font-extrabold text-xs rounded-xl shadow-md"
              >
                {isSubmitting ? 'Registrando...' : 'Crear Usuario'}
              </button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  );
};
