using UnityEngine;
using System.Xml;

public class WCSServerInfoXMLParser {
	
	public static WCSServerInfo GetWCSServerInfo( string xmlString ){
		XmlDocument xmlDocument = new XmlDocument ();
		xmlDocument.LoadXml (xmlString);

		XmlNode rootNode = xmlDocument.DocumentElement;

		// Parse server general info.
		string serverLabel = rootNode.SelectSingleNode(LocalName("Service")).SelectSingleNode (LocalName("label")).InnerText;

		return new WCSServerInfo( serverLabel );
	}


	private static string LocalName(string name)
	{
		return "//*[local-name()='" + name + "']";
	}
}
