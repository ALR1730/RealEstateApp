import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { catalogsService, propertiesService } from '../../api/services';
import { PropertyType, SaleType, Improvement } from '../../types';
import { Loader } from '../../components/common/Loader';
import { 
  Building2, 
  Upload, 
  Trash2, 
  Plus, 
  Check, 
  DollarSign, 
  Sparkles, 
  Video, 
  Compass, 
  ArrowLeft,
  CheckCircle2,
  AlertCircle
} from 'lucide-react';

export const CreateEditPropertyPage: React.FC = () => {
  const { id } = useParams<{ id?: string }>();
  const isEditing = Boolean(id);
  const navigate = useNavigate();

  const [propertyTypes, setPropertyTypes] = useState<PropertyType[]>([]);
  const [saleTypes, setSaleTypes] = useState<SaleType[]>([]);
  const [improvements, setImprovements] = useState<Improvement[]>([]);
  const [selectedImprovements, setSelectedImprovements] = useState<number[]>([]);

  // Form Fields
  const [form, setForm] = useState({
    propertyTypeId: '',
    saleTypeId: '',
    price: '',
    landSizeMeters: '',
    bedrooms: '',
    bathrooms: '',
    description: '',
    provinceName: 'Santo Domingo',
    sector: 'Piantini',
    videoTourUrl: '',
    virtualTour360Url: '',
  });

  // Images state (up to 15 files)
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

        if (isEditing && id) {
          const prop = await propertiesService.getById(Number(id));
          if (prop) {
            setForm({
              propertyTypeId: String(prop.propertyTypeId),
              saleTypeId: String(prop.saleTypeId),
              price: String(prop.price),
              landSizeMeters: String(prop.landSizeMeters),
              bedrooms: String(prop.bedrooms),
              bathrooms: String(prop.bathrooms),
              description: prop.description || '',
              provinceName: prop.provinceName || 'Santo Domingo',
              sector: prop.sector || 'Piantini',
              videoTourUrl: prop.videoTourUrl || '',
              virtualTour360Url: prop.virtualTour360Url || '',
            });
            setSelectedImprovements(prop.improvements?.map((i) => i.id) || []);
            if (prop.images) {
              setPreviews(prop.images.map((i) => i.imageUrl));
            }
          }
        }
      } catch (err) {
        console.error("Error loading catalogs:", err);
      } finally {
        setIsLoading(false);
      }
    };
    loadCatalogs();
  }, [id, isEditing]);

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      const selected = Array.from(e.target.files);
      const combined = [...files, ...selected].slice(0, 15);
      setFiles(combined);

      const previewUrls = combined.map((f) => URL.createObjectURL(f));
      setPreviews(previewUrls);
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
      formData.append('VideoTourUrl', form.videoTourUrl);
      formData.append('VirtualTour360Url', form.virtualTour360Url);

      // Selected improvements
      selectedImprovements.forEach((impId) => {
        formData.append('SelectedImprovementIds', String(impId));
      });

      // Photos (up to 15 files)
      files.forEach((file) => {
        formData.append('Photos', file);
      });

      if (isEditing && id) {
        formData.append('Id', id);
        await propertiesService.update(Number(id), formData);
      } else {
        await propertiesService.create(formData);
      }

      navigate('/agent/properties');
    } catch (err: any) {
      console.error("Error saving property:", err);
      setError(err.response?.data?.error || "Error al registrar la propiedad. Revisa los datos ingresados.");
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) {
    return <Loader text="Cargando formulario y catálogos..." />;
  }

  return (
    <div className="max-w-4xl mx-auto space-y-8 pb-20">
      
      {/* Header */}
      <div className="flex items-center justify-between pb-4 border-b border-slate-200">
        <div>
          <button
            type="button"
            onClick={() => navigate(-1)}
            className="flex items-center gap-1 text-xs font-bold text-slate-500 hover:text-slate-900 mb-2"
          >
            <ArrowLeft className="w-3.5 h-3.5" />
            <span>Volver a Mis Propiedades</span>
          </button>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight">
            {isEditing ? 'Editar Inmueble Publicado' : 'Publicar Nuevo Inmueble en RD$'}
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Carga hasta 15 fotografías, amenidades y enlaces a tours virtuales 360°.
          </p>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="space-y-8">
        
        {/* Photo Upload Section (Up to 15 photos) */}
        <div className="bg-white p-6 sm:p-8 rounded-3xl border border-slate-200 shadow-xs space-y-4">
          <div className="flex justify-between items-center">
            <div>
              <h3 className="font-bold text-base text-slate-900 flex items-center gap-2">
                <Upload className="w-5 h-5 text-brand-600" />
                Galería Fotográfica (Hasta 15 Fotos)
              </h3>
              <p className="text-xs text-slate-500">
                La primera imagen seleccionada será la portada principal de la propiedad.
              </p>
            </div>
            <span className="text-xs font-mono font-bold bg-slate-100 px-3 py-1 rounded-full text-slate-700">
              {previews.length} / 15 Fotos
            </span>
          </div>

          <div className="grid grid-cols-2 sm:grid-cols-4 md:grid-cols-5 gap-3 pt-2">
            {previews.map((url, idx) => (
              <div key={idx} className="relative aspect-square rounded-2xl overflow-hidden border border-slate-200 group bg-slate-100">
                <img src={url} alt="" className="w-full h-full object-cover" />
                <button
                  type="button"
                  onClick={() => handleRemoveFile(idx)}
                  className="absolute top-1.5 right-1.5 p-1.5 bg-black/60 hover:bg-rose-600 text-white rounded-lg transition-colors shadow-sm"
                  title="Eliminar foto"
                >
                  <Trash2 className="w-3.5 h-3.5" />
                </button>
                {idx === 0 && (
                  <span className="absolute bottom-1.5 left-1.5 text-[9px] font-extrabold uppercase bg-brand-600 text-white px-2 py-0.5 rounded shadow-sm">
                    Portada
                  </span>
                )}
              </div>
            ))}

            {previews.length < 15 && (
              <label className="aspect-square rounded-2xl border-2 border-dashed border-slate-300 hover:border-brand-500 bg-slate-50 hover:bg-brand-50/50 flex flex-col items-center justify-center cursor-pointer transition-all text-center p-2 text-slate-500 hover:text-brand-600">
                <Plus className="w-6 h-6 mb-1" />
                <span className="text-[11px] font-bold">Agregar Fotos</span>
                <input
                  type="file"
                  multiple
                  accept="image/*"
                  onChange={handleFileSelect}
                  className="hidden"
                />
              </label>
            )}
          </div>
        </div>

        {/* Basic Property Details */}
        <div className="bg-white p-6 sm:p-8 rounded-3xl border border-slate-200 shadow-xs space-y-6">
          <h3 className="font-bold text-base text-slate-900">Datos Principales del Inmueble</h3>
          
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5">
                Tipo de Propiedad *
              </label>
              <select
                required
                value={form.propertyTypeId}
                onChange={(e) => setForm({ ...form, propertyTypeId: e.target.value })}
                className="w-full px-3.5 py-2.5 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500 bg-white"
              >
                <option value="">Seleccione el tipo...</option>
                {propertyTypes.map((pt) => (
                  <option key={pt.id} value={pt.id}>
                    {pt.name}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5">
                Modalidad de Negocio *
              </label>
              <select
                required
                value={form.saleTypeId}
                onChange={(e) => setForm({ ...form, saleTypeId: e.target.value })}
                className="w-full px-3.5 py-2.5 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500 bg-white"
              >
                <option value="">Seleccione la modalidad...</option>
                {saleTypes.map((st) => (
                  <option key={st.id} value={st.id}>
                    {st.name}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5">
                Precio (RD$) *
              </label>
              <div className="relative">
                <DollarSign className="w-4 h-4 absolute left-3 top-3 text-slate-400" />
                <input
                  type="number"
                  required
                  min={1}
                  placeholder="Ej: 5500000"
                  value={form.price}
                  onChange={(e) => setForm({ ...form, price: e.target.value })}
                  className="w-full pl-9 pr-3 py-2 text-xs sm:text-sm font-mono font-bold rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500 bg-slate-50/50"
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5">
                Habitaciones
              </label>
              <input
                type="number"
                min={0}
                placeholder="3"
                value={form.bedrooms}
                onChange={(e) => setForm({ ...form, bedrooms: e.target.value })}
                className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500"
              />
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5">
                Baños
              </label>
              <input
                type="number"
                min={0}
                placeholder="2"
                value={form.bathrooms}
                onChange={(e) => setForm({ ...form, bathrooms: e.target.value })}
                className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500"
              />
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5">
                Metros² (m²)
              </label>
              <input
                type="number"
                min={1}
                placeholder="150"
                value={form.landSizeMeters}
                onChange={(e) => setForm({ ...form, landSizeMeters: e.target.value })}
                className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500"
              />
            </div>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5">
                Provincia
              </label>
              <input
                type="text"
                value={form.provinceName}
                onChange={(e) => setForm({ ...form, provinceName: e.target.value })}
                className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500"
              />
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5">
                Sector / Barrio
              </label>
              <input
                type="text"
                placeholder="Ej: Piantini, Naco, Bella Vista..."
                value={form.sector}
                onChange={(e) => setForm({ ...form, sector: e.target.value })}
                className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500"
              />
            </div>
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5">
              Descripción Completa
            </label>
            <textarea
              rows={4}
              required
              placeholder="Describe las características atractivas del inmueble, acabados, cercanías a colegios y plazas comerciales..."
              value={form.description}
              onChange={(e) => setForm({ ...form, description: e.target.value })}
              className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500"
            />
          </div>
        </div>

        {/* Amenities & Improvements Checklist */}
        <div className="bg-white p-6 sm:p-8 rounded-3xl border border-slate-200 shadow-xs space-y-4">
          <div className="flex items-center gap-2">
            <Sparkles className="w-5 h-5 text-brand-600" />
            <h3 className="font-bold text-base text-slate-900">Amenidades y Mejoras Incluidas</h3>
          </div>
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-3 pt-2">
            {improvements.map((imp) => {
              const isSelected = selectedImprovements.includes(imp.id);
              return (
                <button
                  type="button"
                  key={imp.id}
                  onClick={() => handleToggleImprovement(imp.id)}
                  className={`flex items-center gap-2 p-3 rounded-2xl border text-xs font-semibold text-left transition-all ${
                    isSelected
                      ? 'bg-brand-50 border-brand-500 text-brand-900 shadow-2xs font-bold'
                      : 'bg-slate-50/50 border-slate-200 text-slate-600 hover:bg-slate-100'
                  }`}
                >
                  <div className={`w-4 h-4 rounded flex items-center justify-center text-white ${isSelected ? 'bg-brand-600' : 'border border-slate-300'}`}>
                    {isSelected && <Check className="w-3 h-3" />}
                  </div>
                  <span className="truncate">{imp.name}</span>
                </button>
              );
            })}
          </div>
        </div>

        {/* Multimedia Virtual Tours */}
        <div className="bg-white p-6 sm:p-8 rounded-3xl border border-slate-200 shadow-xs space-y-4">
          <h3 className="font-bold text-base text-slate-900">Enlaces de Tours Multimedia</h3>
          
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5 flex items-center gap-1.5">
                <Video className="w-4 h-4 text-rose-500" />
                Enlace a Video Tour (YouTube / Vimeo)
              </label>
              <input
                type="url"
                placeholder="https://www.youtube.com/watch?v=..."
                value={form.videoTourUrl}
                onChange={(e) => setForm({ ...form, videoTourUrl: e.target.value })}
                className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500"
              />
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5 flex items-center gap-1.5">
                <Compass className="w-4 h-4 text-sky-500" />
                Enlace a Recorrido Virtual 360° (Matterport / Kuula)
              </label>
              <input
                type="url"
                placeholder="https://my.matterport.com/show/?m=..."
                value={form.virtualTour360Url}
                onChange={(e) => setForm({ ...form, virtualTour360Url: e.target.value })}
                className="w-full px-3.5 py-2 text-xs sm:text-sm rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500"
              />
            </div>
          </div>
        </div>

        {error && (
          <div className="p-4 bg-rose-50 rounded-2xl border border-rose-200 text-xs text-rose-700 flex items-center gap-2 font-semibold">
            <AlertCircle className="w-5 h-5 shrink-0 text-rose-600" />
            <span>{error}</span>
          </div>
        )}

        {/* Submit Actions */}
        <div className="flex justify-end gap-3 pt-4">
          <button
            type="button"
            onClick={() => navigate('/agent/properties')}
            className="px-6 py-3 rounded-xl border border-slate-200 font-bold text-xs text-slate-600 hover:bg-slate-100"
          >
            Cancelar
          </button>
          <button
            type="submit"
            disabled={isSubmitting}
            className="px-8 py-3 bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 text-white font-extrabold text-xs sm:text-sm rounded-xl shadow-lg shadow-emerald-600/30 transition-all"
          >
            {isSubmitting ? 'Guardando Inmueble...' : isEditing ? 'Guardar Cambios' : 'Publicar Inmueble en Catálogo'}
          </button>
        </div>
      </form>
    </div>
  );
};
