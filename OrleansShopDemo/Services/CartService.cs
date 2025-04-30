// Copyright 2020 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading.Tasks;
using Grpc.Core;
using Orleans;
using OrleansShopDemo.Grains.Cart.Constants;
using OrleansShopDemo.Grains.Cart.Interfaces;
using Shop;
using static Shop.CartService;

namespace OrleansShopDemo.Services 
{
    public class CartService(IClusterClient clusterClient) : CartServiceBase
    {
        private readonly static Empty Empty = new();

        public override async Task<Empty> AddItem(AddItemRequest request, ServerCallContext context)
        {
            await clusterClient.GetGrain<ICartGrain>(CartGrainId.Id).AddItem(request);
            return Empty;
        }

        public override async Task<Cart> GetCart(GetCartRequest request, ServerCallContext context)
        {
            return await clusterClient.GetGrain<ICartGrain>(CartGrainId.Id).GetCart(request);
        }

        public override async Task<Empty> EmptyCart(EmptyCartRequest request, ServerCallContext context)
        {
            await clusterClient.GetGrain<ICartGrain>(CartGrainId.Id).EmptyCart(request);
            return Empty;
        }
    }
}
