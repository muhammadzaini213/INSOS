// Spike: batched inserts (50 events/POST, the game's real batch size)
// → 100 req/s == 5000 inserts/s on POST /rest/v1/analytics_events.
//
//   SUPABASE_URL=https://xyz.supabase.co SUPABASE_ANON_KEY=... k6 run Tools/LoadTest/k6-spike-batch.js
//
// Abort-on-error is OFF by design: 429/5xx during a spike are findings, not script bugs.

import http from 'k6/http';
import { check } from 'k6';

const BASE = __ENV.SUPABASE_URL;
const KEY = __ENV.SUPABASE_ANON_KEY;
const BATCH = 50; // AnalyticsConfig.batchSize default

if (!BASE || !KEY) {
  throw new Error('Set SUPABASE_URL and SUPABASE_ANON_KEY env vars.');
}

export const options = {
  scenarios: {
    spike: {
      executor: 'ramping-arrival-rate',
      preAllocatedVUs: 200,
      maxVUs: 1000,
      timeUnit: '1s',
      stages: [
        { target: 100, duration: '30s' }, // spike up (100 * 50 = 5000 inserts/s)
        { target: 100, duration: '3m' },  // hold
        { target: 0, duration: '30s' },   // drop
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

function makeEvent(vu, iter, i) {
  return {
    event_name: pick(EVENTS),
    object_name: pick(OBJECTS),
    parent_name: 'MainPanel',
    scene_name: pick(SCENES),
    player_name: `loadtest-${vu}`,
    device_id: `loadtest-${vu}-${iter}-${i}`,
    duration_ms: Math.floor(Math.random() * 5000),
  };
}

export default function () {
  const url = `${BASE}/rest/v1/analytics_events`;
  const batch = [];
  for (let i = 0; i < BATCH; i++) {
    batch.push(makeEvent(__VU, __ITER, i));
  }

  const res = http.post(url, JSON.stringify(batch), { headers: HEADERS });

  check(res, {
    '2xx': (r) => r.status >= 200 && r.status < 300,
  });
}
