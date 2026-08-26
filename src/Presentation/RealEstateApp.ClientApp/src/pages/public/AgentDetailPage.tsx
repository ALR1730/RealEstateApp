import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { propertiesService, adminService } from '../../api/services';
import { Property } from '../../types';
import { PropertyCard } from '../../components/properties/PropertyCard';
import { Loader } from '../../components/common/Loader';
import { Phone, Mail, MessageSquare, ShieldCheck, ArrowLeft, Building2 } from 'lucide-react';

export const AgentDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [agent, setAgent] = useState<any | null>(null);
  const [properties, setProperties] = useState<Property[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const loadAgentDetails = async () => {
      if (!id) return;
      try {
        setIsLoading(true);
        const [agents, allProps] = await Promise.all([
          adminService.getAgents(),
          propertiesService.getAll({ agentId: id }),
        ]);

        const currentAgent = agents.find((a: any) => a.id === id);
        setAgent(currentAgent || { id, fullName: 'Agente Inmobiliario', email: '', phone: '' });

        const agentProps = allProps.filter((p) => p.agentId === id);
        setProperties(agentProps);
      } catch (err) {
        console.error("Error loading agent details:", err);
      } finally {
        setIsLoading(false);
      }
    };

    loadAgentDetails();
  }, [id]);

  if (isLoading) return <Loader text="Cargando perfil del agente y su portafolio..." />;
  if (!agent) return <div className="text-center py-20 text-slate-500 font-bold">Agente no encontrado.</div>;

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8">
      
      <Link
        to="/agents"
        className="inline-flex items-center gap-1.5 text-xs font-bold text-slate-500 hover:text-slate-900 transition-colors"
      >
        <ArrowLeft className="w-4 h-4" />
        <span>Volver al Directorio de Agentes</span>
      </Link>

      {/* Agent Card Header */}
      <div className="bg-white rounded-3xl p-6 sm:p-10 border border-slate-200 shadow-sm flex flex-col sm:flex-row items-center sm:items-start gap-6 sm:gap-8">
        <div className="w-28 h-28 sm:w-36 sm:h-36 rounded-3xl overflow-hidden bg-brand-50 border-2 border-brand-100 flex items-center justify-center shrink-0 shadow-inner">
          {agent.photoUrl ? (
            <img src={agent.photoUrl} alt={agent.fullName} className="w-full h-full object-cover" />
          ) : (
            <span className="text-3xl sm:text-4xl font-extrabold text-brand-600">
              {agent.fullName?.charAt(0) || 'A'}
            </span>
          )}
        </div>

        <div className="flex-1 text-center sm:text-left space-y-2">
          <div className="flex flex-wrap items-center justify-center sm:justify-start gap-2">
            <h1 className="text-2xl sm:text-3xl font-extrabold text-slate-900 tracking-tight">
              {agent.fullName}
            </h1>
            <span className="inline-flex items-center gap-1 px-2.5 py-0.5 bg-emerald-50 text-emerald-700 text-xs font-bold rounded-full border border-emerald-200">
              <ShieldCheck className="w-3.5 h-3.5" />
              <span>Verificado</span>
            </span>
          </div>

          <p className="text-xs text-slate-500 max-w-xl">
            Corredor inmobiliario profesional acreditado en RealEstateApp para la gestión y comercialización de propiedades de alta plusvalía.
          </p>

          <div className="flex flex-wrap items-center justify-center sm:justify-start gap-4 pt-3 text-xs text-slate-600 font-medium">
            {agent.phone && (
              <a
                href={`tel:${agent.phone}`}
                className="flex items-center gap-1.5 hover:text-brand-600 transition-colors"
              >
                <Phone className="w-4 h-4 text-brand-600" />
                <span>{agent.phone}</span>
              </a>
            )}
            {agent.email && (
              <a
                href={`mailto:${agent.email}`}
                className="flex items-center gap-1.5 hover:text-brand-600 transition-colors"
              >
                <Mail className="w-4 h-4 text-brand-600" />
                <span>{agent.email}</span>
              </a>
            )}
            <div className="flex items-center gap-1.5 text-brand-600 font-bold">
              <Building2 className="w-4 h-4" />
              <span>{properties.length} Propiedades Publicadas</span>
            </div>
          </div>
        </div>

        <div className="shrink-0 flex flex-col gap-2 w-full sm:w-auto">
          {agent.phone && (
            <a
              href={`https://wa.me/${agent.phone.replace(/[^0-9]/g, '')}`}
              target="_blank"
              rel="noreferrer"
              className="px-5 py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl font-bold text-xs shadow-md transition-all flex items-center justify-center gap-2"
            >
              <MessageSquare className="w-4 h-4" />
              <span>Chat de WhatsApp</span>
            </a>
          )}
        </div>
      </div>

      {/* Agent's Properties Portfolio */}
      <div className="space-y-6">
        <div className="flex items-center justify-between pb-3 border-b border-slate-200">
          <h2 className="text-xl font-extrabold text-slate-900 tracking-tight">
            Portafolio Inmobiliario de {agent.fullName}
          </h2>
          <span className="text-xs font-mono font-bold text-slate-500">
            {properties.length} inmuebles
          </span>
        </div>

        {properties.length === 0 ? (
          <div className="bg-white rounded-3xl p-12 text-center border border-slate-200">
            <Building2 className="w-12 h-12 text-slate-300 mx-auto mb-2" />
            <p className="text-xs font-bold text-slate-600">Este agente no tiene propiedades activas en este momento.</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
            {properties.map((prop) => (
              <PropertyCard key={prop.id} property={prop} />
            ))}
          </div>
        )}
      </div>
    </div>
  );
};
