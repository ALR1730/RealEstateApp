import React, { useState, useEffect } from 'react';
import { catalogsService } from '../../api/services';
import { PropertyType } from '../../types';
import { Loader } from '../../components/common/Loader';
import { Modal } from '../../components/common/Modal';
import { Building2, Plus, Edit2, Trash2, Home } from 'lucide-react';

export const ManagePropertyTypesPage: React.FC = () => {
  const [types, setTypes] = useState<PropertyType[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const [modalOpen, setModalOpen] = useState(false);
  const [editingType, setEditingType] = useState<PropertyType | null>(null);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [deleteConfirmType, setDeleteConfirmType] = useState<PropertyType | null>(null);
  const [deleteError, setDeleteError] = useState<string | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  const loadTypes = async () => {
    try {
      const data = await catalogsService.getPropertyTypes();
      setTypes(data || []);
    } catch (err) {
      console.error("Error loading property types:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    Promise.resolve().then(() => loadTypes()).catch(console.error);
  }, []);

  const handleOpenCreate = () => {
    setEditingType(null);
    setName('');
    setDescription('');
    setModalOpen(true);
  };

  const handleOpenEdit = (pt: PropertyType) => {
    setEditingType(pt);
    setName(pt.name);
    setDescription(pt.description || '');
    setModalOpen(true);
  };

  const handleOpenDelete = (pt: PropertyType) => {
    setDeleteConfirmType(pt);
    setDeleteError(null);
  };

  const handleConfirmDelete = async () => {
    if (!deleteConfirmType) return;
    try {
      setIsDeleting(true);
      setDeleteError(null);
      await catalogsService.deletePropertyType(deleteConfirmType.id);
      setDeleteConfirmType(null);
      loadTypes();
    } catch (err: any) {
      console.error("Error deleting property type:", err);
      setDeleteError(err.response?.data?.error || "No se puede eliminar este tipo de propiedad porque tiene inmuebles asociados.");
    } finally {
      setIsDeleting(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setIsSubmitting(true);
      if (editingType) {
        await catalogsService.updatePropertyType(editingType.id, { name, description });
      } else {
        await catalogsService.createPropertyType({ name, description });
      }
      setModalOpen(false);
      loadTypes();
    } catch (err) {
      console.error("Error saving property type:", err);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      
      <div className="flex justify-between items-center pb-4 border-b border-slate-200">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <Building2 className="w-6 h-6 text-brand-600" />
            Mantenimiento de Tipos de Propiedad
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Gestión del catálogo de categorías de inmuebles (Apartamentos, Casas, Villas, etc.).
          </p>
        </div>

        <button
          onClick={handleOpenCreate}
          className="flex items-center gap-2 px-4 py-2 bg-brand-600 hover:bg-brand-700 text-white font-bold text-xs rounded-xl shadow-md transition-all"
        >
          <Plus className="w-4 h-4" />
          <span>Nuevo Tipo</span>
        </button>
      </div>

      {isLoading ? (
        <Loader text="Cargando tipos de propiedad..." />
      ) : (
        <div className="bg-white rounded-3xl border border-slate-200 overflow-hidden shadow-xs">
          <table className="w-full text-left border-collapse text-xs">
            <thead>
              <tr className="bg-slate-50 text-slate-700 font-extrabold uppercase tracking-wider border-b border-slate-200">
                <th className="py-3.5 px-4">Nombre</th>
                <th className="py-3.5 px-4">Descripción</th>
                <th className="py-3.5 px-4">Inmuebles Registrados</th>
                <th className="py-3.5 px-4 text-right">Acciones</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {types.map((t) => (
                <tr key={t.id} className="hover:bg-slate-50/80 transition-colors">
                  <td className="py-3 px-4 font-bold text-slate-900">
                    {t.name}
                  </td>
                  <td className="py-3 px-4 text-slate-600 max-w-sm truncate">
                    {t.description || 'Sin descripción'}
                  </td>
                  <td className="py-3 px-4 font-mono font-bold text-brand-600">
                    {t.propertiesCount !== undefined ? `${t.propertiesCount} inmuebles` : 'N/A'}
                  </td>
                  <td className="py-3 px-4 text-right space-x-1">
                    <button
                      onClick={() => handleOpenEdit(t)}
                      className="p-2 text-slate-600 hover:text-brand-600 hover:bg-slate-100 rounded-lg transition-colors"
                      title="Editar"
                    >
                      <Edit2 className="w-4 h-4" />
                    </button>
                    <button
                      onClick={() => handleOpenDelete(t)}
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
          title={editingType ? 'Editar Tipo de Propiedad' : 'Crear Nuevo Tipo de Propiedad'}
        >
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase mb-1">Nombre *</label>
              <input
                type="text"
                required
                placeholder="Ej: Apartamento, Villa, Penthouse..."
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500"
              />
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase mb-1">Descripción</label>
              <textarea
                rows={3}
                placeholder="Descripción general de este tipo de inmueble..."
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
                {isSubmitting ? 'Guardando...' : 'Guardar Tipo'}
              </button>
            </div>
          </form>
        </Modal>
      )}

      {deleteConfirmType && (
        <Modal
          isOpen={!!deleteConfirmType}
          onClose={() => { setDeleteConfirmType(null); setDeleteError(null); }}
          title="Confirmar Eliminación"
        >
          <div className="space-y-4">
            <p className="text-sm text-slate-600">
              ¿Estás seguro de que deseas eliminar el tipo de propiedad <strong className="text-slate-900 font-bold">{deleteConfirmType.name}</strong>? Esta acción no se puede deshacer.
            </p>

            {deleteError && (
              <div className="p-3 bg-rose-50 border border-rose-200 rounded-xl text-rose-700 text-xs font-semibold">
                {deleteError}
              </div>
            )}

            <div className="pt-3 flex justify-end gap-2">
              <button
                type="button"
                onClick={() => { setDeleteConfirmType(null); setDeleteError(null); }}
                className="px-4 py-2 rounded-xl text-xs font-bold text-slate-600 hover:bg-slate-100"
              >
                Cancelar
              </button>
              <button
                type="button"
                disabled={isDeleting}
                onClick={handleConfirmDelete}
                className="px-5 py-2 bg-rose-600 hover:bg-rose-700 disabled:opacity-50 text-white font-extrabold text-xs rounded-xl shadow-md"
              >
                {isDeleting ? 'Eliminando...' : 'Eliminar Definitivamente'}
              </button>
            </div>
          </div>
        </Modal>
      )}
    </div>
  );
};
