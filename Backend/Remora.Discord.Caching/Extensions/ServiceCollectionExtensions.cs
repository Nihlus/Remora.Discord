//
//  ServiceCollectionExtensions.cs
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Caching.API;
using Remora.Discord.Caching.Clients;
using Remora.Discord.Caching.Responders;
using Remora.Discord.Caching.Services;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Gateway.Responders;

namespace Remora.Discord.Caching.Extensions
{
    /// <summary>
    /// Defines extension methods for the <see cref="IServiceCollection"/> interface.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds caching implementations of various API types, overriding the normally non-caching versions.
        /// </summary>
        /// <remarks>
        /// By default the client <see cref="MemoryCacheClient"/> will be used if no other client
        /// is added to the service collection. To register a new cache client, use
        /// <see cref="CacheBuilder.UseClient{TClient}"/>.
        /// </remarks>
        /// <remarks>
        /// To use a different cache entry options for any cached type can be configured using
        /// <see cref="IOptions{CacheSettings}"/>.
        /// </remarks>
        /// <param name="services">The services.</param>
        /// <param name="configure">Action to configure the caching.</param>
        /// <returns>The services, with caching enabled.</returns>
        public static IServiceCollection AddDiscordCaching(this IServiceCollection services, Action<CacheBuilder>? configure = null)
        {
            configure?.Invoke(new CacheBuilder(services));

            services.AddMemoryCache();
            services.AddOptions<CacheSettings>();
            services.TryAddSingleton<ICacheService, CacheService>();
            services.TryAddSingleton<ICacheClient, MemoryCacheClient>();

            services
                .Replace(ServiceDescriptor.Singleton<IDiscordRestChannelAPI, CachingDiscordRestChannelAPI>())
                .Replace(ServiceDescriptor.Singleton<IDiscordRestEmojiAPI, CachingDiscordRestEmojiAPI>())
                .Replace(ServiceDescriptor.Singleton<IDiscordRestGuildAPI, CachingDiscordRestGuildAPI>())
                .Replace(ServiceDescriptor.Singleton<IDiscordRestInviteAPI, CachingDiscordRestInviteAPI>())
                .Replace(ServiceDescriptor.Singleton<IDiscordRestOAuth2API, CachingDiscordRestOAuth2API>())
                .Replace(ServiceDescriptor.Singleton<IDiscordRestTemplateAPI, CachingDiscordRestTemplateAPI>())
                .Replace(ServiceDescriptor.Singleton<IDiscordRestUserAPI, CachingDiscordRestUserAPI>())
                .Replace(ServiceDescriptor.Singleton<IDiscordRestVoiceAPI, CachingDiscordRestVoiceAPI>())
                .Replace(ServiceDescriptor.Singleton<IDiscordRestWebhookAPI, CachingDiscordRestWebhookAPI>());

            services
                .AddResponder<EarlyCacheResponder>(ResponderGroup.Early)
                .AddResponder<LateCacheResponder>(ResponderGroup.Late);

            return services;
        }
    }
}
