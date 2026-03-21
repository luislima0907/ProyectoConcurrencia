import http from 'k6/http';
import { sleep, check, group } from 'k6';
import { Counter } from 'k6/metrics';

// Contadores de éxito por operación
const depositosOk = new Counter('depositos_ok');
const retirosOk = new Counter('retiros_ok');
const notaDebitoOk = new Counter('notaDebito_ok');
const notaCreditoOk = new Counter('notaCredito_ok');
const chequesOk = new Counter('cheques_ok');

export const options = {
  // Opciones de ejecución
  scenarios: {
    load: {
      executor: 'constant-vus',
      vus: 50,
      duration: '10s',
    },
  },
  thresholds: {
    'http_req_duration{type:all}': ['p(95)<1000'],
    'http_req_failed': ['rate<0.01'],
  },
};

// Configuración
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5195';
const HEADERS = { headers: { 'Content-Type': 'application/json' } };

// Cuentas de prueba
const accounts = [
  { NumeroCuenta: '0000000001', IdCliente: 1, FechaCheque: '2026-01-01' },
  { NumeroCuenta: '0000000002', IdCliente: 2, FechaCheque: '2026-02-01' },
  { NumeroCuenta: '0000000003', IdCliente: 3, FechaCheque: '2026-03-01' },
];

// Tarjetas en la base de datos
const cards = [
  { NumeroTarjeta: '0000001095072985', NumeroCuenta: '0000000002', Pin: '3312' },
  { NumeroTarjeta: '1111222233334444', NumeroCuenta: '0000000001', Pin: '1234' },
  { NumeroTarjeta: '5555666677778888', NumeroCuenta: '0000000002', Pin: '5678' },
];

// Cuentas calientes
const hotAccounts = [accounts[0]];

function pickAccount() {
  // Selección aleatoria de cuenta (incluye hotAccounts)
  if (Math.random() < 0.10) {
    return hotAccounts[Math.floor(Math.random() * hotAccounts.length)];
  }
  return accounts[Math.floor(Math.random() * accounts.length)];
}

function randAmount(min = 10, max = 500) {
  return Math.round((Math.random() * (max - min) + min) * 100) / 100;
}

function buildHeaders() {
  return HEADERS;
}

// POST con timeout extendido
function post(url, payload, headers) {
  return http.post(url, payload, headers, { timeout: '60s' });
}

// Registro si la respuesta falla
function logIfFailed(res, name, expectedStatuses = [200]) {
    try {
        if (!expectedStatuses.includes(res.status)) {
            const body = res && res.body ? String(res.body).slice(0, 1000) : 'no body';

            if (body.includes('DEADLOCK')) {
                console.log(`DEADLOCK DETECTED in ${name} - status=${res.status}`);
            } else {
                console.log(`${name} FAILED - status=${res.status} body=${body}`);
            }
        }
    } catch (e) {
        console.log(`${name} FAILED - could not read response body: ${e}`);
    }
}

// Rellena NumeroCuenta a 20 caracteres
function padAccount(num) {
  return String(num).padEnd(20, ' ');
}

// Rellena NumeroTarjeta a 20 caracteres o devuelve null
function padCard(num) {
  if (num === null || num === undefined) return null;
  return String(num).padEnd(20, ' ');
}

// Rellena PIN a 4 caracteres o devuelve null
function padCardPin(num){
    if (num === null || num === undefined) return null;
    return String(num).padEnd(4, ' ');
}

// Selecciona tarjeta para la cuenta o devuelve una aleatoria
function pickCardForAccount(account) {
  try {
    if (!Array.isArray(cards) || cards.length === 0) return null;
    if (!account || !account.NumeroCuenta) {
      return cards[Math.floor(Math.random() * cards.length)];
    }

    const match = cards.filter(c => String(c.NumeroCuenta) === String(account.NumeroCuenta));
    if (match.length > 0) {
      return match[Math.floor(Math.random() * match.length)];
    }

    return cards[Math.floor(Math.random() * cards.length)];
  } catch (e) {
    return cards[Math.floor(Math.random() * cards.length)];
  }
}

