import React from 'react';
import { Link } from 'react-router-dom';
import { useCompare } from '../../context/CompareContext';
import { formatCurrencyRD } from '../../utils/formatters';
import { Loader } from '../../components/common/Loader';
import {
  BarChart3,
  Bed,
  Bath,
  Maximize2,
  MapPin,
  User,
  Image as ImageIcon,
  ArrowLeft,
} from 'lucide-react';

export const ComparePage: React.FC = () => {
  const { compareProperties, removeFromCompare, clearCompare } = useCompare();

  if (compareProperties.length === 0) {
    return (
      <div className="min-h-[60vh] flex flex-col items-center justify-center text-center px-4 py-20 space-y-4">
        <BarChart3 className="w-12 h-12 text-slate-300" />
        <h2 className="text-xl font-extrabold text-slate-900">
          No hay propiedades para comparar
        </h2>
        <p className="text-xs text-slate-500 max-w-sm">
          Agrega propiedades desde el catálogo para iniciar una comparación lado a lado.
        </p>
        <Link
          to="/catalog"
          className="inline-block px-5 py-2.5 bg-brand-600 hover:bg-brand-700 text-white font-bold text-xs rounded-xl shadow-md"
        >
          Explorar Catálogo
        </Link>
      </div>
    );
  }

  const rows: { label: string; render: (p: (typeof compareProperties)[0]) => React.ReactNode }[] = [
    {
      label: 'Imagen',
      render: (p) => {
        const img =
          p.images && p.images.length > 0
            ? p.images[0].imageUrl
            : 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?auto=format&fit=crop&w=400&q=80';
        return (
          <img
            src={img}
            alt={p.description || `Propiedad ${p.code}`}
            className="w-full h-32 sm:h-40 object-cover rounded-2xl"
          />
        );
      },
    },
    {
      label: 'Precio',
      render: (p) => (
        <span className="text-sm font-extrabold text-slate-900">
          {formatCurrencyRD(p.price)}
        </span>
      ),
    },
    {
      label: 'Tipo',
      render: (p) => <span className="text-xs font-semibold text-slate-700">{p.propertyTypeName || '-'}</span>,
    },
    {
      label: 'Tipo de Venta',
      render: (p) => <span className="text-xs font-semibold text-slate-700">{p.saleTypeName || '-'}</span>,
    },
    {
      label: 'Habitaciones',
      render: (p) => (
        <div className="flex items-center justify-center gap-1">
          <Bed className="w-4 h-4 text-slate-400" />
          <span className="text-xs font-bold text-slate-700">{p.bedrooms}</span>
        </div>
      ),
    },
    {
      label: 'Baños',
      render: (p) => (
        <div className="flex items-center justify-center gap-1">
          <Bath className="w-4 h-4 text-slate-400" />
          <span className="text-xs font-bold text-slate-700">{p.bathrooms}</span>
        </div>
      ),
    },
    {
      label: 'Tamaño',
      render: (p) => (
        <div className="flex items-center justify-center gap-1">
          <Maximize2 className="w-4 h-4 text-slate-400" />
          <span className="text-xs font-bold text-slate-700">{p.landSizeMeters} m²</span>
        </div>
      ),
    },
    {
      label: 'Provincia',
      render: (p) => <span className="text-xs font-semibold text-slate-700">{p.provinceName || '-'}</span>,
    },
    {
      label: 'Sector',
      render: (p) => <span className="text-xs font-semibold text-slate-700">{p.sector || '-'}</span>,
    },
    {
      label: 'Agente',
      render: (p) => (
        <div className="flex items-center justify-center gap-1.5">
          {p.agentPhotoUrl ? (
            <img src={p.agentPhotoUrl} alt="Agente" className="w-5 h-5 rounded-full object-cover" />
          ) : (
            <User className="w-4 h-4 text-slate-400" />
          )}
          <span className="text-xs font-semibold text-slate-700 truncate max-w-[80px]">
            {p.agentName || '-'}
          </span>
        </div>
      ),
    },
    {
      label: 'Estado',
      render: (p) => (
        <span className={`inline-flex items-center gap-1 text-[11px] font-bold uppercase tracking-wide px-2 py-0.5 rounded-full border ${
          p.status === 'Available'
            ? 'bg-emerald-50 text-emerald-700 border-emerald-200/60'
            : p.status === 'Reserved'
            ? 'bg-amber-50 text-amber-700 border-amber-200/60'
            : 'bg-rose-50 text-rose-700 border-rose-200/60'
        }`}>
          <span className="w-1.5 h-1.5 rounded-full bg-current opacity-75" />
          {p.status === 'Available' ? 'Disponible' : p.status === 'Reserved' ? 'Reservada' : 'Vendida'}
        </span>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center pb-4 border-b border-slate-200">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <BarChart3 className="w-6 h-6 text-brand-600" />
            Comparar Propiedades
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Compara hasta 4 propiedades lado a lado para tomar la mejor decisión.
          </p>
        </div>
        <div className="flex gap-2">
          <Link
            to="/catalog"
            className="flex items-center gap-1 px-4 py-2 bg-white border border-slate-200 hover:bg-slate-50 text-slate-700 font-bold text-xs rounded-xl"
          >
            <ArrowLeft className="w-3.5 h-3.5" />
            Catálogo
          </Link>
          <button
            onClick={clearCompare}
            className="px-4 py-2 bg-rose-50 hover:bg-rose-100 text-rose-700 font-bold text-xs rounded-xl border border-rose-200"
          >
            Limpiar
          </button>
        </div>
      </div>

      {/* Comparison Table */}
      <div className="bg-white rounded-3xl border border-slate-200 shadow-xs overflow-x-auto">
        <table className="w-full min-w-[640px]">
          <thead>
            <tr className="border-b border-slate-100">
              <th className="w-[140px] p-4 text-left text-[11px] font-bold uppercase tracking-wider text-slate-400">
                Característica
              </th>
              {compareProperties.map((p) => (
                <th key={p.id} className="p-4 text-center min-w-[180px]">
                  <div className="flex flex-col items-center gap-1">
                    <span className="text-xs font-bold text-slate-900 truncate max-w-[160px]">
                      #{p.code}
                    </span>
                    <button
                      onClick={() => removeFromCompare(p.id)}
                      className="text-[10px] font-semibold text-rose-500 hover:text-rose-700"
                    >
                      Quitar
                    </button>
                  </div>
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {rows.map((row, idx) => (
              <tr
                key={row.label}
                className={idx % 2 === 0 ? 'bg-slate-50/60' : 'bg-white'}
              >
                <td className="p-4 text-xs font-bold text-slate-600">{row.label}</td>
                {compareProperties.map((p) => (
                  <td key={p.id} className="p-4 text-center">
                    {row.render(p)}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};
