export interface AddCartItemRequest {
  plantId: number;
  quantity: number;
}

export interface UpdateCartItemQuantityRequest {
  quantity: number;
}

export interface CartItemDto {
  id: number;
  plantId: number;
  plantName: string;
  scientificName: string;
  price: number;
  imageUrl: string;
  quantity: number;
  lineTotal: number;
}

export interface CartSummaryDto {
  items: CartItemDto[];
  totalQuantity: number;
  totalAmount: number;
}