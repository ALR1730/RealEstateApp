import React from 'react';
import { Property } from '../../types';
import { useCompare } from '../../context/CompareContext';
import { Check, Plus } from 'lucide-react';

interface CompareCheckboxProps {
  property: Property;
}

export const CompareCheckbox: React.FC<CompareCheckboxProps> = ({ property }) => {
  const { isInCompare, addToCompare, removeFromCompare } = useCompare();
  const inCompare = isInCompare(property.id);

  const handleToggle = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (inCompare) {
      removeFromCompare(property.id);
    } else {
      addToCompare(property);
    }
  };

  return (
    <button
      onClick={handleToggle}
      className={`pointer-events-auto p-2 rounded-full backdrop-blur-md transition-all shadow-md ${
        inCompare
          ? 'bg-brand-600 text-white hover:bg-brand-700'
          : 'bg-white/80 text-slate-700 hover:bg-white hover:text-brand-600'
      }`}
      title={inCompare ? 'Quitar de comparación' : 'Agregar a comparación'}
    >
      {inCompare ? <Check className="w-4 h-4" /> : <Plus className="w-4 h-4" />}
    </button>
  );
};
