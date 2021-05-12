//
//  Program.cs
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Commands.Extensions;
using Remora.Discord.Caching;
using Remora.Discord.Caching.Extensions;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Services;
using Remora.Discord.Core;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Gateway.Results;
using Remora.Discord.Samples.SlashCommands.Commands;
using Remora.Results;

namespace Remora.Discord.Samples.SlashCommands
{
    /// <summary>
    /// Represents the main class of the program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entrypoint of the program.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous program execution.</returns>
        public static async Task Main(string[] args)
        {
            var cancellationSource = new CancellationTokenSource();

            Console.CancelKeyPress += (_, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cancellationSource.Cancel();
            };

            var botToken =
                Environment.GetEnvironmentVariable("REMORA_BOT_TOKEN")
                ?? throw new InvalidOperationException
                (
                    "No bot token has been provided. Set the REMORA_BOT_TOKEN environment variable to a valid token."
                );

            var serviceCollection = new ServiceCollection()
                .AddLogging
                (
                    c => c
                        .AddConsole()
                        .AddFilter("System.Net.Http.HttpClient.*.LogicalHandler", LogLevel.Warning)
                        .AddFilter("System.Net.Http.HttpClient.*.ClientHandler", LogLevel.Warning)
                )
                .AddDiscordGateway(_ => botToken)
                .AddDiscordCommands(true)
                .AddCommandGroup<HttpCatCommands>();

            if (Environment.GetEnvironmentVariable("REMORA_REDIS") is { Length: > 0 } connectionString)
            {
                serviceCollection.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = connectionString;
                });

                serviceCollection.AddDiscordCaching(b => b.UseDistributedCache());
            }
            else
            {
                serviceCollection.AddDiscordCaching();
            }

            serviceCollection.AddHttpClient();

            var services = serviceCollection.BuildServiceProvider(true);
            var log = services.GetRequiredService<ILogger<Program>>();

            Snowflake? debugServer = null;
#if DEBUG
            var debugServerString = Environment.GetEnvironmentVariable("REMORA_DEBUG_SERVER");
            if (debugServerString is not null)
            {
                if (!Snowflake.TryParse(debugServerString, out debugServer))
                {
                    log.LogWarning("Failed to parse debug server from environment");
                }
            }
#endif

            var slashService = services.GetRequiredService<SlashService>();

            var checkSlashSupport = slashService.SupportsSlashCommands();
            if (!checkSlashSupport.IsSuccess)
            {
                log.LogWarning
                (
                    "The registered commands of the bot don't support slash commands: {Reason}",
                    checkSlashSupport.Unwrap().Message
                );
            }
            else
            {
                var updateSlash = await slashService.UpdateSlashCommandsAsync(debugServer, cancellationSource.Token);
                if (!updateSlash.IsSuccess)
                {
                    log.LogWarning("Failed to update slash commands: {Reason}", updateSlash.Unwrap().Message);
                }
            }

            var gatewayClient = services.GetRequiredService<DiscordGatewayClient>();

            var runResult = await gatewayClient.RunAsync(cancellationSource.Token);
            if (!runResult.IsSuccess)
            {
                switch (runResult.Error)
                {
                    case ExceptionError exe:
                    {
                        log.LogError
                        (
                            exe.Exception,
                            "Exception during gateway connection: {ExceptionMessage}",
                            exe.Message
                        );

                        break;
                    }
                    case GatewayWebSocketError:
                    case GatewayDiscordError:
                    {
                        log.LogError("Gateway error: {Message}", runResult.Unwrap().Message);
                        break;
                    }
                    default:
                    {
                        log.LogError("Unknown error: {Message}", runResult.Unwrap().Message);
                        break;
                    }
                }
            }

            log.LogInformation("Bye bye");
        }
    }
}
