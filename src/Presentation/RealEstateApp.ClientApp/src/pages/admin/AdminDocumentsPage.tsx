import React, { useState, useEffect } from 'react';
import { documentsService } from '../../api/services';
import { PropertyDocument } from '../../types';
import { formatDate } from '../../utils/formatters';
import { Loader } from '../../components/common/Loader';
import { FileText, Trash2, Download, Image as ImageIcon, RefreshCw, Building2 } from 'lucide-react';

function formatSize(bytes: number): string {
  if (bytes >= 1024 * 1024) return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  if (bytes >= 1024) return `${Math.round(bytes / 1024)} KB`;
  return `${bytes} B`;
}

export const AdminDocumentsPage: React.FC = () => {
  const [documents, setDocuments] = useState<PropertyDocument[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const loadAll = async () => {
    try {
      const data = await documentsService.getAll();
      setDocuments(data || []);
    } catch (err) {
      console.error("Error loading documents:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    const load = async () => {
      try {
        const data = await documentsService.getAll();
        setDocuments(data || []);
      } catch (err) {
        console.error("Error loading documents:", err);
      } finally {
        setIsLoading(false);
      }
    };
    load();
  }, []);

  const handleDelete = async (doc: PropertyDocument) => {
    if (!window.confirm(`¿Eliminar "${doc.originalFileName}"? Esta acción no se puede deshacer.`)) return;
    try {
      await documentsService.remove(doc.id);
      await loadAll();
    } catch (err) {
      console.error("Error deleting document:", err);
      alert("Error al eliminar el documento.");
    }
  };

  if (isLoading) return <Loader text="Cargando documentos legales..." />;

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-4 border-b border-slate-200">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
            <FileText className="w-6 h-6 text-brand-600" />
            Documentos Legales por Propiedad
          </h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Registro central de títulos, contratos y certificaciones subidos por los agentes.
          </p>
        </div>
        <button
          onClick={loadAll}
          className="inline-flex items-center gap-1.5 px-3 py-2 bg-white border border-slate-200 text-slate-600 rounded-xl font-bold text-xs hover:bg-slate-50 transition-colors"
        >
          <RefreshCw className="w-3.5 h-3.5" />
          Refrescar
        </button>
      </div>

      {documents.length === 0 ? (
        <div className="bg-white rounded-3xl p-12 text-center border border-slate-200 space-y-2">
          <FileText className="w-12 h-12 text-slate-300 mx-auto" />
          <h3 className="text-base font-bold text-slate-800">Sin documentos registrados</h3>
          <p className="text-xs text-slate-500 max-w-sm mx-auto">
            Los agentes pueden subir la documentación legal de sus propiedades desde el portal del agente.
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
                  <th className="py-3.5 px-4">Propiedad</th>
                  <th className="py-3.5 px-4">Tipo</th>
                  <th className="py-3.5 px-4">Subido Por</th>
                  <th className="py-3.5 px-4">Fecha</th>
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
                      <p className="text-[10px] text-slate-400">{formatSize(doc.sizeBytes)}</p>
                    </td>
                    <td className="py-3 px-4">
                      <span className="font-mono font-bold text-slate-700">#{doc.propertyCode || doc.propertyId}</span>
                      <p className="text-[11px] text-slate-500 truncate max-w-xs flex items-center gap-1">
                        <Building2 className="w-3 h-3" /> {doc.propertyName || ''}
                      </p>
                    </td>
                    <td className="py-3 px-4">
                      <span className="inline-flex items-center px-2.5 py-1 bg-slate-100 text-slate-600 font-bold rounded-lg text-[10px]">
                        {doc.documentType}
                      </span>
                    </td>
                    <td className="py-3 px-4 font-semibold text-slate-600">{doc.uploadedByName || doc.uploadedBy}</td>
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