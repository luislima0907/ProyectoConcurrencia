# Pruebas de carga con k6 para CajeroAPI

Este directorio contiene un script de carga para k6 (`load-test.js`) diseñado para ejercitar los endpoints principales de tu API (depósitos, retiros, notas, cheques) y forzar contención en algunas cuentas para observar comportamientos de concurrencia y deadlocks.

Resumen rápido
- Script: `k6/load-test.js`
- Objetivo: probar concurrencia, throughput, latencias y reproducir deadlocks en staging.
- Shell objetivo: Windows `cmd.exe` (ejemplos abajo).

Requisitos
- k6 instalado en tu máquina de pruebas. En Windows puedes instalarlo con Chocolatey o Scoop, o descargar el binario.
  - Chocolatey (si lo tienes):

```powershell
choco install k6
```

  - Scoop (si lo tienes):

```powershell
scoop install k6
```

  - O descarga desde https://k6.io/

Preparar la API
- Asegúrate de que la API esté corriendo en un entorno de staging que imite producción (mismo SQL Server, tamaños de datos).
- Haz snapshot/backup antes de pruebas si vas a alterar datos.

Variables de entorno y configuración
- Puedes cambiar la URL base con la variable de entorno `BASE_URL`.
- El script tiene parámetros internos para cuentas totales y cuentas calientes; modifícalos en `load-test.js` si quieres más/o menos contención.

Ejemplos de ejecución (Windows cmd)
- Ejecución básica (usa la URL por defecto `http://localhost:5000`):

```cmd
k6 run .\k6\load-test.js
```

- Ejecutar apuntando a otra URL:

```cmd
k6 run --env BASE_URL=http://staging.miapi.local:5000 .\k6\load-test.js
```

- Guardar resultados en JSON para análisis posterior:

```cmd
k6 run --env BASE_URL=http://localhost:5000 --out json=reports\k6-output.json .\k6\load-test.js
```

- Ejecutar una prueba rápida local (ignora escenarios definidos, usa VUs/tiempo desde CLI):

```cmd
k6 run --env BASE_URL=http://localhost:5000 --vus 50 --duration 2m .\k6\load-test.js
```

Nota: cuando el script define `scenarios` (como el que viene), opciones `--vus` y `--duration` son ignoradas; usa un run rápido para debug de endpoints.

Integración con InfluxDB + Grafana (opcional)
- Para obtener dashboards en tiempo real, instala InfluxDB y Grafana y enlaza k6 con InfluxDB:

```cmd
k6 run --env BASE_URL=http://localhost:5000 --out influxdb=http://127.0.0.1:8086/k6 .\k6\load-test.js
```

- Con Grafana puedes importar dashboards para k6 o crear uno con métricas: `http_req_duration`, `http_reqs`, `http_req_failed`, etc.

Reproducción controlada de deadlocks
- Para forzar deadlocks, aumenta el número de `HOT_ACCOUNTS` en el script y la probabilidad de selección (ej. 50–80%).
- Puedes también reducir `TOTAL_ACCOUNTS` o crear dos tipos de transacciones que accedan a recursos en orden inverso.
- Ejecuta muchas VUs apuntando a esas cuentas calientes para aumentar la probabilidad de deadlock.

Monitoreo y captura de deadlocks en SQL Server
- Crea una sesión de Extended Events para capturar `xml_deadlock_report` y volcar a archivo; ejemplo T-SQL para crear session (ejecutar en SQL Server Management Studio contra tu DB de staging):

```sql
CREATE EVENT SESSION [DeadlockCapture] ON SERVER
ADD EVENT sqlserver.xml_deadlock_report
ADD TARGET package0.event_file(SET filename=N'C:\\temp\\deadlocks.xel')
WITH (MAX_MEMORY=4096 KB, EVENT_RETENTION_MODE=ALLOW_SINGLE_EVENT_LOSS, MAX_DISPATCH_LATENCY=30 SECONDS);
GO
ALTER EVENT SESSION [DeadlockCapture] ON SERVER STATE = START;
GO
```

- Después de la prueba, detén y analiza el `.xel` con SSMS (verás los deadlock graphs). También puedes leerlos con T-SQL o herramientas.

Métricas y logs que debes recoger
- En el servidor de la aplicación: CPU, memoria, uso de red, conteo de threads/conexiones, pool usage.
- En SQL Server: sys.dm_tran_locks, sys.dm_exec_requests (blocking), sys.dm_os_wait_stats, perfmon counters (Physical Disk/sec, Avg Disk sec/Read), y el XE de deadlocks.
- Resultados de k6: `http_req_duration` p50/p90/p99, `http_req_failed` rate, throughput (`http_reqs`).

Interpretación rápida
- Deadlock graphs te dirán qué queries/orden de acceso causaron el conflicto.
- Si hay muchos bloqueos/esperas tipo `LCK_M_XX` o `PAGEIOLATCH_*`, puedes tener I/O lento o escaneo de tablas.
- Reintentos aplicacionales (exponential backoff) suelen ser una solución práctica para deadlocks raros.

Consejos para ajustar el script
- Para maximizar probabilidad de deadlocks: set `HOT_ACCOUNTS` alto, `TOTAL_ACCOUNTS` bajo, y aumentar VUs o el `spike` scenario.
- Para validar que reintentos funcionan: añade lógica en el cliente para reintentar cuando la API devuelva 4xx/5xx con mensajes de deadlock.

Buenas prácticas
- No ejecutes pruebas agresivas en producción.
- Automatiza (por ejemplo en CI) y documenta cada ejecución (fecha, script, versión de la API, datos de la BD).

Siguientes pasos sugeridos (puedo ayudarte con cualquiera de estos):
- Generar un `k6` script específico que fuerce un patrón de deadlock (en tu caso, puedo editar `load-test.js` para crear transacciones en orden inverso).  
- Preparar un ejemplo de configuración de InfluxDB+Grafana y un dashboard básico para visualizar resultados.  
- Crear un pequeño script para automatizar la captura y extracción de `xml_deadlock_report` después de la prueba.

¿Quieres que:
- 1) prepare un `k6` script adicional que intente reproducir deadlocks de forma controlada, o
- 2) te dé la guía paso a paso para crear la sesión Extended Events y extraer los `.xel` (con scripts de análisis), o
- 3) preparar instrucciones para conectar k6 a InfluxDB y un dashboard básico de Grafana?

Dime cuál prefieres y lo preparo (puedo generar el script o los pasos concretos en el repo).
