import React, { useState, useEffect } from 'react';
import { reviewsService } from '../../api/services';
import { Review } from '../../types';
import { formatDate } from '../../utils/formatters';
import { StarRating } from '../../components/common/StarRating';
import { Loader } from '../../components/common/Loader';
import { Star } from 'lucide-react';

export const AdminReviewsPage: React.FC = () => {
  const [reviews, setReviews] = useState<Review[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const data = await reviewsService.getAll();
        setReviews(data || []);
      } catch (err) {
        console.error("Error loading reviews:", err);
      } finally {
        setIsLoading(false);
      }
    };
    load();
  }, []);

  const average = reviews.length
    ? reviews.reduce((acc, r) => acc + r.rating, 0) / reviews.length
    : 0;

  if (isLoading) return <Loader text="Cargando reseñas de agentes..." />;

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-4 border-b border-slate-200">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <Star className="w-6 h-6 text-amber-500" />
            Reseñas de Agentes
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Calificaciones y comentarios de clientes tras completar transacciones.
          </p>
        </div>
        <div className="flex items-center gap-2 bg-white rounded-2xl border border-slate-200 px-4 py-2.5 shadow-xs">
          <StarRating value={average} size="sm" showValue />
          <span className="text-xs font-bold text-slate-600">{reviews.length} reseñas</span>
        </div>
      </div>

      {reviews.length === 0 ? (
        <div className="bg-white rounded-3xl p-12 text-center border border-slate-200 space-y-2">
          <Star className="w-12 h-12 text-slate-300 mx-auto" />
          <h3 className="text-base font-bold text-slate-800">Aún no hay reseñas registradas</h3>
          <p className="text-xs text-slate-500 max-w-sm mx-auto">
            Cuando los clientes califiquen a sus agentes tras una compra, aparecerán aquí.
          </p>
        </div>
      ) : (
        <div className="bg-white rounded-3xl border border-slate-200 overflow-hidden shadow-xs">
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse text-xs">
              <thead>
                <tr className="bg-slate-50 text-slate-700 font-extrabold uppercase tracking-wider border-b border-slate-200">
                  <th className="py-3.5 px-4">Cliente</th>
                  <th className="py-3.5 px-4">Propiedad</th>
                  <th className="py-3.5 px-4">Calificación</th>
                  <th className="py-3.5 px-4">Comentario</th>
                  <th className="py-3.5 px-4">Fecha</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {reviews.map((r) => (
                  <tr key={r.id} className="hover:bg-slate-50/80 transition-colors">
                    <td className="py-3 px-4">
                      <span className="font-bold text-slate-800">{r.clientName || r.clientId}</span>
                      <p className="text-[10px] text-slate-400">{r.clientEmail}</p>
                    </td>
                    <td className="py-3 px-4">
                      <span className="font-mono font-bold text-slate-700">#{r.propertyCode || r.propertyId}</span>
                      <p className="text-[11px] text-slate-500 truncate max-w-xs">{r.propertyName}</p>
                    </td>
                    <td className="py-3 px-4">
                      <StarRating value={r.rating} size="sm" />
                    </td>
                    <td className="py-3 px-4 text-slate-600 max-w-md">{r.comment || '—'}</td>
                    <td className="py-3 px-4 text-slate-600 font-medium">{formatDate(r.created)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
};