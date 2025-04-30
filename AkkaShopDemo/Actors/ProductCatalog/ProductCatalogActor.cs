using System;
using System.Collections.Generic;
using Akka.Actor;
using Grpc.Core;
using Shop;

namespace AkkaShopDemo.Actors.ProductCatalog
{
    public class ProductCatalogActor : ReceiveActor
    {
        private static readonly List<Product> products = [
            new Product {
                Id = Guid.Parse("338BE966-7D01-4F20-9E38-8B920A057D73").ToString(),
                Name = "Sunglasses",
                Description = "Add a modern touch to your outfits with these sleek aviator sunglasses.",
                Picture = "/static/img/products/sunglasses.jpg",
                PriceUsd = new Money {
                    CurrencyCode = "USD",
                    Units = 19,
                    Nanos = 990000000
                },
                Categories = { "accessories" }
            },
            new Product {
                Id = Guid.Parse("7141F43C-9A3A-4916-9E7C-8FA7C2DCBA90").ToString(),
                Name = "Tank Top",
                Description = "Perfectly cropped cotton tank, with a scooped neckline.",
                Picture = "/static/img/products/tank-top.jpg",
                PriceUsd = new Money {
                    CurrencyCode = "USD",
                    Units = 18,
                    Nanos = 990000000
                },
                Categories = { "clothing", "tops" }
            },
            new Product {
                Id = Guid.Parse("0CA5F261-07F0-4A24-A495-33A1BAFF274E").ToString(),
                Name = "Watch",
                Description = "This gold-tone stainless steel watch will work with most of your outfits.",
                Picture = "/static/img/products/watch.jpg",
                PriceUsd = new Money {
                    CurrencyCode = "USD",
                    Units = 109,
                    Nanos = 990000000
                },
                Categories = { "accessories" }
            },
            new Product {
                Id = Guid.Parse("B762C515-CAD4-4AFD-9CCF-6AE12CE69EA8").ToString(),
                Name = "Loafers",
                Description = "A neat addition to your summer wardrobe.",
                Picture = "/static/img/products/loafers.jpg",
                PriceUsd = new Money {
                    CurrencyCode = "USD",
                    Units = 89,
                    Nanos = 990000000
                },
                Categories = { "footwear" }
            },
            new Product {
                Id = Guid.Parse("CF13645D-6FA3-4B0F-9C80-9ED533A5FC28").ToString(),
                Name = "Hairdryer",
                Description = "This lightweight hairdryer has 3 heat and speed settings. It's perfect for travel.",
                Picture = "/static/img/products/hairdryer.jpg",
                PriceUsd = new Money {
                    CurrencyCode = "USD",
                    Units = 24,
                    Nanos = 990000000
                },
                Categories = { "hair", "beauty" }
            },
            new Product {
                Id = Guid.Parse("221A3979-870D-4FAA-838A-1C299F0D4E9C").ToString(),
                Name = "Candle Holder",
                Description = "This small but intricate candle holder is an excellent gift.",
                Picture = "/static/img/products/candle-holder.jpg",
                PriceUsd = new Money {
                    CurrencyCode = "USD",
                    Units = 18,
                    Nanos = 990000000
                },
                Categories = { "decor", "home" }
            },
            new Product {
                Id = Guid.Parse("8EB8F230-268F-41CE-A694-4D6926591FA0").ToString(),
                Name = "Salt & Pepper Shakers",
                Description = "Add some flavor to your kitchen.",
                Picture = "/static/img/products/salt-and-pepper-shakers.jpg",
                PriceUsd = new Money {
                    CurrencyCode = "USD",
                    Units = 18,
                    Nanos = 490000000
                },
                Categories = { "kitchen" }
            },
            new Product {
                Id = Guid.Parse("D862C156-AB1B-4C71-A067-FC66D12F05D3").ToString(),
                Name = "Bamboo Glass Jar",
                Description = "This bamboo glass jar can hold 57 oz (1.7 l) and is perfect for any kitchen.",
                Picture = "/static/img/products/bamboo-glass-jar.jpg",
                PriceUsd = new Money {
                    CurrencyCode = "USD",
                    Units = 5,
                    Nanos = 490000000
                },
                Categories = { "kitchen" }
            },
            new Product {
                Id = Guid.Parse("47458EA7-EDE9-4CB3-BB90-7A4742196937").ToString(),
                Name = "Mug",
                Description = "A simple mug with a mustard interior.",
                Picture = "/static/img/products/mug.jpg",
                PriceUsd = new Money {
                    CurrencyCode = "USD",
                    Units = 8,
                    Nanos = 990000000
                },
                Categories = { "kitchen" }
            }
        ];

        public ProductCatalogActor()
        {
            Receive<GetProductRequest>(GetProduct);
            Receive<ListProductsRequest>(ListProducts);
            Receive<SearchProductsRequest>(SearchProducts);
        }

        public void GetProduct(GetProductRequest request)
        {
            Sender.Tell(products.Find(p => p.Id == request.Id) ?? throw new RpcException(new Grpc.Core.Status(StatusCode.FailedPrecondition, $"No product with id={request.Id}")));
        }

        public void ListProducts(ListProductsRequest _)
        {
            var response = new ListProductsResponse();
            response.Products.AddRange(products);

            Sender.Tell(response);
        }

        public void SearchProducts(SearchProductsRequest request)
        {
            var resultProducts = products.FindAll(p => (
                request.Query.Contains(p.Id.ToString(), StringComparison.OrdinalIgnoreCase) ||
                request.Query.Contains(p.Name, StringComparison.OrdinalIgnoreCase) ||
                request.Query.Contains(p.Description, StringComparison.OrdinalIgnoreCase)
            ));

            var response = new SearchProductsResponse();
            response.Results.AddRange(resultProducts);

            Sender.Tell(response);
        }
    }

    public class ListProductsRequest 
    { }
}
