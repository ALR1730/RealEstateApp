import React from 'react';
import { Compass } from 'lucide-react';

interface VirtualTourEmbedProps {
  url: string;
}

export const VirtualTourEmbed: React.FC<VirtualTourEmbedProps> = ({ url }) => {
  if (!url || url.trim() === '') {
    return (
      <div className="bg-white rounded-3xl p-6 sm:p-8 border border-slate-200 space-y-4 shadow-xs">
        <div className="flex items-center gap-2">
          <Compass className="w-5 h-5 text-sky-600" />
          <h3 className="font-extrabold text-lg text-slate-900">Recorrido Virtual 360°</h3>
        </div>
        <div className="flex flex-col items-center justify-center py-12 bg-sky-50 rounded-2xl border border-sky-100">
          <Compass className="w-10 h-10 text-sky-300 mb-3" />
          <p className="text-xs text-sky-600 font-semibold text-center">
            No hay recorrido virtual disponible para esta propiedad.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-3xl p-6 sm:p-8 border border-slate-200 space-y-4 shadow-xs">
      <div className="flex items-center gap-2">
        <Compass className="w-5 h-5 text-sky-600" />
        <h3 className="font-extrabold text-lg text-slate-900">Recorrido Virtual 360°</h3>
      </div>
      <div className="relative w-full overflow-hidden rounded-2xl border border-slate-200" style={{ paddingBottom: '56.25%' }}>
        <iframe
          src={url}
          title="Recorrido Virtual 360°"
          className="absolute inset-0 w-full h-full"
          allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
          allowFullScreen
        />
      </div>
    </div>
  );
};
