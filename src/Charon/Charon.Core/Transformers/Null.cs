﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.IO;
using Charon.Core.Dom;
using Charon.Core.IO;

namespace Charon.Core.Transformers
{
	[Description("Null transformer. Returns that data unaltered.")]
	[Transformer("Null", true)]
	[Serializable]
	public class Null : Transformer
	{
		public Null(Dictionary<string, Variant> args)
            : base(args)
        {
        }

        protected override BitStream internalEncode(BitStream data)
        {
			return data;
        }

        protected override BitStream internalDecode(BitStream data)
        {
			return data;
        }
	}
}