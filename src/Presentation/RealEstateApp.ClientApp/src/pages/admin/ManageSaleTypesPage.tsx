import React, { useState, useEffect } from 'react';
import { catalogsService } from '../../api/services';
import { SaleType } from '../../types';
import { Loader } from '../../components/common/Loader';
import { Modal } from '../../components/common/Modal';
import { Layers, Plus, Edit2, Trash2 } from 'lucide-react';

export const ManageSaleTypesPage: React.FC = () => {
  const [types, setTypes] = useState<SaleType[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const [modalOpen, setModalOpen] = useState(false);
  const [editingType, setEditingType] = useState<SaleType | null>(null);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const loadTypes = async () => {
    try {
      const data = await catalogsService.getSaleTypes();
      setTypes(data || []);
    } catch (err) {
      console.error("Error loading sale types:", err);
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

  const handleOpenEdit = (st: SaleType) => {
    setEditingType(st);
    setName(st.name);
    setDescription(st.description || '');
    setModalOpen(true);
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm("¿Seguro que deseas eliminar este tipo de venta?")) return;
    try {
      await catalogsService.deleteSaleType(id);
      loadTypes();
    } catch (err: any) {
      console.error("Error deleting sale type:", err);
      alert(err.response?.data?.error || "No se puede eliminar: tiene propiedades asociadas.");
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setIsSubmitting(true);
      if (editingType) {
        await catalogsService.updateSaleType(editingType.id, { name, description });
      } else {
        await catalogsService.createSaleType({ name, description });
      }
      setModalOpen(false);
      loadTypes();
    } catch (err) {
      console.error("Error saving sale type:", err);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      
      <div className="flex justify-between items-center pb-4 border-b border-slate-200">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <Layers className="w-6 h-6 text-brand-600" />
            Mantenimiento de Tipos de Venta
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Gestión de modalidades de comercialización (Venta, Alquiler, Alquiler Amueblado, etc.).
          </p>
        </div>

        <button
          onClick={handleOpenCreate}
          className="flex items-center gap-2 px-4 py-2 bg-brand-600 hover:bg-brand-700 text-white font-bold text-xs rounded-xl shadow-md transition-all"
        >
          <Plus className="w-4 h-4" />
          <span>Nueva Modalidad</span>
        </button>
      </div>

      {isLoading ? (
        <Loader text="Cargando tipos de venta..." />
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
                      onClick={() => handleDelete(t.id)}
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
          title={editingType ? 'Editar Tipo de Venta' : 'Crear Nuevo Tipo de Venta'}
        >
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase mb-1">Nombre *</label>
              <input
                type="text"
                required
                placeholder="Ej: Venta, Alquiler, Rent-to-Own..."
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500"
              />
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase mb-1">Descripción</label>
              <textarea
                rows={3}
                placeholder="Descripción de la modalidad comercial..."
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
                {isSubmitting ? 'Guardando...' : 'Guardar Modalidad'}
              </button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  );
};
