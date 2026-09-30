import { describe, expect, it } from 'vitest'
import {
  calcBuyTotal,
  calcSellTotal,
  getAlpha,
  getDemandFactor,
  getPriceDecayRate,
  getSupplyFactor,
} from './priceCalculator'

describe('price calculator', () => {
  it('derives the price floor and decay from rarity', () => {
    expect(getPriceDecayRate(0)).toBe(0.1)
    expect(getPriceDecayRate(3)).toBe(0.5)
    expect(getAlpha(0)).toBe(0.5)
    expect(getAlpha(3)).toBeCloseTo(0.1)
  })

  it('starts demand and supply at the base price for one item', () => {
    expect(getDemandFactor(2, 1)).toBe(1)
    expect(getSupplyFactor(2, 1)).toBe(1)
  })

  it('applies exponential demand and supply factors for larger quantities', () => {
    expect(getDemandFactor(0, 2)).toBeCloseTo(1.0951625819640405)
    expect(getSupplyFactor(0, 2)).toBeCloseTo(0.9524187090179798)
  })

  it('rounds the buy and sell previews to hundredths', () => {
    expect(calcBuyTotal(10, 0, 2)).toBe(21.9)
    expect(calcSellTotal(10, 0, 2)).toBe(19.05)
  })
})