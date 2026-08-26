import React, { useState, useEffect } from 'react';
import { offersService } from '../../api/services';
import { Offer } from '../../types';
import { formatCurrencyRD, formatDate } from '../../utils/formatters';
import { Badge } from '../../components/common/Badge';
import { Loader } from '../../components/common/Loader';
import { Tag, ExternalLink, ShieldCheck, Clock, Check, X } from 'lucide-react';
import { Link } from 'react-router-dom';

export const MyOffersPage: React.FC = () => {
  const [offers, setOffers] = useState<Offer[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isProcessing, setIsProcessing] = useState(false);

  const loadOffers = async () => {
    try {
      setIsLoading(true);
      const data = await offersService.getMyOffers();
      setOffers(data || []);
    } catch (err) {
      console.error("Error loading client offers:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadOffers();
  }, []);

  const handleAcceptCounterOffer = async (offerId: number) => {
    if (!window.confirm("¿Seguro que deseas aceptar la contraoferta del agente?")) return;
    try {
      setIsProcessing(true);
      await offersService.acceptCounterOffer(offerId);
      await loadOffers();
      alert("¡Contraoferta aceptada! La propiedad ha sido marcada como VENDIDA.");
    } catch (err: any) {
      console.error("Error accepting counter offer:", err);
      alert(err.response?.data?.error || "Error al aceptar la contraoferta.");
    } finally {
      setIsProcessing(false);
    }
  };

  const handleRejectCounterOffer = async (offerId: number) => {
    if (!window.confirm("¿Seguro que deseas rechazar la contraoferta del agente?")) return;
    try {
      setIsProcessing(true);
      await offersService.rejectCounterOffer(offerId);
      await loadOffers();
    } catch (err: any) {
      console.error("Error rejecting counter offer:", err);
      alert(err.response?.data?.error || "Error al rechazar la contraoferta.");
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center pb-4 border-b border-slate-200">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <Tag className="w-6 h-6 text-emerald-600" />
            Mis Ofertas Económicas
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Historial de propuestas de compra enviadas y estado de resolución por el agente.
          </p>
        </div>
      </div>

      {isLoading ? (
        <Loader text="Consultando tus ofertas..." />
      ) : offers.length === 0 ? (
        <div className="bg-white rounded-3xl p-12 text-center border border-slate-200 space-y-4">
          <Tag className="w-12 h-12 text-slate-300 mx-auto" />
          <h3 className="text-base font-bold text-slate-800">No has enviado ninguna oferta</h3>
          <p className="text-xs text-slate-500 max-w-sm mx-auto">
            Desde la página de detalle de cualquier inmueble puedes enviar tu propuesta formal de compra en RD$.
          </p>
          <Link
            to="/catalog"
            className="inline-block px-5 py-2.5 bg-brand-600 hover:bg-brand-700 text-white font-bold text-xs rounded-xl shadow-md"
          >
            Ver Propiedades Disponibles
          </Link>
        </div>
      ) : (
        <div className="bg-white rounded-3xl border border-slate-200 overflow-hidden shadow-xs">
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse text-xs">
              <thead>
                <tr className="bg-slate-50 text-slate-700 font-extrabold uppercase tracking-wider border-b border-slate-200">
                  <th className="py-3.5 px-4">Propiedad</th>
                  <th className="py-3.5 px-4">Monto Ofrecido (RD$)</th>
                  <th className="py-3.5 px-4">Fecha de Envío</th>
                  <th className="py-3.5 px-4">Estado</th>
                  <th className="py-3.5 px-4">Acción</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {offers.map((offer) => (
                  <tr key={offer.id} className="hover:bg-slate-50/80 transition-colors">
                    <td className="py-3 px-4">
                      <span className="font-bold text-slate-800">#{offer.propertyCode || offer.propertyId}</span>
                      <p className="text-[11px] text-slate-500 truncate max-w-xs">{offer.propertyDescription || 'Inmueble residencial'}</p>
                    </td>
                    <td className="py-3 px-4 font-mono font-bold text-sm text-emerald-600">
                      {formatCurrencyRD(offer.amount)}
                    </td>
                    <td className="py-3 px-4 text-slate-600 font-medium">
                      {formatDate(offer.created)}
                    </td>
                    <td className="py-3 px-4">
                      <Badge status={offer.status} />
                    </td>
                    <td className="py-3 px-4">
                      {offer.status === 'CounterOffered' ? (
                        <div className="space-y-2">
                          <div className="p-2.5 bg-amber-50 rounded-xl border border-amber-200/60 mb-2">
                            <p className="text-[10px] text-amber-600 uppercase font-bold mb-0.5">Contraoferta del Agente</p>
                            <p className="font-mono font-bold text-sm text-amber-700">{formatCurrencyRD(offer.counterOfferAmount || 0)}</p>
                            {offer.counterOfferMessage && (
                              <p className="text-[11px] text-amber-800 mt-1 leading-relaxed">{offer.counterOfferMessage}</p>
                            )}
                          </div>
                          <div className="flex gap-1.5">
                            <button
                              onClick={() => handleAcceptCounterOffer(offer.id)}
                              disabled={isProcessing}
                              className="inline-flex items-center gap-1 px-3 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl font-bold text-xs shadow-xs transition-all"
                              title="Aceptar Contraoferta"
                            >
                              <Check className="w-3.5 h-3.5" />
                              <span>Aceptar Contraoferta</span>
                            </button>
                            <button
                              onClick={() => handleRejectCounterOffer(offer.id)}
                              disabled={isProcessing}
                              className="inline-flex items-center gap-1 px-3 py-1.5 bg-rose-50 hover:bg-rose-100 text-rose-700 rounded-xl font-bold text-xs border border-rose-200 transition-all"
                              title="Rechazar Contraoferta"
                            >
                              <X className="w-3.5 h-3.5" />
                              <span>Rechazar Contraoferta</span>
                            </button>
                          </div>
                        </div>
                      ) : (
                        <Link
                          to={`/property/${offer.propertyId}`}
                          className="inline-flex items-center gap-1 text-brand-600 hover:text-brand-700 font-bold"
                        >
                          <span>Ver Inmueble</span>
                          <ExternalLink className="w-3.5 h-3.5" />
                        </Link>
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
