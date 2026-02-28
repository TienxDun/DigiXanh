describe('Authentication - Login', () => {
  beforeEach(() => {
    cy.visit('/login');
  });

  it('should display login form', () => {
    cy.get('input[type="email"]').should('be.visible');
    cy.get('input[type="password"]').should('be.visible');
    cy.get('button[type="submit"]').should('be.visible');
  });

  it('should login with valid credentials', () => {
    cy.get('input[type="email"]').type('user@example.com');
    cy.get('input[type="password"]').type('User@123');
    cy.get('button[type="submit"]').click();

    cy.url().should('include', '/');
    cy.contains('Đăng xuất').should('be.visible');
  });

  it('should show error with invalid credentials', () => {
    cy.get('input[type="email"]').type('wrong@example.com');
    cy.get('input[type="password"]').type('WrongPass123');
    cy.get('button[type="submit"]').click();

    cy.contains('Email hoặc mật khẩu không đúng').should('be.visible');
  });

  it('should validate email format', () => {
    cy.get('input[type="email"]').type('invalid-email');
    cy.get('input[type="password"]').type('Password123');
    cy.get('button[type="submit"]').click();

    cy.contains('Email không hợp lệ').should('be.visible');
  });
});
