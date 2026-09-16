import React, { Component, ErrorInfo, ReactNode } from "react";
import {
  AlertTriangle,
  RefreshCw,
  Home,
  WifiOff,
  ShieldAlert,
  ServerCrash,
  Clock,
  DatabaseZap,
  FileX,
  Unplug,
} from "lucide-react";

// ============================================================
// INVENTARIO DE ERRORES DEL PROYECTO
// ============================================================
// RED:      A1-VITE_API_URL vacia | A2-API dormida(Render 15min) | A3-backend caido
//           A4-CORS rechazado | A5-AllowedHosts 400 | A6-sin internet
// AUTH:     B1-JWT expirado/401 | B2-Rol incorrecto/403 | B3-localStorage corrupto
//           B4-Token SignalR no propagado
// REACT:    C1-chunk lazy no encontrado | C2-.map() sobre null(A1) | C3-prop undefined
//           C4-useContext fuera de Provider | C5-Leaflet sin window
//           C6-URL malformada VirtualTour/Video | C7-GalleryModal indice fuera de rango
//           C8-Recharts data=null
// FORMS:    D1-413 imagen grande | D2-400 validacion backend
// SIGNALR:  E1-Hub rechaza conexion | E2-reconexion perdida | E3-token caducado
// CONTEXT:  F1-CurrencyContext rates falla | F2-CompareContext.rehydrate falla
// ROUTER:   G3-Suspense+lazy chunk caido -> pantalla en blanco
// ============================================================

type ErrorCategory =
  | "network"
  | "auth"
  | "render"
  | "server"
  | "timeout"
  | "chunk"
  | "generic";

interface Props {
  children: ReactNode;
  context?: string;
  fallback?: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
  category: ErrorCategory;
  retryCount: number;
}

function classifyError(error: Error): ErrorCategory {
  const msg = (error.message ?? "").toLowerCase();
  const name = (error.name ?? "").toLowerCase();

  if (
    msg.includes("failed to fetch dynamically imported module") ||
    msg.includes("loading chunk") ||
    msg.includes("loading css chunk") ||
    name.includes("chunkloaderror")
  )
    return "chunk";

  if (
    msg.includes("network error") ||
    msg.includes("err_network") ||
    msg.includes("failed to fetch") ||
    msg.includes("err_internet_disconnected") ||
    msg.includes("err_name_not_resolved")
  )
    return "network";

  if (
    msg.includes("timeout") ||
    msg.includes("econnaborted") ||
    msg.includes("etimedout") ||
    msg.includes("504") ||
    msg.includes("503")
  )
    return "timeout";

  if (
    msg.includes("401") ||
    msg.includes("403") ||
    msg.includes("unauthorized") ||
    msg.includes("forbidden")
  )
    return "auth";

  if (
    msg.includes("500") ||
    msg.includes("502") ||
    msg.includes("server error") ||
    msg.includes("internal server") ||
    msg.includes("invalid host")
  )
    return "server";

  if (
    name.includes("typeerror") ||
    msg.includes("is not a function") ||
    msg.includes("cannot read properties of null") ||
    msg.includes("cannot read properties of undefined") ||
    msg.includes("map is not a function") ||
    msg.includes("undefined is not iterable")
  )
    return "render";

  return "generic";
}

export class ErrorBoundary extends Component<Props, State> {
  public state: State = {
    hasError: false,
    error: null,
    category: "generic",
    retryCount: 0,
  };

  public static getDerivedStateFromError(error: Error): Partial<State> {
    return { hasError: true, error, category: classifyError(error) };
  }

