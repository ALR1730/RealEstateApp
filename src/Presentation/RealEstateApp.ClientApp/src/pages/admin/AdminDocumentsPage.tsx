import React, { useState, useEffect } from 'react';
import { documentsService } from '../../api/services';
import { PropertyDocument } from '../../types';
import { formatDate } from '../../utils/formatters';
import { Loader } from '../../components/common/Loader';
import { FileText, Trash2, Download, Image as ImageIcon, RefreshCw, Building2 } from 'lucide-react';
import { useLanguage } from '../../context/LanguageContext';

function formatSize(bytes: number): string {
  if (bytes >= 1024 * 1024) return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  if (bytes >= 1024) return `${Math.round(bytes / 1024)} KB`;
  return `${bytes} B`;
}

function getDocTypeLabel(type: string, t: (k: string, f?: string) => string): string {
  if (!type) return '';
  const normalized = type.toLowerCase().trim();
  if (normalized === 'título de propiedad' || normalized === 'titulo de propiedad' || normalized === 'property title') {
    return t('docType.title', 'Título de propiedad');
  }
  if (normalized === 'contrato de compra-venta' || normalized === 'contrato de compraventa' || normalized === 'sales contract') {
    return t('docType.salesContract', 'Contrato de compra-venta');
  }
  if (normalized === 'contrato de alquiler' || normalized === 'lease agreement') {
    return t('docType.leaseAgreement', 'Contrato de alquiler');
  }
  if (normalized === 'carta de pre-aprobación' || normalized === 'carta de pre-aprobacion' || normalized === 'pre-approval letter') {
    return t('docType.preApproval', 'Carta de pre-aprobación');
  }
  if (normalized === 'certificación de registro' || normalized === 'certificacion de registro' || normalized === 'registry certification') {
    return t('docType.registryCert', 'Certificación de registro');
  }
  if (normalized === 'otro' || normalized === 'other') {
    return t('docType.other', 'Otro');
  }
  return type;
}

