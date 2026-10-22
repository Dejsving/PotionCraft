import type {
  DamageTypeEnum,
  DiceTypeEnum,
  HerbModifierEffectEnum,
  HerbTypeEnum,
  PotionTypeEnum,
  RarityEnum,
  TerrainEnum,
} from './enums'

/**
 * Владение инструментом персонажа. Соответствует PotionCraft.Contracts.Tool.
 */
export interface Tool {
  proficiency: boolean
  expertise: boolean
  modifier: number
}

/**
 * Трава — растение, используемое в гербализме и алхимии. Соответствует PotionCraft.Contracts.Models.Herb.
 */
export interface Herb {
  id: string
  name: string
  description: string
  herbType: HerbTypeEnum
  rarity: RarityEnum
  effect: string
  modifierEffect: HerbModifierEffectEnum
  difficulty: number
  formulaDiceCount: number | null
  formulaDiceType: DiceTypeEnum | null
  formulaIncludesAlchemyMod: boolean | null
  formulaPrefix: string | null
  damageType: DamageTypeEnum | null
  replacementDamageTypes: DamageTypeEnum[] | null
  requiresDmJudgement: boolean
  /** Ключи — числовое значение TerrainEnum в виде строки (сериализация Dictionary в JSON). */
  habitats: Record<string, number>
}

/**
 * Результат сбора трав. Соответствует PotionCraft.Contracts.Models.GatheringResult.
 */
export interface GatheringResult {
  herb: Herb | null
  quantity: number
}

/**
 * Рецепт алхимического зелья. Соответствует PotionCraft.Contracts.Models.AlchemyRecipe.
 */
export interface AlchemyRecipe {
  id: string
  name: string
  description: string
  potionType: PotionTypeEnum
  brewingDC: number
  ingredients: Herb[]
  modifications: string[]
}

/**
 * Готовое зелье. Соответствует PotionCraft.Contracts.Models.Potion.
 */
export interface Potion {
  id: string
  name: string
  description: string
  type: PotionTypeEnum
  recipe: AlchemyRecipe
  durationRounds: number
}

/**
 * Элемент сумки — зелье или яд с указанием количества. Соответствует PotionCraft.Contracts.Models.PotionBagItem.
 */
export interface PotionBagItem {
  potion: Potion | null
  quantity: number
}

/**
 * Сумка персонажа. Соответствует PotionCraft.Contracts.Models.CharacterBag.
 */
export interface CharacterBag {
  id: string
  /** Ключи — идентификатор травы (Guid). */
  herbs: Record<string, GatheringResult>
  /** Ключи — идентификатор зелья (Guid). */
  potions: Record<string, PotionBagItem>
  /** Ключи — идентификатор яда (Guid). */
  poisons: Record<string, PotionBagItem>
  coins: number
  goldCoins: number
  silverCoins: number
  copperCoins: number
}

/**
 * Персонаж игрока. Соответствует PotionCraft.Contracts.PlayerCharacter.
 */
export interface PlayerCharacter {
  id: string
  name: string
  intelligence: number
  wisdom: number
  proficiencyBonus: number
  alchemistTool: Tool
  herbalismTool: Tool
  poisonerTool: Tool
  bag: CharacterBag
  selectedBy: string | null
  wisdomModifier: number
  intelligenceModifier: number
  alchemistModify: number
  herbalismModify: number
  poisonerModify: number
}

/**
 * Элемент ассортимента магазина. Соответствует PotionCraft.Contracts.Models.ShopItem.
 */
export interface ShopItem {
  id: string
  name: string
  category: 'herb' | 'potion' | 'poison'
  rarity: number
  rarityName: string
  buyPrice: number
  sellPrice: number
  availableQuantity: number
  herbType: number
}

/**
 * Элемент сделки — предмет с указанием количества. Соответствует PotionCraft.Contracts.Models.TradeItem.
 */
