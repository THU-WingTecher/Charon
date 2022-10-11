using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Charon.Core.Dom;
using Charon.Core.IO;

namespace Charon.Core.Cracker
{
	/// <summary>
	/// Interface required by data cracker
	/// </summary>
	public interface ICrackable
	{
		/// <summary>
		/// Called by data cracker to crack data into DataElement instance.
		/// </summary>
		/// <param name="context">DataCracker instance</param>
		/// <param name="data">Data to crack</param>
		void Crack(DataCracker context, BitStream data);
	}
}
