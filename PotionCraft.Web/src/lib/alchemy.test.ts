import { describe, expect, it } from 'vitest'
import { getBasePotionTypeLabel } from './alchemy'
import { HerbTypeEnum } from '../types/enums'

describe('base potion type label', () => {
  it('identifies a healing potion base', () => {
    expect(getBasePotionTypeLabel(HerbTypeEnum.HealingBase)).toBe('Зелье исцеления')
  })

  it('identifies a poison base', () => {
    expect(getBasePotionTypeLabel(HerbTypeEnum.PoisonBase)).toBe('Зелье яда')
  })

  it('identifies a magic potion base', () => {
    expect(getBasePotionTypeLabel(HerbTypeEnum.Magic)).toBe('Магическое зелье')
  })

  it('returns null for herbs that cannot be potion bases', () => {
    expect(getBasePotionTypeLabel(HerbTypeEnum.HealingModifier)).toBeNull()
  })
})
