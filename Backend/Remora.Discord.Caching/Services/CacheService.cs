//
//  CacheService.cs
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
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Caching.Clients;
using Remora.Results;

namespace Remora.Discord.Caching.Services
{
    /// <inheritdoc />
    public class CacheService : ICacheService
    {
        private readonly ICacheClient _cacheClient;
        private readonly CacheSettings _cacheSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheService"/> class.
        /// </summary>
        /// <param name="cacheClient">The cache manager.</param>
        /// <param name="cacheSettings">The cache settings.</param>
        public CacheService(IOptions<CacheSettings> cacheSettings, ICacheClient? cacheClient = null)
        {
            _cacheSettings = cacheSettings.Value;
            _cacheClient = cacheClient ?? throw new InvalidOperationException("No cache client was provided in the service collection.\nInstall the client you want to use (e.g. Remora.Discord.Caching.Memory) and configure it through Services.AddDiscordCaching(b => b.UseXXX()).");
        }

        /// <inheritdoc />
        public Task CacheAsync(CacheKey key, IRole role)
        {
            return _cacheClient.StoreAsync(key, role);
        }

        /// <inheritdoc />
        public Task CacheAsync(CacheKey key, IVoiceRegion voiceRegion)
        {
            return _cacheClient.StoreAsync(key, voiceRegion);
        }

        /// <summary>
        /// Attempts to retrieve a value from the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <typeparam name="TInstance">The instance type.</typeparam>
        /// <returns>value from the cache provider.</returns>
        public Task<Result<TInstance>> GetValueAsync<TInstance>(CacheKey key) where TInstance : notnull
        {
            return _cacheClient.RetrieveAsync<TInstance>(key);
        }

        /// <inheritdoc />
        public Task EvictAsync(CacheKey key)
        {
            return _cacheClient.EvictAsync(key);
        }

        /// <inheritdoc />
        public Task CacheAsync<T>(CacheKey key, IReadOnlyList<T> items)
        {
            return _cacheClient.StoreAsync(key, items);
        }

        /// <inheritdoc />
        public Task CacheAsync(CacheKey key, IConnection connection)
        {
            return _cacheClient.StoreAsync(key, connection);
        }

        /// <inheritdoc />
        public Task CacheAsync(CacheKey key, IApplication application)
        {
            return _cacheClient.StoreAsync(key, application);
        }

        /// <inheritdoc />
        public Task CacheAsync(CacheKey key, IWelcomeScreen welcomeScreen)
        {
            return _cacheClient.StoreAsync(key, welcomeScreen);
        }

        /// <inheritdoc />
        public Task CacheAsync(CacheKey key, IGuildWidget guildWidget)
        {
            return _cacheClient.StoreAsync(key, guildWidget);
        }

        /// <inheritdoc />
        public async Task CacheAsync(CacheKey key, IWebhook webhook)
        {
            await _cacheClient.StoreAsync(key, webhook);

            if (!webhook.User.HasValue)
            {
                return;
            }

            var user = webhook.User.Value;
            var userKey = KeyHelpers.CreateUserCacheKey(user.ID);
            await CacheAsync(userKey, user);
        }

        /// <inheritdoc />
        public async Task CacheAsync(CacheKey key, ITemplate template)
        {
            await _cacheClient.StoreAsync(key, template);

            var creatorKey = KeyHelpers.CreateUserCacheKey(template.Creator.ID);
            await CacheAsync(creatorKey, template.Creator);
        }

        /// <inheritdoc />
        public async Task CacheAsync(CacheKey key, IIntegration integration)
        {
            await _cacheClient.StoreAsync(key, integration);

            if (!integration.User.HasValue)
            {
                return;
            }

            var user = integration.User.Value;
            var userKey = KeyHelpers.CreateUserCacheKey(user.ID);
            await CacheAsync(userKey, user);
        }

        /// <inheritdoc />
        public async Task CacheAsync(CacheKey key, IBan ban)
        {
            await _cacheClient.StoreAsync(key, ban);

            var userKey = KeyHelpers.CreateUserCacheKey(ban.User.ID);
            await CacheAsync(userKey, ban.User);
        }

        /// <inheritdoc />
        public async Task CacheAsync(CacheKey key, IGuildMember member)
        {
            await _cacheClient.StoreAsync(key, member);

            if (!member.User.HasValue)
            {
                return;
            }

            var user = member.User.Value;
            var userKey = KeyHelpers.CreateUserCacheKey(user.ID);
            await CacheAsync(userKey, user);
        }

