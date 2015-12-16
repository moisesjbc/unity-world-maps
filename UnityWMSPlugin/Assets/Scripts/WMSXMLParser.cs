using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class WMSXMLParser {
	
	public static List<string> GetLayers( string xmlString ){
		Debug.Log(xmlString);
		List<string> layers = new List<string>();

		XmlDocument xmlDocument = new XmlDocument ();
		xmlDocument.LoadXml (xmlString);

		XmlNode rootNode = xmlDocument.DocumentElement;

		XmlNodeList layersNodes = rootNode.SelectSingleNode("Capability").SelectNodes ("Layer");

		foreach( XmlNode layerNode in layersNodes ){
			string layerTitle = layerNode.SelectSingleNode("Title").InnerText;

			XmlNodeList subLayersNodes = layerNode.SelectNodes ("Layer");

			foreach( XmlNode subLayerNode in subLayersNodes ){
				Debug.Log ( subLayerNode.SelectSingleNode("Title").InnerText );
				layers.Add ( layerTitle + "/" + subLayerNode.SelectSingleNode("Title").InnerText );
			}
		}

		return layers;
	}

}
