# World maps

![Screenshot of World Maps](Images/world_maps_screenshot.png)

**World Maps** is a Unity package which allows users to add real world LOD maps to their scenes. Just drop the right plane prefab into the scene and configure it so its textures are retrieved from a [WMS](https://en.wikipedia.org/wiki/Web_Map_Service) or a [BingMaps](https://en.wikipedia.org/wiki/Bing_Maps) server.

## Adding world maps to the scene

### Adding a WMS map to the scene

**Note**: only WMS 1.1.0 has been tested.

1. Drag the prefab WorldMaps/Prefabs/WCSTerrain.prefab and drop it into the scene. A plane with a red question mark should appear.

  ![](Images/Tutorial/WMS/DroppingWMSPrefabIntoScene.png)

2. With the plane selected, a WMSComponent inspector should appear on the right. 

  ![](Images/Tutorial/WMS/WMSInspector.png)

4. Edit the inspector fields to meet your requirements:
  1. Select a server(\*) from the bookmarks list or enter a custom URL and wait a moment for the inspector to load the data from server. If any errors arises, it would be output to both the inspector and the console.
  2. Under the "Layers" panel, mark the layer(s) you want to display. Every time a layer is marked / unmarked, the plane texture is updated from server.
  3. Under the "Bounding box" panel, select a bounding box from the list provided by the server or insert a custom one. To see the changes in the scene view, the button "Update bounding box preview" must be pressed.

5. Enjoy the new scene with your map!

(\*) **Important:** Some WMS servers include restrictions about its use. Check them before using such servers in your projects.


## Using the BingMaps component

In order to use BingMaps you need one or two things:

- According to your specific app, **you may need to get permission from Microsoft** to use its API.
- Generating and using a BingMaps key.

For configuring the BingMaps module, do the following:

1. Get a BingMaps key by following the steps listed in ["Creating a Bing Maps Key"](https://msdn.microsoft.com/es-es/library/ff428642.aspx)
2. Visit <http://dev.virtualearth.net/REST/V1/Imagery/Metadata/Road?mapVersion=v1&output=xml&key=BingMapsKey>, **replacing BingMapsKey with your Bing Maps key**.
3. Visiting previous URL should return an XML file. Copy the image url returned in \<ImageUrl\>.
4. Paste the URL into the "Server URL" text field in BingMaps inspector.
4. Press "Update preview" to check that everything is OK.

