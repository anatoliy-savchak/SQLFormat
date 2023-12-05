using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_Format.Helpers
{
    public class SqlDomBuilderMerge
    {
        private readonly SqlBuilder _sqlBuilder;
        public string option_alias_src = "s";
        public string option_alias_dest = "t";
        public bool option_insertonly = false;
        public bool use_star_in_src = false;
        public bool use_star_in_targ = false;
        public bool use_output = false;
        public bool use_intesect = false;
        public string varSuffix = "";
        public string whereLineSource = "";
        public string whereLineDest = "";

        public SqlDomBuilderMerge(SqlBuilder sqlBuilder)
        { 
            _sqlBuilder = sqlBuilder;
        }

        public void ProduceMerge(CreateTableStatement sourceTable, CreateTableStatement targTable)
        {
            var optionAllias0 = option_alias_src;
            var optionAlliasDest0 = option_alias_dest;
            var tableDefinition = targTable.Definition;
            string optionAllias = optionAllias0;
            string optionAlliasDest = optionAlliasDest0;
            if (!String.IsNullOrEmpty(optionAllias)) optionAllias = optionAllias + '.';
            if (!String.IsNullOrEmpty(optionAlliasDest)) optionAlliasDest = optionAlliasDest + '.';
            string tableName = TSQLHelper.Identifiers2Value(targTable.SchemaObjectName.Identifiers);


            var result = this._sqlBuilder;
            string sep = null;
            var primaryColumns = tableDefinition.ColumnDefinitions
                .Where(c => TSQLHelper.ColumnIsPrimaryKey(c, tableDefinition, true)).ToList();

            if (use_output)
            {
                _sqlBuilder.Append($"declare @mergeOutput{varSuffix} table([action] nvarchar(10)");

                if (primaryColumns.Any())
                {
                    var mergeOutputColumnTrunks = primaryColumns
                        .Select(c => $"{TSQLHelper.Identifier2Value(c.ColumnIdentifier)} {TSQLHelper.Column2TypeStr(c)}").ToList();

                    _sqlBuilder.Append(", ", skipIdent: true);
                    ProduceColumnLines(this._sqlBuilder, mergeOutputColumnTrunks, forwardComma: false, inline: true);
                }
                _sqlBuilder.AppendLine(");", skipIdent: true).NL();
            }

            // with src as
            {
                result.AppendLine($";with src as (").Indent();
                if (!use_star_in_src)
                {
                    result.AppendLine($"select").Indent();
                    var columns = tableDefinition.ColumnDefinitions
                        .Select(c => $"{TSQLHelper.Identifier2Value(c.ColumnIdentifier)} = {optionAllias}{TSQLHelper.Identifier2Value(c.ColumnIdentifier)}").ToList();
                    ProduceColumnLines(this._sqlBuilder, columns, forwardComma: true, inline: false);
                    result.Unindent();
                }
                else
                {
                    result.AppendLine($"select *");
                }
                result.AppendLine($"from {tableName} {optionAllias0}");
                if (!string.IsNullOrEmpty(whereLineSource))
                    result.AppendLine(whereLineSource);
                result.Unindent()
                    .AppendLine($")");
            }
            // , targ as
            {
                result.AppendLine($", targ as (").Indent();
                if (!use_star_in_targ)
                {
                    result.AppendLine($"select").Indent();
                    var columns = tableDefinition.ColumnDefinitions
                        .Select(c => $"{TSQLHelper.Identifier2Value(c.ColumnIdentifier)} = {optionAlliasDest}{TSQLHelper.Identifier2Value(c.ColumnIdentifier)}").ToList();
                    ProduceColumnLines(this._sqlBuilder, columns, forwardComma: true, inline: false);
                    result.Unindent();
                }
                else
                {
                    result.AppendLine($"select *");
                }
                result.AppendLine($"from {tableName} {optionAlliasDest0}");
                if (!string.IsNullOrEmpty(whereLineDest))
                    result.AppendLine(whereLineDest);
                result.Unindent()
                    .AppendLine($")");
            }
            //merge
            {
                result.AppendLine($"merge targ as {optionAlliasDest0}")
                    .AppendLine($"using src as {optionAllias0}")
                    .Append($"on (");

                var columns = primaryColumns
                    .Select( c => $"{optionAlliasDest}{TSQLHelper.Identifier2Value(c.ColumnIdentifier)} = {optionAllias}{TSQLHelper.Identifier2Value(c.ColumnIdentifier)}")
                    .ToList();

                bool _inline = false;
                if (_inline)
                {
                    ProduceColumnLines(this._sqlBuilder, columns, forwardComma: true, inline: true, sep: " and");
                    result.AppendLine(")", skipIdent: true);
                }
                else
                {
                    result.AppendLine("").Indent();
                    ProduceColumnLines(this._sqlBuilder, columns, forwardComma: true, inline: false, sep: " and");
                    result.Unindent().AppendLine(")");
                }
            }
            //insert
            {
                result.AppendLine($"when not matched by target then")
                    .Indent()
                    .Append($"insert(");
                var columns = tableDefinition.ColumnDefinitions.Where(c => !TSQLHelper.ColumnIsIdentity(c));
                var insert = columns.Select(r => TSQLHelper.Identifier2Value(r.ColumnIdentifier)).ToList();
                result.AppendLine().Indent();
                ProduceColumnLines(_sqlBuilder, insert, false, false);
                result.Unindent().AppendLine($")");
                result.AppendLine($"values(").Indent();
                var values = columns.Select(r => $"{optionAllias}{TSQLHelper.Identifier2Value(r.ColumnIdentifier)}").ToList();
                ProduceColumnLines(_sqlBuilder, values, false, false);
                result.Unindent().AppendLine($")");
                result.Unindent();
            }
            //update
            if (!option_insertonly)
            {
                if (use_intesect)
                {
                    var intersectColumns = tableDefinition.ColumnDefinitions.Where(c => 
                        !(TSQLHelper.ColumnIsIdentity(c) || TSQLHelper.ColumnIsPrimaryKey(c, tableDefinition))
                        );

                    result.AppendLine($"when matched and not exists(").Indent();
                    result.AppendLine($"select").Indent();
                    var source = intersectColumns.Select(r => $"{optionAllias}{TSQLHelper.Identifier2Value(r.ColumnIdentifier)}").ToList();
                    ProduceColumnLines(_sqlBuilder, source, true, false);
                    result.Unindent();
                    result.AppendLine($"intersect");
                    result.AppendLine($"select").Indent();
                    var targ = intersectColumns.Select(r => $"{optionAlliasDest}{TSQLHelper.Identifier2Value(r.ColumnIdentifier)}").ToList();
                    ProduceColumnLines(_sqlBuilder, targ, true, false);
                    result.Unindent().Unindent();
                    result.AppendLine(") then");
                }
                else
                {
                    result.AppendLine("when matched then");
                }

                result.Indent().AppendLine($"update set").Indent();
                var updateColumns = tableDefinition.ColumnDefinitions.Where(c =>
                    !(TSQLHelper.ColumnIsIdentity(c) || TSQLHelper.ColumnIsPrimaryKey(c, tableDefinition))
                    ).Select(c=> $"{TSQLHelper.Identifier2Value(c.ColumnIdentifier)} = {optionAllias}{TSQLHelper.Identifier2Value(c.ColumnIdentifier)}").ToList();

                ProduceColumnLines(_sqlBuilder, updateColumns, true, false);
                result.Unindent().Unindent();
            }

            //delete
            if (!option_insertonly)
            {
                result.AppendLine("when not matched by source then");
                result.Indent().AppendLine("delete");
                result.Unindent();
            }

            if (use_output)
            {
                result.Append($"output $action, ");
                var output = primaryColumns.Select(c => $"inserted.{TSQLHelper.Identifier2Value(c.ColumnIdentifier)}").ToList();
                ProduceColumnLines(_sqlBuilder, output, true, true);
                result.AppendLine();

                result.Append($"into @mergeOutput(action, ");
                var output2 = primaryColumns.Select(c => $"{TSQLHelper.Identifier2Value(c.ColumnIdentifier)}").ToList();
                ProduceColumnLines(_sqlBuilder, output2, true, true);
                result.AppendLine(")", skipIdent: true);
            }
            result.AppendLine(";");
        }

        public static void ProduceColumnLines(SqlBuilder sqlBuilder, List<string> columns, bool forwardComma, bool inline, string sep = ",")
        {
            if (inline && forwardComma)
            {
                //sqlBuilder.Append(sep + " ", skipIdent: true);
                forwardComma = false;
            }
            int i = -1;
            int lastIndex = columns.Count-1;
            foreach (var name in columns)
            {
                i++;
                string line = $"{(forwardComma && (i > 0) ? $"{sep} " : "")}{name}{(!forwardComma && (i != lastIndex) ? $"{sep}" : "")}{(inline && (i != lastIndex) ? " " : "")}";

                if (inline)
                    sqlBuilder.Append(line, skipIdent: true);
                else
                    sqlBuilder.AppendLine(line);
            }
        }

        public void Test(CreateTableStatement sourceTable)
        {
            var insertColumns = sourceTable.Definition.ColumnDefinitions.Where(c => !TSQLHelper.ColumnIsIdentity(c)).Select(r => TSQLHelper.Identifier2Value(r.ColumnIdentifier)).ToList();
            _sqlBuilder.AppendLine("forwardComma: false, inline: false, colPrefix: ''");
            ProduceColumnLines(_sqlBuilder, insertColumns, false, false);
            _sqlBuilder.NL();

            _sqlBuilder.AppendLine("forwardComma: false, inline: true, colPrefix: ''");
            ProduceColumnLines(_sqlBuilder, insertColumns, false, true);
            _sqlBuilder.NL();

            _sqlBuilder.AppendLine("forwardComma: true, inline: false, colPrefix: ''");
            ProduceColumnLines(_sqlBuilder, insertColumns, true, false);
            _sqlBuilder.NL();

            _sqlBuilder.AppendLine("forwardComma: true, inline: true, colPrefix: ''");
            ProduceColumnLines(_sqlBuilder, insertColumns, true, true);
            _sqlBuilder.NL();
        }
    }
}
