import { HerbTypeEnum, hasHerbType } from '../types/enums'

/** Возвращает тип зелья, которое получится из выбранной травы-основы. */
export function getBasePotionTypeLabel(herbType: number): string | null {
  if (hasHerbType(herbType, HerbTypeEnum.HealingBase)) return 'Зелье исцеления'
  if (hasHerbType(herbType, HerbTypeEnum.PoisonBase)) return 'Зелье яда'
  if (hasHerbType(herbType, HerbTypeEnum.Magic)) return 'Магическое зелье'
  return null
}
