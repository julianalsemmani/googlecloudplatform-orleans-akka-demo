using Akka.Actor;
using Akka.Cluster.Hosting;
using Akka.Cluster.Sharding;
using Akka.DependencyInjection;
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
using AkkaShopDemo.Extractor;
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
                var serviceProvider = builder.Services.BuildServiceProvider();
                builder.Services.AddAkka("AkkaShopDemo", configurationBuilder =>
                {
                    const int remotingPort = 8110;
                    const int managementPort = 8558;
                    string hostname = "";

                    if (builder.Environment.IsDevelopment())
                    {
                        hostname = "localhost";
                    }
                    else
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
                            var sp = serviceProvider; // hentet utenfor

                            var sharding = ClusterSharding.Get(system);
                            var settings = ClusterShardingSettings.Create(system).WithRememberEntities(true);

                            // 1. Start sharded CartActor
                            var cartRegion = sharding.Start(
                                typeName: nameof(CartActor),
                                entityPropsFactory: entityId => Props.Create(() => new CartActor(sp.GetRequiredService<ILogger<CartActor>>())),
                                settings: settings,
                                messageExtractor: new CartMessageExtractor()
                            );
                            registry.Register<CartActor>(cartRegion);

                            // 2. Start sharded CheckoutActor
                            var checkoutRegion = sharding.Start(
                                typeName: nameof(CheckoutActor),
                                entityPropsFactory: entityId => resolver.Props<CheckoutActor>(),
                                settings: settings,
                                messageExtractor: new CheckoutMessageExtractor()
                            );
                            registry.Register<CheckoutActor>(checkoutRegion);

                            // 3. Start sharded ShippingActor
                            var shippingRegion = sharding.Start(
                                typeName: nameof(ShippingActor),
                                entityPropsFactory: entityId => Props.Create(() => new ShippingActor(sp.GetRequiredService<ILogger<ShippingActor>>())),
                                settings: settings,
                                messageExtractor: new ShippingMessageExtractor()
                            );
                            registry.Register<ShippingActor>(shippingRegion);

                            // 4. Start sharded PaymentActor
                            var paymentRegion = sharding.Start(
                                typeName: nameof(PaymentActor),
                                entityPropsFactory: entityId => Props.Create(() => new PaymentActor(sp.GetRequiredService<ILogger<PaymentActor>>())),
                                settings: settings,
                                messageExtractor: new PaymentMessageExtractor()
                            );
                            registry.Register<PaymentActor>(paymentRegion);

                            // 5. Singleton actors (ikke sharded)
                            var currencyActor = system.ActorOf(resolver.Props<CurrencyActor>(), "currencyActor");
                            var productCatalogActor = system.ActorOf(resolver.Props<ProductCatalogActor>(), "productCatalogActor");
                            var recommendationActor = system.ActorOf(resolver.Props<RecommendationActor>(), "recommendationActor");

                            registry.Register<CurrencyActor>(currencyActor);
                            registry.Register<ProductCatalogActor>(productCatalogActor);
                            registry.Register<RecommendationActor>(recommendationActor);
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
