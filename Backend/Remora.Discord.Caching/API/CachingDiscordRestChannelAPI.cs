//
//  CachingDiscordRestChannelAPI.cs
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
    public class CachingDiscordRestChannelAPI : DiscordRestChannelAPI
    {
        private readonly ICacheService _cacheService;

        /// <inheritdoc cref="DiscordRestChannelAPI" />
        public CachingDiscordRestChannelAPI
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
        public override async Task<Result<IChannel>> GetChannelAsync
        (
            Snowflake channelID,
            CancellationToken ct = default
        )
        {
            var key = KeyHelpers.CreateChannelCacheKey(channelID);
            var cache = await _cacheService.GetValueAsync<IChannel>(key);
            if (cache.IsSuccess)
            {
               return Result<IChannel>.FromSuccess(cache.Entity);
            }

            var getChannel = await base.GetChannelAsync(channelID, ct);
            if (!getChannel.IsSuccess)
            {
                return getChannel;
            }

            var channel = getChannel.Entity;
            await _cacheService.CacheAsync(key, channel);

            return getChannel;
        }

        /// <inheritdoc />
        public override async Task<Result<IChannel>> ModifyChannelAsync
        (
            Snowflake channelID,
            Optional<string> name = default,
            Optional<Stream> icon = default,
            Optional<ChannelType> type = default,
            Optional<int?> position = default,
            Optional<string?> topic = default,
            Optional<bool?> isNsfw = default,
            Optional<int?> rateLimitPerUser = default,
            Optional<int?> bitrate = default,
            Optional<int?> userLimit = default,
            Optional<IReadOnlyList<IPermissionOverwrite>?> permissionOverwrites = default,
            Optional<Snowflake?> parentId = default,
            Optional<VideoQualityMode?> videoQualityMode = default,
            Optional<bool> isArchived = default,
            Optional<TimeSpan> autoArchiveDuration = default,
            Optional<bool> isLocked = default,
            CancellationToken ct = default
        )
        {
            var modificationResult = await base.ModifyChannelAsync
            (
                channelID,
                name,
                icon,
                type,
                position,
                topic,
                isNsfw,
                rateLimitPerUser,
                bitrate,
                userLimit,
                permissionOverwrites,
                parentId,
                videoQualityMode,
                isArchived,
                autoArchiveDuration,
                isLocked,
                ct
            );

            if (!modificationResult.IsSuccess)
            {
                return modificationResult;
            }

            var channel = modificationResult.Entity;
            var key = KeyHelpers.CreateChannelCacheKey(channelID);
            await _cacheService.CacheAsync(key, channel);

            return modificationResult;
        }

        /// <inheritdoc />
        public override async Task<Result> DeleteChannelAsync
        (
            Snowflake channelID,
            CancellationToken ct = default
        )
        {
            var deleteResult = await base.DeleteChannelAsync(channelID, ct);
            if (!deleteResult.IsSuccess)
            {
                return deleteResult;
            }

            var key = KeyHelpers.CreateChannelCacheKey(channelID);
            await _cacheService.EvictAsync(key);

            return deleteResult;
        }

        /// <inheritdoc />
        public override async Task<Result<IMessage>> GetChannelMessageAsync
        (
            Snowflake channelID,
            Snowflake messageID,
            CancellationToken ct = default
        )
        {
            var key = KeyHelpers.CreateMessageCacheKey(channelID, messageID);
            var cache = await _cacheService.GetValueAsync<IMessage>(key);
            if (cache.IsSuccess)
            {
               return Result<IMessage>.FromSuccess(cache.Entity);
            }

            var getMessage = await base.GetChannelMessageAsync(channelID, messageID, ct);
            if (!getMessage.IsSuccess)
            {
                return getMessage;
            }

            var message = getMessage.Entity;
            await _cacheService.CacheAsync(key, message);

            return getMessage;
        }

        /// <inheritdoc />
        public override async Task<Result<IMessage>> CreateMessageAsync
        (
            Snowflake channelID,
            Optional<string> content = default,
            Optional<string> nonce = default,
            Optional<bool> isTTS = default,
            Optional<FileData> file = default,
            Optional<IEmbed> embed = default,
            Optional<IAllowedMentions> allowedMentions = default,
            Optional<IMessageReference> messageReference = default,
            CancellationToken ct = default
        )
        {
            var createResult = await base.CreateMessageAsync
            (
                channelID,
                content,
                nonce,
                isTTS,
                file,
                embed,
                allowedMentions,
                messageReference,
                ct
            );

            if (!createResult.IsSuccess)
            {
                return createResult;
            }

            var message = createResult.Entity;
            var key = KeyHelpers.CreateMessageCacheKey(channelID, message.ID);
            await _cacheService.CacheAsync(key, message);

            return createResult;
        }

        /// <inheritdoc />
        public override async Task<Result<IMessage>> EditMessageAsync
        (
            Snowflake channelID,
            Snowflake messageID,
            Optional<string?> content = default,
            Optional<IEmbed?> embed = default,
            Optional<MessageFlags?> flags = default,
            Optional<IAllowedMentions?> allowedMentions = default,
            Optional<IReadOnlyList<IAttachment>> attachments = default,
            CancellationToken ct = default
        )
        {
            var editResult = await base.EditMessageAsync
            (
                channelID,
                messageID,
                content,
                embed,
                flags,
                allowedMentions,
                attachments,
                ct
            );

            if (!editResult.IsSuccess)
            {
                return editResult;
            }

            var message = editResult.Entity;
            var key = KeyHelpers.CreateMessageCacheKey(channelID, messageID);
            await _cacheService.CacheAsync(key, message);

            return editResult;
        }

        /// <inheritdoc />
        public override async Task<Result> DeleteMessageAsync
        (
            Snowflake channelID,
            Snowflake messageID,
            CancellationToken ct = default
        )
        {
            var deleteResult = await base.DeleteMessageAsync(channelID, messageID, ct);
            if (!deleteResult.IsSuccess)
            {
                return deleteResult;
            }

            var key = KeyHelpers.CreateMessageCacheKey(channelID, messageID);
            await _cacheService.EvictAsync(key);

            return deleteResult;
        }

        /// <inheritdoc />
        public override async Task<Result> BulkDeleteMessagesAsync
        (
            Snowflake channelID,
            IReadOnlyList<Snowflake> messageIDs,
            CancellationToken ct = default
        )
        {
            var deleteResult = await base.BulkDeleteMessagesAsync(channelID, messageIDs, ct);
            if (!deleteResult.IsSuccess)
            {
                return deleteResult;
            }

            foreach (var messageID in messageIDs)
            {
                var key = KeyHelpers.CreateMessageCacheKey(channelID, messageID);
                await _cacheService.EvictAsync(key);
            }

            return deleteResult;
        }

        /// <inheritdoc />
        public override async Task<Result<IInvite>> CreateChannelInviteAsync
        (
            Snowflake channelID,
            Optional<TimeSpan> maxAge = default,
            Optional<int> maxUses = default,
            Optional<bool> isTemporary = default,
            Optional<bool> isUnique = default,
            Optional<InviteTarget> targetType = default,
            Optional<Snowflake> targetUserID = default,
            Optional<Snowflake> targetApplicationID = default,
            CancellationToken ct = default
        )
        {
            var createResult = await base.CreateChannelInviteAsync
            (
                channelID,
                maxAge,
                maxUses,
                isTemporary,
                isUnique,
                targetType,
                targetUserID,
                targetApplicationID,
                ct
            );

            if (!createResult.IsSuccess)
            {
                return createResult;
            }

            var invite = createResult.Entity;
            var key = KeyHelpers.CreateInviteCacheKey(invite.Code);
            await _cacheService.CacheAsync(key, invite);

            return createResult;
        }

        /// <inheritdoc />
        public override async Task<Result> DeleteChannelPermissionAsync
        (
            Snowflake channelID,
            Snowflake overwriteID,
            CancellationToken ct = default
        )
        {
            var deleteResult = await base.DeleteChannelPermissionAsync(channelID, overwriteID, ct);
            if (!deleteResult.IsSuccess)
            {
                return deleteResult;
            }

            var key = KeyHelpers.CreateChannelPermissionCacheKey(channelID, overwriteID);
            await _cacheService.EvictAsync(key);

            return deleteResult;
        }

        /// <inheritdoc />
        public override async Task<Result<IReadOnlyList<IMessage>>> GetPinnedMessagesAsync
        (
            Snowflake channelID,
            CancellationToken ct = default
        )
        {
            var key = KeyHelpers.CreatePinnedMessagesCacheKey(channelID);
            var cache = await _cacheService.GetValueAsync<IReadOnlyList<IMessage>>(key);
            if (cache.IsSuccess)
            {
               return Result<IReadOnlyList<IMessage>>.FromSuccess(cache.Entity);
            }

            var getResult = await base.GetPinnedMessagesAsync(channelID, ct);
            if (!getResult.IsSuccess)
            {
                return getResult;
            }

            var messages = getResult.Entity;
            await _cacheService.CacheAsync(key, messages);

            foreach (var message in messages)
            {
                var messageKey = KeyHelpers.CreateMessageCacheKey(channelID, message.ID);
                await _cacheService.CacheAsync(messageKey, message);
            }

            return getResult;
        }

        /// <inheritdoc />
        public override async Task<Result> DeletePinnedChannelMessageAsync
        (
            Snowflake channelID,
            Snowflake messageID,
            CancellationToken ct = default
        )
        {
            var deleteResult = await base.DeletePinnedChannelMessageAsync(channelID, messageID, ct);
            if (!deleteResult.IsSuccess)
            {
                return deleteResult;
            }

            var key = KeyHelpers.CreateMessageCacheKey(channelID, messageID);
            await _cacheService.EvictAsync(key);

            return deleteResult;
        }

        /// <inheritdoc />
        public override async Task<Result<IChannel>> StartPublicThreadAsync
        (
            Snowflake channelID,
            Snowflake messageID,
            string name,
            TimeSpan autoArchiveDuration,
            CancellationToken ct = default
        )
        {
            var createResult = await base.StartPublicThreadAsync(channelID, messageID, name, autoArchiveDuration, ct);
            if (!createResult.IsSuccess)
            {
                return createResult;
            }

            var key = KeyHelpers.CreateChannelCacheKey(channelID);
            _cacheService.Cache(key, createResult.Entity);

            return createResult;
        }

        /// <inheritdoc />
        public override async Task<Result<IChannel>> StartPrivateThreadAsync
        (
            Snowflake channelID,
            string name,
            TimeSpan autoArchiveDuration,
            CancellationToken ct = default
        )
        {
            var createResult = await base.StartPrivateThreadAsync(channelID, name, autoArchiveDuration, ct);
            if (!createResult.IsSuccess)
            {
                return createResult;
            }

            var key = KeyHelpers.CreateChannelCacheKey(channelID);
            _cacheService.Cache(key, createResult.Entity);

            return createResult;
        }

        /// <inheritdoc />
        public override async Task<Result<IThreadQueryResponse>> ListActiveThreadsAsync
        (
            Snowflake channelID,
            CancellationToken ct = default
        )
        {
            var getResult = await base.ListActiveThreadsAsync(channelID, ct);
            if (!getResult.IsSuccess)
            {
                return getResult;
            }

            foreach (var channel in getResult.Entity.Threads)
            {
                var key = KeyHelpers.CreateChannelCacheKey(channel.ID);
                _cacheService.Cache(key, channel);
            }

            return getResult;
        }

        /// <inheritdoc />
        public override async Task<Result<IThreadQueryResponse>> ListPublicArchivedThreadsAsync
        (
            Snowflake channelID,
            Optional<DateTimeOffset> before = default,
            Optional<int> limit = default,
            CancellationToken ct = default
        )
        {
            var getResult = await base.ListPublicArchivedThreadsAsync(channelID, before, limit, ct);
            if (!getResult.IsSuccess)
            {
                return getResult;
            }

            foreach (var channel in getResult.Entity.Threads)
            {
                var key = KeyHelpers.CreateChannelCacheKey(channel.ID);
                _cacheService.Cache(key, channel);
            }

            return getResult;
        }

        /// <inheritdoc />
        public override async Task<Result<IThreadQueryResponse>> ListPrivateArchivedThreadsAsync
        (
            Snowflake channelID,
            Optional<DateTimeOffset> before = default,
            Optional<int> limit = default,
            CancellationToken ct = default
        )
        {
            var getResult = await base.ListPrivateArchivedThreadsAsync(channelID, before, limit, ct);
            if (!getResult.IsSuccess)
            {
                return getResult;
            }

            foreach (var channel in getResult.Entity.Threads)
            {
                var key = KeyHelpers.CreateChannelCacheKey(channel.ID);
                _cacheService.Cache(key, channel);
            }

            return getResult;
        }

        /// <inheritdoc />
        public override async Task<Result<IThreadQueryResponse>> ListJoinedPrivateArchivedThreadsAsync
        (
            Snowflake channelID,
            Optional<DateTimeOffset> before = default,
            Optional<int> limit = default,
            CancellationToken ct = default
        )
        {
            var getResult = await base.ListJoinedPrivateArchivedThreadsAsync(channelID, before, limit, ct);
            if (!getResult.IsSuccess)
            {
                return getResult;
            }

            foreach (var channel in getResult.Entity.Threads)
            {
                var key = KeyHelpers.CreateChannelCacheKey(channel.ID);
                _cacheService.Cache(key, channel);
            }

            return getResult;
        }
    }
}
