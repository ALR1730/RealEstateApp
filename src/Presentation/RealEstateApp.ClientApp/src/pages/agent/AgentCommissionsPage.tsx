import React, { useState, useEffect } from 'react';
import { commissionsService } from '../../api/services';
import { Commission, CommissionSummary } from '../../types';
import { formatCurrencyRD, formatDate } from '../../utils/formatters';
import { Badge } from '../../components/common/Badge';
import { Loader } from '../../components/common/Loader';
import { Wallet, TrendingUp, CheckCircle2, Clock, Percent } from 'lucide-react';

export const AgentCommissionsPage: React.FC = () => {
  const [commissions, setCommissions] = useState<Commission[]>([]);
  const [summary, setSummary] = useState<CommissionSummary | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const [commissionsData, summaryData] = await Promise.all([
          commissionsService.getMyCommissions(),
          commissionsService.getMySummary(),
        ]);
        setCommissions(commissionsData || []);
        setSummary(summaryData || null);
      } catch (err) {
        console.error("Error loading commissions:", err);
      } finally {
        setIsLoading(false);
      }
    };
    load();
  }, []);

  if (isLoading) return <Loader text="Consultando tus comisiones..." />;

  return (
    <div className="space-y-6">
      <div className="pb-4 border-b border-slate-200">
        <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
          <Wallet className="w-6 h-6 text-emerald-600" />
          Mis Comisiones por Venta
        </h2>
        <p className="text-xs text-slate-500 mt-0.5">
          Comisiones generadas automáticamente al aceptar ofertas, según la tasa de tu plan de suscripción.
        </p>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-xs space-y-1">
          <span className="text-[11px] font-bold text-slate-400 uppercase flex items-center gap-1">
            <TrendingUp className="w-3.5 h-3.5" /> Total Generado
          </span>
          <p className="text-xl font-extrabold font-mono text-slate-900">
            {formatCurrencyRD(summary?.totalAmount || 0)}
          </p>
        </div>
        <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-xs space-y-1">
          <span className="text-[11px] font-bold text-slate-400 uppercase flex items-center gap-1">
            <Clock className="w-3.5 h-3.5" /> Pendientes de Cobro
          </span>
          <p className="text-xl font-extrabold font-mono text-amber-600">
            {formatCurrencyRD(summary?.pendingAmount || 0)}
          </p>
        </div>
        <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-xs space-y-1">
          <span className="text-[11px] font-bold text-slate-400 uppercase flex items-center gap-1">
            <CheckCircle2 className="w-3.5 h-3.5" /> Cobradas
          </span>
          <p className="text-xl font-extrabold font-mono text-emerald-600">
            {formatCurrencyRD(summary?.paidAmount || 0)}
          </p>
        </div>
        <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-xs space-y-1">
          <span className="text-[11px] font-bold text-slate-400 uppercase flex items-center gap-1">
            <Percent className="w-3.5 h-3.5" /> Tasa Promedio
          </span>
          <p className="text-xl font-extrabold font-mono text-brand-600">
            {summary?.averageRate?.toFixed(2) || '0.00'}%
          </p>
        </div>
      </div>

      {/* Commissions Table */}
      {commissions.length === 0 ? (
        <div className="bg-white rounded-3xl p-12 text-center border border-slate-200 space-y-2">
          <Wallet className="w-12 h-12 text-slate-300 mx-auto" />
          <h3 className="text-base font-bold text-slate-800">Aún no tienes comisiones registradas</h3>
          <p className="text-xs text-slate-500 max-w-sm mx-auto">
            Cuando aceptes una oferta de compra, el sistema generará automáticamente tu comisión según el plan activo.
          </p>
        </div>
      ) : (
        <div className="bg-white rounded-3xl border border-slate-200 overflow-hidden shadow-xs">
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse text-xs">
              <thead>
                <tr className="bg-slate-50 text-slate-700 font-extrabold uppercase tracking-wider border-b border-slate-200">
                  <th className="py-3.5 px-4">Propiedad</th>
                  <th className="py-3.5 px-4">Precio Venta</th>
                  <th className="py-3.5 px-4">Tasa</th>
                  <th className="py-3.5 px-4">Comisión</th>
                  <th className="py-3.5 px-4">Fecha</th>
                  <th className="py-3.5 px-4">Estado</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {commissions.map((c) => (
                  <tr key={c.id} className="hover:bg-slate-50/80 transition-colors">
                    <td className="py-3 px-4">
                      <span className="font-bold text-slate-800 font-mono">#{c.propertyCode || c.propertyId}</span>
                      <p className="text-[11px] text-slate-500 truncate max-w-xs">{c.propertyName || 'Inmueble'}</p>
                    </td>
                    <td className="py-3 px-4 font-mono font-bold text-slate-800">
                      {formatCurrencyRD(c.salePrice)}
                    </td>
                    <td className="py-3 px-4 font-mono font-bold text-brand-600">
                      {c.rate.toFixed(2)}%
                    </td>
                    <td className="py-3 px-4 font-mono font-bold text-emerald-600 text-sm">
                      {formatCurrencyRD(c.amount)}
                    </td>
                    <td className="py-3 px-4 text-slate-600 font-medium">
                      {formatDate(c.created)}
                    </td>
                    <td className="py-3 px-4">
                      <Badge status={c.status} />
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