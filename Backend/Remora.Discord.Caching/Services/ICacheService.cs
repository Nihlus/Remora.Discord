//
//  ICacheService.cs
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
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Results;

namespace Remora.Discord.Caching.Services
{
    /// <summary>
    /// Handles cache insert/evict operations for various types.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Caches given webhook.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="items">The webhook.</param>
        /// <typeparam name="T">The instance type.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CacheAsync<T>(CacheKey key, IReadOnlyList<T> items);

        /// <summary>
        /// Caches given connection.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="connection">The connection.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CacheAsync(CacheKey key, IConnection connection);

        /// <summary>
        /// Caches given application.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="application">The application.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CacheAsync(CacheKey key, IApplication application);

        /// <summary>
        /// Caches given guild welcome screen.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="welcomeScreen">The welcome screen.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CacheAsync(CacheKey key, IWelcomeScreen welcomeScreen);

        /// <summary>
        /// Caches given guild widget.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="guildWidget">The webhook.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CacheAsync(CacheKey key, IGuildWidget guildWidget);

        /// <summary>
        /// Caches given webhook.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="webhook">The webhook.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CacheAsync(CacheKey key, IWebhook webhook);

        /// <summary>
        /// Caches given template.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="template">The template.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CacheAsync(CacheKey key, ITemplate template);

        /// <summary>
        /// Caches given template.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="integration">The integration.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CacheAsync(CacheKey key, IIntegration integration);

        /// <summary>
        /// Caches given ban.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="ban">The ban.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CacheAsync(CacheKey key, IBan ban);

        /// <summary>
        /// Caches given member.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="member">The member.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CacheAsync(CacheKey key, IGuildMember member);

        /// <summary>
        /// Caches given preview.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="preview">The preview.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CacheAsync(CacheKey key, IGuildPreview preview);

        /// <summary>
        /// Caches given guild.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="guild">The guild.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task CacheAsync(CacheKey key, IGuild guild);

        /// <summary>
        /// Caches given emoji.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="emoji">The emoji.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task CacheAsync(CacheKey key, IEmoji emoji);

        /// <summary>
        /// Caches given guild.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="invite">The invite.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task CacheAsync(CacheKey key, IInvite invite);

        /// <summary>
        /// Caches given guild.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="message">The message.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task CacheAsync(CacheKey key, IMessage message);

        /// <summary>
        /// Caches given guild.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="channel">The channel.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task CacheAsync(CacheKey key, IChannel channel);

        /// <summary>
        /// Caches given guild.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="user">The user.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task CacheAsync(CacheKey key, IUser user);

        /// <summary>
        /// Caches given guild.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="presence">The presence.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task CacheAsync(CacheKey key, IPresence presence);

        /// <summary>
        /// Caches given guild.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="role">The role.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task CacheAsync(CacheKey key, IRole role);

        /// <summary>
        /// Caches given guild.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="voiceRegion">The voiceRegion.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task CacheAsync(CacheKey key, IVoiceRegion voiceRegion);

        /// <summary>
        /// Attempts to retrieve a value from the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <typeparam name="TInstance">The instance type.</typeparam>
        /// <returns>value from the cache provider.</returns>
        public Task<Result<TInstance>> GetValueAsync<TInstance>(CacheKey key) where TInstance : notnull;

        /// <summary>
        /// Removes value of the given key from the cache.
        /// </summary>
        /// <param name="key">Key of the value.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task EvictAsync(CacheKey key);
    }
}
