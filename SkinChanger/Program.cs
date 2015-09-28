using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SkinChanger
{
    internal class Program
    {
        public static Menu Menu { get; private set; }
        public static PlayerWatcher[] PlayerWatchers;
        public static Obj_AI_Hero[] PlayerList;

        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            InfoManager.Initialize();
            
            CustomEvents.Game.OnGameLoad += loadArgs =>
            {
                // Assigns 0 to the local player to be first, 1 to all other allies to be after the local player,
                // then finally 2 to all enemies so they're at the end.
                PlayerList =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .OrderBy(player => player == ObjectManager.Player ? 0 : player.IsAlly ? 1 : 2)
                        .ToArray();
                PlayerWatchers = new PlayerWatcher[PlayerList.Length];

                Menu = new Menu("Skin Changer", "everesty_skin_changer", true);

                var allyLabeled = false;
                var enemyLabeled = false;

                for (var i = 0; i < PlayerList.Length; i++)
                {
                    var player = PlayerList[i];

                    if (player.IsAlly && !allyLabeled)
                    {
                        Menu.AddSubMenu(new Menu("----- Ally Team -----", "ally_label"));
                        allyLabeled = true;
                    }
                    else if (player.IsEnemy && !enemyLabeled)
                    {
                        Menu.AddSubMenu(new Menu("----- Enemy Team -----", "enemy_label"));
                        enemyLabeled = true;
                    }

                    PlayerWatchers[i] = new PlayerWatcher(player, i);
                }

                Menu.AddToMainMenu();

                Game.OnInput += OnGameInput;
            };
        }

        private static void OnGameInput(GameInputEventArgs args)
        {
            if (!args.Input.StartsWith("/model "))
            {
                return;
            }

            args.Process = false;

            var playerIndex = 0;
            var modelIndex = -1;
            var modelArgIndex = 0;
            var messageArgs = args.Input.Replace("/model ", "").Split(' ');

            if (messageArgs.Length == 2)
            {
                modelArgIndex = 1;

                if (!int.TryParse(messageArgs[0], out playerIndex) || playerIndex < 0 || playerIndex >= PlayerList.Length)
                {
                    playerIndex = -1;
                }
            }

            if (messageArgs.Length >= 1)
            {
                if (int.TryParse(messageArgs[modelArgIndex], out modelIndex))
                {
                    if (modelIndex < 0 || modelIndex >= InfoManager.ModelNames.Length)
                    {
                        modelIndex = -1;
                    }
                }
                else
                {
                    modelIndex = Array.IndexOf(InfoManager.ModelNames, messageArgs[modelArgIndex]);
                }
            }

            if (playerIndex > -1 && modelIndex > -1)
            {
                PlayerWatchers[playerIndex].SetModelIndex(modelIndex);
            }
        }
    }
}
