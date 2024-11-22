export interface Order {
  id: string;
  items: OrderItem[];
  status: string;
}

export interface OrderItem {
  productId: string;
  quantity: number;
}
