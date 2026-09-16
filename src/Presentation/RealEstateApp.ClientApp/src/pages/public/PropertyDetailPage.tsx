import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { Property, PriceHistory } from '../../types';
import { propertiesService, favoritesService } from '../../api/services';
import { useCurrency } from '../../context/CurrencyContext';
import { Badge } from '../../components/common/Badge';
import { Loader } from '../../components/common/Loader';
import { GalleryModal } from '../../components/properties/GalleryModal';
import { PriceHistoryChart } from '../../components/properties/PriceHistoryChart';
import { VirtualTourEmbed } from '../../components/properties/VirtualTourEmbed';
import { VideoPlayer } from '../../components/properties/VideoPlayer';
import { MortgageCalculator } from '../../components/simulator/MortgageCalculator';
import { OfferModal } from '../../components/offers/OfferModal';
import { AppointmentModal } from '../../components/appointments/AppointmentModal';
import { ChatBox } from '../../components/chat/ChatBox';
import { useAuth } from '../../context/AuthContext';
import { 
  Bed, 
  Bath, 
  Maximize2, 
  MapPin, 
  Heart, 
  Tag, 
  Calendar, 
  MessageSquare, 
  Sparkles, 
  Phone, 
  Check, 
  Share2, 
  ArrowLeft,
  Eye
} from 'lucide-react';

