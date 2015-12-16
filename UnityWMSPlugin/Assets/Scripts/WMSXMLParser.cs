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
		WMSLayer[] layers = parseWMSLayers( rootNode.SelectSingleNode("Capability").SelectNodes ("Layer") );

		return new WMSInfo(layers);
	}


	private static WMSLayer[] parseWMSLayers( XmlNodeList layersNodes )
	{
		WMSLayer[] layers = null;
		if (layersNodes.Count > 0) {
			layers = new WMSLayer[layersNodes.Count];
			int i = 0;
			foreach (XmlNode layerNode in layersNodes) {
				layers [i] = parseWMSLayer (layerNode);
				i++;
			}

			return layers;
		}
		return layers;
	}


	private static WMSLayer parseWMSLayer( XmlNode layerXmlNode )
	{
		WMSLayer layer = new WMSLayer();

		layer.title = layerXmlNode.SelectSingleNode("Title").InnerText;

		Debug.Log ("layer.title: " + layer.title);
		layer.subLayers = parseWMSLayers (layerXmlNode.SelectNodes ("Layer"));

		return layer;
	}

}
