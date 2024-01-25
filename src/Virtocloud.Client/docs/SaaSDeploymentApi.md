# VirtoCloud.Client.Api.SaaSDeploymentApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**AppProjectsCreate**](SaaSDeploymentApi.md#appprojectscreate) | **POST** /api/saas/app-projects/create |  |
| [**AppProjectsDelete**](SaaSDeploymentApi.md#appprojectsdelete) | **POST** /api/saas/app-projects/delete |  |
| [**AppProjectsGetOrganizationsList**](SaaSDeploymentApi.md#appprojectsgetorganizationslist) | **GET** /api/saas/app-projects/org-list |  |
| [**AppProjectsList**](SaaSDeploymentApi.md#appprojectslist) | **GET** /api/saas/app-projects/list |  |
| [**AppProjectsUpdate**](SaaSDeploymentApi.md#appprojectsupdate) | **PUT** /api/saas/app-projects/update |  |
| [**AppsKibana**](SaaSDeploymentApi.md#appskibana) | **GET** /apps/kibana |  |
| [**CacheClear**](SaaSDeploymentApi.md#cacheclear) | **GET** /api/saas/cache/clear |  |
| [**DocsGetContent**](SaaSDeploymentApi.md#docsgetcontent) | **GET** /api/saas/Docs/content/{path} |  |
| [**DocsGetMenu**](SaaSDeploymentApi.md#docsgetmenu) | **GET** /api/saas/Docs/menu |  |
| [**EnvironmentsCanCreate**](SaaSDeploymentApi.md#environmentscancreate) | **GET** /api/saas/environments/can-create/{appProject} |  |
| [**EnvironmentsCanSave**](SaaSDeploymentApi.md#environmentscansave) | **POST** /api/saas/environments/can-save |  |
| [**EnvironmentsCreate**](SaaSDeploymentApi.md#environmentscreate) | **POST** /api/saas/environments |  |
| [**EnvironmentsDelete**](SaaSDeploymentApi.md#environmentsdelete) | **DELETE** /api/saas/environments |  |
| [**EnvironmentsDownloadManifest**](SaaSDeploymentApi.md#environmentsdownloadmanifest) | **GET** /api/saas/environments/manifest/{appProject}/{appName} |  |
| [**EnvironmentsGetClusters**](SaaSDeploymentApi.md#environmentsgetclusters) | **GET** /api/saas/environments/clusters |  |
| [**EnvironmentsGetEnvironment**](SaaSDeploymentApi.md#environmentsgetenvironment) | **GET** /api/saas/environments/{envName} |  |
| [**EnvironmentsGetEnvironment_0**](SaaSDeploymentApi.md#environmentsgetenvironment_0) | **GET** /api/saas/environments/{organizationName}/{envName} |  |
| [**EnvironmentsGetImageTags**](SaaSDeploymentApi.md#environmentsgetimagetags) | **GET** /api/saas/environments/tags |  |
| [**EnvironmentsGetImages**](SaaSDeploymentApi.md#environmentsgetimages) | **GET** /api/saas/environments/images/{appProjectName} |  |
| [**EnvironmentsGetLimits**](SaaSDeploymentApi.md#environmentsgetlimits) | **GET** /api/saas/environments/limits |  |
| [**EnvironmentsGetServicePlan**](SaaSDeploymentApi.md#environmentsgetserviceplan) | **GET** /api/saas/environments/service-plan/{name} |  |
| [**EnvironmentsGetSummary**](SaaSDeploymentApi.md#environmentsgetsummary) | **GET** /api/saas/environments/summary |  |
| [**EnvironmentsGetTier**](SaaSDeploymentApi.md#environmentsgettier) | **GET** /api/saas/environments/tier |  |
| [**EnvironmentsList**](SaaSDeploymentApi.md#environmentslist) | **GET** /api/saas/environments |  |
| [**EnvironmentsUpdate**](SaaSDeploymentApi.md#environmentsupdate) | **PUT** /api/saas/environments |  |
| [**EnvironmentsUpdate_0**](SaaSDeploymentApi.md#environmentsupdate_0) | **PUT** /api/saas/environments/update |  |
| [**EnvironmentsValidateName**](SaaSDeploymentApi.md#environmentsvalidatename) | **POST** /api/saas/environments/validate-name |  |
| [**MetricsGetMetrics**](SaaSDeploymentApi.md#metricsgetmetrics) | **POST** /api/saas/metrics |  |
| [**RssGetNews**](SaaSDeploymentApi.md#rssgetnews) | **GET** /api/saas/Rss/news |  |
| [**SystemVersion**](SaaSDeploymentApi.md#systemversion) | **POST** /api/saas/system/version |  |
| [**TemplatesGetClusters**](SaaSDeploymentApi.md#templatesgetclusters) | **GET** /api/saas/templates/clusters |  |
| [**TemplatesGetServicePlan**](SaaSDeploymentApi.md#templatesgetserviceplan) | **GET** /api/saas/templates/service-plan/{name} |  |
| [**TemplatesGetServicePlans**](SaaSDeploymentApi.md#templatesgetserviceplans) | **GET** /api/saas/templates/service-plans |  |

<a id="appprojectscreate"></a>
# **AppProjectsCreate**
> void AppProjectsCreate (Object? body = null)



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class AppProjectsCreateExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);
            var body = null;  // Object? |  (optional) 

            try
            {
                apiInstance.AppProjectsCreate(body);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.AppProjectsCreate: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the AppProjectsCreateWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.AppProjectsCreateWithHttpInfo(body);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.AppProjectsCreateWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **body** | **Object?** |  | [optional]  |

### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: application/json-patch+json, application/json, text/json
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="appprojectsdelete"></a>
# **AppProjectsDelete**
> void AppProjectsDelete (Object? body = null)



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class AppProjectsDeleteExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);
            var body = null;  // Object? |  (optional) 

            try
            {
                apiInstance.AppProjectsDelete(body);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.AppProjectsDelete: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the AppProjectsDeleteWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.AppProjectsDeleteWithHttpInfo(body);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.AppProjectsDeleteWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **body** | **Object?** |  | [optional]  |

### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: application/json-patch+json, application/json, text/json
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="appprojectsgetorganizationslist"></a>
# **AppProjectsGetOrganizationsList**
> void AppProjectsGetOrganizationsList ()



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class AppProjectsGetOrganizationsListExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);

            try
            {
                apiInstance.AppProjectsGetOrganizationsList();
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.AppProjectsGetOrganizationsList: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the AppProjectsGetOrganizationsListWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.AppProjectsGetOrganizationsListWithHttpInfo();
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.AppProjectsGetOrganizationsListWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters
This endpoint does not need any parameter.
### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="appprojectslist"></a>
# **AppProjectsList**
> void AppProjectsList ()



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class AppProjectsListExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);

            try
            {
                apiInstance.AppProjectsList();
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.AppProjectsList: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the AppProjectsListWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.AppProjectsListWithHttpInfo();
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.AppProjectsListWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters
This endpoint does not need any parameter.
### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="appprojectsupdate"></a>
# **AppProjectsUpdate**
> void AppProjectsUpdate (Object? body = null)



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class AppProjectsUpdateExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);
            var body = null;  // Object? |  (optional) 

            try
            {
                apiInstance.AppProjectsUpdate(body);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.AppProjectsUpdate: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the AppProjectsUpdateWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.AppProjectsUpdateWithHttpInfo(body);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.AppProjectsUpdateWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **body** | **Object?** |  | [optional]  |

### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: application/json-patch+json, application/json, text/json
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="appskibana"></a>
# **AppsKibana**
> void AppsKibana ()



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class AppsKibanaExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);

            try
            {
                apiInstance.AppsKibana();
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.AppsKibana: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the AppsKibanaWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.AppsKibanaWithHttpInfo();
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.AppsKibanaWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters
This endpoint does not need any parameter.
### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="cacheclear"></a>
# **CacheClear**
> void CacheClear ()



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class CacheClearExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);

            try
            {
                apiInstance.CacheClear();
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.CacheClear: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the CacheClearWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.CacheClearWithHttpInfo();
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.CacheClearWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters
This endpoint does not need any parameter.
### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="docsgetcontent"></a>
# **DocsGetContent**
> void DocsGetContent (string path)



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class DocsGetContentExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);
            var path = "path_example";  // string | 

            try
            {
                apiInstance.DocsGetContent(path);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.DocsGetContent: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the DocsGetContentWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.DocsGetContentWithHttpInfo(path);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.DocsGetContentWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **path** | **string** |  |  |

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="docsgetmenu"></a>
# **DocsGetMenu**
> void DocsGetMenu ()



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class DocsGetMenuExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);

            try
            {
                apiInstance.DocsGetMenu();
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.DocsGetMenu: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the DocsGetMenuWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.DocsGetMenuWithHttpInfo();
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.DocsGetMenuWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters
This endpoint does not need any parameter.
### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="environmentscancreate"></a>
# **EnvironmentsCanCreate**
> void EnvironmentsCanCreate (string appProject)



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class EnvironmentsCanCreateExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);
            var appProject = "appProject_example";  // string | 

            try
            {
                apiInstance.EnvironmentsCanCreate(appProject);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsCanCreate: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the EnvironmentsCanCreateWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.EnvironmentsCanCreateWithHttpInfo(appProject);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsCanCreateWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **appProject** | **string** |  |  |

### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="environmentscansave"></a>
# **EnvironmentsCanSave**
> void EnvironmentsCanSave (CloudEnvironment? cloudEnvironment = null)



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class EnvironmentsCanSaveExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);
            var cloudEnvironment = new CloudEnvironment?(); // CloudEnvironment? |  (optional) 

            try
            {
                apiInstance.EnvironmentsCanSave(cloudEnvironment);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsCanSave: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the EnvironmentsCanSaveWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.EnvironmentsCanSaveWithHttpInfo(cloudEnvironment);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsCanSaveWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **cloudEnvironment** | [**CloudEnvironment?**](CloudEnvironment?.md) |  | [optional]  |

### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: application/json-patch+json, application/json, text/json
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="environmentscreate"></a>
# **EnvironmentsCreate**
> void EnvironmentsCreate (NewEnvironmentModel? newEnvironmentModel = null)



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class EnvironmentsCreateExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);
            var newEnvironmentModel = new NewEnvironmentModel?(); // NewEnvironmentModel? |  (optional) 

            try
            {
                apiInstance.EnvironmentsCreate(newEnvironmentModel);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsCreate: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the EnvironmentsCreateWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.EnvironmentsCreateWithHttpInfo(newEnvironmentModel);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsCreateWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **newEnvironmentModel** | [**NewEnvironmentModel?**](NewEnvironmentModel?.md) |  | [optional]  |

### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: application/json-patch+json, application/json, text/json
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="environmentsdelete"></a>
# **EnvironmentsDelete**
> void EnvironmentsDelete (List<string>? complexIds = null, string? appProjectId = null)



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class EnvironmentsDeleteExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);
            var complexIds = new List<string>?(); // List<string>? |  (optional) 
            var appProjectId = "appProjectId_example";  // string? |  (optional) 

            try
            {
                apiInstance.EnvironmentsDelete(complexIds, appProjectId);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsDelete: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the EnvironmentsDeleteWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.EnvironmentsDeleteWithHttpInfo(complexIds, appProjectId);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsDeleteWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **complexIds** | [**List&lt;string&gt;?**](string.md) |  | [optional]  |
| **appProjectId** | **string?** |  | [optional]  |

### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="environmentsdownloadmanifest"></a>
# **EnvironmentsDownloadManifest**
> void EnvironmentsDownloadManifest (string appProject, string appName)



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class EnvironmentsDownloadManifestExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);
            var appProject = "appProject_example";  // string | 
            var appName = "appName_example";  // string | 

            try
            {
                apiInstance.EnvironmentsDownloadManifest(appProject, appName);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsDownloadManifest: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the EnvironmentsDownloadManifestWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.EnvironmentsDownloadManifestWithHttpInfo(appProject, appName);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsDownloadManifestWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **appProject** | **string** |  |  |
| **appName** | **string** |  |  |

### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="environmentsgetclusters"></a>
# **EnvironmentsGetClusters**
> void EnvironmentsGetClusters ()



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class EnvironmentsGetClustersExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);

            try
            {
                apiInstance.EnvironmentsGetClusters();
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsGetClusters: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the EnvironmentsGetClustersWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.EnvironmentsGetClustersWithHttpInfo();
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsGetClustersWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters
This endpoint does not need any parameter.
### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="environmentsgetenvironment"></a>
# **EnvironmentsGetEnvironment**
> List&lt;CloudEnvironment&gt; EnvironmentsGetEnvironment (string envName)



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class EnvironmentsGetEnvironmentExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);
            var envName = "envName_example";  // string | 

            try
            {
                List<CloudEnvironment> result = apiInstance.EnvironmentsGetEnvironment(envName);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsGetEnvironment: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the EnvironmentsGetEnvironmentWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    ApiResponse<List<CloudEnvironment>> response = apiInstance.EnvironmentsGetEnvironmentWithHttpInfo(envName);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsGetEnvironmentWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **envName** | **string** |  |  |

### Return type

[**List&lt;CloudEnvironment&gt;**](CloudEnvironment.md)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="environmentsgetenvironment_0"></a>
# **EnvironmentsGetEnvironment_0**
> List&lt;CloudEnvironment&gt; EnvironmentsGetEnvironment_0 (string organizationName, string envName)



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class EnvironmentsGetEnvironment_0Example
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);
            var organizationName = "organizationName_example";  // string | 
            var envName = "envName_example";  // string | 

            try
            {
                List<CloudEnvironment> result = apiInstance.EnvironmentsGetEnvironment_0(organizationName, envName);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsGetEnvironment_0: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the EnvironmentsGetEnvironment_0WithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    ApiResponse<List<CloudEnvironment>> response = apiInstance.EnvironmentsGetEnvironment_0WithHttpInfo(organizationName, envName);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsGetEnvironment_0WithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **organizationName** | **string** |  |  |
| **envName** | **string** |  |  |

### Return type

[**List&lt;CloudEnvironment&gt;**](CloudEnvironment.md)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="environmentsgetimagetags"></a>
# **EnvironmentsGetImageTags**
> void EnvironmentsGetImageTags (string? imageName = null)



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class EnvironmentsGetImageTagsExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);
            var imageName = "imageName_example";  // string? |  (optional) 

            try
            {
                apiInstance.EnvironmentsGetImageTags(imageName);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsGetImageTags: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the EnvironmentsGetImageTagsWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.EnvironmentsGetImageTagsWithHttpInfo(imageName);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsGetImageTagsWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **imageName** | **string?** |  | [optional]  |

### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="environmentsgetimages"></a>
# **EnvironmentsGetImages**
> void EnvironmentsGetImages (string appProjectName)



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class EnvironmentsGetImagesExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);
            var appProjectName = "appProjectName_example";  // string | 

            try
            {
                apiInstance.EnvironmentsGetImages(appProjectName);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsGetImages: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the EnvironmentsGetImagesWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.EnvironmentsGetImagesWithHttpInfo(appProjectName);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsGetImagesWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **appProjectName** | **string** |  |  |

### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="environmentsgetlimits"></a>
# **EnvironmentsGetLimits**
> void EnvironmentsGetLimits ()



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class EnvironmentsGetLimitsExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);

            try
            {
                apiInstance.EnvironmentsGetLimits();
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsGetLimits: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the EnvironmentsGetLimitsWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.EnvironmentsGetLimitsWithHttpInfo();
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsGetLimitsWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters
This endpoint does not need any parameter.
### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="environmentsgetserviceplan"></a>
# **EnvironmentsGetServicePlan**
> void EnvironmentsGetServicePlan (string name)



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class EnvironmentsGetServicePlanExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);
            var name = "name_example";  // string | 

            try
            {
                apiInstance.EnvironmentsGetServicePlan(name);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsGetServicePlan: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the EnvironmentsGetServicePlanWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.EnvironmentsGetServicePlanWithHttpInfo(name);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsGetServicePlanWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **name** | **string** |  |  |

### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="environmentsgetsummary"></a>
# **EnvironmentsGetSummary**
> void EnvironmentsGetSummary ()



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class EnvironmentsGetSummaryExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);

            try
            {
                apiInstance.EnvironmentsGetSummary();
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsGetSummary: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the EnvironmentsGetSummaryWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.EnvironmentsGetSummaryWithHttpInfo();
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsGetSummaryWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters
This endpoint does not need any parameter.
### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="environmentsgettier"></a>
# **EnvironmentsGetTier**
> void EnvironmentsGetTier ()



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class EnvironmentsGetTierExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);

            try
            {
                apiInstance.EnvironmentsGetTier();
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsGetTier: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the EnvironmentsGetTierWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.EnvironmentsGetTierWithHttpInfo();
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsGetTierWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters
This endpoint does not need any parameter.
### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="environmentslist"></a>
# **EnvironmentsList**
> List&lt;CloudEnvironment&gt; EnvironmentsList ()



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class EnvironmentsListExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);

            try
            {
                List<CloudEnvironment> result = apiInstance.EnvironmentsList();
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsList: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the EnvironmentsListWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    ApiResponse<List<CloudEnvironment>> response = apiInstance.EnvironmentsListWithHttpInfo();
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsListWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters
This endpoint does not need any parameter.
### Return type

[**List&lt;CloudEnvironment&gt;**](CloudEnvironment.md)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="environmentsupdate"></a>
# **EnvironmentsUpdate**
> void EnvironmentsUpdate (CloudEnvironment? cloudEnvironment = null)



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class EnvironmentsUpdateExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);
            var cloudEnvironment = new CloudEnvironment?(); // CloudEnvironment? |  (optional) 

            try
            {
                apiInstance.EnvironmentsUpdate(cloudEnvironment);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsUpdate: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the EnvironmentsUpdateWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.EnvironmentsUpdateWithHttpInfo(cloudEnvironment);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsUpdateWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **cloudEnvironment** | [**CloudEnvironment?**](CloudEnvironment?.md) |  | [optional]  |

### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: application/json-patch+json, application/json, text/json
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="environmentsupdate_0"></a>
# **EnvironmentsUpdate_0**
> void EnvironmentsUpdate_0 (string? manifest = null, string? appProject = null)



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class EnvironmentsUpdate_0Example
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);
            var manifest = "manifest_example";  // string? |  (optional) 
            var appProject = "appProject_example";  // string? |  (optional) 

            try
            {
                apiInstance.EnvironmentsUpdate_0(manifest, appProject);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsUpdate_0: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the EnvironmentsUpdate_0WithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.EnvironmentsUpdate_0WithHttpInfo(manifest, appProject);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsUpdate_0WithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **manifest** | **string?** |  | [optional]  |
| **appProject** | **string?** |  | [optional]  |

### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: multipart/form-data
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="environmentsvalidatename"></a>
# **EnvironmentsValidateName**
> void EnvironmentsValidateName (EnvironmentNameValidationRequest? environmentNameValidationRequest = null)



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class EnvironmentsValidateNameExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);
            var environmentNameValidationRequest = new EnvironmentNameValidationRequest?(); // EnvironmentNameValidationRequest? |  (optional) 

            try
            {
                apiInstance.EnvironmentsValidateName(environmentNameValidationRequest);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsValidateName: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the EnvironmentsValidateNameWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.EnvironmentsValidateNameWithHttpInfo(environmentNameValidationRequest);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.EnvironmentsValidateNameWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **environmentNameValidationRequest** | [**EnvironmentNameValidationRequest?**](EnvironmentNameValidationRequest?.md) |  | [optional]  |

### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: application/json-patch+json, application/json, text/json
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="metricsgetmetrics"></a>
# **MetricsGetMetrics**
> void MetricsGetMetrics (Object? body = null)



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class MetricsGetMetricsExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);
            var body = null;  // Object? |  (optional) 

            try
            {
                apiInstance.MetricsGetMetrics(body);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.MetricsGetMetrics: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the MetricsGetMetricsWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.MetricsGetMetricsWithHttpInfo(body);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.MetricsGetMetricsWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **body** | **Object?** |  | [optional]  |

### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: application/json-patch+json, application/json, text/json
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="rssgetnews"></a>
# **RssGetNews**
> void RssGetNews ()



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class RssGetNewsExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);

            try
            {
                apiInstance.RssGetNews();
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.RssGetNews: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the RssGetNewsWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.RssGetNewsWithHttpInfo();
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.RssGetNewsWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters
This endpoint does not need any parameter.
### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="systemversion"></a>
# **SystemVersion**
> void SystemVersion (Object? body = null)



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class SystemVersionExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);
            var body = null;  // Object? |  (optional) 

            try
            {
                apiInstance.SystemVersion(body);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.SystemVersion: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the SystemVersionWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.SystemVersionWithHttpInfo(body);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.SystemVersionWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **body** | **Object?** |  | [optional]  |

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json-patch+json, application/json, text/json
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="templatesgetclusters"></a>
# **TemplatesGetClusters**
> void TemplatesGetClusters ()



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class TemplatesGetClustersExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);

            try
            {
                apiInstance.TemplatesGetClusters();
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.TemplatesGetClusters: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the TemplatesGetClustersWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.TemplatesGetClustersWithHttpInfo();
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.TemplatesGetClustersWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters
This endpoint does not need any parameter.
### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="templatesgetserviceplan"></a>
# **TemplatesGetServicePlan**
> void TemplatesGetServicePlan (string name)



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class TemplatesGetServicePlanExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);
            var name = "name_example";  // string | 

            try
            {
                apiInstance.TemplatesGetServicePlan(name);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.TemplatesGetServicePlan: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the TemplatesGetServicePlanWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.TemplatesGetServicePlanWithHttpInfo(name);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.TemplatesGetServicePlanWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **name** | **string** |  |  |

### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="templatesgetserviceplans"></a>
# **TemplatesGetServicePlans**
> void TemplatesGetServicePlans ()



### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Client;
using VirtoCloud.Client.Model;

namespace Example
{
    public class TemplatesGetServicePlansExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            // create instances of HttpClient, HttpClientHandler to be reused later with different Api classes
            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new SaaSDeploymentApi(httpClient, config, httpClientHandler);

            try
            {
                apiInstance.TemplatesGetServicePlans();
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SaaSDeploymentApi.TemplatesGetServicePlans: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the TemplatesGetServicePlansWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    apiInstance.TemplatesGetServicePlansWithHttpInfo();
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SaaSDeploymentApi.TemplatesGetServicePlansWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters
This endpoint does not need any parameter.
### Return type

void (empty response body)

### Authorization

[oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Success |  -  |
| **401** | Unauthorized |  -  |
| **403** | Forbidden |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

