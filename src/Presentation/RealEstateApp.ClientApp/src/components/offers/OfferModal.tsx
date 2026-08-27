import React, { useState } from 'react';
import { Property } from '../../types';
import { offersService } from '../../api/services';
import { formatCurrencyRD } from '../../utils/formatters';
import { Modal } from '../common/Modal';
import { Tag, DollarSign, Send, ShieldAlert, CheckCircle } from 'lucide-react';

interface OfferModalProps {
  isOpen: boolean;
  onClose: () => void;
  property: Property;
  onOfferSuccess?: () => void;
}

export const OfferModal: React.FC<OfferModalProps> = ({
  isOpen,
  onClose,
  property,
  onOfferSuccess,
}) => {
  const [amount, setAmount] = useState<number>(property.price);
  const [notes, setNotes] = useState<string>('');
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (amount <= 0) {
      setErrorMessage("Por favor ingrese un monto válido de oferta.");
      return;
    }

    try {
      setIsSubmitting(true);
      setErrorMessage(null);
      await offersService.makeOffer({
        propertyId: property.id,
        amount,
        notes,
      });

      setSuccessMessage("¡Tu propuesta económica ha sido enviada formalmente al agente!");
      setTimeout(() => {
        onOfferSuccess?.();
        onClose();
        setSuccessMessage(null);
      }, 2000);
    } catch (err: any) {
      console.error("Error making offer:", err);
      setErrorMessage(err.response?.data?.error || "Ocurrió un error al enviar la oferta. Intente de nuevo.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Enviar Oferta Formal de Compra / Alquiler">
      {successMessage ? (
        <div className="py-8 text-center space-y-3 animate-in zoom-in-95">
          <CheckCircle className="w-12 h-12 text-emerald-500 mx-auto" />
          <h4 className="font-extrabold text-lg text-slate-900">{successMessage}</h4>
          <p className="text-xs text-slate-500">
            El agente evaluará tu oferta. Podrás seguir el estado desde tu panel de ofertas.
          </p>
        </div>
      ) : (
        <form onSubmit={handleSubmit} className="space-y-4">
          
          {/* Property Summary Header */}
          <div className="p-3.5 bg-slate-50 rounded-2xl border border-slate-100 flex items-center justify-between text-xs">
            <div>
              <span className="font-bold text-slate-800">Propiedad #{property.code}</span>
              <p className="text-slate-500 truncate max-w-xs">{property.description}</p>
            </div>
            <div className="text-right">
              <span className="text-[10px] text-slate-400 uppercase font-bold">Precio Catálogo</span>
              <p className="font-bold text-brand-600 font-mono">{formatCurrencyRD(property.price)}</p>
            </div>
          </div>

          {/* Amount Field */}
          <div>
            <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5">
              Monto Ofrecido (Pesos Dominicanos RD$)
            </label>
            <div className="relative">
              <DollarSign className="w-4 h-4 absolute left-3 top-3 text-slate-400" />
              <input
                type="number"
                min={1}
                step={1000}
                required
                value={amount}
                onChange={(e) => setAmount(Number(e.target.value))}
                className="w-full pl-9 pr-3 py-2.5 text-sm font-mono font-bold text-slate-900 rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500 bg-slate-50/50"
              />
            </div>
            <p className="text-[11px] text-slate-500 mt-1 font-mono">
              Equivale a: <span className="font-bold text-brand-600">{formatCurrencyRD(amount)}</span>
            </p>
          </div>

          {/* Notes / Conditions */}
          <div>
            <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5">
              Condiciones o Comentarios (Opcional)
            </label>
            <textarea
              rows={3}
              placeholder="Ej: Sujeto a aprobación hipotecaria con Banco Popular / BHD, fecha de cierre prevista..."
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500"
            />
          </div>

          {/* Notice Alert */}
          <div className="p-3 bg-amber-50 rounded-xl border border-amber-200/60 flex items-start gap-2.5 text-[11px] text-amber-800">
            <ShieldAlert className="w-4 h-4 shrink-0 mt-0.5 text-amber-600" />
            <span>
              <strong>Regla Atómica de Negocio:</strong> Al ser aceptada tu oferta, la propiedad cambiará instantáneamente a &quot;Vendida&quot; y se bloquearán las demás propuestas.
            </span>
          </div>

          {errorMessage && (
            <p className="text-xs text-rose-600 font-semibold">{errorMessage}</p>
          )}

          {/* Actions */}
          <div className="pt-3 flex justify-end gap-2">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-xs font-bold text-slate-600 hover:bg-slate-100 rounded-xl transition-colors"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="flex items-center gap-1.5 px-5 py-2 text-xs font-bold text-white bg-brand-600 hover:bg-brand-700 disabled:opacity-50 rounded-xl shadow-md shadow-brand-600/20 transition-all"
            >
              <Send className="w-3.5 h-3.5" />
              <span>{isSubmitting ? 'Enviando...' : 'Enviar Oferta Formal'}</span>
            </button>
          </div>
        </form>
      )}
    </Modal>
  );
};
