using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

public struct Player
{
    public int Win;
    public int Loss;
    public ulong UID;
    public int Elo;
    public int WinFactor;
    public int LossFactor;

    public Player(ulong uid)
    {
        UID = uid;
        Win = 0;
        Loss = 0;
        Elo = 200;
        WinFactor = 70;
        LossFactor = 30;
    }
}


public class PlayerManager
{
    private const string FilePath = "players.json"; // Path to store player data
    private List<Player> _players;

    public PlayerManager()
    {
        _players = LoadPlayers();
    }

    // Load player data from the file
    public List<Player> LoadPlayers()
    {
        if (File.Exists(FilePath))
        {
            var jsonData = File.ReadAllText(FilePath);
            return JsonConvert.DeserializeObject<List<Player>>(jsonData) ?? new List<Player>();
        }
        else
        {
            return new List<Player>();
        }
    }

    // Save player data to the file
    public void SavePlayers()
    {
        var jsonData = JsonConvert.SerializeObject(_players, Formatting.Indented);
        File.WriteAllText(FilePath, jsonData);
    }

    // Add or update a player
    public void AddOrUpdatePlayer(ulong uid)
    {
        var player = _players.FirstOrDefault(p => p.UID == uid);
        if (player.Equals(default(Player)))
        {
            _players.Add(new Player(uid)); // Add new player
        }

        SavePlayers(); // Persist data
    }

    // Get a player by UID
    public Player GetPlayer(ulong uid)
    {
        return _players.FirstOrDefault(p => p.UID == uid);
    }

    // Update wins or losses for a player
    public int UpdatePlayerStats(ulong uid, double teamE, bool win)
    {
        var player = GetPlayer(uid);
        if (player.Equals(default(Player)))
        {
            AddOrUpdatePlayer(uid);
            player = GetPlayer(uid); // After adding, get the updated player
        }

        int points;

        if (win)
        {
            points = (int)(player.WinFactor * (1 - teamE));
            player.Elo += points;

            if (player.WinFactor < 70) player.WinFactor += 10;
            if (player.LossFactor > 30) player.LossFactor -= 10;

            player.Win++;
        }
        else
        {
            points = (int)(player.LossFactor * (0 - teamE));
            player.Elo += points;

            if (player.WinFactor > 30) player.WinFactor -= 10;
            if (player.LossFactor < 70) player.LossFactor += 10;

            player.Loss++;
        }

        // Update the player in the list
        _players[_players.FindIndex(p => p.UID == uid)] = player;
        SavePlayers(); // Persist updated stats

        return points;
    }

    public int GetPlayerElo(ulong uid)
    {
        var player = GetPlayer(uid);
        if (player.Equals(default(Player)))
        {
            AddOrUpdatePlayer(uid);
            player = GetPlayer(uid); // After adding, get the updated player
        }

        return player.Elo;
    }

    // Get all players for display or other purposes
    public List<Player> GetAllPlayers()
    {
        return _players;
    }
}
