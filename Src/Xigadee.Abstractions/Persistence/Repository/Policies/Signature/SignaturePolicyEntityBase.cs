﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Xigadee
{
    /// <summary>
    /// This policy is used to create a simple text signature for an entity.
    /// </summary>
    public abstract class SignaturePolicyEntityBase: SignaturePolicyBase
    {

        /// <summary>
        /// This is not supported.
        /// </summary>
        public override void RegisterChildPolicy(ISignaturePolicy childPolicy)
        {
            throw new NotSupportedException("Child policies not supported for an entity policy.");
        }

        /// <summary>
        /// Validate is not supported for an entity leaf node.
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override ISignaturePolicy Validate()
        {
            throw new NotSupportedException("Validate is not supported for an signature entity leaf node");
        }
    }
}
