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


namespace Microsoft.Azure.Commands.NotificationHubs.Commands.Namespace
{
    using Microsoft.Azure.Commands.NotificationHubs.Models;
    using Microsoft.Azure.Management.NotificationHubs.Models;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Linq;
    using System.IO;
    using Newtonsoft.Json;

    [Cmdlet(VerbsCommon.New, "AzureNotificationHubsNamespaceAuthorizationRules"), OutputType(typeof(SharedAccessAuthorizationRuleAttributes))]
    public class NewAzureNotificationHubsNamespaceAuthorizationRules : AzureNotificationHubsCmdletBase
    {
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            HelpMessage = "The name of the resource group")]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1,
            HelpMessage = "Namespace Name.")]
        [ValidateNotNullOrEmpty]
        public string NamespaceName { get; set; }

        [Parameter(Mandatory = true,
            Position = 2,
            ParameterSetName = InputFileParameterSetName,
            HelpMessage = "File name containing a single AuthorizationRule definition.")]
        [ValidateNotNullOrEmpty]
        public string InputFile { get; set; }

        [Parameter(Mandatory = true,
            Position = 2,
            ParameterSetName = SASRuleParameterSetName,
            HelpMessage = "Namespace AuthorizationRule Object.")]
        [ValidateNotNullOrEmpty]
        public SharedAccessAuthorizationRuleAttributes SASRule { get; set; }

        public override void ExecuteCmdlet()
        {
            if (!string.IsNullOrEmpty(ResourceGroupName) && !string.IsNullOrEmpty(NamespaceName))
            {
                SharedAccessAuthorizationRuleAttributes sasRule = null;
                if (!string.IsNullOrEmpty(InputFile))
                {
                    string fileName = this.TryResolvePath(InputFile);
                    if (!(new FileInfo(fileName)).Exists)
                    {
                        throw new PSArgumentException(string.Format("File {0} does not exist", fileName));
                    }

                    try
                    {
                        sasRule = JsonConvert.DeserializeObject<SharedAccessAuthorizationRuleAttributes>(File.ReadAllText(fileName));
                    }
                    catch (JsonException)
                    {
                        WriteVerbose("Deserializing the input role definition failed.");
                        throw;
                    }
                }
                else
                {
                    sasRule = SASRule;
                }

                // Create a new namespace authorizationRule
                var authRule = Client.CreateOrUpdateNamespaceAuthorizationRules(ResourceGroupName, NamespaceName, sasRule.Name, sasRule.Rights,
                    sasRule.PrimaryKey, sasRule.SecondaryKey == null ? null : sasRule.SecondaryKey);
                WriteObject(authRule);
            }
        }
    }
}
