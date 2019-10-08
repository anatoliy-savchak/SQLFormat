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
		public virtual string Translate(TableDefinition tableDefinition, object options) { return null; }
		public abstract string GetCaption();
		public abstract void SetupOptionsContent(Control Parent, EventHandler changedHandler);

		public virtual string TranslateExt(CreateTableStatement createTableStatement, object options)
		{
			return Translate(createTableStatement.Definition, options);
		}
	}
}
