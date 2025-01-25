namespace LCBot.Modules
{
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;

    public class GeneralModule : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        [Alias("h")]
        public async Task HelpAsync()
        {
            string reply = "Commands and Instructions:\n" +
                "\n1. STEP - GATHER PARTICIPANTS\n" +
                "!p - list of current participants, make sure it is equal to 10 before proceeding further\n" +
                "!add {username} - add user to current participants by their username or @ them\n" +
                "!rm {username} - remove user from current participants by their username or @ them\n" +
                "!pfc - add every active user from the voice channel you are currently in to the list of participants\n" +
                "\n2. STEP - RANDOMIZE INTO TEAMS\n" +
                "!t - show current teams and their members\n" +
                "!ts - shuffle participants into new teams, can be redone\n" +
                "!pick - pick teams by clicking on desired teamate's button in rotating manner\n" +
                "\n3. STEP - DECIDE THE WINNER\n" +
                "!winner {1/2} - decide the winner by the team number, wins and losses will be added to the stats\n" +
                "!l - create a leaderboard sorted by most wins\n" +
                "\n4. OTHER\n" +
                "!reset - reset participants and both teams lists" +
                "!rank - check players info";

            await this.ReplyAsync(reply);
        }
    }
}
