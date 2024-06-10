using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_Format.Helpers
{
    public class SqlBuilder
    {
        public const string BEGIN = "begin";
        public const string END = "end";
        public const string BEGIN_TRY = "begin try";
        public const string END_TRY = "end try";
        public const string BEGIN_CATCH = "begin catch";
        public const string BEGIN_END = "end catch";

        public string TableName { get; set; }
        public string TableNameFull { get; set; }
        public int CurrIdent { get; set; }
        public StringBuilder stringBuilder { get; } = new StringBuilder();

        public SqlBuilder()
        {

        }

        public override string ToString()
        {
            return stringBuilder.ToString();
        }

        public SqlBuilder Indent(int tabCount = 1)
        {
            CurrIdent += tabCount;
            return this;
        }

        public SqlBuilder Unindent(int tabCount = 1)
        {
            CurrIdent -= tabCount;
            if (CurrIdent < 0) CurrIdent = 0;
            return this;
        }

        public SqlBuilder Append(string s = "", bool skipIdent = false)
        {
            if (CurrIdent == 0 || skipIdent)
                stringBuilder.Append(s);
            else
                stringBuilder.Append(new string('\t', CurrIdent) + s);
            return this;
        }

        public SqlBuilder AppendLine(string line = "", bool skipIdent = false)
        {
            if (CurrIdent == 0 || skipIdent)
                stringBuilder.AppendLine(line);
            else
                stringBuilder.AppendLine(new string('\t', CurrIdent) + line);
            return this;
        }

        public SqlBuilder NL(string line = "", bool skipIdent = false)
        {
            return AppendLine("", skipIdent);
        }

        public SqlBuilder AppendBegin(string line = BEGIN)
        {
            AppendLine(line);
            Indent();
            return this;
        }

        public SqlBuilder AppendEnd(string line = END, bool addSpaceafterwards = true)
        {
            Unindent();
            AppendLine(line);
            if (addSpaceafterwards)
                AppendLine();
            return this;
        }

        public SqlBuilder AppendIf(string lineCondition)
        {
            AppendLine($"if {lineCondition}");
            return this;
        }

        public SqlBuilder AppendCatchTypicalRollback()
        {
            AppendBegin(BEGIN_CATCH);
            AppendLine($"if @@trancount > 0 begin print('rollback');  rollback; end");
            AppendLine($";throw;");
            AppendEnd(BEGIN_END);
            return this;
        }

        public SqlBuilder AppendSpRename(string sourceNameFull, string destNamePure, string objType, string printType = null)
        {
            if (printType != null)
                AppendLine($"print 'Renaming {printType} {sourceNameFull} to {destNamePure}...';");
            AppendLine($"exec sp_rename @objname=N'{sourceNameFull}', @newname='{destNamePure}', @objtype='{objType}';");
            return this;
        }

        public SqlBuilder AppendText(string text, bool removeGo = true, bool trimDoubleEmptyLines = true)
        {
            var content = text.Replace("\r\n", "\n");
            var lines = text.Split('\n');

            bool prevLineWasEmpty = false;
            foreach (var line in lines)
            {
                string l = line.Trim().ToLowerInvariant();
                if (removeGo && l == "go")
                    continue;
                if (trimDoubleEmptyLines && (prevLineWasEmpty && l == ""))
                    continue;
                prevLineWasEmpty = l == "";
                AppendLine(line.Replace("\r", string.Empty));
            }
            return this;
        }

        public SqlBuilder AppendLines(IEnumerable<string> lines, string separator = null, bool skipIdent = false)
        {
            string prev = null;
            foreach (var line in lines)
            {
                if (prev != null)
                    AppendLine(prev + separator, skipIdent: skipIdent);
                prev = line;
            }
            if (prev != null)
                AppendLine(prev, skipIdent: skipIdent);

            return this;
        }
    }
}
