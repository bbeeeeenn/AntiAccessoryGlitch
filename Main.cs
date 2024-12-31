using Terraria;
using TShockAPI;
using TerrariaApi.Server;
using System.Timers;
using TShockAPI.Hooks;
using Microsoft.Xna.Framework;

namespace AntiAccessoryGlitch
{
    [ApiVersion(2, 1)]
    public class AntiAccessoryGlitch : TerrariaPlugin
    {
        public override string Name => "AntiAccessoryGlitch";
        public override Version Version => new(1, 1, 2);
        public override string Author => "TRANQUILZOIIP";
        public override string Description => "AntiAccessoryGlitch is a TShock plugin designed to stop players from abusing the accessory duplication glitch. It effectively detects and prevents players from equipping the same accessory across all accessory slots, ensuring balanced and fair gameplay on your Terraria server. Lightweight, reliable, and easy to use—keep your server glitch-free with AntiAccessoryGlitch!";
        private System.Timers.Timer? CheckInventoriesTimer;

        public static readonly string path = Path.Combine(TShock.SavePath, "AntiAccessoryGlitch.json");
        private static Config Config = new();

        private readonly Dictionary<string, DateTime> PlayersToHandle = new();

        public AntiAccessoryGlitch(Main game) : base(game) { }

        public override void Initialize()
        {
            GeneralHooks.ReloadEvent += OnReload;
            if (File.Exists(path))
            {
                Config = Config.Read();
            }
            else
            {
                Config.Write();
            }

            CheckInventoriesTimer = new System.Timers.Timer(Config.timer);
            CheckInventoriesTimer.Elapsed += OnTimerElapsed;
            CheckInventoriesTimer.Enabled = true;
        }

        private void OnReload(ReloadEventArgs e)
        {
            if (File.Exists(path)) Config = Config.Read();
            else Config.Write();
            TShock.Log.ConsoleInfo("ActiAccessoryGlitch reloaded.");
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            foreach (TSPlayer player in TShock.Players) if (player != null && player.Active && !player.Dead)
                {
                    if (player.Group.permissions.Contains("tshock.admin.nokick")) return;
                    CheckPlayerInventory(player);
                }
        }

        void CheckPlayerInventory(TSPlayer player)
        {
            try
            {
                for (int i = 3; i < 9; i++)
                {
                    if (player.TPlayer.armor[i].Name == "") continue;
                    for (int j = i + 1; j < 10; j++)
                    {
                        if (player.TPlayer.armor[i].Name == player.TPlayer.armor[j].Name)
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
                                else player.SendInfoMessage($"Time left: {TimeLeft}");
                                return;
                            };
                            player.SetBuff(type: 47, time: 20 * 60);
                            PlayersToHandle[player.Name] = DateTime.Now;
                            TShock.Utils.Broadcast($"{player.Name} has more than one '{player.TPlayer.armor[i].Name}' equipped.", Color.AliceBlue);
                            player.SendInfoMessage($"You have more than one '{player.TPlayer.armor[i].Name}' from your equipment.\nRemove them or you will be kicked from the server.");
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
                TShock.Log.Error($"AntiAccessoryGlitch: Error checking inventory for player {player.Name}: {ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeneralHooks.ReloadEvent -= OnReload;

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