import React, { useState, useEffect } from 'react';
import { adminService } from '../../api/services';
import { Badge } from '../../components/common/Badge';
import { Loader } from '../../components/common/Loader';
import { Modal } from '../../components/common/Modal';
import { 
  Users, 
  CheckCircle, 
  XCircle, 
  Trash2, 
  ArrowRightLeft, 
  ShieldCheck, 
  Search,
  AlertTriangle
} from 'lucide-react';

export const ManageAgentsPage: React.FC = () => {
  const [agents, setAgents] = useState<any[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');

  // Reassignment Modal State
  const [reassignModalOpen, setReassignModalOpen] = useState(false);
  const [sourceAgent, setSourceAgent] = useState<any>(null);
  const [targetAgentId, setTargetAgentId] = useState('');
  const [isReassigning, setIsReassigning] = useState(false);

  const loadAgents = async () => {
    try {
      const data = await adminService.getAgents();
      setAgents(data || []);
    } catch (err) {
      console.error("Error loading agents:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    Promise.resolve().then(() => loadAgents()).catch(console.error);
  }, []);

  const handleToggleStatus = async (agentId: string, currentStatus: boolean) => {
    try {
      await adminService.toggleAgentStatus(agentId, !currentStatus);
      loadAgents();
    } catch (err) {
      console.error("Error toggling agent status:", err);
    }
  };

  const handleDeleteAgent = async (agentId: string) => {
    if (!window.confirm("¿Seguro que deseas eliminar a este agente? Esta acción eliminará físicamente en cascada todas sus propiedades e imágenes asociadas.")) return;
    try {
      await adminService.deleteAgent(agentId);
      loadAgents();
    } catch (err) {
      console.error("Error deleting agent:", err);
    }
  };

  const handleReassignSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!sourceAgent || !targetAgentId) return;

    try {
      setIsReassigning(true);
      await adminService.reassignProperties(sourceAgent.id, targetAgentId);
      setReassignModalOpen(false);
      setSourceAgent(null);
      setTargetAgentId('');
      await loadAgents();
      alert("¡Cartera de propiedades reasignada exitosamente!");
    } catch (err: any) {
      console.error("Error reassigning properties:", err);
      alert(err.response?.data?.error || "Error al reasignar cartera.");
    } finally {
      setIsReassigning(false);
    }
  };

  const filtered = agents.filter((a) => {
    const term = search.toLowerCase();
    const fullName = `${a.firstName || ''} ${a.lastName || ''}`.toLowerCase();
    return fullName.includes(term) || (a.email && a.email.toLowerCase().includes(term));
  });

  return (
    <div className="space-y-6">
      
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 pb-4 border-b border-slate-200">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <Users className="w-6 h-6 text-royal-600" />
            Administración de Agentes Inmobiliarios
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Control de cuentas, activación/inactivación, reasignación de cartera y eliminación segura.
          </p>
        </div>

        <div className="w-full sm:w-64">
          <div className="relative">
            <Search className="w-4 h-4 absolute left-3 top-2.5 text-slate-400" />
            <input
              type="text"
              placeholder="Buscar agente..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="w-full pl-9 pr-3 py-2 text-xs rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500 bg-white"
            />
          </div>
        </div>
      </div>

      {isLoading ? (
        <Loader text="Cargando listado de agentes..." />
      ) : (
        <div className="bg-white rounded-3xl border border-slate-200 overflow-hidden shadow-xs">
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse text-xs">
              <thead>
                <tr className="bg-slate-50 text-slate-700 font-extrabold uppercase tracking-wider border-b border-slate-200">
                  <th className="py-3.5 px-4">Agente</th>
                  <th className="py-3.5 px-4">Contacto</th>
                  <th className="py-3.5 px-4">Propiedades</th>
                  <th className="py-3.5 px-4">Estado</th>
                  <th className="py-3.5 px-4 text-right">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {filtered.map((agent) => (
                  <tr key={agent.id} className="hover:bg-slate-50/80 transition-colors">
                    <td className="py-3 px-4">
                      <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-navy-800 to-slate-700 text-white flex items-center justify-center font-bold text-xs shrink-0 overflow-hidden">
                          {agent.photoUrl ? (
                            <img src={agent.photoUrl} alt="" className="w-full h-full object-cover" />
                          ) : (
                            agent.firstName ? agent.firstName[0] : 'A'
                          )}
                        </div>
                        <div>
                          <p className="font-bold text-slate-900">{agent.firstName} {agent.lastName}</p>
                          <span className="text-[11px] text-slate-500 font-mono">@{agent.userName}</span>
                        </div>
                      </div>
                    </td>
                    <td className="py-3 px-4 text-slate-600">
                      <p className="font-semibold text-slate-800">{agent.email}</p>
                      <span className="text-[11px] text-slate-500">{agent.phone || 'Sin teléfono'}</span>
                    </td>
                    <td className="py-3 px-4 font-mono font-bold text-slate-700 text-sm">
                      {agent.propertiesCount || 0}
                    </td>
                    <td className="py-3 px-4">
                      <Badge status={agent.isActive ? 'Active' : 'Inactive'} />
                    </td>
                    <td className="py-3 px-4 text-right space-x-1.5">
                      
                      {/* Toggle Status Button */}
                      <button
                        onClick={() => handleToggleStatus(agent.id, agent.isActive)}
                        className={`inline-flex items-center gap-1 px-2.5 py-1.5 rounded-xl font-bold text-xs transition-all ${
                          agent.isActive
                            ? 'bg-amber-50 text-amber-700 hover:bg-amber-100 border border-amber-200'
                            : 'bg-emerald-50 text-emerald-700 hover:bg-emerald-100 border border-emerald-200'
                        }`}
                        title={agent.isActive ? 'Inactivar Agente' : 'Activar Agente'}
                      >
                        {agent.isActive ? <XCircle className="w-3.5 h-3.5" /> : <CheckCircle className="w-3.5 h-3.5" />}
                        <span>{agent.isActive ? 'Inactivar' : 'Activar'}</span>
                      </button>

                      {/* Reassign Properties Button */}
                      <button
                        onClick={() => { setSourceAgent(agent); setReassignModalOpen(true); }}
                        className="inline-flex items-center gap-1 px-2.5 py-1.5 bg-indigo-50 hover:bg-indigo-100 text-royal-700 rounded-xl font-bold text-xs border border-indigo-200 transition-all"
                        title="Reasignar Propiedades a otro Agente"
                      >
                        <ArrowRightLeft className="w-3.5 h-3.5" />
                        <span>Reasignar</span>
                      </button>

                      {/* Delete Cascade Button */}
                      <button
                        onClick={() => handleDeleteAgent(agent.id)}
                        className="p-1.5 text-rose-500 hover:text-rose-700 hover:bg-rose-50 rounded-lg transition-colors"
                        title="Eliminar en Cascada"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Reassign Properties Modal */}
      {reassignModalOpen && sourceAgent && (
        <Modal
          isOpen={reassignModalOpen}
          onClose={() => setReassignModalOpen(false)}
          title={`Reasignar Cartera de ${sourceAgent.firstName} ${sourceAgent.lastName}`}
        >
          <form onSubmit={handleReassignSubmit} className="space-y-4">
            <div className="p-3.5 bg-indigo-50 rounded-2xl border border-indigo-100 text-xs text-royal-900 space-y-1">
              <p className="font-bold">Agente Origen: {sourceAgent.firstName} {sourceAgent.lastName}</p>
              <p className="text-royal-700">Inmuebles a transferir: <strong>{sourceAgent.propertiesCount || 0} propiedades</strong></p>
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5">
                Selecciona el Agente Destino *
              </label>
              <select
                required
                value={targetAgentId}
                onChange={(e) => setTargetAgentId(e.target.value)}
                className="w-full px-3.5 py-2.5 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500 bg-white"
              >
                <option value="">Selecciona un agente activo...</option>
                {agents
                  .filter((a) => a.id !== sourceAgent.id && a.isActive)
                  .map((a) => (
                    <option key={a.id} value={a.id}>
                      {a.firstName} {a.lastName} ({a.email})
                    </option>
                  ))}
              </select>
            </div>

            <div className="pt-3 flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setReassignModalOpen(false)}
                className="px-4 py-2 rounded-xl text-xs font-bold text-slate-600 hover:bg-slate-100"
              >
                Cancelar
              </button>
              <button
                type="submit"
                disabled={isReassigning || !targetAgentId}
                className="px-5 py-2 bg-royal-600 hover:bg-royal-700 disabled:opacity-50 text-white font-extrabold text-xs rounded-xl shadow-md"
              >
                {isReassigning ? 'Transfiriendo...' : 'Confirmar Reasignación'}
              </button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  );
};
