using Grpc.Core;
using BillingS;

namespace GrpcServiceTest.Services
{
    public class BillingService : Billing.BillingBase
    {
        private static List<UserProfile> users = new List<UserProfile>()
            {
                new UserProfile()
                {
                    Name = "boris",
                    Rating = 5000
                },
                new UserProfile()
                {
                    Name = "maria",
                    Rating = 1000
                },
                new UserProfile()
                {
                    Name = "oleg",
                    Rating = 800
                },
            };
        private static List<Coin> Coins = new List<Coin>();
        /// <summary>
        /// Получение пользователей
        /// </summary>
        public override async Task ListUsers(None none, IServerStreamWriter<UserProfile> userProfiles, ServerCallContext context)
        {
            foreach (UserProfile profile in users)
            {
                await userProfiles.WriteAsync(profile);
                await Task.Delay(TimeSpan.FromSeconds(1));
            }            
        }

        /// <summary>
        /// Эмиссия валюты
        /// </summary>
        public override Task<Response> CoinsEmission(EmissionAmount emission, ServerCallContext context)
        {
            if (emission.Amount < users.Count)
                return Task.FromResult(new Response
                {
                    Status = Response.Types.Status.Unspecified,
                    Comment = "Количества монет недостаточно для распределения"
                });
            CreateCoins(emission.Amount);
            var sumRating = Convert.ToDouble(users.Sum(x => x.Rating));
            var emissoinCoins = 0;
            foreach (var profile in users.OrderBy(x => x.Rating))
            {
                var currentPercent = Convert.ToDouble(profile.Rating) / sumRating;
                var curAmount = currentPercent * emission.Amount;
                profile.Amount = (long)curAmount;
                for (int i = 0; i < profile.Amount; i++)
                {
                    var coin = Coins.Where(x => String.IsNullOrEmpty(x.History)).FirstOrDefault();
                    coin.History = $"{profile.Name},";
                    emissoinCoins++;
                }
            }
            users.FirstOrDefault(x => x.Rating == users.Max(x => x.Rating)).Amount += emission.Amount - emissoinCoins;
            return Task.FromResult(new Response
            {
                Status = Response.Types.Status.Ok,
                Comment = "Монеты распределены"
            });
        }

        /// <summary>
        /// Создание монет
        /// </summary>
        private void CreateCoins(long amount)
        {
            for (int i = 0; i < amount; i++)
            {
                var coin = new Coin()
                {
                    Id = Coins.Count + 1,
                    History = String.Empty
                };
                Coins.Add(coin);
            }
        }


        public override Task<Response> MoveCoins(MoveCoinsTransaction transaction, ServerCallContext context)
        {
            var fromUser = users.First(x => x.Name == transaction.SrcUser);
            var toUser = users.First(x => x.Name == transaction.DstUser);
            if(fromUser == null || toUser == null)
            {
                return Task.FromResult(new Response
                {
                    Status = Response.Types.Status.Unspecified,
                    Comment = "Не найден пользователь"
                });
            }

            if(fromUser.Amount < transaction.Amount )
            {
                return Task.FromResult(new Response
                {
                    Status = Response.Types.Status.Failed,
                    Comment = "Недостаточно средств для перевода"
                });
            }

            for(int i = 0; i < transaction.Amount; i++)
            {                   
                var coin = Coins.FirstOrDefault(x => x.History.Trim().Split(',' , StringSplitOptions.RemoveEmptyEntries).LastOrDefault() == fromUser.Name);
                var history = Coins.FirstOrDefault().History.Trim().Split(',', StringSplitOptions.RemoveEmptyEntries);
                coin.History += $"{toUser.Name},";
                toUser.Amount += 1;
                fromUser.Amount -= 1;
            }

            return Task.FromResult(new Response
            {
                Status = Response.Types.Status.Ok,
                Comment = "Транзакция выполнена"
            });
        }

        public override Task<Coin> LongestHistoryCoin(None none, ServerCallContext context)
        {
            var maxStory = Coins.Max(x => (x.History.Split(',', StringSplitOptions.RemoveEmptyEntries)).Length);
            var coin = Coins.OrderBy(x => x.Id).FirstOrDefault(x => x.History.Split(',', StringSplitOptions.RemoveEmptyEntries).Length == maxStory);
            return Task.FromResult(coin);
        }

    }

}
