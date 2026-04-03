using PotionCraft.Contracts.Enums;
using PotionCraft.Contracts.Interfaces;

namespace PotionCraft.Contracts.Services
{
    /// <summary>
    /// Генератор инвентаря магазина на основе редкости предметов.
    /// </summary>
    public class ShopInventoryGenerator : IInventoryGenerator
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
        /// Сервис генерации случайных чисел.
        /// </summary>
        private readonly IDiceRoller _diceRoller;

        /// <summary>
        /// Создаёт экземпляр генератора инвентаря магазина.
        /// </summary>
        /// <param name="diceRoller">Сервис генерации случайных чисел.</param>
        public ShopInventoryGenerator(IDiceRoller diceRoller)
        {
            _diceRoller = diceRoller;
        }

        /// <inheritdoc />
        public int GetQuantityForShop(RarityEnum rarity)
        {
            int index = (int)rarity;

            // Защита от ошибки, если в Enum добавят новые редкости, но не обновят массивы
            if (index < 0 || index >= _spawnChances.Length)
            {
                return 0;
            }

            // 1. Проверяем, появится ли вообще этот товар в магазине
            if (_diceRoller.NextDouble() > _spawnChances[index])
            {
                return 0; // Не повезло, товара сегодня нет
            }

            // 2. Рассчитываем базовое количество в заданных пределах [min; max]
            int min = _minQuantities[index];
            int max = _maxQuantities[index];

            int finalQuantity = _diceRoller.Next(min, max + 1);

            return finalQuantity;
        }
    }
}