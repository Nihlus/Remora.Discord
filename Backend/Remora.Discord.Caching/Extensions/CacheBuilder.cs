//
//  CacheBuilder.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) 2017 Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Remora.Discord.Caching.Clients;
using Remora.Discord.Caching.Services;

namespace Remora.Discord.Caching.Extensions
{
    /// <summary>
    /// Utility class used by <see cref="ServiceCollectionExtensions.AddDiscordCaching"/> to configure the cache.
    /// </summary>
    public class CacheBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheBuilder"/> class.
        /// </summary>
        /// <param name="services">Services of the application.</param>
        public CacheBuilder(IServiceCollection services)
        {
            Services = services;
        }

        /// <summary>
        /// Gets the application services.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Adds a singleton service of the type specified in <typeparamref name="TClient"/> with an
        /// implementation type specified in <see cref="ICacheClient"/> to the
        /// service collection.
        /// </summary>
        /// <typeparam name="TClient">The type of the implementation to use.</typeparam>
        /// <returns>Same instance of the builder.</returns>
        public CacheBuilder UseClient<TClient>() where TClient : class, ICacheClient
        {
            Services.AddSingleton<ICacheClient, TClient>();
            return this;
        }

        /// <summary>
        /// Adds the service <see cref="DistributedCacheClient"/> to the service collection. This causes the
        /// <see cref="ICacheService"/> to use <see cref="IDistributedCache"/> instead of the default
        /// <see cref="IMemoryCache"/>.
        /// </summary>
        /// <remarks>
        /// A distributed cache client should be registered in the service collection.
        /// </remarks>
        /// <returns>Same instance of the builder.</returns>
        public CacheBuilder UseDistributedCache()
        {
            return UseClient<DistributedCacheClient>();
        }
    }
}
