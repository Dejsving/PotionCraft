using PotionCraft.Contracts.Enums;

namespace PotionCraft.Contracts.Models
{
    internal class HerbPrice
    {
        private readonly Random _random = new Random();

        /// <summary>
        /// Базовая цена
        /// </summary>
        /// <remarks>
        /// Первые 4 цифры – цена продажи,
        /// последние 4 чилса – цена покупки,
        /// в зависимости от редкости травы
        /// </remarks>
        private readonly int[] _basePrice = { 5, 15, 40, 100, 1000 };

        /// <summary>
        /// Скорость падения цены от количества. Больше = быстрее падает.
        /// Зависит от редкости (0.1 для частых, 0.5 для очень редких).
        /// </summary>
        private double GetPriceDecayRate(RarityEnum rarity)
        {
            return 0.1 + (int)rarity / 3.0 * 0.4;
        }

        /// <summary>
        /// Амплитуда случайного разброса. 0.1 = ±10%.
        /// </summary>
        private double _delta { get; set; } = 0.1;

        /// <summary>
        /// Минимальный множитель цены (пол).
        /// Зависит от редкости (0.5 для частых, 0.1 для очень редких).
        /// </summary>
        private double GetAlpha(RarityEnum rarity)
        {
            return 0.5 - (int)rarity / 3.0 * 0.4;
        }

        /// <summary>
        /// Рассчитывает цену продажи товара на основе его редкости и запрашиваемого количества.
        /// </summary>
        /// <param name="rarity">Уровень редкости предмета.</param>
        /// <param name="quantity">Количество товаров для покупки.</param>
        /// <returns>Рассчитанная цена покупки, округленная до двух знаков после запятой.</returns>
        public double GetSellPrice(RarityEnum rarity, int quantity)
        {
            var _alpha = GetAlpha(rarity);
            var _priceDecayRate = GetPriceDecayRate(rarity);

            // Фактор предложения: экспоненциальное снижение к полу _alpha
            // При quantity = 1 множитель равен 1, при росте стремится к _alpha
            var supply = _alpha + (1.0 - _alpha) * Math.Exp(-_priceDecayRate * (quantity - 1));

            // Случайный шум в диапазоне [-_delta, +_delta]
            var noise = 1.0 + _delta * (_random.NextDouble() * 2.0 - 1);

            var price = _basePrice[(int)rarity] * supply * noise;

            return Math.Round(price, 2);
        }

        /// <summary>
        /// Рассчитывает цену покупки товара на основе его редкости и запрашиваемого количества.
        /// </summary>
        /// <param name="rarity">Уровень редкости предмета.</param>
        /// <param name="quantity">Количество товаров для покупки.</param>
        /// <returns>Рассчитанная цена покупки, округленная до двух знаков после запятой.</returns>
        public double GetBuyPrice(RarityEnum rarity, int quantity)
        {
            var _alpha = GetAlpha(rarity);
            var _priceDecayRate = GetPriceDecayRate(rarity);

            // Фактор спроса: экспоненциальный рост цены при покупке большого количества
            // При quantity = 1 множитель равен 1, при росте стремится к 1 / _alpha
            var demand = (1.0 / _alpha) - ((1.0 / _alpha) - 1.0)
                * Math.Exp(-_priceDecayRate * (quantity - 1));

            // Случайный шум в диапазоне [-_delta, +_delta]
            var noise = 1.0 + _delta * (_random.NextDouble() * 2.0 - 1);

            var price = _basePrice[(int)rarity + 1] * demand * noise;

            return Math.Round(price, 2);
        }
    }
}