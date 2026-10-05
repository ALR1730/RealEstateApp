import React, { useState, useEffect } from 'react';
import { commissionsService } from '../../api/services';
import { Commission } from '../../types';
import { formatCurrencyRD, formatDate } from '../../utils/formatters';
import { Badge } from '../../components/common/Badge';
import { Loader } from '../../components/common/Loader';
import { Wallet, CheckCircle2, RefreshCw } from 'lucide-react';
import { useLanguage } from '../../context/LanguageContext';

export const AdminCommissionsPage: React.FC = () => {
  const { t, language } = useLanguage();
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
    if (!window.confirm(t('admin.commissions.confirmMarkPaid', "¿Marcar esta comisión como PAGADA?"))) return;
    try {
      setIsUpdating(id);
      await commissionsService.markAsPaid(id);
      await loadAll();
    } catch (err) {
      console.error("Error marking commission as paid:", err);
      alert(t('admin.commissions.errorUpdating', "Error al actualizar la comisión."));
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

  if (isLoading) return <Loader text={t('admin.commissions.loading', "Cargando comisiones...")} />;

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-4 border-b border-slate-200 dark:border-slate-800">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 dark:text-white tracking-tight flex items-center gap-2">
            <Wallet className="w-6 h-6 text-emerald-600 dark:text-emerald-400" />
            {t('admin.commissions.title', "Gestión de Comisiones")}
          </h2>
          <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">
            {t('admin.commissions.subtitle', "Comisiones de agentes generadas por ventas cerradas. Aquí puedes marcarlas como pagadas.")}
          </p>
        </div>
        <button
          onClick={loadAll}
          className="inline-flex items-center gap-1.5 px-3 py-2 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-slate-600 dark:text-slate-300 rounded-xl font-bold text-xs hover:bg-slate-50 dark:hover:bg-slate-700/60 transition-colors"
        >
          <RefreshCw className="w-3.5 h-3.5" />
          {t('admin.commissions.refresh', "Refrescar")}
        </button>
      </div>

      {/* Summary */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="bg-white dark:bg-slate-900 p-5 rounded-2xl border border-slate-200 dark:border-slate-800 shadow-xs space-y-1">
          <span className="text-[11px] font-bold text-slate-400 dark:text-slate-500 uppercase">{t('admin.commissions.pendingPay', "Por Pagar")}</span>
          <p className="text-xl font-extrabold font-mono text-amber-600 dark:text-amber-400">{formatCurrencyRD(totals.pending)}</p>
          <p className="text-[10px] text-slate-400 dark:text-slate-500 font-semibold">
            {totals.pendingCount} {totals.pendingCount === 1 ? t('admin.commissions.pendingCountSingle', "comisión pendiente") : t('admin.commissions.pendingCount', "comisiones pendientes")}
          </p>
        </div>
        <div className="bg-white dark:bg-slate-900 p-5 rounded-2xl border border-slate-200 dark:border-slate-800 shadow-xs space-y-1">
          <span className="text-[11px] font-bold text-slate-400 dark:text-slate-500 uppercase">{t('admin.commissions.paid', "Pagadas")}</span>
          <p className="text-xl font-extrabold font-mono text-emerald-600 dark:text-emerald-400">{formatCurrencyRD(totals.paid)}</p>
          <p className="text-[10px] text-slate-400 dark:text-slate-500 font-semibold">
            {totals.paidCount} {totals.paidCount === 1 ? t('admin.commissions.paidCountSingle', "comisión pagada") : t('admin.commissions.paidCount', "comisiones pagadas")}
          </p>
        </div>
        <div className="bg-white dark:bg-slate-900 p-5 rounded-2xl border border-slate-200 dark:border-slate-800 shadow-xs space-y-1">
          <span className="text-[11px] font-bold text-slate-400 dark:text-slate-500 uppercase">{t('admin.commissions.totalGenerated', "Total Generado")}</span>
          <p className="text-xl font-extrabold font-mono text-slate-900 dark:text-white">{formatCurrencyRD(totals.paid + totals.pending)}</p>
          <p className="text-[10px] text-slate-400 dark:text-slate-500 font-semibold">
            {commissions.length} {commissions.length === 1 ? t('admin.commissions.transactionSingle', "transacción") : t('admin.commissions.transactions', "transacciones")}
          </p>
        </div>
      </div>

      {commissions.length === 0 ? (
        <div className="bg-white dark:bg-slate-900 rounded-3xl p-12 text-center border border-slate-200 dark:border-slate-800 space-y-2">
          <Wallet className="w-12 h-12 text-slate-300 dark:text-slate-600 mx-auto" />
          <h3 className="text-base font-bold text-slate-800 dark:text-slate-200">{t('admin.commissions.emptyTitle', "Sin comisiones registradas")}</h3>
          <p className="text-xs text-slate-500 dark:text-slate-400 max-w-sm mx-auto">
            {t('admin.commissions.emptyDesc', "Las comisiones se generan automáticamente cuando un agente acepta una oferta de compra.")}
          </p>
        </div>
      ) : (
        <div className="bg-white dark:bg-slate-900 rounded-3xl border border-slate-200 dark:border-slate-800 overflow-hidden shadow-xs">
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse text-xs">
              <thead>
                <tr className="bg-slate-50 dark:bg-slate-800/70 text-slate-700 dark:text-slate-300 font-extrabold uppercase tracking-wider border-b border-slate-200 dark:border-slate-800">
                  <th className="py-3.5 px-4">{t('admin.commissions.colAgent', "Agente")}</th>
                  <th className="py-3.5 px-4">{t('admin.commissions.colProperty', "Propiedad")}</th>
                  <th className="py-3.5 px-4">{t('admin.commissions.colSalePrice', "Precio Venta")}</th>
                  <th className="py-3.5 px-4">{t('admin.commissions.colRate', "Tasa")}</th>
                  <th className="py-3.5 px-4">{t('admin.commissions.colCommission', "Comisión")}</th>
                  <th className="py-3.5 px-4">{t('admin.commissions.colDate', "Fecha")}</th>
                  <th className="py-3.5 px-4">{t('admin.commissions.colStatus', "Estado")}</th>
                  <th className="py-3.5 px-4 text-right">{t('admin.commissions.colAction', "Acción")}</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 dark:divide-slate-800">
                {commissions.map((c) => (
                  <tr key={c.id} className="hover:bg-slate-50/80 dark:hover:bg-slate-800/40 transition-colors">
                    <td className="py-3 px-4 font-bold text-slate-800 dark:text-slate-200">{c.agentName || c.agentId}</td>
                    <td className="py-3 px-4">
                      <span className="font-mono font-bold text-slate-700 dark:text-slate-300">#{c.propertyCode || c.propertyId}</span>
                      <p className="text-[11px] text-slate-500 dark:text-slate-400 truncate max-w-xs">{c.propertyName}</p>
                    </td>
                    <td className="py-3 px-4 font-mono font-bold text-slate-800 dark:text-slate-200">{formatCurrencyRD(c.salePrice)}</td>
                    <td className="py-3 px-4 font-mono font-bold text-brand-600 dark:text-brand-400">{c.rate.toFixed(2)}%</td>
                    <td className="py-3 px-4 font-mono font-bold text-emerald-600 dark:text-emerald-400">{formatCurrencyRD(c.amount)}</td>
                    <td className="py-3 px-4 text-slate-600 dark:text-slate-400 font-medium">{formatDate(c.created, language === 'en' ? 'en-US' : 'es-DO')}</td>
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
                          {isUpdating === c.id ? t('admin.common.loading', 'Procesando...') : t('admin.commissions.markPaid', 'Marcar Pagada')}
                        </button>
                      ) : (
                        <span className="text-[11px] font-bold text-slate-300 dark:text-slate-600 uppercase tracking-wider">{t('admin.commissions.completed', "Completada")}</span>
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