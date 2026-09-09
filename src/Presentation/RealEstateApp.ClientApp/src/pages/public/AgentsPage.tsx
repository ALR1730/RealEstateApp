import React, { useState, useEffect } from 'react';
import { agentsService } from '../../api/services';
import { Loader } from '../../components/common/Loader';
import { Users, Phone, Mail, Home, Search, ShieldCheck } from 'lucide-react';

export const AgentsPage: React.FC = () => {
  const [agents, setAgents] = useState<any[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [search, setSearch] = useState<string>('');

  useEffect(() => {
    const fetchAgents = async () => {
      try {
        setIsLoading(true);
        const data = await agentsService.getPublicAgents();
        setAgents(data || []);
      } catch (err) {
        console.error("Error loading agents:", err);
      } finally {
        setIsLoading(false);
      }
    };
    fetchAgents();
  }, []);

  const filtered = agents.filter((a) => {
    const term = search.toLowerCase();
    const fullName = `${a.firstName || ''} ${a.lastName || ''}`.toLowerCase();
    return fullName.includes(term) || (a.email && a.email.toLowerCase().includes(term));
  });

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8 pb-20">
      
      {/* Header */}
      <div className="bg-navy-950 text-white rounded-3xl p-8 sm:p-12 relative overflow-hidden shadow-xl">
        <div className="relative z-10 max-w-2xl space-y-3">
          <span className="text-brand-400 font-extrabold text-xs uppercase tracking-widest">
            Fuerza de Ventas Certificada
          </span>
          <h1 className="text-2xl sm:text-4xl font-extrabold tracking-tight">
            Directorio Oficial de Agentes Inmobiliarios
          </h1>
          <p className="text-xs sm:text-sm text-slate-300">
            Conoce a nuestros asesores certificados en el mercado inmobiliario de República Dominicana.
          </p>
        </div>
      </div>

      {/* Search Bar */}
      <div className="max-w-md">
        <div className="relative">
          <Search className="w-4 h-4 absolute left-3 top-3 text-slate-400" />
          <input
            type="text"
            placeholder="Buscar agente por nombre o correo..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-9 pr-4 py-2.5 bg-white border border-slate-200 rounded-2xl text-xs sm:text-sm focus:outline-none focus:ring-2 focus:ring-brand-500 shadow-2xs"
          />
        </div>
      </div>

      {/* Grid of Agents */}
      {isLoading ? (
        <Loader text="Consultando directorio de agentes..." />
      ) : filtered.length === 0 ? (
        <div className="p-12 text-center bg-white rounded-3xl border border-slate-200 text-slate-500">
          No se encontraron agentes que coincidan con la búsqueda.
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
          {filtered.map((agent) => (
            <div
              key={agent.id}
              className="bg-white rounded-3xl p-6 border border-slate-200 shadow-xs hover:shadow-xl hover:border-slate-300 transition-all flex flex-col justify-between space-y-4 text-center"
            >
              <div className="space-y-3">
                <div className="w-20 h-20 mx-auto rounded-full bg-gradient-to-tr from-navy-800 to-slate-700 text-white flex items-center justify-center font-bold text-xl overflow-hidden shadow-md">
                  {agent.photoUrl ? (
                    <img src={agent.photoUrl} alt="" className="w-full h-full object-cover" />
                  ) : (
                    agent.firstName ? agent.firstName[0] : 'A'
                  )}
                </div>

                <div>
                  <h3 className="font-bold text-base text-slate-900">
                    {agent.firstName} {agent.lastName}
                  </h3>
                  <p className="text-xs text-slate-500 truncate">{agent.email}</p>
                </div>

                <div className="pt-2 border-t border-slate-100 space-y-1 text-xs text-slate-600">
                  {agent.phone && (
                    <div className="flex items-center justify-center gap-1.5 font-semibold text-brand-600">
                      <Phone className="w-3.5 h-3.5" />
                      <span>{agent.phone}</span>
                    </div>
                  )}
                  <div className="flex items-center justify-center gap-1.5 font-semibold text-slate-500">
                    <Home className="w-3.5 h-3.5" />
                    <span>{agent.propertiesCount || 0} Propiedades asignadas</span>
                  </div>
                </div>
              </div>

              <div className="pt-2">
                <span className="inline-flex items-center gap-1 text-[10px] font-extrabold uppercase px-3 py-1 rounded-full bg-emerald-50 text-emerald-700 border border-emerald-200/60">
                  <ShieldCheck className="w-3 h-3" />
                  Corredor Autorizado
                </span>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
