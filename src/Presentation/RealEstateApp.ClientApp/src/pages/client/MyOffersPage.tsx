import React, { useState, useEffect } from 'react';
import { offersService, reviewsService } from '../../api/services';
import { Offer } from '../../types';
import { useCurrency } from '../../context/CurrencyContext';
import { formatDate } from '../../utils/formatters';
import { Badge } from '../../components/common/Badge';
import { Loader } from '../../components/common/Loader';
import { StarRating } from '../../components/common/StarRating';
import { Tag, ExternalLink, ShieldCheck, Clock, Check, X, Star } from 'lucide-react';
import { Link } from 'react-router-dom';

export const MyOffersPage: React.FC = () => {
  const { formatPrice } = useCurrency();
  const [offers, setOffers] = useState<Offer[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isProcessing, setIsProcessing] = useState(false);

  // Estado del modal de reseña
  const [reviewOffer, setReviewOffer] = useState<Offer | null>(null);
  const [reviewRating, setReviewRating] = useState(0);
  const [reviewComment, setReviewComment] = useState('');
  const [alreadyReviewed, setAlreadyReviewed] = useState(false);
  const [isReviewSubmitting, setIsReviewSubmitting] = useState(false);

  const loadOffers = async () => {
    try {
      const data = await offersService.getMyOffers();
      setOffers(data || []);
    } catch (err) {
      console.error("Error loading client offers:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    Promise.resolve().then(() => loadOffers()).catch(console.error);
  }, []);

  const openReview = async (offer: Offer) => {
    setReviewOffer(offer);
    setReviewRating(0);
    setReviewComment('');
    setAlreadyReviewed(false);

    if (offer.propertyId) {
      try {
        const reviewed = await reviewsService.hasReviewed(offer.agentId || '', offer.propertyId);
        setAlreadyReviewed(reviewed);
      } catch {
        setAlreadyReviewed(false);
      }
    }
  };

  const handleAcceptCounterOffer = async (offerId: number) => {
    if (!window.confirm("¿Aceptas la contraoferta propuesta por el agente? Esto cerrará la negociación formalmente.")) return;
    try {
      setIsProcessing(true);
      await offersService.acceptCounterOffer(offerId);
      await loadOffers();
      alert("¡Contraoferta aceptada exitosamente! Has cerrado la negociación.");
    } catch (err: any) {
      alert(err.response?.data?.message || "Error al aceptar contraoferta.");
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
      alert(err.response?.data?.message || "Error al rechazar contraoferta.");
    } finally {
      setIsProcessing(false);
    }
  };

  const handleSubmitReview = async () => {
    if (!reviewOffer?.propertyId) return;
    if (reviewRating === 0) {
      alert("Por favor selecciona una calificación de 1 a 5 estrellas.");
      return;
    }

    try {
      setIsReviewSubmitting(true);
      await reviewsService.create({
        agentId: reviewOffer.agentId || '',
        propertyId: reviewOffer.propertyId,
        rating: reviewRating,
        comment: reviewComment.trim() || undefined,
      });
      setAlreadyReviewed(true);
      alert("¡Gracias! Tu reseña ha sido registrada exitosamente.");
      setReviewOffer(null);
    } catch (err: any) {
      alert(err.response?.data?.message || "Error al enviar la reseña.");
    } finally {
      setIsReviewSubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      
      {/* Header */}
      <div className="flex justify-between items-center pb-4 border-b border-slate-200 dark:border-slate-800">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 dark:text-white tracking-tight flex items-center gap-2">
            <Tag className="w-6 h-6 text-emerald-600" />
            Mis Ofertas Económicas
          </h2>
          <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">
            Historial de propuestas de compra enviadas y estado de resolución por el agente.
          </p>
        </div>
      </div>

      {isLoading ? (
        <Loader text="Consultando tus ofertas..." />
      ) : offers.length === 0 ? (
        <div className="bg-white dark:bg-slate-900 rounded-3xl p-12 text-center border border-slate-200 dark:border-slate-800 shadow-sm space-y-4">
          <Tag className="w-12 h-12 text-slate-300 dark:text-slate-600 mx-auto" />
          <h3 className="text-base font-bold text-slate-800 dark:text-slate-200">No has enviado ninguna oferta</h3>
          <p className="text-xs text-slate-500 dark:text-slate-400 max-w-sm mx-auto">
            Desde la página de detalle de cualquier inmueble puedes enviar tu propuesta formal de compra.
          </p>
          <Link
            to="/catalog"
            className="inline-block px-5 py-2.5 bg-brand-600 hover:bg-brand-700 text-white font-bold text-xs rounded-xl shadow-md"
          >
            Ver Propiedades Disponibles
          </Link>
        </div>
      ) : (
        <div className="bg-white dark:bg-slate-900 rounded-3xl border border-slate-200 dark:border-slate-800 overflow-hidden shadow-sm">
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse text-xs">
              <thead>
                <tr className="bg-slate-50 dark:bg-slate-800/60 text-slate-700 dark:text-slate-300 font-extrabold uppercase tracking-wider border-b border-slate-200 dark:border-slate-800">
                  <th className="py-3.5 px-4">Propiedad</th>
                  <th className="py-3.5 px-4">Monto Ofrecido</th>
                  <th className="py-3.5 px-4">Fecha de Envío</th>
                  <th className="py-3.5 px-4">Estado</th>
                  <th className="py-3.5 px-4">Acción</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 dark:divide-slate-800">
                {offers.map((offer) => (
                  <tr key={offer.id} className="hover:bg-slate-50/80 dark:hover:bg-slate-800/40 transition-colors">
                    <td className="py-3 px-4">
                      <span className="font-bold text-slate-800 dark:text-slate-200">#{offer.propertyCode || offer.propertyId}</span>
                      <p className="text-[11px] text-slate-500 dark:text-slate-400 truncate max-w-xs">{offer.propertyDescription || 'Inmueble residencial'}</p>
                    </td>
                    <td className="py-3 px-4 font-mono font-bold text-sm text-emerald-600 dark:text-emerald-400">
                      {formatPrice(offer.amount)}
                    </td>
                    <td className="py-3 px-4 text-slate-600 dark:text-slate-400 font-medium">
                      {formatDate(offer.created)}
                    </td>
                    <td className="py-3 px-4">
                      <Badge status={offer.status} />
                    </td>
                    <td className="py-3 px-4">
                      {offer.status === 'CounterOffered' || (offer.status as any) === 'Contraofertada' ? (
                        <div className="space-y-2">
                          <div className="p-2.5 bg-amber-50 dark:bg-amber-950/40 rounded-xl border border-amber-200/60 dark:border-amber-800/60 mb-2">
                            <p className="text-[10px] text-amber-600 dark:text-amber-400 uppercase font-bold mb-0.5">Contraoferta del Agente</p>
                            <p className="font-mono font-bold text-sm text-amber-700 dark:text-amber-300">{formatPrice(offer.counterOfferAmount || 0)}</p>
                            {offer.counterOfferMessage && (
                              <p className="text-[11px] text-amber-800 dark:text-amber-200 mt-1 leading-relaxed">{offer.counterOfferMessage}</p>
                            )}
                          </div>
                          <div className="flex gap-1.5">
                            <button
                              onClick={() => handleAcceptCounterOffer(offer.id)}
                              disabled={isProcessing}
                              className="inline-flex items-center gap-1 px-3 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl font-bold text-xs shadow-sm transition-all"
                              title="Aceptar Contraoferta"
                            >
                              <Check className="w-3.5 h-3.5" />
                              <span>Aceptar Contraoferta</span>
                            </button>
                            <button
                              onClick={() => handleRejectCounterOffer(offer.id)}
                              disabled={isProcessing}
                              className="inline-flex items-center gap-1 px-3 py-1.5 bg-rose-50 dark:bg-rose-950/40 hover:bg-rose-100 dark:hover:bg-rose-900/60 text-rose-700 dark:text-rose-300 rounded-xl font-bold text-xs border border-rose-200 dark:border-rose-800/60 transition-all"
                              title="Rechazar Contraoferta"
                            >
                              <X className="w-3.5 h-3.5" />
                              <span>Rechazar Contraoferta</span>
                            </button>
                          </div>
                        </div>
                      ) : offer.status === 'Accepted' || (offer.status as any) === 'Aceptada' ? (
                        <div className="space-y-2">
                          <span className="inline-flex items-center gap-1 px-2.5 py-1 bg-emerald-50 dark:bg-emerald-950/40 text-emerald-700 dark:text-emerald-300 text-[10px] font-bold rounded-lg border border-emerald-200 dark:border-emerald-800/60">
                            <Check className="w-3 h-3" />
                            Transacción Completada
                          </span>
                          <button
                            onClick={() => openReview(offer)}
                            className="inline-flex items-center gap-1 px-3 py-1.5 bg-amber-50 dark:bg-amber-950/40 hover:bg-amber-100 dark:hover:bg-amber-900/60 text-amber-700 dark:text-amber-300 rounded-xl font-bold text-xs border border-amber-200 dark:border-amber-800/60 transition-all"
                            title="Calificar al agente por esta venta"
                          >
                            <Star className="w-3.5 h-3.5" />
                            <span>Calificar Agente</span>
                          </button>
                        </div>
                      ) : (
                        <Link
                          to={`/properties/${offer.propertyId}`}
                          className="inline-flex items-center gap-1 text-brand-600 dark:text-brand-400 hover:text-brand-700 font-bold"
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

      {/* Modal de Calificación al Agente */}
      {reviewOffer && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 backdrop-blur-sm p-4">
          <div className="bg-white dark:bg-slate-900 rounded-3xl shadow-2xl w-full max-w-md p-6 sm:p-8 space-y-5 border border-slate-200 dark:border-slate-800">
            <div className="flex items-start justify-between">
              <div>
                <h3 className="text-lg font-extrabold text-slate-900 flex items-center gap-2">
                  <Star className="w-5 h-5 text-amber-500" />
                  Calificar al Agente
                </h3>
                <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">
                  #{reviewOffer.propertyCode || reviewOffer.propertyId} · Venta por {formatPrice(reviewOffer.amount)}
                </p>
              </div>
              <button
                onClick={() => setReviewOffer(null)}
                className="p-2 text-slate-400 hover:text-slate-700 dark:hover:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-xl transition-colors"
                title="Cerrar"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            {alreadyReviewed ? (
              <div className="p-4 bg-emerald-50 dark:bg-emerald-950/40 border border-emerald-200 dark:border-emerald-800/60 rounded-2xl text-center space-y-1">
                <ShieldCheck className="w-8 h-8 text-emerald-600 dark:text-emerald-400 mx-auto" />
                <p className="text-sm font-bold text-emerald-800 dark:text-emerald-300">¡Ya calificaste a este agente!</p>
                <p className="text-[11px] text-emerald-700 dark:text-emerald-400">Solo se permite una reseña por transacción.</p>
              </div>
            ) : (
              <>
                <div className="space-y-3">
                  <label className="block text-[11px] font-extrabold uppercase tracking-wider text-slate-500 dark:text-slate-400">
                    Tu Calificación
                  </label>
                  <div className="flex flex-col items-center gap-2 py-2 bg-slate-50 dark:bg-slate-800/60 rounded-2xl border border-slate-100 dark:border-slate-800">
                    <StarRating value={reviewRating} onChange={setReviewRating} size="lg" />
                    <p className="text-xs font-bold text-slate-600 dark:text-slate-300">
                      {reviewRating === 0 ? 'Toca las estrellas para calificar' :
                        reviewRating <= 2 ? 'Experiencia regular' :
                        reviewRating === 3 ? 'Buena experiencia' :
                        reviewRating === 4 ? 'Muy buena experiencia' : '¡Excelente experiencia!'}
                    </p>
                  </div>
                </div>

                <div className="space-y-2">
                  <label htmlFor="review-comment" className="block text-[11px] font-extrabold uppercase tracking-wider text-slate-500 dark:text-slate-400">
                    Comentario (opcional)
                  </label>
                  <textarea
                    id="review-comment"
                    value={reviewComment}
                    onChange={(e) => setReviewComment(e.target.value)}
                    rows={4}
                    maxLength={1000}
                    placeholder="Cuéntanos cómo fue la atención del agente durante el proceso..."
                    className="w-full rounded-2xl border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800 px-4 py-3 text-sm text-slate-800 dark:text-slate-200 placeholder:text-slate-400 focus:border-brand-500 focus:ring-1 focus:ring-brand-500 focus:outline-none"
                  />
                </div>

                <div className="flex gap-3 pt-1">
                  <button
                    onClick={() => setReviewOffer(null)}
                    className="flex-1 px-4 py-2.5 bg-slate-100 dark:bg-slate-800 hover:bg-slate-200 dark:hover:bg-slate-700 text-slate-700 dark:text-slate-300 rounded-xl font-bold text-xs transition-colors"
                  >
                    Cancelar
                  </button>
                  <button
                    onClick={handleSubmitReview}
                    disabled={isReviewSubmitting || reviewRating < 1}
                    className="flex-1 px-4 py-2.5 bg-amber-500 hover:bg-amber-600 disabled:opacity-50 disabled:cursor-not-allowed text-white rounded-xl font-bold text-xs shadow-md transition-all inline-flex items-center justify-center gap-1.5"
                  >
                    <Star className="w-3.5 h-3.5" />
                    {isReviewSubmitting ? 'Enviando...' : 'Enviar Calificación'}
                  </button>
                </div>
              </>
            )}
          </div>
        </div>
      )}
    </div>
  );
};
