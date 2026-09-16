// Tour visual headless en vivo contra http://localhost:5173 (puppeteer-core).
// Uso: node scripts/dev/tour-live.mjs
const { createRequire } = require("module");
const path = require("path");
const fs = require("fs");
const requirePuppeteer = createRequire(path.join(process.env.TEMP, "opencode", "ppt", "node_modules"));

const PPT = requirePuppeteer("puppeteer-core");
const CHROME = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe";
const APP = "http://localhost:5173";
const API = "http://localhost:5196/api/v1";

const CREDS = {
  client: { email: "client@realestate.com", password: "Client123!" },
  agent: { email: "agent@realestate.com", password: "Agent123!" },
  owner: { email: "owner@realestate.com", password: "Owner123!" },
  admin: { email: "admin@realestate.com", password: "Admin123!" },
};

const ROUTES = {
  public: [
    "/", "/catalog", "/properties", "/property/1", "/agents", "/agent-details/1",
    "/simulator", "/map", "/login", "/register", "/register-agent", "/forgot-password",
    "/reset-password", "/compare", "/pending-activation", "/confirm-email", "/ruta-inexistente-xyz",
  ],
  client: [
    "/client", "/client/favorites", "/client/offers", "/client/appointments",
    "/client/saved-searches", "/client/chats", "/client/profile", "/client/activity", "/client/buy-ability",
  ],
  agent: [
    "/agent", "/agent/properties", "/agent/properties/create", "/agent/properties/edit/1",
    "/agent/offers", "/agent/appointments", "/agent/chats", "/agent/profile", "/agent/verification",
    "/agent/subscription", "/agent/avm", "/agent/leads", "/agent/commissions", "/agent/documents/1",
  ],
  owner: [
    "/owner", "/owner/properties/create", "/owner/properties/edit/1", "/owner/offers", "/owner/profile",
  ],
  admin: [
    "/admin", "/admin/agents", "/admin/developers", "/admin/developers/create",
    "/admin/developers/edit/1", "/admin/admins", "/admin/admins/create", "/admin/all-properties",
    "/admin/verifications", "/admin/subscriptions", "/admin/reviews", "/admin/commissions",
    "/admin/documents", "/admin/users", "/admin/property-types", "/admin/sale-types",
    "/admin/improvements", "/admin/profile",
  ],
};

const results = [];

const NOISE = {
  "/owner/offers": [/403/i, /agent offers/i],
};

async function login(page, { email, password }) {
  await page.goto(APP + "/login", { waitUntil: "domcontentloaded", timeout: 30000 });
  await page.waitForSelector('input[type="email"]', { timeout: 15000 });
  await page.type('input[type="email"]', email, { delay: 8 });
  await page.type('input[type="password"]', password, { delay: 8 });
  await Promise.all([
    page.click('button[type="submit"]'),
    page.waitForFunction(() => localStorage.getItem("realestate_jwt_token"), { timeout: 20000 }),
  ]);
}

async function visit(page, route) {
  const errors = new Set();
  const onErr = (args) => { if (args.length) errors.add(String(args[0] ?? "").slice(0, 160)); };
  const onConsole = (m) => { if (m.type() === "error") errors.add(m.text().slice(0, 160)); };
  const onReqFailed = (r) => { const e = r.failure(); if (e) errors.add("REQ " + r.url().slice(0, 90) + " -> " + e.errorText); };
  const onPageErr = (e) => errors.add("PAGEERROR " + String(e.message || e).slice(0, 160));
  page.on("console", onConsole);
  page.on("pageerror", onPageErr);
  page.on("requestfailed", onReqFailed);
  let crashed = false;
  let h1 = "";
  let jsErrorsDup = page.evaluate(() => {
    window.__tourErrors = [];
    const orig = console.error;
    console.error = (...a) => { window.__tourErrors.push(a.map(String).join(" ").slice(0, 200)); orig(...a); };
  });
  try {
    const t0 = Date.now();
    await page.goto(APP + route, { waitUntil: "networkidle2", timeout: 25000 });
    await new Promise((r) => setTimeout(r, 900));
    const snap = await page.evaluate(() => ({
      h1: (document.querySelector("h1")?.textContent || document.querySelector("h2")?.textContent || "").trim().slice(0, 90),
      overlay: !!document.querySelector("#vite-error-overlay"),
      crashText: /lamentamos|error inesperado|algo sali|ha ocurrido un error|no se pudo cargar/i.test(document.body.innerText.slice(0, 4000)),
      bodyLen: document.body.innerText.length,
      reactErrors: (window.__tourErrors || []).slice(0, 3),
    }));
    h1 = snap.h1;
    crashed = snap.overlay || snap.crashText;
    for (const e2 of snap.reactErrors || []) errors.add("REACT " + e2);
    errors.add("LOADED " + (Date.now() - t0) + "ms len=" + snap.bodyLen);
  } catch (err) {
    errors.add("NAV " + String(err.message).slice(0, 160));
  }
  page.off("console", onConsole);
  page.off("pageerror", onPageErr);
  page.off("requestfailed", onReqFailed);
  await jsErrorsDup.catch(() => {});
  return { h1, crashed, errors: [...errors] };
}

