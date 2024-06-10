using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SQL_Format.Helpers;

namespace SQL_Format
{
	public class SQLTranslatorMyScript: SQLTranslator
	{
		public override string GetCaption()
		{
			return "My Script";
		}

		public override void SetupOptionsContent(Control Parent, EventHandler changedHandler)
		{
        }

        public override string TranslateExt2(CreateTableStatement createTableStatement, object options, string content, TSqlScript sqlScript)
        {
            SqlBuilder result = new SqlBuilder();
			SqlDomBuilder sqlDomBuilder = new SqlDomBuilder(sqlScript, result);
            result.TableNameFull = TSQLHelper.Identifiers2Value(createTableStatement.SchemaObjectName.Identifiers);
            result.TableName = TSQLHelper.Identifiers2ValueLast(createTableStatement.SchemaObjectName.Identifiers);

            int[] precs = new int[] { 18, 26, 20};

            result.AppendLine($"----------------- {result.TableNameFull} -----------------");
            result.AppendLine($"raiserror('Altering {result.TableNameFull}...', 10, 1) with nowait;");

            int addedCount = 0;
            foreach (var c in createTableStatement.Definition.ColumnDefinitions)
            {
                string colType = TSQLHelper.Column2TypeStr(c).ToUpperInvariant();
                if (colType.StartsWith("DECIMAL"))
                {
                    int prec = TSQLHelper.ColumnDecimalPrecision(c);
                    if (precs.Contains(prec))
                    {
                        addedCount++;
                        bool isNullable = TSQLHelper.ColumnIsNullable(c);
                        string null_ = isNullable ? " NULL" : " NOT NULL";
                        string colName = TSQLHelper.Identifier2Value(c.ColumnIdentifier);
                        string subalterColumn = $"column {colName} {colType}{null_}";
                        result.AppendIf($"not exists(select * from INFORMATION_SCHEMA.COLUMNS c where c.TABLE_NAME = '{result.TableName}' and c.COLUMN_NAME = '{colName}' and c.NUMERIC_PRECISION = {prec})").AppendBegin();
                        {
                            string alterColumn = $"alter {subalterColumn}";
                            result.AppendLine($"raiserror('{alterColumn}...', 10, 1) with nowait;");
                            if (isNullable)
                                result.AppendLine($"update {result.TableNameFull} set {colName} = try_cast({colName} as {colType});");
                            result.AppendLine($"alter table {result.TableNameFull}").Indent();
                            result.AppendLine($"{alterColumn};").Unindent();
                            result.AppendEnd(addSpaceafterwards: false);
                        }
                        result.AppendLine("else").Indent();
                        {
                            result.AppendLine($"raiserror('already {subalterColumn}.', 10, 1) with nowait;");
                            result.Unindent();
                        }

                        result.AppendLine("GO");
                    }
                }
            }
            result.AppendLine($"----------------------------------");
            return result.ToString();
		}
	}
}
