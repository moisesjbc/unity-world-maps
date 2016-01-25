using UnityEngine;
using System.Xml;

public class WCSServerInfoXMLParser {
	
	public static WCSServerInfo GetWCSServerInfo( string xmlString ){
		XmlDocument xmlDocument = new XmlDocument ();
		xmlDocument.LoadXml (xmlString);

		XmlNode rootNode = xmlDocument.DocumentElement;

		XmlNamespaceManager namespacesManager = new XmlNamespaceManager(xmlDocument.NameTable);
		namespacesManager.AddNamespace("wcs", "http://www.opengis.net/wcs");

		// Parse server general info.
		string serverLabel = rootNode.SelectSingleNode ("wcs:Service", namespacesManager).SelectSingleNode ("wcs:label", namespacesManager).InnerText;

		return new WCSServerInfo( serverLabel );
	}
}
