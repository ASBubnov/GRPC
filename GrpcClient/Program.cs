using Grpc.Net.Client;
using BillingC;
using Grpc.Core;

// создаем канал для обмена сообщениями с сервером
// параметр - адрес сервера gRPC
//AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
var httpHandler = new HttpClientHandler();
httpHandler.ServerCertificateCustomValidationCallback =
    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
using var channel = GrpcChannel.ForAddress("http://localhost:5201");
// создаем клиент
var client = new Billing.BillingClient(channel);

//Вывод пользователей
Console.Write("Пользователи:\n");
var replyU = client.ListUsers(new None());
while (await replyU.ResponseStream.MoveNext(new CancellationToken()))
{
    var current = replyU.ResponseStream.Current;
    Console.WriteLine($"Имя: {current.Name} Рейтинг:{current.Rating} Баланс: {current.Amount}");
}
Console.WriteLine();

//Эмиссия
Console.Write("Введите количество монет для эмиссии:");
var amount = Convert.ToInt32(Console.ReadLine());
var replyE = client.CoinsEmission(new EmissionAmount() {Amount = amount});
Console.WriteLine($"{replyE.Status} {replyE.Comment}");
Console.WriteLine();

//Вывод пользователей
Console.Write("Пользователи:\n");
var replyUE = client.ListUsers(new None());
while (await replyUE.ResponseStream.MoveNext(new CancellationToken()))
{
    var current = replyUE.ResponseStream.Current;
    Console.WriteLine($"Имя: {current.Name} Рейтинг:{current.Rating} Баланс: {current.Amount}");
}
Console.WriteLine();

//перемещение монет
Console.Write("Перемещение монет(по умолчанию с Бориса на Олега):");
var amountCoins = Convert.ToInt32(Console.ReadLine());
var replyMC = client.MoveCoins(new MoveCoinsTransaction() { Amount = amountCoins, SrcUser = "boris", DstUser = "oleg" });
Console.WriteLine($"{replyMC.Status} {replyMC.Comment}");
Console.WriteLine();


//Вывод пользователей
Console.Write("Пользователи:\n");
var replyUAM = client.ListUsers(new None());
while (await replyUAM.ResponseStream.MoveNext(new CancellationToken()))
{
    var current = replyUAM.ResponseStream.Current;
    Console.WriteLine($"Имя: {current.Name} Рейтинг:{current.Rating} Баланс: {current.Amount}");
}
Console.WriteLine();

//самая длинная история
Console.Write("Монета с самой длинной историей:\n");
var replyHC = client.LongestHistoryCoin(new None());
Console.WriteLine($"ID: {replyHC.Id}\nИстория:{replyHC.History}");
Console.WriteLine();


Console.ReadKey();