async function run() {
  // Resolver ids reales desde el API publico (el tour no debe usar ids inventados)
  let agentId = "1";
  let propId = "1";
  try {
    const agents = await (await fetch(API + "/agents")).json();
    if (Array.isArray(agents) && agents.length) {
      agentId = agents[0].id;
      const props = await (await fetch(API + "/agents/" + agentId + "/properties")).json();
      if (Array.isArray(props) && props.length) propId = props[0].id;
    }
  } catch (e) { console.log("WARN resolviendo ids (usa fallback 1):", e.message); }

  const ROUTES_PUBLIC = [
    "/", "/catalog", "/properties", "/property/" + propId, "/agents", "/agent-details/" + agentId,
    "/simulator", "/map", "/login", "/register", "/register-agent", "/forgot-password",
    "/reset-password", "/compare", "/pending-activation", "/confirm-email", "/ruta-inexistente-xyz",
  ];

  const browser = await PPT.launch({ executablePath: CHROME, headless: true, args: ["--no-sandbox", "--disable-gpu"] });
  const base = await browser.createBrowserContext();

  const pub = await base.newPage();
  for (const route of ROUTES_PUBLIC) {
    const r = await visit(pub, route);
    results.push({ role: "public", route, ok: !r.crashed, ...r });
  }

  for (const [role, creds] of Object.entries(CREDS)) {
    const ctx = await browser.createBrowserContext();
    const page = await ctx.newPage();
    try { await login(page, creds); results.push({ role, route: "login", ok: true, h1: "LOGIN OK" }); }
    catch (e) { results.push({ role, route: "login", ok: false, h1: "LOGIN FALLO", errors: [`${e.message}`.slice(0, 160)] }); }

    // Resolver ids reales por rol (evitar 404 por ids ficticios)
    let roleRoutes = ROUTES[role];
    if (role === "owner") {
      try {
        const tok = await page.evaluate(() => localStorage.getItem("realestate_jwt_token"));
        const mine = await (await fetch(API + "/owners/my-properties", { headers: { Authorization: "Bearer " + tok } })).json();
        const props = (mine && Array.isArray(mine.properties) && mine.properties) || [];
        if (props.length) {
          roleRoutes = ROUTES[role].map((r) => r.replace("/owner/properties/edit/1", "/owner/properties/edit/" + props[0].id));
        } else {
          roleRoutes = ROUTES[role].filter((r) => r !== "/owner/properties/edit/1");
          results.push({ role, route: "owner/properties/edit/1", ok: true, h1: "SKIP (owner sin propiedades; la pantalla edit es la misma componente Create reuse)", errors: [], ownSkipped: "1" });
        }
      } catch (e) { console.log("WARN resolviendo propiedad owner:", e.message); }
    }

    for (const route of roleRoutes) {
      const r = await visit(page, route);
      if (NOISE[route]) {
        r.errors = (r.errors || []).filter((e) => !NOISE[route].some((re) => re.test(e)));
        if (!r.errors.length) r.noiseSuprimido = "1";
      }
      results.push({ role, route, ok: !r.crashed, ...r });
    }
    await ctx.close();
  }

  await browser.close();

  // Reporte
  const lines = [];
  const fails = results.filter((r) => !r.ok);
  const noise = results.filter((r) => r.noiseSuprimido).length;
  const contended = results.filter((r) => !r.route.includes("login") && !r.ownSkipped);
  lines.push("RESULTADO TOUR LIVE (" + contended.length + " rutas, " + fails.length + " problematicas" + (noise ? ", " + noise + " con ruido conocido suprimido" : "") + ")");
  for (const r of results) {
    if (!r.route.includes("login") && r.errors && r.errors.length) {
      lines.push(`${r.role.padEnd(7)} ${r.route.padEnd(42)} ${r.ok ? "OK  " : "FAIL"} h1=${r.h1 || "—"} | ${r.errors.slice(0, 2).join(" ; ")}`);
    } else if (!r.route.includes("login")) {
      lines.push(`${r.role.padEnd(7)} ${r.route.padEnd(42)} ${r.ok ? "OK  " : "FAIL"} h1=${r.h1 || "—"}`);
    } else {
      lines.push(`${r.role.padEnd(7)} ${"login".padEnd(42)} ${r.ok ? "OK  " : "FAIL"} ${r.errors && r.errors[0] || ""}`);
    }
  }
  // detalles de errores completos
  lines.push("");
  lines.push("---- DETALLE ERRORES ----");
  for (const r of results) if (!r.ok) lines.push(`${r.role} ${r.route}: ${(r.errors || []).join(" | ")}`);

  const out = lines.join("\n");
  fs.writeFileSync(path.join(process.env.TEMP, "opencode", "live", "tour-report.txt"), out);
  console.log(out);
}

run().catch((e) => { console.error("TOUR CRASH:", e); process.exit(1); });