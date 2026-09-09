import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { documentsService, propertiesService } from '../../api/services';
import { PropertyDocument, DOCUMENT_TYPES } from '../../types';
import { formatDate } from '../../utils/formatters';
import { Loader } from '../../components/common/Loader';
import { FileText, Upload, Trash2, ArrowLeft, File, Download, Image as ImageIcon, Building2 } from 'lucide-react';

function formatSize(bytes: number): string {
  if (bytes >= 1024 * 1024) return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  if (bytes >= 1024) return `${Math.round(bytes / 1024)} KB`;
  return `${bytes} B`;
}

export const AgentDocumentsPage: React.FC = () => {
  const { propertyId } = useParams<{ propertyId: string }>();
  const pid = Number(propertyId);

  const [documents, setDocuments] = useState<PropertyDocument[]>([]);
  const [property, setProperty] = useState<any | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const [documentType, setDocumentType] = useState<string>(DOCUMENT_TYPES[0]);
  const [file, setFile] = useState<File | null>(null);
  const [isUploading, setIsUploading] = useState(false);

  const loadAll = async () => {
    try {
      const [docs, propData] = await Promise.all([
        documentsService.getByProperty(pid),
        propertiesService.getMyProperties(),
      ]);
      setDocuments(docs || []);
      if (propData && propData.length) {
        setProperty(propData.find((p: any) => p.id === pid) || null);
      }
    } catch (err) {
      console.error("Error loading documents:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    Promise.resolve().then(() => loadAll()).catch(console.error);
  }, [pid]);

  const handleUpload = async () => {
    if (!file) {
      alert("Selecciona un archivo para subir.");
      return;
    }
    try {
      setIsUploading(true);
      await documentsService.upload(pid, documentType, file);
      setFile(null);
      await loadAll();
    } catch (err: any) {
      console.error("Error uploading document:", err);
      alert(err.response?.data?.error || "Error al subir el documento.");
    } finally {
      setIsUploading(false);
    }
  };

  const handleDelete = async (doc: PropertyDocument) => {
    if (!window.confirm(`¿Eliminar "${doc.originalFileName}"? Esta acción no se puede deshacer.`)) return;
    try {
      await documentsService.remove(doc.id);
      await loadAll();
    } catch (err: any) {
      console.error("Error deleting document:", err);
      alert(err.response?.data?.error || "Error al eliminar el documento.");
    }
  };

  if (isLoading) return <Loader text="Cargando documentos de la propiedad..." />;

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-4 border-b border-slate-200">
        <div>
          <Link
            to="/agent/properties"
            className="inline-flex items-center gap-1 text-[11px] font-bold text-slate-400 hover:text-slate-700 mb-2 transition-colors"
          >
            <ArrowLeft className="w-3.5 h-3.5" />
            Volver a Mis Propiedades
          </Link>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <FileText className="w-6 h-6 text-brand-600" />
            Documentos Legales
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            {property ? (
              <>
                <Building2 className="inline w-3.5 h-3.5 mr-1" />
                {property.name || 'Inmueble'} · <span className="font-mono">#{property.code}</span>
              </>
            ) : (
              `Inmueble #${pid}`
            )}
          </p>
        </div>
      </div>

      {/* Upload Card */}
      <div className="bg-white rounded-3xl border border-slate-200 shadow-xs p-6 space-y-4">
        <h3 className="text-sm font-extrabold text-slate-800 flex items-center gap-2">
          <Upload className="w-4 h-4 text-emerald-600" />
          Subir Nueva Documentación
        </h3>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div>
            <label className="block text-[11px] font-extrabold uppercase tracking-wider text-slate-500 mb-1.5">
              Tipo de Documento
            </label>
            <select
              value={documentType}
              onChange={(e) => setDocumentType(e.target.value)}
              className="w-full rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 text-sm font-semibold text-slate-700 focus:border-brand-500 focus:ring-1 focus:ring-brand-500 focus:outline-none"
            >
              {DOCUMENT_TYPES.map((t) => (
                <option key={t} value={t}>{t}</option>
              ))}
            </select>
          </div>
          <div className="sm:col-span-2">
            <label className="block text-[11px] font-extrabold uppercase tracking-wider text-slate-500 mb-1.5">
              Archivo (PDF, imagen u otro · máx 10 MB)
            </label>
            <div className="flex flex-col sm:flex-row gap-3">
              <div className="flex-1">
                <label className="flex-1 cursor-pointer flex items-center gap-2 rounded-xl border-2 border-dashed border-slate-200 bg-slate-50 hover:bg-slate-100 hover:border-brand-300 px-4 py-2.5 transition-colors">
                  <File className="w-4 h-4 text-slate-400 shrink-0" />
                  <span className="text-sm font-semibold text-slate-600 truncate">
                    {file ? file.name : 'Elegir archivo...'}
                  </span>
                  <input
                    type="file"
                    className="hidden"
                    onChange={(e) => setFile(e.target.files?.[0] || null)}
                  />
                </label>
              </div>
              <button
                onClick={handleUpload}
                disabled={isUploading}
                className="inline-flex items-center justify-center gap-1.5 px-5 py-2.5 bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 disabled:cursor-not-allowed text-white rounded-xl font-bold text-xs shadow-md transition-all"
              >
                <Upload className="w-4 h-4" />
                {isUploading ? 'Subiendo...' : 'Subir Documento'}
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Documents List */}
      {documents.length === 0 ? (
        <div className="bg-white rounded-3xl p-12 text-center border border-slate-200 space-y-2">
          <FileText className="w-12 h-12 text-slate-300 mx-auto" />
          <h3 className="text-base font-bold text-slate-800">Sin documentos registrados</h3>
          <p className="text-xs text-slate-500 max-w-sm mx-auto">
            Sube títulos de propiedad, contratos, certificaciones o cartas de pre-aprobación para tener el expediente completo.
          </p>
        </div>
      ) : (
        <div className="bg-white rounded-3xl border border-slate-200 overflow-hidden shadow-xs">
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse text-xs">
              <thead>
                <tr className="bg-slate-50 text-slate-700 font-extrabold uppercase tracking-wider border-b border-slate-200">
                  <th className="py-3.5 px-4">Vista Previa</th>
                  <th className="py-3.5 px-4">Archivo</th>
                  <th className="py-3.5 px-4">Tipo</th>
                  <th className="py-3.5 px-4">Tamaño</th>
                  <th className="py-3.5 px-4">Subido el</th>
                  <th className="py-3.5 px-4 text-right">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {documents.map((doc) => (
                  <tr key={doc.id} className="hover:bg-slate-50/80 transition-colors">
                    <td className="py-3 px-4">
                      {doc.isImage ? (
                        <img
                          src={doc.fileUrl}
                          alt={doc.originalFileName}
                          className="w-12 h-12 rounded-xl object-cover border border-slate-200"
                        />
                      ) : (
                        <div className="w-12 h-12 rounded-xl bg-brand-50 border border-brand-100 flex items-center justify-center">
                          <ImageIcon className="w-5 h-5 text-brand-500" />
                        </div>
                      )}
                    </td>
                    <td className="py-3 px-4">
                      <span className="font-bold text-slate-800">{doc.originalFileName}</span>
                      <p className="text-[10px] text-slate-400">{doc.contentType}</p>
                    </td>
                    <td className="py-3 px-4">
                      <span className="inline-flex items-center px-2.5 py-1 bg-slate-100 text-slate-600 font-bold rounded-lg text-[10px]">
                        {doc.documentType}
                      </span>
                    </td>
                    <td className="py-3 px-4 text-slate-600 font-medium">{formatSize(doc.sizeBytes)}</td>
                    <td className="py-3 px-4 text-slate-600 font-medium">{formatDate(doc.uploadedAt)}</td>
                    <td className="py-3 px-4 text-right space-x-1">
                      <a
                        href={doc.fileUrl}
                        target="_blank"
                        rel="noreferrer"
                        className="inline-flex p-2 text-slate-600 hover:text-brand-600 hover:bg-slate-100 rounded-lg transition-colors"
                        title="Abrir Documento"
                      >
                        <Download className="w-4 h-4" />
                      </a>
                      <button
                        onClick={() => handleDelete(doc)}
                        className="p-2 text-rose-600 hover:bg-rose-50 rounded-lg transition-colors"
                        title="Eliminar Documento"
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
    </div>
  );
};