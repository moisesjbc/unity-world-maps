# unity-wms-plugin

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

