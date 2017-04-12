MpcNET
===========
.NET Client Library for [**Music Player Daemon**](https://www.musicpd.org/)

[![Build Status](https://travis-ci.org/glucaci/MpcNET.svg?branch=master)](https://travis-ci.org/glucaci/MpcNET)

## Installation

## Usage
### Connection
To create a client for MPD, you must first create a `IPEndPoint` for the Server with the right IP and Port. 
````C#
var mpdEndpoint = new IPEndPoint(IPAddress.Loopback, 6600);
````
Then create a Client and Connect to MPD.
````C#
var client = new Mpc(mpdEndpoint);
var connected = await client.ConnectAsync();
````
The `ConnectAsync()` method is returning a bool to indicate if the connection was successfully. However, this can be queried directly on the Client also:
````C#
var isConnected = client.IsConnected;
````
and for MPD version, additional property is available:
````C#
var mpdVersion = client.Version
````
To disconnect the Client use the follow method:
````C#
await client.DisconnectAsync();
````
### Send Command

## API Content
