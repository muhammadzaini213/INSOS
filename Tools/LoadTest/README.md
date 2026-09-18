# Supabase Spike Load Test

Hits `POST /rest/v1/analytics_events` with the **exact payload shape** the game sends
(`AnalyticsService.BuildJsonArray`: `event_name, object_name, parent_name, scene_name,
player_name, device_id, duration_ms`; headers `apikey`, `Authorization: Bearer`,
`Content-Type: application/json`, `Prefer: return=minimal`).

## Profiles

| Script | Requests/s | Events/POST | Inserts/s |
|---|---|---|---|
| `k6-spike-single.js` | 5000 | 1 | 5000 |
| `k6-spike-batch.js` | 100 | 50 (game's real batch size) | 5000 |

Both: spike up 30s → hold 3m → drop 30s. No abort-on-error: **429/5xx are findings.**

## Prereqs

- `k6` installed: https://k6.io/docs/get-started/installation/
- Supabase URL + anon key **as env vars** (same names as CI). Never commit them.

## Run

```bash
# Single-row profile
SUPABASE_URL=https://xyz.supabase.co SUPABASE_ANON_KEY=... k6 run Tools/LoadTest/k6-spike-single.js

# Batched profile
SUPABASE_URL=https://xyz.supabase.co SUPABASE_ANON_KEY=... k6 run Tools/LoadTest/k6-spike-batch.js
```

(PowerShell: `$env:SUPABASE_URL="..."; $env:SUPABASE_ANON_KEY="..."; k6 run Tools/LoadTest/k6-spike-single.js`)

One k6 instance may not reach 5000 rps if the runner saturates — if `http_reqs` rate
plateaus below target while the runner is CPU-bound, split across machines or use k6 Cloud.

## Read the result

k6 summary gives achieved RPS, `p(95)`/`p(99)` latency, and `http_req_failed` rate.
Also check the Supabase dashboard for 429s / connection-pool saturation during the window.

## ⚠️ Cleanup (production table)

Load rows use **game-like `event_name`s** — the only identifiers are the
`device_id LIKE 'loadtest-%'` prefix and the run's timestamp window.
Record start/end time of each run, then delete:

```sql
-- Dry run first:
SELECT count(*) FROM analytics_events
WHERE device_id LIKE 'loadtest-%'
  AND created_at BETWEEN '<RUN_START_UTC>' AND '<RUN_END_UTC>';

-- Then delete (same WHERE clause):
DELETE FROM analytics_events
WHERE device_id LIKE 'loadtest-%'
  AND created_at BETWEEN '<RUN_START_UTC>' AND '<RUN_END_UTC>';
```

A 3-minute hold at 5000 inserts/s ≈ **900k rows** — verify table size/quota before running.
