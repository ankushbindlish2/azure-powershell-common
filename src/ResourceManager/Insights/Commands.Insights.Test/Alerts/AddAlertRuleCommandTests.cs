﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System;
using System.Management.Automation;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Commands.Insights.Alerts;
using Microsoft.Azure.Management.Insights;
using Microsoft.Azure.Management.Insights.Models;
using Microsoft.WindowsAzure.Commands.ScenarioTest;
using Moq;
using Xunit;

namespace Microsoft.Azure.Commands.Insights.Test.Alerts
{
    public class AddAlertRuleCommandTests
    {
        private readonly AddAlertRuleCommand cmdlet;
        private readonly Mock<InsightsManagementClient> insightsManagementClientMock;
        private readonly Mock<IAlertOperations> insightsAlertRuleOperationsMock;
        private Mock<ICommandRuntime> commandRuntimeMock;
        private AzureOperationResponse response;
        private string resourceGroup;
        private RuleCreateOrUpdateParameters createOrUpdatePrms;

        public AddAlertRuleCommandTests()
        {
            insightsAlertRuleOperationsMock = new Mock<IAlertOperations>();
            insightsManagementClientMock = new Mock<InsightsManagementClient>();
            commandRuntimeMock = new Mock<ICommandRuntime>();
            cmdlet = new AddAlertRuleCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                InsightsManagementClient = insightsManagementClientMock.Object
            };

            response = new AzureOperationResponse()
            {
                RequestId = Guid.NewGuid().ToString(),
                StatusCode = HttpStatusCode.OK,
            };

            insightsAlertRuleOperationsMock.Setup(f => f.CreateOrUpdateRuleAsync(It.IsAny<string>(), It.IsAny<RuleCreateOrUpdateParameters>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<AzureOperationResponse>(response))
                .Callback((string resourceGrp, RuleCreateOrUpdateParameters createOrUpdateParams, CancellationToken t) =>
                {
                    resourceGroup = resourceGrp;
                    createOrUpdatePrms = createOrUpdateParams;
                });

            insightsManagementClientMock.SetupGet(f => f.AlertOperations).Returns(this.insightsAlertRuleOperationsMock.Object);
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void AddAlertRuleCommandParametersProcessing()
        {
            cmdlet.RuleType = AlertRuleTypes.Metric;
            cmdlet.Name = Utilities.Name;
            cmdlet.Location = "East US";
            cmdlet.ResourceGroup = Utilities.ResourceGroup;
            cmdlet.Operator = ConditionOperator.GreaterThan;
            cmdlet.Threshold = 1;
            cmdlet.ResourceUri = "/subscriptions/a93fb07c-6c93-40be-bf3b-4f0deba10f4b/resourceGroups/Default-Web-EastUS/providers/microsoft.web/sites/misitiooeltuyo";
            cmdlet.MetricName = "Requests";
            cmdlet.TimeAggregationOperator = TimeAggregationOperator.Total;
            cmdlet.CustomEmails = new string[] {"gu@macrosoft.com,h@dd.com"};

            cmdlet.ExecuteCmdlet();

            Assert.Equal(Utilities.ResourceGroup, this.resourceGroup);
            Assert.NotNull(this.createOrUpdatePrms);
        }
    }
}
