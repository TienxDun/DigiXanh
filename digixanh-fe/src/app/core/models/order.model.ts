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

// ============== Admin Order Models ==============

export interface AdminOrderDto {
  id: number;
  orderDate: Date;
  customerName: string;
  customerEmail: string;
  totalAmount: number;
  finalAmount: number;
  status: string;
  paymentMethod: string;
  phone: string;
}

export interface AdminOrderDetailDto extends AdminOrderDto {
  shippingAddress: string;
  transactionId?: string;
  updatedAt?: Date;
  items: OrderItemDto[];
  statusHistory: OrderStatusHistoryDto[];
}

export interface OrderStatusHistoryDto {
  id: number;
  oldStatus: string;
  newStatus: string;
  changedBy?: string;
  reason?: string;
  changedAt: Date;
}

export interface UpdateOrderStatusRequest {
  newStatus: number;
  reason?: string;
}

export interface OrderStatusOption {
  value: number;
  name: string;
  displayName: string;
}

export interface AdminOrderQueryParams {
  page?: number;
  pageSize?: number;
  status?: number | null;
  search?: string;
}
