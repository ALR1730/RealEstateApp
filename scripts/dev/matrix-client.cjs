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

async function api(method, path, { token, body, raw } = {}) {
  const headers = {};
  if (token) headers.Authorization = `Bearer ${token}`;
  let payload;
  if (raw !== undefined) { payload = raw; }
  else if (body !== undefined) { headers['Content-Type'] = 'application/json'; payload = JSON.stringify(body); }
  const r = await fetch(`${base}${path}`, { method, headers, body: payload });
  let json = null;
  try { const t = await r.text(); json = t ? JSON.parse(t) : null; } catch { json = null; }
  return { status: r.status, json };
}

(async () => {
  console.log('=== MATRIZ FUNCIONAL ROL CLIENT ===');

  const creds = {
    client: ['client@realestate.com', 'Client123!'],
    client2: ['client2@realestate.com', 'Client123!'],
    agent: ['agent@realestate.com', 'Agent123!'],
  };
  const client = await auth(...creds.client);
  const client2 = await auth(...creds.client2);
  const agent = await auth(...creds.agent);

  // ---- datapreparacion: propiedad disponible del agente demo
  const cats = await api('GET', '/properties');
  assert(cats.status === 200 && Array.isArray(cats.json), 'GET /properties catalogo', `status=${cats.status}`);
  const agentList = await api('GET', '/agents');
  const theAgent = (agentList.json || []).find((a) => a.id === agent.id) || (agentList.json || [])[0];
  const agentId = (theAgent && (theAgent.id || theAgent.agentId)) || agent.id;
  assert(!!theAgent, 'GET /agents lista publica', 'no agentes');

const pool = (Array.isArray(cats.json) ? cats.json : []).filter((p) => p.agentId === agentId);
  const target = pool.find((p) => /dispon|available/i.test(p.status || '')) || pool[0];
  if (!target) {
    console.error(`\nFATAL: no hay propiedad del agente (${agentId}) en el catalogo (todas vendidas?). Reinicia API + seed para estado limpio.`);
    process.exit(2);
  }
  const pid = target.id;
  const price = Number(target.price);
  info('propiedad', `${target.name} ($ ${price}) status=${target.status}`);

  // ===================== 1. CUENTA / PERFIL / ACTIVIDAD =====================
  console.log('\n[1] Cuenta, Perfil, Actividad');
  const profile = await api('GET', '/account/profile', { token: client.token });
  assert(profile.status === 200 && (profile.json.userName || profile.json.email), 'GET /account/profile', `status=${profile.status}`);
  const profileForm = new FormData();
  profileForm.set('id', client.id);
  profileForm.set('firstName', profile.json.firstName || profile.json.userName || 'Cliente');
  profileForm.set('lastName', profile.json.lastName || 'Apellido');
  profileForm.set('email', profile.json.email);
  profileForm.set('userName', profile.json.userName);
  profileForm.set('phone', profile.json.phone || '');
  const up = await api('PUT', '/account/profile', { token: client.token, raw: profileForm });
  assert(up.status === 200, 'PUT /account/profile (multipart)', `status=${up.status} ${JSON.stringify(up.json).slice(0, 80)}`);
  const act = await api('GET', '/account/activity', { token: client.token });
  assert(act.status === 200 && Array.isArray(act.json), 'GET /account/activity', `status=${act.status}`);
  info('activity items', Array.isArray(act.json) ? act.json.length : 'n/a');

  // ===================== 2. MONEDA =====================
  console.log('\n[2] Tipo de cambio');
  const cur = await api('GET', '/currency/rates');
  assert(cur.status === 200, 'GET /currency/rates publico', `status=${cur.status}`);

  // ===================== 3. FAVORITOS =====================
  console.log('\n[3] Favoritos');
  const fav = await api('POST', `/favorites/${pid}`, { token: client.token });
  assert(fav.status === 200 || fav.status === 201, 'POST /favorites/{id}', `status=${fav.status}`);
  const favCheck = await api('GET', `/favorites/check/${pid}`, { token: client.token });
  assert(favCheck.status === 200, 'GET /favorites/check/{id}', `status=${favCheck.status} ${JSON.stringify(favCheck.json).slice(0, 60)}`);
  const favList = await api('GET', '/favorites', { token: client.token });
  assert(favList.status === 200 && (Array.isArray(favList.json) || (favList.json && favList.json.data)), 'GET /favorites', `status=${favList.status}`);
  const favItems = Array.isArray(favList.json) ? favList.json : (favList.json && favList.json.data) || [];
  assert(favItems.some((f) => (f.propertyId || f.id) == Number(pid) || (f.property && f.property.id == Number(pid))), 'favoritos incluye propiedad', JSON.stringify(favItems[0] || {}).slice(0, 80));
  const favDel = await api('DELETE', `/favorites/${pid}`, { token: client.token });
  assert(favDel.status === 200, 'DELETE /favorites/{id}', `status=${favDel.status}`);

  // ===================== 4. BUSQUEDAS GUARDADAS =====================
  console.log('\n[4] Busquedas guardadas');
const ss = await api('POST', '/savedsearches', {
    token: client.token,
    body: { name: 'Casa en Cap Cana', minPrice: 300000, maxPrice: 600000, emailAlertsEnabled: true },
  });
  assert(ss.status === 200 || ss.status === 201, 'POST /savedsearches', `status=${ss.status}`);
  const ssList = await api('GET', '/savedsearches', { token: client.token });
  const ssArr = Array.isArray(ssList.json) ? ssList.json : (ssList.json && ssList.json.data) || [];
  const ssCreated = ssArr.find((s) => /cap cana/i.test(s.name || ''));
  const ssId = (ssCreated && (ssCreated.id || ssCreated.Id)) || 0;
  assert(ssList.status === 200 && !!ssCreated, 'GET /savedsearches incluye la creada (con id real)', `status=${ssList.status} ids=${ssArr.map((x) => x.id).join(',')}`);
  const ssTog = await api('PATCH', `/savedsearches/${ssId}/toggle-alerts`, { token: client.token });
  assert(ssTog.status === 200, 'PATCH /savedsearches/{id}/toggle-alerts', `status=${ssTog.status} ${JSON.stringify(ssTog.json).slice(0, 80)}`);
  const ssDel = await api('DELETE', `/savedsearches/${ssId}`, { token: client.token });
  assert(ssDel.status === 200, 'DELETE /savedsearches/{id}', `status=${ssDel.status}`);

  // ===================== 5. RESEÃ‘AS =====================
  console.log('\n[5] ReseÃ±as');
const hasR = await api('GET', `/reviews/has-reviewed?agentId=${agentId}&propertyId=${pid}`, { token: client.token });
  assert(hasR.status === 200, 'GET /reviews/has-reviewed', `status=${hasR.status} ${JSON.stringify(hasR.json).slice(0, 60)}`);
  const canR = await api('GET', `/reviews/can-review?agentId=${agentId}&propertyId=${pid}`, { token: client.token });
  assert(canR.status === 200, 'GET /reviews/can-review', `status=${canR.status} ${JSON.stringify(canR.json).slice(0, 60)}`);
  info('gate reviews (antes de transaccion)', `canReview=${JSON.stringify(canR.json).slice(0, 60)}`);
  const sum = await api('GET', `/reviews/agent/${agentId}/summary`);
  assert(sum.status === 200, 'GET /reviews/agent/{id}/summary', `status=${sum.status}`);
  info('summary', JSON.stringify(sum.json).slice(0, 80));

  // ===================== 6. BUY-ABILITY =====================
  console.log('\n[6] Capacidad de compra');
  const buy = await api('POST', '/buyability/evaluate', {
    token: client.token,
    body: { monthlyGrossIncome: 150000, monthlyNetIncome: 120000, monthlyDebtPayments: 10000, availableDownPayment: 2000000, currency: 'DOP' },
  });
  assert(buy.status === 200 && buy.json && buy.json.maxPropertyPrice > 0, 'POST /buy-ability/evaluate', `status=${buy.status} ${JSON.stringify(buy.json).slice(0, 90)}`);
  const buyLast = await api('GET', '/buyability/last', { token: client.token });
  assert(buyLast.status === 200, 'GET /buy-ability/last', `status=${buyLast.status}`);

  // ===================== 7. CHATS =====================
  console.log('\n[7] Chats');
  const conv = await api('GET', '/chats/conversations', { token: client.token });
  assert(conv.status === 200 && Array.isArray(conv.json), 'GET /chats/conversations (client)', `status=${conv.status}`);
  info('conversaciones', Array.isArray(conv.json) ? conv.json.length : 'n/a');
  const thread = await api('GET', `/chats/thread?clientId=${client.id}&agentId=${agentId}&propertyId=${pid}`, { token: client.token });
  assert(thread.status === 200 && Array.isArray(thread.json), 'GET /chats/thread', `status=${thread.status} ${JSON.stringify(thread.json).slice(0, 60)}`);
  const send = await api('POST', '/chats/send', { token: client.token, body: { recipientId: agentId, propertyId: pid, messageContent: 'Hola, me interesa la propiedad' } });
  assert(send.status === 200 || send.status === 201, 'POST /chats/send', `status=${send.status}`);
  const thread2 = await api('GET', `/chats/thread?clientId=${client.id}&agentId=${agentId}&propertyId=${pid}`, { token: client.token });
  const msgs = Array.isArray(thread2.json) ? thread2.json : [];
  assert(msgs.some((mm) => /interesa/.test(mm.content || mm.messageContent || '')), 'mensaje persiste en hilo', `total=${msgs.length}`);

  // ===================== 8. CITAS (crear) =====================
  console.log('\n[8] Citas (solicitud cliente)');
  const appt = await api('POST', '/appointments', {
    token: client.token,
    body: { propertyId: pid, appointmentDate: new Date(Date.now() + 86400000).toISOString(), comments: 'Visita de fin de semana' },
  });
  assert(appt.status === 200 || appt.status === 201 || appt.status === 400, 'POST /appointments', `status=${appt.status} ${JSON.stringify(appt.json).slice(0, 100)}`);
  const myAppts = await api('GET', '/appointments/my-appointments', { token: client.token });
  assert(myAppts.status === 200 && Array.isArray(myAppts.json), 'GET /appointments/my-appointments', `status=${myAppts.status}`);

  // ===================== 9. OFERTAS (flujo completo cross-rol) =====================
  console.log('\n[9] Ofertas: full lifecycle con contra-oferta y cascade-reject');
  const amt1 = Math.round(price * 1.05);
  const amt2 = Math.round(price * 1.12);
  const counterAmt = Math.round(price * 1.09);
  const o1 = await api('POST', '/offers', { token: client.token, body: { propertyId: pid, montoOfertado: amt1, notes: 'Oferta de client1' } });
  assert(o1.status === 200 || o1.status === 201, 'POST /offers (client1)', `status=${o1.status} ${JSON.stringify(o1.json).slice(0, 90)}`);
  const o1id = (o1.json && (o1.json.id || o1.json.Id)) || 0;
  assert(o1id, 'offer id devuelto', 'no id');
  const o2 = await api('POST', '/offers', { token: client2.token, body: { propertyId: pid, montoOfertado: amt2, notes: 'Oferta de client2' } });
  assert(o2.status === 200 || o2.status === 201, 'POST /offers (client2)', `status=${o2.status}`);
  const o2id = (o2.json && (o2.json.id || o2.json.Id)) || 0;

  const my1 = await api('GET', '/offers/my-offers', { token: client.token });
  const my1Arr = Array.isArray(my1.json) ? my1.json : [];
  assert(my1.status === 200 && my1Arr.some((x) => Number(x.id) === Number(o1id) && /pending/i.test(x.status)), 'GET my-offers client1 -> Oferta Pending', `status=${my1.status} ${JSON.stringify(my1Arr.find((x) => Number(x.id) === Number(o1id)) || {}).slice(0, 90)}`);

  const recv = await api('GET', '/offers/received', { token: agent.token });
  const recvArr = Array.isArray(recv.json) ? recv.json : [];
  assert(recv.status === 200 && recvArr.some((x) => Number(x.id) === Number(o1id)) && recvArr.some((x) => Number(x.id) === Number(o2id)), 'GET received (agent) ve ambas', `status=${recv.status}`);
  const revO1 = recvArr.find((x) => Number(x.id) === Number(o1id));

  const co = await api('POST', `/offers/${o1id}/counter-offer`, { token: agent.token, body: { counterAmount: counterAmt, counterMessage: 'Podemos en $' + counterAmt } });
  assert(co.status === 200, 'POST /offers/{id}/counter-offer (agent)', `status=${co.status} ${JSON.stringify(co.json).slice(0, 80)}`);

  const my1b = await api('GET', '/offers/my-offers', { token: client.token });
  const my1bArr = Array.isArray(my1b.json) ? my1b.json : [];
  const o1now = my1bArr.find((x) => Number(x.id) === Number(o1id));
  assert(o1now && /counter|contra/i.test(o1now.status), 'client1 ve contra-oferta (CounterOffered)', `status=${o1now && o1now.status}`);

  const ac = await api('POST', `/offers/${o1id}/accept-counter`, { token: client.token });
  assert(ac.status === 200, 'POST /offers/{id}/accept-counter (client1)', `status=${ac.status} ${JSON.stringify(ac.json).slice(0, 80)}`);

  const recv2 = await api('GET', '/offers/received', { token: agent.token });
  const recv2Arr = Array.isArray(recv2.json) ? recv2.json : [];
  const r1 = recv2Arr.find((x) => Number(x.id) === Number(o1id));
const r2 = recv2Arr.find((x) => Number(x.id) === Number(o2id));
  assert(r1 && /accepted|aceptada/i.test(r1.status), 'oferta1 = Accepted', `status=${r1 && r1.status}`);
  assert(r2 && /rejected|rechazada/i.test(r2.status), 'oferta2 = Rejected (cascade atomico)', `status=${r2 && r2.status}`);

  const o3 = await api('POST', '/offers', { token: client.token, body: { propertyId: pid, montoOfertado: amt1 } });
  assert(o3.status >= 400, 'nueva oferta sobre vendida -> rechazada (4xx)', `status=${o3.status} ${JSON.stringify(o3.json).slice(0, 80)}`);

  const myProps = await api('GET', '/properties/my-properties', { token: agent.token });
  const myPropsArr = Array.isArray(myProps.json) ? myProps.json : [];
  const sold = myPropsArr.find((p) => Number(p.id) === Number(pid));
  assert(sold && /vendid|sold/i.test(sold.status || ''), 'propiedad paso a Vendida (vista agente)', `status=${sold && sold.status} found=${!!sold}`);

  const rev = await api('POST', '/reviews', { token: client.token, body: { agentId, propertyId: pid, rating: 5, comment: 'Excelente atencion' } });
  assert(rev.status === 200 || rev.status === 201, 'POST /reviews tras transaccion completada', `status=${rev.status} ${JSON.stringify(rev.json).slice(0, 100)}`);
  const sum2 = await api('GET', `/reviews/agent/${agentId}/summary`);
  const avg2 = sum2.json && sum2.json.averageRating;
  assert(sum2.status === 200 && avg2 > 0, 'summary refleja nueva reseña', `status=${sum2.status} avg=${avg2}`);

  // ===================== RESUMEN =====================
  console.log('\n======== RESULTADOS ROL CLIENT ========');
  console.log(`PASS ${PASS}  FAIL ${FAIL}  SKIP ${SKIP}`);
  if (failures.length) { console.log('FALLAS:'); failures.forEach((f) => console.log('  - ' + f)); process.exit(1); }
  console.log('MATRIZ CLIENT COMPLETA EN VERDE');

  const fs = require('fs');
  const path = require('path');
  fs.writeFileSync(path.join(__dirname, 'matrix-client-report.txt'), JSON.stringify({ pass: PASS, fail: FAIL, skip: SKIP }, null, 2), 'utf8');
})().catch((e) => { console.error('FATAL', e); process.exit(1); });
