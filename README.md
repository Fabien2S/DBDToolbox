# DBDToolbox

_DBDToolbox_ is an application that can extract multiple assets from the game _Dead by Daylight_

## How to use

```
DBDToolbox.Runtime.exe [game path] [output path]
```

When not specified, the game path is "C:/Program Files (x86)/Steam/steamapps/common/Dead by Daylight/DeadByDaylight/Content/Paks"
When not specified, the output path is "./output" (a "output" folder is created in the current directory)

## Supported assets

The currently supported assets type are:

- Localization files (.locres -> .locres (text files))
- Sound files (.bnk/.wem -> .ogg)

## Dependencies

- [UETools](https://github.com/UETools/UETools) to extract assets from Dead by Daylight's pak files
- [www2ogg](https://github.com/hcs64/ww2ogg) to convert wem files to ogg
- [ReVorb](https://github.com/ItsBranK/ReVorb) to make the ogg thing playable in VLC

