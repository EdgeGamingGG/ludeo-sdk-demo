/*
This file is part of Cohtml, Gameface and Prysm - modern user interface technologies.

Copyright (c) 2012-2023 Coherent Labs AD and/or its licensors. All
rights reserved in all media.

The coded instructions, statements, computer programs, and/or related
material (collectively the "Data") in these files contain confidential
and unpublished information proprietary Coherent Labs and/or its
licensors, which is protected by United States of America federal
copyright law and by international treaties.

This software or source code is supplied under the terms of a license
agreement and nondisclosure agreement with Coherent Labs AD and may
not be copied, disclosed, or exploited except in accordance with the
terms of that agreement. The Data may not be disclosed or distributed to
third parties, in whole or in part, without the prior written consent of
Coherent Labs AD.

COHERENT LABS MAKES NO REPRESENTATION ABOUT THE SUITABILITY OF THIS
SOURCE CODE FOR ANY PURPOSE. THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT
HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
MERCHANTABILITY, NONINFRINGEMENT, AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER, ITS AFFILIATES,
PARENT COMPANIES, LICENSORS, SUPPLIERS, OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
ANY WAY OUT OF THE USE OR PERFORMANCE OF THIS SOFTWARE OR SOURCE CODE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using cohtml.Net;
using UnityEngine;
using UnityEngine.Networking;

namespace cohtml
{
public class DefaultResourceHandler : ResourceHandler,
	IResourceHandler,
	ISystemStorable,
	ILocationsSearchable
{
	public const string Coui = "coui";
	public const string CouiProtocol = Coui + "://";
	public const string UIResourcesPath = Cohtml + "UIResources";
	public const string PreloadedHost = "preloaded";
	public const string Cohtml = "Cohtml/";
	public const string SharedImagesPath = Cohtml + "SharedImages";
	private const string ResourceHandlerString = "ResourceHandler: ";
	private const string UIResourcesHost = "uiresources";

	private abstract class RequestData
	{
		readonly string[] ImageExtensions =
			{ ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".psd", ".tga", ".astc", ".pkm", ".dds", ".ktx" };

		public enum PathFormat
		{
			Local,
			Web
		};

		internal uint Id;
		internal UriBuilder UriBuilder;
		internal bool IsAbsolutePath;
		internal UnityWebRequest UnityRequest;
		internal IResourceRequest CohtmlRequest;
		internal string Error;

		protected RequestData(IResourceRequest request)
		{
			CohtmlRequest = request;
			SetupUri();
		}

		private void SetupUri()
		{
			string url = CohtmlRequest.GetURL().Replace('\\', Path.AltDirectorySeparatorChar);
			UriBuilder uri = new UriBuilder(UnityWebRequest.UnEscapeURL(url));
			IsAbsolutePath = Path.IsPathRooted(url);
			if (uri.Scheme == Uri.UriSchemeFile || IsAbsolutePath)
			{
				LogHandler.LogWarning($"It's not a good practice to use \"{uri}\". Use build's relative paths with \"coui://\" protocol.");
			}

			UriBuilder = uri;
		}

		internal bool IsLocalRequest
		{
			get { return UriBuilder.Scheme == Coui || UriBuilder.Scheme == Uri.UriSchemeFile; }
		}

		internal bool IsPreloadedRequest
		{
			get { return UriBuilder.Host == PreloadedHost; }
		}

		internal bool IsImageRequest
		{
			get
			{
				string fileExtension = Path.GetExtension(UriBuilder.Path);
				return Array.FindIndex(ImageExtensions, extension => extension == fileExtension) != -1;
			}
		}

		internal void SetUnityRequest(string path)
		{
			Id = CohtmlRequest.GetId();
			UnityRequest = new UnityWebRequest
			{
				method = CohtmlRequest.GetMethod(),
				downloadHandler = new DownloadHandlerBuffer(),
				uri = new Uri(ToFullPath(path, RequestData.PathFormat.Web))
			};

			string requestData = CohtmlRequest.GetBody();
			if (requestData != null)
			{
				UnityRequest.uploadHandler = new UploadHandlerRaw(Utils.EncodeString(requestData));
				UnityRequest.SetRequestHeader("Content-Type", "application/json");
			}

			uint headersCount = CohtmlRequest.GetHeadersCount();
			for (uint i = 0; i < headersCount; i++)
			{
				requestData = CohtmlRequest.GetHeaderName(i);
				UnityRequest.SetRequestHeader(requestData, CohtmlRequest.GetHeaderValue(requestData));
			}
		}

		public string ToFullPath(string rootPath, PathFormat format)
		{
			if (!IsLocalRequest || IsAbsolutePath)
			{
				return UriBuilder.Uri.AbsoluteUri;
			}

			if (IsPreloadedRequest)
			{
				return rootPath;
			}

			#if !PLATFORM_ANDROID
			if (format == PathFormat.Web)
			{
				rootPath = $"{Uri.UriSchemeFile}:///{rootPath}";
			}
			#endif

			return Path.Combine(rootPath, UriBuilder.Uri.LocalPath.Trim(Path.AltDirectorySeparatorChar));
		}

		private void LogErrorMessage(string error)
		{
			LogHandler.LogError($"{ResourceHandlerString}{error}\r\n	URL: {UriBuilder}");
		}

		internal virtual void RespondWithFailure(string error)
		{
			LogErrorMessage(error);
		}

		public abstract void RespondWithSuccess();

		public void AbortPendingUnityRequest()
		{
			if (UnityRequest != null && !UnityRequest.isDone && !string.IsNullOrEmpty(UnityRequest.url))
			{
				UnityRequest.Abort();
				LogHandler.Log($"Request URL:{UnityRequest.url} aborted.");
			}
		}
	}

	private class ResourceRequestData : RequestData
	{
		internal IResourceResponse Response;

		public ResourceRequestData(IResourceRequest request, IResourceResponse response)
			: base(request)
		{
			Response = response;
		}

		internal override void RespondWithFailure(string error)
		{
			base.RespondWithFailure(error);
			Response.Finish(ResourceResponse.Status.Failure);
		}

		public override void RespondWithSuccess()
		{
			Response.Finish(ResourceResponse.Status.Success);
		}
	}

	private class ResourceStreamRequestData : RequestData
	{
		internal IResourceStreamResponse Response;

		public ResourceStreamRequestData(IResourceRequest request, IResourceStreamResponse response)
			: base(request)
		{
			Response = response;
		}

		internal override void RespondWithFailure(string error)
		{
			base.RespondWithFailure(error);
			Response.Finish(ResourceStreamResponse.Status.Failure);
		}

		public override void RespondWithSuccess()
		{
			Response.Finish(ResourceStreamResponse.Status.Success);
		}
	}

	static KeyValuePair<string, List<string>> _defaultHostLocations;

	List<RequestData> m_PendingRequests;
	IDictionary<string, List<string>> m_HostLocationsMap;

	public DefaultResourceHandler()
	{
		m_PendingRequests = new List<RequestData>();
		_defaultHostLocations = new KeyValuePair<string, List<string>>(UIResourcesHost,
			new List<string> { Path.Combine(CohtmlUISystem.StreamingAssetsPath, UIResourcesPath) });

		m_HostLocationsMap = new Dictionary<string, List<string>>();
		m_HostLocationsMap.Add(_defaultHostLocations);
	}

	public static KeyValuePair<string, List<string>> DefaultHostLocations
	{
		get { return _defaultHostLocations; }
	}

	public CohtmlUISystem System { get; set; }

	public IDictionary<string, List<string>> HostLocationsMap
	{
		get { return m_HostLocationsMap; }
		set { m_HostLocationsMap = value; }
	}

	public int PendingRequestsCount => m_PendingRequests.Count;

	public override void OnResourceRequest(IResourceRequest request, IResourceResponse response)
	{
		try
		{
			ResourceRequestData requestData = new ResourceRequestData(request, response);
			System.StartCoroutine(TryPreloadedTextureRequestAsync(requestData));
		}
		catch (SystemException error)
		{
			response.Finish(ResourceResponse.Status.Failure);
			LogHandler.LogError($"{ResourceHandlerString}{error.Message}\r\n	URL: {request.GetURL()}");
		}
	}

	public override void OnResourceStreamRequest(IResourceRequest request, IResourceStreamResponse response)
	{
		try
		{
			ResourceStreamRequestData requestData = new ResourceStreamRequestData(request, response);
			System.StartCoroutine(RequestResourceStreamAsync(requestData));
		}
		catch (SystemException error)
		{
			response.Finish(ResourceStreamResponse.Status.Failure);
			LogHandler.LogError($"{ResourceHandlerString}{error.Message}\r\n	URL: {request.GetURL()}");
		}
	}

	public override void OnAbortResourceRequest(uint id)
	{
		RequestData abortedRequest = m_PendingRequests.Find(x => x.Id == id);
		if (abortedRequest != null)
		{
			abortedRequest.RespondWithFailure($"{abortedRequest.UnityRequest.url} aborted.");
			RemovePendingRequest(abortedRequest);
		}
		else
		{
			LogHandler.LogError($"Cannot abort request with ID: {id}. It's doesn't exist.");
		}
	}

	public override void Dispose()
	{
		if (System != null)
		{
			System.StopAllCoroutines();
		}

		AbortAllPendingRequests();

		base.Dispose();
	}

	IEnumerator RequestResourceAsync(ResourceRequestData requestData)
	{
		if (!requestData.IsLocalRequest || (requestData.IsLocalRequest && requestData.IsAbsolutePath))
		{
			yield return RequestAsync(requestData, requestData.UriBuilder.Uri.AbsoluteUri);
			CheckForFailedRequest(requestData);
			yield break;
		}

		List<string> rootPaths;
		m_HostLocationsMap.TryGetValue(requestData.UriBuilder.Host, out rootPaths);
		if (rootPaths == null || rootPaths.Count == 0)
		{
			requestData.Error = "Invalid host locations map.";
			CheckForFailedRequest(requestData);
			yield break;
		}

		foreach (string path in rootPaths)
		{
			yield return RequestAsync(requestData, path);
			if (string.IsNullOrEmpty(requestData.Error))
			{
				yield break;
			}
		}

		CheckForFailedRequest(requestData);
	}

	IEnumerator RequestResourceStreamAsync(ResourceStreamRequestData requestData)
	{
		if (!requestData.IsLocalRequest || requestData.IsPreloadedRequest || (requestData.IsLocalRequest && requestData.IsAbsolutePath))
		{
			yield return RequestStreamAsync(requestData, requestData.UriBuilder.Uri.AbsoluteUri);
			yield break;
		}

		List<string> rootPaths;
		m_HostLocationsMap.TryGetValue(requestData.UriBuilder.Host, out rootPaths);
		if (rootPaths == null || rootPaths.Count == 0)
		{
			requestData.Error = "Invalid host locations map.";
			CheckForFailedRequest(requestData);
			yield break;
		}

		foreach (string path in rootPaths)
		{
			yield return RequestStreamAsync(requestData, path);
			if (string.IsNullOrEmpty(requestData.Error))
			{
				yield break;
			}
		}

		CheckForFailedRequest(requestData);
	}

	IEnumerator TryPreloadedTextureRequestAsync(ResourceRequestData requestData)
	{
		if (IsLiveViewRequest(requestData.UriBuilder.Uri.AbsoluteUri))
		{
			RequestLiveViewResource(requestData);
			yield break;
		}

		if (!requestData.IsLocalRequest ||
		    !(requestData.UriBuilder.Host == PreloadedHost || requestData.IsImageRequest))
		{
			yield return System.StartCoroutine(RequestResourceAsync(requestData));
			yield break;
		}

		UnityEngine.ResourceRequest request =
			Resources.LoadAsync<Texture>(Utils.CombinePaths(SharedImagesPath,
				Path.GetFileNameWithoutExtension(requestData.UriBuilder.Path)));

		yield return request;

		if (request.asset == null)
		{
			yield return System.StartCoroutine(RequestResourceAsync(requestData));
			yield break;
		}

		Texture texture = request.asset as Texture;
		if (!texture)
		{
			requestData.RespondWithFailure($"{ResourceHandlerString}: Cannot load preloaded image named {request.asset.name}");

			yield break;
		}

		ResourceResponse.UserImageData data = System.UserImagesManager.CreateUserImageData(texture);
		requestData.Response.ReceiveUserImage(data);
		RespondWithSuccess(requestData);
		RemovePendingRequest(requestData);

		LogHandler.Log($"Preloaded image loaded: {texture.name}");
		System.UserImagesManager.AddPreloadedTexture(data.Texture, texture);
	}

	void RequestLiveViewResource(ResourceRequestData requestData)
	{
		ResourceResponse.UserImageData data =
			System.UserImagesManager.CreateLiveViewImageData(requestData.UriBuilder.Uri.AbsoluteUri);
		if (data != null)
		{
			requestData.Response.ReceiveUserImage(data);
			requestData.RespondWithSuccess();
		}
		else
		{
			requestData.RespondWithFailure($"Cannot find live View Url: {requestData.UriBuilder.Uri}");
		}
	}

	bool IsLiveViewRequest(string uri)
	{
		return System.UserImagesManager.ContainsLiveView(uri);
	}

	IEnumerator RequestAsync(ResourceRequestData requestData, string path)
	{
		requestData.SetUnityRequest(path);
		using (requestData.UnityRequest)
		{
			AddPendingRequest(requestData);
			yield return requestData.UnityRequest.SendWebRequest();

			// Already aborted request. Skip responding twice
			if (requestData.UnityRequest == null)
			{
				yield break;
			}

			if (string.IsNullOrEmpty(requestData.UnityRequest.error))
			{
				if (requestData.UnityRequest.downloadedBytes == 0)
				{
					RespondWithSuccess(requestData);
					RemovePendingRequest(requestData);
					yield break;
				}

				byte[] bytes = requestData.UnityRequest.downloadHandler.data;

				requestData.Response.SetStatus((ushort)requestData.UnityRequest.responseCode);
				IntPtr data = requestData.Response.GetSpace((ulong)bytes.LongLength);
				Marshal.Copy(bytes, 0, data, bytes.Length);

				RespondWithSuccess(requestData);
				RemovePendingRequest(requestData);
			}
			else
			{
				requestData.Error = requestData.UnityRequest.error;
				RemovePendingRequest(requestData);
			}
		}
	}

	IEnumerator RequestStreamAsync(ResourceStreamRequestData requestData, string path)
	{
		if (requestData.IsLocalRequest)
		{
			string localPathOfRequest = path;
			if (requestData.IsPreloadedRequest)
			{
				localPathOfRequest = requestData.ToFullPath(path, RequestData.PathFormat.Local);
			}

			UnitySyncStreamReader unityStreamReader = FileReader.Reader.OpenFile(localPathOfRequest);

			if (unityStreamReader != null)
			{
				requestData.Response.SetStreamReader(unityStreamReader);
				RespondWithSuccess(requestData);
				RemovePendingRequest(requestData);

				yield break;
			}
		}

		requestData.SetUnityRequest(path);
		using (requestData.UnityRequest)
		{
			AddPendingRequest(requestData);
			yield return requestData.UnityRequest.SendWebRequest();

			// Already aborted request. Skip responding twice
			if (requestData.UnityRequest == null)
			{
				yield break;
			}

			if (string.IsNullOrEmpty(requestData.UnityRequest.error))
			{
				if (requestData.UnityRequest.downloadedBytes == 0)
				{
					RespondWithSuccess(requestData);
					RemovePendingRequest(requestData);
					yield break;
				}

				requestData.Response.SetStreamReader(new StreamReader(requestData.UnityRequest.downloadHandler.data));
				RespondWithSuccess(requestData);
				RemovePendingRequest(requestData);
				yield break;
			}

			requestData.Error = requestData.UnityRequest.error;
			RemovePendingRequest(requestData);
		}
	}

	void CheckForFailedRequest(RequestData requestData)
	{
		if (!string.IsNullOrEmpty(requestData.Error))
		{
			requestData.RespondWithFailure(requestData.Error);
			RemovePendingRequest(requestData);
		}
	}

	void RespondWithSuccess(RequestData requestData)
	{
		requestData.Error = string.Empty;
		requestData.RespondWithSuccess();
	}

	void AddPendingRequest(RequestData requestData)
	{
		m_PendingRequests.Add(requestData);
	}

	void RemovePendingRequest(RequestData requestToDelete)
	{
		if (requestToDelete == null)
		{
			return;
		}

		requestToDelete.AbortPendingUnityRequest();
		requestToDelete.UnityRequest = null;
		m_PendingRequests.Remove(requestToDelete);
	}

	void AbortAllPendingRequests()
	{
		foreach (RequestData request in m_PendingRequests)
		{
			request.RespondWithSuccess();
			request.AbortPendingUnityRequest();
		}

		m_PendingRequests.Clear();
	}
}
}
