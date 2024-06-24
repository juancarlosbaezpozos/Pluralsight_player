using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pluralsight.Domain.Authentication;
using Pluralsight.Domain.Persistance;
using Pluralsight.Domain.Serialization;

namespace Pluralsight.Domain;

public class RestHelper : IRestHelper
{
	private readonly string baseUrl;

	private readonly string refreshTokenResource;

	private readonly string userAgent;

	private readonly ILogger log;

	private readonly IUserRepository userRepository;

	public RestHelper(string baseUrl, IUserRepository userRepository, string refreshTokenResource, string userAgent, ILogger log)
	{
		this.baseUrl = baseUrl;
		this.userRepository = userRepository;
		this.refreshTokenResource = refreshTokenResource;
		this.userAgent = userAgent;
		this.log = log;
	}

	public async Task<RestResponse<T>> BasicGet<T>(string resource)
	{
		string requestUri = BuildUrl(resource);
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
		HttpClient client = HttpClientFactory.GetClient();
		AddDefaultHeaders(httpRequestMessage.Headers);
		return await AsyncSend<T>(client, httpRequestMessage);
	}

	public async Task<RestResponse<T>> AuthenticatedGet<T>(string resource)
	{
		RestResponse<T> restResponse = await AuthenticatedGet<T>(resource, userRepository.Load()?.AuthToken);
		if (restResponse.StatusCode == HttpStatusCode.Unauthorized)
		{
			return await AuthenticatedGet<T>(resource, await RefreshAuthenticationToken());
		}
		return restResponse;
	}

	private async Task<RestResponse<T>> AuthenticatedGet<T>(string resource, AuthenticationToken authenticationToken)
	{
		string requestUri = BuildUrl(resource);
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
		HttpClient client = HttpClientFactory.GetClient();
		AddDefaultHeaders(httpRequestMessage.Headers);
		httpRequestMessage.Headers.Add("ps-jwt", authenticationToken?.Jwt);
		return await AsyncSend<T>(client, httpRequestMessage);
	}

	private async Task<AuthenticationToken> RefreshAuthenticationToken()
	{
		User user = userRepository.Load();
		string resource = refreshTokenResource + user.DeviceInfo.DeviceId;
		RestResponse<DeviceAuthorizationResponse> restResponse = await BasicPost<DeviceAuthorizationResponse, RegisteredDevice>(resource, user.DeviceInfo);
		if (restResponse.StatusCode == HttpStatusCode.OK)
		{
			user.UserHandle = restResponse.Data.UserHandle;
			user.AuthToken = new AuthenticationToken
			{
				Expiration = restResponse.Data.Expiration,
				Jwt = restResponse.Data.Token
			};
			userRepository.Save(user);
		}
		else if (restResponse.StatusCode == HttpStatusCode.Unauthorized)
		{
			throw new RefreshTokenRevokedException();
		}
		return user.AuthToken;
	}

	public async Task<RestResponse<T>> AuthenticatedDelete<T>(string resource)
	{
		RestResponse<T> restResponse = await AuthenticatedDelete<T>(resource, userRepository.Load()?.AuthToken);
		if (restResponse.StatusCode == HttpStatusCode.Unauthorized)
		{
			return await AuthenticatedDelete<T>(resource, await RefreshAuthenticationToken());
		}
		return restResponse;
	}

	private async Task<RestResponse<T>> AuthenticatedDelete<T>(string resource, AuthenticationToken authenticationToken)
	{
		string requestUri = BuildUrl(resource);
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, requestUri);
		HttpClient client = HttpClientFactory.GetClient();
		AddDefaultHeaders(httpRequestMessage.Headers);
		httpRequestMessage.Headers.Add("ps-jwt", authenticationToken?.Jwt);
		return await AsyncSend<T>(client, httpRequestMessage);
	}

	private async Task<RestResponse<T>> AsyncSend<T>(HttpClient client, HttpRequestMessage message)
	{
		return await HandleExceptions(async delegate
		{
			HttpResponseMessage response = await client.SendAsync(message);
			return await ConvertResponseToRestResponse<T>(response);
		});
	}

	private async Task<RestResponse<TResp>> AsyncPost<TResp, TReq>(HttpClient client, HttpRequestMessage message, TReq request)
	{
		message.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
		return await AsyncSend<TResp>(client, message);
	}

	private async Task<RestResponse<T>> HandleExceptions<T>(Func<Task<RestResponse<T>>> executeRequest)
	{
		try
		{
			return await executeRequest();
		}
		catch (HttpRequestException e)
		{
			log.LogException(e);
			return new RestResponse<T>
			{
				StatusCode = (HttpStatusCode)0
			};
		}
		catch (TaskCanceledException e2)
		{
			log.LogException(e2);
			return new RestResponse<T>
			{
				StatusCode = (HttpStatusCode)0
			};
		}
	}

	public async Task<RestResponse<TResp>> BasicPost<TResp, TReq>(string resource, TReq request)
	{
		string requestUri = BuildUrl(resource);
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
		HttpClient client = HttpClientFactory.GetClient();
		AddDefaultHeaders(httpRequestMessage.Headers);
		return await AsyncPost<TResp, TReq>(client, httpRequestMessage, request);
	}

	public async Task<RestResponse<TResp>> AuthenticatedPost<TResp, TReq>(string resource, TReq request)
	{
		RestResponse<TResp> restResponse = await AuthenticatedPost<TResp, TReq>(resource, request, userRepository.Load()?.AuthToken);
		if (restResponse.StatusCode == HttpStatusCode.Unauthorized)
		{
			return await AuthenticatedPost<TResp, TReq>(resource, request, await RefreshAuthenticationToken());
		}
		return restResponse;
	}

	private async Task<RestResponse<TResp>> AuthenticatedPost<TResp, TReq>(string resource, TReq request, AuthenticationToken authenticationToken)
	{
		string requestUri = BuildUrl(resource);
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
		HttpClient client = HttpClientFactory.GetClient();
		AddDefaultHeaders(httpRequestMessage.Headers);
		httpRequestMessage.Headers.Add("ps-jwt", authenticationToken?.Jwt);
		return await AsyncPost<TResp, TReq>(client, httpRequestMessage, request);
	}

	private void AddDefaultHeaders(HttpRequestHeaders headers)
	{
		headers.Accept.ParseAdd("application/json");
		headers.UserAgent.ParseAdd(userAgent);
	}

	private string BuildUrl(string resource)
	{
		if (resource.StartsWith("/"))
		{
			return baseUrl + resource;
		}
		return baseUrl + "/" + resource;
	}

	private async Task<RestResponse<T>> ConvertResponseToRestResponse<T>(HttpResponseMessage response)
	{
		string text = await response.Content.ReadAsStringAsync();
		if (response.IsSuccessStatusCode)
		{
			RestResponse<T> restResponse = new RestResponse<T>();
			restResponse.StatusCode = response.StatusCode;
			restResponse.RawContent = text;
			restResponse.Data = JsonConvert.DeserializeObject<T>(text, new JsonConverter[1]
			{
				new TimeSpanMillisecondConverter()
			});
			return restResponse;
		}
		log.Log("Unsuccessful request to " + response.RequestMessage.RequestUri?.ToString() + ". Response Code: " + response.StatusCode, LogLevel.Warn);
		return new RestResponse<T>
		{
			StatusCode = response.StatusCode,
			RawContent = text
		};
	}
}
