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

		// Parse server coverages.
		WCSCoverage[] coverages = ParseCoverages(rootNode.SelectSingleNode ("wcs:ContentMetadata",namespacesManager).SelectNodes("wcs:CoverageOfferingBrief",namespacesManager), namespacesManager);

		return new WCSServerInfo( serverLabel, coverages );
	}
		

	private static WCSCoverage[] ParseCoverages(XmlNodeList coverageNodes, XmlNamespaceManager namespacesManager)
	{
		WCSCoverage[] coverages = new WCSCoverage[coverageNodes.Count];

		int i = 0;
		foreach( XmlNode layerNode in coverageNodes ){
			string layerLabel = layerNode.SelectSingleNode ("wcs:label", namespacesManager).InnerText;
			string layerName = layerNode.SelectSingleNode ("wcs:name", namespacesManager).InnerText;
			coverages [i] = new WCSCoverage(layerLabel, layerName);
			i++;
		}

		Debug.LogWarning ("Parsed layers");

		return coverages;
	}
}
