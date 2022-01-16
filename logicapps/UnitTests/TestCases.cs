// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace TestCases
{
    using System.Net.Http;
    using System.Net;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json.Linq;
    using System.IO;
    using System.Linq;
    using TestFramework;

    /// <summary>
    /// The test cases
    /// </summary>
    [TestClass]
    public class TestCases
    {
        /// <summary>
        /// Simple request-response workflow test.
        /// </summary>
        [TestMethod]
        public void Stateless1()
        {
            var workflowName = "Stateless1";
            var workflowDefinition = File.ReadAllText($"{workflowName}\\workflow.json");
            var hostDefinition = File.ReadAllText("host.json");
            var localSettingsDefinition = File.ReadAllText("local.settings.json");

            using (new WorkflowTestHost(new WorkflowTestInput[] { new WorkflowTestInput(workflowName, workflowDefinition) }, host: hostDefinition, localSettings: localSettingsDefinition))
            using (var client = new HttpClient())
            {
                // Get workflow callback URL.
                var response = client.PostAsync(TestEnvironment.GetTriggerCallbackRequestUri(flowName: workflowName, triggerName: "manual"), null).Result;
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                // Run the workflow.
                response = client.PostAsync(response.Content.ReadAsAsync<CallbackUrlDefinition>().Result.Value, null).Result;
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                // Check workflow response.
                Assert.AreEqual("success", JObject.Parse(response.Content.ReadAsStringAsync().Result).SelectToken("code").Value<string>());

            }
        }
[TestMethod]
        public void Stateful()
        {
            var workflowName = "Stateful1";
            var workflowDefinition = File.ReadAllText($"{workflowName}\\workflow.json");
            var hostDefinition = File.ReadAllText("host.json");
            var localSettingsDefinition = File.ReadAllText("local.settings.json");

            using (new WorkflowTestHost(new WorkflowTestInput[] { new WorkflowTestInput(workflowName, workflowDefinition) }, host: hostDefinition, localSettings: localSettingsDefinition))
            using (var client = new HttpClient())
            {
                // Get workflow callback URL.
                var response = client.PostAsync(TestEnvironment.GetTriggerCallbackRequestUri(flowName: workflowName, triggerName: "manual"), null).Result;
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                // Run the workflow.
                response = client.PostAsync(response.Content.ReadAsAsync<CallbackUrlDefinition>().Result.Value, null).Result;
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                // Check workflow response.
                Assert.AreEqual("success", JObject.Parse(response.Content.ReadAsStringAsync().Result).SelectToken("code").Value<string>());

                //Check workflow run status.
                response = client.GetAsync(TestEnvironment.GetRunsRequestUriWithManagementHost(flowName: workflowName)).Result;
                var responseContent = response.Content.ReadAsAsync<JToken>().Result;
                Assert.AreEqual("Succeeded", responseContent["value"][0]["properties"]["status"].ToString());
                var runId = responseContent["value"].FirstOrDefault()["name"].ToString();

                // // Check action result.
                response = client.GetAsync(TestEnvironment.GetRunActionsRequestUri(flowName: workflowName, runName: runId)).Result;
                responseContent = response.Content.ReadAsAsync<JToken>().Result;
                Assert.AreEqual("Succeeded", responseContent["value"].Where(actionResult => actionResult["name"].ToString().Equals("Compose")).FirstOrDefault()["properties"]["status"]);
            }
        }

    }
}
