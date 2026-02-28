// ***********************************************
// Custom Cypress Commands
// ***********************************************

Cypress.Commands.add('login', (email: string, password: string) => {
  cy.session([email, password], () => {
    cy.request('POST', 'https://localhost:5001/api/auth/login', {
      email,
      password
    }).then((response) => {
      expect(response.status).to.eq(200);
      window.localStorage.setItem('token', response.body.token);
    });
  });
});

Cypress.Commands.add('loginAsAdmin', () => {
  cy.login('admin@digixanh.com', 'Admin@123');
});

Cypress.Commands.add('loginAsUser', () => {
  cy.login('user@example.com', 'User@123');
});

Cypress.Commands.add('addToCart', (plantId: number, quantity: number) => {
  cy.request({
    method: 'POST',
    url: 'https://localhost:5001/api/cart/items',
    headers: {
      Authorization: `Bearer ${window.localStorage.getItem('token')}`
    },
    body: {
      plantId,
      quantity
    }
  });
});

Cypress.Commands.add('clearCart', () => {
  cy.request({
    method: 'DELETE',
    url: 'https://localhost:5001/api/cart',
    headers: {
      Authorization: `Bearer ${window.localStorage.getItem('token')}`
    },
    failOnStatusCode: false
  });
});

Cypress.Commands.add('createPlant', (plantData: any) => {
  cy.request({
    method: 'POST',
    url: 'https://localhost:5001/api/admin/plants',
    headers: {
      Authorization: `Bearer ${window.localStorage.getItem('token')}`
    },
    body: plantData
  });
});
