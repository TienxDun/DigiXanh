export interface CreateOrderRequest {
  recipientName: string;
  phone: string;
  shippingAddress: string;
  paymentMethod: PaymentMethod;
  returnUrl?: string;
}

export enum PaymentMethod {
  Cash = 0,
  VNPay = 1
}

export interface CreateOrderResponse {
  success: boolean;
  orderId: number;
  message: string;
  paymentUrl?: string;
  order?: OrderDetailDto;
}

export interface VNPayReturnResponse {
  success: boolean;
  orderId: number;
  message: string;
  status: string;
  transactionId?: string;
  order?: OrderDetailDto;
}

export interface OrderDto {
  id: number;
  orderDate: Date;
  totalAmount: number;
  discountAmount: number;
  finalAmount: number;
  status: string;
  paymentMethod: string;
  transactionId?: string;
}

export interface OrderDetailDto extends OrderDto {
  recipientName: string;
  phone: string;
  shippingAddress: string;
  items: OrderItemDto[];
}

export interface OrderItemDto {
  id: number;
  plantId: number;
  plantName: string;
  scientificName?: string;
  imageUrl?: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}
