import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { adminService } from '../../api/services';
import { Badge } from '../../components/common/Badge';
import { Loader } from '../../components/common/Loader';
import { 
  ShieldCheck, 
  CheckCircle, 
  XCircle, 
  Plus, 
  Search 
} from 'lucide-react';

export const ManageAdminsPage: React.FC = () => {
  const navigate = useNavigate();
  const [admins, setAdmins] = useState<any[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');

  const loadAdmins = async () => {
    try {
      const data = await adminService.getAdmins();
      setAdmins(data || []);
    } catch (err) {
      console.error("Error loading admins:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    Promise.resolve().then(() => loadAdmins()).catch(console.error);
  }, []);

  const handleToggleStatus = async (userId: string, currentStatus: boolean) => {
    try {
      await adminService.toggleAdminStatus(userId, !currentStatus);
      loadAdmins();
    } catch (err) {
      console.error("Error toggling admin status:", err);
    }
  };

  const filtered = admins.filter((a) => {
    const term = search.toLowerCase();
    return (
      (a.userName && a.userName.toLowerCase().includes(term)) ||
      (a.email && a.email.toLowerCase().includes(term))
    );
  });

  return (
    <div className="space-y-6">
      
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 pb-4 border-b border-slate-200">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <ShieldCheck className="w-6 h-6 text-royal-600" />
            Administración de Administradores
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Gestión de cuentas de administradores del sistema, activación/inactivación y creación.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <div className="w-56">
            <div className="relative">
              <Search className="w-4 h-4 absolute left-3 top-2.5 text-slate-400" />
              <input
                type="text"
                placeholder="Buscar admin..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                className="w-full pl-9 pr-3 py-2 text-xs rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500 bg-white"
              />
            </div>
          </div>

          <button
            onClick={() => navigate('/admin/admins/create')}
            className="flex items-center gap-1.5 px-4 py-2 bg-slate-900 hover:bg-slate-800 text-white font-bold text-xs rounded-xl shadow-md transition-all"
          >
            <Plus className="w-4 h-4" />
            <span>Nuevo Admin</span>
          </button>
        </div>
      </div>

      {isLoading ? (
        <Loader text="Cargando listado de administradores..." />
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
                  <th className="py-3.5 px-4 text-right">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {filtered.map((admin) => (
                  <tr key={admin.id} className="hover:bg-slate-50/80 transition-colors">
                    <td className="py-3 px-4 font-bold text-slate-900">
                      @{admin.userName}
                    </td>
                    <td className="py-3 px-4 text-slate-600">
                      {admin.email}
                    </td>
                    <td className="py-3 px-4 text-slate-500 font-mono">
                      {admin.phoneNumber || 'N/A'}
                    </td>
                    <td className="py-3 px-4">
                      <Badge status={admin.isActive ? 'Active' : 'Inactive'} />
                    </td>
                    <td className="py-3 px-4 text-right">
                      <button
                        onClick={() => handleToggleStatus(admin.id, admin.isActive)}
                        className={`inline-flex items-center gap-1 px-2.5 py-1.5 rounded-xl font-bold text-xs transition-all ${
                          admin.isActive
                            ? 'bg-amber-50 text-amber-700 hover:bg-amber-100 border border-amber-200'
                            : 'bg-emerald-50 text-emerald-700 hover:bg-emerald-100 border border-emerald-200'
                        }`}
                        title={admin.isActive ? 'Inactivar Administrador' : 'Activar Administrador'}
                      >
                        {admin.isActive ? <XCircle className="w-3.5 h-3.5" /> : <CheckCircle className="w-3.5 h-3.5" />}
                        <span>{admin.isActive ? 'Inactivar' : 'Activar'}</span>
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
