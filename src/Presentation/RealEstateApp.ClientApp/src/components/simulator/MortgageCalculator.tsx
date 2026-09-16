import React, { useState, useMemo } from 'react';
import { formatCurrencyRD, calculateFrenchAmortization } from '../../utils/formatters';
import { AmortizationTable } from './AmortizationTable';
import { Calculator, DollarSign, Calendar, Percent, FileText, ChevronDown, ChevronUp } from 'lucide-react';

interface MortgageCalculatorProps {
  initialPrice?: number;
}

export const MortgageCalculator: React.FC<MortgageCalculatorProps> = ({
  initialPrice = 5000000,
}) => {
  const [price, setPrice] = useState<number>(initialPrice);
  const [downPaymentPercent, setDownPaymentPercent] = useState<number>(20);
  const [rate, setRate] = useState<number>(11.5);
  const [years, setYears] = useState<number>(20);
  const [showTable, setShowTable] = useState<boolean>(false);

  const downPaymentAmount = useMemo(() => {
    return Math.round((price * downPaymentPercent) / 100);
  }, [price, downPaymentPercent]);

  const simulation = useMemo(() => {
    return calculateFrenchAmortization(price, downPaymentAmount, rate, years);
  }, [price, downPaymentAmount, rate, years]);

  return (
    <div className="bg-white rounded-3xl p-6 sm:p-8 border border-slate-200 shadow-sm space-y-6">
      
      {/* Title */}
      <div className="flex items-center justify-between pb-4 border-b border-slate-100">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-brand-100 text-brand-700 flex items-center justify-center font-bold">
            <Calculator className="w-5 h-5" />
          </div>
          <div>
            <h3 className="font-extrabold text-lg sm:text-xl text-slate-900">
              Simulador Hipotecario (RD$)
            </h3>
            <p className="text-xs text-slate-500">
              Sistema Francés de Amortización con cuota fija mensual en Pesos Dominicanos.
            </p>
          </div>
        </div>
        <span className="hidden sm:inline-block px-3 py-1 bg-brand-50 text-brand-700 text-xs font-extrabold rounded-full border border-brand-200/60">
          República Dominicana
        </span>
      </div>

      {/* Controls Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        
        {/* Precio de la Propiedad */}
        <div className="space-y-2">
          <div className="flex justify-between items-center text-xs font-bold text-slate-700">
            <span>Precio del Inmueble</span>
            <span className="text-brand-600 font-mono">{formatCurrencyRD(price)}</span>
          </div>
          <input
            type="range"
            min={500000}
            max={50000000}
            step={50000}
            value={price}
            onChange={(e) => setPrice(Number(e.target.value))}
            className="w-full h-2 bg-slate-200 rounded-lg appearance-none cursor-pointer accent-brand-600"
          />
          <div className="relative">
            <DollarSign className="w-4 h-4 absolute left-3 top-2.5 text-slate-400" />
            <input
              type="number"
              value={price}
              onChange={(e) => setPrice(Math.max(0, Number(e.target.value)))}
              className="w-full pl-9 pr-3 py-2 text-xs font-mono rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500"
            />
          </div>
        </div>

        {/* Pago Inicial / Enganche (%) */}
        <div className="space-y-2">
          <div className="flex justify-between items-center text-xs font-bold text-slate-700">
            <span>Inicial ({downPaymentPercent}%)</span>
            <span className="text-emerald-600 font-mono">{formatCurrencyRD(downPaymentAmount)}</span>
          </div>
          <input
            type="range"
            min={10}
            max={80}
            step={5}
            value={downPaymentPercent}
            onChange={(e) => setDownPaymentPercent(Number(e.target.value))}
            className="w-full h-2 bg-slate-200 rounded-lg appearance-none cursor-pointer accent-brand-600"
          />
          <div className="flex gap-2">
            {[10, 20, 30, 40].map((p) => (
              <button
                key={p}
                onClick={() => setDownPaymentPercent(p)}
                className={`flex-1 py-1.5 text-xs font-bold rounded-lg border transition-all ${
                  downPaymentPercent === p
                    ? 'bg-brand-600 text-white border-brand-600'
                    : 'bg-slate-50 text-slate-700 border-slate-200 hover:bg-slate-100'
                }`}
              >
                {p}%
              </button>
            ))}
          </div>
        </div>

        {/* Tasa de Interés Anual */}
        <div className="space-y-2">
          <div className="flex justify-between items-center text-xs font-bold text-slate-700">
            <span>Tasa de Interés Anual</span>
            <span className="text-royal-600 font-mono">{rate}%</span>
          </div>
          <input
            type="range"
            min={5}
            max={25}
            step={0.25}
            value={rate}
            onChange={(e) => setRate(Number(e.target.value))}
            className="w-full h-2 bg-slate-200 rounded-lg appearance-none cursor-pointer accent-brand-600"
          />
          <div className="relative">
            <Percent className="w-4 h-4 absolute left-3 top-2.5 text-slate-400" />
            <input
              type="number"
              step="0.1"
              value={rate}
              onChange={(e) => setRate(Math.max(0.1, Number(e.target.value)))}
              className="w-full pl-9 pr-3 py-2 text-xs font-mono rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500"
            />
          </div>
        </div>

        {/* Plazo en Años */}
        <div className="space-y-2">
          <div className="flex justify-between items-center text-xs font-bold text-slate-700">
            <span>Plazo del Préstamo</span>
            <span className="text-navy-900 font-mono">{years} Años ({years * 12} meses)</span>
          </div>
          <input
            type="range"
            min={5}
            max={30}
            step={5}
            value={years}
            onChange={(e) => setYears(Number(e.target.value))}
            className="w-full h-2 bg-slate-200 rounded-lg appearance-none cursor-pointer accent-brand-600"
          />
          <div className="flex gap-2">
            {[10, 15, 20, 25, 30].map((y) => (
              <button
                key={y}
                onClick={() => setYears(y)}
                className={`flex-1 py-1.5 text-xs font-bold rounded-lg border transition-all ${
                  years === y
                    ? 'bg-brand-600 text-white border-brand-600'
                    : 'bg-slate-50 text-slate-700 border-slate-200 hover:bg-slate-100'
                }`}
              >
                {y}a
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* Result Cards Summary */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 p-5 bg-gradient-to-br from-slate-900 to-navy-900 rounded-2xl text-white shadow-lg">
        <div>
          <span className="text-xs text-slate-400 font-medium">Cuota Fija Mensual</span>
          <p className="text-2xl sm:text-3xl font-extrabold text-emerald-400 font-mono mt-1">
            {formatCurrencyRD(simulation.monthlyInstallment)}
          </p>
          <span className="text-[10px] text-slate-400">Por {simulation.totalMonths} meses</span>
        </div>

        <div>
          <span className="text-xs text-slate-400 font-medium">Capital a Financiar</span>
          <p className="text-lg font-bold text-white font-mono mt-1">
            {formatCurrencyRD(simulation.loanAmount)}
          </p>
          <span className="text-[10px] text-slate-400">80% del valor total</span>
        </div>

        <div>
          <span className="text-xs text-slate-400 font-medium">Intereses Totales</span>
          <p className="text-lg font-bold text-amber-400 font-mono mt-1">
            {formatCurrencyRD(simulation.totalInterest)}
          </p>
          <span className="text-[10px] text-slate-400">Costo financiero acumulado</span>
        </div>

        <div>
          <span className="text-xs text-slate-400 font-medium">Costo Total del Crédito</span>
          <p className="text-lg font-bold text-sky-400 font-mono mt-1">
            {formatCurrencyRD(simulation.totalCost)}
          </p>
          <span className="text-[10px] text-slate-400">Capital + Intereses</span>
        </div>
      </div>

      {/* Toggle Amortization Table Button */}
      <div className="flex justify-center pt-2">
        <button
          onClick={() => setShowTable(!showTable)}
          className="flex items-center gap-2 px-6 py-2.5 bg-slate-100 hover:bg-slate-200 text-slate-800 rounded-xl text-xs font-bold transition-all"
        >
          <FileText className="w-4 h-4 text-brand-600" />
          <span>{showTable ? 'Ocultar Tabla de Amortización' : 'Ver Tabla de Amortización Completa'}</span>
          {showTable ? <ChevronUp className="w-4 h-4" /> : <ChevronDown className="w-4 h-4" />}
        </button>
      </div>

      {/* Amortization Table */}
      {showTable && (
        <div className="pt-4 border-t border-slate-100 animate-in fade-in">
          <AmortizationTable schedule={simulation.schedule} />
        </div>
      )}
    </div>
  );
};
