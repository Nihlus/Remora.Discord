//
//  DiscordRestGuildAPI.cs
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
using System.Drawing;
using System.IO;
using System.Linq;
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
    public class DiscordRestGuildAPI : IDiscordRestGuildAPI
    {
        private readonly DiscordHttpClient _discordHttpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordRestGuildAPI"/> class.
        /// </summary>
        /// <param name="discordHttpClient">The Discord HTTP client.</param>
        /// <param name="jsonOptions">The json options.</param>
        public DiscordRestGuildAPI(DiscordHttpClient discordHttpClient, IOptions<JsonSerializerOptions> jsonOptions)
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
        public virtual async Task<Result<IGuild>> CreateGuildAsync
        (
            string name,
            Optional<Stream> icon = default,
            Optional<VerificationLevel> verificationLevel = default,
            Optional<MessageNotificationLevel> defaultMessageNotifications = default,
            Optional<ExplicitContentFilterLevel> explicitContentFilter = default,
            Optional<IReadOnlyList<IRole>> roles = default,
            Optional<IReadOnlyList<IPartialChannel>> channels = default,
            Optional<Snowflake> afkChannelID = default,
            Optional<TimeSpan> afkTimeout = default,
            Optional<Snowflake> systemChannelID = default,
            Optional<SystemChannelFlags> systemChannelFlags = default,
            CancellationToken ct = default
        )
        {
            if (name.Length is < 2 or > 100)
            {
                return new ArgumentOutOfRangeError(nameof(name), "The name must be between 2 and 100 characters.");
            }

            await using var memoryStream = new MemoryStream();

            var packIcon = await ImagePacker.PackImageAsync(new Optional<Stream?>(icon.Value), ct);
            if (!packIcon.IsSuccess)
            {
                return Result<IGuild>.FromError(packIcon);
            }

            var iconData = packIcon.Entity;

            return await _discordHttpClient.PostAsync<IGuild>
            (
                "guilds",
                b => b.WithJson
                (
                    json =>
                    {
                        json.WriteString("name", name);
                        json.Write("icon", iconData, _jsonOptions);
                        json.Write("verification_level", verificationLevel, _jsonOptions);
                        json.Write("default_message_notifications", defaultMessageNotifications, _jsonOptions);
                        json.Write("explicit_content_filter", explicitContentFilter, _jsonOptions);
                        json.Write("roles", roles, _jsonOptions);
                        json.Write("channels", channels, _jsonOptions);
                        json.Write("afk_channel_id", afkChannelID, _jsonOptions);

                        if (afkTimeout.HasValue)
                        {
                            json.WriteNumber("afk_timeout", (ulong)afkTimeout.Value.TotalSeconds);
                        }

                        json.Write("system_channel_id", systemChannelID, _jsonOptions);
                        json.Write("system_channel_flags", systemChannelFlags, _jsonOptions);
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IGuild>> GetGuildAsync
        (
            Snowflake guildID,
            Optional<bool> withCounts = default,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IGuild>
            (
                $"guilds/{guildID}",
                b =>
                {
                    if (withCounts.HasValue)
                    {
                        b.AddQueryParameter("with_counts", withCounts.Value.ToString());
                    }
                },
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IGuildPreview>> GetGuildPreviewAsync
        (
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IGuildPreview>
            (
                $"guilds/{guildID}/preview",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual async Task<Result<IGuild>> ModifyGuildAsync
        (
            Snowflake guildID,
            Optional<string> name = default,
            Optional<VerificationLevel?> verificationLevel = default,
            Optional<MessageNotificationLevel?> defaultMessageNotifications = default,
            Optional<ExplicitContentFilterLevel?> explicitContentFilter = default,
            Optional<Snowflake?> afkChannelID = default,
            Optional<TimeSpan> afkTimeout = default,
            Optional<Stream?> icon = default,
            Optional<Snowflake> ownerID = default,
            Optional<Stream?> splash = default,
            Optional<Stream?> discoverySplash = default,
            Optional<Stream?> banner = default,
            Optional<Snowflake?> systemChannelID = default,
            Optional<SystemChannelFlags> systemChannelFlags = default,
            Optional<Snowflake?> rulesChannelID = default,
            Optional<Snowflake?> publicUpdatesChannelID = default,
            Optional<string?> preferredLocale = default,
            Optional<IReadOnlyList<GuildFeature>> features = default,
            Optional<string?> description = default,
            CancellationToken ct = default
        )
        {
            await using var memoryStream = new MemoryStream();

            Optional<string?> iconData = default;
            if (icon.HasValue)
            {
                if (icon.Value is null)
                {
                    iconData = new Optional<string?>(null);
                }
                else
                {
                    var packImage = await ImagePacker.PackImageAsync(icon.Value, ct);
                    if (!packImage.IsSuccess)
                    {
                        return Result<IGuild>.FromError(packImage);
                    }

                    iconData = packImage.Entity;
                }
            }

            Optional<string?> splashData = default;
            if (splash.HasValue)
            {
                if (splash.Value is null)
                {
                    splashData = new Optional<string?>(null);
                }
                else
                {
                    var packImage = await ImagePacker.PackImageAsync(splash.Value, ct);
                    if (!packImage.IsSuccess)
                    {
                        return Result<IGuild>.FromError(packImage);
                    }

                    splashData = packImage.Entity;
                }
            }

            Optional<string?> discoverySplashData = default;
            if (discoverySplash.HasValue)
            {
                if (discoverySplash.Value is null)
                {
                    discoverySplashData = new Optional<string?>(null);
                }
                else
                {
                    var packImage = await ImagePacker.PackImageAsync(discoverySplash.Value, ct);
                    if (!packImage.IsSuccess)
                    {
                        return Result<IGuild>.FromError(packImage);
                    }

                    discoverySplashData = packImage.Entity;
                }
            }

            var packBanner = await ImagePacker.PackImageAsync(banner, ct);
            if (!packBanner.IsSuccess)
            {
                return Result<IGuild>.FromError(packBanner);
            }

            var bannerData = packBanner.Entity;

            return await _discordHttpClient.PatchAsync<IGuild>
            (
                $"guilds/{guildID}",
                b => b.WithJson
                (
                    json =>
                    {
                        json.Write("name", name, _jsonOptions);
                        json.Write("verification_level", verificationLevel, _jsonOptions);
                        json.Write("default_message_notifications", defaultMessageNotifications, _jsonOptions);

                        json.Write("explicit_content_filter", explicitContentFilter, _jsonOptions);
                        json.Write("afk_channel_id", afkChannelID, _jsonOptions);

                        if (afkTimeout.HasValue)
                        {
                            json.WriteNumber("afk_timeout", (ulong)afkTimeout.Value.TotalSeconds);
                        }

                        json.Write("icon", iconData, _jsonOptions);
                        json.Write("owner_id", ownerID, _jsonOptions);
                        json.Write("splash", splashData, _jsonOptions);
                        json.Write("discovery_splash", discoverySplashData, _jsonOptions);
                        json.Write("banner", bannerData, _jsonOptions);
                        json.Write("system_channel_id", systemChannelID, _jsonOptions);
                        json.Write("system_channel_flags", systemChannelFlags, _jsonOptions);
                        json.Write("rules_channel_id", rulesChannelID, _jsonOptions);
                        json.Write("public_updates_channel_id", publicUpdatesChannelID, _jsonOptions);
                        json.Write("preferred_locale", preferredLocale, _jsonOptions);
                        json.Write("features", features, _jsonOptions);
                        json.Write("description", description, _jsonOptions);
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result> DeleteGuildAsync(Snowflake guildID, CancellationToken ct = default)
        {
            return _discordHttpClient.DeleteAsync
            (
                $"guilds/{guildID}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IReadOnlyList<IChannel>>> GetGuildChannelsAsync
        (
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IReadOnlyList<IChannel>>
            (
                $"guilds/{guildID}/channels",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IChannel>> CreateGuildChannelAsync
        (
            Snowflake guildID,
            string name,
            Optional<ChannelType> type = default,
            Optional<string> topic = default,
            Optional<int> bitrate = default,
            Optional<int> userLimit = default,
            Optional<int> rateLimitPerUser = default,
            Optional<int> position = default,
            Optional<IReadOnlyList<IPermissionOverwrite>> permissionOverwrites = default,
            Optional<Snowflake> parentID = default,
            Optional<bool> isNsfw = default,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.PostAsync<IChannel>
            (
                $"guilds/{guildID}/channels",
                b => b.WithJson
                (
                    json =>
                    {
                        json.WriteString("name", name);
                        json.Write("type", type, _jsonOptions);
                        json.Write("topic", topic, _jsonOptions);
                        json.Write("bitrate", bitrate, _jsonOptions);
                        json.Write("user_limit", userLimit, _jsonOptions);
                        json.Write("rate_limit_per_user", rateLimitPerUser, _jsonOptions);
                        json.Write("position", position, _jsonOptions);
                        json.Write("permission_overwrites", permissionOverwrites, _jsonOptions);
                        json.Write("parent_id", parentID, _jsonOptions);
                        json.Write("nsfw", isNsfw, _jsonOptions);
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result> ModifyGuildChannelPositionsAsync
        (
            Snowflake guildID,
            IReadOnlyList
            <
                (
                    Snowflake ChannelID,
                    int? Position,
                    bool? LockPermissions,
                    Snowflake? ParentID
                )
            > positionModifications,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.PatchAsync
            (
                $"guilds/{guildID}/channels",
                b => b.WithJsonArray
                (
                    json =>
                    {
                        foreach (var (channelID, position, lockPermissions, parentID) in positionModifications)
                        {
                            json.WriteStartObject();
                            {
                                json.WriteString("id", channelID.ToString());
                                if (position is null)
                                {
                                    json.WriteNull("position");
                                }
                                else
                                {
                                    json.WriteNumber("position", position.Value);
                                }

                                if (lockPermissions is null)
                                {
                                    json.WriteNull("lock_permissions");
                                }
                                else
                                {
                                    json.WriteBoolean("lock_permissions", lockPermissions.Value);
                                }

                                if (parentID is null)
                                {
                                    json.WriteNull("parent_id");
                                }
                                else
                                {
                                    json.WriteString("parent_id", parentID.Value.ToString());
                                }
                            }
                            json.WriteEndObject();
                        }
                    }
                ),
                ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IGuildMember>> GetGuildMemberAsync
        (
            Snowflake guildID,
            Snowflake userID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IGuildMember>
            (
                $"guilds/{guildID}/members/{userID}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual async Task<Result<IReadOnlyList<IGuildMember>>> ListGuildMembersAsync
        (
            Snowflake guildID,
            Optional<int> limit = default,
            Optional<Snowflake> after = default,
            CancellationToken ct = default
        )
        {
            if (limit.HasValue && limit.Value is < 1 or > 1000)
            {
                return new ArgumentOutOfRangeError(nameof(limit), "The limit must be between 1 and 1000.");
            }

            return await _discordHttpClient.GetAsync<IReadOnlyList<IGuildMember>>
            (
                $"guilds/{guildID}/members",
                b =>
                {
                    if (limit.HasValue)
                    {
                        b.AddQueryParameter("limit", limit.Value.ToString());
                    }

                    if (after.HasValue)
                    {
                        b.AddQueryParameter("after", after.Value.ToString());
                    }
                },
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual async Task<Result<IReadOnlyList<IGuildMember>>> SearchGuildMembersAsync
        (
            Snowflake guildID,
            string query,
            Optional<int> limit = default,
            CancellationToken ct = default
        )
        {
            if (limit.HasValue && limit.Value is < 1 or > 1000)
            {
                return new ArgumentOutOfRangeError(nameof(limit), "The limit must be between 1 and 1000.");
            }

            return await _discordHttpClient.GetAsync<IReadOnlyList<IGuildMember>>
            (
                $"guilds/{guildID}/members/search",
                b =>
                {
                    b.AddQueryParameter("query", query);

                    if (limit.HasValue)
                    {
                        b.AddQueryParameter("limit", limit.Value.ToString());
                    }
                },
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IGuildMember?>> AddGuildMemberAsync
        (
            Snowflake guildID,
            Snowflake userID,
            string accessToken,
            Optional<string> nickname = default,
            Optional<IReadOnlyList<Snowflake>> roles = default,
            Optional<bool> isMuted = default,
            Optional<bool> isDeafened = default,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.PutAsync<IGuildMember?>
            (
                $"guilds/{guildID}/members/{userID}",
                b => b.WithJson
                (
                    json =>
                    {
                        json.WriteString("access_token", accessToken);
                        json.Write("nick", nickname, _jsonOptions);
                        json.Write("roles", roles, _jsonOptions);
                        json.Write("mute", isMuted, _jsonOptions);
                        json.Write("deaf", isDeafened, _jsonOptions);
                    }
                ),
                true,
                ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result> ModifyGuildMemberAsync
        (
            Snowflake guildID,
            Snowflake userID,
            Optional<string?> nickname = default,
            Optional<IReadOnlyList<Snowflake>?> roles = default,
            Optional<bool?> isMuted = default,
            Optional<bool?> isDeafened = default,
            Optional<Snowflake?> channelID = default,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.PatchAsync
            (
                $"guilds/{guildID}/members/{userID}",
                b => b.WithJson
                (
                    json =>
                    {
                        json.Write("nick", nickname, _jsonOptions);
                        json.Write("roles", roles, _jsonOptions);
                        json.Write("mute", isMuted, _jsonOptions);
                        json.Write("deaf", isDeafened, _jsonOptions);
                        json.Write("channel_id", channelID, _jsonOptions);
                    }
                ),
                ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<string>> ModifyCurrentUserNickAsync
        (
            Snowflake guildID,
            Optional<string?> nickname = default,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.PatchAsync<string>
            (
                $"guilds/{guildID}/members/@me/nick",
                b => b.WithJson(json => json.Write("nick", nickname, _jsonOptions)),
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result> AddGuildMemberRoleAsync
        (
            Snowflake guildID,
            Snowflake userID,
            Snowflake roleID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.PutAsync
            (
                $"guilds/{guildID}/members/{userID}/roles/{roleID}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result> RemoveGuildMemberRoleAsync
        (
            Snowflake guildID,
            Snowflake userID,
            Snowflake roleID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.DeleteAsync
            (
                $"guilds/{guildID}/members/{userID}/roles/{roleID}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result> RemoveGuildMemberAsync
        (
            Snowflake guildID,
            Snowflake userID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.DeleteAsync
            (
                $"guilds/{guildID}/members/{userID}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IReadOnlyList<IBan>>> GetGuildBansAsync
        (
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IReadOnlyList<IBan>>
            (
                $"guilds/{guildID}/bans",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IBan>> GetGuildBanAsync
        (
            Snowflake guildID,
            Snowflake userID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IBan>
            (
                $"guilds/{guildID}/bans/{userID}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result> CreateGuildBanAsync
        (
            Snowflake guildID,
            Snowflake userID,
            Optional<int> deleteMessageDays = default,
            Optional<string> reason = default,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.PutAsync
            (
                $"guilds/{guildID}/bans/{userID}",
                b => b.WithJson
                (
                    json =>
                    {
                        json.Write("delete_message_days", deleteMessageDays, _jsonOptions);
                        json.Write("reason", reason, _jsonOptions);
                    }
                ),
                ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result> RemoveGuildBanAsync
        (
            Snowflake guildID,
            Snowflake userID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.DeleteAsync
            (
                $"guilds/{guildID}/bans/{userID}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IReadOnlyList<IRole>>> GetGuildRolesAsync
        (
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IReadOnlyList<IRole>>
            (
                $"guilds/{guildID}/roles",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IRole>> CreateGuildRoleAsync
        (
            Snowflake guildID,
            Optional<string> name = default,
            Optional<IDiscordPermissionSet> permissions = default,
            Optional<Color> colour = default,
            Optional<bool> isHoisted = default,
            Optional<bool> isMentionable = default,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.PostAsync<IRole>
            (
                $"guilds/{guildID}/roles",
                b => b.WithJson
                (
                    json =>
                    {
                        json.Write("name", name, _jsonOptions);
                        json.Write("permissions", permissions, _jsonOptions);
                        json.Write("color", colour, _jsonOptions);
                        json.Write("hoist", isHoisted, _jsonOptions);
                        json.Write("mentionable", isMentionable, _jsonOptions);
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IReadOnlyList<IRole>>> ModifyGuildRolePositionsAsync
        (
            Snowflake guildID,
            IReadOnlyList<(Snowflake RoleID, Optional<int?> Position)> modifiedPositions,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.PatchAsync<IReadOnlyList<IRole>>
            (
                $"guilds/{guildID}/roles",
                b => b.WithJsonArray
                (
                    json =>
                    {
                        foreach (var (roleID, position) in modifiedPositions)
                        {
                            json.WriteStartObject();
                            {
                                json.WriteString("id", roleID.ToString());
                                json.Write("position", position, _jsonOptions);
                            }
                            json.WriteEndObject();
                        }
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IRole>> ModifyGuildRoleAsync
        (
            Snowflake guildID,
            Snowflake roleID,
            Optional<string?> name = default,
            Optional<IDiscordPermissionSet?> permissions = default,
            Optional<Color?> colour = default,
            Optional<bool?> isHoisted = default,
            Optional<bool?> isMentionable = default,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.PatchAsync<IRole>
            (
                $"guilds/{guildID}/roles/{roleID}",
                b => b.WithJson
                (
                    json =>
                    {
                        json.Write("name", name, _jsonOptions);
                        json.Write("permissions", permissions, _jsonOptions);
                        json.Write("color", colour, _jsonOptions);
                        json.Write("hoist", isHoisted, _jsonOptions);
                        json.Write("mentionable", isMentionable, _jsonOptions);
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result> DeleteGuildRoleAsync
        (
            Snowflake guildId,
            Snowflake roleID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.DeleteAsync
            (
                $"guilds/{guildId}/roles/{roleID}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual async Task<Result<IPruneCount>> GetGuildPruneCountAsync
        (
            Snowflake guildID,
            Optional<int> days = default,
            Optional<IReadOnlyList<Snowflake>> includeRoles = default,
            CancellationToken ct = default
        )
        {
            if (days.HasValue && days.Value is < 1 or > 30)
            {
                return new ArgumentOutOfRangeError
                (
                    nameof(days),
                    "The requested number of days must be between 1 and 30."
                );
            }

            return await _discordHttpClient.GetAsync<IPruneCount>
            (
                $"guilds/{guildID}/prune",
                b =>
                {
                    if (days.HasValue)
                    {
                        b.AddQueryParameter("days", days.Value.ToString());
                    }

                    if (includeRoles.HasValue)
                    {
                        b.AddQueryParameter
                        (
                            "include_roles",
                            string.Join(',', includeRoles.Value.Select(s => s.ToString()))
                        );
                    }
                },
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual async Task<Result<IPruneCount>> BeginGuildPruneAsync
        (
            Snowflake guildID,
            Optional<int> days = default,
            Optional<bool> computePruneCount = default,
            Optional<IReadOnlyList<Snowflake>> includeRoles = default,
            Optional<string> reason = default,
            CancellationToken ct = default
        )
        {
            if (days.HasValue && days.Value is < 1 or > 30)
            {
                return new ArgumentOutOfRangeError
                (
                    nameof(days),
                    "The requested number of days must be between 1 and 30."
                );
            }

            return await _discordHttpClient.PostAsync<IPruneCount>
            (
                $"guilds/{guildID}/prune",
                b => b.WithJson
                (
                    json =>
                    {
                        json.Write("days", days, _jsonOptions);
                        json.Write("compute_prune_count", computePruneCount, _jsonOptions);
                        json.Write("include_roles", includeRoles, _jsonOptions);
                        json.Write("reason", reason, _jsonOptions);
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IReadOnlyList<IVoiceRegion>>> GetGuildVoiceRegionsAsync
        (
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IReadOnlyList<IVoiceRegion>>
            (
                $"guilds/{guildID}/regions",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IReadOnlyList<IInvite>>> GetGuildInvitesAsync
        (
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IReadOnlyList<IInvite>>
            (
                $"guilds/{guildID}/invites",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IReadOnlyList<IIntegration>>> GetGuildIntegrationsAsync
        (
            Snowflake guildID,
            Optional<bool> includeApplications = default,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IReadOnlyList<IIntegration>>
            (
                $"guilds/{guildID}/integrations",
                b =>
                {
                    if (includeApplications.HasValue)
                    {
                        b.AddQueryParameter
                        (
                            "include_applications",
                            includeApplications.Value.ToString().ToLowerInvariant()
                        );
                    }
                },
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IGuildWidget>> GetGuildWidgetSettingsAsync
        (
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IGuildWidget>
            (
                $"guilds/{guildID}/widget",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IGuildWidget>> ModifyGuildWidgetAsync
        (
            Snowflake guildID,
            Optional<bool> isEnabled = default,
            Optional<Snowflake?> channelID = default,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.PatchAsync<IGuildWidget>
            (
                $"guilds/{guildID}/widget",
                b => b.WithJson
                (
                    json =>
                    {
                        json.Write("enabled", isEnabled, _jsonOptions);
                        json.Write("channel_id", channelID, _jsonOptions);
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IPartialInvite>> GetGuildVanityUrlAsync
        (
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IPartialInvite>
            (
                $"guilds/{guildID}/vanity-url",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<Stream>> GetGuildWidgetImageAsync
        (
            Snowflake guildID,
            Optional<WidgetImageStyle> style = default,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetContentAsync
            (
                $"guilds/{guildID}/widget.png",
                b =>
                {
                    if (style.HasValue)
                    {
                        b.AddQueryParameter("style", style.Value.ToString().ToLowerInvariant());
                    }
                },
                ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IWelcomeScreen>> GetGuildWelcomeScreenAsync
        (
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IWelcomeScreen>
            (
                $"guilds/{guildID}/welcome-screen",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IWelcomeScreen>> ModifyGuildWelcomeScreenAsync
        (
            Snowflake guildID,
            Optional<bool?> isEnabled = default,
            Optional<IReadOnlyList<IWelcomeScreenChannel>?> welcomeChannels = default,
            Optional<string?> description = default,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.PatchAsync<IWelcomeScreen>
            (
                $"guilds/{guildID}/welcome-screen",
                b =>
                {
                    b.WithJson
                    (
                        json =>
                        {
                            json.Write("enabled", isEnabled, _jsonOptions);
                            json.Write("welcome_channels", welcomeChannels, _jsonOptions);
                            json.Write("description", description, _jsonOptions);
                        }
                    );
                },
                ct: ct
            );
        }

        /// <inheritdoc/>
        public virtual Task<Result<IVoiceState>> ModifyCurrentUserVoiceStateAsync
        (
            Snowflake guildID,
            Snowflake channelID,
            Optional<bool> suppress = default,
            Optional<DateTimeOffset?> requestToSpeakTimestamp = default,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.PatchAsync<IVoiceState>
            (
                $"guilds/{guildID}/voice-states/@me",
                b => b.WithJson
                (
                    json =>
                    {
                        json.WriteString("channel_id", channelID.ToString());
                        json.Write("suppress", suppress, _jsonOptions);
                        json.Write("request_to_speak_timestamp", requestToSpeakTimestamp, _jsonOptions);
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IVoiceState>> ModifyUserVoiceStateAsync
        (
            Snowflake guildID,
            Snowflake userID,
            Snowflake channelID,
            Optional<bool> suppress = default,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.PatchAsync<IVoiceState>
            (
                $"guilds/{guildID}/voice-states/{userID}",
                b => b.WithJson
                (
                    json =>
                    {
                        json.WriteString("channel_id", channelID.ToString());
                        json.Write("suppress", suppress, _jsonOptions);
                    }
                ),
                ct: ct
            );
        }
    }
}