  public componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.group(`[ErrorBoundary] ${classifyError(error).toUpperCase()}`);
    console.error("Error:", error.message);
    console.error("Componente:", errorInfo.componentStack);
    console.groupEnd();
  }

  private handleRetry = () => {
    if (this.state.category === "chunk") { window.location.reload(); return; }
    this.setState((prev) => ({
      hasError: false,
      error: null,
      category: "generic",
      retryCount: prev.retryCount + 1,
    }));
  };

  private handleGoHome = () => {
    try {
      localStorage.removeItem("realestate_compare");
    } catch {
      // ignore
    }
    this.setState({ hasError: false, error: null, category: "generic", retryCount: 0 });
    window.location.href = "/";
  };

  private handleClearSession = () => {
    try {
      localStorage.removeItem("realestate_jwt_token");
      localStorage.removeItem("realestate_user");
      localStorage.removeItem("realestate_compare");
      localStorage.removeItem("realestate_currency");
    } catch {
      // ignore
    }
    window.location.href = "/login";
  };

  public render() {
    if (!this.state.hasError) return this.props.children;
    if (this.props.fallback) return this.props.fallback;

    const { error, category, retryCount } = this.state;
    const isAuth = category === "auth";
    const isChunk = category === "chunk";
    const ctxLabel = this.props.context ? ` en "${this.props.context}"` : "";

    const barColor =
      isAuth || category === "server"
        ? "from-red-500 to-rose-500"
        : isChunk
        ? "from-blue-500 to-cyan-500"
        : category === "network"
        ? "from-slate-400 to-slate-500"
        : "from-amber-400 to-orange-500";

    type CfgEntry = { icon: ReactNode; title: string; desc: string; hint?: string; bg: string; color: string };
    const configs: Record<ErrorCategory, CfgEntry> = {
      network: {
        icon: <WifiOff className="w-8 h-8" />,
        title: "Sin conexion a internet",
        desc: "No se pudo conectar con el servidor" + ctxLabel + ". Verifica tu conexion.",
        hint: "Si persiste, el servidor de la API puede estar temporalmente caido.",
        bg: "bg-slate-100 dark:bg-slate-800",
        color: "text-slate-500 dark:text-slate-400",
      },
      timeout: {
        icon: <Clock className="w-8 h-8" />,
        title: "El servidor tardo demasiado",
        desc: "La solicitud" + ctxLabel + " tardo demasiado. Render (plan gratuito) hiberna tras 15 min de inactividad.",
        hint: "Espera ~45 s a que el servidor despierte y recarga la pagina.",
        bg: "bg-amber-100 dark:bg-amber-950/60",
        color: "text-amber-600 dark:text-amber-400",
      },
      auth: {
        icon: <ShieldAlert className="w-8 h-8" />,
        title: "Sesion expirada o sin permisos",
        desc: "Tu sesion ha vencido o no tienes permisos para acceder" + ctxLabel + ".",
        hint: "Limpia las cookies del navegador si el error persiste tras iniciar sesion.",
        bg: "bg-red-100 dark:bg-red-950/60",
        color: "text-red-600 dark:text-red-400",
      },
      server: {
        icon: <ServerCrash className="w-8 h-8" />,
        title: "Error en el servidor (500/502)",
        desc: "El servidor respondio con un error inesperado" + ctxLabel + ". Intenta de nuevo en unos minutos.",
        hint: "Verifica que AllowedHosts en appsettings.json incluya el dominio de Render.",
        bg: "bg-red-100 dark:bg-red-950/60",
        color: "text-red-600 dark:text-red-400",
      },
      chunk: {
        icon: <FileX className="w-8 h-8" />,
        title: "Actualizacion disponible",
        desc: "Se detecto una nueva version de la app" + ctxLabel + ". Recarga para obtenerla.",
        hint: "Ocurre cuando hay un nuevo deploy y el navegador tiene assets del build anterior en cache.",
        bg: "bg-blue-100 dark:bg-blue-950/60",
        color: "text-blue-600 dark:text-blue-400",
      },
      render: {
        icon: <DatabaseZap className="w-8 h-8" />,
        title: "Error al procesar los datos",
        desc: "Hubo un problema al procesar los datos recibidos" + ctxLabel + ". La API puede estar mal configurada.",
        hint: "Verifica que VITE_API_URL apunte a https://realestateapp-api-g8h1.onrender.com/api/v1",
        bg: "bg-amber-100 dark:bg-amber-950/60",
        color: "text-amber-600 dark:text-amber-400",
      },
      generic: {
        icon: <AlertTriangle className="w-8 h-8" />,
        title: "Problema inesperado",
        desc: "La aplicacion encontro un error inesperado" + ctxLabel + ". Puedes reintentar o volver al inicio.",
        bg: "bg-amber-100 dark:bg-amber-950/60",
        color: "text-amber-600 dark:text-amber-400",
      },
    };

    const cfg = configs[category];

    return (
      <div
        role="alert"
        aria-live="assertive"
        className="min-h-[480px] flex items-center justify-center p-6 bg-slate-50 dark:bg-slate-950"
      >
        <div className="max-w-lg w-full bg-white dark:bg-slate-900 rounded-2xl shadow-xl border border-slate-200 dark:border-slate-800 overflow-hidden">
          <div className={`h-1.5 w-full bg-gradient-to-r ${barColor}`} />
          <div className="p-8 space-y-5">
            <div className="flex items-start gap-4">
              <div className={`flex-shrink-0 w-14 h-14 rounded-xl flex items-center justify-center ${cfg.bg} ${cfg.color}`}>
                {cfg.icon}
              </div>
              <div className="space-y-1 pt-1">
                <h2 className="text-lg font-bold text-slate-900 dark:text-white leading-snug">{cfg.title}</h2>
                <p className="text-sm text-slate-600 dark:text-slate-400">{cfg.desc}</p>
              </div>
            </div>

            {cfg.hint && (
              <div className="flex items-start gap-2.5 p-3.5 bg-slate-50 dark:bg-slate-800/60 rounded-xl border border-slate-200/80 dark:border-slate-700/60">
                <Unplug className="w-4 h-4 text-slate-400 flex-shrink-0 mt-0.5" />
                <p className="text-xs text-slate-500 dark:text-slate-400 leading-relaxed">{cfg.hint}</p>
              </div>
            )}

            {error && (
              <details>
                <summary className="text-xs text-slate-400 dark:text-slate-500 cursor-pointer hover:text-slate-600 dark:hover:text-slate-300 transition-colors select-none">
                  Ver detalles tecnicos
                </summary>
                <div className="mt-2 p-3 bg-slate-100 dark:bg-slate-800 rounded-lg font-mono text-xs text-slate-700 dark:text-slate-300 overflow-x-auto max-h-28 break-words whitespace-pre-wrap">
                  <span className="text-red-500 dark:text-red-400">[{error.name}]</span> {error.message}
                </div>
              </details>
            )}

            {retryCount > 0 && (
              <p className="text-xs text-slate-400 dark:text-slate-500 text-center">
                Intentos de recarga: {retryCount}
              </p>
            )}

            <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-3 pt-1">
              <button
                id="eb-retry"
                onClick={this.handleRetry}
                className="flex-1 flex items-center justify-center gap-2 px-4 py-2.5 bg-brand-600 hover:bg-brand-700 active:bg-brand-800 text-white font-semibold rounded-xl text-sm transition-colors shadow-sm"
              >
                <RefreshCw className="w-4 h-4" />
                {isChunk ? "Actualizar aplicacion" : "Reintentar"}
              </button>

              {isAuth ? (
                <button
                  id="eb-login"
                  onClick={this.handleClearSession}
                  className="flex-1 flex items-center justify-center gap-2 px-4 py-2.5 bg-slate-100 dark:bg-slate-800 hover:bg-slate-200 dark:hover:bg-slate-700 text-slate-700 dark:text-slate-300 font-semibold rounded-xl text-sm transition-colors"
                >
                  <ShieldAlert className="w-4 h-4" />
                  Ir a iniciar sesion
                </button>
              ) : (
                <button
                  id="eb-home"
                  onClick={this.handleGoHome}
                  className="flex-1 flex items-center justify-center gap-2 px-4 py-2.5 bg-slate-100 dark:bg-slate-800 hover:bg-slate-200 dark:hover:bg-slate-700 text-slate-700 dark:text-slate-300 font-semibold rounded-xl text-sm transition-colors"
                >
                  <Home className="w-4 h-4" />
                  Volver al inicio
                </button>
              )}
            </div>
          </div>
        </div>
      </div>
    );
  }
}