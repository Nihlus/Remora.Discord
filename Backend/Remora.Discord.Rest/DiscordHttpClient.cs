//
//  DiscordHttpClient.cs
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
using Remora.Discord.API.Objects;
using Remora.Discord.Rest.API;
using Remora.Discord.Rest.Results;
using Remora.Results;

namespace Remora.Discord.Rest
{
    /// <summary>
    /// Represents a specialized HTTP client for the Discord API.
    /// </summary>
    [PublicAPI]
    public class DiscordHttpClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly List<DiscordRequestCustomization> _customizations;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordHttpClient"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The Http client factory.</param>
        /// <param name="serializerOptions">The serialization options.</param>
        public DiscordHttpClient
        (
            IHttpClientFactory httpClientFactory,
            IOptions<JsonSerializerOptions> serializerOptions
        )
        {
            _httpClientFactory = httpClientFactory;
            _serializerOptions = serializerOptions.Value;
            _customizations = new List<DiscordRequestCustomization>();
        }

        /// <summary>
        /// Creates a customization that will be applied to all requests made by the <see cref="DiscordHttpClient"/>.
        /// The customization is removed when it is disposed.
        /// </summary>
        /// <param name="requestCustomizer">The action that customizes the request.</param>
        /// <returns>The created customization.</returns>
        public DiscordRequestCustomization WithCustomization(Action<RestRequestBuilder> requestCustomizer)
        {
            var customization = new DiscordRequestCustomization(this, requestCustomizer);
            _customizations.Add(customization);

            return customization;
        }

        /// <summary>
        /// Removes a customization from the client.
        /// </summary>
        /// <param name="customization">The customization to remove.</param>
        internal void RemoveCustomization(DiscordRequestCustomization customization)
        {
            _customizations.Remove(customization);
        }

        /// <summary>
        /// Performs a GET request to the Discord REST API at the given endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="configureRequestBuilder">The request builder for the request.</param>
        /// <param name="allowNullReturn">Whether to allow null return values inside the creation result.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <typeparam name="TEntity">The entity type to retrieve.</typeparam>
        /// <returns>A retrieval result which may or may not have succeeded.</returns>
        public async Task<Result<TEntity>> GetAsync<TEntity>
        (
            string endpoint,
            Action<RestRequestBuilder>? configureRequestBuilder = null,
            bool allowNullReturn = false,
            CancellationToken ct = default
        )
        {
            configureRequestBuilder ??= _ => { };

            var requestBuilder = new RestRequestBuilder(endpoint);
            configureRequestBuilder(requestBuilder);

            requestBuilder.WithMethod(HttpMethod.Get);

            foreach (var customization in _customizations)
            {
                customization.RequestCustomizer(requestBuilder);
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("Discord");
                using var request = requestBuilder.Build();
                using var response = await httpClient.SendAsync
                (
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    ct
                );

                return await UnpackResponseAsync<TEntity>(response, allowNullReturn, ct);
            }
            catch (Exception e)
            {
                return e;
            }
        }

        /// <summary>
        /// Performs a GET request to the Discord REST API at the given endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="configureRequestBuilder">The request builder for the request.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A retrieval result which may or may not have succeeded.</returns>
        public async Task<Result<Stream>> GetContentAsync
        (
            string endpoint,
            Action<RestRequestBuilder>? configureRequestBuilder = null,
            CancellationToken ct = default
        )
        {
            configureRequestBuilder ??= _ => { };

            var requestBuilder = new RestRequestBuilder(endpoint);
            configureRequestBuilder(requestBuilder);

            requestBuilder.WithMethod(HttpMethod.Get);

            foreach (var customization in _customizations)
            {
                customization.RequestCustomizer(requestBuilder);
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("Discord");
                using var request = requestBuilder.Build();
                using var response = await httpClient.SendAsync
                (
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    ct
                );

                var unpackedResponse = await UnpackResponseAsync(response, ct);
                if (!unpackedResponse.IsSuccess)
                {
                    return Result<Stream>.FromError(unpackedResponse);
                }

                var responseContent = await response.Content.ReadAsStreamAsync();
                return Result<Stream>.FromSuccess(responseContent);
            }
            catch (Exception e)
            {
                return e;
            }
        }

