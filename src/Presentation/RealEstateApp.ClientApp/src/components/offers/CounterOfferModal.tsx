import React, { useState } from 'react';
import { offersService } from '../../api/services';
import { formatCurrencyRD } from '../../utils/formatters';
import { Modal } from '../common/Modal';
import { DollarSign, Send, ArrowLeftRight, CheckCircle } from 'lucide-react';

interface CounterOfferModalProps {
  isOpen: boolean;
  onClose: () => void;
  offerId: number;
  originalAmount: number;
  onSubmit?: () => void;
}

export const CounterOfferModal: React.FC<CounterOfferModalProps> = ({
  isOpen,
  onClose,
  offerId,
  originalAmount,
  onSubmit,
}) => {
  const [amount, setAmount] = useState<number>(originalAmount);
  const [message, setMessage] = useState<string>('');
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (amount <= 0) {
      setErrorMessage("Por favor ingrese un monto válido para la contraoferta.");
      return;
    }

    try {
      setIsSubmitting(true);
      setErrorMessage(null);
      await offersService.counterOffer(offerId, { amount, message: message || undefined });

      setSuccessMessage("¡Contraoferta enviada exitosamente!");
      setTimeout(() => {
        onSubmit?.();
        onClose();
        setSuccessMessage(null);
        setAmount(originalAmount);
        setMessage('');
      }, 2000);
    } catch (err: any) {
      console.error("Error sending counter offer:", err);
      setErrorMessage(err.response?.data?.error || "Ocurrió un error al enviar la contraoferta. Intente de nuevo.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Enviar Contraoferta">
      {successMessage ? (
        <div className="py-8 text-center space-y-3 animate-in zoom-in-95">
          <CheckCircle className="w-12 h-12 text-emerald-500 mx-auto" />
          <h4 className="font-extrabold text-lg text-slate-900">{successMessage}</h4>
          <p className="text-xs text-slate-500">
            El comprador recibirá tu propuesta de contraoferta para su evaluación.
          </p>
        </div>
      ) : (
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="p-3.5 bg-slate-50 rounded-2xl border border-slate-100 flex items-center justify-between text-xs">
            <div>
              <span className="text-[10px] text-slate-400 uppercase font-bold">Monto Original del Cliente</span>
              <p className="font-bold text-slate-800 font-mono">{formatCurrencyRD(originalAmount)}</p>
            </div>
            <ArrowLeftRight className="w-5 h-5 text-slate-300" />
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5">
              Monto de Contraoferta (Pesos Dominicanos RD$)
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

          <div>
            <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5">
              Mensaje (Opcional)
            </label>
            <textarea
              rows={3}
              placeholder="Ej: Gracias por su interés. Considerando el estado actual del mercado, le proponemos..."
              value={message}
              onChange={(e) => setMessage(e.target.value)}
              className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-brand-500/20 focus:border-brand-500"
            />
          </div>

          {errorMessage && (
            <p className="text-xs text-rose-600 font-semibold">{errorMessage}</p>
          )}

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
              className="flex items-center gap-1.5 px-5 py-2 text-xs font-bold text-white bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 rounded-xl shadow-md shadow-emerald-600/20 transition-all"
            >
              <Send className="w-3.5 h-3.5" />
              <span>{isSubmitting ? 'Enviando...' : 'Enviar Contraoferta'}</span>
            </button>
          </div>
        </form>
      )}
    </Modal>
  );
};
