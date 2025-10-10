# BlockerPasses-CS2
Blocks passages if there are not a certain number of players on the server

## Features
- **MenuManager Integration**: Uses MenuManagerCS2 for enhanced menu system
- **Fallback Support**: Falls back to native ChatMenu if MenuManager is not available
- **Management Menu**: Easy-to-use menu interface for administrators
- **Position Tools**: Get current position and eye angles
- **Entity Information**: View detailed information about map entities
- **Configuration Control**: Reload configuration and view settings
- **🎨 Texture System**: Create and apply custom textures to blocks
- **🖼️ Image Support**: Support for custom texture images and patterns
- **📋 Pre-built Textures**: Ready-to-use textures including 2x2 pattern
- **🌐 Multi-language**: English and Russian language support

## Installation
1. Install [CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp), [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master) and [ResourcePrecacher](https://github.com/Pisex/ResourcePrecacher/releases/tag/1.0f)
2. **Optional**: Install [MenuManagerCS2](https://github.com/MenusMM/MenuManagerCS2) for enhanced menu system
   - Download and build MenuManagerCS2
   - Copy `MenuManagerApi.dll` to `3rd_party/` folder
3. Clone this repository and build the project
4. Place `BlockerPasses.dll` in your plugins directory

### After installing ResourcePrecacher, all the paths you write in the config, write them there as well

## Commands

### Basic Commands
`css_bp_reload`, `!bp_reload` - reloads the configuration (only for `@css/root`)
`css_bp_getpos`, `css_bp_geteye` - gets current position and eye angles (only for `@css/root`)
`css_bp_menu`, `css_bp` - opens the management menu (only for `@css/root`)

### Texture Management Commands
`css_bp_createtexture <name> <display_name> [texture_path] [category]` - creates a new texture
`css_bp_applytexture <block_index> <texture_name>` - applies texture to a block
`css_bp_textures` - lists all available textures

### Block Management Commands
`css_bp_add [invisibility] [quota]` - adds a block at current position
`css_bp_list` - lists all blocks on current map
`css_bp_removeall` - removes all blocks from current map

## Texture System

### Creating Textures
```bash
# Create a custom texture
css_bp_createtexture my_logo "Server Logo" materials/logos/server_logo.vmt branding

# Create a 2x2 pattern texture
css_bp_createtexture 2x2_pattern "2x2 Pattern" null patterns
```

### Applying Textures
```bash
# Apply texture to block
css_bp_applytexture 1 my_logo
css_bp_applytexture 2 2x2_pattern
```

### Pre-built Textures
- **white_block** - Classic white block
- **blue_block** - Blue colored block
- **red_block** - Red colored block
- **green_block** - Green colored block
- **2x2_pattern** - 2x2 checkerboard pattern (like competitive mode)

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
1. **🔄 Reload Config** - Reloads the plugin configuration
2. **📍 Get Position** - Gets current player position and angles (if enabled)
3. **👁️ Get Eye Angles** - Gets current player eye angles (if enabled)
4. **⚙️ Current Settings** - Shows current minimum players and map name
5. **🗺️ Map Entities** - Shows all entities configured for the current map
6. **🎨 Texture Management** - Manage textures and apply them to blocks

## Requirements
- CounterStrikeSharp with runtime
- **Optional**: MenuManagerCS2 for enhanced menu system
  - Requires `MenuManagerApi.dll` in `3rd_party/` folder
  - Requires PlayerSettings and AnyBaseLibCS2 plugins

## Documentation
- **[Texture System Guide](TEXTURE_GUIDE.md)** - Quick start guide for texture system
- **[Detailed Texture Documentation](TEXTURE_SYSTEM.md)** - Complete texture system documentation
- **[Example Configuration](blocker_passes_example.json)** - Example configuration with texture examples

## Contributing
Feel free to submit issues and enhancement requests!

## License
This project is licensed under the MIT License.
