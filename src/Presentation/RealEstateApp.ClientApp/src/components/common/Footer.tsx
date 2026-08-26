import React from 'react';
import { Link } from 'react-router-dom';
import { Building2, ShieldCheck, Phone, Mail, MapPin, Heart } from 'lucide-react';

export const Footer: React.FC = () => {
  return (
    <footer className="bg-navy-950 text-slate-400 pt-16 pb-12 border-t border-slate-800">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-10 pb-12 border-b border-slate-800/80">
          
          {/* Col 1: Brand & Bio */}
          <div className="space-y-4">
            <div className="flex items-center gap-2.5">
              <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-brand-600 to-emerald-400 flex items-center justify-center text-white shadow-lg shadow-brand-500/30">
                <Building2 className="w-6 h-6" />
              </div>
              <span className="font-extrabold text-xl tracking-tight text-white">
                RealEstate<span className="text-brand-500">App</span>
              </span>
            </div>
            <p className="text-sm leading-relaxed text-slate-400">
              Ecosistema transaccional inmobiliario de clase empresarial para República Dominicana. Negociación de ofertas en tiempo real y simulación hipotecaria profesional.
            </p>
            <div className="flex items-center gap-2 text-xs font-semibold text-emerald-400 bg-emerald-950/40 border border-emerald-800/40 px-3 py-1.5 rounded-lg w-fit">
              <ShieldCheck className="w-4 h-4" />
              <span>Transacciones Seguras en RD$</span>
            </div>
          </div>

          {/* Col 2: Accesos Rápidos */}
          <div>
            <h4 className="text-white font-bold text-sm tracking-wider uppercase mb-4">Navegación</h4>
            <ul className="space-y-2.5 text-sm">
              <li>
                <Link to="/catalog" className="hover:text-brand-400 transition-colors">
                  Catálogo de Inmuebles
                </Link>
              </li>
              <li>
                <Link to="/simulator" className="hover:text-brand-400 transition-colors">
                  Simulador Hipotecario (RD$)
                </Link>
              </li>
              <li>
                <Link to="/agents" className="hover:text-brand-400 transition-colors">
                  Directorio de Corredores
                </Link>
              </li>
              <li>
                <Link to="/login" className="hover:text-brand-400 transition-colors">
                  Acceso al Sistema
                </Link>
              </li>
            </ul>
          </div>

          {/* Col 3: Soluciones por Perfil */}
          <div>
            <h4 className="text-white font-bold text-sm tracking-wider uppercase mb-4">Plataforma</h4>
            <ul className="space-y-2.5 text-sm">
              <li>
                <Link to="/register" className="hover:text-brand-400 transition-colors">
                  Registro de Compradores
                </Link>
              </li>
              <li>
                <Link to="/register-agent" className="hover:text-brand-400 transition-colors">
                  Unirse como Agente Inmobiliario
                </Link>
              </li>
              <li>
                <span className="text-slate-500 cursor-default">
                  Arquitectura Onion .NET 10 + React SPA
                </span>
              </li>
              <li>
                <span className="text-slate-500 cursor-default">
                  Regla Atómica de Cierre en Cascada
                </span>
              </li>
            </ul>
          </div>

          {/* Col 4: Contacto & Región */}
          <div>
            <h4 className="text-white font-bold text-sm tracking-wider uppercase mb-4">Contacto</h4>
            <ul className="space-y-3 text-sm">
              <li className="flex items-start gap-2.5">
                <MapPin className="w-4 h-4 text-brand-500 shrink-0 mt-0.5" />
                <span>Santo Domingo, República Dominicana</span>
              </li>
              <li className="flex items-center gap-2.5">
                <Phone className="w-4 h-4 text-brand-500 shrink-0" />
                <span>+1 (809) 555-0199</span>
              </li>
              <li className="flex items-center gap-2.5">
                <Mail className="w-4 h-4 text-brand-500 shrink-0" />
                <span>soporte@realestateapp.com</span>
              </li>
            </ul>
          </div>
        </div>

        {/* Bottom copyright */}
        <div className="pt-8 flex flex-col sm:flex-row items-center justify-between gap-4 text-xs text-slate-500">
          <p>© {new Date().getFullYear()} RealEstateApp. Todos los derechos reservados.</p>
          <div className="flex items-center gap-1">
            <span>Desarrollado con</span>
            <Heart className="w-3.5 h-3.5 text-rose-500 fill-rose-500" />
            <span>para el mercado inmobiliario dominicano</span>
          </div>
        </div>
      </div>
    </footer>
  );
};
