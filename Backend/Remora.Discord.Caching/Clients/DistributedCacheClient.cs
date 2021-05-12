//
//  DistributedCacheClient.cs
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
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Remora.Discord.Caching.Results;
using Remora.Discord.Caching.Services;
using Remora.Results;

namespace Remora.Discord.Caching.Clients
{
    /// <summary>
    /// Stores the cache into <see cref="IDistributedCache"/>.
    /// </summary>
    public class DistributedCacheClient : ICacheClient
    {
        private readonly IDistributedCache _cache;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly CacheSettings _cacheSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedCacheClient"/> class.
        /// </summary>
        /// <param name="jsonOptions">The JSON options.</param>
        /// <param name="cache">The distributed cache.</param>
        /// <param name="cacheSettings">The cache settings.</param>
        public DistributedCacheClient(IOptions<JsonSerializerOptions> jsonOptions, IDistributedCache cache, IOptions<CacheSettings> cacheSettings)
        {
            _cache = cache;
            _jsonOptions = jsonOptions.Value;
            _cacheSettings = cacheSettings.Value;
        }

        /// <inheritdoc />
        public async Task StoreAsync<TInstance>(CacheKey key, TInstance value)
        {
            var expirationSetting = _cacheSettings.GetAbsoluteExpirationOrDefault<TInstance>();

            if (expirationSetting <= TimeSpan.Zero)
            {
                return;
            }

            var cacheOptions = new DistributedCacheEntryOptions();

            if (expirationSetting != TimeSpan.MaxValue)
            {
                cacheOptions.SetAbsoluteExpiration(expirationSetting);
                cacheOptions.SetSlidingExpiration(_cacheSettings.GetSlidingExpirationOrDefault<TInstance>());
            }

            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, _jsonOptions);

            await _cache.SetAsync(key.Key, bytes, cacheOptions);
        }

        /// <inheritdoc />
        public async Task<Result<TInstance>> RetrieveAsync<TInstance>(CacheKey key) where TInstance : notnull
        {
            var result = await _cache.GetAsync(key.Key);

            return result != null
                ? JsonSerializer.Deserialize<TInstance>(result, _jsonOptions)!
                : Result<TInstance>.FromError(new CacheNotFoundError(key));
        }

        /// <inheritdoc />
        public async Task EvictAsync(CacheKey key)
        {
            await _cache.RemoveAsync(key.Key);
        }
    }
}
