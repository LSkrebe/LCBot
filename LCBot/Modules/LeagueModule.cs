using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class LeagueModule : ModuleBase<SocketCommandContext>
{
    // List to store participant user IDs (UIDs)
    public static List<ulong> participants = new List<ulong>();

    // Command to list all current players
    [Command("p")]
    public async Task PlayersAsync()
    {
        if (participants.Count == 0)
        {
            await ReplyAsync("There are no participants in the list.");
            return;
        }

        string reply = "Players for current custom game:\n";

        for(int i = 0; i < participants.Count; i++)
        {
            var username = Context.Guild.GetUser(participants[i]).DisplayName;
            reply += $"{i+1}. {username}\n";
        }

        await ReplyAsync($"```{reply}```");
    }

    // Command to add all users from the current voice channel to the participants list
    [Command("pfc")]
    public async Task AddFromVoiceChannelAsync()
    {
        var voiceChannel = ((SocketGuildUser)Context.User).VoiceChannel;
        if (voiceChannel == null)
        {
            await ReplyAsync("You need to be in a voice channel to add users.");
            return;
        }

        // Add users from the voice channel to the participants list
        foreach (var user in voiceChannel.ConnectedUsers)
        {
            if (!participants.Contains(user.Id))
            {
                participants.Add(user.Id);
            }
        }

        await PlayersAsync();
    }

    // Command to add a specific user to the participants list
    [Command("add")]
    public async Task AddUserAsync(SocketUser user)
    {
        if (participants.Contains(user.Id))
        {
            await ReplyAsync($"{user.Username} is already in the participant list.");
        }
        else
        {
            participants.Add(user.Id);

            await PlayersAsync();
        }
    }

    // Command to remove a specific user from the participants list
    [Command("rm")]
    public async Task RemoveUserAsync(SocketUser user)
    {
        if (participants.Contains(user.Id))
        {
            participants.Remove(user.Id);

            await PlayersAsync();
        }
        else
        {
            await ReplyAsync($"{user.Username} is not in the participant list.");
        }
    }

    // Command to remove a specific user from the participants list
    [Command("reset")]
    public async Task RemoveAllAsync()
    {
        participants.Clear();
        team1.Clear();
        team2.Clear();

        await ReplyAsync("Reset successfully");
    }

    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------

    // List to store team 1 UID
    public static List<ulong> team1 = new List<ulong>();

    // List to store team 2 UID
    public static List<ulong> team2 = new List<ulong>();

    // Storing in JSON
    private static PlayerManager playerManager = new PlayerManager();

    //--------------------------------------------------------------------------------------------

    //pick teams
    [Command("pick")]
    public async Task PickTeamsAsync()
    {
        if (participants.Count < 2)
        {
            await ReplyAsync("There are not enough participants in the list.");
            return;
        }

        team1.Clear();
        team2.Clear();

        var builder = new ComponentBuilder();
        for (int i = 0; i < participants.Count; i++)
        {
            builder.WithButton($"{Context.Guild.GetUser(participants[i]).DisplayName}", $"pick_{i}", ButtonStyle.Primary, row: (i - 5) / 5);
        }

        await ReplyAsync($"```{ListPickings(Context.Guild)}(Team 1 first pick)```", components: builder.Build());
    }

    public async Task PickButtonAsync(SocketMessageComponent component)
    {
        // Acknowledge the interaction immediately
        await component.DeferAsync();

        var customId = component.Data.CustomId;
        int index = int.Parse(customId.Split('_')[1]);

        var guild = (component.Message.Channel as SocketGuildChannel)?.Guild;
        if (guild == null) return;

        if (participants.Count <= index) return;

        var player = participants[index];

        if(team1.Count == team2.Count) team1.Add(player);
        else team2.Add(player);

        var builder = new ComponentBuilder();
        for (int i = 0; i < participants.Count; i++)
        {
            builder.WithButton($"{guild.GetUser(participants[i]).DisplayName}", $"pick_{i}", ButtonStyle.Primary, disabled: team1.Contains(participants[i]) || team2.Contains(participants[i]), row: (i - 5) / 5);
        }

        if(team1.Count + team2.Count < participants.Count)
        {
            await component.Message.ModifyAsync(msg =>
            {
                msg.Content = $"```{ListPickings(guild)}```";
                msg.Components = builder.Build();
            });
        }
        else if (team1.Count + team2.Count == participants.Count)
        {
            await component.Message.ModifyAsync(msg =>
            {
                msg.Content = ListTeams(guild);
                msg.Components = null;
            });
        }
    }

    private string ListPickings(SocketGuild guild)
    {
        string t1 = "TEAM 1: \n";
        string t2 = "TEAM 2: \n";

        for (int i = 0; i < participants.Count; i++)
        {
            var player = participants[i];
            var username = guild.GetUser(participants[i]).DisplayName;

            if (team1.Contains(player))
                t1 += $"{username}\n";
            else if (team2.Contains(player))
                t2 += $"{username}\n";
        }

        return $"{t1}\n{t2}\n";
    }

    //--------------------------------------------------------------------------------------------

    [Command("ts")]
    public async Task RandomizeTeamsAsync()
    {
        if (participants.Count < 2)
        {
            await ReplyAsync("Not enough participants.");
            return;
        }

        // Shuffle the participants list randomly
        Random rand = new Random();
        var shuffledParticipants = participants.OrderBy(x => rand.Next()).ToList();

        // Split the shuffled list into two teams
        int midPoint = shuffledParticipants.Count / 2;
        team1 = shuffledParticipants.Take(midPoint).ToList();
        team2 = shuffledParticipants.Skip(midPoint).ToList();

        await TeamsAsync();
    }

    //--------------------------------------------------------------------------------------------

    [Command("t")]
    public async Task TeamsAsync()
    {
        if (team1.Count == 0 || team2.Count == 0)
        {
            await ReplyAsync("Participants haven't joined into teams.");
            return;
        }

        await ReplyAsync(ListTeams(Context.Guild));
    }

    private string ListTeams(SocketGuild guild)
    {
        int team1w = 0, team1l = 0;
        int team2w = 0, team2l = 0;

        string team1players = "";

        for (int i = 0; i < team1.Count; i++)
        {
            var username = guild.GetUser(team1[i]).DisplayName;
            if (username != null) team1players += $"{i + 1}. {username}\n";

            var cur1 = playerManager.GetPlayer(team1[i]);
            team1w += cur1.Win;
            team1l += cur1.Loss;
        }

        string team2players = "";

        for (int i = 0; i < team2.Count; i++)
        {
            var username = guild.GetUser(team2[i]).DisplayName;
            if (username != null) team2players += $"{i + 1}. {username}\n";

            var cur2 = playerManager.GetPlayer(team2[i]);
            team2w += cur2.Win;
            team2l += cur2.Loss;
        }

        var wr1 = CalculateWinRate(team1w, team1l);
        var wr2 = CalculateWinRate(team2w, team2l);

        return $"```Team 1: ({wr1:F2}% WR)\n{team1players}\nTeam 2: ({wr2:F2}% WR)\n{team2players}```";
    }

    //--------------------------------------------------------------------------------------------

    [Command("winner")]
    public async Task TeamDecide(int team)
    {
        // Ensure a valid team argument (team 1 or team 2)
        if (team != 1 && team != 2)
        {
            await ReplyAsync("Please provide a valid team number (1 or 2).");
            return;
        }
        else if (team1.Count == 0 || team2.Count == 0)
        {
            await ReplyAsync("No teams have formed yet.");
            return;
        }

        int team1elo = 0;
        int team2elo = 0;
        foreach (var player in team1) team1elo += playerManager.GetPlayerElo(player);
        foreach (var player in team2) team2elo += playerManager.GetPlayerElo(player);

        team1elo /= team1.Count;
        team2elo /= team2.Count;

        double team1E = 1 / (1 + Math.Pow(10, ((team2elo - team1elo) / 400.0)));
        double team2E = 1 / (1 + Math.Pow(10, ((team1elo - team2elo) / 400.0)));

        Dictionary<string, int> gains = new Dictionary<string, int>();

        // Add or update players when winner is determined
        if (team == 1)
        {
            UpdateTeam(team1, team1E, true);
            UpdateTeam(team2, team2E, false);
        }
        else if (team == 2)
        {
            UpdateTeam(team1, team1E, false);
            UpdateTeam(team2, team2E, true);
        }

        var sortedGains = gains.OrderByDescending(pair => pair.Value).ToList();
        string ans = "Game results\n\n";
        for(int i=0; i<sortedGains.Count; i++)
        {
            var name = sortedGains[i].Key;
            var points = sortedGains[i].Value;
            var sign = points < 0 ? "" : "+";

            ans += $"{i + 1, -3} {name, -20} {sign}{points}\n";
        }

        await ReplyAsync($"```{ans}```");

        void UpdateTeam(List<ulong> team, double teamE, bool win)
        {
            for (int i = 0; i < team.Count; i++)
            {
                var points = playerManager.UpdatePlayerStats(team[i], teamE, win);
                var name = Context.Guild.GetUser(team[i]).DisplayName;

                gains[name] = points;
            }
        }
    }

    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------

    [Command("l")]
    [Alias("leaderboard")]
    public async Task Leaderboard()
    {
        var players = playerManager.GetAllPlayers();

        // Sort players by Wins in descending order
        players.Sort((x, y) => y.Elo.CompareTo(x.Elo));

        // Header for leaderboard
        string reply = "Top Players by Wins:\n";
        reply += $"{"Rank",-8}{"Name",-20}{"W/L",-8}{"WinRate",-10}{"Elo"}\n";
        reply += new string('-', 40) + "\n";

        // Build leaderboard content
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            var user = Context.Guild.GetUser(player.UID); // Get Discord user
            string username = user?.DisplayName ?? "Unknown"; // Handle missing users
            string winLoss = $"{player.Win}/{player.Loss}";
            double winRate = CalculateWinRate(player.Win, player.Loss);
            int elo = player.Elo;

            reply += $"{i + 1,-8}{username,-20}{winLoss,-8}{$"{winRate:F2}%",-10}{elo}\n";
        }

        // Reply with leaderboard
        await ReplyAsync($"```{reply}```"); // Wrap in code block for better Discord formatting
    }

    private static double CalculateWinRate(int wins, int losses)
    {
        if (wins + losses == 0)
            return 0; // Avoid division by zero when no matches played

        return (double)wins / (wins + losses) * 100;
    }

    //--------------------------------------------------------------------------------------------

    [Command("rank")]
    public async Task Rank(SocketUser? user = null)
    {
        SocketUser cur;
        if (user == null) cur = Context.User;
        else cur = user;

        playerManager.AddOrUpdatePlayer(cur.Id);

        var player = playerManager.GetPlayer(cur.Id);
        var ans = $"{cur.Username, -15}\n\n" +
            $"{"Rank:", -15}{CalculateRank(player.Elo)}\n" +
            $"{"Elo:",-15}{player.Elo}\n" +
            $"{"Win/Loss",-15}{player.Win}/{player.Loss}\n" +
            $"{"Winrate:",-15}{CalculateWinRate(player.Win,player.Loss):F2}%\n" +
            $"{"Win Factor:",-15}{player.WinFactor}\n" +
            $"{"Loss Factor:",-15}{player.LossFactor}";

        await ReplyAsync($"```{ans}```");
    }

    public string CalculateRank(int elo)
    {
        switch (elo)
        {
            case < 100: return "Iron";
            case int n when n >= 100 && n < 200: return "Bronze";
            case int n when n >= 200 && n < 300: return "Silver";
            case int n when n >= 400 && n < 500: return "Gold";
            case int n when n >= 500 && n < 600: return "Platinum";
            case int n when n >= 600 && n < 700: return "Emerald";
            case int n when n >= 700 && n < 800: return "Diamond";
            case int n when n >= 800 && n < 900: return "Master";
            case int n when n >= 900 && n < 1000: return "Grandmaster";
            case >= 1000: return "Challenger";
            default: return "";
        }
    }
}
