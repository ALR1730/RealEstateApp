import React, { useEffect } from 'react';
import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import iconUrl from 'leaflet/dist/images/marker-icon.png';
import iconRetinaUrl from 'leaflet/dist/images/marker-icon-2x.png';
import shadowUrl from 'leaflet/dist/images/marker-shadow.png';
import { Property } from '../../types';
import { MapMarker } from './MapMarker';

delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({ iconUrl, iconRetinaUrl, shadowUrl });

interface PropertyMapProps {
  properties: Property[];
}

export const PropertyMap: React.FC<PropertyMapProps> = ({ properties }) => {
  return (
    <div className="w-full h-full min-h-[500px] rounded-3xl overflow-hidden border border-slate-200 shadow-xs">
      <MapContainer
        center={[18.7357, -70.1627]}
        zoom={8}
        scrollWheelZoom={true}
        className="w-full h-full"
        style={{ height: '100%', minHeight: '500px' }}
      >
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />
        {properties
          .filter((p) => p.latitude && p.longitude)
          .map((property) => (
            <Marker
              key={property.id}
              position={[property.latitude!, property.longitude!]}
            >
              <Popup>
                <MapMarker property={property} />
              </Popup>
            </Marker>
          ))}
      </MapContainer>
    </div>
  );
};