export const PropertyDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { isAuthenticated, isClient } = useAuth();
  const { formatPrice } = useCurrency();

  const [property, setProperty] = useState<Property | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isFavorite, setIsFavorite] = useState<boolean>(false);
  const [priceHistory, setPriceHistory] = useState<PriceHistory[]>([]);
  const [refreshKey, setRefreshKey] = useState(0);

  // Modals state
  const [galleryOpen, setGalleryOpen] = useState<boolean>(false);
  const [galleryIndex, setGalleryIndex] = useState<number>(0);
  const [offerModalOpen, setOfferModalOpen] = useState<boolean>(false);
  const [appointmentModalOpen, setAppointmentModalOpen] = useState<boolean>(false);
  const [chatOpen, setChatOpen] = useState<boolean>(false);

  useEffect(() => {
    let isMounted = true;
    const fetchPropertyDetails = async () => {
      if (!id) return;
      try {
        const data = await propertiesService.getById(Number(id));
        if (!isMounted) return;
        setProperty(data);

        if (isAuthenticated && isClient) {
          try {
            const isFav = await favoritesService.checkIsFavorite(Number(id));
            if (isMounted) setIsFavorite(isFav);
          } catch {
            // ignore
          }
        }

        try {
          const history = await propertiesService.getPriceHistory(Number(id));
          if (isMounted) setPriceHistory(history);
        } catch {
          // ignore
        }
      } catch (err) {
        console.error("Error loading property:", err);
      } finally {
        if (isMounted) setIsLoading(false);
      }
    };

    fetchPropertyDetails();
    return () => {
      isMounted = false;
    };
  }, [id, isAuthenticated, isClient, refreshKey]);

  const handleToggleFavorite = async () => {
    if (!isAuthenticated || !isClient) {
      alert("Inicia sesión como Cliente para guardar propiedades.");
      return;
    }
    if (!property) return;

    try {
      if (isFavorite) {
        await favoritesService.remove(property.id);
        setIsFavorite(false);
      } else {
        await favoritesService.add(property.id);
        setIsFavorite(true);
      }
    } catch (err) {
      console.error("Error toggling favorite:", err);
    }
  };

  const handleShare = () => {
    if (navigator.share) {
      navigator.share({
        title: `Propiedad #${property?.code} en RD$`,
        text: property?.description,
        url: window.location.href,
      }).catch(() => {});
    } else {
      navigator.clipboard.writeText(window.location.href);
      alert("Enlace copiado al portapapeles.");
    }
  };

  if (isLoading) {
    return (
      <div className="py-20">
        <Loader text="Cargando detalles de la propiedad..." size="lg" />
      </div>
    );
  }

  if (!property) {
    return (
      <div className="max-w-xl mx-auto py-20 text-center space-y-4">
        <h2 className="text-2xl font-bold text-slate-800 dark:text-slate-200">Propiedad no encontrada</h2>
        <p className="text-xs text-slate-500 dark:text-slate-400">El inmueble solicitado no existe o fue retirado del catálogo.</p>
        <Link to="/catalog" className="inline-block px-5 py-2 bg-brand-600 text-white font-bold text-xs rounded-xl">
          Volver al Catálogo
        </Link>
      </div>
    );
  }

  const isSold = property.status === 'Sold' || property.status === 'Vendida';
  const images = property.images || [];
  const firstImg = images.length > 0 ? images[0] : null;
  const mainImage = firstImg
    ? (typeof firstImg === 'string' ? firstImg : firstImg.imageUrl || 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?auto=format&fit=crop&w=1200&q=80')
    : 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?auto=format&fit=crop&w=1200&q=80';

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8 pb-20">
      
      {/* Top Breadcrumb & Actions Bar */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div className="flex items-center gap-2 text-xs font-semibold text-slate-500 dark:text-slate-400">
          <Link to="/catalog" className="hover:text-brand-600 flex items-center gap-1">
            <ArrowLeft className="w-4 h-4" />
            <span>Catálogo</span>
          </Link>
          <span>/</span>
          <span className="text-slate-900 dark:text-slate-200 font-bold">{property.propertyTypeName || 'Propiedad'}</span>
          <span>/</span>
          <span className="font-mono text-slate-400">#{property.code}</span>
        </div>

        <div className="flex items-center gap-2">
          {/* Share Button */}
          <button
            onClick={handleShare}
            className="p-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 text-slate-700 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-slate-800 text-xs font-bold transition-colors flex items-center gap-1.5"
            title="Compartir enlace"
          >
            <Share2 className="w-4 h-4" />
            <span>Compartir</span>
          </button>

          {/* Favorite Button */}
          {isClient && (
            <button
              onClick={handleToggleFavorite}
              className={`px-4 py-2.5 rounded-xl border text-xs font-bold transition-colors flex items-center gap-2 shadow-xs ${
                isFavorite
                  ? 'bg-rose-50 dark:bg-rose-950/40 border-rose-200 dark:border-rose-800 text-rose-600 dark:text-rose-400'
                  : 'bg-white dark:bg-slate-900 border-slate-200 dark:border-slate-800 text-slate-700 dark:text-slate-300 hover:text-rose-600'
              }`}
            >
              <Heart className={`w-4 h-4 ${isFavorite ? 'fill-current' : ''}`} />
              <span>{isFavorite ? 'Guardado en Favoritos' : 'Guardar en Favoritos'}</span>
            </button>
          )}
        </div>
      </div>

      {/* Hero Gallery Grid (Up to 5 photo preview) */}
      <div className="relative rounded-3xl overflow-hidden shadow-lg bg-slate-900 border border-slate-200 dark:border-slate-800">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-2 aspect-[16/9] max-h-[500px] p-2 bg-slate-900">
          
          {/* Main Photo (Takes 2 cols) */}
          <div
            onClick={() => { setGalleryIndex(0); setGalleryOpen(true); }}
            className="md:col-span-2 relative overflow-hidden rounded-2xl cursor-pointer group"
          >
            <img
              src={mainImage}
              alt={property.description}
              className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
            />
            <div className="absolute inset-0 bg-black/20 group-hover:bg-black/0 transition-colors" />
          </div>

          {/* Side Photos (Col 3 & 4) */}
          <div className="hidden md:grid md:col-span-2 grid-cols-2 gap-2">
            {[1, 2, 3, 4].map((idx) => {
              const img = images[idx];
              const imgSrc = img ? (typeof img === 'string' ? img : img.imageUrl) : null;
              return (
                <div
                  key={idx}
                  onClick={() => { setGalleryIndex(idx < images.length ? idx : 0); setGalleryOpen(true); }}
                  className="relative rounded-2xl overflow-hidden bg-slate-800 cursor-pointer group"
                >
                  {imgSrc ? (
                    <img
                      src={imgSrc}
                      alt=""
                      className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                    />
                  ) : (
                    <div className="w-full h-full flex items-center justify-center bg-slate-800 text-slate-500 text-xs font-semibold">
                      RealEstateApp
                    </div>
                  )}
                  {idx === 4 && images.length > 5 && (
                    <div className="absolute inset-0 bg-black/60 backdrop-blur-xs flex items-center justify-center text-white font-extrabold text-sm">
                      +{images.length - 4} fotos
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        </div>

        {/* Floating View All Photos Button */}
        {images.length > 0 && (
          <button
            onClick={() => { setGalleryIndex(0); setGalleryOpen(true); }}
            className="absolute bottom-5 right-5 flex items-center gap-2 px-4 py-2 bg-slate-900/80 hover:bg-slate-900 text-white backdrop-blur-md rounded-xl text-xs font-bold shadow-xl transition-all"
          >
            <Eye className="w-4 h-4 text-emerald-400" />
            <span>Ver {images.length} Fotografías</span>
          </button>
        )}
      </div>

      {/* Main Layout: Details (Left) + Actions/Agent (Right) */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        {/* Left Column: Details & Specs */}
        <div className="lg:col-span-2 space-y-8">
          
          {/* Header Info */}
          <div className="bg-white rounded-3xl p-6 sm:p-8 border border-slate-200 space-y-4 shadow-xs">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <div className="flex items-center gap-2">
                <Badge status={property.status} />
                <span className="text-xs font-extrabold px-3 py-1 bg-brand-50 text-brand-700 rounded-full border border-brand-200/60 uppercase tracking-wider">
                  {property.propertyTypeName || 'Inmueble'}
                </span>
                <span className="text-xs font-extrabold px-3 py-1 bg-indigo-50 text-indigo-700 rounded-full border border-indigo-200/60 uppercase tracking-wider">
                  {property.saleTypeName || 'Venta'}
                </span>
              </div>
              <span className="font-mono text-xs font-bold text-slate-500 bg-slate-100 px-3 py-1 rounded-lg">
                Código: #{property.code}
              </span>
            </div>

            <div className="space-y-2">
              <p className="text-3xl sm:text-4xl font-extrabold text-slate-900 dark:text-slate-100 tracking-tight font-mono text-emerald-600 dark:text-emerald-400">
                {formatPrice(property.price)}
              </p>
              <div className="flex items-center gap-1.5 text-xs sm:text-sm text-slate-500 dark:text-slate-400 font-medium">
                <MapPin className="w-4 h-4 text-brand-600 shrink-0" />
                <span>
                  {property.provinceName ? `${property.provinceName}, ${property.municipalityName || ''} - ${property.sector || 'RD'}` : 'República Dominicana'}
                </span>
              </div>
            </div>

            {/* Core Specs Bar */}
            <div className="grid grid-cols-3 gap-4 pt-4 border-t border-slate-100 dark:border-slate-800 text-center">
              <div className="p-3 bg-slate-50 dark:bg-slate-800/60 rounded-2xl">
                <Bed className="w-5 h-5 text-brand-600 mx-auto mb-1" />
                <span className="text-base font-extrabold text-slate-800 dark:text-slate-200">{property.bedrooms ?? property.rooms ?? 0}</span>
                <p className="text-[11px] text-slate-500 dark:text-slate-400 uppercase font-semibold">Habitaciones</p>
              </div>
              <div className="p-3 bg-slate-50 dark:bg-slate-800/60 rounded-2xl">
                <Bath className="w-5 h-5 text-brand-600 mx-auto mb-1" />
                <span className="text-base font-extrabold text-slate-800 dark:text-slate-200">{property.bathrooms}</span>
                <p className="text-[11px] text-slate-500 dark:text-slate-400 uppercase font-semibold">Baños</p>
              </div>
              <div className="p-3 bg-slate-50 dark:bg-slate-800/60 rounded-2xl">
                <Maximize2 className="w-5 h-5 text-brand-600 mx-auto mb-1" />
                <span className="text-base font-extrabold text-slate-800 dark:text-slate-200">{property.landSizeMeters ?? property.sizeInMeters ?? 0}</span>
                <p className="text-[11px] text-slate-500 dark:text-slate-400 uppercase font-semibold">Metros² (m²)</p>
              </div>
            </div>
          </div>

          {/* Description */}
          <div className="bg-white dark:bg-slate-900 rounded-3xl p-6 sm:p-8 border border-slate-200 dark:border-slate-800 space-y-4 shadow-sm">
            <h3 className="font-extrabold text-lg text-slate-900 dark:text-slate-100">Descripción del Inmueble</h3>
            <p className="text-xs sm:text-sm text-slate-600 dark:text-slate-300 leading-relaxed whitespace-pre-line">
              {property.description}
            </p>
          </div>

          {/* Amenities & Improvements */}
          {property.improvements && property.improvements.length > 0 && (
            <div className="bg-white dark:bg-slate-900 rounded-3xl p-6 sm:p-8 border border-slate-200 dark:border-slate-800 space-y-4 shadow-sm">
              <div className="flex items-center gap-2">
                <Sparkles className="w-5 h-5 text-brand-600" />
                <h3 className="font-extrabold text-lg text-slate-900 dark:text-slate-100">Amenidades y Mejoras</h3>
              </div>
              <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
                {property.improvements.map((imp) => (
                  <div key={imp.id} className="flex items-center gap-2 p-2.5 rounded-xl bg-slate-50 dark:bg-slate-800/60 border border-slate-100 dark:border-slate-700/60 text-xs font-semibold text-slate-700 dark:text-slate-300">
                    <Check className="w-4 h-4 text-emerald-500 shrink-0" />
                    <span className="truncate">{imp.name}</span>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Virtual Tour 360° Embedded */}
          {(property.virtualTour360Url || property.tour360Url) && (
            <VirtualTourEmbed url={property.virtualTour360Url || property.tour360Url || ''} />
          )}

          {/* Video Tour Embedded */}
          {(property.videoTourUrl || property.videoUrl) && (
            <VideoPlayer url={property.videoTourUrl || property.videoUrl || ''} />
          )}

          {/* Price History Timeline */}
          {priceHistory.length > 0 && (
            <PriceHistoryChart history={priceHistory} />
          )}

          {/* Interactive Mortgage Calculator Embedded for this Property */}
          <MortgageCalculator initialPrice={property.price} />
        </div>

        {/* Right Column: Sticky Action Card & Agent Info */}
        <div className="space-y-6">
          
          {/* Action Card */}
          <div className="bg-white dark:bg-slate-900 rounded-3xl p-6 border border-slate-200 dark:border-slate-800 shadow-sm space-y-6 sticky top-24">
            
            <div className="space-y-1">
              <span className="text-[11px] font-bold uppercase tracking-wider text-slate-400">
                Valor Transaccional
              </span>
              <p className="text-2xl sm:text-3xl font-extrabold text-slate-900 dark:text-slate-100 font-mono text-emerald-600 dark:text-emerald-400">
                {formatPrice(property.price)}
              </p>
            </div>

            {/* Action Buttons */}
            <div className="space-y-2.5">
              {!isSold ? (
                <>
                  <button
                    onClick={() => {
                      if (!isAuthenticated) {
                        alert("Inicia sesión como Cliente para enviar ofertas.");
                        navigate('/login');
                        return;
                      }
                      setOfferModalOpen(true);
                    }}
                    className="w-full py-3 bg-brand-600 hover:bg-brand-700 text-white font-extrabold text-xs sm:text-sm rounded-2xl shadow-lg shadow-brand-600/30 flex items-center justify-center gap-2 transition-all cursor-pointer"
                  >
                    <Tag className="w-4 h-4" />
                    Enviar Oferta de Compra
                  </button>

                  <button
                    onClick={() => {
                      if (!isAuthenticated) {
                        alert("Inicia sesión para agendar una visita.");
                        navigate('/login');
                        return;
                      }
                      setAppointmentModalOpen(true);
                    }}
                    className="w-full py-3 bg-slate-900 hover:bg-slate-800 text-white font-extrabold text-xs sm:text-sm rounded-2xl flex items-center justify-center gap-2 transition-all cursor-pointer"
                  >
                    <Calendar className="w-4 h-4" />
                    Agendar Visita Inmobiliaria
                  </button>

                  <button
                    onClick={() => {
                      if (!isAuthenticated) {
                        alert("Inicia sesión para chatear con el agente.");
                        navigate('/login');
                        return;
                      }
                      setChatOpen(!chatOpen);
                    }}
                    className="w-full py-3 bg-indigo-50 hover:bg-indigo-100 text-royal-700 font-extrabold text-xs sm:text-sm rounded-2xl border border-indigo-200/60 flex items-center justify-center gap-2 transition-all cursor-pointer"
                  >
                    <MessageSquare className="w-4 h-4" />
                    {chatOpen ? 'Cerrar Chat Directo' : 'Chatear con el Agente'}
                  </button>
                </>
              ) : (
                <div className="p-4 bg-rose-50 rounded-2xl border border-rose-200 text-center space-y-1">
                  <span className="font-extrabold text-xs text-rose-700 uppercase tracking-wide">
                    Propiedad Vendida
                  </span>
                  <p className="text-[11px] text-rose-600">
                    Esta propiedad ya fue vendida y no admite nuevas ofertas ni solicitudes de visitas.
                  </p>
                </div>
              )}
            </div>

            {/* Agent Profile Card */}
            <div className="pt-4 border-t border-slate-100 space-y-3">
              <span className="text-[10px] font-extrabold uppercase tracking-widest text-slate-400">
                Agente Responsable
              </span>
              
              <div className="flex items-center gap-3">
                <div className="w-12 h-12 rounded-2xl bg-gradient-to-tr from-navy-800 to-slate-700 text-white flex items-center justify-center font-bold text-sm overflow-hidden shadow-md">
                  {property.agentPhotoUrl ? (
                    <img src={property.agentPhotoUrl} alt="" className="w-full h-full object-cover" />
                  ) : (
                    property.agentName ? property.agentName[0] : 'A'
                  )}
                </div>
                <div className="overflow-hidden">
                  <h4 className="font-bold text-slate-900 text-sm truncate">
                    {property.agentName || 'Agente Inmobiliario'}
                  </h4>
                  <p className="text-xs text-slate-500 truncate">{property.agentEmail}</p>
                </div>
              </div>

              {property.agentPhone && (
                <div className="flex items-center gap-2 text-xs font-semibold text-slate-700 bg-slate-50 p-2.5 rounded-xl border border-slate-100">
                  <Phone className="w-4 h-4 text-brand-600" />
                  <span>{property.agentPhone}</span>
                </div>
              )}
            </div>
          </div>

          {/* Chat Drawer Widget */}
          {chatOpen && property.agentId && (
            <div className="animate-in fade-in zoom-in-95">
              <ChatBox
                propertyId={property.id}
                recipientId={property.agentId}
                recipientName={property.agentName}
                propertyCode={property.code}
              />
            </div>
          )}
        </div>
      </div>

      {/* Gallery Modal */}
      <GalleryModal
        isOpen={galleryOpen}
        onClose={() => setGalleryOpen(false)}
        images={images}
        initialIndex={galleryIndex}
      />

      {/* Offer Modal */}
      <OfferModal
        isOpen={offerModalOpen}
        onClose={() => setOfferModalOpen(false)}
        property={property}
        onOfferSuccess={() => setRefreshKey((k) => k + 1)}
      />

      {/* Appointment Modal */}
      <AppointmentModal
        isOpen={appointmentModalOpen}
        onClose={() => setAppointmentModalOpen(false)}
        property={property}
      />
    </div>
  );
};
