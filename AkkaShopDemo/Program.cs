using Akka.Cluster.Hosting;
using Akka.Discovery.Config.Hosting;
using Akka.Hosting;
using Akka.Management;
using Akka.Management.Cluster.Bootstrap;
using Akka.Remote.Hosting;
using AkkaShopDemo.Actors.Cart;
using AkkaShopDemo.Actors.Checkout;
using AkkaShopDemo.Actors.Currency;
using AkkaShopDemo.Actors.Payment;
using AkkaShopDemo.Actors.ProductCatalog;
using AkkaShopDemo.Actors.Recommendation;
using AkkaShopDemo.Actors.Shipping;
using AkkaShopDemo.Services;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;


namespace AkkaShopDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddOpenTelemetry().UseAzureMonitor(o =>
                {
                    o.ConnectionString = "InstrumentationKey=a84f3e65-6343-4ead-8d1f-644c3a90b8b1";
                    // Set the Credential property to enable AAD based authentication:
                    // o.Credential = new DefaultAzureCredential();
                });

                builder.Logging.ClearProviders();
                builder.Logging.AddConsole();
                builder.Logging.AddDebug();

                builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                builder.WebHost.ConfigureKestrel((context, options) =>
                {
                    options.Configure(context.Configuration.GetSection("Kestrel"));
                });

                builder.Services.AddGrpcHealthChecks()
                .AddCheck("Sample", () => HealthCheckResult.Healthy());

                builder.Services.AddGrpc();
                builder.Services.AddGrpcReflection();
                builder.Services.AddAkka("AkkaShopDemo", configurationBuilder =>
                {
                    const int remotingPort = 8110;
                    const int managementPort = 8558;
                    string hostname = "";

                    if (builder.Environment.IsDevelopment())
                    {
                        hostname = "localhost";
                    } else
                    {
                        hostname = GetPublicIpAddress();
                    }


                    configurationBuilder
                        .WithAkkaManagement(new AkkaManagementOptions
                        {
                            HostName = hostname,
                            BindHostName = hostname,
                            Port = managementPort,
                            BindPort = managementPort
                        })
                        .WithClusterBootstrap(setup =>
                        {
                            setup.ContactPointDiscovery.ServiceName = "AkkaShopDemo";
                            setup.ContactPointDiscovery.RequiredContactPointsNr = 1;
                        })
                        .WithConfigDiscovery(options =>
                        {
                            options.Services = new List<Service>
                            {
                                new Service
                                {
                                    Name = "AkkaShopDemo",
                                    Endpoints = [ $"{hostname}:{managementPort}" ]
                                }
                            };
                        })
                        .WithRemoting(hostname, remotingPort)
                        .WithClustering()
                        .WithActors((system, registry, resolver) =>
                        {
                            var cartActorProps = resolver.Props<CartActor>();
                            var cartActor = system.ActorOf(cartActorProps, "cartActor");

                            var checkoutActorProps = resolver.Props<CheckoutActor>();
                            var checkoutActor = system.ActorOf(checkoutActorProps, "checkoutActor");

                            var productCatalogActorProps = resolver.Props<ProductCatalogActor>();
                            var productCatalogActor = system.ActorOf(productCatalogActorProps, "productCatalogActor");

                            var recommendationActorProps = resolver.Props<RecommendationActor>();
                            var recommendationActor = system.ActorOf(recommendationActorProps, "recommendationActor");

                            var shippingActorProps = resolver.Props<ShippingActor>();
                            var shippingActor = system.ActorOf(shippingActorProps, "shippingActor");

                            var paymentActorProps = resolver.Props<PaymentActor>();
                            var paymentActor = system.ActorOf(paymentActorProps, "paymentActor");

                            var currencyActorProps = resolver.Props<CurrencyActor>();
                            var currencyActor = system.ActorOf(currencyActorProps, "currencyActor");

                            registry.Register<CartActor>(cartActor);
                            registry.Register<CheckoutActor>(checkoutActor);
                            registry.Register<ProductCatalogActor>(productCatalogActor);
                            registry.Register<RecommendationActor>(recommendationActor);
                            registry.Register<ShippingActor>(shippingActor);
                            registry.Register<PaymentActor>(paymentActor);
                            registry.Register<CurrencyActor>(currencyActor);
                        });
                });

                var app = builder.Build();

                app.MapGrpcService<CurrencyService>();
                app.MapGrpcService<ProductCatalogService>();
                app.MapGrpcService<RecommendationService>();
                app.MapGrpcService<ShippingService>();
                app.MapGrpcService<PaymentService>();
                app.MapGrpcService<CartService>();
                app.MapGrpcService<CheckoutService>();

                app.MapGrpcReflectionService();

                app.MapGrpcHealthChecksService().AllowAnonymous();

                app.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static string GetPublicIpAddress()
        {
            var hostName = Dns.GetHostName();
            var addresses = Dns.GetHostAddresses(hostName);
            var ip = addresses.FirstOrDefault(ip =>
                ip.AddressFamily == AddressFamily.InterNetwork &&
                !Equals(ip, IPAddress.Any) &&
                !Equals(ip, IPAddress.Loopback));
            if (ip is null)
                throw new Exception($"Could not find a valid public IP address for host name {hostName}");

            return ip.ToString();
        }
    }
}
