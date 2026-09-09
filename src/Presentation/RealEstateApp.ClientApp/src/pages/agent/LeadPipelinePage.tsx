import React, { useState, useEffect } from 'react';
import { Lead, LeadPipelineStats, CreateLeadPayload } from '../../types';
import { leadPipelineService } from '../../api/services';
import { formatCurrencyRD } from '../../utils/formatters';
import { Loader } from '../../components/common/Loader';
import {
  Kanban,
  Plus,
  Trash2,
  ChevronLeft,
  ChevronRight,
  Sparkles,
  Phone,
  Mail,
  Clock,
  TrendingUp,
} from 'lucide-react';

const PIPELINE_STAGES = ['Nuevo Lead', 'Contactado', 'Visita', 'Oferta', 'Cierre', 'Ganado', 'Perdido'];

const stageColors: Record<string, string> = {
  'Nuevo Lead': 'border-sky-300 bg-sky-50',
  'Contactado': 'border-indigo-300 bg-indigo-50',
  'Visita': 'border-amber-300 bg-amber-50',
  'Oferta': 'border-purple-300 bg-purple-50',
  'Cierre': 'border-brand-300 bg-brand-50',
  'Ganado': 'border-emerald-300 bg-emerald-50',
  'Perdido': 'border-rose-300 bg-rose-50',
};

const priorityColor: Record<string, string> = {
  'Baja': 'bg-slate-100 text-slate-600',
  'Normal': 'bg-sky-100 text-sky-700',
  'Alta': 'bg-amber-100 text-amber-700',
  'Urgente': 'bg-rose-100 text-rose-700',
};

const emptyForm: CreateLeadPayload = {
  leadName: '',
  leadEmail: '',
  leadPhone: '',
  notes: '',
  priority: 'Normal',
  source: 'Portal',
  estimatedBudget: undefined,
  budgetCurrency: 'DOP',
};

