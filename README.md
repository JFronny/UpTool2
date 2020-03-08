# UpTool2
Downloading software from online repos since 2019

[![CodeFactor](https://www.codefactor.io/repository/github/jfronny/uptool2/badge)](https://www.codefactor.io/repository/github/jfronny/uptool2)
[![GitHub release (latest by date)](https://img.shields.io/github/v/release/JFronny/UpTool2)](https://github.com/JFronny/UpTool2/releases/latest)
[![GitHub repo size](https://img.shields.io/github/repo-size/JFronny/UpTool2)](https://github.com/JFronny/UpTool2/archive/master.zip)
[![GitHub All Releases](https://img.shields.io/github/downloads/JFronny/UpTool2/total)](https://github.com/JFronny/UpTool2/releases)
[![Discord](https://img.shields.io/discord/466965965658128384)](https://discordapp.com/invite/UjhHBqt)

[Default Repo](https://gist.github.com/JFronny/f1ccbba3d8a2f5862592bb29fdb612c4)
## Folder layout
- %APPDATA%\UpTool2
  - Apps
    - __APPGUID
      - `info.xml` Local copy of some app information, like [this](https://github.com/JFronny/UpTool2#app-layout) but missing ID, File, Hash and Icon
      - [`package.zip`](https://github.com/JFronny/UpTool2#package-layout) The package that was downloaded on install
      - `app` The app install path
        - `__APPFILES` Copy of the app files from above, may contain user-configs
  - `info.xml` File used by UpTool2 for saving info
    - `Version` The installed version. Not used anymore
    - `Repos` The list of package repos
      - `__REPO` A repository
        - `Name` The display name of the repository
        - `Link` The source of the repo xml
    - `Local Repo` A preprocessed copy of the online repos
      - [`__APP`](https://github.com/JFronny/UpTool2#app-layout) A normal app with the icon processed as Base64
  - `Install` The folder containing the actual tool
    - `__ZIP CONTENTS` The app files
    - `tmp` A temporary folder used during updates
      - `__FILES FOR UPDATE` The downloaded update files
## Repo layout
- `repo` The main repo tag
  - `__APPLINK` Links to external app XMLs
  - `__REPOLINK` Links to external repos
  - [`__APP`](https://github.com/JFronny/UpTool2#app-layout) Apps
## App layout
- app
  - `Name` Name of the application
  - `Description` Description that gets displayed on the right panel
  - `Version` Version for update checking, might get removed
  - `ID` The Guid used for identification
  - `File` A link to the package file
  - `Hash` The files SHA256 Hash
  - `Icon` The apps icon, (optional)
  - `MainFile` Main binary, used for starting, (optional)
## Package layout
  - `Install.bat` The script for installing the app
  - `Remove.bat` The script for removing the app
  - `Data` The folder containing binaries
    - `__APPFILES` The binaries
