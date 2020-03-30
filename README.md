[<img alt="latest release" src="https://img.shields.io/github/v/release/iSnackyCracky/KeePassRDP?style=flat-square">](https://github.com/iSnackyCracky/KeePassRDP/releases/latest) <img alt="downloads (all releases)" src="https://img.shields.io/github/downloads/iSnackyCracky/KeePassRDP/total?style=flat-square">

# KeePassRDP
## Overview
KeePassRDP is a plugin for KeePass 2.x which adds multiple options to connect via RDP to the URL of an entry.

## Installation
1. Download the zip file from the newest [release](https://github.com/iSnackyCracky/KeePassRDP/releases)
2. Unzip and copy the KeePassRDP.plgx file to your KeePass plugins folder.

## Usage
To connect via RDP to a machine, select the entry containing the IP-address or hostname, right-click and select *KeePassRDP* \> *Open RDP connection* (or just press <kbd>CTRL</kbd> + <kbd>M</kbd>).

To use the other connection options, just select the corresponding entries in the context-menu.

## Features
- Connect to host via RDP
- Connect to host via RDP admin session (`mstsc.exe /admin` parameter)
- Customize `mstsc.exe` parameters (`/f`, `/span`, `/multimon`, `/w`, `/h`)
- Gather and show possible Windows or domain credentials when the connection entry is inside a group called "RDP" (see below for details)


### RDP subgroup / folder
This is how I use the extension on a daily basis (I work for an MSP where we store credentials for customer domains or machines inside KeePass):

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

If we now want to connect to one of the machines in the RDP subgroup (with credential usage), just select the machine-entry, press <kbd>CTRL</kbd> + <kbd>M</kbd> and KeePassRDP shows you a dialog with viable account-entries (with titles like e.g. *domain-admin*, *local user*, ...) it always ignores entries where the title contains **[rdpignore]** or where a custom field named **rdpignore** is created with a value not equal to *false* (not case-sensitive).
This "ignore-flag" can be toggled via the KeePassRDP context menu since v1.9.0.

![credential selection dialog](https://isnackycracky.github.io/KeePassRDP/img/credential_picker.jpg)

Now just select the entry you want and click ok (or press <kbd>Enter</kbd>).

## How it works
The plugin basically just calls the default `mstsc.exe` with the `/v:<address>` (and optionally other) parameter(s) to connect.

If you choose to open a connection *with credentials* it stores the credentials into the Windows Credential Manager ("Vault") for usage by the `mstsc.exe` process.

These Credentials then get removed again after about 10 seconds.

## Third-party Software
This plugin uses the following third-party libraries:
- the *awesome* "ListView" wrapper [**ObjectListView**](http://objectlistview.sourceforge.net/cs/index.html) by Phillip Piper
- the *awesome* "Windows Credential Management API" wrapper [**CredentialManagement**](https://github.com/ilyalozovyy/credentialmanagement) by [iLya Lozovyy](https://github.com/ilyalozovyy)
