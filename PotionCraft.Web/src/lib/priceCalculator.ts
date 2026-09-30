/** Возвращает скорость падения цены в зависимости от редкости. */
export function getPriceDecayRate(rarity: number): number {
  return 0.1 + (rarity / 3.0) * 0.4
}

/** Возвращает минимальный множитель цены для указанной редкости. */
export function getAlpha(rarity: number): number {
  return 0.5 - (rarity / 3.0) * 0.4
}

/** Рассчитывает множитель спроса для количества товара. */
export function getDemandFactor(rarity: number, quantity: number): number {
  const alpha = getAlpha(rarity)
  const decay = getPriceDecayRate(rarity)
  return 1.0 / alpha - (1.0 / alpha - 1.0) * Math.exp(-decay * (quantity - 1))
}

/** Рассчитывает множитель предложения для количества товара. */
export function getSupplyFactor(rarity: number, quantity: number): number {
  const alpha = getAlpha(rarity)
  const decay = getPriceDecayRate(rarity)
  return alpha + (1.0 - alpha) * Math.exp(-decay * (quantity - 1))
}

/** Рассчитывает предварительную стоимость покупки с округлением до сотых. */
export function calcBuyTotal(baseBuyPrice: number, rarity: number, quantity: number): number {
  return Math.round(baseBuyPrice * getDemandFactor(rarity, quantity) * quantity * 100) / 100
}

/** Рассчитывает предварительную выручку от продажи с округлением до сотых. */
export function calcSellTotal(baseSellPrice: number, rarity: number, quantity: number): number {
  return Math.round(baseSellPrice * getSupplyFactor(rarity, quantity) * quantity * 100) / 100
}