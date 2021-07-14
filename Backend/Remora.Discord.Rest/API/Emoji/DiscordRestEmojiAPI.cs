//
//  DiscordRestEmojiAPI.cs
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
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;
using Remora.Discord.Rest.Extensions;
using Remora.Discord.Rest.Utility;
using Remora.Results;

namespace Remora.Discord.Rest.API
{
    /// <inheritdoc />
    [PublicAPI]
    public class DiscordRestEmojiAPI : IDiscordRestEmojiAPI
    {
        private readonly DiscordHttpClient _discordHttpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordRestEmojiAPI"/> class.
        /// </summary>
        /// <param name="discordHttpClient">The Discord HTTP client.</param>
        /// <param name="jsonOptions">The JSON options.</param>
        public DiscordRestEmojiAPI(DiscordHttpClient discordHttpClient, IOptions<JsonSerializerOptions> jsonOptions)
        {
            _discordHttpClient = discordHttpClient;
            _jsonOptions = jsonOptions.Value;
        }

        /// <inheritdoc cref="DiscordHttpClient.WithCustomization"/>
        public DiscordRequestCustomization WithCustomization(Action<RestRequestBuilder> requestCustomizer)
        {
            return _discordHttpClient.WithCustomization(requestCustomizer);
        }

        /// <inheritdoc />
        public virtual Task<Result<IReadOnlyList<IEmoji>>> ListGuildEmojisAsync
        (
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IReadOnlyList<IEmoji>>
            (
                $"guilds/{guildID}/emojis",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IEmoji>> GetGuildEmojiAsync
        (
            Snowflake guildID,
            Snowflake emojiID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IEmoji>
            (
                $"guilds/{guildID}/emojis/{emojiID}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual async Task<Result<IEmoji>> CreateGuildEmojiAsync
        (
            Snowflake guildID,
            string name,
            Stream image,
            IReadOnlyList<Snowflake> roles,
            CancellationToken ct = default
        )
        {
            if (image.Length > 256000)
            {
                return new NotSupportedError("Image too large (max 256k).");
            }

            var packImage = await ImagePacker.PackImageAsync(image, ct);
            if (!packImage.IsSuccess)
            {
                return Result<IEmoji>.FromError(packImage);
            }

            var emojiData = packImage.Entity;

            return await _discordHttpClient.PostAsync<IEmoji>
            (
                $"guilds/{guildID}/emojis",
                b => b.WithJson
                (
                    json =>
                    {
                        json.WriteString("name", name);
                        json.WriteString("image", emojiData);

                        json.WritePropertyName("roles");
                        JsonSerializer.Serialize(json, roles, _jsonOptions);
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IEmoji>> ModifyGuildEmojiAsync
        (
            Snowflake guildID,
            Snowflake emojiID,
            Optional<string> name = default,
            Optional<IReadOnlyList<Snowflake>?> roles = default,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.PatchAsync<IEmoji>
            (
                $"guilds/{guildID}/emojis/{emojiID}",
                b => b.WithJson
                (
                    json =>
                    {
                        json.Write("name", name, _jsonOptions);
                        json.Write("roles", roles, _jsonOptions);
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result> DeleteGuildEmojiAsync
        (
            Snowflake guildID,
            Snowflake emojiID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.DeleteAsync
            (
                $"guilds/{guildID}/emojis/{emojiID}",
                ct: ct
            );
        }
    }
}
