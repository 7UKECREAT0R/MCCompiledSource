# Manual Installation

The installer is the easiest way to install, uninstall, and update the software; However, having control over how the
software is installed is needed in some circumstances.

## Download
Grab your desired release from the [Releases Page](https://github.com/7UKECREAT0R/MCCompiled/releases).
MCCompiled has a portable binary, so unzip the release where you would like it to be installed at.

## Registering to the System
Add the directory containing `mc-compiled.exe` to your system-wide <tooltip term="PATH">PATH variable</tooltip>. Restart
any open terminal instances and then make sure that a command like `mc-compiled --version` works properly.

![Run result of 'mc-compiled --version'](WindowsTerminal_HfUGMGIKyg.png){width="400"}

### Protocol Launch
When the web editor wants to launch the language server, it uses a URL protocol to do so. To register it with your system,
open an administrator-level command line and run the command `mc-compiled --protocol true`. You may need to restart your
browser or computer if the server fails to launch.

## Uninstall
Uninstalling is as simple as running `mc-compiled --protocol false` (if protocol support was installed in the first place),
removing the entry from your <tooltip term="PATH">PATH variable</tooltip>, and deleting the folder that holds all the
files related to the binary. Additionally, remove `%localappdata%/.mccompiled` to remove any cached files.
{ignore-vars="true"}
