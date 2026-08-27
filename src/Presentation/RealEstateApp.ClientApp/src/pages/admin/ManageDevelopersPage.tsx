import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { adminService } from '../../api/services';
import { Badge } from '../../components/common/Badge';
import { Loader } from '../../components/common/Loader';
import { 
  Code, 
  CheckCircle, 
  XCircle, 
  Plus, 
  Pencil, 
  Search 
} from 'lucide-react';

export const ManageDevelopersPage: React.FC = () => {
  const navigate = useNavigate();
  const [developers, setDevelopers] = useState<any[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');

  const loadDevelopers = async () => {
    try {
      const data = await adminService.getDevelopers();
      setDevelopers(data || []);
    } catch (err) {
      console.error("Error loading developers:", err);
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
    } catch (err) {
      console.error("Error toggling developer status:", err);
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
      
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 pb-4 border-b border-slate-200">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <Code className="w-6 h-6 text-royal-600" />
            Administración de Desarrolladores
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Gestión de cuentas de desarrolladores de la API, activación/inactivación y creación.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <div className="w-56">
            <div className="relative">
              <Search className="w-4 h-4 absolute left-3 top-2.5 text-slate-400" />
              <input
                type="text"
                placeholder="Buscar desarrollador..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                className="w-full pl-9 pr-3 py-2 text-xs rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500 bg-white"
              />
            </div>
          </div>

          <button
            onClick={() => navigate('/admin/developers/create')}
            className="flex items-center gap-1.5 px-4 py-2 bg-royal-600 hover:bg-royal-700 text-white font-bold text-xs rounded-xl shadow-md transition-all"
          >
            <Plus className="w-4 h-4" />
            <span>Nuevo Developer</span>
          </button>
        </div>
      </div>

      {isLoading ? (
        <Loader text="Cargando listado de desarrolladores..." />
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
                {filtered.map((dev) => (
                  <tr key={dev.id} className="hover:bg-slate-50/80 transition-colors">
                    <td className="py-3 px-4 font-bold text-slate-900">
                      @{dev.userName}
                    </td>
                    <td className="py-3 px-4 text-slate-600">
                      {dev.email}
                    </td>
                    <td className="py-3 px-4 text-slate-500 font-mono">
                      {dev.phoneNumber || 'N/A'}
                    </td>
                    <td className="py-3 px-4">
                      <Badge status={dev.isActive ? 'Active' : 'Inactive'} />
                    </td>
                    <td className="py-3 px-4 text-right space-x-1.5">
                      
                      <button
                        onClick={() => handleToggleStatus(dev.id, dev.isActive)}
                        className={`inline-flex items-center gap-1 px-2.5 py-1.5 rounded-xl font-bold text-xs transition-all ${
                          dev.isActive
                            ? 'bg-amber-50 text-amber-700 hover:bg-amber-100 border border-amber-200'
                            : 'bg-emerald-50 text-emerald-700 hover:bg-emerald-100 border border-emerald-200'
                        }`}
                        title={dev.isActive ? 'Inactivar Desarrollador' : 'Activar Desarrollador'}
                      >
                        {dev.isActive ? <XCircle className="w-3.5 h-3.5" /> : <CheckCircle className="w-3.5 h-3.5" />}
                        <span>{dev.isActive ? 'Inactivar' : 'Activar'}</span>
                      </button>

                      <button
                        onClick={() => navigate(`/admin/developers/edit/${dev.id}`)}
                        className="inline-flex items-center gap-1 px-2.5 py-1.5 bg-indigo-50 hover:bg-indigo-100 text-royal-700 rounded-xl font-bold text-xs border border-indigo-200 transition-all"
                        title="Editar Desarrollador"
                      >
                        <Pencil className="w-3.5 h-3.5" />
                        <span>Editar</span>
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
