import React from 'react';
import { useCurrency } from '../../context/CurrencyContext';
import { DollarSign, Banknote } from 'lucide-react';

export const CurrencySwitcher: React.FC = () => {
  const { currency, toggleCurrency } = useCurrency();

  return (
    <button
      onClick={toggleCurrency}
      className="flex items-center gap-1.5 px-2.5 py-1.5 rounded-lg text-xs font-bold transition-all border border-slate-200/80 hover:border-brand-400 bg-white hover:bg-brand-50 text-slate-700 hover:text-brand-700"
      title={currency === 'DOP' ? 'Cambiar a USD' : 'Cambiar a RD$'}
    >
      {currency === 'DOP' ? (
        <Banknote className="w-3.5 h-3.5" />
      ) : (
        <DollarSign className="w-3.5 h-3.5" />
      )}
      <span>{currency === 'DOP' ? 'RD$' : 'USD'}</span>
    </button>
  );
};
