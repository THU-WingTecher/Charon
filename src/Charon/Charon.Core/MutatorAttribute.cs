using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Charon.Core
{
	// Mark a class as a Charon Mutator
	public class MutatorAttribute : Attribute
	{
		public string description = null;

		public MutatorAttribute()
		{
			description = "Unknown";
		}

		public MutatorAttribute(string description)
		{
			this.description = description;
		}
	}
}
