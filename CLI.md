# CLI

### Package Management

Installing a package (set \<package> to a file for local): uptool install \<package>

Upgrading a package: uptool upgrade \<package>

Reinstalling a package: uptool reinstall \<package>

Removing a package: uptool remove \<package>

Removing a package and all its configuration and data files: uptool purge \<package>

Upgrade all packages: uptool dist-upgrade

### Cache Management

List installed packages: uptool list

Search for a package: uptool search \<text>

Show package info: uptool show \<package>

Updating the cache: uptool update

### Repos Management

Add a repository: uptool add-repo \<name> \<link>

Remove a repository: uptool remove-repo \<name>

List repositories: uptool list-repo

### Other

Upgrading UpToolCLI: uptool upgrade-self

Start an app: uptool start \<package>