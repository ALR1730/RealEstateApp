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
  return { token: j.jwToken, id: j.id, email: j.email };
}

async function api(method, path, { token, body, raw } = {}) {
  const headers = {};
  if (token) headers.Authorization = `Bearer ${token}`;
  let payload;
  if (raw !== undefined) payload = raw;
  else if (body !== undefined) { headers['Content-Type'] = 'application/json'; payload = JSON.stringify(body); }
  const r = await fetch(`${base}${path}`, { method, headers, body: payload });
  let json = null;
  try { const t = await r.text(); json = t ? JSON.parse(t) : null; } catch { json = null; }
  return { status: r.status, json };
}

function ownerList(json) {
  if (Array.isArray(json)) return json;
  if (json && Array.isArray(json.properties)) return json.properties;
  return [];
}
function tinyPng() {
  return Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==', 'base64');
}
function isAvailable(p) { return /dispon|available/i.test(p.status || ''); }

(async () => {
  console.log('=== MATRIZ FUNCIONAL ROL OWNER ===');
  const owner = await auth('owner@realestate.com', 'Owner123!');

  const provinces = await api('GET', '/provinces');
  const province = (provinces.json || [])[0];
  assert(!!province, 'GET /provinces');
  const muns = await api('GET', `/provinces/${province.id}/municipalities`);
  const munic = (muns.json || [])[0];
  assert(!!munic, `GET /provinces/${province.id}/municipalities`);
  const types = await api('GET', '/propertytypes');
  const ptype = (types.json || [])[0];
  const sales = await api('GET', '/saletypes');
  const stype = (sales.json || [])[0];
  assert(!!ptype && !!stype, 'GET /propertytypes /saletypes');

  async function ownerForm(over = {}) {
    const f = new FormData();
    const base_ = {
      id: 0, name: 'Solar Familiar Matrix', price: 90000, currency: 'DOP', rooms: 1, bathrooms: 1,
      sizeInMeters: 250, description: 'Creada por matriz funcional rol owner', propertyTypeId: ptype.id,
      saleTypeId: stype.id, provinceId: province.id, municipalityId: munic.id, sector: 'Sector Matrix',
      fullAddress: 'Av Matrix 777', latitude: 18.49, longitude: -69.93, montoSeparacion: 800,
      porcentajeInicialRequerido: 5, isFinanciable: false, isFeatured: false,
    };
    Object.assign(base_, over);
    for (const [k, v] of Object.entries(base_)) f.set(k, String(v));
    f.append('files', new Blob([tinyPng()], { type: 'image/png' }), 'im1.png');
    return f;
  }

  // [0] estado base
  console.log('\n[0] Estado base (my-properties + conteo activas)');
  const mine = await api('GET', '/owners/my-properties', { token: owner.token });
  const mineArr = ownerList(mine.json);
  assert(mine.status === 200 && mine.json && mine.json.maxAllowed === 2, 'GET /owners/my-properties (con maxAllowed)', `status=${mine.status} count=${mineArr.length} max=${mine.json && mine.json.maxAllowed}`);
  let available = mineArr.filter(isAvailable);
  info('activas', `${available.length} de ${mineArr.length} (max 2)`);

  // [1] limite: crear hasta tope + rechazo al exceder
  console.log('\n[1] Limite de 2 activas (crear hasta tope + rechazo)');
  const created = [];
  while (available.length < 2) {
    const mk = await api('POST', '/owners/properties', { token: owner.token, raw: await ownerForm({ name: `Solar Matrix ${created.length + 1}` }) });
    assert(mk.status === 200 || mk.status === 201, `POST /owners/properties (activa #${available.length + 1})`, `status=${mk.status}`);
    if (mk.status < 300) {
      created.push(mk.json);
      const refreshed = await api('GET', '/owners/my-properties', { token: owner.token });
      available = ownerList(refreshed.json).filter(isAvailable);
    } else break;
  }
  const over = await api('POST', '/owners/properties', { token: owner.token, raw: await ownerForm({ name: 'Solar EXCEDENTE' }) });
  const limitOk = over.status >= 400 && /m[aá]ximo|l[ií]mite|propiedad/i.test(JSON.stringify(over.json));
  assert(limitOk, 'POST /owners/properties sobre el tope -> rechazo (limite 2)', `status=${over.status} ${JSON.stringify(over.json).slice(0, 90)}`);
  info('mensaje limite', JSON.stringify(over.json).slice(0, 140));
  const mineOver = await api('GET', '/owners/my-properties', { token: owner.token });
  const overAvail = ownerList(mineOver.json).filter(isAvailable);
  assert(overAvail.length <= 2, 'no supera las 2 activas', `activas=${overAvail.length}`);

  // [2] liberar slot -> crear de nuevo
  console.log('\n[2] Liberar slot y volver a publicar');
  const del = await api('DELETE', `/owners/properties/${overAvail[0].id}`, { token: owner.token });
  assert(del.status === 200 || del.status === 204, `DELETE /owners/properties/${overAvail[0].id}`, `status=${del.status}`);
  const afterDel = await api('GET', '/owners/my-properties', { token: owner.token });
  const availAfterDel = ownerList(afterDel.json).filter(isAvailable);
  assert(availAfterDel.length <= 1, 'slot liberado (activas <= 1)', `activas=${availAfterDel.length}`);
  const re = await api('POST', '/owners/properties', { token: owner.token, raw: await ownerForm({ name: 'Solar Re-publicado' }) });
  assert(re.status === 200 || re.status === 201, 'POST /owners/properties tras liberar slot -> 200', `status=${re.status}`);
  const afterRe = await api('GET', '/owners/my-properties', { token: owner.token });
  const reCreated = ownerList(afterRe.json).find((p) => p.name === 'Solar Re-publicado');
  assert(!!reCreated, 'propiedad republicada visible', `found=${!!reCreated}`);

  // [3] update (cambio de precio) + get por id + price-history
  console.log('\n[3] Actualizar precio + get por id');
  const updTarget = reCreated || available[0];
  const upd = await api('PUT', `/owners/properties/${updTarget.id}`, { token: owner.token, raw: await ownerForm({ id: updTarget.id, name: updTarget.name, price: 95000 }) });
  assert(upd.status === 200, `PUT /owners/properties/${updTarget.id}`, `status=${upd.status}`);
  const g1 = await api('GET', `/owners/properties/${updTarget.id}`, { token: owner.token });
  assert(g1.status === 200 && Number(g1.json && g1.json.price) === 95000, `GET /owners/properties/${updTarget.id} (precio nuevo)`, `status=${g1.status} price=${g1.json && g1.json.price}`);
  const ph = await api('GET', `/properties/${updTarget.id}/price-history`);
  const phArr = Array.isArray(ph.json) ? ph.json : [];
  assert(ph.status === 200 && phArr.length >= 1, `price-history refleja cambio (${updTarget.id})`, `count=${phArr.length}`);

  console.log('\n======== RESULTADOS ROL OWNER ========');
  console.log(`PASS ${PASS}  FAIL ${FAIL}  SKIP ${SKIP}`);
  if (failures.length) { console.log('FALLAS:'); failures.forEach((f) => console.log(`  - ${f}`)); }
  else console.log('MATRIZ OWNER COMPLETA EN VERDE');
  if (process.argv[2] === '--exit-code') process.exit(FAIL === 0 ? 0 : 1);
})().catch((e) => { console.error('FATAL', e); process.exit(2); });