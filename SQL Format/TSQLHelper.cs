using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_Format
{
	public static class TSQLHelper
	{
		public static string Identifier2Value(Identifier identifier)
		{
			if (identifier == null) return null;
			if (identifier.QuoteType == QuoteType.SquareBracket)
			{
				return $"[{identifier.Value}]";
			}
			return identifier.Value;
		}

		public static string Identifiers2Value(IList<Identifier> identifiers)
		{
			if ((identifiers == null) || (identifiers.Count == 0)) return null;
			string result = Identifier2Value(identifiers[0]);
			for(int i = 1; i < identifiers.Count; i++)
				result = $"{result}.{Identifier2Value(identifiers[i])}";
			return result;
		}

		public static string Column2TypeStr(ColumnDefinition columnDefinition)
		{
			if (columnDefinition.DataType == null) return null;
			string result = columnDefinition.DataType.Name.Identifiers[0].Value.ToLowerInvariant();
			//+		Name	{Microsoft.SqlServer.TransactSql.ScriptDom.SchemaObjectName}	Microsoft.SqlServer.TransactSql.ScriptDom.SchemaObjectName
			if (columnDefinition.DataType is ParameterizedDataTypeReference)
			{
				if (((ParameterizedDataTypeReference)columnDefinition.DataType).Parameters.Count > 0)
				{
					result = $"{result}({((ParameterizedDataTypeReference)columnDefinition.DataType).Parameters[0].Value})";
				}
			}

			return result;
		}
	}
}
