import http from 'k6/http';
import { sleep, check, group } from 'k6';

export const options = {
  // Escenario simple: 20 segundos con VUs constantes para una prueba corta
  scenarios: {
    load: {
      executor: 'constant-vus',
      vus: 50,
      duration: '20s',
    },
  },
  thresholds: {
    'http_req_duration{type:all}': ['p(95)<1000'],
    'http_req_failed': ['rate<0.05'],
  },
};

// Unificar constantes de configuración
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5195';
const HEADERS = { headers: { 'Content-Type': 'application/json' } };

// Genera una lista de cuentas para simular tráfico. Ajusta según tu DB.
const TOTAL_ACCOUNTS = 200; // número de cuentas distintas para reducir contención
const HOT_ACCOUNTS = 5; // cuentas calientes que causarán contención intencional
const accounts = [];
for (let i = 1; i <= TOTAL_ACCOUNTS; i++) {
  const num = String(1000000000000000 + i).slice(-20); // formatea 20 caracteres
  accounts.push({ NumeroCuenta: num, IdCliente: (i % 50) + 1 });
}

const hotAccounts = [];
for (let i = 0; i < HOT_ACCOUNTS; i++) {
  hotAccounts.push(accounts[i]);
}

function pickAccount() {
  // 20% probabilidad de usar una hot account para forzar contención
  if (Math.random() < 0.20) {
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

// Helper para loguear respuesta cuando falla (útil para depuración local)
function logIfFailed(res, name, expectedStatuses = [200]) {
  try {
    if (!expectedStatuses.includes(res.status)) {
      // res.body puede ser grande; limitamos a 1000 caracteres
      const body = res && res.body ? String(res.body).slice(0, 1000) : 'no body';
      console.log(`${name} FAILED - status=${res.status} body=${body}`);
    }
  } catch (e) {
    console.log(`${name} FAILED - could not read response body: ${e}`);
  }
}

// Comprobación única de health antes de iniciar la carga
export function setup() {
  const health = http.get(`${BASE_URL}/health`);
  const ok = health && health.status === 200;
  console.log(`health check: ${ok ? 'OK' : 'FAILED'} - status=${health ? health.status : 'no response'}`);
  check(health, { 'health status 200': (r) => r && r.status === 200 });
  return { healthOk: ok };
}

// Función principal (única export default)
export default function () {
  // Mix de operaciones: depósitos 35%, retiros 25%, notaDebito 15%, notaCredito 15%, cheques 10%
  const r = Math.random();
  const account = pickAccount();

  if (r < 0.35) {
    // Deposito
    group('deposito', function () {
      const payload = JSON.stringify({
        NumeroCuenta: account.NumeroCuenta,
        IdCliente: account.IdCliente,
        Monto: randAmount(10, 2000),
        IdPersonaQueDeposita: null,
      });

      const res = http.post(`${BASE_URL}/depositos`, payload, buildHeaders());
      check(res, {
        'deposito status is 200': (r) => r.status === 200,
      });
      logIfFailed(res, 'deposito', [200]);
    });
  } else if (r < 0.60) {
    // Retiro
    group('retiro', function () {
      const payload = JSON.stringify({
        NumeroCuenta: account.NumeroCuenta,
        IdCliente: account.IdCliente,
        Monto: randAmount(20, 500),
        NumeroTarjeta: '',
        Pin: '',
      });

      const res = http.post(`${BASE_URL}/retiros`, payload, buildHeaders());
      check(res, {
        'retiro status is 200 or 204 or 400': (r) => [200, 204, 400].includes(r.status),
      });
      logIfFailed(res, 'retiro', [200, 204, 400]);
    });
  } else if (r < 0.75) {
    // Nota débito
    group('notaDebito', function () {
      const payload = JSON.stringify({
        NumeroCuenta: account.NumeroCuenta,
        IdCliente: account.IdCliente,
        Monto: randAmount(5, 300),
        NumeroTarjeta: '',
      });

      const res = http.post(`${BASE_URL}/notas-debito`, payload, buildHeaders());
      check(res, {
        'notaDebito status': (r) => [200, 400].includes(r.status),
      });
      logIfFailed(res, 'notaDebito', [200, 400]);
    });
  } else if (r < 0.90) {
    // Nota crédito
    group('notaCredito', function () {
      const payload = JSON.stringify({
        NumeroCuenta: account.NumeroCuenta,
        IdCliente: account.IdCliente,
        Monto: randAmount(5, 300),
        NumeroTarjeta: null,
        IdPersona: null,
      });

      const res = http.post(`${BASE_URL}/notas-credito`, payload, buildHeaders());
      check(res, { 'notaCredito status': (r) => [200, 400].includes(r.status) });
      logIfFailed(res, 'notaCredito', [200, 400]);
    });
  } else {
    // Cheque
    group('cheque', function () {
      const payload = JSON.stringify({
        NumeroCuenta: account.NumeroCuenta,
        IdCliente: account.IdCliente,
        Monto: randAmount(50, 1000),
        FechaCheque: new Date().toISOString().slice(0, 10),
        IdPersonaCobra: account.IdCliente,
      });

      const res = http.post(`${BASE_URL}/cheques`, payload, buildHeaders());
      check(res, { 'cheque status': (r) => [200, 400].includes(r.status) });
      logIfFailed(res, 'cheque', [200, 400]);
    });
  }

  // Pequeña pausa para simular tiempo de usuario
  sleep(Math.random() * 1.5);
}
