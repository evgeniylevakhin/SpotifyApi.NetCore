using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SpotifyApi.NetCore.Http;

namespace SpotifyApi.NetCore
{
    public class UserAccountsService : AccountsService, IUserAccountsService
    {
        private const string AccountsAuthorizeUrl = "https://accounts.spotify.com/authorize";

        private readonly IRefreshTokenProvider _refreshTokenProvider;

        #region constructors

        public UserAccountsService(
            HttpClient httpClient,
            IConfiguration configuration,
            IRefreshTokenProvider refreshTokenProvider,
            IBearerTokenStore bearerTokenStore
            ) : base(httpClient, configuration, bearerTokenStore)
        {
            _refreshTokenProvider = refreshTokenProvider;
        }

        public UserAccountsService(
            HttpClient httpClient,
            IConfiguration configuration,
            IRefreshTokenProvider refreshTokenProvider
            ) : this(httpClient, configuration, refreshTokenProvider, null) { }

        public UserAccountsService(
            IRefreshTokenProvider refreshTokenProvider
            ) : this(new HttpClient(), null, refreshTokenProvider, null) { }

        public UserAccountsService(
            HttpClient httpClient,
            IConfiguration configuration
            ) : this(httpClient, configuration, null, null) { }

        public UserAccountsService(
            IConfiguration configuration
            ) : this(new HttpClient(), configuration, null, null) { }

        #endregion

        [Obsolete("This method will be removed in next major version")]
        public async Task<BearerAccessToken> GetUserAccessToken(string userHash)
        {
            if (_refreshTokenProvider == null)
                throw new InvalidOperationException("A Refresh Token Provider has not been set. See the constructors of UserAccountsService for options.");

            // get the refresh token for this user
            string refreshToken = await _refreshTokenProvider.GetRefreshToken(userHash);
            if (string.IsNullOrEmpty(refreshToken))
                throw new UnauthorizedAccessException($"No refresh token found for user \"{userHash}\"");

            return await GetAccessToken(userHash, 
                $"grant_type=refresh_token&refresh_token={refreshToken}&redirect_uri={_config["SpotifyAuthRedirectUri"]}");
        }

        public async Task<BearerAccessToken> RefreshUserAccessToken(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken)) throw new ArgumentNullException(nameof(refreshToken));

            return await RefreshAccessToken(
                $"grant_type=refresh_token&refresh_token={refreshToken}&redirect_uri={_config["SpotifyAuthRedirectUri"]}");
        }

        public string AuthorizeUrl(string state, string[] scopes)
        {
            return AuthorizeUrl(state, scopes, _config["SpotifyApiClientId"], _config["SpotifyAuthRedirectUri"]);
        }

        public static string AuthorizeUrl(string state, string[] scopes, string spotifyApiClientId, string spotifyAuthRedirectUri)
        {
            if (string.IsNullOrEmpty(spotifyApiClientId)) throw new ArgumentNullException(nameof(spotifyApiClientId));
            if (string.IsNullOrEmpty(spotifyAuthRedirectUri)) throw new ArgumentNullException(nameof(spotifyAuthRedirectUri));

            string scope = scopes == null || scopes.Length == 0 ? "" : string.Join("%20", scopes);
            return $"{AccountsAuthorizeUrl}/?client_id={spotifyApiClientId}&response_type=code&redirect_uri={spotifyAuthRedirectUri}&scope={scope}&state={state}";
        }

        public async Task<BearerAccessRefreshToken> RequestAccessRefreshToken(string userHash, string code)
        {
            var token = await RequestAccessRefreshToken(code);
            await _bearerTokenStore.InsertOrReplace(userHash, token);
            return token;
        }

        public async Task<BearerAccessRefreshToken> RequestAccessRefreshToken(string code)
        {
            var now = DateTime.UtcNow;
            // POST the code to get the tokens
            var token = await GetAuthorizationTokens(code);
            // set absolute expiry
            token.SetExpires(now);
            token.EnforceInvariants();
            return token;
        }

        protected internal virtual async Task<BearerAccessRefreshToken> GetAuthorizationTokens(string code)
        {
            var result = await _http.Post(TokenUrl,
                $"grant_type=authorization_code&code={code}&redirect_uri={_config["SpotifyAuthRedirectUri"]}",
                GetHeader(_config));
            return JsonConvert.DeserializeObject<BearerAccessRefreshToken>(result);
        }

        private void ValidateConfig()
        {
            if (string.IsNullOrEmpty(_config["SpotifyAuthRedirectUri"]))
                throw new ArgumentNullException("SpotifyAuthRedirectUri", "Expecting configuration value for `SpotifyAuthRedirectUri`");
        }

    }
}
