# BlockerPasses-CS2
Blocks passages if there are not a certain number of players on the server

## Features
- **MenuManager Integration**: Uses MenuManagerCS2 for enhanced menu system
- **Fallback Support**: Falls back to native ChatMenu if MenuManager is not available
- **Management Menu**: Easy-to-use menu interface for administrators
- **Position Tools**: Get current position and eye angles
- **Entity Information**: View detailed information about map entities
- **Configuration Control**: Reload configuration and view settings

## Installation
1. Install [CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp), [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master) and [ResourcePrecacher](https://github.com/Pisex/ResourcePrecacher/releases/tag/1.0f)
2. **Optional**: Install [MenuManagerCS2](https://github.com/MenusMM/MenuManagerCS2) for enhanced menu system
   - Download and build MenuManagerCS2
   - Copy `MenuManagerApi.dll` to `3rd_party/` folder
3. Clone this repository and build the project
4. Place `BlockerPasses.dll` in your plugins directory

### After installing ResourcePrecacher, all the paths you write in the config, write them there as well

## Commands
`css_bp_reload`, `!bp_reload` - reloads the configuration (only for `@css/root`)
`css_bp_getpos`, `css_bp_geteye` - gets current position and eye angles (only for `@css/root`)
`css_bp_menu`, `css_bp` - opens the management menu (only for `@css/root`)

## Menu System

This plugin supports both **MenuManagerCS2** and **CounterStrikeSharp's native ChatMenu** systems, providing flexibility and enhanced functionality.

### MenuManagerCS2 Integration
- **Enhanced UI**: Better visual design and user experience
- **Advanced Features**: More menu options and customization
- **Consistent Interface**: Unified menu system across plugins
- **Automatic Detection**: Automatically uses MenuManager if available

### Fallback Support
- **Native ChatMenu**: Falls back to CounterStrikeSharp's built-in menu system
- **No External Dependencies**: Works out of the box without additional plugins
- **Reliable Performance**: Stable menu system with proper error handling
- **Easy Development**: Simple API for menu creation and management

### Menu Options
1. **Reload Config** - Reloads the plugin configuration
2. **Get Position** - Gets current player position and angles (if enabled)
3. **Get Eye Angles** - Gets current player eye angles (if enabled)
4. **Current Settings** - Shows current minimum players and map name
5. **Map Entities** - Shows all entities configured for the current map

## Requirements
- CounterStrikeSharp with runtime
- **Optional**: MenuManagerCS2 for enhanced menu system
  - Requires `MenuManagerApi.dll` in `3rd_party/` folder
  - Requires PlayerSettings and AnyBaseLibCS2 plugins
