using System;
using System.Net.Http;
using System.Threading.Tasks;
using SpotifyApi.NetCore.Http;

namespace SpotifyApi.NetCore
{
    public class BrowseApi : SpotifyWebApi, IBrowseApi
    {
        public BrowseApi(HttpClient httpClient, IAuthorizationApi authorizationApi) : base(httpClient, authorizationApi)
        {
        }

        public async Task<dynamic> GetRecommendation(string[] seedArtists, string[] seedGenres, string[] seedTracks)
        {
            throw new NotImplementedException();
        }

        public async Task<dynamic> GetRecommendation(string[] seedArtists, string[] seedGenres, string[] seedTracks, int limit)
        {
            throw new NotImplementedException();
        }
    }
}