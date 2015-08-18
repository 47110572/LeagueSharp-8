using System;
using System.Collections.Generic;
using System.IO;
using LeagueSharp;
using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Events;
using LeagueSharp.SDK.Core.Extensions.SharpDX;
using LeagueSharp.SDK.Core.UI.IMenu;
using LeagueSharp.SDK.Core.Utils;
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
        private static Dictionary<string, uint> _stats;
        private static Font _font;
        private static Line _line;
        private static int _drawX = Drawing.Width - 30;
        private static int _drawY = 100;
        private static Rectangle _lastRenderArea;
        private static bool _dragging;
        private static float _dragX;
        private static float _dragY;
        private static uint _movements;

        private static void Main(string[] mainArgs)
        {
            Bootstrap.Init(new string[0]);
            
            LoadData();
            RegisterStatListeners();

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

            Load.OnLoad += (loadSender, loadArgs) =>
            {
                // Draw Our statistics
                Drawing.OnEndScene += OnEndScene;

                Game.OnWndProc += args =>
                {
                    if (!MenuManager.Instance.MenuVisible)
                        return;

                    var keys = new WindowsKeys(args);

                    if (keys.Msg == WindowsMessages.LBUTTONDOWN &&
                        keys.Cursor.IsUnderRectangle(_lastRenderArea.X, _lastRenderArea.Y, _lastRenderArea.Width,
                            _lastRenderArea.Height))
                    {
                        _dragX = keys.Cursor.X - _drawX;
                        _dragY = keys.Cursor.Y - _drawY;
                        _dragging = true;
                    }
                    else if (keys.Msg == WindowsMessages.MOUSEMOVE && _dragging)
                    {
                        _drawX = (int) (keys.Cursor.X - _dragX);
                        _drawY = (int) (keys.Cursor.Y - _dragY);
                    }
                    else if (keys.Msg == WindowsMessages.LBUTTONUP)
                    {
                        _dragging = false;
                    }
                };

                AppDomain.CurrentDomain.DomainUnload += (sender, args) => SaveData();
                AppDomain.CurrentDomain.ProcessExit += (sender, args) => SaveData();
            };
        }

        private static void OnEndScene(EventArgs drawArgs)
        {
            // Only when the menu is open.
            if (!MenuManager.Instance.MenuVisible)
                return;

            if (_dragging)
            {
                
            }

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
            Load.OnLoad += (loadSender, loadArgs) =>
            {
                Obj_AI_Base.OnIssueOrder += (sender, args) =>
                {
                    if (sender.NetworkId == GameObjects.Player.NetworkId)
                    {
                        _movements++;
                        _stats["Movement Commands"]++;
                    }
                };

                Game.OnNotify += args =>
                {
                    if (args.NetworkId != GameObjects.Player.NetworkId)
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

                Game.OnEnd += args => _stats[args.WinningTeam == GameObjects.Player.Team
                    ? "Wins"
                    : "Losses"]++;
            };
        }

        #region Data Managment

        private static void SaveData()
        {
            _stats["Clicks Last Game"] = _movements;

            if (!File.Exists(SaveFile))
                File.Create(SaveFile).Close();

            using (var file = File.Open(SaveFile, FileMode.Truncate, FileAccess.Write))
            using (var writer = new BinaryWriter(file))
            {
                writer.Write(_drawX);
                writer.Write(_drawY);
                writer.Write(_stats.Count);

                foreach (var pair in _stats)
                {
                    writer.Write(pair.Key);
                    writer.Write(pair.Value);
                }
            }

            File.WriteAllText(SaveLog,
                "[" + DateTime.Now.ToShortDateString() + "] Clicks: " + _movements + "\r\n" + (File.Exists(SaveLog)
                    ? File.ReadAllText(SaveLog)
                    : ""));
        }

        private static void LoadData()
        {
            // Default the stats dictionary
            _stats = new Dictionary<string, uint>
            {
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
                _drawX = reader.ReadInt32();
                _drawY = reader.ReadInt32();

                var count = reader.ReadInt32();

                for (var i = 0; i < count; i++)
                {
                    var key = reader.ReadString();
                    var value = reader.ReadUInt32();
                    _stats[key] = value;
                }
            }
        }

        #endregion
    }
}