export const LeadPipelinePage: React.FC = () => {
  const [leads, setLeads] = useState<Lead[]>([]);
  const [stats, setStats] = useState<LeadPipelineStats | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isCreating, setIsCreating] = useState<boolean>(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [form, setForm] = useState<CreateLeadPayload>(emptyForm);
  const [error, setError] = useState<string>('');

  const loadData = async () => {
    try {
      const [leadsData, statsData] = await Promise.all([
        leadPipelineService.getAll().catch(() => [] as Lead[]),
        leadPipelineService.getStats().catch(() => null),
      ]);
      setLeads(leadsData);
      setStats(statsData);
    } catch (err) {
      console.error('Error cargando pipeline:', err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    Promise.resolve().then(() => loadData()).catch(console.error);
  }, []);

  const openCreate = () => {
    setEditingId(null);
    setForm(emptyForm);
    setIsCreating(true);
    setError('');
  };

  const openEdit = (lead: Lead) => {
    setEditingId(lead.id);
    setForm({
      leadName: lead.leadName,
      leadEmail: lead.leadEmail,
      leadPhone: lead.leadPhone,
      notes: lead.notes,
      priority: lead.priority,
      source: lead.source,
      estimatedBudget: lead.estimatedBudget,
      budgetCurrency: lead.budgetCurrency || 'DOP',
    });
    setIsCreating(true);
    setError('');
  };

  const handleSubmit = async () => {
    if (!form.leadName.trim()) {
      setError('El nombre del lead es obligatorio.');
      return;
    }
    try {
      if (editingId !== null) {
        await leadPipelineService.update(editingId, form);
      } else {
        await leadPipelineService.create(form);
      }
      setIsCreating(false);
      await loadData();
    } catch (err: unknown) {
      const message = err && typeof err === 'object' && 'response' in err
        ? (err as { response?: { data?: { error?: string } } }).response?.data?.error
        : undefined;
      setError(message || 'No se pudo guardar el lead.');
    }
  };

  const handleDelete = async (lead: Lead) => {
    if (!window.confirm(`¿Eliminar el lead "${lead.leadName}"? Esta acción no se puede deshacer.`)) return;
    try {
      await leadPipelineService.remove(lead.id);
      await loadData();
    } catch (err) {
      console.error('Error eliminando lead:', err);
    }
  };

  const handleMove = async (lead: Lead, direction: -1 | 1) => {
    const idx = PIPELINE_STAGES.indexOf(lead.stage);
    const next = PIPELINE_STAGES[idx + direction];
    if (!next) return;
    try {
      await leadPipelineService.moveToStage(lead.id, next);
      await loadData();
    } catch (err) {
      console.error('Error moviendo lead:', err);
    }
  };

  if (isLoading) {
    return <Loader text="Cargando tu pipeline de leads..." size="lg" />;
  }

  const renderCard = (lead: Lead) => {
    const idx = PIPELINE_STAGES.indexOf(lead.stage);
    return (
      <div
        key={lead.id}
        className="bg-white rounded-2xl border border-slate-200 shadow-xs p-4 space-y-2.5 cursor-grab hover:border-brand-300 hover:shadow-md transition-all"
      >
        <div className="flex items-start justify-between gap-2">
          <div className="space-y-1">
            <h4 className="font-extrabold text-slate-900 text-sm leading-tight">{lead.leadName}</h4>
            <span className={`inline-block px-2 py-0.5 rounded-full text-[10px] font-extrabold ${priorityColor[lead.priority] || priorityColor['Normal']}`}>
              {lead.priority}
            </span>
          </div>
          <div className="flex items-center gap-1">
            <button
              onClick={() => handleMove(lead, -1)}
              disabled={idx === 0}
              className="p-1 text-slate-400 hover:text-brand-600 disabled:opacity-30 rounded-lg transition-colors"
              title="Mover a etapa anterior"
            >
              <ChevronLeft className="w-4 h-4" />
            </button>
            <button
              onClick={() => handleMove(lead, 1)}
              disabled={idx === PIPELINE_STAGES.length - 1}
              className="p-1 text-slate-400 hover:text-brand-600 disabled:opacity-30 rounded-lg transition-colors"
              title="Mover a siguiente etapa"
            >
              <ChevronRight className="w-4 h-4" />
            </button>
          </div>
        </div>

        <div className="space-y-1 text-xs text-slate-600">
          {lead.leadPhone && (
            <p className="flex items-center gap-1.5"><Phone className="w-3.5 h-3.5 text-slate-400" /> {lead.leadPhone}</p>
          )}
          {lead.leadEmail && (
            <p className="flex items-center gap-1.5 truncate"><Mail className="w-3.5 h-3.5 text-slate-400" /> {lead.leadEmail}</p>
          )}
        </div>

        {lead.estimatedBudget != null && (
          <p className="text-[11px] font-bold text-slate-500">
            Presupuesto: <span className="font-mono font-extrabold text-slate-800">{formatCurrencyRD(Number(lead.estimatedBudget))}</span>
          </p>
        )}

        {lead.nextFollowUpDate && (
          <p className="flex items-center gap-1.5 text-[11px] text-amber-600 font-semibold">
            <Clock className="w-3.5 h-3.5" /> Seguimiento: {new Date(lead.nextFollowUpDate).toLocaleDateString('es-DO')}
          </p>
        )}

        {lead.source && (
          <p className="text-[10px] text-slate-400 uppercase font-bold">Fuente: {lead.source}</p>
        )}

        <div className="flex items-center justify-between pt-2 border-t border-slate-100">
          {editingId === lead.id ? (
            <button onClick={() => setIsCreating(false)} className="text-[11px] font-bold text-slate-500">Cancelar</button>
          ) : (
            <button
              onClick={() => openEdit(lead)}
              className="text-[11px] font-bold text-brand-600 hover:underline"
            >
              Editar
            </button>
          )}
          <button
            onClick={() => handleDelete(lead)}
            className="p-1.5 text-rose-500 hover:bg-rose-50 rounded-lg transition-colors"
            title="Eliminar lead"
          >
            <Trash2 className="w-4 h-4" />
          </button>
        </div>
      </div>
    );
  };

  return (
    <div className="space-y-8">
      {/* Header */}
      <div className="rounded-3xl bg-gradient-to-r from-navy-950 to-slate-900 p-8 text-white flex flex-wrap items-center justify-between gap-4 shadow-lg">
        <div className="space-y-1">
          <span className="text-[11px] font-extrabold uppercase tracking-widest text-brand-400 flex items-center gap-1.5">
            <Kanban className="w-4 h-4" /> F-04 · CRM
          </span>
          <h1 className="text-2xl font-extrabold">Pipeline de Leads (Kanban)</h1>
          <p className="text-xs text-slate-300 max-w-xl">
            Gestiona cada prospecto a través de las etapas del embudo de ventas hasta el cierre.
          </p>
        </div>
        <button
          onClick={openCreate}
          className="px-5 py-2.5 bg-emerald-600 hover:bg-emerald-500 text-white font-extrabold text-xs rounded-xl shadow-lg flex items-center gap-2 transition-all"
        >
          <Plus className="w-4 h-4" /> Nuevo Lead
        </button>
      </div>

      {/* Stats */}
      {stats && (
        <div className="grid grid-cols-2 sm:grid-cols-4 lg:grid-cols-8 gap-4">
          <StatCard label="Total" value={stats.totalLeads} color="text-slate-900" />
          <StatCard label="Nuevos" value={stats.newLeadCount} color="text-sky-600" />
          <StatCard label="Contactados" value={stats.contactedCount} color="text-indigo-600" />
          <StatCard label="Visitas" value={stats.visitCount} color="text-amber-600" />
          <StatCard label="Ofertas" value={stats.offerCount} color="text-purple-600" />
          <StatCard label="Cierres" value={stats.closingCount} color="text-brand-600" />
          <StatCard label="Ganados" value={stats.wonCount} color="text-emerald-600" />
          <StatCard label="Perdidos" value={stats.lostCount} color="text-rose-600" />
        </div>
      )}

      {/* Conversion + pipeline value */}
      {stats && (
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div className="bg-white p-5 rounded-3xl border border-slate-200 shadow-xs flex items-center gap-4">
            <div className="w-12 h-12 rounded-2xl bg-emerald-50 text-emerald-600 flex items-center justify-center">
              <TrendingUp className="w-6 h-6" />
            </div>
            <div>
              <p className="text-[11px] font-extrabold uppercase tracking-wider text-slate-400">Tasa de Conversión</p>
              <p className="text-xl font-extrabold text-slate-900">{stats.conversionRate.toFixed(1)}%</p>
            </div>
          </div>
          <div className="bg-white p-5 rounded-3xl border border-slate-200 shadow-xs flex items-center gap-4">
            <div className="w-12 h-12 rounded-2xl bg-brand-50 text-brand-600 flex items-center justify-center">
              <Sparkles className="w-6 h-6" />
            </div>
            <div>
              <p className="text-[11px] font-extrabold uppercase tracking-wider text-slate-400">Valor Total del Pipeline</p>
              <p className="text-xl font-extrabold font-mono text-slate-900">{formatCurrencyRD(stats.totalPipelineValue)}</p>
            </div>
          </div>
        </div>
      )}

      {/* Kanban board */}
      <div className="overflow-x-auto pb-4">
        <div className="grid grid-cols-7 gap-4 min-w-[1200px]">
          {PIPELINE_STAGES.map((stage) => {
            const stageLeads = leads.filter((l) => l.stage === stage);
            const isWonLoss = stage === 'Ganado' || stage === 'Perdido';
            return (
              <div key={stage} className={`rounded-3xl border p-3 space-y-3 ${stageColors[stage] || 'border-slate-200 bg-slate-50'}`}>
                <div className="flex items-center justify-between px-1">
                  <span className="text-[11px] font-extrabold uppercase tracking-wider text-slate-700">{stage}</span>
                  <span className="text-[11px] font-extrabold text-slate-500 bg-white/70 px-2 py-0.5 rounded-full">{stageLeads.length}</span>
                </div>
                <div className="space-y-3">
                  {stageLeads.map(renderCard)}
                  {stageLeads.length === 0 && (
                    <div className="text-center text-[11px] font-semibold text-slate-400 py-8 border border-dashed border-slate-300 rounded-2xl">
                      Sin leads {isWonLoss ? 'aquí' : 'en esta etapa'}
                    </div>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* Create / Edit modal */}
      {isCreating && (
        <div className="fixed inset-0 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-3xl border border-slate-200 shadow-2xl w-full max-w-lg p-6 space-y-5 animate-in fade-in zoom-in-95">
            <div className="flex items-center justify-between">
              <h2 className="font-extrabold text-lg text-slate-900">
                {editingId !== null ? 'Editar Lead' : 'Nuevo Lead'}
              </h2>
              <button onClick={() => setIsCreating(false)} className="p-2 text-slate-500 hover:bg-slate-100 rounded-lg">
                ✕
              </button>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div className="sm:col-span-2 space-y-1.5">
                <label className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500">Nombre *</label>
                <input
                  value={form.leadName}
                  onChange={(e) => setForm({ ...form, leadName: e.target.value })}
                  className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 text-sm font-semibold focus:outline-hidden focus:ring-2 focus:ring-brand-500"
                  placeholder="Nombre completo del prospecto"
                />
              </div>
              <div className="space-y-1.5">
                <label className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500">Teléfono</label>
                <input
                  value={form.leadPhone || ''}
                  onChange={(e) => setForm({ ...form, leadPhone: e.target.value })}
                  className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 text-sm font-semibold focus:outline-hidden focus:ring-2 focus:ring-brand-500"
                />
              </div>
              <div className="space-y-1.5">
                <label className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500">Email</label>
                <input
                  value={form.leadEmail || ''}
                  onChange={(e) => setForm({ ...form, leadEmail: e.target.value })}
                  className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 text-sm font-semibold focus:outline-hidden focus:ring-2 focus:ring-brand-500"
                />
              </div>
              <div className="space-y-1.5">
                <label className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500">Prioridad</label>
                <select
                  value={form.priority}
                  onChange={(e) => setForm({ ...form, priority: e.target.value })}
                  className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 text-sm font-semibold focus:outline-hidden focus:ring-2 focus:ring-brand-500"
                >
                  <option>Baja</option>
                  <option>Normal</option>
                  <option>Alta</option>
                  <option>Urgente</option>
                </select>
              </div>
              <div className="space-y-1.5">
                <label className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500">Fuente</label>
                <input
                  value={form.source || ''}
                  onChange={(e) => setForm({ ...form, source: e.target.value })}
                  className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 text-sm font-semibold focus:outline-hidden focus:ring-2 focus:ring-brand-500"
                  placeholder="Portal, Referido, Redes..."
                />
              </div>
              <div className="sm:col-span-2 space-y-1.5">
                <label className="text-[11px] font-extrabold uppercase tracking-wider text-slate-500">Notas</label>
                <textarea
                  value={form.notes || ''}
                  onChange={(e) => setForm({ ...form, notes: e.target.value })}
                  rows={3}
                  className="w-full px-3.5 py-2.5 rounded-xl border border-slate-300 text-sm font-semibold focus:outline-hidden focus:ring-2 focus:ring-brand-500"
                />
              </div>
            </div>

            {error && (
              <div className="p-3 bg-rose-50 border border-rose-200 rounded-xl text-xs font-semibold text-rose-700">
                {error}
              </div>
            )}

            <div className="flex justify-end gap-2 pt-2">
              <button
                onClick={() => setIsCreating(false)}
                className="px-4 py-2.5 bg-slate-100 hover:bg-slate-200 text-slate-700 font-extrabold text-xs rounded-xl transition-colors"
              >
                Cancelar
              </button>
              <button
                onClick={handleSubmit}
                className="px-5 py-2.5 bg-brand-600 hover:bg-brand-700 text-white font-extrabold text-xs rounded-xl shadow-lg shadow-brand-600/30 transition-all"
              >
                {editingId !== null ? 'Guardar Cambios' : 'Crear Lead'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

const StatCard: React.FC<{ label: string; value: number; color: string }> = ({ label, value, color }) => (
  <div className="bg-white p-4 rounded-2xl border border-slate-200 shadow-xs text-center">
    <p className={`text-2xl font-extrabold font-mono ${color}`}>{value}</p>
    <p className="text-[10px] font-extrabold uppercase tracking-wider text-slate-400">{label}</p>
  </div>
);
