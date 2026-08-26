import React from 'react';
import { Video } from 'lucide-react';

interface VideoPlayerProps {
  url: string;
}

function extractYouTubeId(url: string): string | null {
  const patterns = [
    /(?:youtube\.com\/watch\?v=)([a-zA-Z0-9_-]{11})/,
    /(?:youtu\.be\/)([a-zA-Z0-9_-]{11})/,
    /(?:youtube\.com\/embed\/)([a-zA-Z0-9_-]{11})/,
    /(?:youtube\.com\/v\/)([a-zA-Z0-9_-]{11})/,
  ];
  for (const pattern of patterns) {
    const match = url.match(pattern);
    if (match) return match[1];
  }
  return null;
}

export const VideoPlayer: React.FC<VideoPlayerProps> = ({ url }) => {
  if (!url || url.trim() === '') {
    return (
      <div className="bg-white rounded-3xl p-6 sm:p-8 border border-slate-200 space-y-4 shadow-xs">
        <div className="flex items-center gap-2">
          <Video className="w-5 h-5 text-rose-600" />
          <h3 className="font-extrabold text-lg text-slate-900">Video Tour</h3>
        </div>
        <div className="flex flex-col items-center justify-center py-12 bg-rose-50 rounded-2xl border border-rose-100">
          <Video className="w-10 h-10 text-rose-300 mb-3" />
          <p className="text-xs text-rose-600 font-semibold text-center">
            No hay video tour disponible para esta propiedad.
          </p>
        </div>
      </div>
    );
  }

  const youtubeId = extractYouTubeId(url);

  return (
    <div className="bg-white rounded-3xl p-6 sm:p-8 border border-slate-200 space-y-4 shadow-xs">
      <div className="flex items-center gap-2">
        <Video className="w-5 h-5 text-rose-600" />
        <h3 className="font-extrabold text-lg text-slate-900">Video Tour</h3>
      </div>
      <div className="relative w-full overflow-hidden rounded-2xl border border-slate-200" style={{ paddingBottom: '56.25%' }}>
        {youtubeId ? (
          <iframe
            src={`https://www.youtube.com/embed/${youtubeId}`}
            title="Video Tour de la Propiedad"
            className="absolute inset-0 w-full h-full"
            allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
            allowFullScreen
          />
        ) : (
          <iframe
            src={url}
            title="Video Tour de la Propiedad"
            className="absolute inset-0 w-full h-full"
            allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
            allowFullScreen
          />
        )}
      </div>
    </div>
  );
};
