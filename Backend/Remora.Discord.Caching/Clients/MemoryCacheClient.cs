//
//  MemoryCacheClient.cs
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

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Remora.Discord.Caching.Results;
using Remora.Discord.Caching.Services;
using Remora.Results;

namespace Remora.Discord.Caching.Clients
{
    /// <summary>
    /// Stores the cache into memory.
    /// </summary>
    public class MemoryCacheClient : ICacheClient
    {
        private readonly IMemoryCache _memoryCache;
        private readonly CacheSettings _cacheSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheClient"/> class.
        /// </summary>
        /// <param name="memoryCache">The memory cache.</param>
        /// <param name="cacheSettings">The cache settings.</param>
        public MemoryCacheClient(IMemoryCache memoryCache, IOptions<CacheSettings> cacheSettings)
        {
            _memoryCache = memoryCache;
            _cacheSettings = cacheSettings.Value;
        }

        /// <inheritdoc />
        public Task StoreAsync<TInstance>(CacheKey key, TInstance value)
        {
            var expirationSetting = _cacheSettings.GetAbsoluteExpirationOrDefault<TInstance>();

            if (expirationSetting <= TimeSpan.Zero)
            {
                return Task.CompletedTask;
            }

            var cacheOptions = new MemoryCacheEntryOptions();

            if (expirationSetting != TimeSpan.MaxValue)
            {
                cacheOptions.SetAbsoluteExpiration(expirationSetting);
                cacheOptions.SetSlidingExpiration(_cacheSettings.GetSlidingExpirationOrDefault<TInstance>());
            }

            _memoryCache.Set(key, value, cacheOptions);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<Result<TInstance>> RetrieveAsync<TInstance>(CacheKey key) where TInstance : notnull
        {
            var result = _memoryCache.TryGetValue(key, out TInstance instance)
                ? Result<TInstance>.FromSuccess(instance)
                : Result<TInstance>.FromError(new CacheNotFoundError(key));

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task EvictAsync(CacheKey key)
        {
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }
    }
}
