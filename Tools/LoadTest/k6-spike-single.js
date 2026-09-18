// Spike: single-row inserts → 5000 req/s on POST /rest/v1/analytics_events.
// Mirrors AnalyticsService.PostEventsCoroutine payload shape (1 event per POST).
//
//   SUPABASE_URL=https://xyz.supabase.co SUPABASE_ANON_KEY=... k6 run Tools/LoadTest/k6-spike-single.js
//
// 5000 rps from one k6 instance needs headroom: raise VUs (see stages) and,
// if the runner saturates, use `k6 run --out cloud` or multiple runners.
// Abort-on-error is OFF by design: 429/5xx during a spike are findings, not script bugs.

import http from 'k6/http';
import { check } from 'k6';

const BASE = __ENV.SUPABASE_URL;
const KEY = __ENV.SUPABASE_ANON_KEY;

if (!BASE || !KEY) {
  throw new Error('Set SUPABASE_URL and SUPABASE_ANON_KEY env vars.');
}

export const options = {
  scenarios: {
    spike: {
      executor: 'ramping-arrival-rate',
      preAllocatedVUs: 2000,
      maxVUs: 8000,
      timeUnit: '1s',
      stages: [
        { target: 5000, duration: '30s' }, // spike up
        { target: 5000, duration: '3m' },  // hold
        { target: 0, duration: '30s' },    // drop
      ],
    },
  },
  thresholds: {
    // Informational only — do NOT abort the run on breach.
    http_req_failed: [],
    http_req_duration: [],
  },
};

const HEADERS = {
  apikey: KEY,
  Authorization: `Bearer ${KEY}`,
  'Content-Type': 'application/json',
  Prefer: 'return=minimal',
};

const EVENTS = ['onClick', 'onCorrectDrop', 'onWrongDrop', 'scene_duration'];
const OBJECTS = ['Correct_Button', 'Wrong_Button', 'MenuButton', 'BackButton'];
const SCENES = ['01_Section 1', '01_Section 2', '01_Section 3'];

function pick(arr) {
  return arr[Math.floor(Math.random() * arr.length)];
}

export default function () {
  const url = `${BASE}/rest/v1/analytics_events`;
  const body = JSON.stringify([
    {
      event_name: pick(EVENTS),
      object_name: pick(OBJECTS),
      parent_name: 'MainPanel',
      scene_name: pick(SCENES),
      player_name: `loadtest-${__VU}`,
      device_id: `loadtest-${__VU}-${__ITER}`,
      duration_ms: Math.floor(Math.random() * 5000),
    },
  ]);

  const res = http.post(url, body, { headers: HEADERS });

  check(res, {
    '2xx': (r) => r.status >= 200 && r.status < 300,
  });
}
