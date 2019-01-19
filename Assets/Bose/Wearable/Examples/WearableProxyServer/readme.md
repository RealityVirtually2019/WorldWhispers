# Wearable Proxy

The WearableProxy is a data provider that allows users to receive motion using a mobile proxy app, rather than connecting to a device directly. This facilitates seamless in-editor testing without having to repeatedly build to a mobile device. All functions of the plugin, including device searching and connecting, sensor configuration, and data acquisition are passed on through the proxy completely transparently, allowing for full in-editor testing functionality.

## Building the Wearable Proxy Server

In order to use the proxy, a companion server app must be built and loaded onto a supported mobile device. To do this:

1. Change the build settings to include only the scene located at `Bose/Wearable/Examples/WearableProxyServer/Scenes/ProxyServer.unity`. 
2. Build the app as normal, and install it onto a mobile device. 

It is recommended to change the display name and bundle identifier of the server app in Xcode to disambiguate the server from the demo application.

To return the demo app to its original functionality, remove the proxy server scene and reactivate the original demo scenes.

In future versions, a build script will be provided that performs these steps automatically.

## Using the Wearable Proxy

To use the Wearable Proxy with the included demos: 

* Find the "Wearable Control" object in the `Root` scene and change the "Editor Default Provider" field to "Wearable Proxy". 
* Launch and start the proxy server app on the mobile device and note the IP and port number displayed, then enter them in the inspector. 
* Press play in the editor and the plugin will automatically connect to the server and transparently pass on commands. 
  * Keep in mind that the wearable device should stay in proximity to your mobile device running the server, and not the client computer running the demo application.

Using the Wearable Proxy with your own apps is similar: ensure a GameObject with the Wearable Control component exists in your scene and set the default editor provider to the proxy, then enter the server information.

Keep in mind that the server application must be started _before_ running your application in-editor, or the plugin will not be able to connect.

## Known Issues

There is a known issue with the current release of the Wearable Proxy: after halting play in the editor with a device still connected, subsequent plays will not register the device in the connection panel. To fix this, stop and start the server app between plays in the editor. This will be resolved in a future release.
