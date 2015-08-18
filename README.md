A repository to host all the [LeaugeSharp](https://joduska.me/). You can check my [tello board](https://trello.com/b/fNKZ7usz/l) for statuses and planned assemblies/features.

AiMnFire
========
Crappy remake of AiM. Don't use pls.

Champions
=========

Urgot (Urgod)
-------------
- Combo
  - Q, W, E, and R (under tower only) usage
  - Q & E Hit Chances (each are separate)
- LaneClear
  - Q & E usage
  - Min Minions to use E (1 through 10)
  - Min % Mana to use Q or E (one option)
- Drawing
  - Draw Q, Extended Q, W, E, and R ranges
  - Draw Combo
- Misc
  - Auto-Interrupt with R

ChoGoth (ChoGodth)
------------------
- Combo
  - Q, W, and R usage
  - Q & W Hit Chance (one option)
- LaneClear
  - Q & R usage
  - Min # Minions to use Q (1 through 10)
  - Min % Mana to use Q or R (one option)
- Drawing
  - Draw Q, W, and R ranges
  - Show Combo Damage on Health Bars
- Misc
  - Auto-Interrupt with Q or W (each are separate)

Teemo (Sateemo)
---------------
- Combo
  - Q, W, and R usage
  - Min % Mana to use W
- LaneClear
  - Q & R usage
  - Min # Minions to use R (1 through 10)
- Harass
  - Q usage
- Drawing
  - Draw Q & R ranges
  - Draw Defensive, Optional, Combat, and/or Offensive Shroom Locations
  - Draw Shroom Locations on Minimap
  - Show Combo Damage on Health Bars
- Misc
  - Auto-W
    - Auto-Use W
    - Min % Mana to use W
  - Kill-Steal with Q
  - Gap-Close with Q

Ryze
----
- Combo
  - Q, W, E, and R usage
  - Use R at % Health
  - Q Hit Chance
  - E Min Units Around Target (0 through 6)
- LaneClear
  - Q & E usage
  - E Min Units Around Target (0 through 6)
  - Spam WHen Passive is Active (including W)
- Harass
  - Q, W, and E usage
  - Q Hit Chance
  - E Min Units Around Target (0 through 6)
- Drawing
  - Draw Q, W, and E ranges
  - Show Combo Damage on Health Bars
- Misc
  - Anti-GapClose with W

Graves (RestInGraves)
---------------------
- Combo
  - Q, W, and R usage
  - W Hit Chance
  - W Min % Mana
  - R Min Distance (0 through 1,500)
- LaneClear
  - Q & W usage
  - Min Minions Hit to use Q (1 through 10)
  - Min Minions Killed to use W (1 through 10)
- Drawing
  - Draw Q, W, E, and R ranges
  - Draw Min R Distance
  - Show Combo Damage on Health Bars

Vladimir
--------
- Combo
  - Q, E, and R usage
  - Only use E around # units (1 through 10)
  - R # of Enemies (1 through 5)
  - R When Enemy is at % Health
  - Only R if Eney can be Killed
- LaneClear
  - Q & E usage
  - E on # Units (1 through 20)
  - E on # Units Killed (1 through 10)
  - Only Q if a CS will be gained (option)
- Drawing
  - Draw Q, E, and R ranges
  - Draw Auto-Stack Status
  - Show Combo Damage on Health Bars
- Misc
  - E Auto-Stacking
    - Auto-Stack E (Keybind)
    - Min Stacks to Start Auto Stacking (0 through 4)
    - Use E # Milliseconds Before Stacks Exire (0 through 1000)
    - Minimum Units to Auto Stack (0 through 10)
    - Minimum % Health to Auto-Stack
  - W at % Health
  - Use W as an Anti-GapCloser

Utility
=======

SkinChanger
-----------
Allows you to change the skin or model of any player in a game. All changes are local (only you will see them).

Supports one command: "/model [player index] {model name}" where the player index is optional (defaults to you) and the model name is the internal name of the desired model.

AutoFeed
--------
Old assembly that intentionally feeds and buys items to help with feeding faster.

Libraries
=========

ChampionLib
-----------
Provides an easier way to create a champion assembly, eliminating some common features that are always the same and allowing customization of those features which vary from assembly to assembly.

VisualLib
---------
Provides some methods to draw to the screen.

Coming Soon
===========
- SkinChanger Update
- Vladimir