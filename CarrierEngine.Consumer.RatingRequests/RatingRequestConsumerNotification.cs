using System;
using System.Text;
using System.Threading.Tasks;
using CarrierEngine.Domain.Dtos;
using MassTransit;

namespace CarrierEngine.Consumer.RatingRequests
{
    public class RatingRequestConsumerNotification : IConsumer<RatingRequestDto>
    {
        public async Task Consume(ConsumeContext<RatingRequestDto> context)
        {
            await Console.Out.WriteLineAsync(
                $"Rating request for Banyan Load {context.Message.BanyanLoadId} started at {DateTime.Now}");

            var sleepTime = new Random().Next(500, 5000);
            await Task.Delay(sleepTime);

            var returnObject = new RatingResponseDto()
            {
                BanyanLoadId = context.Message.BanyanLoadId,
                QuoteAmount = GetRandomQuoteAmount(100, 400, 2),
                QuoteNumber = GetRandomQuoteNumber(8)
            };

            await Console.Out.WriteLineAsync(
                $"Rating request for Banyan Load {returnObject.BanyanLoadId} finished at {DateTime.Now}. Quote # {returnObject.QuoteNumber} for Amount: {returnObject.QuoteAmount} . Simulated work time {sleepTime}ms.");
        }


        private static double GetRandomQuoteAmount(double minValue, double maxValue, int decimalPlaces)
        {
            var randNumber = new Random().NextDouble() * (maxValue - minValue) + minValue;
            return Convert.ToDouble(randNumber.ToString("f" + decimalPlaces));
        }

        private static string GetRandomQuoteNumber(int length)
        {
            const string src = "abcdefghijklmnopqrstuvwxyz0123456789";

            var sb = new StringBuilder();
            var rng = new Random();
            for (var i = 0; i < length; i++)
            {
                var c = src[rng.Next(0, src.Length)];
                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}