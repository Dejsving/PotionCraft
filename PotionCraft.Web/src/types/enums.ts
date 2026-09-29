/**
 * Среда обитания растения — определяет, где его можно найти.
 * Соответствует PotionCraft.Contracts.Enums.TerrainEnum.
 */
export enum TerrainEnum {
  Everewhere = 0,
  Coast = 1,
  Underdark = 2,
  Forest = 3,
  Swamp = 4,
  Arctic = 5,
  Hills = 6,
  Meadows = 7,
  Mountains = 8,
  Deserts = 9,
  Special = 10,
  UnderWater = 11,
}

export const TERRAIN_NAMES: Record<TerrainEnum, string> = {
  [TerrainEnum.Everewhere]: 'Повсеместно',
  [TerrainEnum.Coast]: 'Берега',
  [TerrainEnum.Underdark]: 'Подъземье',
  [TerrainEnum.Forest]: 'Лес',
  [TerrainEnum.Swamp]: 'Болото',
  [TerrainEnum.Arctic]: 'Арктика',
  [TerrainEnum.Hills]: 'Холмы',
  [TerrainEnum.Meadows]: 'Луга',
  [TerrainEnum.Mountains]: 'Горы',
  [TerrainEnum.Deserts]: 'Пустыни',
  [TerrainEnum.Special]: 'Специальный',
  [TerrainEnum.UnderWater]: 'Подводная среда',
}

/**
 * Редкость травы. Соответствует PotionCraft.Contracts.Enums.RarityEnum.
 */
export enum RarityEnum {
  Common = 0,
  Unusual = 1,
  Rare = 2,
  VeryRare = 3,
}

export const RARITY_NAMES: Record<RarityEnum, string> = {
  [RarityEnum.Common]: 'Обычный',
  [RarityEnum.Unusual]: 'Необычный',
  [RarityEnum.Rare]: 'Редкий',
  [RarityEnum.VeryRare]: 'Очень редкий',
}

/** Tailwind-классы badge для отображения редкости. */
export const RARITY_BADGE_CLASSES: Record<RarityEnum, string> = {
  [RarityEnum.Common]: 'bg-slate-500',
  [RarityEnum.Unusual]: 'bg-emerald-600',
  [RarityEnum.Rare]: 'bg-blue-600',
  [RarityEnum.VeryRare]: 'bg-amber-500',
}

/**
 * Результат варки зелья. Соответствует PotionCraft.Contracts.Enums.RollResultEnum.
 */
export enum RollResultEnum {
  CriticalFailure = 0,
  Failure = 1,
  Success = 2,
  CriticalSuccess = 3,
}

/**
 * Тип зелья. Соответствует PotionCraft.Contracts.Enums.PotionTypeEnum.
 */
export enum PotionTypeEnum {
  Potion = 0,
  Poison = 1,
  Magic = 2,
}

export const POTION_TYPE_NAMES: Record<PotionTypeEnum, string> = {
  [PotionTypeEnum.Potion]: 'Зелье',
  [PotionTypeEnum.Poison]: 'Яд',
  [PotionTypeEnum.Magic]: 'Магическое зелье',
}

/**
 * Тип травы (флаги). Соответствует PotionCraft.Contracts.Enums.HerbTypeEnum.
 */
export enum HerbTypeEnum {
  None = 0,
  HealingBase = 1 << 0,
  HealingModifier = 1 << 1,
  PoisonBase = 1 << 2,
  PoisonModifier = 1 << 3,
  Magic = 1 << 4,
}

export const HERB_TYPE_NAMES: Record<HerbTypeEnum, string> = {
  [HerbTypeEnum.None]: 'Тип неопределен',
  [HerbTypeEnum.HealingBase]: 'Основа лечебного зелья',
  [HerbTypeEnum.HealingModifier]: 'Модификатор лечебного зелья',
  [HerbTypeEnum.PoisonBase]: 'Основа яда',
  [HerbTypeEnum.PoisonModifier]: 'Модификатор яда',
  [HerbTypeEnum.Magic]: 'Магический ингредиент',
}

/** Проверяет, содержит ли значение флагов HerbTypeEnum указанный флаг. */
export function hasHerbType(value: number, flag: HerbTypeEnum): boolean {
  return (value & flag) === flag
}

/**
 * Тип кости базовой формулы эффекта. Соответствует PotionCraft.Contracts.Enums.DiceTypeEnum.
 */
export enum DiceTypeEnum {
  D2 = 2,
  D4 = 4,
  D6 = 6,
  D8 = 8,
  D10 = 10,
  D12 = 12,
  D20 = 20,
  D100 = 100,
}

/**
 * Тип урона эффекта зелья/яда. Соответствует PotionCraft.Contracts.Enums.DamageTypeEnum.
 */
export enum DamageTypeEnum {
  Poison = 0,
  Cold = 1,
  Necrotic = 2,
  Fire = 3,
  Acid = 4,
  Radiant = 5,
}

export const DAMAGE_TYPE_NAMES: Record<DamageTypeEnum, string> = {
  [DamageTypeEnum.Poison]: 'Яд',
  [DamageTypeEnum.Cold]: 'Холод',
  [DamageTypeEnum.Necrotic]: 'Некротическая энергия',
  [DamageTypeEnum.Fire]: 'Огонь',
  [DamageTypeEnum.Acid]: 'Кислота',
  [DamageTypeEnum.Radiant]: 'Свет',
}

/**
 * Машиночитаемый эффект травы-модификатора. Соответствует PotionCraft.Contracts.Enums.HerbModifierEffectEnum.
 */
export enum HerbModifierEffectEnum {
  None = 0,
  HealingUpgradeDiceType = 1,
  HealingDoubleCountHalveResultOverTime = 2,
  HealingDoubleCountRemoveAlchemyMod = 3,
  PoisonUpgradeDiceType = 4,
  PoisonDoubleCountHalveDuration = 5,
  PoisonChangeToColdOrNecrotic = 6,
  PoisonChangeToFireOrAcid = 7,
  PoisonChangeToRadiant = 8,
  PoisonNonLethalUnconscious = 9,
  PoisonDelayedNotice = 10,
  PoisonSlowTarget = 11,
  PoisonDisadvantageOnChecks = 12,
  UniversalInvertEffect = 13,
  UniversalDelayEffect = 14,
  UniversalStabilizeBrew = 15,
  FoodSourceConversion = 16,
}

/**
 * Дополнительные правила сбора травы. Соответствует PotionCraft.Contracts.Enums.AdditionalRulesEnum.
 */
export enum AdditionalRulesEnum {
  None = 0,
  OneOnly = 1,
  TwoOnly = 2,
  TwoIfRain = 3,
  TwoIfCave = 4,
  AddElementalWater = 5,
  NightTwoDayReroll = 6,
  ReRoll = 7,
  CheckCommon = 8,
  ReRollIfNoProvisions = 9,
  OnlyOnCoast = 10,
  OneOrTwo = 11,
}
