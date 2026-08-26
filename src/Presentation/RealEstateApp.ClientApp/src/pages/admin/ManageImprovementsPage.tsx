import React, { useState, useEffect } from 'react';
import { catalogsService } from '../../api/services';
import { Improvement } from '../../types';
import { Loader } from '../../components/common/Loader';
import { Modal } from '../../components/common/Modal';
import { Sparkles, Plus, Edit2, Trash2 } from 'lucide-react';

export const ManageImprovementsPage: React.FC = () => {
  const [improvements, setImprovements] = useState<Improvement[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const [modalOpen, setModalOpen] = useState(false);
  const [editingImp, setEditingImp] = useState<Improvement | null>(null);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const loadImprovements = async () => {
    try {
      setIsLoading(true);
      const data = await catalogsService.getImprovements();
      setImprovements(data || []);
    } catch (err) {
      console.error("Error loading improvements:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadImprovements();
  }, []);

  const handleOpenCreate = () => {
    setEditingImp(null);
    setName('');
    setDescription('');
    setModalOpen(true);
  };

  const handleOpenEdit = (imp: Improvement) => {
    setEditingImp(imp);
    setName(imp.name);
    setDescription(imp.description || '');
    setModalOpen(true);
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm("¿Seguro que deseas eliminar esta amenidad?")) return;
    try {
      await catalogsService.deleteImprovement(id);
      loadImprovements();
    } catch (err) {
      console.error("Error deleting improvement:", err);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setIsSubmitting(true);
      if (editingImp) {
        await catalogsService.updateImprovement(editingImp.id, { name, description });
      } else {
        await catalogsService.createImprovement({ name, description });
      }
      setModalOpen(false);
      loadImprovements();
    } catch (err) {
      console.error("Error saving improvement:", err);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      
      <div className="flex justify-between items-center pb-4 border-b border-slate-200">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <Sparkles className="w-6 h-6 text-brand-600" />
            Mantenimiento de Amenidades y Mejoras
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Catálogo de características diferenciales (Piscina, Gimnasio, Planta Eléctrica, Seguridad 24/7, etc.).
          </p>
        </div>

        <button
          onClick={handleOpenCreate}
          className="flex items-center gap-2 px-4 py-2 bg-brand-600 hover:bg-brand-700 text-white font-bold text-xs rounded-xl shadow-md transition-all"
        >
          <Plus className="w-4 h-4" />
          <span>Nueva Amenidad</span>
        </button>
      </div>

      {isLoading ? (
        <Loader text="Cargando catálogo de amenidades..." />
      ) : (
        <div className="bg-white rounded-3xl border border-slate-200 overflow-hidden shadow-xs">
          <table className="w-full text-left border-collapse text-xs">
            <thead>
              <tr className="bg-slate-50 text-slate-700 font-extrabold uppercase tracking-wider border-b border-slate-200">
                <th className="py-3.5 px-4">Amenidad / Mejora</th>
                <th className="py-3.5 px-4">Descripción</th>
                <th className="py-3.5 px-4 text-right">Acciones</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {improvements.map((imp) => (
                <tr key={imp.id} className="hover:bg-slate-50/80 transition-colors">
                  <td className="py-3 px-4 font-bold text-slate-900">
                    {imp.name}
                  </td>
                  <td className="py-3 px-4 text-slate-600 max-w-sm truncate">
                    {imp.description || 'Sin descripción'}
                  </td>
                  <td className="py-3 px-4 text-right space-x-1">
                    <button
                      onClick={() => handleOpenEdit(imp)}
                      className="p-2 text-slate-600 hover:text-brand-600 hover:bg-slate-100 rounded-lg transition-colors"
                      title="Editar"
                    >
                      <Edit2 className="w-4 h-4" />
                    </button>
                    <button
                      onClick={() => handleDelete(imp.id)}
                      className="p-2 text-rose-500 hover:text-rose-700 hover:bg-rose-50 rounded-lg transition-colors"
                      title="Eliminar"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {modalOpen && (
        <Modal
          isOpen={modalOpen}
          onClose={() => setModalOpen(false)}
          title={editingImp ? 'Editar Amenidad' : 'Crear Nueva Amenidad'}
        >
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase mb-1">Nombre *</label>
              <input
                type="text"
                required
                placeholder="Ej: Piscina Infinity, Gimnasio, Ascensor..."
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500"
              />
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase mb-1">Descripción</label>
              <textarea
                rows={3}
                placeholder="Descripción de la mejora o amenidad..."
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500"
              />
            </div>

            <div className="pt-3 flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setModalOpen(false)}
                className="px-4 py-2 rounded-xl text-xs font-bold text-slate-600 hover:bg-slate-100"
              >
                Cancelar
              </button>
              <button
                type="submit"
                disabled={isSubmitting || !name.trim()}
                className="px-5 py-2 bg-brand-600 hover:bg-brand-700 disabled:opacity-50 text-white font-extrabold text-xs rounded-xl shadow-md"
              >
                {isSubmitting ? 'Guardando...' : 'Guardar Amenidad'}
              </button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  );
};
