﻿
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Mikhail Davidov (sirus@haxsys.net)

// $Id$

using System;
using System.Collections.Generic;
using System.Text;
using Charon.Core.Dom;
using Charon.Core.IO;

namespace Charon.Core.Transformers.Encode
{
    [Description("Encode on output as Base64.")]
    [Transformer("Base64Encode", true)]
    [Transformer("encode.Base64Encode")]
    [Serializable]
    public class Base64Encode : Transformer
    {
        public Base64Encode(Dictionary<string,Variant>  args) : base(args)
        {
        }

        protected override BitStream internalEncode(BitStream data)
        {
            var b64s = System.Convert.ToBase64String(data.Value);
            var bytes = System.Text.ASCIIEncoding.ASCII.GetBytes(b64s);
            return new BitStream(bytes);
        }
        
        protected override BitStream internalDecode(BitStream data)
        {
            var b64s = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);
            return new BitStream(System.Convert.FromBase64String(b64s));
        }
    }
}

// end