# MenuManager Setup Instructions

## Download MenuManager.dll

1. Go to [MenuManagerCS2 Releases](https://github.com/NickFox007/MenuManagerCS2/releases)
2. Download the latest release
3. Extract `MenuManager.dll` from the archive
4. Place `MenuManager.dll` in your plugin directory alongside `BlockerPasses.dll`

## File Structure

Your plugin directory should look like this:
```
plugins/
├── BlockerPasses/
│   ├── BlockerPasses.dll
│   ├── MenuManager.dll
│   └── blocker_passes.json
```

## Installation Steps

1. **Install MenuManagerCS2 Core Plugin**:
   - Download and install the main MenuManagerCS2 plugin
   - Follow the installation instructions in the [MenuManagerCS2 repository](https://github.com/NickFox007/MenuManagerCS2)

2. **Add MenuManager.dll**:
   - Copy `MenuManager.dll` to your BlockerPasses plugin directory
   - Ensure the file is in the same directory as `BlockerPasses.dll`

3. **Restart Server**:
   - Restart your CS2 server to load the new dependencies

## Verification

After installation, you can verify the integration by:

1. Joining the server as an admin
2. Using the command `css_bp_menu` or `css_bp`
3. You should see the BlockerPasses management menu

If the menu doesn't appear, check:
- MenuManagerCS2 is properly installed and running
- MenuManager.dll is in the correct location
- You have `@css/root` permissions
- The menu is enabled in your configuration (`"EnableMenu": true`)

## Troubleshooting

### Menu Not Appearing
- Ensure MenuManagerCS2 core plugin is installed and running
- Check that MenuManager.dll is in the plugin directory
- Verify you have proper admin permissions

### Configuration Issues
- Make sure `"EnableMenu": true` in your blocker_passes.json
- Check that all required fields are present in the Menu section

### Permission Errors
- Ensure you have `@css/root` permissions
- Check that the commands are properly registered
