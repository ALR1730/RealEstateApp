const base = 'http://localhost:5196/api/v1';

let PASS = 0;
let FAIL = 0;
let SKIP = 0;
const failures = [];

function assert(cond, label, detail) {
  if (cond) { PASS++; console.log(`  [OK] ${label}`); }
  else { FAIL++; failures.push(label); console.log(`  [FAIL] ${label} :: ${detail || ''}`); }
}
function info(label, detail) { console.log(`  [i] ${label} :: ${String(detail).slice(0, 160)}`); }

async function auth(email, password) {
  const r = await fetch(`${base}/account/authenticate`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  });
  const j = await r.json().catch(() => ({}));
  if (!r.ok || !j.jwToken) throw new Error(`auth ${email}: ${r.status} ${JSON.stringify(j).slice(0, 120)}`);
  return { token: j.jwToken, id: j.id, email: j.email, roles: j.roles };
}

async function api(method, path, { token, body, raw, extraHeaders } = {}) {
  const headers = {};
  if (token) headers.Authorization = `Bearer ${token}`;
  if (extraHeaders) Object.assign(headers, extraHeaders);
  let payload;
  if (raw !== undefined) payload = raw;
  else if (body !== undefined) { headers['Content-Type'] = 'application/json'; payload = JSON.stringify(body); }
  const r = await fetch(`${base}${path}`, { method, headers, body: payload });
  let json = null;
  try { const t = await r.text(); json = t ? JSON.parse(t) : null; } catch { json = null; }
  return { status: r.status, json };
}

function tinyPng() {
  return Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==', 'base64');
}