        /// <summary>
        /// Performs a POST request to the Discord REST API at the given endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="configureRequestBuilder">The request builder for the request.</param>
        /// <param name="allowNullReturn">Whether to allow null return values inside the creation result.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <typeparam name="TEntity">The entity type to create.</typeparam>
        /// <returns>A creation result which may or may not have succeeded.</returns>
        public async Task<Result<TEntity>> PostAsync<TEntity>
        (
            string endpoint,
            Action<RestRequestBuilder>? configureRequestBuilder = null,
            bool allowNullReturn = false,
            CancellationToken ct = default
        )
        {
            configureRequestBuilder ??= _ => { };

            var requestBuilder = new RestRequestBuilder(endpoint);
            configureRequestBuilder(requestBuilder);

            requestBuilder.WithMethod(HttpMethod.Post);

            foreach (var customization in _customizations)
            {
                customization.RequestCustomizer(requestBuilder);
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("Discord");
                using var request = requestBuilder.Build();
                using var response = await httpClient.SendAsync
                (
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    ct
                );

                return await UnpackResponseAsync<TEntity>(response, allowNullReturn, ct);
            }
            catch (Exception e)
            {
                return e;
            }
        }

        /// <summary>
        /// Performs a POST request to the Discord REST API at the given endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="configureRequestBuilder">The request builder for the request.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A post result which may or may not have succeeded.</returns>
        public async Task<Result> PostAsync
        (
            string endpoint,
            Action<RestRequestBuilder>? configureRequestBuilder = null,
            CancellationToken ct = default
        )
        {
            configureRequestBuilder ??= _ => { };

            var requestBuilder = new RestRequestBuilder(endpoint);
            configureRequestBuilder(requestBuilder);

            requestBuilder.WithMethod(HttpMethod.Post);

            foreach (var customization in _customizations)
            {
                customization.RequestCustomizer(requestBuilder);
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("Discord");
                using var request = requestBuilder.Build();
                using var response = await httpClient.SendAsync
                (
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    ct
                );

                return await UnpackResponseAsync(response, ct);
            }
            catch (Exception e)
            {
                return e;
            }
        }

        /// <summary>
        /// Performs a PATCH request to the Discord REST API at the given endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="configureRequestBuilder">The request builder for the request.</param>
        /// <param name="allowNullReturn">Whether to allow null return values inside the creation result.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <typeparam name="TEntity">The entity type to modify.</typeparam>
        /// <returns>A modification result which may or may not have succeeded.</returns>
        public async Task<Result<TEntity>> PatchAsync<TEntity>
        (
            string endpoint,
            Action<RestRequestBuilder>? configureRequestBuilder = null,
            bool allowNullReturn = false,
            CancellationToken ct = default
        )
        {
            configureRequestBuilder ??= _ => { };

            var requestBuilder = new RestRequestBuilder(endpoint);
            configureRequestBuilder(requestBuilder);

            requestBuilder.WithMethod(HttpMethod.Patch);

            foreach (var customization in _customizations)
            {
                customization.RequestCustomizer(requestBuilder);
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("Discord");
                using var request = requestBuilder.Build();
                using var response = await httpClient.SendAsync
                (
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    ct
                );

                return await UnpackResponseAsync<TEntity>(response, allowNullReturn, ct);
            }
            catch (Exception e)
            {
                return e;
            }
        }

        /// <summary>
        /// Performs a PATCH request to the Discord REST API at the given endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="configureRequestBuilder">The request builder for the request.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A modification result which may or may not have succeeded.</returns>
        public async Task<Result> PatchAsync
        (
            string endpoint,
            Action<RestRequestBuilder>? configureRequestBuilder = null,
            CancellationToken ct = default
        )
        {
            configureRequestBuilder ??= _ => { };

            var requestBuilder = new RestRequestBuilder(endpoint);
            configureRequestBuilder(requestBuilder);

            requestBuilder.WithMethod(HttpMethod.Patch);

            foreach (var customization in _customizations)
            {
                customization.RequestCustomizer(requestBuilder);
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("Discord");
                using var request = requestBuilder.Build();
                using var response = await httpClient.SendAsync
                (
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    ct
                );

                return await UnpackResponseAsync(response, ct);
            }
            catch (Exception e)
            {
                return e;
            }
        }

