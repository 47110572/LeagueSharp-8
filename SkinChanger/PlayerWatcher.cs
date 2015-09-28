using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace SkinChanger
{
    internal class PlayerWatcher
    {
        public readonly Obj_AI_Hero Player;
        public readonly Menu Menu;

        private readonly List<Obj_AI_Base> _clones = new List<Obj_AI_Base>();
        private bool _wasDead;
        private int _lastModelIndex;
        private int _lastSkinIndex;

        public PlayerWatcher(Obj_AI_Hero player, int listIndex)
        {
            Player = player;
            Menu = new Menu(string.Format("{0} [{1}]", player.CharData.BaseSkinName, listIndex), "player_" + listIndex);

            Menu.AddItem(
                new MenuItem("skin", "Skin").DontSave()
                    .SetValue(new StringList(InfoManager.GetModelByName(player.CharData.BaseSkinName).GetSkinNames(), player.BaseSkinId)));
            Menu.AddItem(
                new MenuItem("model", "Model").DontSave()
                    .SetValue(new StringList(InfoManager.ModelNames,
                        Array.IndexOf(InfoManager.ModelNames, player.CharData.BaseSkinName))));

            Program.Menu.AddSubMenu(Menu);

            Game.OnUpdate += OnGameUpdate;
            GameObject.OnCreate += OnGameObjectCreate;
            GameObject.OnDelete += OnGameObjectDelete;
        }

        private void OnGameUpdate(EventArgs updateArgs)
        {
            var modelIndex = Menu.Item("model").GetValue<StringList>().SelectedIndex;
            var skinIndex = Menu.Item("skin").GetValue<StringList>().SelectedIndex;

            if (!Player.IsDead && (_wasDead || modelIndex != _lastModelIndex || skinIndex != _lastSkinIndex))
            {
                if (modelIndex != _lastModelIndex)
                {
                    skinIndex = 0;
                    Menu.Item("skin").SetValue(new StringList(InfoManager.GetModelByIndex(modelIndex).GetSkinNames()));
                }

                Player.SetSkin(InfoManager.ModelNames[modelIndex], skinIndex);

                foreach (var clone in _clones)
                {
                    clone.SetSkin(InfoManager.ModelNames[modelIndex], skinIndex);
                }

                // TODO Handle Pix
            }

            _lastModelIndex = modelIndex;
            _lastSkinIndex = skinIndex;
            _wasDead = Player.IsDead;
        }

        private void OnGameObjectCreate(GameObject sender, EventArgs args)
        {
            var unit = sender as Obj_AI_Base;

            if (unit != null && unit.IsValid && unit.Name == Player.Name)
            {
                _clones.Add(unit);
            }
        }

        private void OnGameObjectDelete(GameObject sender, EventArgs args)
        {
            var unit = sender as Obj_AI_Base;

            if (unit != null && unit.IsValid)
            {
                _clones.RemoveAll(obj => obj.NetworkId == unit.NetworkId);
            }
        }

        public void SetModelIndex(int index)
        {
            Menu.Item("model").SetValue(new StringList(InfoManager.ModelNames, index));
        }
    }
}
