describe('Cart & Checkout Flow', () => {
  beforeEach(() => {
    cy.loginAsUser();
    cy.clearCart();
  });

  it('should complete checkout with Cash payment', () => {
    cy.addToCart(1, 2);
    cy.visit('/checkout');
    
    cy.get('input[name="recipientName"]').type('Nguyễn Văn A');
    cy.get('input[name="phone"]').type('0900000000');
    cy.get('textarea[name="shippingAddress"]').type('123 Lê Lợi, Quận 1');
    
    cy.get('button[type="submit"]').click();
    
    cy.url().should('include', '/order-success');
    cy.contains('Đặt hàng thành công').should('be.visible');
  });

  it('should validate checkout form', () => {
    cy.addToCart(1, 1);
    cy.visit('/checkout');
    
    cy.get('button[type="submit"]').click();
    
    cy.contains('Vui lòng nhập tên').should('be.visible');
  });
});
