import React from 'react';
import { MortgageCalculator } from '../../components/simulator/MortgageCalculator';
import { ShieldCheck, CheckCircle2, HelpCircle } from 'lucide-react';

export const MortgagePage: React.FC = () => {
  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-12 pb-20">
      
      {/* Hero */}
      <div className="bg-navy-950 text-white rounded-3xl p-8 sm:p-12 relative overflow-hidden shadow-xl">
        <div className="relative z-10 max-w-2xl space-y-3">
          <span className="text-brand-400 font-extrabold text-xs uppercase tracking-widest">
            Herramienta Financiera Profesional
          </span>
          <h1 className="text-2xl sm:text-4xl font-extrabold tracking-tight">
            Simulador de Préstamos Hipotecarios en RD$
          </h1>
          <p className="text-xs sm:text-sm text-slate-300">
            Calcula la cuota mensual fija para préstamos hipotecarios en República Dominicana bajo el Sistema Francés de Amortización con desglose detallado de capital e intereses.
          </p>
        </div>
      </div>

      {/* Main Interactive Calculator */}
      <MortgageCalculator initialPrice={5500000} />

      {/* Educational & Financial Insights Guide */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="bg-white rounded-3xl p-6 border border-slate-200 shadow-xs space-y-3">
          <div className="w-10 h-10 rounded-xl bg-brand-100 text-brand-700 flex items-center justify-center font-bold">
            <CheckCircle2 className="w-5 h-5" />
          </div>
          <h3 className="font-bold text-base text-slate-900">¿Qué es el Sistema Francés?</h3>
          <p className="text-xs text-slate-600 leading-relaxed">
            Es el método bancario más utilizado en República Dominicana. Se caracteriza por cuotas mensuales constantes a lo largo del crédito, amortizando más intereses al inicio y mayor capital al final.
          </p>
        </div>

        <div className="bg-white rounded-3xl p-6 border border-slate-200 shadow-xs space-y-3">
          <div className="w-10 h-10 rounded-xl bg-indigo-100 text-royal-700 flex items-center justify-center font-bold">
            <ShieldCheck className="w-5 h-5" />
          </div>
          <h3 className="font-bold text-base text-slate-900">Enganche Recomendado (20%)</h3>
          <p className="text-xs text-slate-600 leading-relaxed">
            Las principales entidades financieras (Banco Popular, BHD, Banreservas, APAP) suelen requerir un inicial mínimo del 10% al 20% del valor de tasación del inmueble.
          </p>
        </div>

        <div className="bg-white rounded-3xl p-6 border border-slate-200 shadow-xs space-y-3">
          <div className="w-10 h-10 rounded-xl bg-amber-100 text-amber-700 flex items-center justify-center font-bold">
            <HelpCircle className="w-5 h-5" />
          </div>
          <h3 className="font-bold text-base text-slate-900">Tasas Hipotecarias en RD$</h3>
          <p className="text-xs text-slate-600 leading-relaxed">
            Las tasas de interés promedio para préstamos en Pesos Dominicanos oscilan típicamente entre el 9.5% y el 14% anual, dependiendo del plazo fijado y la entidad bancaria.
          </p>
        </div>
      </div>
    </div>
  );
};
