//
//  DiscordRestWebhookAPI.cs
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
using System.Net.Http;
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
    public class DiscordRestWebhookAPI : IDiscordRestWebhookAPI
    {
        private readonly DiscordHttpClient _discordHttpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordRestWebhookAPI"/> class.
        /// </summary>
        /// <param name="discordHttpClient">The Discord HTTP client.</param>
        /// <param name="jsonOptions">The json options.</param>
        public DiscordRestWebhookAPI(DiscordHttpClient discordHttpClient, IOptions<JsonSerializerOptions> jsonOptions)
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
        public virtual async Task<Result<IWebhook>> CreateWebhookAsync
        (
            Snowflake channelID,
            string name,
            Optional<Stream?> avatar,
            CancellationToken ct = default
        )
        {
            if (name.Length is < 1 or > 80)
            {
                return new ArgumentOutOfRangeError(nameof(name), "Names must be between 1 and 80 characters");
            }

            if (name.Equals("clyde", StringComparison.InvariantCultureIgnoreCase))
            {
                return new NotSupportedError("Names cannot be \"clyde\".");
            }

            var packAvatar = await ImagePacker.PackImageAsync(avatar, ct);
            if (!packAvatar.IsSuccess)
            {
                return Result<IWebhook>.FromError(packAvatar);
            }

            var avatarData = packAvatar.Entity;

            return await _discordHttpClient.PostAsync<IWebhook>
            (
                $"channels/{channelID}/webhooks",
                b => b.WithJson
                (
                    json =>
                    {
                        json.WriteString("name", name);
                        json.WriteString("avatar", avatarData.Value);
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IReadOnlyList<IWebhook>>> GetChannelWebhooksAsync
        (
            Snowflake channelID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IReadOnlyList<IWebhook>>
            (
                $"channels/{channelID}/webhooks",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IReadOnlyList<IWebhook>>> GetGuildWebhooksAsync
        (
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IReadOnlyList<IWebhook>>
            (
                $"guilds/{guildID}/webhooks",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IWebhook>> GetWebhookAsync
        (
            Snowflake webhookID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IWebhook>
            (
                $"webhooks/{webhookID}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IWebhook>> GetWebhookWithTokenAsync
        (
            Snowflake webhookID,
            string token,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IWebhook>
            (
                $"webhooks/{webhookID}/{token}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual async Task<Result<IWebhook>> ModifyWebhookAsync
        (
            Snowflake webhookID,
            Optional<string> name = default,
            Optional<Stream?> avatar = default,
            Optional<Snowflake> channelID = default,
            CancellationToken ct = default
        )
        {
            var packAvatar = await ImagePacker.PackImageAsync(avatar, ct);
            if (!packAvatar.IsSuccess)
            {
                return Result<IWebhook>.FromError(packAvatar);
            }

            var avatarData = packAvatar.Entity;

            return await _discordHttpClient.PatchAsync<IWebhook>
            (
                $"webhooks/{webhookID}",
                b => b.WithJson
                (
                    json =>
                    {
                        json.Write("name", name, _jsonOptions);
                        json.Write("avatar", avatarData, _jsonOptions);
                        json.Write("channel_id", channelID, _jsonOptions);
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual async Task<Result<IWebhook>> ModifyWebhookWithTokenAsync
        (
            Snowflake webhookID,
            string token,
            Optional<string> name = default,
            Optional<Stream?> avatar = default,
            CancellationToken ct = default
        )
        {
            var packAvatar = await ImagePacker.PackImageAsync(avatar, ct);
            if (!packAvatar.IsSuccess)
            {
                return Result<IWebhook>.FromError(packAvatar);
            }

            var avatarData = packAvatar.Entity;

            return await _discordHttpClient.PatchAsync<IWebhook>
            (
                $"webhooks/{webhookID}/{token}",
                b => b.WithJson
                (
                    json =>
                    {
                        json.Write("name", name, _jsonOptions);
                        json.Write("avatar", avatarData, _jsonOptions);
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result> DeleteWebhookAsync(Snowflake webhookID, CancellationToken ct = default)
        {
            return _discordHttpClient.DeleteAsync
            (
                $"webhooks/{webhookID}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result> DeleteWebhookWithTokenAsync
        (
            Snowflake webhookID,
            string token,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.DeleteAsync
            (
                $"webhooks/{webhookID}/{token}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IMessage?>> ExecuteWebhookAsync
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
            Optional<IReadOnlyList<IMessageComponent>> components = default,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.PostAsync<IMessage?>
            (
                $"webhooks/{webhookID}/{token}",
                b =>
                {
                    if (shouldWait.HasValue)
                    {
                        b.AddQueryParameter("wait", shouldWait.Value.ToString());
                    }

                    if (file.HasValue)
                    {
                        b.AddContent(new StreamContent(file.Value.Content), "file", file.Value.Name);
                    }

                    b.WithJson
                    (
                        json =>
                        {
                            json.Write("content", content, _jsonOptions);
                            json.Write("username", username, _jsonOptions);
                            json.Write("avatar_url", avatarUrl, _jsonOptions);
                            json.Write("tts", isTTS, _jsonOptions);
                            json.Write("embeds", embeds, _jsonOptions);
                            json.Write("allowed_mentions", allowedMentions, _jsonOptions);
                            json.Write("thread_id", threadID, _jsonOptions);
                            json.Write("components", components, _jsonOptions);
                        }
                    );
                },
                true,
                ct
            );
        }

        /// <inheritdoc />
        public Task<Result<IMessage>> GetWebhookMessageAsync
        (
            Snowflake webhookID,
            string webhookToken,
            Snowflake messageID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IMessage>
            (
                $"webhooks/{webhookID}/{webhookToken}/messages/{messageID}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual async Task<Result<IMessage>> EditWebhookMessageAsync
        (
            Snowflake webhookID,
            string token,
            Snowflake messageID,
            Optional<string?> content = default,
            Optional<IReadOnlyList<IEmbed>?> embeds = default,
            Optional<IAllowedMentions?> allowedMentions = default,
            Optional<FileData?> file = default,
            Optional<IReadOnlyList<IAttachment>> attachments = default,
            Optional<IReadOnlyList<IMessageComponent>> components = default,
            CancellationToken ct = default
        )
        {
            if (content.HasValue && content.Value?.Length > 2000)
            {
                return new NotSupportedError("Message content is too long (max 2000).");
            }

            if (embeds.HasValue && embeds.Value?.Count > 10)
            {
                return new NotSupportedError("Too many embeds (max 10).");
            }

            return await _discordHttpClient.PatchAsync<IMessage>
            (
                $"webhooks/{webhookID}/{token}/messages/{messageID}",
                b =>
                {
                    if (file.HasValue && file.Value is not null)
                    {
                        b.AddContent(new StreamContent(file.Value.Content), "file", file.Value.Name);
                    }

                    b.WithJson
                    (
                        json =>
                        {
                            json.Write("content", content, _jsonOptions);
                            json.Write("embeds", embeds, _jsonOptions);
                            json.Write("allowed_mentions", allowedMentions, _jsonOptions);
                            json.Write("attachments", attachments, _jsonOptions);
                            json.Write("components", components, _jsonOptions);

                            if (file.HasValue && file.Value is null)
                            {
                                json.WriteNull("file");
                            }
                        }
                    );
                },
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result> DeleteWebhookMessageAsync
        (
            Snowflake webhookID,
            string token,
            Snowflake messageID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.DeleteAsync
            (
                $"webhooks/{webhookID}/{token}/messages/{messageID}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public Task<Result<IMessage>> GetOriginalInteractionResponseAsync
        (
            Snowflake applicationID,
            string interactionToken,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IMessage>
            (
                $"webhooks/{applicationID}/{interactionToken}/messages/@original",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual async Task<Result<IMessage>> EditOriginalInteractionResponseAsync
        (
            Snowflake applicationID,
            string token,
            Optional<string?> content = default,
            Optional<IReadOnlyList<IEmbed>?> embeds = default,
            Optional<IAllowedMentions?> allowedMentions = default,
            Optional<IReadOnlyList<IMessageComponent>> components = default,
            CancellationToken ct = default
        )
        {
            if (content.HasValue && content.Value?.Length > 2000)
            {
                return new NotSupportedError("Message content is too long (max 2000).");
            }

            if (embeds.HasValue && embeds.Value?.Count > 10)
            {
                return new NotSupportedError("Too many embeds (max 10).");
            }

            return await _discordHttpClient.PatchAsync<IMessage>
            (
                $"webhooks/{applicationID}/{token}/messages/@original",
                b =>
                {
                    b.WithJson
                    (
                        json =>
                        {
                            json.Write("content", content, _jsonOptions);
                            json.Write("embeds", embeds, _jsonOptions);
                            json.Write("allowed_mentions", allowedMentions, _jsonOptions);
                            json.Write("components", components, _jsonOptions);
                        }
                    );
                },
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result> DeleteOriginalInteractionResponseAsync
        (
            Snowflake applicationID,
            string token,
            CancellationToken ct
        )
        {
            return _discordHttpClient.DeleteAsync
            (
                $"webhooks/{applicationID}/{token}/messages/@original",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IMessage>> CreateFollowupMessageAsync
        (
            Snowflake applicationID,
            string token,
            Optional<string> content = default,
            Optional<string> username = default,
            Optional<string> avatarUrl = default,
            Optional<bool> isTTS = default,
            Optional<FileData> file = default,
            Optional<IReadOnlyList<IEmbed>> embeds = default,
            Optional<IAllowedMentions> allowedMentions = default,
            Optional<IReadOnlyList<IMessageComponent>> components = default,
            Optional<MessageFlags> flags = default,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.PostAsync<IMessage>
            (
                $"webhooks/{applicationID}/{token}",
                b =>
                {
                    if (file.HasValue)
                    {
                        b.AddContent(new StreamContent(file.Value.Content), "file", file.Value.Name);
                    }

                    b.WithJson
                    (
                        json =>
                        {
                            json.Write("content", content, _jsonOptions);
                            json.Write("username", username, _jsonOptions);
                            json.Write("avatar_url", avatarUrl, _jsonOptions);
                            json.Write("tts", isTTS, _jsonOptions);
                            json.Write("embeds", embeds, _jsonOptions);
                            json.Write("allowed_mentions", allowedMentions, _jsonOptions);
                            json.Write("components", components, _jsonOptions);
                            json.Write("flags", flags, _jsonOptions);
                        }
                    );
                },
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual async Task<Result<IMessage>> EditFollowupMessageAsync
        (
            Snowflake applicationID,
            string token,
            Snowflake messageID,
            Optional<string?> content = default,
            Optional<IReadOnlyList<IEmbed>?> embeds = default,
            Optional<IAllowedMentions?> allowedMentions = default,
            Optional<IReadOnlyList<IMessageComponent>> components = default,
            CancellationToken ct = default
        )
        {
            if (content.HasValue && content.Value?.Length > 2000)
            {
                return new NotSupportedError("Message content is too long (max 2000).");
            }

            if (embeds.HasValue && embeds.Value?.Count > 10)
            {
                return new NotSupportedError("Too many embeds (max 10).");
            }

            return await _discordHttpClient.PatchAsync<IMessage>
            (
                $"webhooks/{applicationID}/{token}/messages/{messageID}",
                b =>
                {
                    b.WithJson
                    (
                        json =>
                        {
                            json.Write("content", content, _jsonOptions);
                            json.Write("embeds", embeds, _jsonOptions);
                            json.Write("allowed_mentions", allowedMentions, _jsonOptions);
                            json.Write("components", components, _jsonOptions);
                        }
                    );
                },
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result> DeleteFollowupMessageAsync
        (
            Snowflake applicationID,
            string token,
            Snowflake messageID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.DeleteAsync
            (
                $"webhooks/{applicationID}/{token}/messages/{messageID}",
                ct: ct
            );
        }
    }
}
