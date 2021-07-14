//
//  DiscordRestApplicationAPI.cs
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
using Remora.Results;

namespace Remora.Discord.Rest.API
{
    /// <inheritdoc />
    [PublicAPI]
    public class DiscordRestApplicationAPI : IDiscordRestApplicationAPI
    {
        private readonly DiscordHttpClient _discordHttpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordRestApplicationAPI"/> class.
        /// </summary>
        /// <param name="discordHttpClient">The Discord HTTP client.</param>
        /// <param name="jsonOptions">The json options.</param>
        public DiscordRestApplicationAPI
        (
            DiscordHttpClient discordHttpClient,
            IOptions<JsonSerializerOptions> jsonOptions
        )
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
        public virtual Task<Result<IReadOnlyList<IApplicationCommand>>> GetGlobalApplicationCommandsAsync
        (
            Snowflake applicationID,
            CancellationToken ct
        )
        {
            return _discordHttpClient.GetAsync<IReadOnlyList<IApplicationCommand>>
            (
                $"applications/{applicationID}/commands",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual async Task<Result<IApplicationCommand>> CreateGlobalApplicationCommandAsync
        (
            Snowflake applicationID,
            string name,
            string description,
            Optional<IReadOnlyList<IApplicationCommandOption>> options,
            Optional<bool> defaultPermission,
            CancellationToken ct
        )
        {
            if (name.Length is < 1 or > 32)
            {
                return new ArgumentOutOfRangeError
                (
                    nameof(name),
                    "The name must be between 1 and 32 characters."
                );
            }

            if (description.Length is < 1 or > 100)
            {
                return new ArgumentOutOfRangeError
                (
                    nameof(description),
                    "The description must be between 1 and 100 characters."
                );
            }

            return await _discordHttpClient.PostAsync<IApplicationCommand>
            (
                $"applications/{applicationID}/commands",
                b => b.WithJson
                (
                    json =>
                    {
                        json.WriteString("name", name);
                        json.WriteString("description", description);
                        json.Write("options", options, _jsonOptions);
                        json.Write("default_permission", defaultPermission, _jsonOptions);
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public async Task<Result<IReadOnlyList<IApplicationCommand>>> BulkOverwriteGlobalApplicationCommandsAsync
        (
            Snowflake applicationID,
            IReadOnlyList<IBulkApplicationCommandData> commands,
            CancellationToken ct = default)
        {
            if (commands.Any(c => c.Name.Length is < 1 or > 32))
            {
                return new ArgumentOutOfRangeError
                (
                    nameof(commands),
                    "Command names must be between 1 and 32 characters."
                );
            }

            if (commands.Any(c => c.Description.Length is < 1 or > 100))
            {
                return new ArgumentOutOfRangeError
                (
                    nameof(commands),
                    "Command descriptions must be between 1 and 100 characters."
                );
            }

            return await _discordHttpClient.PutAsync<IReadOnlyList<IApplicationCommand>>
            (
                $"applications/{applicationID}/commands",
                b => b.WithJsonArray
                (
                    json =>
                    {
                        foreach (var command in commands)
                        {
                            json.WriteStartObject();
                            json.WriteString("name", command.Name);
                            json.WriteString("description", command.Description);
                            json.Write("options", command.Options, _jsonOptions);
                            json.Write("default_permission", command.DefaultPermission);
                            json.WriteEndObject();
                        }
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IApplicationCommand>> GetGlobalApplicationCommandAsync
        (
            Snowflake applicationID,
            Snowflake commandID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IApplicationCommand>
            (
                $"applications/{applicationID}/commands/{commandID}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual async Task<Result<IApplicationCommand>> EditGlobalApplicationCommandAsync
        (
            Snowflake applicationID,
            Snowflake commandID,
            Optional<string> name,
            Optional<string> description,
            Optional<IReadOnlyList<IApplicationCommandOption>?> options,
            Optional<bool> defaultPermission,
            CancellationToken ct
        )
        {
            if (name.HasValue && name.Value.Length is < 1 or > 32)
            {
                return new ArgumentOutOfRangeError
                (
                    nameof(name),
                    "The name must be between 1 and 32 characters."
                );
            }

            if (description.HasValue && description.Value.Length is < 1 or > 100)
            {
                return new ArgumentOutOfRangeError
                (
                    nameof(description),
                    "The description must be between 1 and 100 characters."
                );
            }

            return await _discordHttpClient.PatchAsync<IApplicationCommand>
            (
                $"applications/{applicationID}/commands/{commandID}",
                b => b.WithJson
                (
                    json =>
                    {
                        json.Write("name", name, _jsonOptions);
                        json.Write("description", description, _jsonOptions);
                        json.Write("options", options, _jsonOptions);
                        json.Write("default_permission", defaultPermission, _jsonOptions);
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result> DeleteGlobalApplicationCommandAsync
        (
            Snowflake applicationID,
            Snowflake commandID,
            CancellationToken ct
        )
        {
            return _discordHttpClient.DeleteAsync($"applications/{applicationID}/commands/{commandID}", ct: ct);
        }

        /// <inheritdoc />
        public virtual Task<Result<IReadOnlyList<IApplicationCommand>>> GetGuildApplicationCommandsAsync
        (
            Snowflake applicationID,
            Snowflake guildID,
            CancellationToken ct
        )
        {
            return _discordHttpClient.GetAsync<IReadOnlyList<IApplicationCommand>>
            (
                $"applications/{applicationID}/guilds/{guildID}/commands",
                ct: ct
            );
        }

        /// <inheritdoc />
        public async Task<Result<IReadOnlyList<IApplicationCommand>>> BulkOverwriteGuildApplicationCommandsAsync
        (
            Snowflake applicationID,
            Snowflake guildID,
            IReadOnlyList<IBulkApplicationCommandData> commands,
            CancellationToken ct = default
        )
        {
            if (commands.Any(c => c.Name.Length is < 1 or > 32))
            {
                return new ArgumentOutOfRangeError
                (
                    nameof(commands),
                    "Command names must be between 1 and 32 characters."
                );
            }

            if (commands.Any(c => c.Description.Length is < 1 or > 100))
            {
                return new ArgumentOutOfRangeError
                (
                    nameof(commands),
                    "Command descriptions must be between 1 and 100 characters."
                );
            }

            return await _discordHttpClient.PutAsync<IReadOnlyList<IApplicationCommand>>
            (
                $"applications/{applicationID}/guilds/{guildID}/commands",
                b => b.WithJsonArray
                (
                    json =>
                    {
                        foreach (var command in commands)
                        {
                            json.WriteStartObject();
                            json.WriteString("name", command.Name);
                            json.WriteString("description", command.Description);
                            json.Write("options", command.Options, _jsonOptions);
                            json.Write("default_permission", command.DefaultPermission);
                            json.WriteEndObject();
                        }
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual async Task<Result<IApplicationCommand>> CreateGuildApplicationCommandAsync
        (
            Snowflake applicationID,
            Snowflake guildID,
            string name,
            string description,
            Optional<IReadOnlyList<IApplicationCommandOption>> options,
            Optional<bool> defaultPermission,
            CancellationToken ct
        )
        {
            if (name.Length is < 1 or > 32)
            {
                return new ArgumentOutOfRangeError
                (
                    nameof(name),
                    "The name must be between 1 and 32 characters."
                );
            }

            if (description.Length is < 1 or > 100)
            {
                return new ArgumentOutOfRangeError
                (
                    nameof(description),
                    "The description must be between 1 and 100 characters."
                );
            }

            return await _discordHttpClient.PostAsync<IApplicationCommand>
            (
                $"applications/{applicationID}/guilds/{guildID}/commands",
                b => b.WithJson
                (
                    json =>
                    {
                        json.WriteString("name", name);
                        json.WriteString("description", description);
                        json.Write("options", options, _jsonOptions);
                        json.Write("default_permission", defaultPermission, _jsonOptions);
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IApplicationCommand>> GetGuildApplicationCommandAsync
        (
            Snowflake applicationID,
            Snowflake guildID,
            Snowflake commandID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IApplicationCommand>
            (
                $"applications/{applicationID}/guilds/{guildID}/commands/{commandID}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual async Task<Result<IApplicationCommand>> EditGuildApplicationCommandAsync
        (
            Snowflake applicationID,
            Snowflake guildID,
            Snowflake commandID,
            Optional<string> name,
            Optional<string> description,
            Optional<IReadOnlyList<IApplicationCommandOption>?> options,
            Optional<bool> defaultPermission,
            CancellationToken ct
        )
        {
            if (name.HasValue && name.Value.Length is < 1 or > 32)
            {
                return new ArgumentOutOfRangeError
                (
                    nameof(name),
                    "The name must be between 1 and 32 characters."
                );
            }

            if (description.HasValue && description.Value.Length is < 1 or > 100)
            {
                return new ArgumentOutOfRangeError
                (
                    nameof(description),
                    "The description must be between 1 and 100 characters."
                );
            }

            return await _discordHttpClient.PatchAsync<IApplicationCommand>
            (
                $"applications/{applicationID}/guilds/{guildID}/commands/{commandID}",
                b => b.WithJson
                (
                    json =>
                    {
                        json.Write("name", name, _jsonOptions);
                        json.Write("description", description, _jsonOptions);
                        json.Write("options", options, _jsonOptions);
                        json.Write("default_permission", defaultPermission, _jsonOptions);
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result> DeleteGuildApplicationCommandAsync
        (
            Snowflake applicationID,
            Snowflake guildID,
            Snowflake commandID,
            CancellationToken ct
        )
        {
            return _discordHttpClient.DeleteAsync
            (
                $"applications/{applicationID}/guilds/{guildID}/commands/{commandID}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public Task<Result<IReadOnlyList<IGuildApplicationCommandPermissions>>>
        GetGuildApplicationCommandPermissionsAsync
        (
            Snowflake applicationID,
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IReadOnlyList<IGuildApplicationCommandPermissions>>
            (
                $"applications/{applicationID}/guilds/{guildID}/commands/permissions",
                ct: ct
            );
        }

        /// <inheritdoc />
        public Task<Result<IGuildApplicationCommandPermissions>> GetApplicationCommandPermissionsAsync
        (
            Snowflake applicationID,
            Snowflake guildID,
            Snowflake commandID,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.GetAsync<IGuildApplicationCommandPermissions>
            (
                $"applications/{applicationID}/guilds/{guildID}/commands/{commandID}/permissions",
                ct: ct
            );
        }

        /// <inheritdoc />
        public Task<Result<IGuildApplicationCommandPermissions>> EditApplicationCommandPermissionsAsync
        (
            Snowflake applicationID,
            Snowflake guildID,
            Snowflake commandID,
            IReadOnlyList<IApplicationCommandPermissions> permissions,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.PutAsync<IGuildApplicationCommandPermissions>
            (
                $"applications/{applicationID}/guilds/{guildID}/commands/{commandID}/permissions",
                b => b.WithJson
                (
                    json =>
                    {
                        json.WritePropertyName("permissions");
                        JsonSerializer.Serialize(json, permissions, _jsonOptions);
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public Task<Result<IReadOnlyList<IGuildApplicationCommandPermissions>>>
        BatchEditApplicationCommandPermissionsAsync
        (
            Snowflake applicationID,
            Snowflake guildID,
            IReadOnlyList<IPartialGuildApplicationCommandPermissions> permissions,
            CancellationToken ct = default
        )
        {
            return _discordHttpClient.PutAsync<IReadOnlyList<IGuildApplicationCommandPermissions>>
            (
                $"applications/{applicationID}/guilds/{guildID}/commands/permissions",
                b => b.WithJsonArray
                (
                    json =>
                    {
                        foreach (var permission in permissions)
                        {
                            JsonSerializer.Serialize(json, permission, _jsonOptions);
                        }
                    }
                ),
                ct: ct
            );
        }
    }
}
