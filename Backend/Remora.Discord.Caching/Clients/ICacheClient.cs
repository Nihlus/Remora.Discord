//
//  ICacheClient.cs
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

using System.Threading.Tasks;
using Remora.Discord.Caching.Services;
using Remora.Results;

namespace Remora.Discord.Caching.Clients
{
    /// <summary>
    /// Cache manager for <see cref="ICacheService"/>.
    /// </summary>
    public interface ICacheClient
    {
        /// <summary>
        /// Stores the value in the cache.
        /// </summary>
        /// <param name="key">Key of the item.</param>
        /// <param name="value">Value to cache.</param>
        /// <typeparam name="TInstance">Type of the item.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task StoreAsync<TInstance>(CacheKey key, TInstance value);

        /// <summary>
        /// Attempts to retrieve a value from the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <typeparam name="TInstance">The instance type.</typeparam>
        /// <returns>value from the cache provider.</returns>
        public Task<Result<TInstance>> RetrieveAsync<TInstance>(CacheKey key) where TInstance : notnull;

        /// <summary>
        /// Removes value of the given key from the cache.
        /// </summary>
        /// <param name="key">Key of the value.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task EvictAsync(CacheKey key);
    }
}
