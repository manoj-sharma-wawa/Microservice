﻿#region Copyright
// Copyright Hitachi Consulting
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Auth;

namespace Xigadee
{
    public static partial class AzureExtensionMethods
    {
        /// <summary>
        /// This is the Event Hub key type value.
        /// </summary>
        [ConfigSettingKey("EventHub")]
        public const string KeyEventHubConnection = "EventHubConnection";
        /// <summary>
        /// This is the Event Hub connection
        /// </summary>
        /// <param name="config">The Microservice configuration.</param>
        /// <returns>Returns the connection string.</returns>
        [ConfigSetting("EventHub")]
        public static string EventHubConnection(this IEnvironmentConfiguration config) => config.PlatformOrConfigCache(KeyEventHubConnection);

        #region EventHubConnectionValidate(this IEnvironmentConfiguration Configuration, string serviceBusConnection)
        /// <summary>
        /// This method validates that the Event Hub connection is set.
        /// </summary>
        /// <param name="Configuration">The configuration.</param>
        /// <param name="eventHubConnection">The alternate connection.</param>
        /// <returns>Returns the connection from either the parameter or from the settings.</returns>
        private static string EventHubConnectionValidate(this IEnvironmentConfiguration Configuration, string eventHubConnection)
        {
            var conn = eventHubConnection ?? Configuration.EventHubConnection();

            if (string.IsNullOrEmpty(conn))
                throw new AzureConnectionException();//"Service bus connection string cannot be resolved. Please check the config settings has been set.");

            return conn;
        }
        #endregion


        /// <summary>
        /// This extension allows the Event Hub connection values to be manually set as override parameters.
        /// </summary>
        /// <param name="pipeline">The incoming pipeline.</param>
        /// <param name="connection">The Event Hub connection.</param>
        /// <returns>The passthrough of the pipeline.</returns>
        public static P ConfigOverrideSetEventHubConnection<P>(this P pipeline, string connection)
            where P : IPipeline
        {
            pipeline.ConfigurationOverrideSet(KeyEventHubConnection, connection);
            return pipeline;
        }
    }
}