        /// <summary>
        /// Performs a DELETE request to the Discord REST API at the given endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="configureRequestBuilder">The request builder for the request.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A deletion result which may or may not have succeeded.</returns>
        public async Task<Result> DeleteAsync
        (
            string endpoint,
            Action<RestRequestBuilder>? configureRequestBuilder = null,
            CancellationToken ct = default
        )
        {
            configureRequestBuilder ??= _ => { };

            var requestBuilder = new RestRequestBuilder(endpoint);
            configureRequestBuilder(requestBuilder);

            requestBuilder.WithMethod(HttpMethod.Delete);

            foreach (var customization in _customizations)
            {
                customization.RequestCustomizer(requestBuilder);
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("Discord");
                using var request = requestBuilder.Build();
                using var response = await httpClient.SendAsync
                (
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    ct
                );

                return await UnpackResponseAsync(response, ct);
            }
            catch (Exception e)
            {
                return e;
            }
        }

        /// <summary>
        /// Performs a DELETE request to the Discord REST API at the given endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="configureRequestBuilder">The request builder for the request.</param>
        /// <param name="allowNullReturn">Whether to allow null return values inside the creation result.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <typeparam name="TEntity">The type of entity to create.</typeparam>
        /// <returns>A result which may or may not have succeeded.</returns>
        public async Task<Result<TEntity>> DeleteAsync<TEntity>
        (
            string endpoint,
            Action<RestRequestBuilder>? configureRequestBuilder = null,
            bool allowNullReturn = false,
            CancellationToken ct = default
        )
        {
            configureRequestBuilder ??= _ => { };

            var requestBuilder = new RestRequestBuilder(endpoint);
            configureRequestBuilder(requestBuilder);

            requestBuilder.WithMethod(HttpMethod.Delete);

            foreach (var customization in _customizations)
            {
                customization.RequestCustomizer(requestBuilder);
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("Discord");
                using var request = requestBuilder.Build();
                using var response = await httpClient.SendAsync
                (
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    ct
                );

                return await UnpackResponseAsync<TEntity>(response, allowNullReturn, ct);
            }
            catch (Exception e)
            {
                return e;
            }
        }

        /// <summary>
        /// Performs a PUT request to the Discord REST API at the given endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="configureRequestBuilder">The request builder for the request.</param>
        /// <param name="allowNullReturn">Whether to allow null return values inside the creation result.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <typeparam name="TEntity">The type of entity to create.</typeparam>
        /// <returns>A result which may or may not have succeeded.</returns>
        public async Task<Result<TEntity>> PutAsync<TEntity>
        (
            string endpoint,
            Action<RestRequestBuilder>? configureRequestBuilder = null,
            bool allowNullReturn = false,
            CancellationToken ct = default
        )
        {
            configureRequestBuilder ??= _ => { };

            var requestBuilder = new RestRequestBuilder(endpoint);
            configureRequestBuilder(requestBuilder);

            requestBuilder.WithMethod(HttpMethod.Put);

            foreach (var customization in _customizations)
            {
                customization.RequestCustomizer(requestBuilder);
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("Discord");
                using var request = requestBuilder.Build();
                using var response = await httpClient.SendAsync
                (
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    ct
                );

                return await UnpackResponseAsync<TEntity>(response, allowNullReturn, ct);
            }
            catch (Exception e)
            {
                return e;
            }
        }

