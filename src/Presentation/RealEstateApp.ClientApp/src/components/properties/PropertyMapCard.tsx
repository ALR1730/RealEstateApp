import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Property } from '../../types';
import { useCurrency } from '../../context/CurrencyContext';
import { Bed, Bath, MapPin } from 'lucide-react';

interface PropertyMapCardProps {
  property: Property;
}

export const PropertyMapCard: React.FC<PropertyMapCardProps> = ({ property }) => {
  const navigate = useNavigate();
  const { formatPrice } = useCurrency();

  const firstImg = property.images && property.images.length > 0 ? property.images[0] : null;
  const mainImage = firstImg
    ? (typeof firstImg === 'string' ? firstImg : firstImg.imageUrl || 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?auto=format&fit=crop&w=400&q=80')
    : 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?auto=format&fit=crop&w=400&q=80';

  return (
    <div
      onClick={() => navigate(`/property/${property.id}`)}
      className="flex gap-3 p-3 bg-white dark:bg-slate-900 rounded-2xl border border-slate-200/80 dark:border-slate-800 shadow-sm hover:shadow-md hover:border-slate-300 dark:hover:border-slate-700 cursor-pointer transition-all duration-200"
    >
      <img
        src={mainImage}
        alt={property.description || `Propiedad ${property.code}`}
        className="w-20 h-20 rounded-xl object-cover shrink-0"
        loading="lazy"
      />
      <div className="flex-1 min-w-0 space-y-1">
        <p className="text-sm font-extrabold text-slate-900 dark:text-slate-100 truncate">
          {formatPrice(property.price)}
        </p>
        <p className="text-[11px] font-semibold text-emerald-700 dark:text-emerald-400 uppercase tracking-wide">
          {property.propertyTypeName || 'Inmueble'} • {property.saleTypeName || 'Venta'}
        </p>
        <div className="flex items-center gap-3 text-[11px] text-slate-500 dark:text-slate-400 font-medium">
          <span className="flex items-center gap-1">
            <Bed className="w-3 h-3" />
            {property.bedrooms ?? property.rooms ?? 0}
          </span>
          <span className="flex items-center gap-1">
            <Bath className="w-3 h-3" />
            {property.bathrooms}
          </span>
        </div>
        <div className="flex items-center gap-1 text-[11px] text-slate-400 dark:text-slate-500 font-medium truncate">
          <MapPin className="w-3 h-3 shrink-0" />
          <span className="truncate">
            {property.provinceName
              ? `${property.provinceName}, ${property.sector || 'RD'}`
              : 'República Dominicana'}
          </span>
        </div>
      </div>
    </div>
  );
};