(async () => {
  console.log('=== MATRIZ FUNCIONAL ROL ADMIN ===');
  const admin = await auth('admin@realestate.com', 'Admin123!');
  const agent = await auth('agent@realestate.com', 'Agent123!');
  const agent2 = await auth('agent2@realestate.com', 'Agent123!');
  const client2 = await auth('client2@realestate.com', 'Client123!');

  // [0] catalogs base para crear propiedad
  console.log('\n[0] Catalogs base');
  const provinces = await api('GET', '/provinces');
  const province = (provinces.json || [])[0];
  const muns = await api('GET', `/provinces/${province.id}/municipalities`);
  const munic = (muns.json || [])[0];
  const types = await api('GET', '/propertytypes');
  const ptype = (types.json || [])[0];
  const sales = await api('GET', '/saletypes');
  const stype = (sales.json || [])[0];
  assert(!!province && !!munic && !!ptype && !!stype, 'catalogs base', `prov=${province && province.id} mun=${munic && munic.id} pt=${ptype && ptype.id} st=${stype && stype.id}`);
  const plans = await api('GET', '/subscriptions/admin/plans', { token: admin.token });
  const plansArr = Array.isArray(plans.json) ? plans.json : [];
  assert(plans.status === 200 && plansArr.length > 0, 'GET /subscriptions/admin/plans', `count=${plansArr.length}`);
  const plan1 = await api('GET', `/subscriptions/admin/plans/${plansArr[0].id}`, { token: admin.token });
  assert(plan1.status === 200 && plan1.json && plan1.json.id, `GET /subscriptions/admin/plans/${plansArr[0].id}`, `status=${plan1.status}`);

  // [1] Dashboard KPIs
  console.log('\n[1] Dashboard KPIs');
  const kpi = await api('GET', '/admin/dashboard-kpis', { token: admin.token });
  const k = kpi.json || {};
  assert(kpi.status === 200 && k && (k.totalProperties !== undefined || k.totalUsers !== undefined || k.totalAgents !== undefined), 'GET /admin/dashboard-kpis', `status=${kpi.status} ${JSON.stringify(k).slice(0, 80)}`);
  info('KPIs', JSON.stringify(k).slice(0, 160));

  // [2] Verificaciones KYC: aprobar y rechazar
  console.log('\n[2] Verificaciones KYC (approve / reject)');
  const kf1 = new FormData();
  kf1.set('Cedula', '001-1112223-4');
  kf1.append('CedulaFrontImage', new Blob([tinyPng()], { type: 'image/png' }), 'front.png');
  kf1.append('CedulaBackImage', new Blob([tinyPng()], { type: 'image/png' }), 'back.png');
  const s1 = await api('POST', '/verifications/submit', { token: agent2.token, raw: kf1 });
  assert(s1.status === 200 || s1.status === 201, 'POST /verifications/submit (agent2 #1)', `status=${s1.status}`);
  const vl = await api('GET', '/verifications', { token: admin.token });
  const vlArr = Array.isArray(vl.json) ? vl.json : [];
  const pend1 = vlArr.find((v) => v.agentId === agent2.id && /pend|revis/i.test(v.status || ''));
  assert(vl.status === 200 && !!pend1, 'GET /verifications (admin ve la pendiente de agent2)', `count=${vlArr.length}`);
  const ap = await api('POST', `/verifications/${pend1.id}/approve`, { token: admin.token });
  assert(ap.status === 200, `POST /verifications/${pend1.id}/approve`, `status=${ap.status}`);
  const ksA = await api('GET', '/verifications/my-status', { token: agent2.token });
  const stA = (ksA.json && ksA.json.status) || '';
  assert(/approv|aproba|verif|approved/i.test(stA), 'agent2 my-status = Aprobado', `status=${stA} ${JSON.stringify(ksA.json).slice(0, 60)}`);

  const kf2 = new FormData();
  kf2.set('Cedula', '001-5556667-8');
  kf2.append('CedulaFrontImage', new Blob([tinyPng()], { type: 'image/png' }), 'front2.png');
  kf2.append('CedulaBackImage', new Blob([tinyPng()], { type: 'image/png' }), 'back2.png');
  const s2 = await api('POST', '/verifications/submit', { token: agent2.token, raw: kf2 });
  assert(s2.status === 200 || s2.status === 201, 'POST /verifications/submit (agent2 #2) tras aprobacion', `status=${s2.status}`);
  const vl2 = await api('GET', '/verifications', { token: admin.token });
  const pend2 = (Array.isArray(vl2.json) ? vl2.json : []).find((v) => v.agentId === agent2.id && /pend|revis/i.test(v.status || ''));
  assert(!!pend2, 'nueva verificacion pendiente visible', `found=${!!pend2}`);
  const rej = await api('POST', `/verifications/${pend2.id}/reject`, { token: admin.token, body: { reason: 'Documento ilegible' } });
  assert(rej.status === 200, `POST /verifications/${pend2.id}/reject`, `status=${rej.status}`);
  const ksR = await api('GET', '/verifications/my-status', { token: agent2.token });
  const stR = (ksR.json && ksR.json.status) || '';
  assert(/reject|rechaz/i.test(stR), 'agent2 my-status = Rechazado', `status=${stR} ${JSON.stringify(ksR.json).slice(0, 60)}`);

  // [3] Comisiones: listado global + pagar (admin)
  console.log('\n[3] Comisiones (listado global + pago admin)');
  const preProps = await api('GET', '/properties/my-properties', { token: agent.token });
  const preAvail = (Array.isArray(preProps.json) ? preProps.json : []).find((p) => /disponible/i.test(String(p.status) || ''));
  if (preAvail) {
    const off = await api('POST', '/offers', { token: client2.token, body: { propertyId: preAvail.id, montoOfertado: Math.round(preAvail.price * 0.9), notes: 'Offer para generar comision en matriz admin' } });
    const accO = off.json && off.json.id
      ? await api('POST', `/offers/${off.json.id}/accept`, { token: agent.token })
      : null;
    assert(off.status === 200 || off.status === 201, `POST /offers (client2 -> prop ${preAvail.id})`, `status=${off.status}`);
    assert(accO && (accO.status === 200 || accO.status === 201), 'aceptar oferta genera comision', `status=${accO && accO.status}`);
  } else { SKIP++; console.log('  [SKIP] sin propiedad Disponible para generar comision'); }

  const coms = await api('GET', '/commissions', { token: admin.token });
  const comsArr = Array.isArray(coms.json) ? coms.json : [];
  const unpaid = comsArr.find((c) => !/paid|pagad/i.test(c.status || ''));
  assert(coms.status === 200 && comsArr.length >= 1 && unpaid, 'GET /commissions (admin >=1 pendiente)', `count=${comsArr.length}`);
  const pay = await api('PATCH', `/commissions/${unpaid.id}/pay`, { token: admin.token });
  assert(pay.status === 200, `PATCH /commissions/${unpaid.id}/pay (admin)`, `status=${pay.status} ${JSON.stringify(pay.json).slice(0, 60)}`);
  const coms2 = await api('GET', '/commissions', { token: admin.token });
  const paidC = (Array.isArray(coms2.json) ? coms2.json : []).find((c) => Number(c.id) === Number(unpaid.id));
  assert(paidC && /paid|pagad/i.test(paidC.status || ''), 'comision refleja Paid', `status=${paidC && paidC.status}`);

  // [4] Reassign (admin): crear propiedad como agente -> reassign a agent2 -> devolver
  console.log('\n[4] Reassign entre agentes (admin)');
  const pf = new FormData();
  pf.set('id', '0'); pf.set('name', 'Casa Reassign Admin'); pf.set('price', '180000'); pf.set('currency', 'DOP');
  pf.set('rooms', '3'); pf.set('bathrooms', '2'); pf.set('sizeInMeters', '200'); pf.set('description', 'reassign test');
  pf.set('propertyTypeId', String(ptype.id)); pf.set('saleTypeId', String(stype.id));
  pf.set('provinceId', String(province.id)); pf.set('municipalityId', String(munic.id));
  pf.set('sector', 'Sector R'); pf.set('fullAddress', 'Calle R 1'); pf.set('latitude', '18.5'); pf.set('longitude', '-69.9');
  pf.set('montoSeparacion', '1000'); pf.set('porcentajeInicialRequerido', '10'); pf.set('isFinanciable', 'true'); pf.set('isFeatured', 'false');
  pf.append('files', new Blob([tinyPng()], { type: 'image/png' }), 'i.png');
  const c1 = await api('POST', '/properties', { token: agent.token, raw: pf });
  assert(c1.status === 200 || c1.status === 201, 'POST /properties (agente, para reassign)', `status=${c1.status}`);
  const pMine = await api('GET', '/properties/my-properties', { token: agent.token });
  const rProp = (Array.isArray(pMine.json) ? pMine.json : []).find((p) => p.name === 'Casa Reassign Admin');
  assert(!!rProp, 'propiedad creada visible (agente)', `found=${!!rProp}`);
  const ra = await api('POST', `/properties/${rProp.id}/reassign`, { token: admin.token, body: { newAgentId: agent2.id } });
  assert(ra.status === 200, `POST /properties/${rProp.id}/reassign (admin) -> agent2`, `status=${ra.status} ${JSON.stringify(ra.json).slice(0, 60)}`);
  const a2mine = await api('GET', '/properties/my-properties', { token: agent2.token });
  assert((Array.isArray(a2mine.json) ? a2mine.json : []).some((p) => p.id === rProp.id), 'propiedad aparece en my-properties de agent2', `status=${a2mine.status}`);
  const rb = await api('POST', `/properties/${rProp.id}/reassign`, { token: admin.token, body: { newAgentId: agent.id } });
  assert(rb.status === 200, 'reassign de vuelta a agent', `status=${rb.status}`);

  // [5] Usuarios: agentes/admins/desarrolladores + toggle status + crear
  console.log('\n[5] Usuarios (agentes/admins/developers + toggle + creacion)');
  const ags = await api('GET', '/admin/agents', { token: admin.token });
  const agsArr = Array.isArray(ags.json) ? ags.json : [];
  assert(ags.status === 200 && agsArr.length >= 1, 'GET /admin/agents', `count=${agsArr.length}`);
  const tg = await api('PATCH', `/admin/agents/${agent2.id}/toggle-status`, { token: admin.token, raw: JSON.stringify(false), extraHeaders: { 'Content-Type': 'application/json' } });
  assert(tg.status === 200, `PATCH /admin/agents/${agent2.id}/toggle-status=false`, `status=${tg.status}`);
  const tg2 = await api('PATCH', `/admin/agents/${agent2.id}/toggle-status`, { token: admin.token, raw: JSON.stringify(true), extraHeaders: { 'Content-Type': 'application/json' } });
  assert(tg2.status === 200, 'toggle-status de vuelta a activo', `status=${tg2.status}`);
  const ads = await api('GET', '/admin/admins', { token: admin.token });
  assert(ads.status === 200 && (ads.json || []).length >= 1, 'GET /admin/admins', `count=${(ads.json || []).length}`);
  const devs = await api('GET', '/admin/developers', { token: admin.token });
  assert(devs.status === 200, 'GET /admin/developers', `status=${devs.status} count=${(devs.json || []).length}`);
  const devName = `dev_matrix_${Date.now()}`;
  const cd = await api('POST', '/admin/developers', { token: admin.token, body: { firstName: 'Dev', lastName: 'Matriz', userName: devName, email: `${devName}@test.do`, password: 'Dev123456!', confirmPassword: 'Dev123456!' } });
  assert(cd.status === 200 || cd.status === 201, 'POST /admin/developers (crear)', `status=${cd.status} ${JSON.stringify(cd.json).slice(0, 60)}`);
  const devs2 = await api('GET', '/admin/developers', { token: admin.token });
  assert((devs2.json || []).some((d) => (d.userName || d.username) === devName), 'developer creado en listado');
  const admName = `admin_matrix_${Date.now()}`;
  const ca = await api('POST', '/admin/admins', { token: admin.token, body: { firstName: 'Admin', lastName: 'Matriz', userName: admName, email: `${admName}@test.do`, password: 'Admin123456!', confirmPassword: 'Admin123456!' } });
  assert(ca.status === 200 || ca.status === 201, 'POST /admin/admins (crear)', `status=${ca.status} ${JSON.stringify(ca.json).slice(0, 60)}`);
  const ads2 = await api('GET', '/admin/admins', { token: admin.token });
  assert((ads2.json || []).some((u) => (u.userName || u.username) === admName), 'admin creado en listado');

  // [6] Catalogos CRUD (propertytypes / saletypes / improvements)
  console.log('\n[6] Catalogos CRUD');
  async function catalogCrud(basePath, stub) {
    const nm1 = `${stub} Mtx ${Date.now()}`;
    const mk = await api('POST', basePath, { token: admin.token, body: { name: nm1, description: 'creado por matriz admin' } });
    assert(mk.status === 200 || mk.status === 201, `POST ${basePath} (crear)`, `status=${mk.status}`);
    const id = mk.json && (mk.json.id || mk.json.Id);
    assert(!!id, `POST ${basePath} devuelve id`, `id=${id}`);
    const up = await api('PUT', `${basePath}/${id}`, { token: admin.token, body: { name: `${nm1}-edit`, description: 'actualizado' } });
    assert(up.status === 200, `PUT ${basePath}/${id}`, `status=${up.status}`);
    const del = await api('DELETE', `${basePath}/${id}`, { token: admin.token });
    assert(del.status === 200 || del.status === 204, `DELETE ${basePath}/${id}`, `status=${del.status}`);
  }
  await catalogCrud('/propertytypes', 'TipoCasa');
  await catalogCrud('/saletypes', 'VentaMtx');
  await catalogCrud('/improvements', 'MejoraMtx');

  // [7] Reviews global (admin)
  console.log('\n[7] Reviews (listado global)');
  const revs = await api('GET', '/reviews', { token: admin.token });
  const revsArr = Array.isArray(revs.json) ? revs.json : [];
  assert(revs.status === 200 && revsArr.length >= 1, 'GET /reviews (>=1)', `status=${revs.status} count=${revsArr.length}`);

  console.log('\n======== RESULTADOS ROL ADMIN ========');
  console.log(`PASS ${PASS}  FAIL ${FAIL}  SKIP ${SKIP}`);
  if (failures.length) { console.log('FALLAS:'); failures.forEach((f) => console.log(`  - ${f}`)); }
  else console.log('MATRIZ ADMIN COMPLETA EN VERDE');
  if (process.argv[2] === '--exit-code') process.exit(FAIL === 0 ? 0 : 1);
})().catch((e) => { console.error('FATAL', e); process.exit(2); });