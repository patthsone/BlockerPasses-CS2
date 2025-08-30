# BlockerPasses-CS2
Blocks passages if there are not a certain number of players on the server

# Installation
1. Install [CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp), [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master) and [ResourcePrecacher](https://github.com/Pisex/ResourcePrecacher/releases/tag/1.0f)
3. Download [BlockerPasses-CS2](https://github.com/patthsone/BlockerPasses-CS2/releases/tag/0.0.1)
4. Unzip the archive and upload it to the game server

### After installing ResourcePrecacher, all the paths you write in the config, write them there as well

# Commands
`css_bp_reload`, `!bp_reload`,`!css_bp_getpos`,`css_bp_geteye` - reloads the configuration(only for `@css/root`)

# Config

```json
{
  "Players": 10,      // The number of players after which the passes will open
  "Message": "...",   // A message stating that the passageways are blocked (all the tags are at the bottom)
  "Maps": {
    "de_mirage": [    // Map name
      {
        "ModelPath": "models/props/de_dust/hr_dust/dust_windows/dust_rollupdoor_96x128_surface_lod.vmdl", // Path to the model
        "Color": [ 30, 144, 255 ],                // RGB color in which the model will be colored
        "Origin": "-1600.46 -741.124 -172.965",   // Position where the model will be placed
        "Angles": "0 180 0"                       // Which way the model will be turned
      },
      {
        "ModelPath": "models/props/de_mirage/small_door_b.vmdl",
        "Color": [ 255, 255, 255 ],
        "Origin": "588.428 704.941 -136.517",
        "Angles": "0 270.256 0"
      },
      {
        "ModelPath": "models/props/de_mirage/large_door_c.vmdl",
        "Color": [ 255, 255, 255 ],
        "Origin": "-1007.87 -359.812 -323.64",
        "Angles": "0 270.106 0"
      },
      {
        "ModelPath": "models/props/de_nuke/hr_nuke/chainlink_fence_001/chainlink_fence_001_256_capped.vmdl",
        "Color": [ 255, 255, 255 ],
        "Origin": "-961.146 -14.2419 -169.489",
        "Angles": "0 269.966 0"
      },
      {
        "ModelPath": "models/props/de_nuke/hr_nuke/chainlink_fence_001/chainlink_fence_001_256_capped.vmdl",
        "Color": [ 255, 255, 255 ],
        "Origin": "-961.146 -14.2419 -43.0083",
        "Angles": "0 269.966 0"
      }            
    ],       //if you need to add more maps, put a comma. But the last map doesn't need one! (example)
    "de_dust2": [
     {
        "ModelPath": "",
        "Color": [ 255, 255, 255 ],
        "Origin": "",
        "Angles": ""
     }
   ]
  }
}
```

# Tags
Colors - `{DEFAULT}`, `{WHITE}`, `{DARKRED}`, `{GREEN}`, `{LIGHTYELLOW}`, `{LIGHTBLUE}`, `{OLIVE}`, `{LIME}`, `{RED}`, `{LIGHTPURPLE}`, `{PURPLE}`, `{GREY}`, `{YELLOW}`, `{GOLD}`, `{SILVER}`, `{BLUE}`, `{DARKBLUE}`, `{BLUEGREY}`, `{MAGENTA}`, `{LIGHTRED}`, `{ORANGE}`

`{MINPLAYERS}` - minimum number of players
