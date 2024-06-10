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
	public class SQLTranslatorCopySingle : SQLTranslator
	{
		public override string GetCaption()
		{
			return "Copy Table Sinlge";
		}

		public override void SetupOptionsContent(Control Parent, EventHandler changedHandler)
		{
            this.AddOptionTextBox("Var Suffix", "varSuffix", "11245U1", Parent, changedHandler);
            this.AddOptionCheckBox("Use Transaction", "useTransaction", true, Parent, changedHandler);
            this.AddOptionCheckBox("Rollback", "useRollback", true, Parent, changedHandler);
        }

        public override string TranslateExt2(CreateTableStatement createTableStatement, object options, string content, TSqlScript sqlScript)
        {
            string varSuffix = this.GetOptionString("varSuffix", options) ?? "";
            bool useTransaction = this.GetOptionBoolDef("useTransaction", options, false);
            bool useRollback = this.GetOptionBoolDef("useRollback", options, false);

            SqlBuilder result = new SqlBuilder();
			SqlDomBuilder sqlDomBuilder = new SqlDomBuilder(sqlScript, result);
            

            string tableSuffix = "_new";
            result.TableNameFull = TSQLHelper.Identifiers2Value(createTableStatement.SchemaObjectName.Identifiers);
            result.TableName = TSQLHelper.Identifiers2ValueLast(createTableStatement.SchemaObjectName.Identifiers);
            string tableNameNew = result.TableName + tableSuffix;
            string tableNameFullNew = result.TableNameFull + tableSuffix;
            {
				result.AppendBegin(SqlBuilder.BEGIN_TRY);
				{
                    if (useTransaction) result.AppendLine($"begin tran;").NL();

                    string messageVarName = $"@msg{varSuffix}";
                    result.AppendLine($"declare {messageVarName} nvarchar(2024);").NL();
                    sqlDomBuilder.VarMsgName = messageVarName;
                    sqlDomBuilder.Print($"Running Script{varSuffix}.{result.TableNameFull}...").NL();

                    result.AppendIf($"object_id('{tableNameFullNew}') is null").AppendBegin();
                    {
                        result.AppendLine("-- create new table");
                        sqlDomBuilder.ProduceFullTableRenameContent(sqlScript, tableSuffix, content);

                        result.AppendEnd();
                    }

                    sqlDomBuilder.ProduceCopyTableSingle(createTableStatement, varSuffix, targetTableNameFull: tableNameFullNew);

                    result.AppendBegin();
                    {
                        result.AppendLine("-- rename source table to _old");
                        sqlDomBuilder.ProduceFullTableRenameScript(sqlScript, "_old");
                        result.AppendEnd();
                    }

                    result.AppendBegin();
                    {
                        result.AppendLine("-- rename new table to current");
                        sqlDomBuilder.ProduceFullTableRenameScript(sqlScript, tableSuffix, reverse: true);

                        result.AppendEnd();
                    }

                    sqlDomBuilder.ProduceCopytableVerify(createTableStatement, varSuffix, targetTableNameFull: result.TableNameFull + "_old");

                    result.NL();
                    sqlDomBuilder.Print("Dropping table {result.TableNameFull}_old...");
                    result.AppendLine($"drop table if exists {result.TableNameFull}_old;").NL();

                    if (useTransaction)
                    {
                        if (useRollback)
                            result.AppendLine($"rollback;").NL();
                        else
                        {
                            result.AppendLine($"commit;").NL();
                            sqlDomBuilder.Print("Committed.");
                        }
                    }

                    result.AppendEnd(SqlBuilder.END_TRY, false);
                }
                result.AppendCatchTypicalRollback();
			}
            return result.ToString();
		}
	}
}
