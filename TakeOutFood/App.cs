namespace TakeOutFood
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class App
    {
        private readonly IItemRepository itemRepository;
        private readonly ISalesPromotionRepository salesPromotionRepository;

        public App(IItemRepository itemRepository, ISalesPromotionRepository salesPromotionRepository)
        {
            this.itemRepository = itemRepository;
            this.salesPromotionRepository = salesPromotionRepository;
        }

        public string BestCharge(List<string> inputs)
        {
            List<Item> items = itemRepository.FindAll();
            List<SalesPromotion> salesPromotions = salesPromotionRepository.FindAll();

            var inputsFull = inputs.Select(item => item.Split(' ')).Join(items, input => input[0], item => item.Id,
                (input, item) => new
                {
                    id = input[0],
                    name = item.Name,
                    count = int.Parse(input[2]),
                    price = item.Price
                }).ToList();
            var originPrice = inputsFull.Select(item => item.price * item.count).Aggregate((x, y) => x + y);
            var originPriceDescription = inputsFull.Select(item => $"{ item.name} x {item.count} = {item.price * item.count} yuan\n").Aggregate($"============= Order details =============\n", (x, y) => x + y);

            string savedPriceDescription = null;
            double savedPrice = 0;
            foreach (var salesPromotion in salesPromotions)
            {
                var savedItems = inputsFull.Join(salesPromotion.RelatedItems, input => input.id, related => related, (input, related) => input).ToList();
                if (savedItems.Count == 0)
                {
                    continue;
                }
                var promotionSavedPriceDescription = savedItems.Select(item => $"{item.name}, ").Aggregate((x, y) => x + y).TrimEnd(new char[] { ' ', ',' });
                var promotionSavedPrice = savedItems.Select(item => item.price * item.count * 0.5).Aggregate((x, y) => x + y);
                if (promotionSavedPrice > savedPrice)
                {
                    savedPrice = promotionSavedPrice;
                    savedPriceDescription = promotionSavedPriceDescription;
                }
            }

            var savingDescription = savedPriceDescription == null ?
                            $"-----------------------------------\n" +
                            $"Total：{originPrice} yuan\n" +
                            $"==================================="
                            : "-----------------------------------\n" +
                            "Promotion used:\n" +
                            $"{salesPromotions[0].DisplayName} ({savedPriceDescription}), saving {savedPrice} yuan\n" +
                            $"-----------------------------------\n" +
                            $"Total：{originPrice - savedPrice} yuan\n" +
                            $"===================================";
            return $"{originPriceDescription}{savingDescription}";
        }
    }
}
