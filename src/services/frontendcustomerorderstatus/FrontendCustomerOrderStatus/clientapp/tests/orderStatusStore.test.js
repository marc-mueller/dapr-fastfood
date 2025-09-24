import { setActivePinia, createPinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { useOrderStatusStore } from '@/stores/orderStatusStore'

vi.mock('@/store/apiClient', () => ({
  default: {
    getOrder: vi.fn(async () => ({ data: { id: 'o1', type: 'Inhouse', state: 'Paid' } })),
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

describe('orderStatusStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('updates and categorizes orders', () => {
    const store = useOrderStatusStore()
    store.updateOrder({ id: 'o1', type: 'Inhouse', state: 'Paid' })
    expect(store.ordersInPreparationList.length).toBe(1)
    expect(store.ordersFinishedList.length).toBe(0)

    store.updateOrder({ id: 'o1', type: 'Inhouse', state: 'Prepared' })
    expect(store.ordersInPreparationList.length).toBe(0)
    expect(store.ordersFinishedList.length).toBe(1)
  })

  it('fetches and updates by id', async () => {
    const store = useOrderStatusStore()
    await store.fetchOrderAndUpdateStore('o1')
    expect(store.ordersInPreparationList.length + store.ordersFinishedList.length).toBe(1)
  })
})
