import React, { useState, useEffect } from 'react';
import { Modal } from '../common/Modal';
import { adminService } from '../../api/services';
import { UserCheck } from 'lucide-react';

interface ReassignModalProps {
  isOpen: boolean;
  onClose: () => void;
  propertyId: number;
  propertyCode: string;
  onSuccess: () => void;
}

export const ReassignModal: React.FC<ReassignModalProps> = ({
  isOpen,
  onClose,
  propertyId,
  propertyCode,
  onSuccess,
}) => {
  const [agents, setAgents] = useState<any[]>([]);
  const [selectedAgentId, setSelectedAgentId] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (isOpen) {
      adminService.getAgents().then((data) => {
        setAgents(data.filter((a: any) => a.isActive !== false));
        setIsLoading(false);
      }).catch(() => setIsLoading(false));
    }
  }, [isOpen]);

  const handleSubmit = async () => {
    if (!selectedAgentId) {
      setError('Selecciona un agente');
      return;
    }
    setIsSubmitting(true);
    setError('');
    try {
      await adminService.reassignProperties('', '');
      onSuccess();
      onClose();
    } catch {
      setError('Error al reasignar la propiedad');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={`Reasignar Propiedad #${propertyCode}`} maxWidth="md">
      <div className="space-y-4">
        {isLoading ? (
          <div className="py-8 text-center text-sm text-slate-500">Cargando agentes...</div>
        ) : (
          <>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Agente destino</label>
              <select
                value={selectedAgentId}
                onChange={(e) => setSelectedAgentId(e.target.value)}
                className="w-full border border-slate-300 rounded-xl px-4 py-2.5 text-sm focus:ring-2 focus:ring-brand-500 focus:border-brand-500"
              >
                <option value="">Seleccionar agente...</option>
                {agents.map((agent) => (
                  <option key={agent.id} value={agent.id}>
                    {agent.userName || agent.email}
                  </option>
                ))}
              </select>
            </div>

            {error && <p className="text-sm text-rose-600">{error}</p>}

            <div className="flex justify-end gap-3 pt-2">
              <button
                onClick={onClose}
                className="px-4 py-2 text-sm font-medium text-slate-700 bg-slate-100 hover:bg-slate-200 rounded-xl transition-colors"
              >
                Cancelar
              </button>
              <button
                onClick={handleSubmit}
                disabled={isSubmitting || !selectedAgentId}
                className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-brand-600 hover:bg-brand-700 rounded-xl shadow-sm disabled:opacity-50 transition-colors"
              >
                <UserCheck className="w-4 h-4" />
                {isSubmitting ? 'Reasignando...' : 'Reasignar'}
              </button>
            </div>
          </>
        )}
      </div>
    </Modal>
  );
};
