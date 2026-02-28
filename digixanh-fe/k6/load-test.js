import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');

// Test configuration
export const options = {
  stages: [
    { duration: '2m', target: 100 }, // Ramp up to 100 users
    { duration: '5m', target: 100 }, // Stay at 100 users
    { duration: '2m', target: 200 }, // Spike to 200 users
    { duration: '5m', target: 200 }, // Stay at 200 users
    { duration: '2m', target: 0 },   // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% of requests under 500ms
    http_req_failed: ['rate<0.01'],   // Error rate under 1%
    errors: ['rate<0.05'],            // Custom error rate under 5%
  },
};

const BASE_URL = 'https://localhost:5001';

export default function () {
  // Test 1: Get plants (public endpoint - high traffic)
  const plantsRes = http.get(`${BASE_URL}/api/plants?page=1&pageSize=12`);
  check(plantsRes, {
    'plants status is 200': (r) => r.status === 200,
    'plants response time < 200ms': (r) => r.timings.duration < 200,
  }) || errorRate.add(1);

  sleep(1);

  // Test 2: Get plant detail
  const plantDetailRes = http.get(`${BASE_URL}/api/plants/1`);
  check(plantDetailRes, {
    'plant detail status is 200': (r) => r.status === 200,
    'plant detail response time < 300ms': (r) => r.timings.duration < 300,
  }) || errorRate.add(1);

  sleep(1);

  // Test 3: Get categories
  const categoriesRes = http.get(`${BASE_URL}/api/categories`);
  check(categoriesRes, {
    'categories status is 200': (r) => r.status === 200,
    'categories response time < 100ms': (r) => r.timings.duration < 100,
  }) || errorRate.add(1);

  sleep(2);
}
