import React from 'react';
import { Compass } from 'lucide-react';

interface VirtualTourEmbedProps {
  url: string;
}

export const VirtualTourEmbed: React.FC<VirtualTourEmbedProps> = ({ url }) => {
  if (!url || url.trim() === '') {
    return (
      <div className="bg-white dark:bg-slate-900 rounded-3xl p-6 sm:p-8 border border-slate-200 dark:border-slate-800 space-y-4 shadow-xs">
        <div className="flex items-center gap-2">
          <Compass className="w-5 h-5 text-sky-600 dark:text-sky-400" />
          <h3 className="font-extrabold text-lg text-slate-900 dark:text-white">Recorrido Virtual 360°</h3>
        </div>
        <div className="flex flex-col items-center justify-center py-12 bg-sky-50 dark:bg-sky-950/40 rounded-2xl border border-sky-100 dark:border-sky-800/60">
          <Compass className="w-10 h-10 text-sky-300 dark:text-sky-500 mb-3" />
          <p className="text-xs text-sky-600 dark:text-sky-400 font-semibold text-center">
            No hay recorrido virtual disponible para esta propiedad.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white dark:bg-slate-900 rounded-3xl p-6 sm:p-8 border border-slate-200 dark:border-slate-800 space-y-4 shadow-xs">
      <div className="flex items-center gap-2">
        <Compass className="w-5 h-5 text-sky-600 dark:text-sky-400" />
        <h3 className="font-extrabold text-lg text-slate-900 dark:text-white">Recorrido Virtual 360°</h3>
      </div>
      <div className="relative w-full overflow-hidden rounded-2xl border border-slate-200 dark:border-slate-800" style={{ paddingBottom: '56.25%' }}>
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
