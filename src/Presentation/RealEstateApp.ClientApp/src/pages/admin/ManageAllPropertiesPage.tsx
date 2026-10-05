import React, { useState, useEffect } from 'react';
import { propertiesService, adminService } from '../../api/services';
import { Property, User } from '../../types';
import { formatCurrencyRD, getApiErrorMessage } from '../../utils/formatters';
import { Badge } from '../../components/common/Badge';
import { Loader } from '../../components/common/Loader';
import { Building2, UserCheck, Trash2, Sparkles, Search, ExternalLink } from 'lucide-react';
import { Link } from 'react-router-dom';
import { useLanguage } from '../../context/LanguageContext';

export const ManageAllPropertiesPage: React.FC = () => {
  const { t } = useLanguage();
  const [properties, setProperties] = useState<Property[]>([]);
  const [agents, setAgents] = useState<User[]>([]);
  const [selectedProperty, setSelectedProperty] = useState<Property | null>(null);
  const [targetAgentId, setTargetAgentId] = useState('');
  const [showReassignModal, setShowReassignModal] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [isProcessing, setIsProcessing] = useState(false);

  const loadData = async () => {
    try {
      const [propsData, agentsData] = await Promise.all([
        propertiesService.getAll(),
        adminService.getAgents(),
      ]);
      setProperties(propsData || []);
      setAgents(agentsData || []);
    } catch (err: unknown) {
      console.error("Error loading all properties:", getApiErrorMessage(err));
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    Promise.resolve().then(() => loadData()).catch(console.error);
  }, []);

  const handleToggleFeatured = async (id: number) => {
    try {
      await propertiesService.toggleFeatured(id);
      await loadData();
    } catch (err) {
      console.error("Error toggling featured:", err);
    }
  };

  const handleReassignConfirm = async () => {
    if (!selectedProperty || !targetAgentId) return;
    try {
      setIsProcessing(true);
      await propertiesService.reassign(selectedProperty.id, targetAgentId);
      setShowReassignModal(false);
      setSelectedProperty(null);
      await loadData();
    } catch (err) {
      console.error("Error reassigning property:", err);
    } finally {
      setIsProcessing(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm(t('admin.allProps.deleteConfirm', "¿Seguro que deseas eliminar esta propiedad del sistema de forma permanente?"))) return;
    try {
      await propertiesService.delete(id);
      await loadData();
    } catch (err) {
      console.error("Error deleting property:", err);
    }
  };

  if (isLoading) return <Loader text={t('admin.allProps.loading', 'Cargando catálogo global de inmuebles...')} />;

  const filtered = properties.filter((p) => {
    const term = searchTerm.toLowerCase();
    return (
      (p.code && p.code.toLowerCase().includes(term)) ||
      (p.propertyTypeName && p.propertyTypeName.toLowerCase().includes(term)) ||
      (p.agentName && p.agentName.toLowerCase().includes(term)) ||
      (p.sector && p.sector.toLowerCase().includes(term))
    );
  });

  return (
    <div className="space-y-8 pb-16">
      
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-4 border-b border-slate-200 dark:border-slate-800">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 dark:text-white tracking-tight flex items-center gap-2">
            <Building2 className="w-7 h-7 text-brand-600 dark:text-brand-400" />
            {t('admin.allProps.title', 'Catálogo Global de Inmuebles')}
          </h2>
          <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">
            {t('admin.allProps.subtitle', 'Supervisión integral de todas las propiedades publicadas, reasignación de carteras y destacados.')}
          </p>
        </div>

        <div className="relative w-full sm:w-72">
          <Search className="w-4 h-4 text-slate-400 dark:text-slate-500 absolute left-3 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            placeholder={t('admin.allProps.searchPlaceholder', 'Buscar por código, tipo, agente...')}
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-9 pr-4 py-2 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-xl text-xs text-slate-900 dark:text-white focus:ring-2 focus:ring-brand-500"
          />
        </div>
      </div>

      {/* Table */}
      <div className="bg-white dark:bg-slate-900 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead className="bg-slate-50 dark:bg-slate-800/70 border-b border-slate-200 dark:border-slate-800 text-slate-500 dark:text-slate-400 font-extrabold uppercase tracking-wider">
              <tr>
                <th className="px-6 py-4">{t('admin.allProps.colCodeType', 'Código / Tipo')}</th>
                <th className="px-6 py-4">{t('admin.allProps.colPrice', 'Precio (RD$)')}</th>
                <th className="px-6 py-4">{t('admin.allProps.colAgent', 'Agente Asignado')}</th>
                <th className="px-6 py-4">{t('admin.allProps.colStatus', 'Estado')}</th>
                <th className="px-6 py-4">{t('admin.allProps.colFeatured', 'Destacado')}</th>
                <th className="px-6 py-4 text-right">{t('admin.allProps.colActions', 'Acciones')}</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 dark:divide-slate-800">
              {filtered.map((prop) => (
                <tr key={prop.id} className="hover:bg-slate-50/50 dark:hover:bg-slate-800/40">
                  <td className="px-6 py-4">
                    <span className="font-mono font-bold text-slate-500 dark:text-slate-400">#{prop.code}</span>
                    <p className="font-bold text-slate-900 dark:text-white">{prop.propertyTypeName} ({prop.saleTypeName})</p>
                    <p className="text-[11px] text-slate-400 dark:text-slate-500">{prop.sector}</p>
                  </td>
                  <td className="px-6 py-4 font-mono font-extrabold text-slate-900 dark:text-white">
                    {formatCurrencyRD(prop.price)}
                  </td>
                  <td className="px-6 py-4">
                    <p className="font-bold text-slate-800 dark:text-slate-200">{prop.agentName || t('admin.allProps.noAgent', 'Sin Agente')}</p>
                    <p className="text-[11px] text-slate-400 dark:text-slate-500">{prop.agentPhone}</p>
                  </td>
                  <td className="px-6 py-4">
                    <Badge status={prop.status} />
                  </td>
                  <td className="px-6 py-4">
                    <button
                      onClick={() => handleToggleFeatured(prop.id)}
                      className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-bold transition-all ${
                        prop.isFeatured
                          ? 'bg-amber-500 text-navy-950 shadow-xs'
                          : 'bg-slate-100 dark:bg-slate-800 hover:bg-slate-200 dark:hover:bg-slate-700 text-slate-600 dark:text-slate-300'
                      }`}
                    >
                      <Sparkles className="w-3 h-3" />
                      <span>{prop.isFeatured ? t('admin.allProps.featured', 'Destacado') : t('admin.allProps.standard', 'Estándar')}</span>
                    </button>
                  </td>
                  <td className="px-6 py-4 text-right space-x-2">
                    <Link
                      to={`/property/${prop.id}`}
                      className="p-1.5 inline-block text-brand-600 dark:text-brand-400 hover:bg-brand-50 dark:hover:bg-brand-950/40 rounded-lg"
                      title={t('admin.allProps.viewPublic', 'Ver Ficha Pública')}
                    >
                      <ExternalLink className="w-4 h-4" />
                    </Link>
                    <button
                      onClick={() => {
                        setSelectedProperty(prop);
                        setTargetAgentId(prop.agentId);
                        setShowReassignModal(true);
                      }}
                      className="p-1.5 text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-lg"
                      title={t('admin.allProps.reassignAgent', 'Reasignar Agente')}
                    >
                      <UserCheck className="w-4 h-4" />
                    </button>
                    <button
                      onClick={() => handleDelete(prop.id)}
                      className="p-1.5 text-rose-500 hover:bg-rose-50 dark:hover:bg-rose-950/40 rounded-lg"
                      title={t('admin.allProps.deleteProperty', 'Eliminar Propiedad')}
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Reassign Modal */}
      {showReassignModal && selectedProperty && (
        <div className="fixed inset-0 z-50 bg-black/60 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white dark:bg-slate-900 rounded-3xl p-6 sm:p-8 max-w-md w-full shadow-2xl space-y-4 border border-transparent dark:border-slate-800">
            <h3 className="font-extrabold text-base text-slate-900 dark:text-white flex items-center gap-2">
              <UserCheck className="w-5 h-5 text-brand-600 dark:text-brand-400" />
              {t('admin.allProps.modalTitle', 'Reasignar Propiedad #')}{selectedProperty.code}
            </h3>
            <p className="text-xs text-slate-500 dark:text-slate-400">
              {t('admin.allProps.modalDesc', 'Selecciona el agente receptor para transferir la gestión de esta propiedad.')}
            </p>

            <div>
              <label className="block text-xs font-bold text-slate-700 dark:text-slate-300 uppercase mb-1">
                {t('admin.allProps.modalSelectLabel', 'Nuevo Agente *')}
              </label>
              <select
                value={targetAgentId}
                onChange={(e) => setTargetAgentId(e.target.value)}
                className="w-full p-2.5 text-xs rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-900 dark:text-white"
              >
                <option value="">{t('admin.allProps.modalSelectPlaceholder', 'Selecciona un agente...')}</option>
                {agents.filter((a) => a.isActive).map((a) => (
                  <option key={a.id} value={a.id}>
                    {a.fullName} ({a.email})
                  </option>
                ))}
              </select>
            </div>

            <div className="flex gap-2 justify-end pt-2">
              <button
                onClick={() => setShowReassignModal(false)}
                className="px-4 py-2 bg-slate-100 dark:bg-slate-800 text-slate-700 dark:text-slate-300 hover:bg-slate-200 dark:hover:bg-slate-700 rounded-xl text-xs font-bold"
              >
                {t('admin.common.cancel', 'Cancelar')}
              </button>
              <button
                onClick={handleReassignConfirm}
                disabled={isProcessing || !targetAgentId}
                className="px-5 py-2 bg-brand-600 hover:bg-brand-700 text-white rounded-xl text-xs font-bold disabled:opacity-50"
              >
                {isProcessing ? t('admin.allProps.modalTransferring', 'Reasignando...') : t('admin.allProps.modalConfirm', 'Confirmar Transferencia')}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
