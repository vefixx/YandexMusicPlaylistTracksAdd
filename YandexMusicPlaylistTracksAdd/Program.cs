using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Yandex.Music.Api;
using Yandex.Music.Api.Common.Debug;
using Yandex.Music.Api.Common.Debug.Writer;
using Yandex.Music.Api.Extensions.API;
using Yandex.Music.Api.Models.Track;
using Yandex.Music.Client;

namespace YandexMusicPlaylistTracksAdd;

class Program
{
    private static readonly Regex TrackUrlParseRegex = new Regex(@"/track/(\d+)", RegexOptions.Compiled);
    private static readonly Regex PlaylistUrlParseRegex = new Regex(@"/users/([^/]+)/playlists/(\d+)", RegexOptions.Compiled);
    
    private static readonly Config Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"))!;

    private static readonly YandexMusicClient Client = new YandexMusicClient();
    
    static void Main(string[] args)
    {
        var isAuthorized = Client.Authorize(Config.Token);

        if (!isAuthorized)
        {
            Console.WriteLine($"Authorize failed");
            return;
        }

        var loginInfo = Client.GetLoginInfo();
        Console.WriteLine($"user: {loginInfo.Login}");

        Console.WriteLine("Добавление треков из tracks.txt ...");
        AddToTargetPlaylist();

        Console.WriteLine("Добавление треков из playlists.txt");
        AddTracksFromOtherPlaylistToTarget();
    }
    
    /// <summary>
    /// Добавление треков по ссылке из файла tracks.txt в указанный плейлист.
    /// </summary>
    private static void AddToTargetPlaylist()
    {
        var playlist = Client.GetPlaylist(Config.TargetPlaylistUid);
        var trackIdsInPlaylist = playlist.Tracks.Select(t => t.Id).ToHashSet();

        var trackUrls = File.ReadAllLines("tracks.txt");

        var tracks = new List<YTrack>(trackUrls.Length);
        foreach (var trackUrl in trackUrls)
        {
            Match match = TrackUrlParseRegex.Match(trackUrl);

            if (!match.Success)
            {
                Console.WriteLine($"Ошибка обработки ссылки: {trackUrl}");
                continue;
            }

            var trackId = match.Groups[1].Value;
            
            if (trackIdsInPlaylist.Contains(trackId))
            {
                Console.WriteLine($"Пропуск трека {trackId} (уже добавлен)");
                continue;
            }
            
            var track = Client.GetTrack(trackId);
            Console.WriteLine($"Добавление \"{track.Title}\" (id: {track.Id})");
            tracks.Add(track);
        }
        
        playlist.InsertTracks(tracks.ToArray());
        Console.WriteLine($"Треки успешно добавлены в плейлист");
    }
    
    /// <summary>
    /// Добавление треков из одного плейлиста в указанный конечный
    /// </summary>
    private static void AddTracksFromOtherPlaylistToTarget()
    {
        var targetPlaylist = Client.GetPlaylist(Config.TargetPlaylistUid);
        var trackIdsInTargetPlaylist = targetPlaylist.Tracks.Select(t => t.Id).ToHashSet();
        
        var lines = File.ReadAllLines("playlists.txt");
        
        var tracks = new List<YTrack>();
        
        foreach (var line in lines)
        {
            var split = line.Split(";");
            var url = split[0];
            var count = int.Parse(split[1]);

            if (count < 1)
            {
                Console.WriteLine($"Некорректное значение количества ({count}) в плейлисте {url}");
                continue;
            }

            var match = PlaylistUrlParseRegex.Match(url);
            if (!match.Success)
            {
                Console.WriteLine($"Ошибка обработки ссылки {url}");
                continue;
            }

            string user = match.Groups[1].Value;
            string playlistId = match.Groups[2].Value;

            var otherPlaylist = Client.GetPlaylist(user, playlistId);
            if (otherPlaylist is null)
            {
                Console.WriteLine($"Не удалось получить плейлист, user: {user}, id={playlistId}");
                continue;
            }

            int countToAdd = Math.Min(count, otherPlaylist.TrackCount);
            Console.WriteLine($"Добавление {countToAdd} из плейлиста {otherPlaylist.Title}");
            
            for (int i = 0; i < countToAdd; i++)
            {
                var trackContainer = otherPlaylist.Tracks[i];
                var trackId = trackContainer.Id;

                if (trackIdsInTargetPlaylist.Contains(trackId))
                {
                    Console.WriteLine($"Пропуск трека {trackId} (уже добавлен)");
                    continue;
                }
                
                var track = Client.GetTrack(trackId);
                Console.WriteLine($"Добавление \"{track.Title}\" (id: {track.Id})");
                tracks.Add(track);
            }
        }
        
        targetPlaylist.InsertTracks(tracks.ToArray());
        Console.WriteLine($"Треки успешно добавлены в плейлист");
    }
}