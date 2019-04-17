# KeePassRDP
## Overview
KeePassRDP is a plugin for KeePass 2.x which adds multiple options to connect via RDP to the URL of an entry.

## Installation
1. Download the zip file from the newest [release](https://github.com/iSnackyCracky/KeePassRDP/releases)
2. Unzip and copy the KeePassRDP.plgx file to your KeePass plugins folder.

## Usage
To connect via rdp to a machine, select the entry containing the IP-address or hostname, right-click and select *KeePassRDP* > *Open RDP connection* (or just press <kbd>CTRL</kbd> + <kbd>M</kbd>).

To use the other connection options, just select the corresponding entries in the context-menu.

## Features
- Connect to host via RDP
- Connect to host via RDP admin session (mstsc.exe /admin parameter)
- Gather and show possible Windows or domain credentials when the connection entry is inside a group called "RDP" (see below for details)


### RDP subgroup / folder
This is how I use the extension for on a daily basis (I work for an MSP where we store credentials for customer domains or machines inside KeePass):

Our KeePass Database is structured like this:

![DB structure image](https://isnackycracky.github.io/KeePassRDP/img/db_structure.jpg)

Where each group contains entries specific to that customer.

If there ist just a single jumphost or something like that, we just create an entry like this directly inside the customer group:

![jumphost example image](https://isnackycracky.github.io/KeePassRDP/img/jumphost_entry.jpg)

But if a customer has many hosts and multiple accounts to access them, we create a subgroup called **RDP** (this has to be uppercase and directly inside the customer group to work) inside a customer group:

![rdp subgroup example image](https://isnackycracky.github.io/KeePassRDP/img/rdp_subgroup.jpg)

Which may contain entries like this:

![RDP subgroup example entries](https://isnackycracky.github.io/KeePassRDP/img/rdp_subgroup_entries.jpg)

The customer group itself contains the account-entries in this case (they can also be in different subgroups one level below the customer group):

![cusotmer example entries](https://isnackycracky.github.io/KeePassRDP/img/customer_entries.jpg)

If we now want to connect to one of the machines in the RDP subgroup (with credential usage), just select the machine-entry, press <kbd>CTRL</kbd> + <kbd>M</kbd> and KeePassRDP shows you a dialog with viable account-entries (with titles like e.g. *domain-admin*, *local user*, ...) it always ignores entries where the title ends with **[rdpignore]**

![credential selection dialog](https://isnackycracky.github.io/KeePassRDP/img/credential_picker.jpg)

Now just select the entry you want and klick ok (or press <kbd>Enter</kbd>).


## Third-Party Software
This plugin uses the *awesome* C# ListView wrapper [**ObjectListView**](http://objectlistview.sourceforge.net/cs/index.html) by Phillip Piper
