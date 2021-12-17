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

		public bool? GetOptionBool(string name, object options)
        {
			bool? result = null;
			if (options is Control)
			{
				var r = ((Control)options).Controls.Find(name, true);
				if (r.Length > 0)
				{
					result = (r[0] as CheckBox).Checked;
				}
			}
			return result;
		}

		public bool GetOptionBoolDef(string name, object options, bool def)
        {
			bool? result = GetOptionBool(name, options);
			if (!result.HasValue)
				return def;
			return result.Value;
		}

		public string GetOptionString(string name, object options)
		{
			string result = null;
			if (options is Control)
			{
				var r = ((Control)options).Controls.Find(name, true);
				if (r.Length > 0)
				{
					result = (r[0] as TextBox).Text;
				}
			}
			return result;
		}
	}
}
