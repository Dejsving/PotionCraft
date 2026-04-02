using PotionCraft.Contracts.Enums;
using System;

namespace PotionCraft.Contracts.Services
{
    public static class ShopInventoryGenerator
    {
        /// <summary>
        /// Шанс появления предмета в магазине (от 0.0 до 1.0).
        /// Индексы соответствуют RarityEnum: 0 - Common, 1 - Unusual, 2 - Rare, 3 - VeryRare.
        /// Common - 50%, Unusual - 30%, Rare - 10%, VeryRare - 1%.
        /// </summary>
        private static readonly double[] _spawnChances = { 0.5, 0.3, 0.1, 0.01 };

        /// <summary>
        /// Минимальное количество товара, если он всё-таки появился в магазине.
        /// Для Common (15) и Unusual (5) задан стабильный минимум.
        /// </summary>
        private static readonly int[] _minQuantities = { 15, 5, 1, 1 };

        /// <summary>
        /// Максимальное количество товара.
        /// </summary>
        private static readonly int[] _maxQuantities = { 40, 15, 3, 1 };

        /// <summary>
        /// Генерирует количество товара для заполнения магазина на основе его редкости.
        /// </summary>
        /// <param name="rarity">Уровень редкости предмета.</param>
        /// <returns>Сгенерированное количество товара (может быть 0 для редких вещей).</returns>
        public static int GetQuantityForShop(RarityEnum rarity)
        {
            int index = (int)rarity;

            // Защита от ошибки, если в Enum добавят новые редкости, но не обновят массивы
            if (index < 0 || index >= _spawnChances.Length)
            {
                return 0;
            }

            // 1. Проверяем, появится ли вообще этот товар в магазине
            if (Random.Shared.NextDouble() > _spawnChances[index])
            {
                return 0; // Не повезло, товара сегодня нет
            }

            // 2. Рассчитываем базовое количество в заданных пределах [min; max]
            int min = _minQuantities[index];
            int max = _maxQuantities[index];

            int finalQuantity = Random.Shared.Next(min, max + 1);

            return finalQuantity;
        }
    }
}