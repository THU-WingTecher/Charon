using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;

using NLog;

using Charon.Core;
using Charon.Core.IO;
using Charon.Core.Dom;

namespace Charon.Core.Dom.XPath
{
	public enum CharonXPathNodeType
	{
		Root,
		DataModel,
		StateModel,
		Agent,
		Test,
		Run
	}
}
