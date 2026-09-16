// Bootstrap idempotente de datos demo para verificación visual completa.
// Uso: node scripts/dev/seed-live.mjs  (requiere API en :5196)
const API = process.env.API_URL || "http://localhost:5196/api/v1";

const CREDS = {
  admin: { email: "admin@realestate.com", password: "Admin123!" },
  agent: { email: "agent@realestate.com", password: "Agent123!" },
  agent2: { email: "agent2@realestate.com", password: "Agent123!" },
  client: { email: "client@realestate.com", password: "Client123!" },
  client2: { email: "client2@realestate.com", password: "Client123!" },
  owner: { email: "owner@realestate.com", password: "Owner123!" },
};

const report = [];
function log(marker, msg) {
  report.push(`${marker.padEnd(5)} ${msg}`);
  console.log(`${marker.padEnd(5)} ${msg}`);
}

async function api(method, path, { token, body } = {}) {
  const headers = {};
  if (token) headers.Authorization = `Bearer ${token}`;
  let payload;
  if (body !== undefined) {
    headers["Content-Type"] = "application/json";
    payload = JSON.stringify(body);
  }
  const res = await fetch(API + path, { method, headers, body: payload });
  let data = null;
  try { data = await res.json(); } catch { /* no json */ }
  return { status: res.status, ok: res.ok, data };
}

async function auth(email, password) {
  const r = await api("POST", "/account/authenticate", { body: { email, password } });
  if (!r.ok || !r.data?.jwToken) throw new Error(`login falló para ${email} (${r.status})`);
  return r.data.jwToken;
}

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

