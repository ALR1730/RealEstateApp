import React, { useState, useEffect } from 'react';
import { verificationsService } from '../../api/services';
import { AgentVerification } from '../../types';
import { Loader } from '../../components/common/Loader';
import { ShieldCheck, ShieldAlert, Clock, CheckCircle2, Upload } from 'lucide-react';

export const AgentVerificationPage: React.FC = () => {
  const [verification, setVerification] = useState<AgentVerification | null>(null);
  const [cedula, setCedula] = useState('');
  const [frontFile, setFrontFile] = useState<File | null>(null);
  const [backFile, setBackFile] = useState<File | null>(null);
  const [frontPreview, setFrontPreview] = useState<string | null>(null);
  const [backPreview, setBackPreview] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => {
    let isMounted = true;
    const fetchVerificationStatus = async () => {
      try {
        const data = await verificationsService.getMyStatus();
        if (isMounted) {
          setVerification(data);
          if (data) {
            setCedula(data.cedula || '');
          }
        }
      } catch (err) {
        console.error("Error loading verification status:", err);
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    fetchVerificationStatus();
    return () => {
      isMounted = false;
    };
  }, [refreshKey]);

  const handleFrontChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      const file = e.target.files[0];
      setFrontFile(file);
      setFrontPreview(URL.createObjectURL(file));
    }
  };

  const handleBackChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      const file = e.target.files[0];
      setBackFile(file);
      setBackPreview(URL.createObjectURL(file));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!cedula.trim()) {
      setError("Por favor ingresa tu número de Cédula.");
      return;
    }
    if (!frontFile || !backFile) {
      setError("Debes adjuntar ambas fotos (Frente y Reverso) de tu Cédula.");
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);
      const formData = new FormData();
      formData.append('Cedula', cedula);
      formData.append('CedulaFrontImage', frontFile);
      formData.append('CedulaBackImage', backFile);

      await verificationsService.submit(formData);
      setSuccessMsg("¡Documentos KYC enviados exitosamente! El equipo administrativo revisará tu solicitud.");
      setRefreshKey((k) => k + 1);
    } catch (err: unknown) {
      console.error("Error submitting verification:", err);
      const apiError = (err as { response?: { data?: { error?: string } } })?.response?.data?.error;
      setError(apiError || "Error al enviar la verificación.");
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) return <Loader text="Consultando estado de verificación KYC..." />;

  const isApproved = verification?.status === 'Approved' || verification?.status === 'Aprobado';
  const isPending = verification?.status === 'Pending' || verification?.status === 'Pendiente';
  const isRejected = verification?.status === 'Rejected' || verification?.status === 'Rechazado';

  return (
    <div className="max-w-3xl mx-auto space-y-8 pb-16">
      
      <div className="pb-4 border-b border-slate-200">
        <h2 className="text-2xl font-extrabold text-slate-900 tracking-tight flex items-center gap-2">
          <ShieldCheck className="w-7 h-7 text-brand-600" />
          Verificación de Identidad KYC para Corredores
        </h2>
        <p className="text-xs text-slate-500 mt-0.5">
          Obtén la insignia de Agente Verificado cargando tu documento de identidad oficial dominicano (Cédula).
        </p>
      </div>

      {/* Status Banner */}
      {isApproved && (
        <div className="p-6 bg-emerald-50 rounded-3xl border border-emerald-200 text-emerald-900 space-y-2">
          <div className="flex items-center gap-2 font-extrabold text-sm">
            <CheckCircle2 className="w-5 h-5 text-emerald-600" />
            <span>¡Tu cuenta de Agente está Oficialmente Verificada!</span>
          </div>
          <p className="text-xs text-emerald-700">
            Tus publicaciones muestran la insignia de Corredor Certificado generando máxima confianza en los compradores.
          </p>
        </div>
      )}

      {isPending && (
        <div className="p-6 bg-amber-50 rounded-3xl border border-amber-200 text-amber-900 space-y-2">
          <div className="flex items-center gap-2 font-extrabold text-sm">
            <Clock className="w-5 h-5 text-amber-600" />
            <span>Solicitud KYC en Proceso de Revisión</span>
          </div>
          <p className="text-xs text-amber-700">
            Hemos recibido tus fotos de cédula. Un administrador validará tu identidad en un plazo máximo de 24 horas.
          </p>
        </div>
      )}

      {isRejected && (
        <div className="p-6 bg-rose-50 rounded-3xl border border-rose-200 text-rose-900 space-y-2">
          <div className="flex items-center gap-2 font-extrabold text-sm">
            <ShieldAlert className="w-5 h-5 text-rose-600" />
            <span>Solicitud de Verificación Rechazada</span>
          </div>
          <p className="text-xs text-rose-700">
            Motivo: <strong>{verification?.rejectionReason || 'Documento ilegible o no válido.'}</strong>
          </p>
          <p className="text-xs text-rose-600">Por favor vuelve a subir fotos claras de tu documento abajo.</p>
        </div>
      )}

      {/* KYC Upload Form */}
      {(!isApproved && !isPending) && (
        <form onSubmit={handleSubmit} className="bg-white p-6 sm:p-8 rounded-3xl border border-slate-200 shadow-sm space-y-6">
          
          <div>
            <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider mb-1.5">
              Número de Cédula de Identidad *
            </label>
            <input
              type="text"
              required
              placeholder="001-0000000-0"
              value={cedula}
              onChange={(e) => setCedula(e.target.value)}
              className="w-full px-3.5 py-2.5 text-xs sm:text-sm font-mono rounded-xl border border-slate-200 focus:ring-2 focus:ring-brand-500 bg-slate-50/50"
            />
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
            
            {/* Front Image */}
            <div className="space-y-2">
              <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider">
                Foto Frontal de la Cédula *
              </label>
              <div className="aspect-[4/3] rounded-2xl border-2 border-dashed border-slate-300 hover:border-brand-500 bg-slate-50 flex flex-col items-center justify-center overflow-hidden relative cursor-pointer group">
                {frontPreview ? (
                  <img src={frontPreview} alt="Cédula Frontal" className="w-full h-full object-cover" />
                ) : (
                  <label className="w-full h-full flex flex-col items-center justify-center cursor-pointer p-4 text-center">
                    <Upload className="w-6 h-6 text-brand-600 mb-1" />
                    <span className="text-xs font-bold text-slate-700">Subir Lado Frontal</span>
                    <input
                      type="file"
                      accept="image/*"
                      required
                      onChange={handleFrontChange}
                      className="hidden"
                    />
                  </label>
                )}
              </div>
            </div>

            {/* Back Image */}
            <div className="space-y-2">
              <label className="block text-xs font-bold text-slate-700 uppercase tracking-wider">
                Foto Posterior de la Cédula *
              </label>
              <div className="aspect-[4/3] rounded-2xl border-2 border-dashed border-slate-300 hover:border-brand-500 bg-slate-50 flex flex-col items-center justify-center overflow-hidden relative cursor-pointer group">
                {backPreview ? (
                  <img src={backPreview} alt="Cédula Posterior" className="w-full h-full object-cover" />
                ) : (
                  <label className="w-full h-full flex flex-col items-center justify-center cursor-pointer p-4 text-center">
                    <Upload className="w-6 h-6 text-brand-600 mb-1" />
                    <span className="text-xs font-bold text-slate-700">Subir Lado Posterior</span>
                    <input
                      type="file"
                      accept="image/*"
                      required
                      onChange={handleBackChange}
                      className="hidden"
                    />
                  </label>
                )}
              </div>
            </div>
          </div>

          {error && <p className="text-xs font-bold text-rose-600 p-3 bg-rose-50 rounded-xl">{error}</p>}
          {successMsg && <p className="text-xs font-bold text-emerald-600 p-3 bg-emerald-50 rounded-xl">{successMsg}</p>}

          <div className="pt-2 flex justify-end">
            <button
              type="submit"
              disabled={isSubmitting}
              className="px-6 py-2.5 bg-brand-600 hover:bg-brand-700 disabled:opacity-50 text-white font-extrabold text-xs sm:text-sm rounded-xl shadow-md transition-all"
            >
              {isSubmitting ? 'Enviando Documentos...' : 'Enviar Solicitud de Verificación'}
            </button>
          </div>
        </form>
      )}
    </div>
  );
};
