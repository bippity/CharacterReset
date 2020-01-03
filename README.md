Character Reset
[![Build Status](https://travis-ci.org/bippity/CharacterReset.svg?branch=master)](https://travis-ci.org/bippity/CharacterReset)
============

Enable players to manually reset their character to the server's default SSC character.
This plugin is branched out from CustomStarterItems to only reset the character as TShock has included starter items into itself.

This plugin lets players instantly reset their character to the server's SSC default character.
Since "starter items" is now part of TShock, I pulled the reset command feature out from CustomStarterItems and made it into its own plugin.

Commands:
/resetcharacter <all/stats/inventory/quests/banks> - For players to reset themselves
/resetplayers <all/stats/inventory/quests> - For admins to reset all players
/resetplayer <username> <all/stats/inventory/quests> -For admins to reset a specific player (Online or Offline)
  
  characterreset.* -Can use all commands
characterreset.stats -Players can only reset their Health&Mana (/resetcharacter stats)
characterreset.inventory -Players can only reset their inventory (/resetcharacter inventory)
characterreset.quests -Players can only reset their Angler Quests (/resetcharacter quests)
characterreset.banks -Players can only reset their bank inventory (/resetcharacter banks)
characterreset.players -Allows /resetplayers & /resetplayer

*Players can use "/resetcharacter all" if given all 3 permissions (stats, inventory, quests) 

[Character Reset](https://tshock.co/xf/index.php?resources/character-reset-ssc.4/)
