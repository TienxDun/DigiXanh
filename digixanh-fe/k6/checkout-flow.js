import http from 'k6/http';
import { check, sleep, group } from 'k6';

// Configuration for checkout flow test
export const options = {
  scenarios: {
    checkout_flow: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '1m', target: 50 },
        { duration: '3m', target: 50 },
        { duration: '1m', target: 0 },
      ],
    },
  },
  thresholds: {
    http_req_duration: ['p(95)<1000'],
    http_req_failed: ['rate<0.05'],
  },
};

const BASE_URL = 'https://localhost:5001';

// Helper to get auth token
function login() {
  const res = http.post(`${BASE_URL}/api/auth/login`, {
    email: 'user@example.com',
    password: 'User@123',
  });
  
  if (res.status === 200) {
    return res.json('token');
  }
  return null;
}

export default function () {
  const token = login();
  if (!token) {
    console.error('Login failed');
    return;
  }

  const headers = {
    Authorization: `Bearer ${token}`,
    'Content-Type': 'application/json',
  };

  group('Checkout Flow', () => {
    // Add to cart
    const addToCartRes = http.post(
      `${BASE_URL}/api/cart/items`,
      JSON.stringify({ plantId: 1, quantity: 2 }),
      { headers }
    );
    check(addToCartRes, {
      'add to cart successful': (r) => r.status === 200 || r.status === 201,
    });

    sleep(1);

    // Get cart
    const cartRes = http.get(`${BASE_URL}/api/cart`, { headers });
    check(cartRes, {
      'get cart successful': (r) => r.status === 200,
      'cart has items': (r) => r.json('items') && r.json('items').length > 0,
    });

    sleep(1);

    // Create order
    const orderRes = http.post(
      `${BASE_URL}/api/orders`,
      JSON.stringify({
        recipientName: 'Test User',
        phone: '0900000000',
        shippingAddress: '123 Test Street, District 1',
        paymentMethod: 0, // Cash
      }),
      { headers }
    );
    check(orderRes, {
      'order created': (r) => r.status === 200,
      'order has id': (r) => r.json('orderId') !== undefined,
    });
  });

  sleep(3);
}
