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
		parseWMSLayers( rootNode.SelectSingleNode("Capability").SelectNodes ("Layer"), ref layers );

		return new WMSInfo(layers.ToArray ());
	}


	private static void parseWMSLayers( XmlNodeList layersNodes, ref List<WMSLayer> layers )
	{
		if (layersNodes.Count > 0) {
			foreach (XmlNode layerNode in layersNodes) {
				parseWMSLayer (layerNode, ref layers);
			}
		}
	}


	private static void parseWMSLayer( XmlNode layerXmlNode, ref List<WMSLayer> layers )
	{
		if (layerXmlNode.SelectNodes ("Layer").Count == 0) {
			WMSLayer layer = new WMSLayer();

			layer.title = layerXmlNode.SelectSingleNode("Title").InnerText;

			Debug.Log ("Parsing layer [" + layer.title + "] ...");
			layer.name = layerXmlNode.SelectSingleNode("Name").InnerText;

			XmlNode boundingBoxXmlNode = layerXmlNode.SelectSingleNode ("BoundingBox");
			
			layer.bottomLeftCoordinates.x = float.Parse (boundingBoxXmlNode.Attributes ["minx"].InnerText);
			layer.bottomLeftCoordinates.y = float.Parse (boundingBoxXmlNode.Attributes ["miny"].InnerText);
			layer.topRightCoordinates.x = float.Parse (boundingBoxXmlNode.Attributes ["maxx"].InnerText);
			layer.topRightCoordinates.y = float.Parse (boundingBoxXmlNode.Attributes ["maxy"].InnerText);
		
			layer.boundingBoxSRS = boundingBoxXmlNode.Attributes["SRS"].InnerText;

			Debug.Log ("Parsing layer [" + layer.title + "] ...OK");
			layers.Add (layer);
		} else {
			parseWMSLayers (layerXmlNode.SelectNodes ("Layer"), ref layers);
		}
	}

}
