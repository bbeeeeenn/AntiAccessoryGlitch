using System.Timers;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace AntiAccessoryGlitch
{
    [ApiVersion(2, 1)]
    public class AntiAccessoryGlitch : TerrariaPlugin
    {
        public override string Name => "AntiAccessoryGlitch";
        public override Version Version => new(1, 1, 2);
        public override string Author => "TRANQUILZOIIP - github.com/bbeeeeenn";
        public override string Description =>
            "AntiAccessoryGlitch is a TShock plugin designed to stop players from abusing the accessory duplication glitch. It effectively detects and prevents players from equipping the same accessory across all accessory slots, ensuring balanced and fair gameplay on your Terraria server. Lightweight, reliable, and easy to use—keep your server glitch-free with AntiAccessoryGlitch!";
        private System.Timers.Timer? CheckInventoriesTimer;

        private readonly Dictionary<string, DateTime> PlayersToHandle = new();

        public AntiAccessoryGlitch(Main game)
            : base(game) { }

        public override void Initialize()
        {
            CheckInventoriesTimer = new System.Timers.Timer(1000); // run every 1 second.
            CheckInventoriesTimer.Elapsed += OnTimerElapsed;
            CheckInventoriesTimer.Enabled = true;
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            foreach (TSPlayer player in TShock.Players)
            {
                if (player == null || !player.Active || player.Dead)
                    return;

                if (
                    player.Group.permissions.Contains("tshock.admin.nokick")
                    || player.Group.permissions.Contains("*")
                )
                    continue;
                else
                    CheckPlayerAccessory(player);
            }
        }

        void CheckPlayerAccessory(TSPlayer player)
        {
            try
            {
                for (int i = 0; i < player.Accessories.Count(); i++)
                {
                    string item1 = player.Accessories.ToArray()[i].Name;

                    // Skip if blank slot
                    if (item1 == "")
                        continue;

                    for (int j = i + 1; j < player.Accessories.Count(); j++)
                    {
                        string item2 = player.Accessories.ToArray()[j].Name;
                        if (item1 == item2)
                        {
                            if (PlayersToHandle.ContainsKey(player.Name))
                            {
                                TimeSpan timespan = DateTime.Now - PlayersToHandle[player.Name];
                                int TimeLeft = 20 - timespan.Seconds;
                                if (TimeLeft < 0)
                                {
                                    player.Kick("Exploiter.");
                                    PlayersToHandle.Remove(player.Name);
                                }
                                else
                                    player.SendInfoMessage($"Time left: {TimeLeft}");
                                return;
                            }

                            player.SetBuff(type: 47, time: 20 * 60);
                            PlayersToHandle[player.Name] = DateTime.Now;
                            TShock.Utils.Broadcast(
                                $"{player.Name} has more than one '{player.TPlayer.armor[i].Name}' equipped.",
                                Color.OrangeRed
                            );
                            player.SendInfoMessage(
                                $"You have more than one '{player.TPlayer.armor[i].Name}' from your equipment.\nRemove them or you will be kicked from the server."
                            );
                            return;
                        }
                    }
                }
                if (PlayersToHandle.ContainsKey(player.Name))
                {
                    player.SendInfoMessage("Thank you...");
                    PlayersToHandle.Remove(player.Name);
                }
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(
                    $"AntiAccessoryGlitch: Error checking inventory for player {player.Name}: {ex.Message}"
                );
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (CheckInventoriesTimer != null)
                {
                    CheckInventoriesTimer.Stop();
                    CheckInventoriesTimer.Elapsed -= OnTimerElapsed;
                    CheckInventoriesTimer.Dispose();
                    CheckInventoriesTimer = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
