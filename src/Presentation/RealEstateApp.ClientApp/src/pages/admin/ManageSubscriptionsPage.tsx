import React, { useState, useEffect } from 'react';
import { subscriptionsService } from '../../api/services';
import { SubscriptionPlan, SubscriptionPlanInput } from '../../types';
import { formatCurrencyRD, getApiErrorMessage } from '../../utils/formatters';
import { Loader } from '../../components/common/Loader';
import {
  Award,
  PlusCircle,
  Save,
  X,
  Pencil,
  Trash2,
  Power,
  CheckCircle2,
  XCircle,
  Percent,
  Home,
  Star,
  Video,
  Box,
  RefreshCw,
} from 'lucide-react';
import { useLanguage } from '../../context/LanguageContext';

const emptyForm: SubscriptionPlanInput = {
  name: '',
  description: '',
  monthlyPrice: 0,
  maxActiveProperties: 3,
  maxFeaturedProperties: 0,
  allows3DTours: true,
  allowsVideo: true,
  commissionPercentage: 5,
  isActive: true,
};

export const ManageSubscriptionsPage: React.FC = () => {
  const { t } = useLanguage();
  const [plans, setPlans] = useState<SubscriptionPlan[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const [editingPlanId, setEditingPlanId] = useState<number | null>(null);
  const [editingValue, setEditingValue] = useState<string>('');
  const [isSaving, setIsSaving] = useState(false);

  const [editorOpen, setEditorOpen] = useState(false);
  const [editingPlan, setEditingPlan] = useState<SubscriptionPlan | null>(null);
  const [form, setForm] = useState<SubscriptionPlanInput>(emptyForm);

  const loadPlans = async () => {
    try {
      setIsLoading(true);
      const data = await subscriptionsService.getAdminPlans();
      setPlans(data || []);
    } catch (err) {
      console.error("Error loading subscription plans:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    Promise.resolve()
      .then(() => loadPlans())
      .catch(console.error);
  }, []);

  const openCreate = () => {
    setEditingPlan(null);
    setForm(emptyForm);
    setEditorOpen(true);
  };

  const openEdit = (plan: SubscriptionPlan) => {
    setEditingPlan(plan);
    setForm({
      name: plan.name,
      description: plan.description || '',
      monthlyPrice: plan.monthlyPrice,
      maxActiveProperties: plan.maxActiveProperties ?? 3,
      maxFeaturedProperties: plan.maxFeaturedProperties,
      allows3DTours: plan.allows3DTours ?? true,
      allowsVideo: plan.allowsVideo ?? true,
      commissionPercentage: plan.commissionPercentage ?? 5,
      isActive: plan.isActive ?? true,
    });
    setEditorOpen(true);
  };

  const handleField = (field: keyof SubscriptionPlanInput, value: string | number | boolean) => {
    setForm((prev) => ({ ...prev, [field]: value }));
  };

  const savePlan = async () => {
    if (!form.name.trim()) {
      alert(t('admin.subs.nameRequired', "El nombre del plan es obligatorio."));
      return;
    }
    if (form.monthlyPrice < 0) {
      alert(t('admin.subs.negativePrice', "El precio mensual no puede ser negativo."));
      return;
    }
    try {
      setIsSaving(true);
      if (editingPlan) {
        await subscriptionsService.updatePlan(editingPlan.id, form);
      } else {
        await subscriptionsService.createPlan({
          ...form,
          name: form.name.trim(),
        });
      }
      setEditorOpen(false);
      await loadPlans();
    } catch (err) {
      console.error("Error saving subscription plan:", err);
      alert(t('admin.subs.errorSaving', "Error al guardar el plan de suscripción."));
    } finally {
      setIsSaving(false);
    }
  };

  const getPlanDisplayName = (plan: SubscriptionPlan) => {
    const lower = plan.name.toLowerCase();
    if (lower.includes('gratuito') || lower.includes('starter')) {
      return t('admin.subs.planStarter', plan.name);
    }
    if (lower.includes('profesional') || lower.includes('pro')) {
      return t('admin.subs.planPro', plan.name);
    }
    if (lower.includes('inmobiliaria') || lower.includes('premium') || lower.includes('brokerage')) {
      return t('admin.subs.planAgency', plan.name);
    }
    return plan.name;
  };

  const getPlanDisplayDesc = (plan: SubscriptionPlan) => {
    if (!plan.description) return '';
    const desc = plan.description;
    if (desc.includes('comenzando') || desc.includes('esporádica')) {
      return t('admin.subs.planStarterDesc', desc);
    }
    if (desc.includes('agentes activos') || desc.includes('mayor visibilidad')) {
      return t('admin.subs.planProDesc', desc);
    }
    if (desc.includes('firmas inmobiliarias') || desc.includes('top producers')) {
      return t('admin.subs.planAgencyDesc', desc);
    }
    return desc;
  };

  const toggleActive = async (plan: SubscriptionPlan) => {
    try {
      setIsSaving(true);
      await subscriptionsService.setPlanActive(plan.id, !plan.isActive);
      await loadPlans();
    } catch (err) {
      console.error("Error toggling plan:", err);
      alert(t('admin.subs.errorToggling', "Error al cambiar el estado del plan."));
    } finally {
      setIsSaving(false);
    }
  };

  const deletePlan = async (plan: SubscriptionPlan) => {
    if (!confirm(t('admin.subs.deletePlanConfirm', `¿Eliminar el plan "{0}"? Esta acción no se puede deshacer.`).replace('{0}', getPlanDisplayName(plan)))) return;
    try {
      setIsSaving(true);
      await subscriptionsService.deletePlan(plan.id);
      await loadPlans();
    } catch (err: unknown) {
      console.error("Error deleting plan:", err);
      alert(getApiErrorMessage(err, t('admin.subs.errorDeleting', "No se pudo eliminar el plan.")));
    } finally {
      setIsSaving(false);
    }
  };

  const startEditCommission = (plan: SubscriptionPlan) => {
    setEditingPlanId(plan.id);
    setEditingValue(plan.commissionPercentage?.toString() ?? '5');
  };

  const saveCommission = async (planId: number) => {
    const value = parseFloat(editingValue);
    if (isNaN(value) || value < 0 || value > 100) {
      alert(t('admin.subs.valCommission', "Ingresa un porcentaje entre 0 y 100."));
      return;
    }
    try {
      setIsSaving(true);
      await subscriptionsService.updatePlanCommission(planId, value);
      setEditingPlanId(null);
      await loadPlans();
    } catch (err) {
      console.error("Error updating commission:", err);
      alert(t('admin.subs.errorUpdatingCommission', "Error al actualizar el porcentaje de comisión."));
    } finally {
      setIsSaving(false);
    }
  };

  if (isLoading) return <Loader text={t('admin.subs.loading', "Cargando planes de suscripción...")} />;

  const activeCount = plans.filter((p) => p.isActive).length;

  return (
    <div className="space-y-8 pb-16">

      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-4 border-b border-slate-200 dark:border-slate-800">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 dark:text-white tracking-tight flex items-center gap-2">
            <Award className="w-7 h-7 text-brand-600 dark:text-brand-400" />
            {t('admin.subs.title', "Membresías y Planes de Agentes")}
          </h2>
          <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">
            {t('admin.subs.subtitle', "Configuración de tarifas, límites de inmuebles destacados y beneficios para corredores asociados.")}
          </p>
        </div>

        <button
          onClick={openCreate}
          className="inline-flex items-center gap-2 px-4 py-2.5 bg-brand-600 hover:bg-brand-700 text-white font-extrabold text-xs rounded-xl shadow-md transition-all"
        >
          <PlusCircle className="w-4 h-4" />
          {t('admin.subs.newPlan', "Nuevo Plan")}
        </button>
      </div>

      {/* KPI summary */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-6">
        <div className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-xs space-y-1">
          <span className="text-xs font-bold text-slate-400 dark:text-slate-500 uppercase">{t('admin.subs.activePlans', "Planes Activos")}</span>
          <p className="text-2xl font-extrabold font-mono text-emerald-600 dark:text-emerald-400">{activeCount} / {plans.length}</p>
        </div>
        <div className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-xs space-y-1">
          <span className="text-xs font-bold text-slate-400 dark:text-slate-500 uppercase">{t('admin.subs.revenueModel', "Modelo de Ingresos")}</span>
          <p className="text-2xl font-extrabold font-mono text-emerald-600 dark:text-emerald-400">{t('admin.subs.saasRecurring', "SaaS Recurrente")}</p>
        </div>
        <div className="bg-white dark:bg-slate-900 p-6 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-xs space-y-1">
          <span className="text-xs font-bold text-slate-400 dark:text-slate-500 uppercase">{t('admin.subs.baseCurrency', "Moneda Base")}</span>
          <p className="text-2xl font-extrabold font-mono text-brand-600 dark:text-brand-400">{t('admin.subs.rdCurrency', "Pesos Dominicanos (RD$)")}</p>
        </div>
      </div>

      {/* Plan Editor */}
      {editorOpen && (
        <div className="bg-white dark:bg-slate-900 rounded-3xl border-2 border-brand-200 dark:border-brand-800 shadow-lg p-6 space-y-5">
          <div className="flex items-center justify-between">
            <h3 className="font-extrabold text-base text-slate-900 dark:text-white">
              {editingPlan ? `${t('admin.subs.editPlan', 'Editar Plan')}: ${getPlanDisplayName(editingPlan)}` : t('admin.subs.newPlanModal', 'Nuevo Plan de Suscripción')}
            </h3>
            <button
              onClick={() => setEditorOpen(false)}
              className="p-1.5 text-slate-400 hover:text-slate-600 dark:hover:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-lg"
              title={t('admin.subs.close', "Cerrar")}
            >
              <X className="w-4 h-4" />
            </button>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            <div className="space-y-1 lg:col-span-2">
              <label className="text-[11px] font-bold text-slate-500 uppercase">{t('admin.subs.planName', "Nombre del Plan")} *</label>
              <input
                type="text"
                value={form.name}
                onChange={(e) => handleField('name', e.target.value)}
                placeholder={t('admin.subs.planNamePlaceholder', "Ej. Profesional (Pro)")}
                className="w-full rounded-lg border border-slate-300 bg-slate-50 px-3 py-2 text-sm font-medium text-slate-800 focus:outline-none focus:ring-1 focus:ring-brand-500"
              />
            </div>
            <div className="space-y-1">
              <label className="text-[11px] font-bold text-slate-500 uppercase">{t('admin.subs.monthlyPrice', "Precio Mensual (RD$)")} *</label>
              <input
                type="number"
                min={0}
                step={0.01}
                value={form.monthlyPrice}
                onChange={(e) => handleField('monthlyPrice', parseFloat(e.target.value) || 0)}
                className="w-full rounded-lg border border-slate-300 bg-slate-50 px-3 py-2 text-sm font-mono font-bold text-slate-800 focus:outline-none focus:ring-1 focus:ring-brand-500"
              />
            </div>
            <div className="space-y-1">
              <label className="text-[11px] font-bold text-slate-500 uppercase">{t('admin.subs.salesCommission', "Comisión por Venta (%)")}</label>
              <input
                type="number"
                min={0}
                max={100}
                step={0.25}
                value={form.commissionPercentage?.toString() ?? '5'}
                onChange={(e) => handleField('commissionPercentage', parseFloat(e.target.value) || 0)}
                className="w-full rounded-lg border border-slate-300 bg-slate-50 px-3 py-2 text-sm font-mono font-bold text-slate-800 focus:outline-none focus:ring-1 focus:ring-brand-500"
              />
            </div>
            <div className="space-y-1">
              <label className="text-[11px] font-bold text-slate-500 uppercase">{t('admin.subs.maxActiveProps', "Propiedades Activas Máx.")}</label>
              <input
                type="number"
                min={0}
                value={form.maxActiveProperties}
                onChange={(e) => handleField('maxActiveProperties', parseInt(e.target.value, 10) || 0)}
                className="w-full rounded-lg border border-slate-300 bg-slate-50 px-3 py-2 text-sm font-mono font-bold text-slate-800 focus:outline-none focus:ring-1 focus:ring-brand-500"
              />
            </div>
            <div className="space-y-1">
              <label className="text-[11px] font-bold text-slate-500 uppercase">{t('admin.subs.maxFeaturedProps', "Destacadas Máx.")}</label>
              <input
                type="number"
                min={0}
                value={form.maxFeaturedProperties}
                onChange={(e) => handleField('maxFeaturedProperties', parseInt(e.target.value, 10) || 0)}
                className="w-full rounded-lg border border-slate-300 bg-slate-50 px-3 py-2 text-sm font-mono font-bold text-slate-800 focus:outline-none focus:ring-1 focus:ring-brand-500"
              />
            </div>
            <div className="space-y-1 lg:col-span-3">
              <label className="text-[11px] font-bold text-slate-500 uppercase">{t('admin.subs.description', "Descripción")}</label>
              <textarea
                value={form.description}
                onChange={(e) => handleField('description', e.target.value)}
                placeholder={t('admin.subs.descPlaceholder', "Descripción de beneficios y alcance del plan...")}
                rows={2}
                className="w-full rounded-lg border border-slate-300 bg-slate-50 px-3 py-2 text-sm text-slate-700 focus:outline-none focus:ring-1 focus:ring-brand-500"
              />
            </div>
          </div>

          <div className="flex flex-wrap items-center gap-4">
            <label className="flex items-center gap-2 text-xs font-bold text-slate-700 cursor-pointer">
              <input
                type="checkbox"
                checked={form.allows3DTours}
                onChange={(e) => handleField('allows3DTours', e.target.checked)}
                className="accent-brand-600 w-4 h-4"
              />
              {t('admin.subs.tours3D', "Tours 3D")}
            </label>
            <label className="flex items-center gap-2 text-xs font-bold text-slate-700 cursor-pointer">
              <input
                type="checkbox"
                checked={form.allowsVideo}
                onChange={(e) => handleField('allowsVideo', e.target.checked)}
                className="accent-brand-600 w-4 h-4"
              />
              {t('admin.subs.video', "Videos")}
            </label>
            <label className="flex items-center gap-2 text-xs font-bold text-slate-700 cursor-pointer">
              <input
                type="checkbox"
                checked={form.isActive}
                onChange={(e) => handleField('isActive', e.target.checked)}
                className="accent-brand-600 w-4 h-4"
              />
              {t('admin.subs.planActive', "Plan activo (visible para agentes)")}
            </label>
            <div className="ml-auto flex items-center gap-2">
              <button
                onClick={() => setEditorOpen(false)}
                className="px-4 py-2 text-xs font-bold text-slate-500 hover:text-slate-700 hover:bg-slate-100 rounded-xl transition-colors"
              >
                {t('admin.subs.cancel', "Cancelar")}
              </button>
              <button
                onClick={savePlan}
                disabled={isSaving}
                className="inline-flex items-center gap-2 px-5 py-2 bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 text-white font-extrabold text-xs rounded-xl shadow-md transition-all"
              >
                <Save className="w-4 h-4" />
                {editingPlan ? t('admin.subs.saveChanges', 'Guardar Cambios') : t('admin.subs.createPlan', 'Crear Plan')}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Plans List Table */}
      <div className="bg-white dark:bg-slate-900 rounded-3xl border border-slate-200 dark:border-slate-800 shadow-xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead className="bg-slate-50 dark:bg-slate-800/70 border-b border-slate-200 dark:border-slate-800 text-slate-500 dark:text-slate-400 font-extrabold uppercase tracking-wider">
              <tr>
                <th className="px-6 py-4">{t('admin.subs.colName', "Nombre del Plan")}</th>
                <th className="px-6 py-4">{t('admin.subs.colDesc', "Descripción")}</th>
                <th className="px-6 py-4">{t('admin.subs.colPrice', "Precio Mensual")}</th>
                <th className="px-6 py-4">{t('admin.subs.colLimits', "Límites")}</th>
                <th className="px-6 py-4">{t('admin.subs.colCommission', "Comisión por Venta")}</th>
                <th className="px-6 py-4">{t('admin.subs.colBenefits', "Beneficios")}</th>
                <th className="px-6 py-4">{t('admin.subs.colStatus', "Estado")}</th>
                <th className="px-6 py-4 text-right">{t('admin.subs.colActions', "Acciones")}</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 dark:divide-slate-800">
              {plans.map((plan) => (
                <tr key={plan.id} className={`hover:bg-slate-50/50 dark:hover:bg-slate-800/40 ${!plan.isActive ? 'opacity-60' : ''}`}>
                  <td className="px-6 py-4 font-bold text-slate-900 dark:text-white text-sm">
                    {getPlanDisplayName(plan)}
                  </td>
                  <td className="px-6 py-4 text-slate-500 dark:text-slate-400 max-w-xs">
                    {getPlanDisplayDesc(plan)}
                  </td>
                  <td className="px-6 py-4 font-mono font-bold text-slate-900 dark:text-white text-sm">
                    {plan.monthlyPrice === 0 ? t('admin.subs.free', 'Gratuito') : formatCurrencyRD(plan.monthlyPrice)}
                  </td>
                  <td className="px-6 py-4 space-y-1">
                    <span className="flex items-center gap-1 font-bold text-slate-700 dark:text-slate-300">
                      <Home className="w-3 h-3 text-brand-600 dark:text-brand-400" />
                      {t('admin.subs.upToActive', 'Hasta {0} activas').replace('{0}', String(plan.maxActiveProperties ?? 3))}
                    </span>
                    <span className="flex items-center gap-1 font-bold text-amber-600 dark:text-amber-400">
                      <Star className="w-3 h-3" />
                      {t('admin.subs.featuredCount', '{0} destacadas').replace('{0}', String(plan.maxFeaturedProperties))}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    {editingPlanId === plan.id ? (
                      <div className="flex items-center gap-1.5">
                        <input
                          type="number"
                          min={0}
                          max={100}
                          step={0.25}
                          value={editingValue}
                          onChange={(e) => setEditingValue(e.target.value)}
                          className="w-20 rounded-lg border border-brand-300 bg-slate-50 px-2 py-1.5 text-xs font-mono font-bold text-slate-800 focus:outline-none focus:ring-1 focus:ring-brand-500"
                          autoFocus
                        />
                        <span className="text-xs font-bold text-slate-400">%</span>
                        <button
                          onClick={() => saveCommission(plan.id)}
                          disabled={isSaving}
                          className="p-1.5 bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 text-white rounded-lg transition-colors"
                          title={t('admin.subs.saveCommission', "Guardar comisión")}
                        >
                          <Save className="w-3.5 h-3.5" />
                        </button>
                      </div>
                    ) : (
                      <button
                        onClick={() => startEditCommission(plan)}
                        className="inline-flex items-center gap-1 font-mono font-extrabold text-brand-700 bg-brand-50 border border-brand-100 px-2.5 py-1 rounded-lg hover:ring-1 hover:ring-brand-400 transition-all"
                        title={t('admin.subs.editCommission', "Editar comisión")}
                      >
                        <Percent className="w-3 h-3" />
                        {(plan.commissionPercentage ?? 5.0).toFixed(2)}%
                      </button>
                    )}
                    <p className="text-[10px] text-slate-400 mt-1">
                      {t('admin.subs.appliesToNew', "Se aplica a cierres nuevos según el plan del agente.")}
                    </p>
                  </td>
                  <td className="px-6 py-4 space-y-1">
                    <span className="inline-flex items-center gap-1 text-[11px] font-bold text-emerald-700 bg-emerald-50 px-2 py-0.5 rounded-md">
                      <CheckCircle2 className="w-3 h-3" /> {t('admin.subs.kycBenefit', "Verificación KYC")}
                    </span>
                    {plan.allows3DTours && (
                      <span className="inline-flex items-center gap-1 text-[11px] font-bold text-brand-700 bg-brand-50 px-2 py-0.5 rounded-md">
                        <Box className="w-3 h-3" /> {t('admin.subs.tours3D', "Tours 3D")}
                      </span>
                    )}
                    {plan.allowsVideo && (
                      <span className="inline-flex items-center gap-1 text-[11px] font-bold text-royal-700 bg-indigo-50 px-2 py-0.5 rounded-md">
                        <Video className="w-3 h-3" /> {t('admin.subs.video', "Videos")}
                      </span>
                    )}
                  </td>
                  <td className="px-6 py-4">
                    {plan.isActive ? (
                      <span className="inline-flex items-center gap-1 text-[11px] font-bold text-emerald-700 bg-emerald-50 px-2 py-0.5 rounded-md">
                        <CheckCircle2 className="w-3 h-3" /> {t('admin.subs.active', "Activo")}
                      </span>
                    ) : (
                      <span className="inline-flex items-center gap-1 text-[11px] font-bold text-slate-500 bg-slate-100 px-2 py-0.5 rounded-md">
                        <XCircle className="w-3 h-3" /> {t('admin.subs.inactive', "Inactivo")}
                      </span>
                    )}
                  </td>
                  <td className="px-6 py-4">
                    <div className="flex items-center justify-end gap-1">
                      <button
                        onClick={() => openEdit(plan)}
                        className="p-2 text-slate-400 hover:text-brand-600 hover:bg-brand-50 rounded-lg transition-colors"
                        title={t('admin.subs.editPlan', "Editar plan")}
                      >
                        <Pencil className="w-4 h-4" />
                      </button>
                      <button
                        onClick={() => toggleActive(plan)}
                        disabled={isSaving}
                        className={`p-2 rounded-lg transition-colors disabled:opacity-50 ${
                          plan.isActive
                            ? 'text-slate-400 hover:text-amber-600 hover:bg-amber-50'
                            : 'text-emerald-600 hover:bg-emerald-50'
                        }`}
                        title={plan.isActive ? t('admin.subs.deactivatePlan', 'Desactivar plan') : t('admin.subs.activatePlan', 'Activar plan')}
                      >
                        <Power className="w-4 h-4" />
                      </button>
                      <button
                        onClick={() => deletePlan(plan)}
                        disabled={isSaving}
                        className="p-2 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg transition-colors disabled:opacity-50"
                        title={t('admin.subs.deletePlan', "Eliminar plan")}
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          {plans.length === 0 && !isLoading && (
            <div className="flex flex-col items-center gap-3 py-14 text-center">
              <RefreshCw className="w-8 h-8 text-slate-300" />
              <p className="text-xs text-slate-400">
                {t('admin.subs.empty', "No hay planes de suscripción registrados. Crea el primero con el botón \"Nuevo Plan\".")}
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};