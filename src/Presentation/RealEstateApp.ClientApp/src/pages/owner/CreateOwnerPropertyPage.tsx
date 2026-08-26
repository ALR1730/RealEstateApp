import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { catalogsService, ownersService } from '../../api/services';
import { PropertyType, SaleType, Improvement } from '../../types';
import { Loader } from '../../components/common/Loader';
import { Upload, Trash2, Plus, Check, DollarSign, ArrowLeft } from 'lucide-react';

export const CreateOwnerPropertyPage: React.FC = () => {
  const navigate = useNavigate();
  const [propertyTypes, setPropertyTypes] = useState<PropertyType[]>([]);
  const [saleTypes, setSaleTypes] = useState<SaleType[]>([]);
  const [improvements, setImprovements] = useState<Improvement[]>([]);
  const [selectedImprovements, setSelectedImprovements] = useState<number[]>([]);

  const [form, setForm] = useState({
    propertyTypeId: '',
    saleTypeId: '',
    price: '',
    landSizeMeters: '',
    bedrooms: '',
    bathrooms: '',
    description: '',
    provinceName: 'Santo Domingo',
    sector: '',
  });

  const [files, setFiles] = useState<File[]>([]);
  const [previews, setPreviews] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadCatalogs = async () => {
      try {
        setIsLoading(true);
        const [pt, st, imp] = await Promise.all([
          catalogsService.getPropertyTypes(),
          catalogsService.getSaleTypes(),
          catalogsService.getImprovements(),
        ]);
        setPropertyTypes(pt);
        setSaleTypes(st);
        setImprovements(imp);
      } catch (err) {
        console.error("Error loading catalogs:", err);
      } finally {
        setIsLoading(false);
      }
    };
    loadCatalogs();
  }, []);

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      const selected = Array.from(e.target.files);
      const combined = [...files, ...selected].slice(0, 10);
      setFiles(combined);
      setPreviews(combined.map((f) => URL.createObjectURL(f)));
    }
  };

  const handleRemoveFile = (index: number) => {
    const newFiles = files.filter((_, i) => i !== index);
    setFiles(newFiles);
    setPreviews(newFiles.map((f) => URL.createObjectURL(f)));
  };

  const handleToggleImprovement = (impId: number) => {
    setSelectedImprovements((prev) =>
      prev.includes(impId) ? prev.filter((id) => id !== impId) : [...prev, impId]
    );
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.propertyTypeId || !form.saleTypeId || !form.price) {
      setError("Por favor completa los campos requeridos.");
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);
      const formData = new FormData();
      formData.append('PropertyTypeId', form.propertyTypeId);
      formData.append('SaleTypeId', form.saleTypeId);
      formData.append('Price', form.price);
      formData.append('LandSizeMeters', form.landSizeMeters);
      formData.append('Bedrooms', form.bedrooms);
      formData.append('Bathrooms', form.bathrooms);
      formData.append('Description', form.description);
      formData.append('ProvinceName', form.provinceName);
      formData.append('Sector', form.sector);

      selectedImprovements.forEach((id) => formData.append('SelectedImprovementIds', String(id)));
      files.forEach((file) => formData.append('Photos', file));

      await ownersService.createProperty(formData);
      navigate('/owner');
    } catch (err: any) {
      console.error("Error saving owner property:", err);
      setError(err.response?.data?.error || "Error al publicar la propiedad directa.");
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) return <Loader text="Cargando formulario..." />;

  return (
    <div className="max-w-4xl mx-auto space-y-8 pb-20">
      <div className="flex items-center justify-between pb-4 border-b border-slate-200">
        <div>
          <button
            type="button"
            onClick={() => navigate('/owner')}
            className="flex items-center gap-1 text-xs font-bold text-slate-500 hover:text-slate-900 mb-2"
          >
            <ArrowLeft className="w-3.5 h-3.5" />
            <span>Volver a Mis Inmuebles</span>
          </button>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight">
            Publicar Inmueble Directo (Propietario)
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Ingresa los datos y sube hasta 10 fotos para tu listado directo sin comisiones.
          </p>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        
        {/* Photo Upload Section */}
        <div className="bg-white p-6 sm:p-8 rounded-3xl border border-slate-200 shadow-xs space-y-4">
          <div className="flex justify-between items-center">
            <h3 className="font-bold text-base text-slate-900 flex items-center gap-2">
              <Upload className="w-5 h-5 text-amber-500" />
              Fotografías del Inmueble (Hasta 10)
            </h3>
            <span className="text-xs font-mono font-bold bg-slate-100 px-3 py-1 rounded-full text-slate-700">
              {previews.length} / 10 Fotos
            </span>
          </div>

          <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 pt-2">
            {previews.map((url, idx) => (
              <div key={idx} className="relative aspect-square rounded-2xl overflow-hidden border border-slate-200 group bg-slate-100">
                <img src={url} alt="" className="w-full h-full object-cover" />
                <button
                  type="button"
                  onClick={() => handleRemoveFile(idx)}
                  className="absolute top-1.5 right-1.5 p-1.5 bg-black/60 hover:bg-rose-600 text-white rounded-lg transition-colors"
                >
                  <Trash2 className="w-3.5 h-3.5" />
                </button>
              </div>
            ))}

            {previews.length < 10 && (
              <label className="aspect-square rounded-2xl border-2 border-dashed border-slate-300 hover:border-amber-500 bg-slate-50 hover:bg-amber-50/50 flex flex-col items-center justify-center cursor-pointer transition-all text-center p-2 text-slate-500 hover:text-amber-600">
                <Plus className="w-6 h-6 mb-1" />
                <span className="text-[11px] font-bold">Subir Fotos</span>
                <input type="file" multiple accept="image/*" onChange={handleFileSelect} className="hidden" />
              </label>
            )}
          </div>
        </div>

        {/* Core details */}
        <div className="bg-white p-6 sm:p-8 rounded-3xl border border-slate-200 shadow-xs space-y-4">
          <h3 className="font-bold text-base text-slate-900">Datos Principales</h3>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase mb-1">Tipo de Propiedad *</label>
              <select
                required
                value={form.propertyTypeId}
                onChange={(e) => setForm({ ...form, propertyTypeId: e.target.value })}
                className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200"
              >
                <option value="">Selecciona tipo...</option>
                {propertyTypes.map((pt) => <option key={pt.id} value={pt.id}>{pt.name}</option>)}
              </select>
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase mb-1">Modalidad de Venta *</label>
              <select
                required
                value={form.saleTypeId}
                onChange={(e) => setForm({ ...form, saleTypeId: e.target.value })}
                className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200"
              >
                <option value="">Selecciona modalidad...</option>
                {saleTypes.map((st) => <option key={st.id} value={st.id}>{st.name}</option>)}
              </select>
            </div>
          </div>

          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase mb-1">Precio (RD$) *</label>
              <input
                type="number"
                required
                min={1}
                value={form.price}
                onChange={(e) => setForm({ ...form, price: e.target.value })}
                className="w-full px-3 py-2 text-xs font-mono font-bold rounded-xl border border-slate-200"
              />
            </div>
            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase mb-1">Metros²</label>
              <input
                type="number"
                min={1}
                value={form.landSizeMeters}
                onChange={(e) => setForm({ ...form, landSizeMeters: e.target.value })}
                className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200"
              />
            </div>
            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase mb-1">Habitaciones</label>
              <input
                type="number"
                min={0}
                value={form.bedrooms}
                onChange={(e) => setForm({ ...form, bedrooms: e.target.value })}
                className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200"
              />
            </div>
            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase mb-1">Baños</label>
              <input
                type="number"
                min={0}
                value={form.bathrooms}
                onChange={(e) => setForm({ ...form, bathrooms: e.target.value })}
                className="w-full px-3 py-2 text-xs rounded-xl border border-slate-200"
              />
            </div>
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-700 uppercase mb-1">Sector / Ubicación</label>
            <input
              type="text"
              placeholder="Ej: Naco, Santo Domingo"
              value={form.sector}
              onChange={(e) => setForm({ ...form, sector: e.target.value })}
              className="w-full px-3.5 py-2 text-xs rounded-xl border border-slate-200"
            />
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-700 uppercase mb-1">Descripción</label>
            <textarea
              rows={3}
              required
              value={form.description}
              onChange={(e) => setForm({ ...form, description: e.target.value })}
              className="w-full px-3.5 py-2 text-xs rounded-xl border border-slate-200"
            />
          </div>
        </div>

        {error && <p className="text-xs font-bold text-rose-600 p-3 bg-rose-50 rounded-xl">{error}</p>}

        <div className="flex justify-end gap-2 pt-2">
          <button
            type="submit"
            disabled={isSubmitting}
            className="px-6 py-2.5 bg-amber-500 hover:bg-amber-600 text-navy-950 font-extrabold text-xs rounded-xl shadow-md"
          >
            {isSubmitting ? 'Publicando...' : 'Publicar Inmueble Directo'}
          </button>
        </div>
      </form>
    </div>
  );
};