export const AdminDocumentsPage: React.FC = () => {
  const { t, language } = useLanguage();
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
    const confirmPattern = t('admin.docs.deleteConfirmSpecific', '¿Eliminar "{name}"? Esta acción no se puede deshacer.');
    const confirmMsg = confirmPattern.replace('{name}', doc.originalFileName);
    if (!window.confirm(confirmMsg)) return;
    try {
      await documentsService.remove(doc.id);
      await loadAll();
    } catch (err) {
      console.error("Error deleting document:", err);
      alert(t('admin.docs.errorDeleting', "Error al eliminar el documento."));
    }
  };

  if (isLoading) return <Loader text={t('admin.docs.loading', "Cargando documentos legales...")} />;

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-4 border-b border-slate-200 dark:border-slate-800">
        <div>
          <h2 className="text-2xl font-extrabold text-slate-900 dark:text-white tracking-tight flex items-center gap-2">
            <FileText className="w-6 h-6 text-brand-600 dark:text-brand-400" />
            {t('admin.docs.title', "Documentos Legales por Propiedad")}
          </h2>
          <p className="text-xs text-slate-500 dark:text-slate-400 mt-0.5">
            {t('admin.docs.subtitle', "Registro central de títulos, contratos y certificaciones subidos por los agentes.")}
          </p>
        </div>
        <button
          onClick={loadAll}
          className="inline-flex items-center gap-1.5 px-3 py-2 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-slate-600 dark:text-slate-300 rounded-xl font-bold text-xs hover:bg-slate-50 dark:hover:bg-slate-700/60 transition-colors"
        >
          <RefreshCw className="w-3.5 h-3.5" />
          {t('admin.docs.refresh', "Refrescar")}
        </button>
      </div>

      {documents.length === 0 ? (
        <div className="bg-white dark:bg-slate-900 rounded-3xl p-12 text-center border border-slate-200 dark:border-slate-800 space-y-3">
          <FileText className="w-12 h-12 text-slate-300 dark:text-slate-600 mx-auto" />
          <h3 className="text-base font-bold text-slate-800 dark:text-white">{t('admin.docs.emptyTitle', "Sin documentos registrados")}</h3>
          <p className="text-xs text-slate-500 dark:text-slate-400 max-w-sm mx-auto">
            {t('admin.docs.emptyDesc', "Los agentes pueden subir la documentación legal de sus propiedades desde el portal del agente.")}
          </p>
        </div>
      ) : (
        <div className="bg-white dark:bg-slate-900 rounded-3xl border border-slate-200 dark:border-slate-800 overflow-hidden shadow-xs">
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse text-xs">
              <thead>
                <tr className="bg-slate-50 dark:bg-slate-800/70 text-slate-700 dark:text-slate-300 font-extrabold uppercase tracking-wider border-b border-slate-200 dark:border-slate-800">
                  <th className="py-3.5 px-4">{t('admin.docs.colPreview', "Vista Previa")}</th>
                  <th className="py-3.5 px-4">{t('admin.docs.colFile', "Archivo")}</th>
                  <th className="py-3.5 px-4">{t('admin.docs.colProperty', "Propiedad")}</th>
                  <th className="py-3.5 px-4">{t('admin.docs.colType', "Tipo")}</th>
                  <th className="py-3.5 px-4">{t('admin.docs.colUploadedBy', "Subido Por")}</th>
                  <th className="py-3.5 px-4">{t('admin.docs.colDate', "Fecha")}</th>
                  <th className="py-3.5 px-4 text-right">{t('admin.docs.colActions', "Acciones")}</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 dark:divide-slate-800">
                {documents.map((doc) => (
                  <tr key={doc.id} className="hover:bg-slate-50/80 dark:hover:bg-slate-800/40 transition-colors">
                    <td className="py-3 px-4">
                      {doc.isImage ? (
                        <img
                          src={doc.fileUrl}
                          alt={doc.originalFileName}
                          className="w-12 h-12 rounded-xl object-cover border border-slate-200 dark:border-slate-700"
                        />
                      ) : (
                        <div className="w-12 h-12 rounded-xl bg-brand-50 dark:bg-brand-950/40 border border-brand-100 dark:border-brand-800/60 flex items-center justify-center">
                          <ImageIcon className="w-5 h-5 text-brand-500" />
                        </div>
                      )}
                    </td>
                    <td className="py-3 px-4">
                      <span className="font-bold text-slate-800 dark:text-slate-200">{doc.originalFileName}</span>
                      <p className="text-[10px] text-slate-400 dark:text-slate-500">{formatSize(doc.sizeBytes)}</p>
                    </td>
                    <td className="py-3 px-4">
                      <span className="font-mono font-bold text-slate-700 dark:text-slate-300">#{doc.propertyCode || doc.propertyId}</span>
                      <p className="text-[11px] text-slate-500 dark:text-slate-400 truncate max-w-xs flex items-center gap-1">
                        <Building2 className="w-3 h-3" /> {doc.propertyName || ''}
                      </p>
                    </td>
                    <td className="py-3 px-4">
                      <span className="inline-flex items-center px-2.5 py-1 bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300 font-bold rounded-lg text-[10px]">
                        {getDocTypeLabel(doc.documentType, t)}
                      </span>
                    </td>
                    <td className="py-3 px-4 font-semibold text-slate-600 dark:text-slate-300">{doc.uploadedByName || doc.uploadedBy}</td>
                    <td className="py-3 px-4 text-slate-600 dark:text-slate-400 font-medium">{formatDate(doc.uploadedAt, language === 'en' ? 'en-US' : 'es-DO')}</td>
                    <td className="py-3 px-4 text-right space-x-1">
                      <a
                        href={doc.fileUrl}
                        target="_blank"
                        rel="noreferrer"
                        className="inline-flex p-2 text-slate-600 dark:text-slate-400 hover:text-brand-600 dark:hover:text-brand-400 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-lg transition-colors"
                        title={t('admin.docs.openDoc', "Abrir Documento")}
                      >
                        <Download className="w-4 h-4" />
                      </a>
                      <button
                        onClick={() => handleDelete(doc)}
                        className="p-2 text-rose-600 hover:bg-rose-50 dark:hover:bg-rose-950/30 rounded-lg transition-colors"
                        title={t('admin.docs.deleteDoc', "Eliminar Documento")}
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