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
      - info.xml
        - Name
        - Description
        - Version
        - MainFile
      - package.zip
        - Install.bat
        - Remove.bat
        - Data
          - __APPFILES
      - app
        - __APPFILES
  - info.xml
    - Version
    - Repos
      - __REPO
        - Name
        - Link
    - Local Repo
      - [__APP](https://github.com/JFronny/UpTool2#app-layout)
  - Install
    - __ZIP CONTENTS
    - tmp
      - __FILES FOR UPDATE
## Repo layout
- repo
  - __APPLINK
  - __REPOLINK
  - [__APP](https://github.com/JFronny/UpTool2#app-layout)
## App layout
- app
  - Name
  - Description
  - Version
  - ID
  - File
  - Hash
  - Icon
  - MainFile
