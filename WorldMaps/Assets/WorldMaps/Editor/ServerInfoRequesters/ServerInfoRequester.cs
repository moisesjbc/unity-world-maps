using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



class ServerTransaction <ResponseType>
	where ResponseType : class
{
	public delegate ResponseType ParsingFunction(string responseText);

	public WWW request = null;
	public ResponseType response = null;
	public string errorLog = null;


	public ServerTransaction(string url)
	{
		request = new WWW (url);
	}


	public RequestStatus Update( ParsingFunction ParseResponse )
	{
		if (request.isDone) {
			if (request.error == null) {
				try{
					response = ParseResponse (request.text);
					return RequestStatus.OK;
				}catch( Exception e ){
					// Parsing error
					this.errorLog = "Couldn't parse response from server: " + e.Message;
					return RequestStatus.ERROR;
				}
			} else {
				// Connection error
				this.errorLog = "Couldn't get info from server: " + request.error;
				return RequestStatus.ERROR;
			}
		} else {
			return RequestStatus.DOWNLOADING;
		}
	}
}
	

public enum RequestStatus
{
	DOWNLOADING,
	OK,
	ERROR
}


public abstract class ServerInfoRequester <ResponseType>
	where ResponseType : class
{
	Dictionary<string, ServerTransaction<ResponseType>> transactions = 
		new Dictionary<string, ServerTransaction<ResponseType>>();


	public string RequestWMSInfo( string serverURL )
	{
		string requestID = GenerateRequestID (serverURL);
		if (!transactions.ContainsKey (requestID) || transactions[requestID].errorLog != null){
			transactions [requestID] = new ServerTransaction<ResponseType> (serverURL);
		}
		return requestID;
	}


	public RequestStatus Update( string requestID )
	{
		return transactions [requestID].Update (ParseResponse);
	}


	/*
	public WMSRequest GetRequest( string requestID )
	{
		return requests_ [requestID];
	}
	*/
		

	protected string GenerateRequestID( string serverURL )
	{
		return serverURL;
	}


	protected abstract ResponseType ParseResponse( string responseText );
}
