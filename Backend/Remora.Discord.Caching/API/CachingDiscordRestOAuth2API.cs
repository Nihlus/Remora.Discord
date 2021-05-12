//
//  CachingDiscordRestOAuth2API.cs
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

using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Caching.Services;
using Remora.Discord.Rest;
using Remora.Discord.Rest.API;
using Remora.Results;

namespace Remora.Discord.Caching.API
{
    /// <inheritdoc />
    public class CachingDiscordRestOAuth2API : DiscordRestOAuth2API
    {
        private readonly ICacheService _cacheService;

        /// <inheritdoc cref="DiscordRestOAuth2API(DiscordHttpClient)" />
        public CachingDiscordRestOAuth2API
        (
            DiscordHttpClient discordHttpClient,
            ICacheService cacheService
        )
            : base(discordHttpClient)
        {
            _cacheService = cacheService;
        }

        /// <inheritdoc />
        public override async Task<Result<IApplication>> GetCurrentBotApplicationInformationAsync
        (
            CancellationToken ct = default
        )
        {
            var key = KeyHelpers.CreateCurrentApplicationCacheKey();
            var cache = await _cacheService.GetValueAsync<IApplication>(key);
            if (cache.IsSuccess)
            {
               return Result<IApplication>.FromSuccess(cache.Entity);
            }

            var getCurrent = await base.GetCurrentBotApplicationInformationAsync(ct);
            if (!getCurrent.IsSuccess)
            {
                return getCurrent;
            }

            var application = getCurrent.Entity;
            await _cacheService.CacheAsync(key, application);

            return getCurrent;
        }
    }
}
