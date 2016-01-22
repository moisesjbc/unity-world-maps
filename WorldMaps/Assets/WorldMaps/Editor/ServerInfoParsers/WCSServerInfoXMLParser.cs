using UnityEngine;
using System.Xml;

public class WCSServerInfoXMLParser {
	
	public static WCSServerInfo GetWCSServerInfo( string xmlString ){
		XmlDocument xmlDocument = new XmlDocument ();
		xmlDocument.LoadXml (xmlString);

		//XmlNode rootNode = xmlDocument.DocumentElement;

		// Parse server general info.
		//string serverTitle = rootNode.SelectSingleNode("Service").SelectSingleNode ("Title").InnerText;

		return new WCSServerInfo( "<server label>" );
	}
}
