import { setActivePinia, createPinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { useOrderStore } from '@/stores/orderStore'

vi.mock('@/store/apiClient', () => ({
  default: {
    post: vi.fn(async (url, body) => {
      if (url === '/order/createOrder') return { data: { orderId: 'abc-123' } }
      return { data: {} }
    }),
    get: vi.fn(async (url) => {
      if (url === '/order/abc-123') return { data: { id: 'abc-123', items: [{ itemPrice: 2, quantity: 3 }] } }
      if (url === '/products') return { data: [{ id: 1, description: 'A' }] }
      return { data: {} }
    }),
  }
}))

vi.mock('@microsoft/signalr', () => ({
  HubConnectionBuilder: class {
    withUrl() { return this }
    build() { return {
      on: () => {},
      start: async () => {},
      invoke: async () => {},
    } }
  }
}))

describe('orderStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('creates an order, fetches it, and computes total', async () => {
    const store = useOrderStore()
    await store.createOrder()
    expect(store.currentOrder?.id).toBe('abc-123')
    expect(store.totalOrderPrice).toBe(6)
  })

  it('fetches products', async () => {
    const store = useOrderStore()
    await store.fetchProducts()
    expect(store.products.length).toBe(1)
  })
})
