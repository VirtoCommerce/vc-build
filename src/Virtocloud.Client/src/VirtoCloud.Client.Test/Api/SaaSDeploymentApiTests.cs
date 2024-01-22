/*
 * VirtoCommerce.SaaS
 *
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * The version of the OpenAPI document: v1
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Xunit;

using VirtoCloud.Client.Client;
using VirtoCloud.Client.Api;
// uncomment below to import models
//using VirtoCloud.Client.Model;

namespace VirtoCloud.Client.Test.Api
{
    /// <summary>
    ///  Class for testing SaaSDeploymentApi
    /// </summary>
    /// <remarks>
    /// This file is automatically generated by OpenAPI Generator (https://openapi-generator.tech).
    /// Please update the test case below to test the API endpoint.
    /// </remarks>
    public class SaaSDeploymentApiTests : IDisposable
    {
        private SaaSDeploymentApi instance;

        public SaaSDeploymentApiTests()
        {
            instance = new SaaSDeploymentApi();
        }

        public void Dispose()
        {
            // Cleanup when everything is done.
        }

        /// <summary>
        /// Test an instance of SaaSDeploymentApi
        /// </summary>
        [Fact]
        public void InstanceTest()
        {
            // TODO uncomment below to test 'IsType' SaaSDeploymentApi
            //Assert.IsType<SaaSDeploymentApi>(instance);
        }

        /// <summary>
        /// Test AppProjectsCreate
        /// </summary>
        [Fact]
        public void AppProjectsCreateTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //Object? body = null;
            //instance.AppProjectsCreate(body);
        }

        /// <summary>
        /// Test AppProjectsDelete
        /// </summary>
        [Fact]
        public void AppProjectsDeleteTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //Object? body = null;
            //instance.AppProjectsDelete(body);
        }

        /// <summary>
        /// Test AppProjectsGetOrganizationsList
        /// </summary>
        [Fact]
        public void AppProjectsGetOrganizationsListTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //instance.AppProjectsGetOrganizationsList();
        }

        /// <summary>
        /// Test AppProjectsList
        /// </summary>
        [Fact]
        public void AppProjectsListTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //instance.AppProjectsList();
        }

        /// <summary>
        /// Test AppProjectsUpdate
        /// </summary>
        [Fact]
        public void AppProjectsUpdateTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //Object? body = null;
            //instance.AppProjectsUpdate(body);
        }

        /// <summary>
        /// Test AppsKibana
        /// </summary>
        [Fact]
        public void AppsKibanaTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //instance.AppsKibana();
        }

        /// <summary>
        /// Test CacheClear
        /// </summary>
        [Fact]
        public void CacheClearTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //instance.CacheClear();
        }

        /// <summary>
        /// Test DocsGetContent
        /// </summary>
        [Fact]
        public void DocsGetContentTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string path = null;
            //instance.DocsGetContent(path);
        }

        /// <summary>
        /// Test DocsGetMenu
        /// </summary>
        [Fact]
        public void DocsGetMenuTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //instance.DocsGetMenu();
        }

        /// <summary>
        /// Test EnvironmentsCanCreate
        /// </summary>
        [Fact]
        public void EnvironmentsCanCreateTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string appProject = null;
            //instance.EnvironmentsCanCreate(appProject);
        }

        /// <summary>
        /// Test EnvironmentsCanSave
        /// </summary>
        [Fact]
        public void EnvironmentsCanSaveTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //CloudEnvironment? cloudEnvironment = null;
            //instance.EnvironmentsCanSave(cloudEnvironment);
        }

        /// <summary>
        /// Test EnvironmentsCreate
        /// </summary>
        [Fact]
        public void EnvironmentsCreateTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //NewEnvironmentModel? newEnvironmentModel = null;
            //instance.EnvironmentsCreate(newEnvironmentModel);
        }

        /// <summary>
        /// Test EnvironmentsDelete
        /// </summary>
        [Fact]
        public void EnvironmentsDeleteTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //List<string>? complexIds = null;
            //string? appProjectId = null;
            //instance.EnvironmentsDelete(complexIds, appProjectId);
        }

        /// <summary>
        /// Test EnvironmentsDownloadManifest
        /// </summary>
        [Fact]
        public void EnvironmentsDownloadManifestTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string appProject = null;
            //string appName = null;
            //instance.EnvironmentsDownloadManifest(appProject, appName);
        }

        /// <summary>
        /// Test EnvironmentsGetClusters
        /// </summary>
        [Fact]
        public void EnvironmentsGetClustersTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //instance.EnvironmentsGetClusters();
        }

        /// <summary>
        /// Test EnvironmentsGetEnvironment
        /// </summary>
        [Fact]
        public void EnvironmentsGetEnvironmentTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string envName = null;
            //var response = instance.EnvironmentsGetEnvironment(envName);
            //Assert.IsType<List<CloudEnvironment>>(response);
        }

        /// <summary>
        /// Test EnvironmentsGetEnvironment_0
        /// </summary>
        [Fact]
        public void EnvironmentsGetEnvironment_0Test()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string organizationName = null;
            //string envName = null;
            //var response = instance.EnvironmentsGetEnvironment_0(organizationName, envName);
            //Assert.IsType<List<CloudEnvironment>>(response);
        }

        /// <summary>
        /// Test EnvironmentsGetImageTags
        /// </summary>
        [Fact]
        public void EnvironmentsGetImageTagsTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string? imageName = null;
            //instance.EnvironmentsGetImageTags(imageName);
        }

        /// <summary>
        /// Test EnvironmentsGetImages
        /// </summary>
        [Fact]
        public void EnvironmentsGetImagesTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string appProjectName = null;
            //instance.EnvironmentsGetImages(appProjectName);
        }

        /// <summary>
        /// Test EnvironmentsGetLimits
        /// </summary>
        [Fact]
        public void EnvironmentsGetLimitsTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //instance.EnvironmentsGetLimits();
        }

        /// <summary>
        /// Test EnvironmentsGetServicePlan
        /// </summary>
        [Fact]
        public void EnvironmentsGetServicePlanTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string name = null;
            //instance.EnvironmentsGetServicePlan(name);
        }

        /// <summary>
        /// Test EnvironmentsGetSummary
        /// </summary>
        [Fact]
        public void EnvironmentsGetSummaryTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //instance.EnvironmentsGetSummary();
        }

        /// <summary>
        /// Test EnvironmentsGetTier
        /// </summary>
        [Fact]
        public void EnvironmentsGetTierTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //instance.EnvironmentsGetTier();
        }

        /// <summary>
        /// Test EnvironmentsList
        /// </summary>
        [Fact]
        public void EnvironmentsListTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //var response = instance.EnvironmentsList();
            //Assert.IsType<List<CloudEnvironment>>(response);
        }

        /// <summary>
        /// Test EnvironmentsUpdate
        /// </summary>
        [Fact]
        public void EnvironmentsUpdateTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //CloudEnvironment? cloudEnvironment = null;
            //instance.EnvironmentsUpdate(cloudEnvironment);
        }

        /// <summary>
        /// Test EnvironmentsUpdate_0
        /// </summary>
        [Fact]
        public void EnvironmentsUpdate_0Test()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string? manifest = null;
            //string? appProject = null;
            //instance.EnvironmentsUpdate_0(manifest, appProject);
        }

        /// <summary>
        /// Test EnvironmentsValidateName
        /// </summary>
        [Fact]
        public void EnvironmentsValidateNameTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //EnvironmentNameValidationRequest? environmentNameValidationRequest = null;
            //instance.EnvironmentsValidateName(environmentNameValidationRequest);
        }

        /// <summary>
        /// Test MetricsGetMetrics
        /// </summary>
        [Fact]
        public void MetricsGetMetricsTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //Object? body = null;
            //instance.MetricsGetMetrics(body);
        }

        /// <summary>
        /// Test RssGetNews
        /// </summary>
        [Fact]
        public void RssGetNewsTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //instance.RssGetNews();
        }

        /// <summary>
        /// Test SystemVersion
        /// </summary>
        [Fact]
        public void SystemVersionTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //Object? body = null;
            //instance.SystemVersion(body);
        }

        /// <summary>
        /// Test TemplatesGetClusters
        /// </summary>
        [Fact]
        public void TemplatesGetClustersTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //instance.TemplatesGetClusters();
        }

        /// <summary>
        /// Test TemplatesGetServicePlan
        /// </summary>
        [Fact]
        public void TemplatesGetServicePlanTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string name = null;
            //instance.TemplatesGetServicePlan(name);
        }

        /// <summary>
        /// Test TemplatesGetServicePlans
        /// </summary>
        [Fact]
        public void TemplatesGetServicePlansTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //instance.TemplatesGetServicePlans();
        }
    }
}