﻿using System;
using CarrierEngine.Domain;
using MassTransit;

namespace CarrierEngine.Consumer.RatingRequests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Rating Request";

            var bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri(RabbitMqConstants.RabbitMqRootUri), h =>
                {
                    h.Username(RabbitMqConstants.UserName); 
                    h.Password(RabbitMqConstants.Password);
                });

                cfg.ReceiveEndpoint(RabbitMqConstants.RatingRequestQueue, ep =>
                {
                    ep.PrefetchCount = 50;
                    ep.UseMessageRetry(r => r.Interval(2, 100));
                    ep.Consumer<RatingRequestConsumerNotification>();
                });
            });

            bus.StartAsync();

            Console.WriteLine("Listening for Rating Request events. Press enter to exit");
            Console.ReadLine();

            bus.StopAsync();
        }
    }
}
