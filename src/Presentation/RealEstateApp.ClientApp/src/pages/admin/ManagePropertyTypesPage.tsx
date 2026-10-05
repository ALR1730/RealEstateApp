import React, { useState, useEffect } from 'react';
import { catalogsService } from '../../api/services';
import { PropertyType } from '../../types';
import { Loader } from '../../components/common/Loader';
import { Modal } from '../../components/common/Modal';
import { getApiErrorMessage } from '../../utils/formatters';
import { Building2, Plus, Edit2, Trash2 } from 'lucide-react';
import { useLanguage } from '../../context/LanguageContext';

function getPropertyTypeName(name: string, language: string): string {
  if (language !== 'en' || !name) return name;
  const n = name.toLowerCase().trim();
  if (n === 'apartamento') return 'Apartment';
  if (n === 'casa') return 'House';
  if (n === 'villa') return 'Villa';
  if (n === 'penthouse') return 'Penthouse';
  if (n === 'solar' || n === 'terreno') return 'Land / Lot';
  if (n === 'local comercial' || n === 'comercial') return 'Commercial Property';
  if (n === 'edificio') return 'Building';
  if (n === 'oficina') return 'Office';
  if (n === 'finca') return 'Farm / Ranch';
  return name;
}

export const ManagePropertyTypesPage: React.FC = () => {
  const { t, language } = useLanguage();
  const [types, setTypes] = useState<PropertyType[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const [modalOpen, setModalOpen] = useState(false);
  const [editingType, setEditingType] = useState<PropertyType | null>(null);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);

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
    setSaveError(null);
    setModalOpen(true);
  };

  const handleOpenEdit = (pt: PropertyType) => {
    setEditingType(pt);
    setName(pt.name);
    setDescription(pt.description || '');
    setSaveError(null);
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
    } catch (err: unknown) {
      console.error("Error deleting property type:", err);
      setDeleteError(getApiErrorMessage(err, t('admin.propType.deleteErrorFallback', "No se puede eliminar este tipo de propiedad porque tiene inmuebles asociados.")));
    } finally {
      setIsDeleting(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setIsSubmitting(true);
      setSaveError(null);
      if (editingType) {
        await catalogsService.updatePropertyType(editingType.id, { name, description });
      } else {
        await catalogsService.createPropertyType({ name, description });
      }
      setModalOpen(false);
      loadTypes();
    } catch (err: unknown) {
      console.error("Error saving property type:", err);
      setSaveError(getApiErrorMessage(err, t('admin.propType.saveError', 'Error al guardar el tipo de propiedad.')));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      
      <div className="flex justify-between items-center pb-4 border-b border-slate-200 dark:border-slate-800">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 dark:text-white tracking-tight flex items-center gap-2">
            <Building2 className="w-6 h-6 text-brand-600 dark:text-brand-400" />
            {t('admin.propType.title', 'Mantenimiento de Tipos de Propiedad')}
          </h2>
          <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">
            {t('admin.propType.subtitle', 'Gestión del catálogo de categorías de inmuebles (Apartamentos, Casas, Villas, etc.).')}
          </p>
        </div>

        <button
          onClick={handleOpenCreate}
          className="flex items-center gap-2 px-4 py-2 bg-brand-600 hover:bg-brand-700 text-white font-bold text-xs rounded-xl shadow-md transition-all"
        >
          <Plus className="w-4 h-4" />
          <span>{t('admin.propType.new', 'Nuevo Tipo')}</span>
        </button>
      </div>

      {isLoading ? (
        <Loader text={t('admin.propType.loading', 'Cargando tipos de propiedad...')} />
      ) : types.length === 0 ? (
        <div className="bg-white dark:bg-slate-900 rounded-3xl p-12 text-center border border-slate-200 dark:border-slate-800 space-y-3">
          <Building2 className="w-12 h-12 text-slate-300 dark:text-slate-600 mx-auto" />
          <h3 className="text-base font-bold text-slate-800 dark:text-white">
            {t('admin.propType.emptyTitle', 'Sin tipos de propiedad')}
          </h3>
          <p className="text-xs text-slate-500 dark:text-slate-400 max-w-sm mx-auto">
            {t('admin.propType.emptyDesc', 'No hay tipos de propiedad registrados en el catálogo. Comienza creando uno nuevo.')}
          </p>
        </div>
      ) : (
        <div className="bg-white dark:bg-slate-900 rounded-3xl border border-slate-200 dark:border-slate-800 overflow-hidden shadow-xs">
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse text-xs">
              <thead>
                <tr className="bg-slate-50 dark:bg-slate-800/70 text-slate-700 dark:text-slate-300 font-extrabold uppercase tracking-wider border-b border-slate-200 dark:border-slate-800">
                  <th className="py-3.5 px-4">{t('admin.propType.colName', 'Nombre')}</th>
                  <th className="py-3.5 px-4">{t('admin.propType.colDesc', 'Descripción')}</th>
                  <th className="py-3.5 px-4">{t('admin.propType.colRegistered', 'Inmuebles Registrados')}</th>
                  <th className="py-3.5 px-4 text-right">{t('admin.common.actions', 'Acciones')}</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 dark:divide-slate-800">
                {types.map((pt) => (
                  <tr key={pt.id} className="hover:bg-slate-50/80 dark:hover:bg-slate-800/40 transition-colors">
                    <td className="py-3 px-4 font-bold text-slate-900 dark:text-white">
                      {language === 'en' ? getPropertyTypeName(pt.name, language) : pt.name}
                      {language === 'en' && getPropertyTypeName(pt.name, language) !== pt.name && (
                        <span className="block text-[10px] text-slate-400 font-normal">
                          {t('admin.propType.originalName', 'Original')}: {pt.name}
                        </span>
                      )}
                    </td>
                    <td className="py-3 px-4 text-slate-600 dark:text-slate-300 max-w-sm truncate">
                      {pt.description || t('admin.propType.noDesc', 'Sin descripción')}
                    </td>
                    <td className="py-3 px-4 font-mono font-bold text-brand-600 dark:text-brand-400">
                      {pt.propertiesCount !== undefined
                        ? `${pt.propertiesCount} ${pt.propertiesCount === 1 ? t('admin.propType.propertyCountSingle', 'inmueble') : t('admin.propType.propertiesCount', 'inmuebles')}`
                        : 'N/A'}
                    </td>
                    <td className="py-3 px-4 text-right space-x-1">
                      <button
                        onClick={() => handleOpenEdit(pt)}
                        className="p-2 text-slate-600 dark:text-slate-400 hover:text-brand-600 dark:hover:text-brand-400 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-lg transition-colors"
                        title={t('admin.common.edit', 'Editar')}
                      >
                        <Edit2 className="w-4 h-4" />
                      </button>
                      <button
                        onClick={() => handleOpenDelete(pt)}
                        className="p-2 text-rose-500 hover:text-rose-700 hover:bg-rose-50 dark:hover:bg-rose-950/40 rounded-lg transition-colors"
                        title={t('admin.common.delete', 'Eliminar')}
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
      )}

      {modalOpen && (
        <Modal
          isOpen={modalOpen}
          onClose={() => setModalOpen(false)}
          title={editingType ? t('admin.propType.editTitle', 'Editar Tipo de Propiedad') : t('admin.propType.createTitle', 'Crear Nuevo Tipo de Propiedad')}
        >
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-xs font-bold text-slate-700 dark:text-slate-300 uppercase mb-1">{t('admin.propType.nameLabel', 'Nombre *')}</label>
              <input
                type="text"
                required
                placeholder={t('admin.propType.namePlaceholder', 'Ej: Apartamento, Villa, Penthouse...')}
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:ring-2 focus:ring-brand-500"
              />
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-700 dark:text-slate-300 uppercase mb-1">{t('admin.propType.descLabel', 'Descripción')}</label>
              <textarea
                rows={3}
                placeholder={t('admin.propType.descPlaceholder', 'Descripción general de este tipo de inmueble...')}
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:ring-2 focus:ring-brand-500"
              />
            </div>

            {saveError && (
              <div className="p-3 bg-rose-50 dark:bg-rose-950/40 border border-rose-200 dark:border-rose-800/60 rounded-xl text-rose-700 dark:text-rose-300 text-xs font-semibold">
                {saveError}
              </div>
            )}

            <div className="pt-3 flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setModalOpen(false)}
                className="px-4 py-2 rounded-xl text-xs font-bold text-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800"
              >
                {t('admin.common.cancel', 'Cancelar')}
              </button>
              <button
                type="submit"
                disabled={isSubmitting || !name.trim()}
                className="px-5 py-2 bg-brand-600 hover:bg-brand-700 disabled:opacity-50 text-white font-extrabold text-xs rounded-xl shadow-md"
              >
                {isSubmitting ? t('admin.common.saving', 'Guardando...') : t('admin.propType.save', 'Guardar Tipo')}
              </button>
            </div>
          </form>
        </Modal>
      )}

      {deleteConfirmType && (
        <Modal
          isOpen={!!deleteConfirmType}
          onClose={() => { setDeleteConfirmType(null); setDeleteError(null); }}
          title={t('admin.propType.deleteTitle', 'Confirmar Eliminación')}
        >
          <div className="space-y-4">
            <p className="text-sm text-slate-600 dark:text-slate-300">
              {t('admin.propType.deletePrompt', '¿Estás seguro de que deseas eliminar el tipo de propiedad')} <strong className="text-slate-900 dark:text-white font-bold">{deleteConfirmType.name}</strong>? {t('admin.propType.deleteWarning', 'Esta acción no se puede deshacer.')}
            </p>

            {deleteError && (
              <div className="p-3 bg-rose-50 dark:bg-rose-950/40 border border-rose-200 dark:border-rose-800/60 rounded-xl text-rose-700 dark:text-rose-300 text-xs font-semibold">
                {deleteError}
              </div>
            )}

            <div className="pt-3 flex justify-end gap-2">
              <button
                type="button"
                onClick={() => { setDeleteConfirmType(null); setDeleteError(null); }}
                className="px-4 py-2 rounded-xl text-xs font-bold text-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800"
              >
                {t('admin.common.cancel', 'Cancelar')}
              </button>
              <button
                type="button"
                disabled={isDeleting}
                onClick={handleConfirmDelete}
                className="px-5 py-2 bg-rose-600 hover:bg-rose-700 disabled:opacity-50 text-white font-extrabold text-xs rounded-xl shadow-md"
              >
                {isDeleting ? t('admin.propType.deleting', 'Eliminando...') : t('admin.propType.deleteDefinitely', 'Eliminar Definitivamente')}
              </button>
            </div>
          </div>
        </Modal>
      )}
    </div>
  );
};
