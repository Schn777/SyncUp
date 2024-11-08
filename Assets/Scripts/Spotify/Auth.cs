using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using UnityEngine;

public class TokenData
{
    public string UserId { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime Expiration { get; set; }
}

public class Auth
{
    private string _accessToken;
    public string AccessToken => _accessToken;

    private string _refreshToken;
    private DateTime _expiration;

    private readonly ConfigManager _config;
    private static EmbedIOAuthServer _server;

    public Auth(int userId)
    {
        _config = new ConfigManager(Path.Combine(Application.dataPath, "Scripts/Spotify/Data/spotify.config"));
        Init(userId.ToString());
    }

    private async void Init(string userId)
    {
        await GetOrRefreshToken(userId);
    }

    /// <summary>
    /// Starts the authorization process to obtain a new authorization code for a specific user.
    /// This method opens a browser for the user to log in and authorize the app.
    /// </summary>
    /// <param name="userId">The ID of the user requesting authorization.</param>
    private async Task GetNewAuthorizationCode(string userId)
    {
        _server = new EmbedIOAuthServer(new Uri(
            _config.GetString("SERVER_REDIRECT_URI")),
            _config.GetInt("SERVER_PORT")
        );

        await _server.Start();

        _server.AuthorizationCodeReceived += async (sender, response) => await OnAuthorizationCodeReceived(sender, response, userId);
        _server.ErrorReceived += async (sender, error, state) => await OnErrorReceived(sender, error, state);

        var request = new LoginRequest(_server.BaseUri, _config.GetString("CLIENT_ID"), LoginRequest.ResponseType.Code)
        {
            Scope = new List<string>
                {
                    Scopes.UgcImageUpload,
                    Scopes.UserReadPlaybackState,
                    Scopes.UserModifyPlaybackState,
                    Scopes.UserReadCurrentlyPlaying,
                    Scopes.Streaming,
                    Scopes.AppRemoteControl,
                    Scopes.UserReadEmail,
                    Scopes.UserReadPrivate,
                    Scopes.PlaylistReadCollaborative,
                    Scopes.PlaylistModifyPublic,
                    Scopes.PlaylistReadPrivate,
                    Scopes.PlaylistModifyPrivate,
                    Scopes.UserLibraryModify,
                    Scopes.UserLibraryRead,
                    Scopes.UserTopRead,
                    Scopes.UserReadPlaybackPosition,
                    Scopes.UserReadRecentlyPlayed,
                    Scopes.UserFollowRead,
                    Scopes.UserFollowModify
                }
        };

        var uri = request.ToUri();
        try
        {
            BrowserUtil.Open(uri);
        }
        catch (Exception ex)
        {
            Debug.Log($"Unable to open URL, manually open: {uri}, Exception: {ex}");
        }
    }

    /// <summary>
    /// Handles any errors that occur during the authorization process.
    /// </summary>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="error">The error message received.</param>
    /// <param name="state">The state of the authorization request.</param>
    private async Task OnErrorReceived(object sender, string error, string state)
    {
        Debug.LogError($"Error received: {error}");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Handles the event when the authorization code is received from Spotify.
    /// This method exchanges the code for an access token and refresh token.
    /// </summary>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="response">The response containing the authorization code.</param>
    /// <param name="userId">The ID of the user for whom the code is received.</param>
    private async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response, string userId)
    {
        var tokenResponse = await new OAuthClient(SpotifyClientConfig.CreateDefault()).RequestToken(
            new AuthorizationCodeTokenRequest(
                _config.GetString("CLIENT_ID"),
                _config.GetString("CLIENT_SECRET"),
                response.Code,
                new Uri(_config.GetString("SERVER_REDIRECT_URI"))
            )
        );

        _accessToken = tokenResponse.AccessToken;
        _refreshToken = tokenResponse.RefreshToken;
        _expiration = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn);

        SaveTokenData(userId, _accessToken, _refreshToken, _expiration);

        await _server.Stop();
    }

    /// <summary>
    /// Retrieves an existing token from storage or refreshes it if expired.
    /// If no valid token is found, the authorization process starts to get a new one.
    /// </summary>
    /// <param name="userId">The ID of the user whose token is being fetched or refreshed.</param>
    private async Task GetOrRefreshToken(string userId)
    {
        var tokenDataList = LoadTokenData();
        var userToken = tokenDataList.Find(u => u.UserId == userId);

        if (userToken != null && !IsTokenExpired(userToken))
        {
            _accessToken = userToken.AccessToken;
            _refreshToken = userToken.RefreshToken;
            _expiration = userToken.Expiration;
        }
        else if (userToken != null && IsTokenExpired(userToken))
        {
            await RefreshToken(userToken);
        }
        else
        {
            await GetNewAuthorizationCode(userId);
        }
    }

    private async Task RefreshToken(TokenData tokenData)
    {
        try
        {
            var refreshRequest = new AuthorizationCodeRefreshRequest(_config.GetString("CLIENT_ID"), _config.GetString("CLIENT_SECRET"), tokenData.RefreshToken);
            var tokenResponse = await new OAuthClient(SpotifyClientConfig.CreateDefault()).RequestToken(refreshRequest);

            _accessToken = tokenResponse.AccessToken;
            _refreshToken = tokenResponse.RefreshToken;
            _expiration = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn);

            SaveTokenData(tokenData.UserId, _accessToken, _refreshToken, _expiration);

            Debug.Log("Token refreshed successfully.");
        }
        catch (APIException e)
        {
            Debug.LogError($"Failed to refresh token: {e.Message}");
            await GetNewAuthorizationCode(tokenData.UserId);
        }
    }

    /// <summary>
    /// Checks whether the stored token for a user has expired.
    /// </summary>
    /// <param name="tokenData">The token data to check.</param>
    /// <returns>True if the token has expired, false otherwise.</returns>
    private bool IsTokenExpired(TokenData tokenData)
    {
        return DateTime.Now >= tokenData.Expiration;
    }

    /// <summary>
    /// Saves the user's token data (access token, refresh token, and expiration time) to persistent storage.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="accessToken">The access token to save.</param>
    /// <param name="refreshToken">The refresh token to save.</param>
    /// <param name="expiration">The expiration time of the access token.</param>
    private void SaveTokenData(string userId, string accessToken, string refreshToken, DateTime expiration)
    {
        var tokenDataList = LoadTokenData();
        var existingUser = tokenDataList.Find(u => u.UserId == userId);

        if (existingUser != null)
        {
            existingUser.AccessToken = accessToken;
            existingUser.RefreshToken = refreshToken;
            existingUser.Expiration = expiration;
        }
        else
        {
            tokenDataList.Add(new TokenData
            {
                UserId = userId,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Expiration = expiration
            });
        }

        string jsonOutput = JsonConvert.SerializeObject(tokenDataList, Formatting.Indented);
        File.WriteAllText(_config.GetString("USERS_PATH"), jsonOutput);
    }

    /// <summary>
    /// Loads the stored token data for all users from persistent storage.
    /// </summary>
    /// <returns>A list of token data for all users.</returns>
    private List<TokenData> LoadTokenData()
    {
        if (!File.Exists(_config.GetString("USERS_PATH")))
            return new List<TokenData>();

        var json = File.ReadAllText(_config.GetString("USERS_PATH"));

        try
        {
            return JsonConvert.DeserializeObject<List<TokenData>>(json);
        }
        catch (JsonSerializationException)
        {
            Debug.LogError("JSON format is incorrect. Please check the users.json file (" + _config.GetString("USERS_PATH") + ").");
            return new List<TokenData>();
        }
    }
}