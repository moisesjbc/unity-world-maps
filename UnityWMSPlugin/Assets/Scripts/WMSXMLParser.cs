using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Linq;

public class WMSXMLParser {
	
	public static WMSInfo GetWMSInfo( string xmlString ){
		XmlDocument xmlDocument = new XmlDocument ();
		xmlDocument.LoadXml (xmlString);

		XmlNode rootNode = xmlDocument.DocumentElement;

		// Parse WMS layers.
		List<WMSLayer> layers = new List<WMSLayer> ();
		parseWMSLayers( rootNode.SelectSingleNode("Capability").SelectNodes ("Layer"), ref layers, null );

		return new WMSInfo(layers.ToArray ());
	}


	public static void Update(WMSInfo info, string filepath)
	{
		XmlDocument xmlDocument = new XmlDocument ();
		xmlDocument.LoadXml (File.ReadAllText(filepath));

		XmlNode rootNode = xmlDocument.DocumentElement;

		updateWMSLayers (xmlDocument, rootNode.SelectSingleNode ("Capability").SelectNodes ("Layer"), info.layers.ToList() );

		xmlDocument.Save (filepath);
	
		Debug.Log ("XML updated (" + filepath + ")");
	}


	private static void updateWMSLayers( XmlDocument xmlDocument, XmlNodeList layersNodes, List<WMSLayer> layers )
	{
		if (layersNodes.Count > 0) {
			foreach (XmlNode layerNode in layersNodes) {
				updateWMSLayer ( xmlDocument, layerNode, layers);
			}
		}
	}


	private static void updateWMSLayer( XmlDocument xmlDocument, XmlNode layerXmlNode, List<WMSLayer> layers )
	{
		if (layerXmlNode != null) {
			if (layerXmlNode.Attributes ["queryable"] != null && layerXmlNode.Attributes ["queryable"].InnerText == "1") {
				// Update the "selected" layer attribute in XML.
				XmlAttribute layerSelectedAttribute = xmlDocument.CreateAttribute("selected");
				bool layerSelected = layers.Find (element => element.title == layerXmlNode.SelectSingleNode ("Title").InnerText).selected;
				layerSelectedAttribute.Value = layerSelected ? "1" : "0";
				layerXmlNode.Attributes.Append (layerSelectedAttribute);
			}
			updateWMSLayers (xmlDocument, layerXmlNode.SelectNodes ("Layer"), layers);
		}
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
		if (layerXmlNode != null ) {
			WMSLayer layer = new WMSLayer ();

			layer.title = layerXmlNode.SelectSingleNode ("Title").InnerText;

			if (layerXmlNode.SelectSingleNode ("Name") != null) {
				layer.name = layerXmlNode.SelectSingleNode ("Name").InnerText;
			} else {
				layer.name = "";
			}
			layer.boundingBoxes = parseWMSBoundingBoxes (layerXmlNode.SelectNodes ("BoundingBox"));
			layer.parentLayer = parentLayer;

			layer.selected = (layerXmlNode.Attributes ["selected"] != null && layerXmlNode.Attributes ["selected"].InnerText == "1");
			if (layerXmlNode.SelectSingleNode ("Name") != null) {
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
		
			boundingBoxes.Add ( boundingBox );
		}

		return boundingBoxes;
	}
}
