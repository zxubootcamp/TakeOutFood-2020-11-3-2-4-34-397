namespace TakeOutFood
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class App
    {
        private IItemRepository itemRepository;
        private ISalesPromotionRepository salesPromotionRepository;

        public App(IItemRepository itemRepository, ISalesPromotionRepository salesPromotionRepository)
        {
            this.itemRepository = itemRepository;
            this.salesPromotionRepository = salesPromotionRepository;
        }

        public string BestCharge(List<string> inputs)
        {
            StringBuilder resultBuilderOrigin = new StringBuilder("============= Order details =============\n");
            double originPrice = 0;
            SortedDictionary<string, int> inputItems = new SortedDictionary<string, int>();
            //TODO: write code here
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

            List<Item> items = itemRepository.FindAll();
            foreach (var itemId in inputItems.Keys)
            {
                Item itemInfo = items.Where(c => c.Id == itemId).FirstOrDefault();
                double currentPrice = inputItems[itemId] * itemInfo.Price;
                resultBuilderOrigin.Append($"{itemInfo.Name} x {inputItems[itemId]} = {currentPrice} yuan\n");
                originPrice += currentPrice;
            }

            List<SalesPromotion> salesPromotions = salesPromotionRepository.FindAll();
            StringBuilder resultBuilderDiscount = new StringBuilder();
            double totalSaving = 0;
            foreach (var salesPromotion in salesPromotions)
            {
                if (salesPromotion.Type == "50%_DISCOUNT_ON_SPECIFIED_ITEMS")
                {
                    double saving = 0;
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var itemId in salesPromotion.RelatedItems)
                    {
                        if (inputItems.ContainsKey(itemId))
                        {
                            Item item = items.Where(c => c.Id == itemId).FirstOrDefault();
                            stringBuilder.Append(item.Name + ", ");
                            saving += item.Price * 0.5;
                        }
                    }
                    if (stringBuilder.Length > 0)
                    {
                        string names = stringBuilder.ToString().TrimEnd(new char[] { ' ', ',' });
                        resultBuilderDiscount.Append($"{salesPromotion.DisplayName} ({names}), saving {saving} yuan\n");
                    }
                    totalSaving += saving;
                }
            }
            if (resultBuilderDiscount.Length > 0)
            {
                resultBuilderDiscount.Insert(0, "Promotion used:\n");
                resultBuilderDiscount.Insert(0, "-----------------------------------\n");
            }
            resultBuilderDiscount.Append("-----------------------------------\n");
            resultBuilderDiscount.Append($"Total：{originPrice - totalSaving} yuan\n");
            resultBuilderDiscount.Append("===================================");
            string result = resultBuilderOrigin.ToString() + resultBuilderDiscount.ToString();
            Console.WriteLine(result);
            return result;
        }
    }
}
