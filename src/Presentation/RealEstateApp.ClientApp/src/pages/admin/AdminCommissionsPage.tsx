import React, { useState, useEffect } from 'react';
import { commissionsService } from '../../api/services';
import { Commission } from '../../types';
import { formatCurrencyRD, formatDate } from '../../utils/formatters';
import { Badge } from '../../components/common/Badge';
import { Loader } from '../../components/common/Loader';
import { Wallet, CheckCircle2, RefreshCw } from 'lucide-react';

export const AdminCommissionsPage: React.FC = () => {
  const [commissions, setCommissions] = useState<Commission[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isUpdating, setIsUpdating] = useState<number | null>(null);

  const loadAll = async () => {
    try {
      const data = await commissionsService.getAll();
      setCommissions(data || []);
    } catch (err) {
      console.error("Error loading commissions:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    const load = async () => {
      try {
        const data = await commissionsService.getAll();
        setCommissions(data || []);
      } catch (err) {
        console.error("Error loading commissions:", err);
      } finally {
        setIsLoading(false);
      }
    };
    load();
  }, []);

  const handleMarkAsPaid = async (id: number) => {
    if (!window.confirm("¿Marcar esta comisión como PAGADA?")) return;
    try {
      setIsUpdating(id);
      await commissionsService.markAsPaid(id);
      await loadAll();
    } catch (err) {
      console.error("Error marking commission as paid:", err);
      alert("Error al actualizar la comisión.");
    } finally {
      setIsUpdating(null);
    }
  };

  const totals = commissions.reduce(
    (acc, c) => {
      if (c.status === 'Pagada') {
        acc.paid += c.amount;
        acc.paidCount += 1;
      } else {
        acc.pending += c.amount;
        acc.pendingCount += 1;
      }
      return acc;
    },
    { paid: 0, pending: 0, paidCount: 0, pendingCount: 0 }
  );

  if (isLoading) return <Loader text="Cargando comisiones..." />;

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-4 border-b border-slate-200">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <Wallet className="w-6 h-6 text-emerald-600" />
            Gestión de Comisiones
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Comisiones de agentes generadas por ventas cerradas. Aquí puedes marcarlas como pagadas.
          </p>
        </div>
        <button
          onClick={loadAll}
          className="inline-flex items-center gap-1.5 px-3 py-2 bg-white border border-slate-200 text-slate-600 rounded-xl font-bold text-xs hover:bg-slate-50 transition-colors"
        >
          <RefreshCw className="w-3.5 h-3.5" />
          Refrescar
        </button>
      </div>

      {/* Summary */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-xs space-y-1">
          <span className="text-[11px] font-bold text-slate-400 uppercase">Por Pagar</span>
          <p className="text-xl font-extrabold font-mono text-amber-600">{formatCurrencyRD(totals.pending)}</p>
          <p className="text-[10px] text-slate-400 font-semibold">{totals.pendingCount} comisiones pendientes</p>
        </div>
        <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-xs space-y-1">
          <span className="text-[11px] font-bold text-slate-400 uppercase">Pagadas</span>
          <p className="text-xl font-extrabold font-mono text-emerald-600">{formatCurrencyRD(totals.paid)}</p>
          <p className="text-[10px] text-slate-400 font-semibold">{totals.paidCount} comisiones pagadas</p>
        </div>
        <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-xs space-y-1">
          <span className="text-[11px] font-bold text-slate-400 uppercase">Total Generado</span>
          <p className="text-xl font-extrabold font-mono text-slate-900">{formatCurrencyRD(totals.paid + totals.pending)}</p>
          <p className="text-[10px] text-slate-400 font-semibold">{commissions.length} transacciones</p>
        </div>
      </div>

      {commissions.length === 0 ? (
        <div className="bg-white rounded-3xl p-12 text-center border border-slate-200 space-y-2">
          <Wallet className="w-12 h-12 text-slate-300 mx-auto" />
          <h3 className="text-base font-bold text-slate-800">Sin comisiones registradas</h3>
          <p className="text-xs text-slate-500 max-w-sm mx-auto">
            Las comisiones se generan automáticamente cuando un agente acepta una oferta de compra.
          </p>
        </div>
      ) : (
        <div className="bg-white rounded-3xl border border-slate-200 overflow-hidden shadow-xs">
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse text-xs">
              <thead>
                <tr className="bg-slate-50 text-slate-700 font-extrabold uppercase tracking-wider border-b border-slate-200">
                  <th className="py-3.5 px-4">Agente</th>
                  <th className="py-3.5 px-4">Propiedad</th>
                  <th className="py-3.5 px-4">Precio Venta</th>
                  <th className="py-3.5 px-4">Tasa</th>
                  <th className="py-3.5 px-4">Comisión</th>
                  <th className="py-3.5 px-4">Fecha</th>
                  <th className="py-3.5 px-4">Estado</th>
                  <th className="py-3.5 px-4 text-right">Acción</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {commissions.map((c) => (
                  <tr key={c.id} className="hover:bg-slate-50/80 transition-colors">
                    <td className="py-3 px-4 font-bold text-slate-800">{c.agentName || c.agentId}</td>
                    <td className="py-3 px-4">
                      <span className="font-mono font-bold text-slate-700">#{c.propertyCode || c.propertyId}</span>
                      <p className="text-[11px] text-slate-500 truncate max-w-xs">{c.propertyName}</p>
                    </td>
                    <td className="py-3 px-4 font-mono font-bold text-slate-800">{formatCurrencyRD(c.salePrice)}</td>
                    <td className="py-3 px-4 font-mono font-bold text-brand-600">{c.rate.toFixed(2)}%</td>
                    <td className="py-3 px-4 font-mono font-bold text-emerald-600">{formatCurrencyRD(c.amount)}</td>
                    <td className="py-3 px-4 text-slate-600 font-medium">{formatDate(c.created)}</td>
                    <td className="py-3 px-4">
                      <Badge status={c.status} />
                    </td>
                    <td className="py-3 px-4 text-right">
                      {c.status === 'Pendiente' ? (
                        <button
                          onClick={() => handleMarkAsPaid(c.id)}
                          disabled={isUpdating === c.id}
                          className="inline-flex items-center gap-1 px-3 py-1.5 bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 text-white rounded-xl font-bold text-[11px] transition-all"
                        >
                          <CheckCircle2 className="w-3.5 h-3.5" />
                          {isUpdating === c.id ? 'Procesando...' : 'Marcar Pagada'}
                        </button>
                      ) : (
                        <span className="text-[11px] font-bold text-slate-300 uppercase tracking-wider">Completada</span>
                      )}
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