export interface TradeItem {
  itemId: string
  quantity: number
  category: 'herb' | 'potion' | 'poison'
}

/**
 * Запрос на выполнение сделки в магазине. Соответствует PotionCraft.Contracts.Models.TradeRequest.
 */
export interface TradeRequest {
  characterId: string
  itemsToBuy: TradeItem[]
  itemsToSell: TradeItem[]
}

/**
 * Результат выполнения сделки в магазине. Соответствует PotionCraft.Contracts.Models.TradeResult.
 */
export interface TradeResult {
  success: boolean
  balanceChange: number
  message: string
  updatedBag: CharacterBag | null
}

/**
 * Результат применения модификаторов к базовому эффекту травы. Соответствует PotionCraft.Contracts.Models.PotionEffectResult.
 */
export interface PotionEffectResult {
  diceCount: number
  diceType: DiceTypeEnum | null
  includesAlchemyMod: boolean
  damageType: DamageTypeEnum | null
  damageTypeChoices: DamageTypeEnum[]
  notes: string[]
  dmJudgementNote: string | null
  isFixedEffect: boolean
  formulaText: string
  generatedDescription: string
}

// ----- Пользователи и авторизация -----

/** Роль пользователя: Admin — администратор, Master — мастер, Player — игрок. */
export type UserRole = 'Admin' | 'Master' | 'Player'

/** Текущий авторизованный пользователь. Соответствует ответу GET /api/auth/me. */
export interface CurrentUser {
  id: string
  userName: string
  email: string
  roles: string[]
}

/** Краткие данные пользователя для списка администратора. Соответствует ответу GET /api/users. */
export interface UserSummary {
  id: string
  userName: string
  email: string
  roles: string[]
}

/** Тело запроса POST /api/auth/register. */
export interface RegisterRequest {
  userName: string
  email: string
  password: string
}

/** Ответ POST /api/auth/register: пользователь создан, но сессия не выставлена до подтверждения email. */
export interface RegisterResponse {
  email: string
  requiresEmailConfirmation: true
}

/** Тело запроса POST /api/auth/confirm-email. */
export interface ConfirmEmailRequest {
  email: string
  code: string
  rememberMe?: boolean
}

/** Тело ответа 403 POST /api/auth/login: пароль верный, но email не подтверждён (код уже отправлен). */
export interface EmailConfirmationRequiredBody {
  message: string
  requiresEmailConfirmation: true
  email: string
}

/** Тело запроса POST /api/auth/login. Логин — имя пользователя или email. */
export interface LoginRequest {
  login: string
  password: string
  rememberMe: boolean
}

// ----- Запросы/ответы API, специфичные для контроллеров сервера -----

/** Тело запроса POST /api/characters. */
export interface CreateCharacterRequest {
  name: string
  intelligence: number
  wisdom: number
  proficiencyBonus: number
  herbalismProficiencyLevel: number
  herbalismToolModifier: number
  alchemistProficiencyLevel: number
  alchemistToolModifier: number
  poisonerProficiencyLevel: number
  poisonerToolModifier: number
}

/** Тело запроса POST /api/characters/{id}/bag/coins. */
export interface CoinsUpdateRequest {
  gold: number
  silver: number
  copper: number
  isAdd: boolean
}

/** Тело запроса POST /api/gathering/{characterId}. */
export interface GatheringApiRequest {
  terrain: TerrainEnum
  isRaining: boolean
  isNight: boolean
  isCave: boolean
  hasProvisions: boolean
  rollsCount: number
  difficulty: number
}

/** Ответ POST /api/gathering/{characterId}. */
export interface GatheringApiResponse {
  characterName: string
  rollResults: number[]
  totalSuccesses: number
  /** Ключи — идентификатор травы (Guid). */
  gatheredHerbs: Record<string, GatheringResult>
}

/** Тело запроса POST /api/alchemy/resolve. */
export interface ResolvePotionEffectRequest {
  baseHerbId: string
  modifierHerbIds: string[]
}