// Health check
export function setup() {
  const health = http.get(`${BASE_URL}/health`);
  const ok = health && health.status === 200;
  console.log(`health check: ${ok ? 'OK' : 'FAILED'} - status=${health ? health.status : 'no response'}`);
  check(health, { 'health status 200': (r) => r && r.status === 200 });
  return { healthOk: ok };
}

// Escenario de operaciones: distribución por tipo
export default function () {
  const r = Math.random();
  const account = pickAccount();

  if (r < 0.35) {
    group('deposito', function () {
      const payload = JSON.stringify({
        NumeroCuenta: padAccount(account.NumeroCuenta),
        IdCliente: account.IdCliente,
        Monto: randAmount(10, 2000),
        IdPersonaQueDeposita: null,
      });

      const res = post(`${BASE_URL}/depositos`, payload, buildHeaders());
      check(res, {
        'deposito status is 200': (r) => r.status === 200,
      });
      if (res.status === 200) depositosOk.add(1);
      logIfFailed(res, 'deposito', [200]);
    });
  } else if (r < 0.60) {
    group('retiro', function () {
      const chosenCard = (account && account.IdCliente === 3) ? null : pickCardForAccount(account);
      const payload = JSON.stringify({
        NumeroCuenta: padAccount(account.NumeroCuenta),
        IdCliente: account.IdCliente,
        Monto: randAmount(20, 500),
        NumeroTarjeta: chosenCard ? padCard(chosenCard.NumeroTarjeta) : null,
        Pin: chosenCard ? padCardPin(chosenCard.Pin) : null,
      });

      const res = post(`${BASE_URL}/retiros`, payload, buildHeaders());
      check(res, {
        'retiro status is 200 or 204 or 400': (r) => [200, 204, 400].includes(r.status),
      });
      if ([200,204].includes(res.status)) retirosOk.add(1);
      logIfFailed(res, 'retiro', [200, 204, 400]);
    });
  } else if (r < 0.75) {
    group('notaDebito', function () {
      const chosenCard = (account && account.IdCliente === 3) ? null : pickCardForAccount(account);
       const payload = JSON.stringify({
         NumeroCuenta: padAccount(account.NumeroCuenta),
         IdCliente: account.IdCliente,
         Monto: randAmount(5, 300),
         NumeroTarjeta: chosenCard ? padCard(chosenCard.NumeroTarjeta) : null,
       });

       const res = post(`${BASE_URL}/notas-debito`, payload, buildHeaders());
      check(res, {
        'notaDebito status': (r) => [200, 400].includes(r.status),
      });
      if (res.status === 200) notaDebitoOk.add(1);
      logIfFailed(res, 'notaDebito', [200, 400]);
    });
  } else if (r < 0.90) {
    group('notaCredito', function () {
      const payload = JSON.stringify({
        NumeroCuenta: padAccount(account.NumeroCuenta),
        IdCliente: account.IdCliente,
        Monto: randAmount(5, 300),
        NumeroTarjeta: null,
        IdPersona: null,
      });

      const res = post(`${BASE_URL}/notas-credito`, payload, buildHeaders());
      check(res, { 'notaCredito status': (r) => [200, 400].includes(r.status) });
      if (res.status === 200) notaCreditoOk.add(1);
      logIfFailed(res, 'notaCredito', [200, 400]);
    });
  } else {
    group('cheque', function () {
      const payload = JSON.stringify({
        NumeroCuenta: padAccount(account.NumeroCuenta),
        IdCliente: account.IdCliente,
        Monto: randAmount(50, 1000),
        FechaCheque: account.FechaCheque,
        IdPersonaCobra: 4,
      });

      const res = post(`${BASE_URL}/cheques`, payload, buildHeaders());
      check(res, { 'cheque status': (r) => [200, 400].includes(r.status) });
      if (res.status === 200) chequesOk.add(1);
      logIfFailed(res, 'cheque', [200, 400]);
    });
  }

  sleep(Math.random() * 1.5);
}