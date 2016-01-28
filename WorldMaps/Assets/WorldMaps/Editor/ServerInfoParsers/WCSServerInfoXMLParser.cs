using UnityEngine;
using System.Xml;

public class WCSServerInfoXMLParser {
	
	public static WCSServerInfo GetWCSServerInfo( string xmlString ){
		XmlDocument xmlDocument = new XmlDocument ();
		xmlDocument.LoadXml (xmlString);

		XmlNode rootNode = xmlDocument.DocumentElement;

		XmlNamespaceManager namespacesManager = new XmlNamespaceManager(xmlDocument.NameTable);
		namespacesManager.AddNamespace("wcs", "http://www.opengis.net/wcs");
		namespacesManager.AddNamespace ("gml", "http://www.opengis.net/gml");

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
			Debug.LogFormat ("Bounding boxes for coverage [{0}]", layerLabel);
			BoundingBox[] boundingBoxes = ParseBoundingBoxes (layerNode.SelectNodes ("wcs:lonLatEnvelope", namespacesManager), namespacesManager);
			coverages [i] = new WCSCoverage(layerLabel, layerName, boundingBoxes);
			i++;
		}
			
		return coverages;
	}


	private static BoundingBox[] ParseBoundingBoxes(XmlNodeList boundingBoxNodes, XmlNamespaceManager namespacesManager)
	{
		BoundingBox[] boundingBoxes = new BoundingBox[boundingBoxNodes.Count];

		int i = 0;
		foreach (XmlNode boundingBoxNode in boundingBoxNodes) {
			XmlNodeList coordinatesNodes = boundingBoxNode.SelectNodes ("gml:pos", namespacesManager);

			boundingBoxes [i] = new BoundingBox ();
			boundingBoxes[i].bottomLeftCoordinates = ParseVector2 (coordinatesNodes.Item (0));
			boundingBoxes[i].topRightCoordinates = ParseVector2 (coordinatesNodes.Item (1));

			i++;
		}

		return boundingBoxes;
	}


	private static Vector2 ParseVector2(XmlNode vectorNode)
	{
		Vector2 result = Vector2.zero;

		string[] tokens = vectorNode.InnerText.Split (new char[]{ ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
		result [0] = float.Parse (tokens [0]);
		result [1] = float.Parse (tokens [1]);

		return result;
	}
}
