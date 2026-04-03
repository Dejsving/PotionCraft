using PotionCraft.Contracts.Enums;

namespace PotionCraft.Contracts.Services
{
    public static class HerbPriceCalculator
    {
        /// <summary>
        /// Базовая цена
        /// </summary>
        /// <remarks>
        /// Первые 4 цифры – цена продажи,
        /// последние 4 чилса – цена покупки,
        /// в зависимости от редкости травы
        /// </remarks>
        private static readonly int[] _basePrice = { 5, 15, 40, 100, 1000 };

        /// <summary>
        /// Амплитуда случайного разброса. 0.1 = ±10%.
        /// </summary>
        private static double _delta { get; set; } = 0.1;

        /// <summary>
        /// Скорость падения цены от количества. Больше = быстрее падает.
        /// Зависит от редкости (0.1 для частых, 0.5 для очень редких).
        /// </summary>
        private static double GetPriceDecayRate(RarityEnum rarity)
        {
            return 0.1 + (int)rarity / 3.0 * 0.4;
        }

        /// <summary>
        /// Минимальный множитель цены (пол).
        /// Зависит от редкости (0.5 для частых, 0.1 для очень редких).
        /// </summary>
        private static double GetAlpha(RarityEnum rarity)
        {
            return 0.5 - (int)rarity / 3.0 * 0.4;
        }

        /// <summary>
        /// Общая формула расчёта цены с учётом редкости, количества и направления сделки (покупка/продажа).
        /// </summary>
        /// <param name="rarity">Уровень редкости предмета.</param>
        /// <param name="quantity">Количество товаров.</param>
        /// <param name="isBuy">true — цена покупки, false — цена продажи.</param>
        /// <returns>Рассчитанная цена, округлённая до двух знаков после запятой.</returns>
        private static double CalculatePrice(RarityEnum rarity, int quantity, bool isBuy)
        {
            var alpha = GetAlpha(rarity);
            var priceDecayRate = GetPriceDecayRate(rarity);
            var expFactor = Math.Exp(-priceDecayRate * (quantity - 1));

            double factor;
            int basePriceIndex;

            if (isBuy)
            {
                // Фактор спроса: экспоненциальный рост цены при покупке большого количества
                // При quantity = 1 множитель равен 1, при росте стремится к 1 / alpha
                var inverseAlpha = 1.0 / alpha;
                factor = inverseAlpha - (inverseAlpha - 1.0) * expFactor;
                basePriceIndex = (int)rarity + 1;
            }
            else
            {
                // Фактор предложения: экспоненциальное снижение к полу alpha
                // При quantity = 1 множитель равен 1, при росте стремится к alpha
                factor = alpha + (1.0 - alpha) * expFactor;
                basePriceIndex = (int)rarity;
            }

            // Случайный шум в диапазоне [-_delta, +_delta]
            var noise = 1.0 + _delta * (Random.Shared.NextDouble() * 2.0 - 1);

            var price = _basePrice[basePriceIndex] * factor * noise;

            return Math.Round(price, 2);
        }

        /// <summary>
        /// Рассчитывает цену продажи товара на основе его редкости и запрашиваемого количества.
        /// </summary>
        /// <param name="rarity">Уровень редкости предмета.</param>
        /// <param name="quantity">Количество товаров для покупки.</param>
        /// <returns>Рассчитанная цена покупки, округленная до двух знаков после запятой.</returns>
        public static double GetSellPrice(RarityEnum rarity, int quantity)
            => CalculatePrice(rarity, quantity, isBuy: false);

        /// <summary>
        /// Рассчитывает цену покупки товара на основе его редкости и запрашиваемого количества.
        /// </summary>
        /// <param name="rarity">Уровень редкости предмета.</param>
        /// <param name="quantity">Количество товаров для покупки.</param>
        /// <returns>Рассчитанная цена покупки, округленная до двух знаков после запятой.</returns>
        public static double GetBuyPrice(RarityEnum rarity, int quantity)
            => CalculatePrice(rarity, quantity, isBuy: true);
    }
}