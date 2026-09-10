const fs = require('fs');
const path = require('path');

const ROOT = path.resolve(__dirname, '..', '..');
const SERVICES = path.join(ROOT, 'src', 'Presentation', 'RealEstateApp.ClientApp', 'src', 'api', 'services.ts');
const CONTROLLERS_DIR = path.join(ROOT, 'src', 'Presentation', 'RealEstateApp.Presentation.WebApi', 'Controllers', 'v1');

function normalizeSegments(p) {
  let s = (p || '').trim();
  s = s.replace(/\?.*$/, '');
  s = s.replace(/^\/+/, '');
  s = s.replace(/\/+$/, '');
  const segs = s.split('/').filter(Boolean);
  return segs
    .map((seg) => (/{[^}]+}/.test(seg) || /\$\{[^}]+\}/.test(seg) ? '*' : seg.toLowerCase()))
    .filter((seg) => seg !== 'api' && seg !== 'v1');
}

function extractSpaCalls(src) {
  const calls = [];
  let idx = 0;
  while ((idx = src.indexOf('apiClient', idx)) !== -1) {
    let rest = src.slice(idx + 'apiClient'.length);
    const verb = /^\s*\.\s*(get|post|put|patch|delete)/.exec(rest);
    if (verb) {
      rest = rest.slice(verb[0].length);
      const paren = rest.indexOf('(');
      if (paren !== -1) {
        const after = rest.slice(paren);
        let q = null;
        let qi = -1;
        for (const c of ['`', "'", '"']) {
          const at = after.indexOf(c);
          if (at !== -1 && (qi === -1 || at < qi)) { qi = at; q = c; }
        }
        if (q) {
          const start = after.indexOf(q);
          const end = after.indexOf(q, start + 1);
          if (end !== -1) {
            let p = after.slice(start + 1, end);
            if (/\$\{[^}]+\}/.test(p)) p = p.replace(/\$\{[^}]+\}/g, '*');
            calls.push({ method: verb[1].toUpperCase(), raw: p.trim(), segs: normalizeSegments(p) });
          }
        }
      }
    }
    idx += 'apiClient'.length;
  }
  return calls;
}

const spaCalls = extractSpaCalls(fs.readFileSync(SERVICES, 'utf8'));

const apiEndpoints = [];
const ctrlFiles = fs.readdirSync(CONTROLLERS_DIR).filter((f) => f.endsWith('.cs'));
for (const f of ctrlFiles) {
  const text = fs.readFileSync(path.join(CONTROLLERS_DIR, f), 'utf8');
  const lines = text.split(/\r?\n/);
  const cname = (f.match(/^(\w+)Controller\.cs$/) || [])[1];
  if (!cname) continue;
  const lower = cname.toLowerCase();
  let base = [`api`, `v1`, lower];
  let pending = [];
  let action = null;
  const flushAction = () => {
    if (!action || pending.length === 0) { pending = []; action = null; return; }
    const verb = pending.find((a) => a.type === 'verb');
    const route = pending.find((a) => a.type === 'route');
    if (!verb) { pending = []; action = null; return; }
    const relPath = (verb.inlinePath || (route && route.inlinePath) || '');
    apiEndpoints.push({
      controller: cname,
      method: verb.method,
      rel: relPath,
      segs: normalizeSegments([...base, relPath].join('/')),
      line: action.line,
    });
    pending = [];
    action = null;
  };
  lines.forEach((line, idx) => {
    const attrRe = /\[(Http(?:Get|Post|Put|Patch|Delete))(?:\("([^"]*)"\))?\]/;
    const routeRe = /\[Route\(\s*"([^"]*)"\s*\)\]/;
    const classRe = /class\s+\w+Controller\s*:\s*BaseApiController/;
    const methodRe = /public\s+(?:async\s+)?(?:Task|IActionResult|ActionResult|StatusCodes\s+[A-Z]+Card)\b/;
    const attr = line.match(attrRe);
    const route = line.match(routeRe);
    if (attr) {
      pending.push({ type: 'verb', method: attr[1].replace(/^Http/, '').toUpperCase(), inlinePath: attr[2] || '' });
    } else if (route) {
      pending.push({ type: 'route', inlinePath: route[1] });
    }
    if (classRe.test(line)) {
      flushAction();
      pending = [];
      const r = lines.slice(0, idx).join(' ').match(/\[Route\(\s*"([^"]*)"\s*\)\]/);
      if (r) {
        base = normalizeSegments(r[1].replace(/\[controller\]/gi, lower)).filter((s) => s !== '*');
      }
    }
    if (methodRe.test(line)) {
      action = { line: idx + 1 };
      flushAction();
    }
  });
  flushAction();
}

function segScore(spaSegs, apiSegs) {
  if (spaSegs.length !== apiSegs.length) return -1;
  let score = 0;
  for (let i = 0; i < spaSegs.length; i++) {
    const a = spaSegs[i];
    const b = apiSegs[i];
    if (a === '*' || b === '*') continue;
    if (a !== b) return -1;
    score++;
  }
  return score;
}

const apiFreeze = apiEndpoints.map((e) => ({ ...e, used: false }));
const red = [];
for (const call of spaCalls) {
  let best = null;
  let bestScore = -1;
  for (const e of apiFreeze) {
    if (e.method !== call.method) continue;
    const s = segScore(call.segs, e.segs);
    if (s > bestScore) { bestScore = s; best = e; }
  }
  if (best) best.used = true;
  else red.push(call);
}
const amber = apiFreeze.filter((e) => !e.used);

const samePathNoMethod = [];
for (const call of red) {
  let other = null;
  let bestScore = -1;
  for (const e of apiFreeze) {
    if (e.method === call.method) continue;
    const s = segScore(call.segs, e.segs);
    if (s > bestScore) { bestScore = s; other = e; }
  }
  if (other) {
    samePathNoMethod.push({ method: call.method, rel: call.raw, has: `${other.method} ${other.rel}` });
  }
}

const report = {
  spaCalls: spaCalls.length,
  apiEndpoints: apiEndpoints.length,
  red: red.map((r) => ({ method: r.method, rel: r.raw })),
  amber: amber.map((a) => ({ method: a.method, rel: a.rel, controller: a.controller })),
  samePathNoMethod,
};

console.log('=== AUDITORIA DE CONTRATO SPA <-> API ===');
console.log(`SPA llamadas API: ${report.spaCalls}`);
console.log(`Endpoints API v1  : ${report.apiEndpoints}`);
console.log('');
console.log(`RED (SPA llama a ruta inexistente)         : ${report.red.length}`);
for (const r of report.red) console.log(`  - ${r.method} ${r.rel}`);
console.log('');
console.log(`AMBER (endpoint API sin uso en SPA)        : ${report.amber.length}`);
for (const a of report.amber) console.log(`  - ${a.method} api/v1/${a.controller.toLowerCase()}/${a.rel}`);
console.log('');
console.log(`RED con misma ruta pero otro verbo (probable error verbo/case) : ${report.samePathNoMethod.length}`);
for (const s of report.samePathNoMethod) console.log(`  - ${s.method} ${s.rel} => API tiene ${s.has}`);

const out = path.join(ROOT, 'scripts', 'dev', 'contract-audit-report.txt');
fs.writeFileSync(out, JSON.stringify(report, null, 2), 'utf8');
console.log(`\nReporte completo: ${out}`);