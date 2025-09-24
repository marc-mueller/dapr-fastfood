import { setActivePinia, createPinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { useKitchenStore } from '@/stores/kitchenStore'

vi.mock('@/store/apiClient', () => ({
  default: {
    getPendingOrders: vi.fn(async () => ({ data: [ { id: 'o1', orderReference: '001', items: [{ id: 'i1', productDescription: 'A', state: 'Pending' }] } ] })),
    getPendingOrder: vi.fn(async () => ({ data: { id: 'o1', orderReference: '001', items: [{ id: 'i1', productDescription: 'A', state: 'Finished' }] } })),
    finishOrderItem: vi.fn(async () => ({})),
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

describe('kitchenStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('loads pending orders', async () => {
    const store = useKitchenStore()
    await store.fetchPendingOrders()
    expect(store.pendingOrders.length).toBe(1)
    expect(store.pendingOrders[0].orderReference).toBe('001')
  })

  it('finishes an order item (no throw)', async () => {
    const store = useKitchenStore()
    await expect(store.finishOrderItem('i1')).resolves.toBeUndefined()
  })
})
