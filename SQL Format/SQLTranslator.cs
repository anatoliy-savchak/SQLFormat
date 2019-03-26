using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SQL_Format
{
	public abstract class SQLTranslator
	{
		public abstract string Translate(TableDefinition tableDefinition, object options);
		public abstract string GetCaption();
		public abstract void SetupOptionsContent(Control Parent, EventHandler changedHandler);
	}
}
