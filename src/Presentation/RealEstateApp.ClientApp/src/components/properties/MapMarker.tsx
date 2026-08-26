import React from 'react';
import { Link } from 'react-router-dom';
import { Property } from '../../types';
import { useCurrency } from '../../context/CurrencyContext';
import { Bed, Bath, ArrowRight } from 'lucide-react';

interface MapMarkerProps {
  property: Property;
}

export const MapMarker: React.FC<MapMarkerProps> = ({ property }) => {
  const { formatPrice } = useCurrency();

  const mainImage =
    property.images && property.images.length > 0
      ? property.images[0].imageUrl
      : 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?auto=format&fit=crop&w=400&q=80';

  return (
    <div className="min-w-[220px] max-w-[260px] space-y-2">
      <img
        src={mainImage}
        alt={property.description || `Propiedad ${property.code}`}
        className="w-full h-28 object-cover rounded-lg"
        loading="lazy"
      />
      <div className="space-y-1">
        <p className="text-base font-extrabold text-slate-900">
          {formatPrice(property.price)}
        </p>
        <p className="text-[11px] font-semibold text-emerald-700 uppercase tracking-wide">
          {property.propertyTypeName || 'Inmueble'} • {property.saleTypeName || 'Venta'}
        </p>
        <div className="flex items-center gap-3 text-[11px] text-slate-500 font-medium">
          <span className="flex items-center gap-1">
            <Bed className="w-3 h-3" />
            {property.bedrooms} hab
          </span>
          <span className="flex items-center gap-1">
            <Bath className="w-3 h-3" />
            {property.bathrooms} baños
          </span>
        </div>
      </div>
      <Link
        to={`/property/${property.id}`}
        className="inline-flex items-center gap-1 text-[11px] font-bold text-brand-600 hover:text-brand-700"
      >
        Ver Detalle
        <ArrowRight className="w-3 h-3" />
      </Link>
    </div>
  );
};
