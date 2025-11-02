using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public struct Bet
{
    public ulong UID; // Discord user ID
    public bool Active; // True if the user has placed a double-or-nothing bet

    public Bet(ulong uid)
    {
        UID = uid;
        Active = true;
    }
}

public class BetManager
{
    private const string FilePath = "bets.json"; // Path to store bet data
    private List<Bet> _bets;

    public BetManager()
    {
        _bets = LoadBets();
    }

    // Load bets from JSON
    public List<Bet> LoadBets()
    {
        if (File.Exists(FilePath))
        {
            var jsonData = File.ReadAllText(FilePath);
            return JsonConvert.DeserializeObject<List<Bet>>(jsonData) ?? new List<Bet>();
        }
        else
        {
            return new List<Bet>();
        }
    }

    // Save bets to JSON
    public void SaveBets()
    {
        var jsonData = JsonConvert.SerializeObject(_bets, Formatting.Indented);
        File.WriteAllText(FilePath, jsonData);
    }

    // Add a bet for a player
    public void PlaceBet(ulong uid)
    {
        if (!_bets.Any(b => b.UID == uid))
        {
            _bets.Add(new Bet(uid));
            SaveBets();
        }
    }

    // Check if a player has an active bet
    public bool HasBet(ulong uid)
    {
        return _bets.Any(b => b.UID == uid && b.Active);
    }

    // Apply double or nothing multiplier to a point gain/loss
    public int ApplyBetMultiplier(ulong uid, int points)
    {
        if (HasBet(uid))
        {
            points *= 2; // Double the points
        }

        return points;
    }

    // Clear all bets after a game
    public void ResetBets()
    {
        _bets.Clear();
        SaveBets();
    }

    // Get all active bets (optional, for debugging or display)
    public List<Bet> GetAllBets()
    {
        return _bets;
    }
}
