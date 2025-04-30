using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Serialization;
using OrleansShopDemo.Grains.Cart;
using OrleansShopDemo.Grains.Cart.Interfaces;
using OrleansShopDemo.Grains.Checkout;
using OrleansShopDemo.Grains.Checkout.Interfaces;
using OrleansShopDemo.Grains.Currency;
using OrleansShopDemo.Grains.Currency.Interfaces;
using OrleansShopDemo.Grains.Payment;
using OrleansShopDemo.Grains.Payment.Interfaces;
using OrleansShopDemo.Grains.ProductCatalog;
using OrleansShopDemo.Grains.ProductCatalog.Interfaces;
using OrleansShopDemo.Services;
using System;

namespace OrleansShopDemo
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
                    o.ConnectionString = "InstrumentationKey=81647713-4f8b-4792-b7a0-fe8e7ed832a9";
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
                builder.Services.AddScoped<ICurrencyGrain, CurrencyGrain>();
                builder.Services.AddScoped<IProductCatalogGrain, ProductCatalogGrain>();
                builder.Services.AddScoped<IPaymentGrain, PaymentGrain>();
                builder.Services.AddScoped<ICartGrain, CartGrain>();
                builder.Services.AddScoped<ICheckoutGrain, CheckoutGrain>();

                builder.Host.UseOrleans((hostBuilder, siloBuilder) =>
                {
                    var configuration = hostBuilder.Configuration;
                    siloBuilder.Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = configuration.GetValue<string>("Orleans:Cluster:ClusterId");
                        options.ServiceId = configuration.GetValue<string>("Orleans:Cluster:ServiceId");
                    });

                    siloBuilder.AddMemoryGrainStorageAsDefault();
                    siloBuilder.Services.AddSerializer(serializerBuilder => serializerBuilder.AddProtobufSerializer());
                    #pragma warning disable ORLEANSEXP003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                    siloBuilder.AddDistributedGrainDirectory();
                    #pragma warning restore ORLEANSEXP003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

                    siloBuilder.UseAdoNetClustering(options =>
                    {
                        options.Invariant = configuration.GetValue<string>("Orleans:Cluster:Invariant");
                        options.ConnectionString = configuration.GetValue<string>("Orleans:Cluster:ConnectionString");
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
    }
}
