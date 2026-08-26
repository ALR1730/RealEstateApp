import React, { useState } from 'react';
import { PropertyImage } from '../../types';
import { X, ChevronLeft, ChevronRight, Maximize } from 'lucide-react';

interface GalleryModalProps {
  isOpen: boolean;
  onClose: () => void;
  images: PropertyImage[];
  initialIndex?: number;
}

export const GalleryModal: React.FC<GalleryModalProps> = ({
  isOpen,
  onClose,
  images,
  initialIndex = 0,
}) => {
  const [currentIndex, setCurrentIndex] = useState(initialIndex);

  if (!isOpen || images.length === 0) return null;

  const prev = () => setCurrentIndex((idx) => (idx === 0 ? images.length - 1 : idx - 1));
  const next = () => setCurrentIndex((idx) => (idx === images.length - 1 ? 0 : idx + 1));

  return (
    <div className="fixed inset-0 z-50 bg-black/95 flex flex-col justify-between p-4 sm:p-6 animate-in fade-in">
      {/* Top Bar */}
      <div className="flex justify-between items-center text-white z-10">
        <span className="text-sm font-semibold tracking-wider">
          Fotografía {currentIndex + 1} de {images.length}
        </span>
        <button
          onClick={onClose}
          className="p-2 rounded-full bg-white/10 hover:bg-white/20 transition-colors"
        >
          <X className="w-6 h-6" />
        </button>
      </div>

      {/* Main Image & Navigation Arrows */}
      <div className="relative flex-1 flex items-center justify-center overflow-hidden my-4">
        {images.length > 1 && (
          <button
            onClick={prev}
            className="absolute left-2 sm:left-6 p-3 rounded-full bg-black/50 text-white hover:bg-brand-600 transition-all z-10"
          >
            <ChevronLeft className="w-6 h-6" />
          </button>
        )}

        <img
          src={images[currentIndex].imageUrl}
          alt={`Foto ${currentIndex + 1}`}
          className="max-h-full max-w-full object-contain rounded-2xl shadow-2xl transition-all duration-300"
        />

        {images.length > 1 && (
          <button
            onClick={next}
            className="absolute right-2 sm:right-6 p-3 rounded-full bg-black/50 text-white hover:bg-brand-600 transition-all z-10"
          >
            <ChevronRight className="w-6 h-6" />
          </button>
        )}
      </div>

      {/* Thumbnails Row */}
      {images.length > 1 && (
        <div className="flex items-center justify-center gap-2 overflow-x-auto py-2 custom-scrollbar">
          {images.map((img, i) => (
            <button
              key={img.id || i}
              onClick={() => setCurrentIndex(i)}
              className={`w-16 h-12 rounded-lg overflow-hidden border-2 transition-all shrink-0 ${
                i === currentIndex ? 'border-brand-500 scale-105' : 'border-transparent opacity-50 hover:opacity-100'
              }`}
            >
              <img src={img.imageUrl} alt="" className="w-full h-full object-cover" />
            </button>
          ))}
        </div>
      )}
    </div>
  );
};
