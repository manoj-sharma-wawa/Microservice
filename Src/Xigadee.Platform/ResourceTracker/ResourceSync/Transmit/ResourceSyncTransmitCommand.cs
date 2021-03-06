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

namespace Xigadee
{
    /// <summary>
    /// This command is used to synchronise and consolidate resource performance counters and signalling 
    /// across multiple Microservices.
    /// </summary>
    public class ResourceSyncTransmitCommand: ResourceSyncCommandBase<ResourceSyncTransmitCommandStatistics, ResourceSyncTransmitCommandPolicy>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceSyncTransmitCommand"/> class.
        /// </summary>
        /// <param name="policy">The optional command policy. If this is null, then the policy will be created.</param>
        public ResourceSyncTransmitCommand(ResourceSyncTransmitCommandPolicy policy = null):base(policy)
        {

        }

        protected override void StartInternal()
        {
            base.StartInternal();
        }

        protected override void StopInternal()
        {
            base.StopInternal();
        }
    }
}
