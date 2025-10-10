# Changelog

All notable changes to BlockerPasses-CS2 will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.0.5] - 2024-12-19

### Added
- **Enhanced texture system**: Improved texture management with better color application
- **Better error handling**: More robust error handling for texture operations
- **Improved menu interface**: Enhanced visual design with emojis and better organization
- **Extended texture support**: Better support for custom textures and patterns
- **Performance optimizations**: Improved rendering performance for texture applications

### Changed
- **Updated texture rendering**: Better color application and visual effects
- **Enhanced menu system**: Improved user experience with better visual indicators
- **Refined texture commands**: Better error messages and user feedback
- **Updated documentation**: Improved examples and usage instructions

### Fixed
- **Texture application bugs**: Fixed issues with texture color application
- **Menu display issues**: Fixed visual inconsistencies in menu system
- **Command error handling**: Improved error messages for invalid commands

## [0.2.0] - 2024-12-19

### Added
- **Texture System**: Complete texture management system with image support
- **TextureEntity class**: For storing texture information and metadata
- **TextureSettings class**: For texture application settings and parameters
- **css_bp_createtexture command**: Create custom textures with images
- **css_bp_applytexture command**: Apply textures to existing blocks
- **css_bp_textures command**: List all available textures
- **Pre-built textures**: white_block, blue_block, red_block, green_block, 2x2_pattern
- **2x2 pattern texture**: Special texture mimicking competitive mode white blocks
- **Texture management menu**: New menu section for texture operations
- **VMT/VTF file support**: Support for custom Source Engine materials
- **Texture categories**: basic, patterns, custom, branding
- **Multi-language support**: English and Russian translations for texture commands
- **Comprehensive documentation**: TEXTURE_GUIDE.md and TEXTURE_SYSTEM.md
- **Enhanced example configuration**: Updated blocker_passes_example.json with texture examples
- **Texture rendering system**: Automatic color application and special effects
- **Backward compatibility**: 100% compatibility with existing blocks

### Changed
- **Enhanced configuration structure**: Added AvailableTextures and DefaultTextureSettings
- **Updated SpawnProp method**: Now supports texture settings parameter
- **Improved menu system**: Added texture management options
- **Extended translations**: Added texture-related translations for EN/RU
- **Updated README.md**: Added texture system documentation and examples

### Technical Details
- **New dependencies**: None (maintains existing dependencies)
- **API changes**: Extended BlockEntity with TexturePath and TextureSettings
- **Configuration migration**: Automatic addition of new fields
- **Performance**: Minimal impact, textures applied only when specified

### Documentation
- **TEXTURE_GUIDE.md**: Quick start guide for texture system
- **TEXTURE_SYSTEM.md**: Detailed technical documentation
- **Updated README.md**: Comprehensive feature overview
- **Enhanced examples**: Real-world usage examples in configuration

## [0.1.0] - Previous Release

### Added
- Basic block spawning system
- MenuManagerCS2 integration with fallback support
- Position and angle tools
- Entity management commands
- Multi-language support (EN/RU)
- Configuration management system

### Features
- Block passage blocking based on player count
- Admin menu system
- Position tracking tools
- Entity information display
- Configuration reload functionality
