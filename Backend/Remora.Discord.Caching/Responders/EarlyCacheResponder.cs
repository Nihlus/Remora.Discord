//
//  EarlyCacheResponder.cs
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
using JetBrains.Annotations;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Caching.Services;
using Remora.Discord.Core;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Remora.Discord.Caching.Responders
{
    /// <summary>
    /// Caches incoming data from the gateway.
    /// </summary>
    [UsedImplicitly]
    public class EarlyCacheResponder :
        IResponder<IChannelCreate>,
        IResponder<IChannelUpdate>,
        IResponder<IGuildBanAdd>,
        IResponder<IGuildCreate>,
        IResponder<IGuildEmojisUpdate>,
        IResponder<IGuildMemberAdd>,
        IResponder<IGuildMembersChunk>,
        IResponder<IGuildMemberUpdate>,
        IResponder<IGuildRoleCreate>,
        IResponder<IGuildRoleUpdate>,
        IResponder<IMessageCreate>,
        IResponder<IMessageReactionAdd>,
        IResponder<IUserUpdate>,
        IResponder<IInteractionCreate>
    {
        private readonly ICacheService _cacheService;

        /// <summary>
        /// Initializes a new instance of the <see cref="EarlyCacheResponder"/> class.
        /// </summary>
        /// <param name="cacheService">The cache service.</param>
        public EarlyCacheResponder(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        /// <inheritdoc/>
        public async Task<Result> RespondAsync(IChannelCreate gatewayEvent, CancellationToken ct = default)
        {
            var key = KeyHelpers.CreateChannelCacheKey(gatewayEvent.ID);
            await _cacheService.CacheAsync(key, gatewayEvent);

            return Result.FromSuccess();
        }

        /// <inheritdoc/>
        public async Task<Result> RespondAsync(IChannelUpdate gatewayEvent, CancellationToken ct = default)
        {
            var key = KeyHelpers.CreateChannelCacheKey(gatewayEvent.ID);
            await _cacheService.CacheAsync(key, gatewayEvent);

            return Result.FromSuccess();
        }

        /// <inheritdoc/>
        public async Task<Result> RespondAsync(IGuildBanAdd gatewayEvent, CancellationToken ct = default)
        {
            var key = KeyHelpers.CreateGuildBanCacheKey(gatewayEvent.GuildID, gatewayEvent.User.ID);
            await _cacheService.CacheAsync(key, new Ban(null, gatewayEvent.User));

            return Result.FromSuccess();
        }

        /// <inheritdoc/>
        public async Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken ct = default)
        {
            var key = KeyHelpers.CreateGuildCacheKey(gatewayEvent.ID);
            await _cacheService.CacheAsync(key, gatewayEvent);

            return Result.FromSuccess();
        }

        /// <inheritdoc/>
        public async Task<Result> RespondAsync(IGuildEmojisUpdate gatewayEvent, CancellationToken ct = default)
        {
            foreach (var emoji in gatewayEvent.Emojis)
            {
                if (emoji.ID is null)
                {
                    continue;
                }

                var key = KeyHelpers.CreateEmojiCacheKey(gatewayEvent.GuildID, emoji.ID.Value);
                await _cacheService.CacheAsync(key, emoji);
            }

            return Result.FromSuccess();
        }

        /// <inheritdoc/>
        public async Task<Result> RespondAsync(IGuildMemberAdd gatewayEvent, CancellationToken ct = default)
        {
            if (!gatewayEvent.User.HasValue)
            {
                return Result.FromSuccess();
            }

            var key = KeyHelpers.CreateGuildMemberKey(gatewayEvent.GuildID, gatewayEvent.User.Value.ID);
            await _cacheService.CacheAsync(key, gatewayEvent);

            return Result.FromSuccess();
        }

        /// <inheritdoc/>
        public async Task<Result> RespondAsync(IGuildMembersChunk gatewayEvent, CancellationToken ct = default)
        {
            foreach (var member in gatewayEvent.Members)
            {
                if (!member.User.HasValue)
                {
                    continue;
                }

                var key = KeyHelpers.CreateGuildMemberKey(gatewayEvent.GuildID, member.User.Value.ID);
                await _cacheService.CacheAsync(key, member);
            }

            if (!gatewayEvent.Presences.HasValue)
            {
                return Result.FromSuccess();
            }

            foreach (var presence in gatewayEvent.Presences.Value!)
            {
                if (!presence.User.ID.HasValue)
                {
                    continue;
                }

                var key = KeyHelpers.CreatePresenceCacheKey(gatewayEvent.GuildID, presence.User.ID.Value);
                await _cacheService.CacheAsync(key, presence);
            }

            return Result.FromSuccess();
        }

        /// <inheritdoc/>
        public async Task<Result> RespondAsync(IGuildMemberUpdate gatewayEvent, CancellationToken ct = default)
        {
            var key = KeyHelpers.CreateGuildMemberKey(gatewayEvent.GuildID, gatewayEvent.User.ID);

            // Since this event isn't playing nice, we'll have to update by creating an object of our own.
            var cache = await _cacheService.GetValueAsync<IGuildMember>(key);
            IGuildMember cachedInstance;

            if (cache.IsSuccess)
            {
                cachedInstance = new GuildMember
                (
                    new Optional<IUser>(gatewayEvent.User),
                    gatewayEvent.Nickname.HasValue ? gatewayEvent.Nickname : cache.Entity.Nickname,
                    gatewayEvent.Roles,
                    gatewayEvent.JoinedAt,
                    gatewayEvent.PremiumSince.HasValue ? gatewayEvent.PremiumSince.Value : cache.Entity.PremiumSince,
                    cache.Entity.IsDeafened,
                    cache.Entity.IsMuted,
                    cache.Entity.IsPending,
                    cache.Entity.Permissions
                );
            }
            else if (gatewayEvent.PremiumSince.HasValue)
            {
                cachedInstance = new GuildMember
                (
                    new Optional<IUser>(gatewayEvent.User),
                    gatewayEvent.Nickname,
                    gatewayEvent.Roles,
                    gatewayEvent.JoinedAt,
                    gatewayEvent.PremiumSince.Value,
                    false,
                    false,
                    default,
                    default
                );
            }
            else
            {
                return Result.FromSuccess();
            }

            await _cacheService.CacheAsync(key, cachedInstance);

            return Result.FromSuccess();
        }

        /// <inheritdoc/>
        public async Task<Result> RespondAsync(IGuildRoleCreate gatewayEvent, CancellationToken ct = default)
        {
            var key = KeyHelpers.CreateGuildRoleCacheKey(gatewayEvent.GuildID, gatewayEvent.Role.ID);
            await _cacheService.CacheAsync(key, gatewayEvent.Role);

            return Result.FromSuccess();
        }

        /// <inheritdoc/>
        public async Task<Result> RespondAsync(IGuildRoleUpdate gatewayEvent, CancellationToken ct = default)
        {
            var key = KeyHelpers.CreateGuildRoleCacheKey(gatewayEvent.GuildID, gatewayEvent.Role.ID);
            await _cacheService.CacheAsync(key, gatewayEvent.Role);

            return Result.FromSuccess();
        }

        /// <inheritdoc/>
        public async Task<Result> RespondAsync(IMessageCreate gatewayEvent, CancellationToken ct = default)
        {
            var key = KeyHelpers.CreateMessageCacheKey(gatewayEvent.ChannelID, gatewayEvent.ID);
            await _cacheService.CacheAsync(key, gatewayEvent);

            return Result.FromSuccess();
        }

        /// <inheritdoc/>
        public async Task<Result> RespondAsync(IMessageReactionAdd gatewayEvent, CancellationToken ct = default)
        {
            if (!gatewayEvent.GuildID.HasValue)
            {
                return Result.FromSuccess();
            }

            if (!gatewayEvent.Member.HasValue)
            {
                return Result.FromSuccess();
            }

            var member = gatewayEvent.Member.Value;
            if (!member.User.HasValue)
            {
                return Result.FromSuccess();
            }

            var key = KeyHelpers.CreateGuildMemberKey(gatewayEvent.GuildID.Value, member.User.Value.ID);
            await _cacheService.CacheAsync(key, member);

            return Result.FromSuccess();
        }

        /// <inheritdoc/>
        public async Task<Result> RespondAsync(IUserUpdate gatewayEvent, CancellationToken ct = default)
        {
            var key = KeyHelpers.CreateUserCacheKey(gatewayEvent.ID);
            await _cacheService.CacheAsync(key, gatewayEvent);

            return Result.FromSuccess();
        }

        /// <inheritdoc />
        public async Task<Result> RespondAsync(IInteractionCreate gatewayEvent, CancellationToken ct = default)
        {
            if (!gatewayEvent.Data.HasValue)
            {
                return Result.FromSuccess();
            }

            var data = gatewayEvent.Data.Value;
            if (!data.Resolved.HasValue)
            {
                return Result.FromSuccess();
            }

            var resolved = data.Resolved.Value;
            if (resolved.Users.HasValue)
            {
                var users = resolved.Users.Value;
                foreach (var (key, value) in users)
                {
                    var cacheKey = KeyHelpers.CreateUserCacheKey(key);
                    await _cacheService.CacheAsync(cacheKey, value);
                }
            }

            if (!resolved.Roles.HasValue || !gatewayEvent.GuildID.HasValue)
            {
                return Result.FromSuccess();
            }

            var roles = resolved.Roles.Value;
            foreach (var (key, value) in roles)
            {
                var cacheKey = KeyHelpers.CreateGuildRoleCacheKey(gatewayEvent.GuildID.Value, key);
                await _cacheService.CacheAsync(cacheKey, value);
            }

            return Result.FromSuccess();
        }
    }
}
