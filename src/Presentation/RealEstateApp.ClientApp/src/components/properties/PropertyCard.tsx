import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { Property } from '../../types';
import { formatCurrencyRD } from '../../utils/formatters';
import { Badge } from '../common/Badge';
import { useAuth } from '../../context/AuthContext';
import { CompareCheckbox } from './CompareCheckbox';
import { favoritesService } from '../../api/services';
import { 
  Bed, 
  Bath, 
  Maximize2, 
  MapPin, 
  Heart, 
  Sparkles, 
  Image as ImageIcon,
  ArrowRight
} from 'lucide-react';

interface PropertyCardProps {
  property: Property;
  onFavoriteToggle?: (propertyId: number, isFav: boolean) => void;
  isFavoriteInitial?: boolean;
}

export const PropertyCard: React.FC<PropertyCardProps> = ({
  property,
  onFavoriteToggle,
  isFavoriteInitial = false,
}) => {
  const { isAuthenticated, isClient } = useAuth();
  const [isFavorite, setIsFavorite] = useState(isFavoriteInitial);
  const [isTogglingFav, setIsTogglingFav] = useState(false);

  const mainImage = property.images && property.images.length > 0
    ? property.images[0].imageUrl
    : 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?auto=format&fit=crop&w=800&q=80';

  const handleFavoriteClick = async (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();

    if (!isAuthenticated || !isClient) {
      alert("Inicia sesión como Cliente para guardar propiedades en tus favoritos.");
      return;
    }

    try {
      setIsTogglingFav(true);
      if (isFavorite) {
        await favoritesService.remove(property.id);
        setIsFavorite(false);
        onFavoriteToggle?.(property.id, false);
      } else {
        await favoritesService.add(property.id);
        setIsFavorite(true);
        onFavoriteToggle?.(property.id, true);
      }
    } catch (err) {
      console.error("Error toggling favorite:", err);
    } finally {
      setIsTogglingFav(false);
    }
  };

  const isSold = property.status === 'Sold';

  return (
    <div className="group relative bg-white rounded-3xl overflow-hidden border border-slate-200/80 shadow-xs hover:shadow-xl hover:border-slate-300 transition-all duration-300 flex flex-col">
      
      {/* Image & Badges Container */}
      <div className="relative aspect-4/3 overflow-hidden bg-slate-100">
        <img
          src={mainImage}
          alt={property.description || `Propiedad ${property.code}`}
          className="w-full h-full object-cover object-center group-hover:scale-105 transition-transform duration-500"
          loading="lazy"
        />

        {/* Gradient Overlay */}
        <div className="absolute inset-0 bg-gradient-to-t from-slate-950/70 via-transparent to-black/20" />

        {/* Top Badges */}
        <div className="absolute top-3 left-3 right-3 flex justify-between items-center pointer-events-none">
          <div className="flex flex-wrap gap-1.5 pointer-events-auto">
            <Badge status={property.status} />
            {property.isFeatured && (
              <span className="inline-flex items-center gap-1 bg-amber-500 text-white font-extrabold text-[10px] uppercase tracking-wider px-2.5 py-0.5 rounded-full shadow-sm">
                <Sparkles className="w-3 h-3" />
                Destacada
              </span>
            )}
          </div>

          {/* Favorite Button */}
          {isClient && (
            <button
              onClick={handleFavoriteClick}
              disabled={isTogglingFav}
              className={`pointer-events-auto p-2 rounded-full backdrop-blur-md transition-all shadow-md ${
                isFavorite
                  ? 'bg-rose-500 text-white hover:bg-rose-600'
                  : 'bg-white/80 text-slate-700 hover:bg-white hover:text-rose-500'
              }`}
              title={isFavorite ? 'Quitar de favoritos' : 'Guardar en favoritos'}
            >
              <Heart className={`w-4 h-4 ${isFavorite ? 'fill-current' : ''}`} />
            </button>
          )}

          {/* Compare Button */}
          <div className="pointer-events-auto">
            <CompareCheckbox property={property} />
          </div>
        </div>

        {/* Bottom Image Info */}
        <div className="absolute bottom-3 left-3 right-3 flex justify-between items-end text-white">
          <div>
            <span className="text-[11px] font-bold tracking-widest uppercase text-emerald-300 bg-emerald-950/60 backdrop-blur-xs px-2 py-0.5 rounded-md">
              {property.propertyTypeName || 'Inmueble'} • {property.saleTypeName || 'Venta'}
            </span>
            <p className="text-xl font-extrabold tracking-tight mt-1 text-white drop-shadow-sm">
              {formatCurrencyRD(property.price)}
            </p>
          </div>

          {/* Photo count indicator */}
          {property.images && property.images.length > 1 && (
            <span className="text-[11px] font-semibold bg-slate-900/60 backdrop-blur-xs px-2 py-1 rounded-lg flex items-center gap-1">
              <ImageIcon className="w-3 h-3" />
              {property.images.length}
            </span>
          )}
        </div>
      </div>

      {/* Body Content */}
      <div className="p-5 flex flex-col flex-1 justify-between">
        <div className="space-y-3">
          
          {/* Location & Code */}
          <div className="flex items-center justify-between text-xs text-slate-500">
            <div className="flex items-center gap-1 truncate max-w-[70%]">
              <MapPin className="w-3.5 h-3.5 text-brand-600 shrink-0" />
              <span className="truncate font-medium">
                {property.provinceName ? `${property.provinceName}, ${property.sector || 'RD'}` : 'República Dominicana'}
              </span>
            </div>
            <span className="font-mono text-[11px] bg-slate-100 font-bold px-2 py-0.5 rounded text-slate-600">
              #{property.code}
            </span>
          </div>

          {/* Description */}
          <p className="text-xs text-slate-600 line-clamp-2 leading-relaxed">
            {property.description || 'Excelente oportunidad inmobiliaria con amenidades completas y ubicación estratégica.'}
          </p>

          {/* Specs: Bedrooms, Bathrooms, Size */}
          <div className="grid grid-cols-3 gap-2 py-2 border-y border-slate-100 text-slate-700 text-xs font-semibold">
            <div className="flex items-center gap-1.5">
              <Bed className="w-4 h-4 text-slate-400" />
              <span>{property.bedrooms} hab</span>
            </div>
            <div className="flex items-center gap-1.5">
              <Bath className="w-4 h-4 text-slate-400" />
              <span>{property.bathrooms} baños</span>
            </div>
            <div className="flex items-center gap-1.5">
              <Maximize2 className="w-4 h-4 text-slate-400" />
              <span>{property.landSizeMeters} m²</span>
            </div>
          </div>
        </div>

        {/* Footer Link Button */}
        <div className="pt-4 mt-2 flex items-center justify-between">
          <div className="flex items-center gap-2">
            {property.agentPhotoUrl ? (
              <img src={property.agentPhotoUrl} alt="Agente" className="w-7 h-7 rounded-full object-cover border border-slate-200" />
            ) : (
              <div className="w-7 h-7 rounded-full bg-slate-200 text-slate-600 flex items-center justify-center font-bold text-[10px]">
                {property.agentName ? property.agentName[0] : 'A'}
              </div>
            )}
            <span className="text-xs font-medium text-slate-600 truncate max-w-[110px]">
              {property.agentName || 'Agente'}
            </span>
          </div>

          <Link
            to={`/property/${property.id}`}
            className="inline-flex items-center gap-1 text-xs font-bold text-brand-600 hover:text-brand-700 group/link"
          >
            <span>Ver Detalle</span>
            <ArrowRight className="w-3.5 h-3.5 group-hover/link:translate-x-1 transition-transform" />
          </Link>
        </div>
      </div>
    </div>
  );
};
