import React, { useState, useEffect } from 'react';
import { verificationsService } from '../../api/services';
import { AgentVerification } from '../../types';
import { Loader } from '../../components/common/Loader';
import { ShieldCheck, Eye, AlertCircle } from 'lucide-react';

export const ManageVerificationsPage: React.FC = () => {
  const [verifications, setVerifications] = useState<AgentVerification[]>([]);
  const [selectedKyc, setSelectedKyc] = useState<AgentVerification | null>(null);
  const [rejectionReason, setRejectionReason] = useState('');
  const [showRejectModal, setShowRejectModal] = useState(false);
  const [showDocModal, setShowDocModal] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [isProcessing, setIsProcessing] = useState(false);
  const [filterStatus, setFilterStatus] = useState<string>('All');
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => {
    let isMounted = true;
    const fetchVerifications = async () => {
      try {
        const data = await verificationsService.getAll();
        if (isMounted) setVerifications(data || []);
      } catch (err) {
        console.error("Error loading verifications:", err);
      } finally {
        if (isMounted) setIsLoading(false);
      }
    };

    fetchVerifications();
    return () => {
      isMounted = false;
    };
  }, [refreshKey]);

  const handleApprove = async (id: number) => {
    if (!window.confirm("¿Aprobar esta verificación KYC y otorgar la insignia de Agente Verificado?")) return;
    try {
      setIsProcessing(true);
      await verificationsService.approve(id);
      setRefreshKey((k) => k + 1);
    } catch (err) {
      console.error("Error approving verification:", err);
    } finally {
      setIsProcessing(false);
    }
  };

  const handleRejectConfirm = async () => {
    if (!selectedKyc || !rejectionReason.trim()) return;
    try {
      setIsProcessing(true);
      await verificationsService.reject(selectedKyc.id, rejectionReason.trim());
      setShowRejectModal(false);
      setRejectionReason('');
      setRefreshKey((k) => k + 1);
    } catch (err) {
      console.error("Error rejecting verification:", err);
    } finally {
      setIsProcessing(false);
    }
  };

  if (isLoading) return <Loader text="Cargando solicitudes de verificación KYC..." />;

  const filtered = verifications.filter((v) => {
    if (filterStatus === 'All') return true;
    if (filterStatus === 'Pending') return v.status === 'Pending' || v.status === 'Pendiente';
    if (filterStatus === 'Approved') return v.status === 'Approved' || v.status === 'Aprobado';
    return v.status === filterStatus;
  });

  return (
    <div className="space-y-8 pb-16">
      
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-4 border-b border-slate-200">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <ShieldCheck className="w-7 h-7 text-brand-600" />
            Validación de Identidad KYC de Agentes
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Revisa las cédulas oficiales y otorga la insignia de Agente Verificado a los corredores aprobados.
          </p>
        </div>

        <div className="flex items-center gap-2">
          <button
            onClick={() => setFilterStatus('All')}
            className={`px-3 py-1.5 rounded-xl text-xs font-bold ${
              filterStatus === 'All' ? 'bg-slate-900 text-white' : 'bg-slate-100 text-slate-600'
            }`}
          >
            Todos ({verifications.length})
          </button>
          <button
            onClick={() => setFilterStatus('Pending')}
            className={`px-3 py-1.5 rounded-xl text-xs font-bold ${
              filterStatus === 'Pending' ? 'bg-amber-500 text-navy-950' : 'bg-amber-50 text-amber-800'
            }`}
          >
            Pendientes ({verifications.filter((v) => v.status === 'Pending' || v.status === 'Pendiente').length})
          </button>
          <button
            onClick={() => setFilterStatus('Approved')}
            className={`px-3 py-1.5 rounded-xl text-xs font-bold ${
              filterStatus === 'Approved' ? 'bg-emerald-600 text-white' : 'bg-emerald-50 text-emerald-800'
            }`}
          >
            Aprobados ({verifications.filter((v) => v.status === 'Approved' || v.status === 'Aprobado').length})
          </button>
        </div>
      </div>

      {/* Verifications Table */}
      {filtered.length === 0 ? (
        <div className="bg-white rounded-3xl p-12 text-center border border-slate-200">
          <p className="text-xs font-bold text-slate-500">No hay solicitudes en este estado.</p>
        </div>
      ) : (
        <div className="bg-white rounded-3xl border border-slate-200 shadow-xs overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs">
              <thead className="bg-slate-50 border-b border-slate-200 text-slate-500 font-extrabold uppercase tracking-wider">
                <tr>
                  <th className="px-6 py-4">Agente</th>
                  <th className="px-6 py-4">Cédula Oficial</th>
                  <th className="px-6 py-4">Documentos</th>
                  <th className="px-6 py-4">Estado</th>
                  <th className="px-6 py-4 text-right">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {filtered.map((item) => (
                  <tr key={item.id} className="hover:bg-slate-50/50">
                    <td className="px-6 py-4">
                      <p className="font-bold text-slate-900">{item.agentName || 'Agente'}</p>
                      <p className="text-[11px] text-slate-500">{item.agentEmail}</p>
                    </td>
                    <td className="px-6 py-4 font-mono font-bold text-slate-700">
                      {item.cedula}
                    </td>
                    <td className="px-6 py-4">
                      <button
                        onClick={() => {
                          setSelectedKyc(item);
                          setShowDocModal(true);
                        }}
                        className="inline-flex items-center gap-1.5 px-3 py-1 bg-slate-100 hover:bg-slate-200 text-slate-800 rounded-lg font-bold text-[11px] transition-colors"
                      >
                        <Eye className="w-3.5 h-3.5" />
                        <span>Ver Cédula Front/Back</span>
                      </button>
                    </td>
                    <td className="px-6 py-4">
                      <span
                        className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-bold ${
                          (item.status === 'Approved' || item.status === 'Aprobado')
                            ? 'bg-emerald-50 text-emerald-700'
                            : (item.status === 'Pending' || item.status === 'Pendiente')
                            ? 'bg-amber-50 text-amber-700'
                            : 'bg-rose-50 text-rose-700'
                        }`}
                      >
                        {(item.status === 'Approved' || item.status === 'Aprobado') && 'Aprobado'}
                        {(item.status === 'Pending' || item.status === 'Pendiente') && 'Pendiente de Revisión'}
                        {(item.status === 'Rejected' || item.status === 'Rechazado') && 'Rechazado'}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-right space-x-2">
                      {(item.status === 'Pending' || item.status === 'Pendiente') && (
                        <>
                          <button
                            onClick={() => handleApprove(item.id)}
                            disabled={isProcessing}
                            className="px-3 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl font-bold text-xs shadow-sm"
                          >
                            Aprobar
                          </button>
                          <button
                            onClick={() => {
                              setSelectedKyc(item);
                              setShowRejectModal(true);
                            }}
                            disabled={isProcessing}
                            className="px-3 py-1.5 bg-rose-600 hover:bg-rose-700 text-white rounded-xl font-bold text-xs shadow-sm"
                          >
                            Rechazar
                          </button>
                        </>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Doc Viewer Modal */}
      {showDocModal && selectedKyc && (
        <div className="fixed inset-0 z-50 bg-black/70 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-3xl p-6 sm:p-8 max-w-2xl w-full shadow-2xl space-y-6">
            <div className="flex justify-between items-center pb-3 border-b border-slate-100">
              <h3 className="font-extrabold text-base text-slate-900">
                Documentos KYC: {selectedKyc.agentName} (Cédula {selectedKyc.cedula})
              </h3>
              <button onClick={() => setShowDocModal(false)} className="text-slate-400 font-bold">✕</button>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div className="space-y-1">
                <span className="text-xs font-bold text-slate-600 uppercase">Lado Frontal</span>
                <div className="aspect-[4/3] rounded-2xl overflow-hidden border border-slate-200 bg-slate-100">
                  {selectedKyc.cedulaFrontImageUrl ? (
                    <img src={selectedKyc.cedulaFrontImageUrl} alt="Front" className="w-full h-full object-cover" />
                  ) : (
                    <div className="flex items-center justify-center h-full text-slate-400 text-xs font-bold">Sin foto frontal</div>
                  )}
                </div>
              </div>

              <div className="space-y-1">
                <span className="text-xs font-bold text-slate-600 uppercase">Lado Posterior</span>
                <div className="aspect-[4/3] rounded-2xl overflow-hidden border border-slate-200 bg-slate-100">
                  {selectedKyc.cedulaBackImageUrl ? (
                    <img src={selectedKyc.cedulaBackImageUrl} alt="Back" className="w-full h-full object-cover" />
                  ) : (
                    <div className="flex items-center justify-center h-full text-slate-400 text-xs font-bold">Sin foto posterior</div>
                  )}
                </div>
              </div>
            </div>

            <div className="flex justify-end pt-2">
              <button
                onClick={() => setShowDocModal(false)}
                className="px-5 py-2 bg-slate-900 text-white rounded-xl text-xs font-bold"
              >
                Cerrar Visor
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Reject Modal */}
      {showRejectModal && selectedKyc && (
        <div className="fixed inset-0 z-50 bg-black/60 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-3xl p-6 sm:p-8 max-w-md w-full shadow-2xl space-y-4">
            <h3 className="font-extrabold text-base text-slate-900 flex items-center gap-2">
              <AlertCircle className="w-5 h-5 text-rose-600" />
              Rechazar Verificación de Agente
            </h3>
            <p className="text-xs text-slate-500">
              Indica la razón por la cual no se aprobó el documento para que el agente pueda corregirlo.
            </p>
            <textarea
              rows={3}
              required
              placeholder="Ej: La imagen posterior es borrosa y el número de cédula no coincide..."
              value={rejectionReason}
              onChange={(e) => setRejectionReason(e.target.value)}
              className="w-full p-3 text-xs rounded-xl border border-slate-200 focus:ring-2 focus:ring-rose-500"
            />
            <div className="flex gap-2 justify-end pt-2">
              <button
                onClick={() => setShowRejectModal(false)}
                className="px-4 py-2 bg-slate-100 text-slate-700 rounded-xl text-xs font-bold"
              >
                Cancelar
              </button>
              <button
                onClick={handleRejectConfirm}
                disabled={isProcessing || !rejectionReason.trim()}
                className="px-5 py-2 bg-rose-600 text-white rounded-xl text-xs font-bold disabled:opacity-50"
              >
                {isProcessing ? 'Procesando...' : 'Confirmar Rechazo'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
