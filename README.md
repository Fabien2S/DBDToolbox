# DBDToolbox

_DBDToolbox_ is an application that can extract assets from the game _Dead by Daylight_.  
It _might_ also works for other games.

## Warning
The recent PTB (5.7.0) is currently unsupported because of the Unreal Engine update (from 4.25 to 4.27).  
This is a [known issue](/../../issues/9) and I will try to fix it in my free time.

## How to use
You can download the latest version at here: [DBDToolbox.zip](https://github.com/Fabien2S/DBDToolbox/releases/latest/download/DBDToolbox.zip)
```
DBDToolbox.Runtime.exe [game path] [output path]
```

### Arguments
- `game path`: Where the pak archives are located
  - When unspecified, value is `C:/Program Files (x86)/Steam/steamapps/common/Dead by Daylight/DeadByDaylight/Content/Paks`
- `output path`: Where the files are extracted
  - When unspecified, value is `./output/` (a "output" folder is created in the current directory)

## Supported assets
- Localization files (.locres -> .locres (text files))
- Sound files (.bnk/.wem -> .ogg)

## Dependencies

- [UETools](https://github.com/UETools/UETools) to extract assets from Dead by Daylight's pak files
- [www2ogg](https://github.com/hcs64/ww2ogg) to convert wem files to ogg
- [ReVorb](https://github.com/ItsBranK/ReVorb) to make the ogg thing playable in VLC
