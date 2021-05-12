//
//  CachingDiscordRestWebhookAPI.cs
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
    /// <inheritdoc />
    public class CachingDiscordRestWebhookAPI : DiscordRestWebhookAPI
    {
        private readonly ICacheService _cacheService;

        /// <inheritdoc cref="DiscordRestWebhookAPI(DiscordHttpClient, IOptions{JsonSerializerOptions})" />
        public CachingDiscordRestWebhookAPI
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
        public override async Task<Result<IWebhook>> CreateWebhookAsync
        (
            Snowflake channelID,
            string name,
            Stream? avatar,
            CancellationToken ct = default
        )
        {
            var createWebhook = await base.CreateWebhookAsync(channelID, name, avatar, ct);
            if (!createWebhook.IsSuccess)
            {
                return createWebhook;
            }

            var webhook = createWebhook.Entity;
            var key = KeyHelpers.CreateWebhookCacheKey(webhook.ID);

            await _cacheService.CacheAsync(key, webhook);

            return createWebhook;
        }

        /// <inheritdoc />
        public override async Task<Result> DeleteWebhookAsync
        (
            Snowflake webhookID,
            CancellationToken ct = default
        )
        {
            var deleteWebhook = await base.DeleteWebhookAsync(webhookID, ct);
            if (!deleteWebhook.IsSuccess)
            {
                return deleteWebhook;
            }

            var key = KeyHelpers.CreateWebhookCacheKey(webhookID);
            await _cacheService.EvictAsync(key);

            return deleteWebhook;
        }

        /// <inheritdoc />
        public override async Task<Result<IMessage>> ExecuteWebhookAsync
        (
            Snowflake webhookID,
            string token,
            Optional<bool> shouldWait = default,
            Optional<string> content = default,
            Optional<string> username = default,
            Optional<string> avatarUrl = default,
            Optional<bool> isTTS = default,
            Optional<FileData> file = default,
            Optional<IReadOnlyList<IEmbed>> embeds = default,
            Optional<IAllowedMentions> allowedMentions = default,
            Optional<Snowflake> threadID = default,
            CancellationToken ct = default
        )
        {
            var execute = await base.ExecuteWebhookAsync
            (
                webhookID,
                token,
                shouldWait,
                content,
                username,
                avatarUrl,
                isTTS,
                file,
                embeds,
                allowedMentions,
                threadID,
                ct
            );

            if (!execute.IsSuccess)
            {
                return execute;
            }

            var message = execute.Entity;
            var key = KeyHelpers.CreateMessageCacheKey(message.ChannelID, message.ID);
            await _cacheService.CacheAsync(key, message);

            return execute;
        }

        /// <inheritdoc />
        public override async Task<Result<IWebhook>> GetWebhookAsync
        (
            Snowflake webhookID,
            CancellationToken ct = default
        )
        {
            var key = KeyHelpers.CreateWebhookCacheKey(webhookID);
            var cache = await _cacheService.GetValueAsync<IWebhook>(key);
            if (cache.IsSuccess)
            {
               return Result<IWebhook>.FromSuccess(cache.Entity);
            }

            var getWebhook = await base.GetWebhookAsync(webhookID, ct);
            if (!getWebhook.IsSuccess)
            {
                return getWebhook;
            }

            var webhook = getWebhook.Entity;
            await _cacheService.CacheAsync(key, webhook);

            return getWebhook;
        }

        /// <inheritdoc />
        public override async Task<Result<IWebhook>> ModifyWebhookAsync
        (
            Snowflake webhookID,
            Optional<string> name = default,
            Optional<Stream?> avatar = default,
            Optional<Snowflake> channelID = default,
            CancellationToken ct = default
        )
        {
            var modifyWebhook = await base.ModifyWebhookAsync(webhookID, name, avatar, channelID, ct);
            if (!modifyWebhook.IsSuccess)
            {
                return modifyWebhook;
            }

            var key = KeyHelpers.CreateWebhookCacheKey(webhookID);
            var webhook = modifyWebhook.Entity;

            await _cacheService.CacheAsync(key, webhook);

            return modifyWebhook;
        }

        /// <inheritdoc />
        public override async Task<Result<IReadOnlyList<IWebhook>>> GetChannelWebhooksAsync
        (
            Snowflake channelID,
            CancellationToken ct = default
        )
        {
            var key = KeyHelpers.CreateChannelWebhooksCacheKey(channelID);
            var cache = await _cacheService.GetValueAsync<IReadOnlyList<IWebhook>>(key);
            if (cache.IsSuccess)
            {
               return Result<IReadOnlyList<IWebhook>>.FromSuccess(cache.Entity);
            }

            var getWebhooks = await base.GetChannelWebhooksAsync(channelID, ct);
            if (!getWebhooks.IsSuccess)
            {
                return getWebhooks;
            }

            var webhooks = getWebhooks.Entity;
            await _cacheService.CacheAsync(key, webhooks);

            foreach (var webhook in webhooks)
            {
                var webhookKey = KeyHelpers.CreateWebhookCacheKey(webhook.ID);
                await _cacheService.CacheAsync(webhookKey, webhook);
            }

            return getWebhooks;
        }

        /// <inheritdoc />
        public override async Task<Result<IReadOnlyList<IWebhook>>> GetGuildWebhooksAsync
        (
            Snowflake guildID, CancellationToken ct = default
        )
        {
            var key = KeyHelpers.CreateGuildWebhooksCacheKey(guildID);
            var cacheResult = await _cacheService.GetValueAsync<IReadOnlyList<IWebhook>>(key);
            if (cacheResult.IsSuccess)
            {
                return Result<IReadOnlyList<IWebhook>>.FromSuccess(cacheResult.Entity);
            }

            var getWebhooks = await base.GetGuildWebhooksAsync(guildID, ct);
            if (!getWebhooks.IsSuccess)
            {
                return getWebhooks;
            }

            foreach (var webhook in getWebhooks.Entity)
            {
                var webhookKey = KeyHelpers.CreateWebhookCacheKey(webhook.ID);
                await _cacheService.CacheAsync(webhookKey, webhook);
            }

            return getWebhooks;
        }

        /// <inheritdoc />
        public override async Task<Result> DeleteWebhookWithTokenAsync
        (
            Snowflake webhookID,
            string token,
            CancellationToken ct = default
        )
        {
            var deleteWebhook = await base.DeleteWebhookWithTokenAsync(webhookID, token, ct);
            if (!deleteWebhook.IsSuccess)
            {
                return deleteWebhook;
            }

            var key = KeyHelpers.CreateWebhookCacheKey(webhookID);
            await _cacheService.EvictAsync(key);

            return deleteWebhook;
        }

        /// <inheritdoc />
        public override async Task<Result<IWebhook>> GetWebhookWithTokenAsync
        (
            Snowflake webhookID,
            string token,
            CancellationToken ct = default
        )
        {
            var key = KeyHelpers.CreateWebhookCacheKey(webhookID);
            var cache = await _cacheService.GetValueAsync<IWebhook>(key);
            if (cache.IsSuccess)
            {
               return Result<IWebhook>.FromSuccess(cache.Entity);
            }

            var getWebhook = await base.GetWebhookWithTokenAsync(webhookID, token, ct);
            if (!getWebhook.IsSuccess)
            {
                return getWebhook;
            }

            var webhook = getWebhook.Entity;
            await _cacheService.CacheAsync(key, webhook);

            return getWebhook;
        }

        /// <inheritdoc />
        public override async Task<Result<IWebhook>> ModifyWebhookWithTokenAsync
        (
            Snowflake webhookID,
            string token,
            Optional<string> name = default,
            Optional<Stream?> avatar = default,
            CancellationToken ct = default
        )
        {
            var modifyWebhook = await base.ModifyWebhookWithTokenAsync(webhookID, token, name, avatar, ct);
            if (!modifyWebhook.IsSuccess)
            {
                return modifyWebhook;
            }

            var key = KeyHelpers.CreateWebhookCacheKey(webhookID);
            var webhook = modifyWebhook.Entity;

            await _cacheService.CacheAsync(key, webhook);

            return modifyWebhook;
        }
    }
}
