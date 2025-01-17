# Seq.App.AzureSecretCheck

![alt text](./src/AzureSecretCheck/Assets/AzureSecretCheckLogo.png "Title")

Azure App Registration Secret Expiration Check.

## Description

This plug-in checks the secrets and certificates of each of the apps specified and returns the days till expiration of the most recent date.

## Installation

This plug-in requires an app registration with a client secret that has `Application.Read.All` access in your Azure Instance.

## Inputs

|Required/Optional|Variable|Type|Description|
|--|--|--|--|
|Required|Tenant Id|string|Directory (tenant) ID of the Azure Instance|
|Required|Client Id|string|The Client ID of this App Registration, that has the necessary access to query expiry|
|Required|Client Secret|string|Client secret from the App Registration for this plugin|
|Optional|Application Object Ids|string[]|A list of App Registration Object Ids, one per line. If left blank, all object ids will be reviewed. _**Note:** This is the Object Id and not the Application Id._|
|Optional|Graph Scopes|string[]|A list of Graph Scopes for the Microsoft.Graph api. Default is blank|
|Optional|Interval (seconds)|int|The time between checks; the default is 3600 (once an hour).|
|Optional|Minimum validity period (days)|int|The minimum amount of days a certificate should be valid; the default is 30|

## Change Log
### 2.1.4 - 2025/01/14
- Application Object Ids are now optional.  If the Application Object Ids are empty, all object Ids will be reviewed.
- Line Comments (//) and Block Comments (/* */) are allowed in the Application Object Ids.

## Authors
- Tony Clark [@TheCriticalPath](https://github.com/TheCriticalPath)