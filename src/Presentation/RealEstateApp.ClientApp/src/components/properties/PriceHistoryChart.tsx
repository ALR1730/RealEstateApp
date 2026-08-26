import React from 'react';
import { PriceHistory } from '../../types';
import { formatCurrencyRD } from '../../utils/formatters';
import { TrendingUp, TrendingDown, Calendar, Minus } from 'lucide-react';

interface PriceHistoryChartProps {
  history: PriceHistory[];
}

export const PriceHistoryChart: React.FC<PriceHistoryChartProps> = ({ history }) => {
  if (!history || history.length === 0) {
    return (
      <div className="bg-white rounded-3xl p-6 sm:p-8 border border-slate-200 space-y-4 shadow-xs">
        <h3 className="font-extrabold text-lg text-slate-900">Historial de Precios</h3>
        <p className="text-xs text-slate-500">No hay cambios de precio registrados para esta propiedad.</p>
      </div>
    );
  }

  const sortedHistory = [...history].sort(
    (a, b) => new Date(b.changeDate).getTime() - new Date(a.changeDate).getTime()
  );

  return (
    <div className="bg-white rounded-3xl p-6 sm:p-8 border border-slate-200 space-y-4 shadow-xs">
      <h3 className="font-extrabold text-lg text-slate-900">Historial de Precios</h3>

      <div className="relative">
        {/* Vertical line */}
        <div className="absolute left-[15px] top-2 bottom-2 w-0.5 bg-slate-200" />

        <div className="space-y-6">
          {sortedHistory.map((entry, index) => {
            const isIncrease = entry.percentageChange > 0;
            const isDecrease = entry.percentageChange < 0;
            const isNeutral = entry.percentageChange === 0;

            return (
              <div key={entry.id} className="relative flex gap-4">
                {/* Dot */}
                <div className={`relative z-10 shrink-0 w-8 h-8 rounded-full flex items-center justify-center border-2 ${
                  isIncrease
                    ? 'bg-emerald-50 border-emerald-400'
                    : isDecrease
                    ? 'bg-rose-50 border-rose-400'
                    : 'bg-slate-50 border-slate-300'
                }`}>
                  {isIncrease ? (
                    <TrendingUp className="w-4 h-4 text-emerald-600" />
                  ) : isDecrease ? (
                    <TrendingDown className="w-4 h-4 text-rose-600" />
                  ) : (
                    <Minus className="w-4 h-4 text-slate-500" />
                  )}
                </div>

                {/* Content */}
                <div className="flex-1 pb-2">
                  <div className="flex flex-wrap items-center gap-2 mb-1">
                    <span className="flex items-center gap-1 text-[11px] font-semibold text-slate-400">
                      <Calendar className="w-3.5 h-3.5" />
                      {new Date(entry.changeDate).toLocaleDateString('es-DO', {
                        day: '2-digit',
                        month: 'short',
                        year: 'numeric',
                      })}
                    </span>
                    {index === 0 && (
                      <span className="text-[10px] font-extrabold uppercase tracking-wider px-2 py-0.5 bg-brand-50 text-brand-700 rounded-full border border-brand-200/60">
                        Más reciente
                      </span>
                    )}
                  </div>

                  <div className="flex flex-wrap items-baseline gap-x-3 gap-y-1">
                    <div className="text-xs text-slate-500">
                      <span className="font-semibold">Anterior:</span>{' '}
                      <span className="font-mono font-bold text-slate-700">{formatCurrencyRD(entry.oldPrice)}</span>
                    </div>
                    <div className="text-xs text-slate-500">
                      <span className="font-semibold">Nuevo:</span>{' '}
                      <span className="font-mono font-bold text-slate-900">{formatCurrencyRD(entry.newPrice)}</span>
                    </div>
                    <span className={`inline-flex items-center gap-1 text-[11px] font-extrabold px-2 py-0.5 rounded-full ${
                      isIncrease
                        ? 'bg-emerald-50 text-emerald-700 border border-emerald-200'
                        : isDecrease
                        ? 'bg-rose-50 text-rose-700 border border-rose-200'
                        : 'bg-slate-50 text-slate-600 border border-slate-200'
                    }`}>
                      {isIncrease ? '+' : ''}{entry.percentageChange.toFixed(1)}%
                    </span>
                  </div>

                  {entry.changeReason && (
                    <p className="mt-1.5 text-[11px] text-slate-500 italic">
                      Motivo: {entry.changeReason}
                    </p>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
};
