describe('VNPay Payment Integration', () => {
  beforeEach(() => {
    cy.loginAsUser();
    cy.clearCart();
  });

  it('should redirect to VNPay for online payment', () => {
    cy.addToCart(1, 2);
    cy.visit('/checkout');
    
    cy.get('input[name="recipientName"]').type('Nguyễn Văn A');
    cy.get('input[name="phone"]').type('0900000000');
    cy.get('textarea[name="shippingAddress"]').type('123 Lê Lợi');
    
    cy.get('.payment-method').contains('VNPay').click();
    
    cy.intercept('POST', '**/api/orders').as('createOrder');
    cy.get('button[type="submit"]').click();
    
    cy.wait('@createOrder').its('response.statusCode').should('eq', 200);
  });

  it('should handle successful payment callback', () => {
    cy.visit('/payment-return?vnp_ResponseCode=00&vnp_TxnRef=123');
    cy.contains('Thanh toán thành công').should('be.visible');
  });
});
