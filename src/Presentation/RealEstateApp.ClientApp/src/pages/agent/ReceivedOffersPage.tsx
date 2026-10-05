import React, { useState, useEffect } from 'react';
import { offersService } from '../../api/services';
import { Offer } from '../../types';
import { useCurrency } from '../../context/CurrencyContext';
import { useLanguage } from '../../context/LanguageContext';
import { formatDate, getApiErrorMessage } from '../../utils/formatters';
import { Badge } from '../../components/common/Badge';
import { Loader } from '../../components/common/Loader';
import { Tag, Check, X, ShieldAlert, AlertTriangle, ArrowLeftRight } from 'lucide-react';
import { CounterOfferModal } from '../../components/offers/CounterOfferModal';

export const ReceivedOffersPage: React.FC = () => {
  const { formatPrice } = useCurrency();
  const { t } = useLanguage();
  const [offers, setOffers] = useState<Offer[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [selectedOfferToAccept, setSelectedOfferToAccept] = useState<Offer | null>(null);
  const [selectedOfferToCounter, setSelectedOfferToCounter] = useState<Offer | null>(null);
  const [isProcessing, setIsProcessing] = useState(false);
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => {
    let isMounted = true;
    const fetchReceivedOffers = async () => {
      try {
        const data = await offersService.getReceivedOffers();
        if (isMounted) setOffers(data || []);
      } catch (err: unknown) {
        console.error("Error loading agent offers:", getApiErrorMessage(err));
      } finally {
        if (isMounted) setIsLoading(false);
      }
    };

    fetchReceivedOffers();
    return () => {
      isMounted = false;
    };
  }, [refreshKey]);

  const handleAcceptConfirm = async () => {
    if (!selectedOfferToAccept) return;
    try {
      setIsProcessing(true);
      await offersService.acceptOffer(selectedOfferToAccept.id);
      setSelectedOfferToAccept(null);
      setRefreshKey((k) => k + 1);
      alert(t('agent.offers.acceptSuccess', "¡Oferta aceptada exitosamente! La propiedad ha sido marcada como VENDIDA y las demás ofertas competidoras fueron rechazadas automáticamente."));
    } catch (err: unknown) {
      console.error("Error accepting offer:", err);
      alert(getApiErrorMessage(err, "Error al aceptar oferta."));
    } finally {
      setIsProcessing(false);
    }
  };

  const handleReject = async (offerId: number) => {
    if (!confirm(t('agent.offers.rejectConfirm', "¿Estás seguro de que deseas rechazar esta oferta?"))) return;
    try {
      await offersService.rejectOffer(offerId);
      setRefreshKey((k) => k + 1);
    } catch (err: unknown) {
      console.error("Error rejecting offer:", err);
      alert(getApiErrorMessage(err, "Error al rechazar la oferta."));
    }
  };

  if (isLoading) {
    return <Loader text={t('agent.offers.loading', "Cargando ofertas recibidas...")} />;
  }

  return (
    <div className="space-y-6">
      
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-extrabold text-slate-900 dark:text-white flex items-center gap-2.5">
            <Tag className="w-7 h-7 text-emerald-600" />
            {t('agent.offers.title', 'Ofertas Recibidas')}
          </h1>
          <p className="text-xs text-slate-500 dark:text-slate-400 mt-1">
            {t('agent.offers.subtitle', 'Gestiona y responde a las propuestas económicas de tus clientes en tiempo real.')}
          </p>
        </div>

        {/* Atomic Cascade Alert Banner */}
        <div className="flex items-center gap-2 px-3.5 py-2 bg-amber-50 dark:bg-amber-950/40 border border-amber-200 dark:border-amber-800/60 rounded-xl text-amber-800 dark:text-amber-300 text-xs font-semibold">
          <ShieldAlert className="w-4 h-4 text-amber-600 shrink-0" />
          <span>{t('agent.offers.ruleBanner', 'Regla de Negocio: Aceptar una oferta cierra la venta y rechaza las demás.')}</span>
        </div>
      </div>

      {offers.length === 0 ? (
        <div className="bg-white dark:bg-slate-900 rounded-3xl p-12 text-center border border-slate-200 dark:border-slate-800 shadow-sm space-y-3">
          <Tag className="w-12 h-12 text-slate-300 dark:text-slate-600 mx-auto" />
          <h3 className="font-bold text-sm text-slate-700 dark:text-slate-300">{t('agent.offers.emptyTitle', 'Bandeja de Ofertas Vacía')}</h3>
          <p className="text-xs text-slate-500 dark:text-slate-400 max-w-sm mx-auto">
            {t('agent.offers.emptySubtitle', 'Las ofertas enviadas por clientes compradores en tus propiedades aparecerán aquí para tu aprobación.')}
          </p>
        </div>
      ) : (
        <div className="bg-white dark:bg-slate-900 rounded-3xl border border-slate-200 dark:border-slate-800 overflow-hidden shadow-sm">
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse text-xs">
              <thead>
                <tr className="bg-slate-50 dark:bg-slate-800/60 text-slate-700 dark:text-slate-300 font-extrabold uppercase tracking-wider border-b border-slate-200 dark:border-slate-800">
                  <th className="py-3.5 px-4">{t('agent.offers.colProperty', 'Propiedad')}</th>
                  <th className="py-3.5 px-4">{t('agent.offers.colBuyer', 'Comprador')}</th>
                  <th className="py-3.5 px-4">{t('agent.offers.colOffer', 'Oferta')}</th>
                  <th className="py-3.5 px-4">{t('agent.offers.colDate', 'Fecha')}</th>
                  <th className="py-3.5 px-4">{t('agent.offers.colStatus', 'Estado')}</th>
                  <th className="py-3.5 px-4 text-right">{t('agent.offers.colActions', 'Acciones')}</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 dark:divide-slate-800">
                {offers.map((offer) => {
                  const isPending = offer.status === 'Pending' || offer.status === 'Pendiente';
                  const isCounter = offer.status === 'CounterOffered' || offer.status === 'Contraofertada';
                  return (
                    <tr key={offer.id} className="hover:bg-slate-50/80 dark:hover:bg-slate-800/40 transition-colors">
                      <td className="py-3 px-4">
                        <span className="font-bold text-slate-900 dark:text-white font-mono">#{offer.propertyCode || offer.propertyId}</span>
                        <p className="text-[11px] text-slate-500 dark:text-slate-400 truncate max-w-xs">{offer.propertyDescription}</p>
                      </td>
                      <td className="py-3 px-4 text-slate-700 dark:text-slate-300 font-semibold">
                        {offer.clientName || 'Cliente'} <br />
                        <span className="text-[10px] text-slate-500 dark:text-slate-400 font-normal">{offer.clientEmail || offer.clientPhone}</span>
                      </td>
                      <td className="py-3 px-4 font-mono font-bold text-sm text-emerald-600 dark:text-emerald-400">
                        {formatPrice(offer.amount)}
                      </td>
                      <td className="py-3 px-4 text-slate-600 dark:text-slate-400">
                        {formatDate(offer.created)}
                      </td>
                      <td className="py-3 px-4">
                        <Badge status={offer.status} />
                      </td>
                      <td className="py-3 px-4 text-right space-x-2">
                        {isPending ? (
                          <>
                            <button
                              onClick={() => setSelectedOfferToAccept(offer)}
                              className="inline-flex items-center gap-1 px-3 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl font-bold text-xs shadow-sm transition-all"
                              title={t('agent.offers.btnAccept', 'Aceptar')}
                            >
                              <Check className="w-3.5 h-3.5" />
                              <span>{t('agent.offers.btnAccept', 'Aceptar')}</span>
                            </button>
                            <button
                              onClick={() => setSelectedOfferToCounter(offer)}
                              className="inline-flex items-center gap-1 px-3 py-1.5 bg-amber-50 dark:bg-amber-950/40 hover:bg-amber-100 dark:hover:bg-amber-900/60 text-amber-700 dark:text-amber-300 rounded-xl font-bold text-xs border border-amber-200 dark:border-amber-800/60 transition-all"
                              title={t('agent.offers.btnCounter', 'Contraoferta')}
                            >
                              <ArrowLeftRight className="w-3.5 h-3.5" />
                              <span>{t('agent.offers.btnCounter', 'Contraoferta')}</span>
                            </button>
                            <button
                              onClick={() => handleReject(offer.id)}
                              className="inline-flex items-center gap-1 px-3 py-1.5 bg-rose-50 dark:bg-rose-950/40 hover:bg-rose-100 dark:hover:bg-rose-900/60 text-rose-700 dark:text-rose-300 rounded-xl font-bold text-xs border border-rose-200 dark:border-rose-800/60 transition-all"
                              title={t('agent.offers.btnReject', 'Rechazar')}
                            >
                              <X className="w-3.5 h-3.5" />
                              <span>{t('agent.offers.btnReject', 'Rechazar')}</span>
                            </button>
                          </>
                        ) : isCounter ? (
                          <div className="text-right space-y-1.5">
                            <div className="p-2.5 bg-amber-50 dark:bg-amber-950/40 rounded-xl border border-amber-200/60 dark:border-amber-800/60">
                              <p className="text-[10px] text-amber-600 dark:text-amber-400 uppercase font-bold mb-0.5">{t('agent.offers.counterSent', 'Contraoferta Enviada')}</p>
                              <p className="font-mono font-bold text-sm text-amber-700 dark:text-amber-300">{formatPrice(offer.counterOfferAmount || 0)}</p>
                              {offer.counterOfferMessage && (
                                <p className="text-[11px] text-amber-800 dark:text-amber-200 mt-1 leading-relaxed">{offer.counterOfferMessage}</p>
                              )}
                            </div>
                          </div>
                        ) : (
                          <span className="text-[11px] text-slate-400 dark:text-slate-500 font-semibold italic">{t('agent.offers.resolved', 'Resuelta')}</span>
                        )}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Atomic Acceptance Confirmation Modal */}
      {selectedOfferToAccept && (
        <div className="fixed inset-0 z-50 bg-black/60 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white dark:bg-slate-900 rounded-3xl p-6 sm:p-8 max-w-md w-full shadow-2xl border border-slate-100 dark:border-slate-800 space-y-5 animate-in zoom-in-95">
            
            <div className="w-12 h-12 rounded-2xl bg-amber-50 dark:bg-amber-950/40 text-amber-600 dark:text-amber-400 flex items-center justify-center mx-auto">
              <AlertTriangle className="w-6 h-6" />
            </div>

            <div className="text-center space-y-2">
              <h3 className="font-extrabold text-lg text-slate-900 dark:text-white">
                {t('agent.offers.acceptTitle', '¿Confirmar Aceptación de Oferta?')}
              </h3>
              <p className="text-xs text-slate-600 dark:text-slate-300 leading-relaxed">
                {t('agent.offers.acceptPrompt', 'Estás a punto de aceptar la oferta de')} <strong className="font-mono text-emerald-600 dark:text-emerald-400">{formatPrice(selectedOfferToAccept.amount)}</strong> {t('agent.offers.forProperty', 'para la propiedad')} <strong>#{selectedOfferToAccept.propertyCode}</strong>.
              </p>
            </div>

            <div className="p-3.5 bg-amber-50 dark:bg-amber-950/40 rounded-2xl border border-amber-200/60 dark:border-amber-800/60 text-[11px] text-amber-900 dark:text-amber-200 space-y-1">
              <p className="font-bold flex items-center gap-1.5">
                <ShieldAlert className="w-4 h-4 text-amber-600 shrink-0" />
                {t('agent.offers.atomicRuleTitle', 'Ejecución de Regla Atómica de Venta:')}
              </p>
              <ul className="list-disc pl-5 space-y-0.5 text-amber-800 dark:text-amber-300">
                <li>{t('agent.offers.atomicRule1', 'El inmueble se marcará permanentemente como "Vendida".')}</li>
                <li>{t('agent.offers.atomicRule2', 'Todas las demás ofertas pendientes de este inmueble se rechazarán en cascada.')}</li>
                <li>{t('agent.offers.atomicRule3', 'Se bloquearán nuevas ofertas y solicitudes de visitas.')}</li>
              </ul>
            </div>

            <div className="flex gap-2 pt-2">
              <button
                type="button"
                onClick={() => setSelectedOfferToAccept(null)}
                disabled={isProcessing}
                className="flex-1 py-2.5 rounded-xl border border-slate-200 dark:border-slate-700 text-xs font-bold text-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800"
              >
                {t('agent.offers.cancel', 'Cancelar')}
              </button>
              <button
                type="button"
                onClick={handleAcceptConfirm}
                disabled={isProcessing}
                className="flex-1 py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-extrabold rounded-xl shadow-lg shadow-emerald-600/20"
              >
                {isProcessing ? t('agent.offers.processingAccept', 'Procesando Cierre...') : t('agent.offers.btnConfirmAccept', 'Sí, Aceptar Oferta')}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Counter Offer Modal */}
      {selectedOfferToCounter && (
        <CounterOfferModal
          isOpen={true}
          onClose={() => setSelectedOfferToCounter(null)}
          offerId={selectedOfferToCounter.id}
          originalAmount={selectedOfferToCounter.amount}
          onSubmit={() => {
            setSelectedOfferToCounter(null);
            setRefreshKey((k) => k + 1);
          }}
        />
      )}
    </div>
  );
};
