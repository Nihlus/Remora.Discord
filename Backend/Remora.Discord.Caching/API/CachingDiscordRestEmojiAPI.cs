//
//  CachingDiscordRestEmojiAPI.cs
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

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Caching.Services;
using Remora.Discord.Core;
using Remora.Discord.Rest;
using Remora.Discord.Rest.API;
using Remora.Results;

namespace Remora.Discord.Caching.API
{
    /// <summary>
    /// Implements a caching version of the channel API.
    /// </summary>
    public class CachingDiscordRestEmojiAPI : DiscordRestEmojiAPI
    {
        private readonly ICacheService _cacheService;

        /// <inheritdoc cref="DiscordRestEmojiAPI" />
        public CachingDiscordRestEmojiAPI
        (
            DiscordHttpClient discordHttpClient,
            IOptions<JsonSerializerOptions> jsonOptions,
            ICacheService cacheService
        )
            : base(discordHttpClient, jsonOptions)
        {
            _cacheService = cacheService;
        }

        /// <inheritdoc />
        public override async Task<Result<IEmoji>> GetGuildEmojiAsync
        (
            Snowflake guildID,
            Snowflake emojiID,
            CancellationToken ct = default
        )
        {
            var key = KeyHelpers.CreateEmojiCacheKey(guildID, emojiID);
            var cache = await _cacheService.GetValueAsync<IEmoji>(key);
            if (cache.IsSuccess)
            {
               return Result<IEmoji>.FromSuccess(cache.Entity);
            }

            var getResult = await base.GetGuildEmojiAsync(guildID, emojiID, ct);
            if (!getResult.IsSuccess)
            {
                return getResult;
            }

            var emoji = getResult.Entity;
            await _cacheService.CacheAsync(key, emoji);

            return getResult;
        }

        /// <inheritdoc />
        public override async Task<Result<IEmoji>> CreateGuildEmojiAsync
        (
            Snowflake guildID,
            string name,
            Stream image,
            IReadOnlyList<Snowflake> roles,
            CancellationToken ct = default
        )
        {
            var createResult = await base.CreateGuildEmojiAsync(guildID, name, image, roles, ct);
            if (!createResult.IsSuccess)
            {
                return createResult;
            }

            var emoji = createResult.Entity;
            if (emoji.ID is null)
            {
                // We can't, or shouldn't, cache this
                return createResult;
            }

            var key = KeyHelpers.CreateEmojiCacheKey(guildID, emoji.ID.Value);
            await _cacheService.CacheAsync(key, emoji);

            return createResult;
        }

        /// <inheritdoc />
        public override async Task<Result<IEmoji>> ModifyGuildEmojiAsync
        (
            Snowflake guildID,
            Snowflake emojiID,
            Optional<string> name = default,
            Optional<IReadOnlyList<Snowflake>?> roles = default,
            CancellationToken ct = default
        )
        {
            var modifyResult = await base.ModifyGuildEmojiAsync(guildID, emojiID, name, roles, ct);
            if (!modifyResult.IsSuccess)
            {
                return modifyResult;
            }

            var emoji = modifyResult.Entity;
            var key = KeyHelpers.CreateEmojiCacheKey(guildID, emojiID);
            await _cacheService.CacheAsync(key, emoji);

            return modifyResult;
        }

        /// <inheritdoc />
        public override async Task<Result> DeleteGuildEmojiAsync
        (
            Snowflake guildID,
            Snowflake emojiID,
            CancellationToken ct = default
        )
        {
            var deleteResult = await base.DeleteGuildEmojiAsync(guildID, emojiID, ct);
            if (!deleteResult.IsSuccess)
            {
                return deleteResult;
            }

            var key = KeyHelpers.CreateEmojiCacheKey(guildID, emojiID);
            await _cacheService.EvictAsync(key);

            return deleteResult;
        }
    }
}
