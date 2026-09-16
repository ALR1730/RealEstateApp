import React from "react";
import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import { ErrorBoundary } from "../components/common/ErrorBoundary";

// Componente que lanza un error al montarse
const Bomb = ({ message, name }: { message: string; name?: string }) => {
  const err = new Error(message);
  if (name) err.name = name;
  throw err;
};

beforeEach(() => {
  vi.spyOn(console, "error").mockImplementation(() => {});
  vi.spyOn(console, "group").mockImplementation(() => {});
  vi.spyOn(console, "groupEnd").mockImplementation(() => {});
});
afterEach(() => {
  vi.restoreAllMocks();
  localStorage.clear();
});

describe("ErrorBoundary Component", () => {

  // â”€â”€ Caso base â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  it("renderiza hijos cuando no hay error", () => {
    render(
      <ErrorBoundary>
        <p data-testid="child">Hijo OK</p>
      </ErrorBoundary>
    );
    expect(screen.getByTestId("child")).toBeTruthy();
  });

  it("muestra fallback personalizado si se pasa la prop fallback", () => {
    render(
      <ErrorBoundary fallback={<div data-testid="custom-fb">Mi fallback</div>}>
        <Bomb message="cualquier error" />
      </ErrorBoundary>
    );
    expect(screen.getByTestId("custom-fb")).toBeTruthy();
  });

  it("incluye el contexto en el mensaje cuando se pasa la prop context", () => {
    render(
      <ErrorBoundary context="Catalogo">
        <Bomb message="network error" />
      </ErrorBoundary>
    );
    expect(screen.getByText(/en "Catalogo"/i)).toBeTruthy();
  });

  // â”€â”€ network â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  describe("Categoria: network", () => {
    it('muestra "Sin conexion a internet" para "network error"', () => {
      render(<ErrorBoundary><Bomb message="network error" /></ErrorBoundary>);
      expect(screen.getByText(/Sin conexion a internet/i)).toBeTruthy();
    });
    it('muestra "Sin conexion a internet" para "failed to fetch"', () => {
      render(<ErrorBoundary><Bomb message="Failed to fetch" /></ErrorBoundary>);
      expect(screen.getByText(/Sin conexion a internet/i)).toBeTruthy();
    });
    it("muestra boton Reintentar", () => {
      render(<ErrorBoundary><Bomb message="network error" /></ErrorBoundary>);
      expect(screen.getByText("Reintentar")).toBeTruthy();
    });
    it("muestra boton Volver al inicio", () => {
      render(<ErrorBoundary><Bomb message="network error" /></ErrorBoundary>);
      expect(screen.getByText("Volver al inicio")).toBeTruthy();
    });
  });

  // â”€â”€ timeout â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  describe("Categoria: timeout", () => {
    it('muestra "El servidor tardo demasiado" para "timeout"', () => {
      render(<ErrorBoundary><Bomb message="timeout exceeded" /></ErrorBoundary>);
      expect(screen.getByText(/El servidor tardo demasiado/i)).toBeTruthy();
    });
    it('muestra "El servidor tardo demasiado" para "504"', () => {
      render(<ErrorBoundary><Bomb message="HTTP 504 Gateway Timeout" /></ErrorBoundary>);
      expect(screen.getByText(/El servidor tardo demasiado/i)).toBeTruthy();
    });
    it("muestra hint sobre Render hibernando", () => {
      render(<ErrorBoundary><Bomb message="timeout" /></ErrorBoundary>);
      expect(screen.getByText(/hiberna/i)).toBeTruthy();
    });
  });

  // â”€â”€ auth â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  describe("Categoria: auth", () => {
    it('muestra "Sesion expirada" para 401', () => {
      render(<ErrorBoundary><Bomb message="Error 401 Unauthorized" /></ErrorBoundary>);
      expect(screen.getByText(/Sesion expirada/i)).toBeTruthy();
    });
    it('muestra "Sesion expirada" para 403 forbidden', () => {
      render(<ErrorBoundary><Bomb message="403 forbidden" /></ErrorBoundary>);
      expect(screen.getByText(/Sesion expirada/i)).toBeTruthy();
    });
    it('muestra boton "Ir a iniciar sesion"', () => {
      render(<ErrorBoundary><Bomb message="unauthorized" /></ErrorBoundary>);
      expect(screen.getByText(/Ir a iniciar sesion/i)).toBeTruthy();
    });
    it("NO muestra Volver al inicio en auth", () => {
      render(<ErrorBoundary><Bomb message="401 unauthorized" /></ErrorBoundary>);
      expect(screen.queryByText(/Volver al inicio/i)).toBeNull();
    });
    it("handleClearSession limpia localStorage y redirige a /login", () => {
      localStorage.setItem("realestate_jwt_token", "tok123");
      localStorage.setItem("realestate_user", '{"id":1}');
      const locSpy = { href: "" };
      Object.defineProperty(window, "location", { value: locSpy, writable: true });
      render(<ErrorBoundary><Bomb message="401 unauthorized" /></ErrorBoundary>);
      fireEvent.click(screen.getByText(/Ir a iniciar sesion/i));
      expect(localStorage.getItem("realestate_jwt_token")).toBeNull();
      expect(localStorage.getItem("realestate_user")).toBeNull();
      expect(window.location.href).toBe("/login");
    });
  });

  // â”€â”€ server â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  describe("Categoria: server", () => {
    it('muestra "Error en el servidor" para 500', () => {
      render(<ErrorBoundary><Bomb message="HTTP 500 Internal Server Error" /></ErrorBoundary>);
      expect(screen.getByText(/Error en el servidor/i)).toBeTruthy();
    });
    it('muestra "Error en el servidor" para 502', () => {
      render(<ErrorBoundary><Bomb message="502 Bad Gateway" /></ErrorBoundary>);
      expect(screen.getByText(/Error en el servidor/i)).toBeTruthy();
    });
    it('muestra "Error en el servidor" para "invalid host"', () => {
      render(<ErrorBoundary><Bomb message="invalid host header" /></ErrorBoundary>);
      expect(screen.getByText(/Error en el servidor/i)).toBeTruthy();
    });
    it("muestra hint de AllowedHosts", () => {
      render(<ErrorBoundary><Bomb message="500 server error" /></ErrorBoundary>);
      expect(screen.getByText(/AllowedHosts/i)).toBeTruthy();
    });
  });

  // â”€â”€ chunk â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  describe("Categoria: chunk (lazy import)", () => {
    it('muestra "Actualizacion disponible" para chunk failure', () => {
      render(
        <ErrorBoundary>
          <Bomb message="Failed to fetch dynamically imported module: /assets/HomePage.js" />
        </ErrorBoundary>
      );
      expect(screen.getByText(/Actualizacion disponible/i)).toBeTruthy();
    });
    it('muestra "Actualizacion disponible" para "loading chunk"', () => {
      render(<ErrorBoundary><Bomb message="loading chunk 42 failed" /></ErrorBoundary>);
      expect(screen.getByText(/Actualizacion disponible/i)).toBeTruthy();
    });
    it('muestra "Actualizar aplicacion" y no "Reintentar"', () => {
      render(
        <ErrorBoundary>
          <Bomb message="Failed to fetch dynamically imported module" />
        </ErrorBoundary>
      );
      expect(screen.getByText("Actualizar aplicacion")).toBeTruthy();
      expect(screen.queryByText("Reintentar")).toBeNull();
    });
    it("handleRetry llama window.location.reload() para chunk", () => {
      const reloadMock = vi.fn();
      Object.defineProperty(window, "location", {
        value: { reload: reloadMock, href: "" },
        writable: true,
      });
      render(
        <ErrorBoundary>
          <Bomb message="Failed to fetch dynamically imported module" />
        </ErrorBoundary>
      );
      fireEvent.click(screen.getByText("Actualizar aplicacion"));
      expect(reloadMock).toHaveBeenCalledOnce();
    });
  });

  // â”€â”€ render (TypeError / data parsing) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  describe("Categoria: render (TypeError)", () => {
    it('muestra "Error al procesar los datos" para TypeError con map', () => {
      render(
        <ErrorBoundary>
          <Bomb message="Cannot read properties of null (reading map)" name="TypeError" />
        </ErrorBoundary>
      );
      expect(screen.getByText(/Error al procesar los datos/i)).toBeTruthy();
    });
    it('muestra "Error al procesar los datos" para "map is not a function"', () => {
      render(<ErrorBoundary><Bomb message="properties.map is not a function" /></ErrorBoundary>);
      expect(screen.getByText(/Error al procesar los datos/i)).toBeTruthy();
    });
    it('muestra "Error al procesar los datos" para "undefined is not iterable"', () => {
      render(<ErrorBoundary><Bomb message="undefined is not iterable" /></ErrorBoundary>);
      expect(screen.getByText(/Error al procesar los datos/i)).toBeTruthy();
    });
    it("muestra hint sobre VITE_API_URL", () => {
      render(
        <ErrorBoundary>
          <Bomb message="Cannot read properties of undefined" name="TypeError" />
        </ErrorBoundary>
      );
      expect(screen.getByText(/VITE_API_URL/i)).toBeTruthy();
    });
  });

  // â”€â”€ generic â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  describe("Categoria: generic", () => {
    it('muestra "Problema inesperado" para errores desconocidos', () => {
      render(<ErrorBoundary><Bomb message="algo raro que no clasifica" /></ErrorBoundary>);
      expect(screen.getByText(/Problema inesperado/i)).toBeTruthy();
    });
    it("muestra boton Reintentar", () => {
      render(<ErrorBoundary><Bomb message="algo raro" /></ErrorBoundary>);
      expect(screen.getByText("Reintentar")).toBeTruthy();
    });
    it("muestra boton Volver al inicio", () => {
      render(<ErrorBoundary><Bomb message="algo raro" /></ErrorBoundary>);
      expect(screen.getByText("Volver al inicio")).toBeTruthy();
    });
  });

  // â”€â”€ Accesibilidad â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  describe("Accesibilidad", () => {
    it('tiene role="alert"', () => {
      render(<ErrorBoundary><Bomb message="network error" /></ErrorBoundary>);
      expect(screen.getByRole("alert")).toBeTruthy();
    });
    it("boton retry tiene id=eb-retry", () => {
      render(<ErrorBoundary><Bomb message="network error" /></ErrorBoundary>);
      expect(document.getElementById("eb-retry")).toBeTruthy();
    });
    it("boton home tiene id=eb-home para errores no-auth", () => {
      render(<ErrorBoundary><Bomb message="network error" /></ErrorBoundary>);
      expect(document.getElementById("eb-home")).toBeTruthy();
    });
    it("boton login tiene id=eb-login para errores auth", () => {
      render(<ErrorBoundary><Bomb message="401 unauthorized" /></ErrorBoundary>);
      expect(document.getElementById("eb-login")).toBeTruthy();
    });
  });

  // â”€â”€ Detalles tecnicos â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  describe("Detalles tecnicos", () => {
    it("muestra el mensaje de error en el bloque de detalles", () => {
      render(<ErrorBoundary><Bomb message="Error especifico de prueba XYZ" /></ErrorBoundary>);
      expect(screen.getByText(/Error especifico de prueba XYZ/)).toBeTruthy();
    });
    it("el elemento <details> esta colapsado por defecto", () => {
      render(<ErrorBoundary><Bomb message="network error" /></ErrorBoundary>);
      const details = document.querySelector("details");
      expect(details).toBeTruthy();
      expect(details!.open).toBe(false);
    });
  });

  // â”€â”€ componentDidCatch logging â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  describe("componentDidCatch logging", () => {
    it("llama a console.group con la categoria en mayuscula", () => {
      const groupSpy = vi.spyOn(console, "group").mockImplementation(() => {});
      render(<ErrorBoundary><Bomb message="network error" /></ErrorBoundary>);
      expect(groupSpy).toHaveBeenCalledWith(expect.stringContaining("NETWORK"));
    });
  });

});