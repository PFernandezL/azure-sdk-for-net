﻿//
// Copyright (c) Microsoft.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using Batch.Tests.Helpers;
using Microsoft.Azure;
using Microsoft.Azure.Management.Batch;
using Microsoft.Azure.Management.Batch.Models;

using Xunit;
using Microsoft.Rest;
using System.Threading.Tasks;

namespace Batch.Tests.InMemoryTests
{
    public class ApplicationPackageTests
    {
        public BatchManagementClient GetBatchManagementClient(RecordedDelegatingHandler handler)
        {
            handler.IsPassThrough = false;
            var client = new BatchManagementClient(handler);
            client.ApiVersion = "2015-12-01";
            client.SubscriptionId = "00000000-0000-0000-0000-000000000000";
            return client;
        }

        [Fact]
        public void IfAnAccountIsCreatedWithAutoStorage_ThenTheAutoStorageAccountIdMustNotBeNull()
        {
            var handler = new RecordedDelegatingHandler();

            var client = GetBatchManagementClient(handler);

            // If storageId is not set this will throw an ArgumentNullException
            var ex = Assert.Throws<ValidationException>(() => client.Account.Create("resourceGroupName", "acctName", new BatchAccountCreateParameters
            {
                Location = "South Central US",
                AutoStorage = new AutoStorageBaseProperties()
            }));

            Assert.Equal("StorageAccountId", ex.Target.ToString());
        }

        [Fact]
        public void AddApplicationPackageValidateMessage()
        {
            var utcNow = DateTime.UtcNow;

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Created)
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(@"{
                    'id': 'foo',
                    'storageUrl': '/subscriptions/12345/resourceGroups/foo/providers/Microsoft.Batch/batchAccounts/acctName',
                    'version' : 'beta',
                    'storageUrlExpiry' : '" + utcNow.ToString("o") + @"',
                    }")
            };


            response.Headers.Add("x-ms-request-id", "1");
            var handler = new RecordedDelegatingHandler(response) { StatusCodeToReturn = HttpStatusCode.OK };
            var client = GetBatchManagementClient(handler);

            var result = client.ApplicationOperations.AddApplicationPackage("resourceGroupName", "acctName", "appId", "beta");

            // Validate headers - User-Agent for certs, Authorization for tokens
            Assert.Equal(HttpMethod.Put, handler.Method);
            Assert.NotNull(handler.RequestHeaders.GetValues("User-Agent"));

