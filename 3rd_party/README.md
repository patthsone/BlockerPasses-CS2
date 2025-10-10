# Third Party Dependencies

This folder contains third-party DLL files required by the BlockerPasses plugin.

## MenuManagerApi.dll

To use the enhanced menu system, you need to place the `MenuManagerApi.dll` file in this folder.

### How to get MenuManagerApi.dll:

1. Download MenuManagerCS2 from: https://github.com/MenusMM/MenuManagerCS2
2. Build the project or download the release
3. Copy `MenuManagerApi.dll` to this folder (`3rd_party/MenuManagerApi.dll`)

### Alternative:

If you don't have MenuManagerApi.dll, the plugin will automatically fall back to using CounterStrikeSharp's native ChatMenu system.

## File Structure:

```
3rd_party/
├── MenuManagerApi.dll  (required for enhanced menus)
└── README.md          (this file)
```