        /// <inheritdoc />
        public async Task CacheAsync(CacheKey key, IGuildPreview preview)
        {
            await _cacheClient.StoreAsync(key, preview);

            foreach (var emoji in preview.Emojis)
            {
                if (emoji.ID is null)
                {
                    continue;
                }

                var emojiKey = KeyHelpers.CreateEmojiCacheKey(preview.ID, emoji.ID.Value);
                await CacheAsync(emojiKey, emoji);
            }
        }

        /// <inheritdoc />
        public async Task CacheAsync(CacheKey key, IGuild guild)
        {
            await _cacheClient.StoreAsync(key, guild);

            if (guild.Channels.HasValue)
            {
                foreach (var channel in guild.Channels.Value!)
                {
                    var channelKey = KeyHelpers.CreateChannelCacheKey(channel.ID);

                    if (!channel.GuildID.HasValue && channel.Type is not ChannelType.DM or ChannelType.GroupDM)
                    {
                        if (channel is Channel record)
                        {
                            // Polyfill the instance with contextual data - bit of a cheat, but it's okay in this
                            // instance
                            await CacheAsync(key, record with { GuildID = guild.ID });
                        }
                    }
                    else
                    {
                        await CacheAsync(channelKey, channel);
                    }
                }
            }

            foreach (var emoji in guild.Emojis)
            {
                if (emoji.ID is null)
                {
                    continue;
                }

                var emojiKey = KeyHelpers.CreateEmojiCacheKey(guild.ID, emoji.ID.Value);
                await CacheAsync(emojiKey, emoji);
            }

            if (guild.Members.HasValue)
            {
                foreach (var guildMember in guild.Members.Value)
                {
                    if (!guildMember.User.HasValue)
                    {
                        continue;
                    }

                    var memberKey = KeyHelpers.CreateGuildMemberKey(guild.ID, guildMember.User.Value.ID);
                    await CacheAsync(memberKey, guildMember);
                }
            }

            foreach (var role in guild.Roles)
            {
                var roleKey = KeyHelpers.CreateGuildRoleCacheKey(guild.ID, role.ID);
                await CacheAsync(roleKey, role);
            }
        }

        /// <inheritdoc />
        public async Task CacheAsync(CacheKey key, IEmoji emoji)
        {
            await _cacheClient.StoreAsync(key, emoji);

            if (!emoji.User.HasValue)
            {
                return;
            }

            var creator = emoji.User.Value;
            var creatorKey = KeyHelpers.CreateUserCacheKey(creator.ID);
            await CacheAsync(creatorKey, creator);
        }

        /// <inheritdoc />
        public async Task CacheAsync(CacheKey key, IInvite invite)
        {
            await _cacheClient.StoreAsync(key, invite);

            if (!invite.Inviter.HasValue)
            {
                return;
            }

            var inviter = invite.Inviter.Value;
            var inviterKey = KeyHelpers.CreateUserCacheKey(inviter.ID);
            await CacheAsync(inviterKey, inviter);
        }

        /// <inheritdoc />
        public async Task CacheAsync(CacheKey key, IMessage message)
        {
            if (!_cacheSettings.ShouldCacheMessages)
            {
                return;
            }

            await _cacheClient.StoreAsync(key, message);

            var authorKey = KeyHelpers.CreateUserCacheKey(message.Author.ID);
            await CacheAsync(authorKey, message.Author);

            if (!message.ReferencedMessage.HasValue || message.ReferencedMessage.Value is null)
            {
                return;
            }

            var referencedMessage = message.ReferencedMessage.Value;
            var referencedMessageKey = KeyHelpers.CreateMessageCacheKey
            (
                referencedMessage.ChannelID,
                referencedMessage.ID
            );

            await CacheAsync(referencedMessageKey, referencedMessage);
        }

        /// <inheritdoc />
        public async Task CacheAsync(CacheKey key, IChannel channel)
        {
            await _cacheClient.StoreAsync(key, channel);
            if (!channel.Recipients.HasValue)
            {
                return;
            }

            foreach (var recipient in channel.Recipients.Value!)
            {
                var recipientKey = KeyHelpers.CreateUserCacheKey(recipient.ID);
                await CacheAsync(recipientKey, recipient);
            }
        }

        /// <inheritdoc />
        public Task CacheAsync(CacheKey key, IUser user)
        {
            return _cacheClient.StoreAsync(key, user);
        }

        /// <inheritdoc />
        public Task CacheAsync(CacheKey key, IPresence presence)
        {
            return _cacheSettings.ShouldCachePresences ? _cacheClient.StoreAsync(key, presence) : Task.CompletedTask;
        }
    }
}
