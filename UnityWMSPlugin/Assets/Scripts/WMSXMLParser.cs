using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class WMSXMLParser {
	
	public static WMSInfo GetWMSInfo( string xmlString ){
		Debug.Log(xmlString);

		XmlDocument xmlDocument = new XmlDocument ();
		xmlDocument.LoadXml (xmlString);

		XmlNode rootNode = xmlDocument.DocumentElement;

		// Parse WMS layers.
		List<WMSLayer> layers = new List<WMSLayer> ();
		parseWMSLayers( rootNode.SelectSingleNode("Capability").SelectNodes ("Layer"), ref layers, null );

		return new WMSInfo(layers.ToArray ());
	}


	private static void parseWMSLayers( XmlNodeList layersNodes, ref List<WMSLayer> layers, WMSLayer parentLayer )
	{
		if (layersNodes.Count > 0) {
			foreach (XmlNode layerNode in layersNodes) {
				parseWMSLayer (layerNode, ref layers, parentLayer);
			}
		}
	}


	private static void parseWMSLayer( XmlNode layerXmlNode, ref List<WMSLayer> layers, WMSLayer parentLayer )
	{
		if (layerXmlNode != null) {
			WMSLayer layer = new WMSLayer ();

			layer.title = layerXmlNode.SelectSingleNode ("Title").InnerText;

			Debug.Log ("Parsing layer [" + layer.title + "] ...");
			if( layerXmlNode.SelectSingleNode ("Name") != null ){
				layer.name = layerXmlNode.SelectSingleNode ("Name").InnerText;
			}else{
				layer.name = "";
			}
			layer.boundingBoxes = parseWMSBoundingBoxes (layerXmlNode.SelectNodes ("BoundingBox"));
			layer.parentLayer = parentLayer;
			Debug.Log ("Parsing layer [" + layer.title + "] ...OK");

			if (layerXmlNode.Attributes ["queryable"] != null && layerXmlNode.Attributes ["queryable"].InnerText == "1") {
				layers.Add (layer);
			}
			parseWMSLayers (layerXmlNode.SelectNodes ("Layer"), ref layers, layer);
		}
	}


	private static List<WMSBoundingBox> parseWMSBoundingBoxes( XmlNodeList bbXmlNodes )
	{
		List<WMSBoundingBox> boundingBoxes = new List<WMSBoundingBox>();

		foreach (XmlNode bbXmlNode in bbXmlNodes) {
			WMSBoundingBox boundingBox = new WMSBoundingBox();

			boundingBox.SRS = bbXmlNode.Attributes ["SRS"].InnerText;
			boundingBox.bottomLeftCoordinates.x = float.Parse (bbXmlNode.Attributes ["minx"].InnerText);
			boundingBox.bottomLeftCoordinates.y = float.Parse (bbXmlNode.Attributes ["miny"].InnerText);
			boundingBox.topRightCoordinates.x = float.Parse (bbXmlNode.Attributes ["maxx"].InnerText);
			boundingBox.topRightCoordinates.y = float.Parse (bbXmlNode.Attributes ["maxy"].InnerText);
		
			Debug.Log ("Parsing bounding box: " + boundingBox.ToString ());
			boundingBoxes.Add ( boundingBox );
		}

		return boundingBoxes;
	}
}
