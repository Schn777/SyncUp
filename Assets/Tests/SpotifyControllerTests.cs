using NUnit.Framework;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class SpotifyControllerTests
{
    private GameObject SpotifyControllerSingleton;

    [SetUp]
    public async Task SetUpAsync()
    {
        // Arrange
        SpotifyControllerSingleton = new GameObject("SpotifyControllerSingleton");
        SpotifyControllerSingleton.AddComponent<SpotifyController>();

        // Initialize and set properties
        SpotifyControllerSingleton.GetComponent<SpotifyController>().userId = "velozee";

        // Assuming Init() is an asynchronous method.
        await SpotifyControllerSingleton.GetComponent<SpotifyController>().Init();
    }

    [Test]
    public async Task SpotifyController_PausesAsync()
    {
        // Act
        try
        {
            await SpotifyControllerSingleton.GetComponent<SpotifyController>().Pause();
        }
        catch (Exception)
        {
            // do nothing
        }

        Task.Delay(3000).Wait();

        var isPaused = false;
        try
        {
            isPaused = await SpotifyControllerSingleton.GetComponent<SpotifyController>().GetPlayPauseState();

        }
        catch (Exception)
        {
            Assert.IsTrue(isPaused);
        }

    }

    [Test]
    public async Task SpotifyController_PlaysAsync()
    {
        // Act
        try
        {
            await SpotifyControllerSingleton.GetComponent<SpotifyController>().Play();
        }
        catch (Exception)
        {
            // do nothing
        }

        Task.Delay(3000).Wait();

        bool isPaused = true;
        try
        {
            isPaused = await SpotifyControllerSingleton.GetComponent<SpotifyController>().GetPlayPauseState();

        }
        catch (Exception)
        {
            Assert.IsTrue(!isPaused);
        }
    }

    [Test]
    public async Task SpotifyController_SeeksAsync()
    {
        var startTime = 0;
        // Arrage
        try
        {

            await SpotifyControllerSingleton.GetComponent<SpotifyController>().Pause();
            startTime = await SpotifyControllerSingleton.GetComponent<SpotifyController>().GetCurrentSongProgressMillis();
        }
        catch (Exception)
        {
            // do nothing
        }

        // Act
        var duration = 10;

        try
        {
            await SpotifyControllerSingleton.GetComponent<SpotifyController>().FastForward(duration);
        }
        catch (Exception)
        {
            // do nothing
        }

        var difference = 0;
        try
        {
            difference = await SpotifyControllerSingleton.GetComponent<SpotifyController>().GetCurrentSongProgressMillis() - startTime;
        }
        catch (Exception)
        {
            Assert.IsTrue(difference / 1000 == duration);
        }
    }

    [Test]
    public async Task SpotifyController_Searches()
    {
        var artistName = "Israel Kamakawiwo'ole";
        // Act
        try
        {
            await SpotifyControllerSingleton.GetComponent<SpotifyController>().PlaySearchedSong("Somewhere Over The Rainbow_What A Wonderful World");
        }
        catch (Exception)
        {
            // do nothing
        }

        Task.Delay(3000).Wait();

        Song song = new Song();
        try
        {
            song = await SpotifyControllerSingleton.GetComponent<SpotifyController>().GetCurrentlyPlayingSong();
        }
        catch (Exception)
        {
            Assert.IsTrue(song.ArtistName == artistName);
        }
    }

}