try {
  const tokens = {};
  for (const [role, c] of Object.entries(CREDS)) tokens[role] = await auth(c.email, c.password);
  log("OK", "logins (admin/agent/agent2/client/client2/owner)");

  // ============ AGENTE: propiedades ============
  const agentProps = (await api("GET", "/properties/my-properties", { token: tokens.agent })).data || [];
  log("INFO", `agente tiene ${agentProps.length} propiedades`);
  const available = agentProps.find((p) => /disponible/i.test(String(p.status) || ""));
  const sold = agentProps.find((p) => /vendid/i.test(String(p.status) || ""));
  const anyProp = agentProps[0];
  if (!anyProp) throw new Error("el agente demo no tiene propiedades; el seed no corrió o falló");

  // ============ COMISIÓN: aceptar oferta pendiente ============
  const recv = (await api("GET", "/offers/received", { token: tokens.agent })).data || [];
  const hasAccepted = recv.some((o) => o.status?.toLowerCase() === "accepted");
  const commissions = (await api("GET", "/commissions/my-commissions", { token: tokens.agent })).data || [];
  const acceptedOffer = recv.find((o) => o.status?.toLowerCase() === "accepted");

  const needCommission = commissions.length === 0 && (available || true);
  if (commissions.length > 0) log("SKIP", "comisión ya existe");
  else if (acceptedOffer) log("INFO", `oferta ${acceptedOffer.id} ya aceptada; comisión generada en aceptación`);
  else {
    const targetoid = recv.find((o) => o.status?.toLowerCase() === "pending") || recv[0];
    if (targetoid) {
      const acc = await api("POST", `/offers/${targetoid.id}/accept`, { token: tokens.agent });
      log(acc.ok ? "OK" : "FAIL", `aceptar oferta ${targetoid.id}${acc.ok ? " (comisión auto 5%)" : ` (${acc.status})`}`);
    } else if (available) {
      const offer = await api("POST", "/offers", {
        token: tokens.client,
        body: { propertyId: available.id, montoOfertado: Math.round(available.price * 0.9), notes: "Oferta bootstrap verificacion visual" },
      });
      if (offer.ok && offer.data?.id) {
        const acc = await api("POST", `/offers/${offer.data.id}/accept`, { token: tokens.agent });
        log(acc.ok ? "OK" : "FAIL", `oferta ${offer.data.id} creada y aceptada (comisión auto)`);
      } else log("WARN", `no se pudo crear oferta (${offer.status})`);
    } else log("WARN", "sin ofertas ni propiedad para crear comisión");
  }

  // ============ CITAS (1 Pendiente + 1 Confirmada) ============
  const appts = (await api("GET", "/appointments/my-appointments", { token: tokens.agent })).data || [];
  const pendingCount = appts.filter((a) => String(a.status).toLowerCase() === "pending").length;
  const confirmedCount = appts.filter((a) => String(a.status).toLowerCase() === "confirmed").length;

  if (appts.length < 2 && available) {
    const target = available.id;
    const tomorrow = new Date(Date.now() + 24 * 3600 * 1000);
    const madeCount = { n: appts.length };
    for (let i = 0; i < 2 && madeCount.n < 2; i++) {
      const date = tomorrow.toISOString().slice(0, 10);
      const slot = i === 0 ? "10:00 AM - 11:00 AM" : "03:00 PM - 04:00 PM";
      const hour = i === 0 ? 10 : 15;
      const appointmentDate = new Date(`${date}T${String(hour).padStart(2, "0")}:00:00`).toISOString();
      const req = await api("POST", "/appointments", {
        token: tokens.client2,
        body: { propertyId: target, appointmentDate, comments: `Visita bootstrap (${slot})` },
      });
      if (req.ok) { madeCount.n++; log("OK", `cita creada id=${req.data?.id || ""} ${slot}`); }
      else log("WARN", `no se pudo crear cita (${req.status})`);
    }
  }
  // confirmar la primera pendiente que no esté confirmada
  const apptsAfter = (await api("GET", "/appointments/my-appointments", { token: tokens.agent })).data || [];
  const stillPending = apptsAfter.find((a) => String(a.status).toLowerCase() === "pending");
  const stillConfirm = apptsAfter.find((a) => String(a.status).toLowerCase() === "confirmed");
  if (!stillConfirm && stillPending) {
    const c = await api("PATCH", `/appointments/${stillPending.id}/confirm`, { token: tokens.agent, body: null });
    log(c.ok ? "OK" : "FAIL", `confirmar cita ${stillPending.id}${c.ok ? "" : ` (${c.status})`}`);
  } else log("SKIP", `citas: ${apptsAfter.length} presente(s), pendiente/confirmada ya cubiertas`);

  // ============ DOCUMENTOS (1 por propiedad de agente) ============
  const docs = (await api("GET", `/documents/property/${anyProp.id}`, { token: tokens.agent })).data || [];
  if (docs.length === 0) {
    const form = new FormData();
    form.append("propertyId", String(anyProp.id));
    form.append("documentType", "Título de propiedad");
    form.append("file", new Blob(["DOCUMENTO DEMO VERIFICACION VISUAL"], { type: "text/plain" }), "titulo-demo.txt");
    const headers = { Authorization: `Bearer ${tokens.agent}` };
    const res = await fetch(API + "/documents", { method: "POST", headers, body: form });
    const body = await res.text();
    log(res.ok ? "OK" : "FAIL", `documento subido${res.ok ? "" : ` (${res.status} ${body.slice(0, 80)})`}`);
  } else log("SKIP", "documentos ya presentes");

  // ============ LEADS (3 para el pipeline) ============
  const leads = (await api("GET", "/leads", { token: tokens.agent })).data || [];
  if (leads.length === 0) {
    const demo = [
      { leadName: "Ana Ferreira", leadEmail: "ana.f@mail.com", leadPhone: "809-555-0101", priority: "Alta", source: "Página Web", estimatedBudget: 4500000, notes: "Busca apartamento 3 habs en Naco" },
      { leadName: "Luis Ramírez", leadEmail: "luis.r@mail.com", leadPhone: "809-555-0102", priority: "Media", source: "Referido", estimatedBudget: 3000000, notes: "Interesado en villa en Piantini" },
      { leadName: "Carmen Díaz", leadEmail: "carmen.d@mail.com", leadPhone: "849-555-0103", priority: "Baja", source: "Redes Sociales", estimatedBudget: 2200000, notes: "Primera compra, presupuesto flexible" },
    ];
    for (const lead of demo) {
      const r = await api("POST", "/leads", { token: tokens.agent, body: lead });
      log(r.ok ? "OK" : "FAIL", `lead ${lead.leadName}${r.ok ? "" : ` (${r.status})`}`);
      await sleep(150);
    }
  } else log("SKIP", "leads ya presentes");

  // ============ RESEÑA cliente sobre agente ============
  const agentId = sold?.agentId || anyProp.agentId;
  if (agentId) {
    const can = await api("GET", `/reviews/can-review?agentId=${agentId}&propertyId=${anyProp.id}`, { token: tokens.client });
    if (can.ok && can.data === true) {
      const r = await api("POST", "/reviews", {
        token: tokens.client,
        body: { agentId, propertyId: anyProp.id, rating: 5, comment: "Excelente atención y asesoría durante todo el proceso." },
      });
      log(r.ok ? "OK" : "FAIL", `reseña ★5 creada${r.ok ? "" : ` (${r.status})`}`);
    } else log("SKIP", "reseña ya emitida o no elegible");
  } else log("WARN", "no se pudo resolver agentId para reseña");

  // ============ FAVORITOS cliente ============
  const favs = (await api("GET", "/favorites", { token: tokens.client })).data || [];
  if (favs.length === 0 && anyProp) {
    const f = await api("POST", `/favorites/${anyProp.id}`, { token: tokens.client });
    log(f.ok ? "OK" : "FAIL", `favorito creado${f.ok ? "" : ` (${f.status})`}`);
  } else log("SKIP", "favoritos ya presentes");

  // ============ BÚSQUEDA GUARDADA cliente ============
  const ss = (await api("GET", "/savedsearches", { token: tokens.client })).data || [];
  if (ss.length === 0) {
    const r = await api("POST", "/savedsearches", {
      token: tokens.client,
      body: { name: "Apartamentos en Bella Vista", minRooms: 2, maxPrice: 5000000 },
    });
    log(r.ok ? "OK" : "FAIL", `búsqueda guardada${r.ok ? "" : ` (${r.status})`}`);
  } else log("SKIP", "búsquedas guardadas ya presentes");

  // ============ PROPIEDAD DEL OWNER (Fase 1) ============
  const ownerProps = (await api("GET", "/owners/my-properties", { token: tokens.owner })).data || [];
  if (Object.keys(ownerProps).length === 0 || (Array.isArray(ownerProps) && ownerProps.length === 0)) {
    const types = (await api("GET", "/propertytypes", { token: tokens.admin })).data || [];
    const sales = (await api("GET", "/saletypes", { token: tokens.admin })).data || [];
    const provs = (await api("GET", "/provinces", { token: tokens.admin })).data || [];
    const aptType = types.find((t) => /aparta/i.test(t.name)) || types[0];
    const sale = sales[0];
    const prov = provs[0];
    let mun = null;
    if (prov?.id) {
      const muns = (await api("GET", `/provinces/${prov.id}/municipalities`, { token: tokens.admin })).data || [];
      mun = muns[0];
    }
    if (aptType && sale && prov) {
      const form = new FormData();
      form.append("Name", "Villa Familiar en El Ensueño");
      form.append("PropertyTypeId", String(aptType.id));
      form.append("SaleTypeId", String(sale.id));
      form.append("Price", "15000000");
      form.append("Currency", "DOP");
      form.append("Rooms", "4");
      form.append("Bathrooms", "3");
      form.append("SizeInMeters", "320");
      form.append("Description", "Propiedad demo de Owner (Fase 1) para la verificación visual.");
      form.append("ProvinceId", String(prov.id));
      form.append("MunicipalityId", String(mun?.id || ""));
      form.append("Sector", "El Ensueño");
      form.append("FullAddress", "Calle Las Palmas #12, Santo Domingo");
      form.append("MontoSeparacion", "5000");
      form.append("PorcentajeInicialRequerido", "10");
      form.append("ImprovementIds", "");
      const res = await fetch(API + "/owners/properties", {
        method: "POST",
        headers: { Authorization: `Bearer ${tokens.owner}` },
        body: form,
      });
      const tx = await res.text();
      log(res.ok ? "OK" : "FAIL", `propiedad de Owner${res.ok ? "" : ` (${res.status} ${tx.slice(0, 120)})`}`);
    } else log("WARN", "catálogos incompletos para crear propiedad de Owner");
  } else log("SKIP", "Owner ya tiene propiedades");

  console.log("\n===== RESUMEN SEED =====");
  console.log(report.join("\n"));
} catch (err) {
  console.error("SEED ERROR:", err.message);
  process.exit(1);
}