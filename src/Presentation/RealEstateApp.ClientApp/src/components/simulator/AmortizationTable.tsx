import React, { useState } from 'react';
import { AmortizationScheduleItem } from '../../types';
import { formatCurrencyRD } from '../../utils/formatters';
import { Printer, ChevronLeft, ChevronRight } from 'lucide-react';
import { useLanguage } from '../../context/LanguageContext';

interface AmortizationTableProps {
  schedule: AmortizationScheduleItem[];
}

export const AmortizationTable: React.FC<AmortizationTableProps> = ({ schedule }) => {
  const { t } = useLanguage();
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 12; // 1 año por página

  const totalPages = Math.ceil(schedule.length / itemsPerPage);
  const currentItems = schedule.slice((currentPage - 1) * itemsPerPage, currentPage * itemsPerPage);

  const handlePrint = () => {
    window.print();
  };

  return (
    <div className="space-y-4">
      {/* Header with print button */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3">
        <div>
          <h4 className="font-bold text-sm text-slate-900 dark:text-white">
            {t('mortgage.tableTitle', 'Tabla de Cuotas Mensuales — Año')} {currentPage} ({t('mortgage.months', 'Meses')} {(currentPage - 1) * 12 + 1} {t('mortgage.of', 'al')} {Math.min(currentPage * 12, schedule.length)})
          </h4>
          <p className="text-xs text-slate-500 dark:text-slate-400">
            {t('mortgage.tableSubtitle', 'Desglose de amortización del capital y distribución de intereses.')}
          </p>
        </div>

        <button
          onClick={handlePrint}
          className="flex items-center gap-1.5 px-3 py-1.5 bg-slate-100 dark:bg-slate-800 hover:bg-slate-200 dark:hover:bg-slate-700 text-slate-700 dark:text-slate-200 rounded-lg text-xs font-semibold transition-colors"
        >
          <Printer className="w-4 h-4 text-slate-600 dark:text-slate-400" />
          <span>{t('mortgage.print', 'Imprimir / PDF')}</span>
        </button>
      </div>

      {/* Table Container */}
      <div className="overflow-x-auto rounded-2xl border border-slate-200 dark:border-slate-800 shadow-xs">
        <table className="w-full text-left border-collapse text-xs">
          <thead>
            <tr className="bg-slate-100/80 dark:bg-slate-800/90 text-slate-700 dark:text-slate-300 font-extrabold uppercase tracking-wider border-b border-slate-200 dark:border-slate-700">
              <th className="py-3 px-4">{t('mortgage.month', 'Mes')}</th>
              <th className="py-3 px-4">{t('mortgage.fixedInstallmentCol', 'Cuota Fija (RD$)')}</th>
              <th className="py-3 px-4">{t('mortgage.interestCol', 'Interés (RD$)')}</th>
              <th className="py-3 px-4">{t('mortgage.principalCol', 'Capital (RD$)')}</th>
              <th className="py-3 px-4">{t('mortgage.balanceCol', 'Balance Restante (RD$)')}</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100 dark:divide-slate-800 font-mono">
            {currentItems.map((item) => (
              <tr key={item.period} className="hover:bg-slate-50/80 dark:hover:bg-slate-800/40 transition-colors">
                <td className="py-2.5 px-4 font-bold text-slate-800 dark:text-slate-200 font-sans">{t('mortgage.month', 'Mes')} {item.period}</td>
                <td className="py-2.5 px-4 font-bold text-emerald-600 dark:text-emerald-400">{formatCurrencyRD(item.installment)}</td>
                <td className="py-2.5 px-4 text-amber-600 dark:text-amber-400">{formatCurrencyRD(item.interest)}</td>
                <td className="py-2.5 px-4 text-royal-600 dark:text-royal-400">{formatCurrencyRD(item.principal)}</td>
                <td className="py-2.5 px-4 text-slate-700 dark:text-slate-300 font-semibold">{formatCurrencyRD(item.remainingBalance)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Pagination Controls */}
      {totalPages > 1 && (
        <div className="flex justify-between items-center pt-2">
          <button
            disabled={currentPage === 1}
            onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
            className="flex items-center gap-1 px-3 py-1.5 rounded-lg border border-slate-200 dark:border-slate-700 text-slate-700 dark:text-slate-300 text-xs font-bold disabled:opacity-40 hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors"
          >
            <ChevronLeft className="w-4 h-4" />
            {t('mortgage.prevYear', 'Año Anterior')}
          </button>

          <span className="text-xs font-semibold text-slate-600 dark:text-slate-400">
            {t('mortgage.year', 'Año')} {currentPage} {t('mortgage.of', 'de')} {totalPages}
          </span>

          <button
            disabled={currentPage === totalPages}
            onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
            className="flex items-center gap-1 px-3 py-1.5 rounded-lg border border-slate-200 dark:border-slate-700 text-slate-700 dark:text-slate-300 text-xs font-bold disabled:opacity-40 hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors"
          >
            {t('mortgage.nextYear', 'Siguiente Año')}
            <ChevronRight className="w-4 h-4" />
          </button>
        </div>
      )}
    </div>
  );
};
