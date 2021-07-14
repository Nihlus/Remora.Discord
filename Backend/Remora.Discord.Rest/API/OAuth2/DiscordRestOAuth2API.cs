//
//  DiscordRestOAuth2API.cs
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
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Results;

namespace Remora.Discord.Rest.API
{
    /// <inheritdoc />
    [PublicAPI]
    public class DiscordRestOAuth2API : IDiscordRestOAuth2API
    {
        private readonly DiscordHttpClient _discordHttpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordRestOAuth2API"/> class.
        /// </summary>
        /// <param name="discordHttpClient">The Discord HTTP client.</param>
        public DiscordRestOAuth2API(DiscordHttpClient discordHttpClient)
        {
            _discordHttpClient = discordHttpClient;
        }

        /// <inheritdoc cref="DiscordHttpClient.WithCustomization"/>
        public DiscordRequestCustomization WithCustomization(Action<RestRequestBuilder> requestCustomizer)
        {
            return _discordHttpClient.WithCustomization(requestCustomizer);
        }

        /// <inheritdoc />
        public virtual Task<Result<IApplication>> GetCurrentBotApplicationInformationAsync
        (
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IApplication>
            (
                "oauth2/applications/@me",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IAuthorizationInformation>> GetCurrentAuthorizationInformationAsync
        (
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IAuthorizationInformation>
            (
                "oauth2/@me",
                ct: ct
            );
        }
    }
}
