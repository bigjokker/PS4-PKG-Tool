# PS4 PKG Tool

[![Github All Releases](https://img.shields.io/github/downloads/pearlxcore/PS4-PKG-Tool/total.svg)]()
[![License](https://img.shields.io/github/license/pearlxcore/PS4-PKG-Tool.svg)](LICENSE)

A desktop tool for managing and viewing your PS4 PKG collection.

Suggestions are welcome. Report any bugs [here](https://github.com/pearlxcore/PS4-PKG-Tool/issues).

**This is not software for obtaining free PS4 games.**

# Requirement

- [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

# Features

- Fast startup with manifest cache. Scan once, load instantly every time after.
- Drag and drop PKG folders onto the window to load them.
- Grouped list view. Group your library by Title, Title ID, Category, System Version, or PKG Type.
- Title column visible in the main grid so game names are shown without clicking.
- Rename PKG by install priority. Files get sequence prefixes: 00 - Base, 01 - Update.
- Rename PKG using naming presets or a custom format with placeholders.
- Move and organize PKGs into folders by title, category, region, or type.
- View PKG information: param.sfo, trophy list, entries, and change info.
- View and extract PKG data including background images and icons.
- Trophy metadata viewer with names, descriptions, and grades.
- Latest Update column shows the newest available update version per PKG.
- Download official update PKGs from a standalone form.
- Filter the grid by category (Game, Patch, Addon, App) or search by filename, title, or ID.
- Set backport labels and check for duplicate PKGs.
- Export your PKG list to Excel.
- Send PKGs to PS4 over the network using Remote Package Installer.
- Check PS5 backward compatibility.

# How to use Remote Package Installer

This is only compatible with PS4 firmware that can run Flatz's Remote Package Installer app. Split update PKG files are currently not supported.

- Open the program settings.
- Set the IP addresses for your PC and PS4.
- Install Node.js and the http-server module (ensure that Node.js is allowed through the firewall).
- If you are unable to install the http-server module via the PS4 PKG Tool, try restarting the tool and reinstalling the module. Alternatively, run `npm install http-server -g` manually in the command prompt.
- Save the changes and exit the program settings.
- Launch the Remote Package Installer app on your PS4.
- Select the PKG file you wish to install, right-click on it, and choose 'Send PKG to PS4'.

# Screenshot

![4](https://github.com/pearlxcore/PS4-PKG-Tool/assets/36906814/85e05c65-4ece-4e56-9674-61144dea1855)
![1](https://github.com/pearlxcore/PS4-PKG-Tool/assets/36906814/9652aa4d-771e-417f-861e-7ae7072231ae)
![2](https://github.com/pearlxcore/PS4-PKG-Tool/assets/36906814/5cf50de3-122e-4e98-8f2f-fa94ee270586)
![3](https://github.com/pearlxcore/PS4-PKG-Tool/assets/36906814/049ce657-649a-4fd1-9c8a-e1ec923569dd)

# Download

https://github.com/pearlxcore/PS4-PKG-Tool/releases

# Support My Work

[![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/R6R524N7X)

[![paypal](https://user-images.githubusercontent.com/36906814/102657760-39d1ce00-41b1-11eb-96fe-c10e2d9b3f39.png)](https://www.paypal.com/paypalme/pearlxcoree)

# License

[GPL-3.0](LICENSE)

# Credit

- [Robin Perris](https://github.com/RobinPerris) (DarkUI)
- [xXxTheDarkprogramerxXx](https://github.com/xXxTheDarkprogramerxXx)
- [Maxton (RIP)](https://github.com/maxton)
- [leecherman](https://sites.google.com/site/theleecherman/)
- [andshrew](https://github.com/andshrew)
- Sony <3
