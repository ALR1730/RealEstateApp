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
  if (raw !== undefined) { payload = raw; }
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
  console.log('=== MATRIZ FUNCIONAL ROL AGENT ===');

  const agent = await auth('agent@realestate.com', 'Agent123!');
  const agent2 = await auth('agent2@realestate.com', 'Agent123!');
  const client = await auth('client@realestate.com', 'Client123!');
  const client2 = await auth('client2@realestate.com', 'Client123!');

  // ---------- catalogs basales ----------
  const provinces = await api('GET', '/provinces');
  const province = (provinces.json || [])[0];
  assert(!!province, 'GET /provinces', `count=${(provinces.json || []).length}`);
  const muns = await api('GET', `/provinces/${province.id}/municipalities`);
  const munic = (muns.json || [])[0];
  assert(!!munic, `GET /provinces/${province.id}/municipalities`, `count=${(muns.json || []).length}`);
  const types = await api('GET', '/propertytypes');
  const ptype = (types.json || [])[0];
  const sales = await api('GET', '/saletypes');
  const stype = (sales.json || [])[0];
  assert(!!ptype && !!stype, 'GET /propertytypes /saletypes', `pt=${ptype && ptype.id} st=${stype && stype.id}`);
  const plans = await api('GET', '/subscriptions/plans');
  assert(plans.status === 200 && (plans.json || []).length > 0, 'GET /subscriptions/plans', `status=${plans.status}`);

  // ---------- propiedad objetivo (Disponible del agente) ----------
  const mine0 = await api('GET', '/properties/my-properties', { token: agent.token });
  const mineArr = Array.isArray(mine0.json) ? mine0.json : [];
  assert(mine0.status === 200 && mineArr.length > 0, 'GET /properties/my-properties', `count=${mineArr.length}`);
  const objetivo = mineArr.find((p) => /dispon|available/i.test(p.status || '')) || mineArr[0];
  assert(!!objetivo, 'propiedad objetivo Disponible', `pid=${objetivo && objetivo.id} status=${objetivo && objetivo.status}`);
  const oid = objetivo.id;
  info('objetivo', `${objetivo.name} ($ ${objetivo.price})`);

  // ===================== 1. CRUD propia propiedad (multipart) =====================
  console.log('\n[1] Propiedades: crear / actualizar / price-history');
  async function propForm(over = {}) {
    const f = new FormData();
    const base_ = {
      id: 0, name: 'Apartamento Matriz Agente', price: 250000, currency: 'DOP', rooms: 2, bathrooms: 2,
      sizeInMeters: 170, description: 'Creacion por matriz funcional rol agent', propertyTypeId: ptype.id,
      saleTypeId: stype.id, provinceId: province.id, municipalityId: munic.id, sector: 'Sector Prueba',
      fullAddress: 'Calle Prueba 123', latitude: 18.4732, longitude: -69.9124, montoSeparacion: 1500,
      porcentajeInicialRequerido: 10, isFinanciable: true, isFeatured: false,
    };
    Object.assign(base_, over);
    for (const [k, v] of Object.entries(base_)) f.set(k, String(v));
    f.append('files', new Blob([tinyPng()], { type: 'image/png' }), 'img1.png');
    return f;
  }
  const cr = await api('POST', '/properties', { token: agent.token, raw: await propForm() });
  assert(cr.status === 200 || cr.status === 201, 'POST /properties (multipart crear)', `status=${cr.status} ${JSON.stringify(cr.json).slice(0, 90)}`);
  const cr1Price = 250000;
  const mine1 = await api('GET', '/properties/my-properties', { token: agent.token });
  const mine1Arr = Array.isArray(mine1.json) ? mine1.json : [];
  const cr1 = mine1Arr.find((p) => p.name === 'Apartamento Matriz Agente');
  assert(!!cr1, 'nueva propiedad visible en my-properties', `found=${!!cr1}`);

  const newPrice = 275000;
  const up = await api('PUT', `/properties/${cr1.id}`, { token: agent.token, raw: await propForm({ id: cr1.id, name: 'Apartamento Matriz Agente', price: newPrice }) });
  assert(up.status === 200, `PUT /properties/${cr1.id} (precio ${newPrice})`, `status=${up.status}`);
  const mine2 = await api('GET', '/properties/my-properties', { token: agent.token });
  const mine2Arr = Array.isArray(mine2.json) ? mine2.json : [];
  const cr1b = mine2Arr.find((p) => p.id === cr1.id);
  assert(Number(cr1b && cr1b.price) === newPrice, 'precio actualizado en my-properties', `price=${cr1b && cr1b.price}`);
  const ph = await api('GET', `/properties/${cr1.id}/price-history`);
  const phArr = Array.isArray(ph.json) ? ph.json : [];
  assert(ph.status === 200 && phArr.length >= 1, `GET /properties/${cr1.id}/price-history (registro)`, `count=${phArr.length} ${JSON.stringify(phArr[0] || {}).slice(0, 60)}`);

  // ===================== 2. Featured =====================
  console.log('\n[2] Featured (toggle)');
  const tf = await api('POST', `/properties/${cr1.id}/toggle-featured?durationDays=15`, { token: agent.token });
  const tfOk = tf.status === 200 || (tf.status === 400 && /l[ií]mite|l[ií]mite de|featured|suscripci|plan/i.test(JSON.stringify(tf.json)));
  assert(tfOk, 'POST /properties/{id}/toggle-featured', `status=${tf.status} ${JSON.stringify(tf.json).slice(0, 90)}`);
  if (tf.status === 400) info('business rule featured', JSON.stringify(tf.json).slice(0, 120));

  // ===================== 3. Documentos =====================
  console.log('\n[3] Documentos (subir/listar/eliminar)');
  const docForm = new FormData();
  docForm.set('propertyId', String(cr1.id));
  docForm.set('documentType', 'Título de propiedad');
  docForm.append('file', new Blob([Buffer.from('titulo-provisional')], { type: 'application/pdf' }), 'titulo.pdf');
  const d1 = await api('POST', '/documents', { token: agent.token, raw: docForm });
  assert(d1.status === 200 || d1.status === 201, 'POST /documents (multipart)', `status=${d1.status} ${JSON.stringify(d1.json).slice(0, 80)}`);
  const docs = await api('GET', `/documents/property/${cr1.id}`, { token: agent.token });
  const docArr = Array.isArray(docs.json) ? docs.json : [];
  const doc = docArr.find((x) => x.propertyId === cr1.id) || docArr[0];
  assert(docs.status === 200 && !!doc, `GET /documents/property/${cr1.id}`, `count=${docArr.length}`);
  const ddel = await api('DELETE', `/documents/${doc.id}`, { token: agent.token });
  assert(ddel.status === 200, `DELETE /documents/${doc.id}`, `status=${ddel.status}`);

  // ===================== 4. KYC (verificacion como agente) =====================
  console.log('\n[4] Verificacion KYC (submit / my-status)');
  const ks = await api('GET', '/verifications/my-status', { token: agent.token });
  assert(ks.status === 200, 'GET /verifications/my-status', `status=${ks.status} ${JSON.stringify(ks.json).slice(0, 70)}`);
  const ksf = new FormData();
  ksf.set('Cedula', '001-1234567-1');
  ksf.append('CedulaFrontImage', new Blob([tinyPng()], { type: 'image/png' }), 'ci-front.png');
  ksf.append('CedulaBackImage', new Blob([tinyPng()], { type: 'image/png' }), 'ci-back.png');
  const ksub = await api('POST', '/verifications/submit', { token: agent2.token, raw: ksf });
  const ksubOk = ksub.status === 200 || ksub.status === 201 || (ksub.status === 400 && /ya existe|ya se|pendiente|existe|ya env/i.test(JSON.stringify(ksub.json)));
  assert(ksubOk, 'POST /verifications/submit (agent2 KYC)', `status=${ksub.status} ${JSON.stringify(ksub.json).slice(0, 100)}`);
  const ks2 = await api('GET', '/verifications/my-status', { token: agent2.token });
  assert(ks2.status === 200, 'GET /verifications/my-status (agent2)', `status=${ks2.status} ${JSON.stringify(ks2.json).slice(0, 70)}`);

  // ===================== 5. Citas: solicitud clientes -> agent confirm/cancel =====================
  console.log('\n[5] Citas (request de clientes, agent confirm/cancel)');
  const appt1 = await api('POST', '/appointments', { token: client.token, body: { propertyId: oid, appointmentDate: new Date(Date.now() + 86400000).toISOString(), comments: 'Visita cliente A' } });
  const appt1Id = appt1.json && (appt1.json.id || appt1.json.appointmentId);
  assert(appt1.status === 200 && !!appt1Id, 'POST /appointments (client A)', `status=${appt1.status} id=${appt1Id}`);
  const appt2 = await api('POST', '/appointments', { token: client2.token, body: { propertyId: oid, appointmentDate: new Date(Date.now() + 2 * 86400000).toISOString(), comments: 'Visita cliente B' } });
  const appt2Id = appt2.json && (appt2.json.id || appt2.json.appointmentId);
  assert(appt2.status === 200 && !!appt2Id, 'POST /appointments (client B)', `status=${appt2.status} id=${appt2Id}`);
  const agAppts = await api('GET', '/appointments/my-appointments', { token: agent.token });
  const agApptsArr = Array.isArray(agAppts.json) ? agAppts.json : [];
  assert(agAppts.status === 200 && agApptsArr.some((a) => Number(a.id) === Number(appt1Id)), 'GET my-appointments (agent ve solicitudes)', `count=${agApptsArr.length}`);
  const conf = await api('PATCH', `/appointments/${appt1Id}/confirm`, { token: agent.token, raw: JSON.stringify('Confirmada por agente'), extraHeaders: { 'Content-Type': 'application/json' } });
  assert(conf.status === 200, `PATCH /appointments/${appt1Id}/confirm`, `status=${conf.status}`);
  const canc = await api('PATCH', `/appointments/${appt2Id}/cancel`, { token: agent.token, raw: JSON.stringify('No disponible'), extraHeaders: { 'Content-Type': 'application/json' } });
  assert(canc.status === 200, `PATCH /appointments/${appt2Id}/cancel`, `status=${canc.status}`);
  const apptsAfter = await api('GET', '/appointments/my-appointments', { token: agent.token });
  const apptsAfterArr = Array.isArray(apptsAfter.json) ? apptsAfter.json : [];
  const a1 = apptsAfterArr.find((a) => Number(a.id) === Number(appt1Id));
  const a2 = apptsAfterArr.find((a) => Number(a.id) === Number(appt2Id));
  assert(a1 && /confirm|acept|approved/i.test(a1.status || ''), 'cita1 = Confirmada', `status=${a1 && a1.status}`);
  assert(a2 && /cancel|declin/i.test(a2.status || ''), 'cita2 = Cancelada', `status=${a2 && a2.status}`);

  // ===================== 6. Leads (pipeline) =====================
  console.log('\n[6] Leads (listado / stats / crear)');
  const leads = await api('GET', '/leads', { token: agent.token });
  const leadsArr = Array.isArray(leads.json) ? leads.json : [];
  assert(leads.status === 200, 'GET /leads', `status=${leads.status} count=${leadsArr.length}`);
  const stats = await api('GET', '/leads/stats', { token: agent.token });
  assert(stats.status === 200 && stats.json && typeof stats.json.totalLeads === 'number', 'GET /leads/stats (shape)', `status=${stats.status} ${JSON.stringify(stats.json).slice(0, 60)}`);
  const nl = await api('POST', '/leads', { token: agent.token, body: { leadName: 'Rolando Matriz', leadEmail: 'rolando@test.do', leadPhone: '809-555-0101', priority: 'Alta', source: 'Matriz', propertyId: oid } });
  assert(nl.status === 200 || nl.status === 201, 'POST /leads', `status=${nl.status} ${JSON.stringify(nl.json).slice(0, 70)}`);
  const leads2 = await api('GET', '/leads', { token: agent.token });
  const leads2Arr = Array.isArray(leads2.json) ? leads2.json : [];
  assert(leads2.status === 200 && leads2Arr.length === leadsArr.length + 1, 'lead creado incrementa listado', `antes=${leadsArr.length} despues=${leads2Arr.length}`);
  const stats2 = await api('GET', '/leads/stats', { token: agent.token });
  assert(stats2.status === 200 && Number(stats2.json && stats2.json.totalLeads) >= 1, 'leads/stats refleja el lead creado', `totalLeads=${stats2.json && stats2.json.totalLeads}`);

  // ===================== 7. AVM (tasacion) =====================
  console.log('\n[7] AVM valuation (+ last)');
  const avm = await api('GET', `/valuation/properties/${oid}/valuation?searchRadiusKm=5`, { token: agent.token });
  assert(avm.status === 200, `GET /valuation/properties/${oid}/valuation`, `status=${avm.status}`);
  const last = await api('GET', `/valuation/properties/${oid}/valuation/last`, { token: agent.token });
  assert(last.status === 200 || last.status === 204, `GET /valuation/properties/${oid}/valuation/last`, `status=${last.status}`);

  // ===================== 8. Reassign (solo Admin; negar a Agente) =====================
  console.log('\n[8] Reassign (regla: Admin-only, agente bloqueado)');
  const ra = await api('POST', `/properties/${cr1.id}/reassign`, { token: agent.token, body: { newAgentId: agent2.id } });
  assert(ra.status === 403, 'POST /properties/{id}/reassign (agente) -> 403 denied', `status=${ra.status} ${JSON.stringify(ra.json).slice(0, 60)}`);
  info('reassign pasara por Admin (matriz rol Admin)', 'endpoint [Authorize(Roles="Admin")]');

  // ===================== 9. Ofertas recibidas + aceptar + comision + pago =====================
  console.log('\n[9] Ofertas (aceptar directo) + comision + pago');
  const offerAmt = Math.round(Number(objetivo.price) * 0.92);
  const of = await api('POST', '/offers', { token: client2.token, body: { propertyId: oid, montoOfertado: offerAmt, notes: 'Oferta del cliente B' } });
  const ofId = of.json && (of.json.id || of.json.offerId);
  assert(of.status === 200 && !!ofId, 'POST /offers (client2)', `status=${of.status} id=${ofId}`);
  const recv = await api('GET', '/offers/received', { token: agent.token });
  const recvArr = Array.isArray(recv.json) ? recv.json : [];
  assert(recvArr.some((x) => Number(x.id) === Number(ofId)), 'GET /offers/received ve la nueva', `count=${recvArr.length}`);
  const acc = await api('POST', `/offers/${ofId}/accept`, { token: agent.token });
  assert(acc.status === 200, `POST /offers/${ofId}/accept (agente)`, `status=${acc.status} ${JSON.stringify(acc.json).slice(0, 70)}`);
  const mineS = await api('GET', '/properties/my-properties', { token: agent.token });
  const mineSArr = Array.isArray(mineS.json) ? mineS.json : [];
  const sold = mineSArr.find((p) => Number(p.id) === Number(oid));
  assert(sold && /vendid|sold/i.test(sold.status || ''), 'propiedad objetivo Vendida (vista agente)', `status=${sold && sold.status}`);
  const of2 = await api('POST', '/offers', { token: client.token, body: { propertyId: oid, montoOfertado: offerAmt - 1000, notes: 'tarde' } });
  assert(of2.status >= 400, 'nueva oferta sobre vendida -> 4xx', `status=${of2.status} ${JSON.stringify(of2.json).slice(0, 70)}`);

  const coms = await api('GET', '/commissions/my-commissions', { token: agent.token });
  const comsArr = Array.isArray(coms.json) ? coms.json : [];
  const unpaid = comsArr.find((c) => !/paid|paid|pagad/i.test(c.status || ''));
  assert(coms.status === 200 && comsArr.length >= 1 && unpaid, 'GET /commissions/my-commissions (>=1 pendiente)', `count=${comsArr.length}`);
  const csum = await api('GET', '/commissions/my-commissions/summary', { token: agent.token });
  const ctotal = csum.json && (csum.json.totalCount ?? csum.json.totalAmount);
  assert(csum.status === 200 && Number(ctotal) > 0, 'GET /commissions/my-commissions/summary', `status=${csum.status} ${JSON.stringify(csum.json).slice(0, 80)}`);
  const pay = await api('PATCH', `/commissions/${unpaid.id}/pay`, { token: agent.token });
  assert(pay.status === 403, 'PATCH /commissions/{id}/pay (agente) -> 403 denied', `status=${pay.status} ${JSON.stringify(pay.json).slice(0, 60)}`);
  info('pago de comision pasara por Admin (matriz rol Admin)', 'endpoint admin-only');
  const coms2 = await api('GET', '/commissions/my-commissions', { token: agent.token });
  const coms2Arr = Array.isArray(coms2.json) ? coms2.json : [];
  const pendingC = coms2Arr.find((c) => Number(c.id) === Number(unpaid.id));
  assert(pendingC && !/paid|pagad/i.test(pendingC.status || ''), 'comision sigue Pendiente (pago admin, a analizar)', `status=${pendingC && pendingC.status}`);

  // ===================== resumen =====================
  console.log('\n======== RESULTADOS ROL AGENT ========');
  console.log(`PASS ${PASS}  FAIL ${FAIL}  SKIP ${SKIP}`);
  if (failures.length) { console.log('FALLAS:'); failures.forEach((f) => console.log(`  - ${f}`)); }
  else { console.log('MATRIZ AGENT COMPLETA EN VERDE'); }
  if (process.argv[2] === '--exit-code') process.exit(FAIL === 0 ? 0 : 1);
})().catch((e) => { console.error('FATAL', e); process.exit(2); });