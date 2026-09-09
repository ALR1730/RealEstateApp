import React from 'react';
import { Star } from 'lucide-react';

interface StarRatingProps {
  value: number;
  onChange?: (value: number) => void;
  size?: 'sm' | 'md' | 'lg';
  showValue?: boolean;
}

const sizeClass: Record<'sm' | 'md' | 'lg', string> = {
  sm: 'w-3.5 h-3.5',
  md: 'w-5 h-5',
  lg: 'w-7 h-7',
};

export const StarRating: React.FC<StarRatingProps> = ({ value, onChange, size = 'md', showValue = false }) => {
  const [hover, setHover] = React.useState(0);
  const isInteractive = Boolean(onChange);

  return (
    <div className="flex items-center gap-1">
      {[1, 2, 3, 4, 5].map((star) => {
        const filled = (hover || value) >= star;
        return (
          <button
            key={star}
            type="button"
            disabled={!isInteractive}
            onClick={() => onChange?.(star)}
            onMouseEnter={() => isInteractive && setHover(star)}
            onMouseLeave={() => isInteractive && setHover(0)}
            className={isInteractive ? 'cursor-pointer transition-transform hover:scale-110' : 'cursor-default'}
            aria-label={`${star} estrella${star > 1 ? 's' : ''}`}
          >
            <Star
              className={`${sizeClass[size]} ${filled ? 'fill-amber-400 text-amber-400' : 'text-slate-300'}`}
            />
          </button>
        );
      })}
      {showValue && (
        <span className="text-xs font-mono font-bold text-slate-600 ml-1">
          {value > 0 ? value.toFixed(1) : '—'}
        </span>
      )}
    </div>
  );
};