using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using UnityEngine;

public class SpotifyController : MonoBehaviour 
{
    public static SpotifyController Instance { get; private set; }
    private SpotifyClient _spotify;
    public string userId="";
    public Boolean AutoStart = false;

    public event EventHandler OnNext;
    public event EventHandler OnPrevious;

    private async void Awake()
    {
        if (Instance == null) if (AutoStart) await Init();          
        else if (Instance != this) Destroy(this.gameObject);
        DontDestroyOnLoad(this);
    }

    public async Task Init()
    {
        var auth = await Auth.CreateAsync(userId);
        _spotify = new SpotifyClient(auth.AccessToken);
    }

    public async Task<bool> GetPlayPauseState()
    {
        CurrentlyPlayingContext currentlyPlayingContext = await _spotify.Player.GetCurrentPlayback();
        if (currentlyPlayingContext != null) 
            return currentlyPlayingContext.IsPlaying;
        return false;
    }

    public async Task Pause()
    {
        await _spotify.Player.PausePlayback();
    }

    public async Task Play()
    {
        await _spotify.Player.ResumePlayback();
    }

    public async Task TogglePlayPause()
    {
        if (await GetPlayPauseState()) 
            await Pause();
        else 
            await Play();
    }

    private async Task Seek(int seconds)
    {
        long ms = seconds * 1000;

        var currentPlaybackInfo = await _spotify.Player.GetCurrentPlayback();
        await _spotify.Player.SeekTo(new PlayerSeekToRequest(currentPlaybackInfo.ProgressMs + ms));
    }

    public async Task FastForward(int seconds)
    {
        await Seek(seconds);
    }

    public async Task Rewind(int seconds)
    {
        await Seek(-seconds);
    }

    public async Task Next()
    {
        await _spotify.Player.SkipNext();
        OnNext?.Invoke(this, EventArgs.Empty);
    }

    public async Task Previous()
    {
        await _spotify.Player.SkipPrevious();
        OnPrevious?.Invoke(this, EventArgs.Empty);
    }

    public async Task<List<Song>> GetPlaylist()
    {
        var userPlaylists = await _spotify.Playlists.CurrentUsers();
        if (userPlaylists.Items.Count == 0)
        {
            Debug.Log("No playlists found for the current user.");
            return null;
        }

        var playlistItems = await _spotify.Playlists.GetItems(userPlaylists.Items[0].Id);
        var songs = new List<Song>();

        foreach (var item in playlistItems.Items)
        {
            if (item.Track is FullTrack track)
            {
                songs.Add(new Song()
                {
                    ID = track.Id,
                    Title = track.Name,
                    AlbumID = track.Album.Id,
                    AlbumName = track.Album.Name,
                    ArtistID = track.Artists[0].Id,
                    ArtistName = track.Artists[0].Name,
                    Duration = track.DurationMs,
                    IsPlayable = track.IsPlayable,
                    IsLocal = track.IsLocal,
                    Tempo = await GetTempo(track.Id),
                    TempoConfidence = await GetTempoConfidence(track.Id)
                });
            }
        }
        return songs;
    }

    public async Task<Song> GetCurrentlyPlayingSong()
    {
        var currentlyPlaying = await _spotify.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest());

        if (currentlyPlaying?.Item is FullTrack track)
        {
            return new Song()
            {
                ID = track.Id,
                Title = track.Name,
                Duration = track.DurationMs,
                ArtistID = track.Artists[0].Id,
                ArtistName = track.Artists[0].Name,
                AlbumID = track.Album.Id,
                AlbumName = track.Album.Name,
                IsPlayable = track.IsPlayable,
                IsLocal = track.IsLocal,
                Tempo = await GetTempo(track.Id),
                TempoConfidence = await GetTempoConfidence(track.Id)
            };
        }

        Debug.Log("No track is currently playing.");
        return null;
    }

    public async Task<int> GetCurrentSongProgressMillis()
    {
        CurrentlyPlayingContext context = await _spotify.Player.GetCurrentPlayback();
        return context.ProgressMs;
    }

    public async Task<bool> PlaySearchedSong(string songName)
    {
        var searchResponse = await _spotify.Search.Item(new SearchRequest(SearchRequest.Types.Track, songName));
        await _spotify.Player.AddToQueue(new PlayerAddToQueueRequest(searchResponse.Tracks.Items[0].Uri));
        await _spotify.Player.SkipNext();

        return GetCurrentlyPlayingSong().Result.ID == searchResponse.Tracks.Items[0].Id;
    }

    public async Task<float> GetTempo(string songId)
    {
        var trackData = await _spotify.Tracks.GetAudioAnalysis(songId);
        return trackData.Track.Tempo;
    }
    
    public async Task<float> GetTempoConfidence(string songId)
    {
        var trackData = await _spotify.Tracks.GetAudioAnalysis(songId);
        return trackData.Track.TempConfidence;
    }
}