        /// <summary>
        /// Performs a PUT request to the Discord REST API at the given endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="configureRequestBuilder">The request builder for the request.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A result which may or may not have succeeded.</returns>
        public async Task<Result> PutAsync
        (
            string endpoint,
            Action<RestRequestBuilder>? configureRequestBuilder = null,
            CancellationToken ct = default
        )
        {
            configureRequestBuilder ??= _ => { };

            var requestBuilder = new RestRequestBuilder(endpoint);
            configureRequestBuilder(requestBuilder);

            requestBuilder.WithMethod(HttpMethod.Put);

            foreach (var customization in _customizations)
            {
                customization.RequestCustomizer(requestBuilder);
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("Discord");
                using var request = requestBuilder.Build();
                using var response = await httpClient.SendAsync
                (
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    ct
                );

                return await UnpackResponseAsync(response, ct);
            }
            catch (Exception e)
            {
                return e;
            }
        }

        /// <summary>
        /// Unpacks a response from the Discord API, attempting to either a plain success or a parsed
        /// error.
        /// </summary>
        /// <param name="response">The response to unpack.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A retrieval result which may or may not have succeeded.</returns>
        private async Task<Result> UnpackResponseAsync
        (
            HttpResponseMessage response,
            CancellationToken ct = default
        )
        {
            if (response.IsSuccessStatusCode)
            {
                return Result.FromSuccess();
            }

            // See if we have a JSON error to get some more details from
            if (response.Content.Headers.ContentLength is <= 0)
            {
                return new HttpResultError(response.StatusCode, response.ReasonPhrase);
            }

            try
            {
                var jsonError = await JsonSerializer.DeserializeAsync<RestError>
                (
                    await response.Content.ReadAsStreamAsync(),
                    _serializerOptions,
                    ct
                );

                if (jsonError is null)
                {
                    return new HttpResultError(response.StatusCode, response.ReasonPhrase);
                }

                return new DiscordRestResultError(jsonError);
            }
            catch
            {
                return new HttpResultError(response.StatusCode, response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Unpacks a response from the Discord API, attempting to either get the requested entity type or a parsed
        /// error.
        /// </summary>
        /// <param name="response">The response to unpack.</param>
        /// <param name="allowNullReturn">Whether to allow null return values inside the creation result.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <typeparam name="TEntity">The entity type to unpack.</typeparam>
        /// <returns>A retrieval result which may or may not have succeeded.</returns>
        private async Task<Result<TEntity>> UnpackResponseAsync<TEntity>
        (
            HttpResponseMessage response,
            bool allowNullReturn = false,
            CancellationToken ct = default
        )
        {
            if (response.IsSuccessStatusCode)
            {
                if (response.Content.Headers.ContentLength == 0)
                {
                    if (!allowNullReturn)
                    {
                        throw new InvalidOperationException("Response content null, but null returns not allowed.");
                    }

                    // Null is okay as a default here, since TEntity might be TEntity?
                    return Result<TEntity>.FromSuccess(default!);
                }

                var entity = await JsonSerializer.DeserializeAsync<TEntity>
                (
                    await response.Content.ReadAsStreamAsync(),
                    _serializerOptions,
                    ct
                );

                if (entity is not null)
                {
                    return Result<TEntity>.FromSuccess(entity);
                }

                if (!allowNullReturn)
                {
                    throw new InvalidOperationException("Response content null, but null returns not allowed.");
                }

                // Null is okay as a default here, since TEntity might be TEntity?
                return Result<TEntity>.FromSuccess(default!);
            }

            // See if we have a JSON error to get some more details from
            if (response.Content.Headers.ContentLength is not > 0)
            {
                return new HttpResultError(response.StatusCode, response.ReasonPhrase);
            }

            try
            {
                var jsonError = await JsonSerializer.DeserializeAsync<RestError>
                (
                    await response.Content.ReadAsStreamAsync(),
                    _serializerOptions,
                    ct
                );

                if (jsonError is null)
                {
                    return new HttpResultError(response.StatusCode, response.ReasonPhrase);
                }

                return new DiscordRestResultError(jsonError);
            }
            catch
            {
                return new HttpResultError(response.StatusCode, response.ReasonPhrase);
            }
        }
    }
}
