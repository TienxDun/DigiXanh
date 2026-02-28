// Cypress E2E Support File
import './commands';

// Declare global Cypress namespace
declare global {
  namespace Cypress {
    interface Chainable {
      login(email: string, password: string): Chainable<void>;
      loginAsAdmin(): Chainable<void>;
      loginAsUser(): Chainable<void>;
      addToCart(plantId: number, quantity: number): Chainable<void>;
      clearCart(): Chainable<void>;
      createPlant(plantData: any): Chainable<void>;
    }
  }
}
