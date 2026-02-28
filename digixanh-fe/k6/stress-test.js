import http from 'k6/http';
import { check, sleep } from 'k6';

// Stress test - find breaking point
export const options = {
  stages: [
    { duration: '2m', target: 100 },   // Normal load
    { duration: '2m', target: 500 },   // High load
    { duration: '2m', target: 1000 },  // Very high load
    { duration: '2m', target: 1500 },  // Stress test
    { duration: '2m', target: 0 },     // Recovery
  ],
  thresholds: {
    http_req_duration: ['p(99)<2000'], // 99% under 2s during stress
  },
};

const BASE_URL = 'https://localhost:5001';

export default function () {
  // Mix of different endpoints
  const endpoints = [
    { url: `${BASE_URL}/api/plants`, method: 'GET' },
    { url: `${BASE_URL}/api/plants/1`, method: 'GET' },
    { url: `${BASE_URL}/api/categories`, method: 'GET' },
    { url: `${BASE_URL}/api/health`, method: 'GET' },
  ];

  const randomEndpoint = endpoints[Math.floor(Math.random() * endpoints.length)];
  
  const res = http.request(randomEndpoint.method, randomEndpoint.url);
  
  check(res, {
    'status is acceptable': (r) => r.status < 500, // Allow some 429/503
    'response time < 5s': (r) => r.timings.duration < 5000,
  });

  sleep(Math.random() * 2);
}
