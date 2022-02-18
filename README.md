Character Reset
[![Build Status](https://travis-ci.org/bippity/CharacterReset.svg?branch=master)](https://travis-ci.org/bippity/CharacterReset)
============

Enable players to manually reset their character to the server's default SSC character.
This plugin is branched out from CustomStarterItems to only reset the character as TShock has included starter items into itself.

## Overview
This plugin lets players instantly reset their character to the server's SSC default character.
Since "starter items" is now part of TShock, I pulled the reset command feature out from CustomStarterItems and made it into its own plugin.

## Commands:
- `/resetcharacter <all|stats|inventory|quests|banks>` - For players to reset themselves
  - the `< >` means this parameter is required for the command to work
  - for example: `/resetcharacter stats` will reset your stats
- `/resetplayers <all|stats|inventory|quests>` - For admins to reset all players
  - the ` | ` means either or: all or stats or inventory, etc.
  - for example: `/resetplayers all` will reset every player's SSC data
- `/resetplayer <username> <all|stats|inventory|quests>` - For admins to reset a specific player (online or offline)
  - username for offline reset is case-sensitive and must be typed in full
  - for usernames containing spaces, surround the name with `" "`
  - for example: `/resetplayer "Artemis the Hunter" inventory`
  
 ## Permissions
- `characterreset.*` - Can use all commands (admin level permission)
- `characterreset.stats` - Players can only reset their Health&Mana `/resetcharacter stats`
- `characterreset.inventory` - Players can only reset their inventory `/resetcharacter inventory`
- `characterreset.quests` - Players can only reset their Angler Quests `/resetcharacter quests`
- `characterreset.banks` - Players can only reset their bank inventory `/resetcharacter banks`
- `characterreset.players` - Allows `/resetplayers` & `/resetplayer` (admin level permission)

*Players can use `/resetcharacter all` if given all 3 permissions (stats, inventory, quests) 

## Source
[Character Reset](https://tshock.co/xf/index.php?resources/character-reset-ssc.4/)
