using System;
using System.Collections.Generic;
using System.IO;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;

namespace StatsSharp
{
    class Program
    {
        private static readonly string SavePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "LS" + Environment.UserName.GetHashCode().ToString("X"));

        private static readonly string SaveFile = Path.Combine(SavePath, "StatsSharp.dat");
        private static readonly string SaveLog = Path.Combine(SavePath, "StatsSharpClicksLog.txt");
        private static readonly Menu Menu = new Menu("StatsSharp", "stats_sharp", true);
        private static Dictionary<string, uint> _stats;
        private static long _gameId;
        private static Font _font;
        private static Line _line;
        private static int _drawX = Drawing.Width - 30;
        private static int _drawY = 100;
        private static Rectangle _lastRenderArea;
        private static bool _dragging;
        private static float _dragX;
        private static float _dragY;

        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] mainArgs)
        {

            // Register the font
            _font = new Font(Drawing.Direct3DDevice, new FontDescription
            {
                FaceName = "Lucida Console",
                Height = 16,
                OutputPrecision = FontPrecision.Default,
                Quality = FontQuality.ClearType
            });
            _line = new Line(Drawing.Direct3DDevice)
            {
                GLLines = true
            };

            Menu.AddItem(new MenuItem("always_show", "Always Show Stats").SetValue(false));
            Menu.AddToMainMenu();

            CustomEvents.Game.OnGameLoad += loadArgs =>
            {
                LoadData();
                RegisterStatListeners();

                // Draw Our statistics
                Drawing.OnEndScene += OnEndScene;

                Game.OnWndProc += args =>
                {
                    if (!MenuGlobals.DrawMenu)
                        return;

                    var cursor = Utils.GetCursorPos();

                    if (args.Msg == (uint) WindowsMessages.WM_LBUTTONDOWN && cursor.X >= _lastRenderArea.X &&
                        cursor.X <= _lastRenderArea.X + _lastRenderArea.Width && cursor.Y >= _lastRenderArea.Y &&
                        cursor.Y <= _lastRenderArea.Y + _lastRenderArea.Height)
                    {
                        _dragX = cursor.X - _drawX;
                        _dragY = cursor.Y - _drawY;
                        _dragging = true;
                    }
                    else if (args.Msg == (uint) WindowsMessages.WM_MOUSEMOVE && _dragging)
                    {
                        _drawX = (int) (cursor.X - _dragX);
                        _drawY = (int) (cursor.Y - _dragY);
                    }
                    else if (args.Msg == (uint) WindowsMessages.WM_LBUTTONUP)
                    {
                        _dragging = false;
                    }
                };

                AppDomain.CurrentDomain.DomainUnload += (sender, args) => SaveData(false);
                AppDomain.CurrentDomain.ProcessExit += (sender, args) => SaveData(false);
                Game.OnEnd += args =>
                {
                    _stats["Clicks Last Game"] = _stats["Clicks This Game"];
                    _stats["Clicks This Game"] = 0;
                    SaveData(true);
                };
            };
        }

        private static void OnEndScene(EventArgs drawArgs)
        {
            if (!MenuGlobals.DrawMenu && !Menu.Item("always_show").GetValue<bool>())
                return;

            var keyText = "";
            var valueText = "";

            foreach (var pair in _stats)
            {
                keyText += "\n" + pair.Key + ":";
                valueText += "\n" + pair.Value;
            }

            var measureKeys = _font.MeasureText(null, keyText, FontDrawFlags.Top);
            var measureValues = _font.MeasureText(null, valueText, FontDrawFlags.Top);
            var measureTitle = _font.MeasureText(null, "Statistics", FontDrawFlags.Top);

            var rect = new Rectangle(_drawX - measureKeys.Width - 10 - measureValues.Width, _drawY,
                measureKeys.Width + 10 + measureValues.Width, measureTitle.Height + _stats.Count * _font.Description.Height);

            measureKeys.X = measureValues.X = measureTitle.X = rect.X;
            measureKeys.Y = measureValues.Y = measureTitle.Y = rect.Y;

            measureTitle.Width = rect.Width;
            measureValues.X += measureKeys.Width + 10;

            rect.X -= 5;
            rect.Y -= 5;
            rect.Width += 10;
            rect.Height += 10;

            _line.Width = rect.Height;
            _line.Draw(
                new[]
                {
                    new Vector2(rect.X, rect.Y + rect.Height / 2f),
                    new Vector2(rect.X + rect.Width, rect.Y + rect.Height / 2f)
                }, new ColorBGRA(0, 0, 0, 100));

            _font.DrawText(null, "Statistics", measureTitle, FontDrawFlags.Top | FontDrawFlags.Center, Color.White);
            _font.DrawText(null, keyText, measureKeys, FontDrawFlags.Top | FontDrawFlags.Left, Color.White);
            _font.DrawText(null, valueText, measureValues, FontDrawFlags.Top | FontDrawFlags.Left, Color.White);

            _lastRenderArea = rect;
        }

        private static void RegisterStatListeners()
        {
            Obj_AI_Base.OnIssueOrder += (sender, args) =>
            {
                if (sender.NetworkId == ObjectManager.Player.NetworkId)
                {
                    _stats["Clicks This Game"]++;
                    _stats["Movement Commands"]++;
                }
            };

            Game.OnNotify += args =>
            {
                if (args.NetworkId != ObjectManager.Player.NetworkId)
                    return;

                switch (args.EventId)
                {
                    case GameEventId.OnChampionKill:
                        _stats["Overall Kills"]++;
                        break;
                    case GameEventId.OnChampionDie:
                        _stats["Overall Deaths"]++;
                        break;
                    case GameEventId.OnDeathAssist:
                        _stats["Overall Assists"]++;
                        break;
                }
            };

            Game.OnEnd += args => _stats[args.WinningTeam == ObjectManager.Player.Team
                ? "Wins"
                : "Losses"]++;
        }

        #region Data Managment

        private static void SaveData(bool writeToLog)
        {
            if (!File.Exists(SaveFile))
                File.Create(SaveFile).Close();

            using (var file = File.Open(SaveFile, FileMode.Truncate, FileAccess.Write))
            using (var writer = new BinaryWriter(file))
            {
                writer.Write(_gameId);
                writer.Write(_drawX);
                writer.Write(_drawY);
                writer.Write(_stats.Count);

                foreach (var pair in _stats)
                {
                    writer.Write(pair.Key);
                    writer.Write(pair.Value);
                }
            }

            if (writeToLog)
            {
                File.WriteAllText(SaveLog,
                    "[" + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff") + "] Clicks: " + _stats["Clicks Last Game"] +
                    "\r\n" + (File.Exists(SaveLog)
                        ? File.ReadAllText(SaveLog)
                        : ""));
            }
        }

        private static void LoadData()
        {
            // Default the stats dictionary
            _stats = new Dictionary<string, uint>
            {
                { "Clicks This Game", 0 },
                { "Clicks Last Game", 0 },
                { "Movement Commands", 0 },
                { "Overall Kills", 0 },
                { "Overall Deaths", 0 },
                { "Overall Assists", 0 },
                { "Wins", 0 },
                { "Losses", 0 }
            };

            if (!File.Exists(SaveFile))
                return;

            using (var file = File.Open(SaveFile, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(file))
            {
                if (file.Length == 162)
                    _gameId = reader.ReadInt64();

                var drawX = reader.ReadInt32();
                var drawY = reader.ReadInt32();

                if (drawX >= 0 && drawX <= Drawing.Width && drawY >= 0 && drawY <= Drawing.Height)
                {
                    _drawX = drawX;
                    _drawY = drawY;
                }

                var count = reader.ReadInt32();

                for (var i = 0; i < count; i++)
                {
                    var key = reader.ReadString();
                    var value = reader.ReadUInt32();
                    _stats[key] = value;
                }

                if (Game.Id == _gameId)
                {
                    _stats["Clicks Last Game"] = _stats["Clicks This Game"];
                    _stats["Clicks This Game"] = 0;
                    SaveData(true);
                }

                _gameId = Game.Id;
            }
        }

        #endregion
    }
}