            // Validate result
            Assert.Equal(utcNow, result.StorageUrlExpiry);
            Assert.Equal("foo", result.Id);
            Assert.Equal("beta", result.Version);
            Assert.Equal("/subscriptions/12345/resourceGroups/foo/providers/Microsoft.Batch/batchAccounts/acctName", result.StorageUrl);
        }

        [Fact]
        public void AddApplicationValidateMessage()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Created)
            {
                StatusCode = HttpStatusCode.Created
            };

            response.Headers.Add("x-ms-request-id", "1");
            var handler = new RecordedDelegatingHandler(response) { StatusCodeToReturn = HttpStatusCode.OK };
            var client = GetBatchManagementClient(handler);

            client.ApplicationOperations.AddApplication(
                "resourceGroupName",
                "acctName",
                "appId",
                new AddApplicationParameters { AllowUpdates = true, DisplayName = "display-name" });


            // Validate headers - User-Agent for certs, Authorization for tokens
            Assert.Equal(HttpMethod.Put, handler.Method);
            Assert.NotNull(handler.RequestHeaders.GetValues("User-Agent"));
        }

        [Fact]
        public void ActivateApplicationPackageValidateMessage()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Created)
            {
                StatusCode = HttpStatusCode.NoContent
            };

            response.Headers.Add("x-ms-request-id", "1");
            var handler = new RecordedDelegatingHandler(response) { StatusCodeToReturn = HttpStatusCode.OK };
            var client = GetBatchManagementClient(handler);

            var result = Task.Factory.StartNew(() => client.ApplicationOperations.ActivateApplicationPackageWithHttpMessagesAsync(
                "resourceGroupName",
                "acctName",
                "appId",
                "version",
                new ActivateApplicationPackageParameters("zip"))).Unwrap().GetAwaiter().GetResult();


            // Validate headers - User-Agent for certs, Authorization for tokens
            Assert.Equal(HttpMethod.Post, handler.Method);
            Assert.NotNull(handler.RequestHeaders.GetValues("User-Agent"));

            // Validate result
            Assert.Equal(HttpStatusCode.NoContent, result.Response.StatusCode);
        }

        [Fact]
        public void DeleteApplicationValidateMessage()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Created)
            {
                StatusCode = HttpStatusCode.NoContent
            };

            response.Headers.Add("x-ms-request-id", "1");
            var handler = new RecordedDelegatingHandler(response) { StatusCodeToReturn = HttpStatusCode.OK };
            var client = GetBatchManagementClient(handler);

            var result = Task.Factory.StartNew(() => client.ApplicationOperations.DeleteApplicationWithHttpMessagesAsync("resourceGroupName", "acctName", "appId")).Unwrap().GetAwaiter().GetResult();

            // Validate headers - User-Agent for certs, Authorization for tokens
            Assert.Equal(HttpMethod.Delete, handler.Method);
            Assert.NotNull(handler.RequestHeaders.GetValues("User-Agent"));

            // Validate result
            Assert.Equal(HttpStatusCode.NoContent, result.Response.StatusCode);
        }

        [Fact]
        public void DeleteApplicationPackageValidateMessage()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Created)
            {
                StatusCode = HttpStatusCode.NoContent
            };

            response.Headers.Add("x-ms-request-id", "1");
            var handler = new RecordedDelegatingHandler(response) { StatusCodeToReturn = HttpStatusCode.OK };
            var client = GetBatchManagementClient(handler);

            var result = Task.Factory.StartNew(() => client.ApplicationOperations.DeleteApplicationPackageWithHttpMessagesAsync(
                            "resourceGroupName",
                            "acctName",
                            "appId",
                            "version")).Unwrap().GetAwaiter().GetResult();

            // Validate headers - User-Agent for certs, Authorization for tokens
            Assert.Equal(HttpMethod.Delete, handler.Method);
            Assert.NotNull(handler.RequestHeaders.GetValues("User-Agent"));

            // Validate result
            Assert.Equal(HttpStatusCode.NoContent, result.Response.StatusCode);
        }

        [Fact]
        public void GetApplicationValidateMessage()
        {
            var utcNow = DateTime.UtcNow;

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Created)
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{
                    'id': 'foo',
                    'allowUpdates': 'true',
                    'displayName' : 'displayName',
                    'defaultVersion' : 'beta',
                    'packages':[
                        {'version':'fooVersion', 'state':'pending', 'format': 'betaFormat', 'lastActivationTime': '" + utcNow.ToString("o") + @"'}],

                    }")
            };


            response.Headers.Add("x-ms-request-id", "1");
            var handler = new RecordedDelegatingHandler(response) { StatusCodeToReturn = HttpStatusCode.OK };
            var client = GetBatchManagementClient(handler);

            var result = client.ApplicationOperations.GetApplication("applicationId", "acctName", "id");

            // Validate headers - User-Agent for certs, Authorization for tokens
            Assert.Equal(HttpMethod.Get, handler.Method);
            Assert.NotNull(handler.RequestHeaders.GetValues("User-Agent"));


            Assert.Equal("foo", result.Id);
            Assert.Equal(true, result.AllowUpdates);
            Assert.Equal("beta", result.DefaultVersion);
            Assert.Equal("displayName", result.DisplayName);
            Assert.Equal(1, result.Packages.Count);
            Assert.Equal("betaFormat", result.Packages.First().Format);
            Assert.Equal(PackageState.Pending, result.Packages.First().State);
            Assert.Equal("fooVersion", result.Packages.First().Version);
            Assert.Equal(utcNow, result.Packages.First().LastActivationTime);
        }

        [Fact]
        public void GetApplicationPackageValidateMessage()
        {
            var utcNow = DateTime.UtcNow;

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Created)
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{
                    'id': 'foo',
                    'storageUrl': '//storageUrl',
                    'state' : 'Pending',
                    'version' : 'beta',
                    'format':'zip',
                    'storageUrlExpiry':'" + utcNow.ToString("o") + @"',
                    'lastActivationTime':'" + utcNow.ToString("o") + @"',
                    }")
            };


            response.Headers.Add("x-ms-request-id", "1");
            var handler = new RecordedDelegatingHandler(response) { StatusCodeToReturn = HttpStatusCode.OK };
            var client = GetBatchManagementClient(handler);

            var result = client.ApplicationOperations.GetApplicationPackage("resourceGroupName", "acctName", "id", "VER");

            // Validate headers - User-Agent for certs, Authorization for tokens
            Assert.Equal(HttpMethod.Get, handler.Method);
            Assert.NotNull(handler.RequestHeaders.GetValues("User-Agent"));

            Assert.Equal("foo", result.Id);
            Assert.Equal("//storageUrl", result.StorageUrl);
            Assert.Equal(PackageState.Pending, result.State);
            Assert.Equal("beta", result.Version);
            Assert.Equal("zip", result.Format);
            Assert.Equal(utcNow, result.LastActivationTime);
            Assert.Equal(utcNow, result.StorageUrlExpiry);
        }


        [Fact]
        public void ListApplicationValidateMessage()
        {
            var utcNow = DateTime.UtcNow;

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Created)
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{ 'value':[{
                    'id': 'foo',
                    'allowUpdates': 'true',
                    'displayName' : 'DisplayName',
                    'defaultVersion' : 'beta',
                    'packages':[
                        {'version':'version1', 'state':'pending', 'format': 'beta', 'lastActivationTime': '" + utcNow.ToString("o") + @"'},
                        {'version':'version2', 'state':'pending', 'format': 'alpha', 'lastActivationTime': '" + utcNow.ToString("o") + @"'}],

                    }]}")
            };


            response.Headers.Add("x-ms-request-id", "1");
            var handler = new RecordedDelegatingHandler(response) { StatusCodeToReturn = HttpStatusCode.OK };
            var client = GetBatchManagementClient(handler);

            var result = client.ApplicationOperations.List("resourceGroupName", "acctName");

            // Validate headers - User-Agent for certs, Authorization for tokens
            Assert.Equal(HttpMethod.Get, handler.Method);
            Assert.NotNull(handler.RequestHeaders.GetValues("User-Agent"));

            Application application = result.Value.First();
            Assert.Equal("foo", application.Id);
            Assert.Equal(true, application.AllowUpdates);
            Assert.Equal("beta", application.DefaultVersion);
            Assert.Equal("DisplayName", application.DisplayName);
            Assert.Equal(application.Packages.Count, 2);
            Assert.Equal("beta", application.Packages.First().Format);
            Assert.Equal(PackageState.Pending, application.Packages.First().State);
            Assert.Equal("version1", application.Packages.First().Version);
            Assert.Equal(utcNow, application.Packages.First().LastActivationTime);
        }

        [Fact]
        public void UpdateApplicationValidateMessage()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Created)
            {
                StatusCode = HttpStatusCode.NoContent
            };

            response.Headers.Add("x-ms-request-id", "1");
            var handler = new RecordedDelegatingHandler(response) { StatusCodeToReturn = HttpStatusCode.OK };
            var client = GetBatchManagementClient(handler);

            client.ApplicationOperations.UpdateApplication(
                "resourceGroupName",
                "acctName",
                "appId", new UpdateApplicationParameters() { AllowUpdates = true, DisplayName = "display-name", DefaultVersion = "blah" }
                );


            // Validate headers - User-Agent for certs, Authorization for tokens
            Assert.Equal("PATCH", handler.Method.ToString());
            Assert.NotNull(handler.RequestHeaders.GetValues("User-Agent"));
        }

        [Fact]
        public void CannotPassNullArgumentsToActivateApplicationPackage()
        {
            var handler = new RecordedDelegatingHandler();
            var client = GetBatchManagementClient(handler);

            Assert.Throws<ValidationException>(() => client.ApplicationOperations.ActivateApplicationPackage(null, "foo", "foo", "foo", new ActivateApplicationPackageParameters()));
            Assert.Throws<ValidationException>(() => client.ApplicationOperations.ActivateApplicationPackage("foo", null, "foo", "foo", new ActivateApplicationPackageParameters()));
            Assert.Throws<ValidationException>(() => client.ApplicationOperations.ActivateApplicationPackage("foo", "foo", null, "foo", new ActivateApplicationPackageParameters()));
            Assert.Throws<ValidationException>(() => client.ApplicationOperations.ActivateApplicationPackage("foo", "foo", "foo", null, new ActivateApplicationPackageParameters()));
            Assert.Throws<ValidationException>(() => client.ApplicationOperations.ActivateApplicationPackage("foo", "foo", "foo", "foo", null));
        }

        [Fact]
        public void CannotPassNullArgumentsToAddApplication()
        {
            var handler = new RecordedDelegatingHandler();
            var client = GetBatchManagementClient(handler);

            Assert.Throws<ValidationException>(() => client.ApplicationOperations.AddApplication(null, "foo", "foo", new AddApplicationParameters()));
            Assert.Throws<ValidationException>(() => client.ApplicationOperations.AddApplication("foo", null, "foo", new AddApplicationParameters()));
            Assert.Throws<ValidationException>(() => client.ApplicationOperations.AddApplication("foo", "foo", null, new AddApplicationParameters()));
            Assert.Throws<NullReferenceException>(() => client.ApplicationOperations.AddApplication("foo", "foo", "foo", null));
        }

        [Fact]
        public void CannotPassNullArgumentsToDeleteApplication()
        {
            var handler = new RecordedDelegatingHandler();
            var client = GetBatchManagementClient(handler);

            Assert.Throws<ValidationException>(() => client.ApplicationOperations.DeleteApplication(null, "foo", "foo"));
            Assert.Throws<ValidationException>(() => client.ApplicationOperations.DeleteApplication("foo", null, "foo"));
            Assert.Throws<ValidationException>(() => client.ApplicationOperations.DeleteApplication("foo", "foo", null));
        }

        [Fact]
        public void CannotPassNullArgumentsToDeleteApplicationPackage()
        {
            var handler = new RecordedDelegatingHandler();
            var client = GetBatchManagementClient(handler);

            Assert.Throws<ValidationException>(() => client.ApplicationOperations.DeleteApplicationPackage(null, "foo", "foo", "foo"));
            Assert.Throws<ValidationException>(() => client.ApplicationOperations.DeleteApplicationPackage("foo", null, "foo", "foo"));
            Assert.Throws<ValidationException>(() => client.ApplicationOperations.DeleteApplicationPackage("foo", "foo", null, "foo"));
            Assert.Throws<ValidationException>(() => client.ApplicationOperations.DeleteApplicationPackage("foo", "foo", "bar", null));
        }

        [Fact]
        public void CannotPassNullArgumentsToGetApplication()
        {
            var handler = new RecordedDelegatingHandler();
            var client = GetBatchManagementClient(handler);
            Assert.Throws<ValidationException>(() => client.ApplicationOperations.GetApplication(null, "foo", "foo"));
            Assert.Throws<ValidationException>(() => client.ApplicationOperations.GetApplication("foo", null, "foo"));
            Assert.Throws<ValidationException>(() => client.ApplicationOperations.GetApplication("foo", "foo", null));
        }

        [Fact]
        public void CannotPassNullArgumentsToListApplications()
        {
            var handler = new RecordedDelegatingHandler();
            var client = GetBatchManagementClient(handler);

            Assert.Throws<ValidationException>(() => client.ApplicationOperations.List(null, "foo"));
            Assert.Throws<ValidationException>(() => client.ApplicationOperations.List("foo", null));
        }

        [Fact]
        public void CannotPassNullArgumentsToUpdateApplication()
        {
            var handler = new RecordedDelegatingHandler();
            var client = GetBatchManagementClient(handler);

            Assert.Throws<ValidationException>(() => client.ApplicationOperations.UpdateApplication(null, "foo", "foo", new UpdateApplicationParameters()));
            Assert.Throws<ValidationException>(() => client.ApplicationOperations.UpdateApplication("foo", null, "foo", new UpdateApplicationParameters()));
            Assert.Throws<ValidationException>(() => client.ApplicationOperations.UpdateApplication("foo", "foo", null, new UpdateApplicationParameters()));
            Assert.Throws<ValidationException>(() => client.ApplicationOperations.UpdateApplication("foo", "foo", "foo", null));
        }
    }
}
