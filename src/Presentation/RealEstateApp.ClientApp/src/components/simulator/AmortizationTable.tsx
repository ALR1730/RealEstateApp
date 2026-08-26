import React, { useState } from 'react';
import { AmortizationScheduleItem } from '../../types';
import { formatCurrencyRD } from '../../utils/formatters';
import { Printer, Download, ChevronLeft, ChevronRight } from 'lucide-react';

interface AmortizationTableProps {
  schedule: AmortizationScheduleItem[];
}

export const AmortizationTable: React.FC<AmortizationTableProps> = ({ schedule }) => {
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
          <h4 className="font-bold text-sm text-slate-900">
            Tabla de Cuotas Mensuales — Año {currentPage} (Meses {(currentPage - 1) * 12 + 1} al {Math.min(currentPage * 12, schedule.length)})
          </h4>
          <p className="text-xs text-slate-500">
            Desglose de amortización del capital y distribución de intereses.
          </p>
        </div>

        <button
          onClick={handlePrint}
          className="flex items-center gap-1.5 px-3 py-1.5 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-lg text-xs font-semibold transition-colors"
        >
          <Printer className="w-4 h-4 text-slate-600" />
          <span>Imprimir / PDF</span>
        </button>
      </div>

      {/* Table Container */}
      <div className="overflow-x-auto rounded-2xl border border-slate-200 shadow-xs">
        <table className="w-full text-left border-collapse text-xs">
          <thead>
            <tr className="bg-slate-100/80 text-slate-700 font-extrabold uppercase tracking-wider border-b border-slate-200">
              <th className="py-3 px-4">Mes</th>
              <th className="py-3 px-4">Cuota Fija (RD$)</th>
              <th className="py-3 px-4">Interés (RD$)</th>
              <th className="py-3 px-4">Capital (RD$)</th>
              <th className="py-3 px-4">Balance Restante (RD$)</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100 font-mono">
            {currentItems.map((item) => (
              <tr key={item.period} className="hover:bg-slate-50/80 transition-colors">
                <td className="py-2.5 px-4 font-bold text-slate-800 font-sans">Mes {item.period}</td>
                <td className="py-2.5 px-4 font-bold text-emerald-600">{formatCurrencyRD(item.installment)}</td>
                <td className="py-2.5 px-4 text-amber-600">{formatCurrencyRD(item.interest)}</td>
                <td className="py-2.5 px-4 text-royal-600">{formatCurrencyRD(item.principal)}</td>
                <td className="py-2.5 px-4 text-slate-700 font-semibold">{formatCurrencyRD(item.remainingBalance)}</td>
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
            className="flex items-center gap-1 px-3 py-1.5 rounded-lg border border-slate-200 text-xs font-bold disabled:opacity-40 hover:bg-slate-50 transition-colors"
          >
            <ChevronLeft className="w-4 h-4" />
            Año Anterior
          </button>

          <span className="text-xs font-semibold text-slate-600">
            Año {currentPage} de {totalPages}
          </span>

          <button
            disabled={currentPage === totalPages}
            onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
            className="flex items-center gap-1 px-3 py-1.5 rounded-lg border border-slate-200 text-xs font-bold disabled:opacity-40 hover:bg-slate-50 transition-colors"
          >
            Siguiente Año
            <ChevronRight className="w-4 h-4" />
          </button>
        </div>
      )}
    </div>
  );
};
