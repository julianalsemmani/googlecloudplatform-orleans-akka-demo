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

using System.Threading.Tasks;
using Grpc.Core;
using Shop;
using static Shop.CartService;
using Akka.Hosting;
using AkkaShopDemo.Actors.Cart;
using Akka.Actor;

namespace AkkaShopDemo.Services 
{
    public class CartService(IRequiredActor<CartActor> cartActor) : CartServiceBase
    {
        private readonly static Empty Empty = new();

        public override async Task<Empty> AddItem(AddItemRequest request, ServerCallContext context)
        {
            cartActor.ActorRef.Tell(request);
            return await Task.FromResult(Empty);
        }

        public override async Task<Cart> GetCart(GetCartRequest request, ServerCallContext context)
        {
            return await cartActor.ActorRef.Ask<Cart>(request);
        }

        public override async Task<Empty> EmptyCart(EmptyCartRequest request, ServerCallContext context)
        {
            cartActor.ActorRef.Tell(request);
            return await Task.FromResult(Empty);
        }
    }
}
