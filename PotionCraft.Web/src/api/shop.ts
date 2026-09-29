import { apiClient } from './client'
import type { ShopItem, TradeRequest, TradeResult } from '../types/models'

/** Возвращает текущий ассортимент магазина. GET /api/shop/inventory */
export function getShopInventory(): Promise<ShopItem[]> {
  return apiClient.get<ShopItem[]>('/api/shop/inventory')
}

/** Выполняет сделку купли/продажи в магазине. POST /api/shop/trade */
export function tradeShopItems(request: TradeRequest): Promise<TradeResult> {
  return apiClient.post<TradeResult>('/api/shop/trade', request)
}
