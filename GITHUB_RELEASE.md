# BlockerPasses-CS2 v0.0.5 - Enhanced Texture System

## 🎨 Enhanced Texture System with Performance Improvements

This update focuses on improving the existing texture system, fixing bugs, and enhancing performance for better user experience.

## ✨ What's New

### 🖼️ Enhanced Texture System
- **Improved color application** - better texture rendering system
- **Performance optimizations** - faster texture processing
- **Extended support** - better work with custom textures

### 🎮 Enhanced Menu Interface
- **Visual improvements** - added emojis and better design
- **Better organization** - more intuitive menu structure
- **Improved user experience** - clearer visual indicators

### 🛠️ Better Error Handling
- **More robust error handling** - improved error handling for texture operations
- **Better error messages** - more informative messages for users
- **Command improvements** - better handling of invalid commands

## 🚀 Quick Start Examples

### Apply 2x2 pattern
```bash
css_bp_applytexture 1 2x2_pattern
css_bp_applytexture 2 2x2_pattern
```

### List available textures
```bash
css_bp_textures
```

### Create custom texture
```bash
css_bp_createtexture server_logo "Server Logo" materials/logos/server_logo.vmt branding
```

## 📋 Pre-built Textures

| Name | Description | Category |
|------|-------------|----------|
| `white_block` | Classic white texture | basic |
| `blue_block` | Blue block texture | basic |
| `red_block` | Red block texture | basic |
| `green_block` | Green block texture | basic |
| `2x2_pattern` | 2x2 pattern like competitive mode | patterns |

## 🔧 Technical Improvements

- **Enhanced texture rendering** - better color application and visual effects
- **Performance optimizations** - improved rendering performance
- **Better error handling** - more robust error handling system
- **100% backward compatibility** with existing blocks
- **Multi-language support** (EN/RU)

## 🐛 Bug Fixes

- Fixed texture application bugs
- Fixed menu display issues
- Improved command error handling
- Enhanced performance optimizations

## 📚 Documentation

- Updated `README.md` with new features
- Enhanced `blocker_passes_example.json` with texture examples
- Comprehensive `TEXTURE_GUIDE.md` and `TEXTURE_SYSTEM.md`

## 🔄 Migration

- Existing blocks continue to work unchanged
- New configuration fields added automatically
- Pre-built textures loaded on first startup

## 📦 Installation

1. Download latest version from GitHub
2. Build project: `dotnet build --configuration Release`
3. Copy `BlockerPasses.dll` to plugins folder
4. Restart server to apply improvements

---

**Full changelog and detailed documentation available in the repository!**

**Thanks for using BlockerPasses-CS2! 🎉**

## 🎨 Major Feature: Texture System with Image Support

This major update introduces a comprehensive texture management system that allows you to create and apply custom textures to blocks, including a special 2x2 pattern that mimics the white blocks from competitive mode.

## ✨ New Features

### 🖼️ Texture System
- **Custom texture creation** with image support
- **2x2 pattern** - special texture mimicking competitive mode white blocks
- **Pre-built textures** - ready-to-use colored blocks
- **VMT/VTF file support** - work with custom Source Engine materials

### 🎯 New Commands
- `css_bp_createtexture` - create new textures
- `css_bp_applytexture` - apply textures to blocks
- `css_bp_textures` - list all available textures

### 🎮 Enhanced Menu
- **New "🎨 Texture Management" section** in main menu
- **MenuManagerCS2 integration** with fallback support
- **Intuitive interface** for texture management

## 🚀 Quick Start Examples

### Create texture with server logo
```bash
css_bp_createtexture server_logo "Server Logo" materials/logos/server_logo.vmt branding
css_bp_applytexture 1 server_logo
```

### Apply 2x2 pattern
```bash
css_bp_applytexture 1 2x2_pattern
css_bp_applytexture 2 2x2_pattern
```

### List available textures
```bash
css_bp_textures
```

## 📋 Pre-built Textures

| Name | Description | Category |
|------|-------------|----------|
| `white_block` | Classic white texture | basic |
| `blue_block` | Blue block texture | basic |
| `red_block` | Red block texture | basic |
| `green_block` | Green block texture | basic |
| `2x2_pattern` | 2x2 pattern like competitive mode | patterns |

## 🔧 Technical Improvements

- **New classes**: `TextureEntity`, `TextureSettings`
- **Enhanced configuration** with texture support
- **Automatic texture rendering** with color application
- **100% backward compatibility** with existing blocks
- **Multi-language support** (EN/RU)

## 📚 Documentation

- `TEXTURE_GUIDE.md` - Quick start guide
- `TEXTURE_SYSTEM.md` - Detailed technical documentation
- Updated `README.md` with new features
- Enhanced `blocker_passes_example.json` with texture examples

## 🔄 Migration

- Existing blocks continue to work unchanged
- New configuration fields added automatically
- Pre-built textures loaded on first startup

## 📦 Installation

1. Download latest version from GitHub
2. Build project: `dotnet build --configuration Release`
3. Copy `BlockerPasses.dll` to plugins folder
4. Update configuration (see examples in documentation)

---

**Full changelog and detailed documentation available in the repository!**

**Thanks for using BlockerPasses-CS2! 🎉**
