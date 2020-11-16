namespace TakeOutFood
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class App
    {
        private IItemRepository itemRepository;
        private ISalesPromotionRepository salesPromotionRepository;

        public App(IItemRepository itemRepository, ISalesPromotionRepository salesPromotionRepository)
        {
            this.itemRepository = itemRepository;
            this.salesPromotionRepository = salesPromotionRepository;
        }

        public string BestCharge2(List<string> inputs)
        {
            SortedDictionary<string, int> inputItems = new SortedDictionary<string, int>();
            foreach (var item in inputs)
            {
                string[] tokenList = item.Split(' ');
                int count = int.Parse(tokenList[2]);
                if (inputItems.ContainsKey(tokenList[0]))
                {
                    inputItems[tokenList[0]] += count;
                }
                else
                {
                    inputItems[tokenList[0]] = count;
                }
            }

            StringBuilder resultBuilderOrigin = new StringBuilder();
            resultBuilderOrigin.Append("============= Order details =============\n");
            double originPrice = 0;
            List<Item> items = itemRepository.FindAll();
            var inputsFull = inputs.Select(item => item.Split(' ')).Join(items, input => input[0], item => item.Id, (input, item) => new { id = input[0], name = item.Name, count = int.Parse(input[2]), price = item.Price }).ToList();

            foreach (var itemId in inputItems.Keys)
            {
                Item itemInfo = items.Where(c => c.Id == itemId).FirstOrDefault();
                double currentPrice = inputItems[itemId] * itemInfo.Price;
                resultBuilderOrigin.Append($"{itemInfo.Name} x {inputItems[itemId]} = {currentPrice} yuan\n");
                originPrice += currentPrice;
            }

            List<SalesPromotion> salesPromotions = salesPromotionRepository.FindAll();
            StringBuilder resultBuilderSaving = new StringBuilder();
            double bestSaving = 0;
            foreach (var salesPromotion in salesPromotions)
            {
                double saving = 0;
                StringBuilder currentPromotion = new StringBuilder();
                if (salesPromotion.Type == "50%_DISCOUNT_ON_SPECIFIED_ITEMS")
                {
                    foreach (var itemId in salesPromotion.RelatedItems)
                    {
                        if (inputItems.ContainsKey(itemId))
                        {
                            Item item = items.Where(c => c.Id == itemId).FirstOrDefault();
                            currentPromotion.Append(item.Name + ", ");
                            saving += item.Price * 0.5;
                        }
                    }
                }
                if (currentPromotion.Length > 0 && saving > bestSaving)
                {
                    string names = currentPromotion.ToString().TrimEnd(new char[] { ' ', ',' });
                    resultBuilderSaving.Clear();
                    resultBuilderSaving.Append($"{salesPromotion.DisplayName} ({names}), saving {saving} yuan\n");
                    bestSaving = saving;
                }
            }

            if (resultBuilderSaving.Length > 0)
            {
                resultBuilderSaving.Insert(0, "Promotion used:\n");
                resultBuilderSaving.Insert(0, "-----------------------------------\n");
            }
            resultBuilderSaving.Append("-----------------------------------\n");
            resultBuilderSaving.Append($"Total：{originPrice - bestSaving} yuan\n");
            resultBuilderSaving.Append("===================================");
            return resultBuilderOrigin.ToString() + resultBuilderSaving.ToString();
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
